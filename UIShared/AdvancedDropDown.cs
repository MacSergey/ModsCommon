using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class AdvancedDropDown<ObjectType, PopupType, EntityType> : MultyAtlasUIButton
        where PopupType : Popup<ObjectType, EntityType>
        where EntityType : PopupEntity<ObjectType>
    {
        #region PROPERTIES

        public event Action<ObjectType> OnValueChanged;
        public event Action<PopupType> OnPopupOpen;
        public event Action<PopupType> OnPopupClose;

        public EntityType Entity { get; private set; }
        public PopupType Popup { get; private set; }

        protected List<ObjectType> Objects { get; } = new List<ObjectType>();

        private ObjectType selectedObject;
        public ObjectType SelectedObject 
        {
            get => selectedObject;
            set
            {
                selectedObject = value;
                Entity.SetObject(0, value, false);
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
            SelectedObject = default;
        }

        protected void ValueChanged(ObjectType value) => OnValueChanged?.Invoke(value);

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
                InitPopup();
                Popup.SelectedObject = SelectedObject;

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

    public abstract class SimpleDropDown<ValueType> : AdvancedDropDown<SimpleDropDown<ValueType>.Item, SimpleDropDown<ValueType>.SimplePopup, SimpleDropDown<ValueType>.SimpleEntity>
    {
        public class SimpleEntity : PopupEntity<Item>
        {
            private CustomUILabel Label { get; set; }

            public SimpleEntity()
            {
                Label = AddUIComponent<CustomUILabel>();
            }

            public override void SetObject(int index, Item value, bool selected)
            {
                base.SetObject(index, value, selected);
                Label.text = value.label;
            }
        }
        public class SimplePopup : Popup<Item, SimpleEntity>
        {

        }
        public readonly struct Item
        {
            public readonly ValueType value;
            public readonly string label;

            public Item(ValueType value, string label)
            {
                this.value = value;
                this.label = label;
            }
        }

        public virtual void AddItem(ValueType item) => AddItem(new Item(item, item.ToString()));
        public virtual void AddItem(ValueType item, string label) => AddItem(new Item(item, label));
    }
}
