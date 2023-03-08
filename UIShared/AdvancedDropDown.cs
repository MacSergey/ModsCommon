using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class AdvancedDropDown<ObjectType, PopupType, EntityType> : CustomUIButton
        where PopupType : Popup<ObjectType, EntityType>
        where EntityType : PopupEntity<ObjectType>
    {
        #region PROPERTIES

        public delegate void StyleDelegate(PopupType popup, ref bool overridden);

        public event Action<ObjectType> OnValueChanged;
        public event Action<PopupType> OnPopupOpen;
        public event Action<PopupType> OnPopupClose;
        public event StyleDelegate OnSetPopupStyle;

        public EntityType Entity { get; private set; }
        public PopupType Popup { get; private set; }

        protected List<ObjectType> Objects { get; } = new List<ObjectType>();

        private int selectedIndex;
        public ObjectType SelectedObject
        {
            get => selectedIndex >= 0 ? Objects[selectedIndex] : default;
            set
            {
                selectedIndex = Objects.FindIndex(o => ReferenceEquals(o, value) || (o != null && o.Equals(value)));
                Entity.SetObject(-1, SelectedObject, false);
            }
        }

        #endregion

        public AdvancedDropDown()
        {
            Entity = AddUIComponent<EntityType>();
            Entity.relativePosition = Vector3.zero;

            Entity.isInteractive = false;
            foreach (var item in Entity.GetComponentsInChildren<UIComponent>())
                item.isInteractive = false;
        }

        public virtual void AddItem(ObjectType item)
        {
            Objects.Add(item);
        }
        public virtual void Clear()
        {
            Objects.Clear();
            selectedIndex = -1;
        }

        protected virtual void ValueChanged(ObjectType value) => OnValueChanged?.Invoke(value);

        #region POPUP

        public override void Update()
        {
            base.Update();
            CheckPopup();
        }
        protected override void OnClick(UIMouseEventParameter p)
        {
            base.OnClick(p);

            if (Popup == null)
                OpenPopup();
            else
                ClosePopup();
        }

        protected void OpenPopup()
        {
            isInteractive = false;

            var root = GetRootContainer();
            Popup = root.AddUIComponent<PopupType>();
            Popup.canFocus = true;

            Popup.StopRefresh();
            {
                var overridden = false;
                OnSetPopupStyle?.Invoke(Popup, ref overridden);
                if(!overridden)
                    SetPopupStyle();

                InitPopup();
                Popup.SelectedObject = SelectedObject;

                Popup.eventSizeChanged += OnPopupSizeChanged;
                Popup.eventKeyDown += OnPopupKeyDown;
                Popup.eventLeaveFocus += OnPopupLeaveFocus;
                Popup.OnSelectedChanged += OnSelectedChanged;

                SetPopupPosition();
                Popup.parent.eventPositionChanged += SetPopupPosition;

                PopupOpening();
                OnPopupOpen?.Invoke(Popup);

                Popup.Focus();
            }
            Popup.StartRefresh();
        }

        protected virtual void SetPopupStyle() { }
        protected virtual void InitPopup() => Popup.Init(Objects, null, null);
        protected virtual void PopupOpening() { }

        public virtual void ClosePopup()
        {
            isInteractive = true;

            if (Popup != null)
            {
                PopupClosing();
                OnPopupClose?.Invoke(Popup);

                Popup.eventLeaveFocus -= OnPopupLeaveFocus;
                Popup.eventKeyDown -= OnPopupKeyDown;

                ComponentPool.Free(Popup);
                Popup = null;
            }
        }
        protected virtual void PopupClosing() { }

        private void CheckPopup()
        {
            if (Popup == null)
                return;

            if (!Popup.containsFocus)
            {
                ClosePopup();
                return;
            }

            if (Input.GetMouseButtonDown(0) && !Popup.Raycast(GetCamera().ScreenPointToRay(Input.mousePosition)))
            {
                ClosePopup();
                return;
            }
        }
        private void OnSelectedChanged(ObjectType value)
        {
            SelectedObject = value;
            ValueChanged(value);
            ClosePopup();
        }
        private void OnPopupLeaveFocus(UIComponent component, UIFocusEventParameter eventParam) => CheckPopup();
        private void OnPopupKeyDown(UIComponent component, UIKeyEventParameter p)
        {
            if (p.keycode == KeyCode.Escape)
            {
                ClosePopup();
                p.Use();
            }
        }
        private void OnPopupSizeChanged(UIComponent component, Vector2 value) => SetPopupPosition();

        private void SetPopupPosition(UIComponent component = null, Vector2 value = default)
        {
            if (Popup != null)
            {
                UIView uiView = Popup.GetUIView();
                var screen = uiView.GetScreenResolution();
                var position = absolutePosition + new Vector3(0, height);
                position.x = MathPos(position.x, Popup.width, screen.x);
                position.y = MathPos(position.y, Popup.height, screen.y);

                Popup.relativePosition = position - Popup.parent.absolutePosition;
            }

            static float MathPos(float pos, float size, float screen) => pos + size > screen ? (screen - size < 0 ? 0 : screen - size) : Mathf.Max(pos, 0);
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            Entity.size = size;
        }

        #endregion
    }

    public abstract class SimpleDropDown<ValueType, EntityType, PopupType> : AdvancedDropDown<DropDownItem<ValueType>, PopupType, EntityType>
        where EntityType : SimpleEntity<ValueType>
        where PopupType : SimplePopup<ValueType, EntityType>
    {
        public new event Action<ValueType> OnValueChanged;
        public new ValueType SelectedObject
        {
            get => base.SelectedObject.value;
            set => base.SelectedObject = new DropDownItem<ValueType>(value, value.ToString());
        }

        public virtual void AddItem(ValueType item) => AddItem(new DropDownItem<ValueType>(item, item.ToString()));
        public virtual void AddItem(ValueType item, string label) => AddItem(new DropDownItem<ValueType>(item, label));
        protected override void ValueChanged(DropDownItem<ValueType> item) => OnValueChanged?.Invoke(item.value);
        protected override void SetPopupStyle() => Popup.DefaultStyle();
        protected override void InitPopup()
        {
            Popup.MaximumSize = new Vector2(width, 700f);
            Popup.EntityHeight = height;
            Popup.width = width;
            Popup.MaxVisibleItems = 0;
            Popup.Init(Objects, null, null);
        }
    }
    public abstract class SimpleEntity<ValueType> : PopupEntity<DropDownItem<ValueType>>
    {
        public SimpleEntity()
        {
            textHorizontalAlignment = UIHorizontalAlignment.Left;
            textPadding = new RectOffset(14, 40, 3, 0);
        }

        public override void SetObject(int index, DropDownItem<ValueType> value, bool selected)
        {
            base.SetObject(index, value, selected);
            text = value.label;
        }
    }
    public abstract class SimplePopup<ValueType, EntityType> : Popup<DropDownItem<ValueType>, EntityType>
        where EntityType : SimpleEntity<ValueType>
    {

    }
    public readonly struct DropDownItem<ValueType>
    {
        public readonly ValueType value;
        public readonly string label;

        public DropDownItem(ValueType value, string label)
        {
            this.value = value;
            this.label = label;
        }

        public override bool Equals(object obj)
        {
            if (obj is DropDownItem<ValueType> item)
                return value.Equals(item.value);
            else
                return false;
        }
        public override int GetHashCode() => value.GetHashCode();
    }

    public class StringSimpleDropDown : SimpleDropDown<string, StringSimpleDropDown.StringEntity, StringSimpleDropDown.StringPopup>
    {
        public class StringEntity : SimpleEntity<string> { }
        public class StringPopup : SimplePopup<string, StringEntity> { }
    }
}
