using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModsCommon.UI
{
    public delegate void PopupStyleDelegate<ObjectType, EntityType, PopupType>(PopupType popup, ref bool overridden)
        where PopupType : CustomUIPanel, IPopup<ObjectType, EntityType>
        where EntityType : CustomUIButton, IPopupEntity<ObjectType>;

    public delegate void EntityStyleDelegate<ObjectType, EntityType>(EntityType entity, ref bool overridden)
        where EntityType : CustomUIButton, IPopupEntity<ObjectType>;

    public interface IPopup<ObjectType, EntityType>
        where EntityType : CustomUIButton, IPopupEntity<ObjectType>
    {
        event Action<ObjectType> OnSelect;
        event EntityStyleDelegate<ObjectType, EntityType> OnSetEntityStyle;

        ObjectType SelectedObject { get; set; }
        Func<ObjectType, ObjectType, bool> IsEqualDelegate { set; }

        Vector2 MaximumSize { get; set; }
        float EntityHeight { get; set; }
        RectOffset ItemsPadding { get; set; }

        void Init(IEnumerable<ObjectType> values, Func<ObjectType, bool> selector, Func<ObjectType, ObjectType, int> sorter);
        void StopRefresh();
        void StartRefresh();
    }
    public interface IPopupEntity<ObjectType>
    {
        event Action<int, ObjectType> OnSelected;

        ObjectType EditObject { get; }
        bool Selected { get; set; }
        int Index { get; set; }
        RectOffset Padding { get; set; }

        void SetObject(int index, ObjectType value, bool selected);
        void PerformWidth();
    }

    public abstract class ObjectDropDown<ObjectType, PopupType, EntityType> : BaseDropDown<PopupType>
        where PopupType : CustomUIPanel, IPopup<ObjectType, EntityType>
        where EntityType : CustomUIButton, IPopupEntity<ObjectType>
    {
        #region PROPERTIES

        public event Action<ObjectType> OnSelectObject;
        public event Action<PopupType> OnPopupOpen;
        public event Action<PopupType> OnPopupClose;
        public event PopupStyleDelegate<ObjectType, EntityType, PopupType> OnSetPopupStyle;
        public event EntityStyleDelegate<ObjectType, EntityType> OnSetEntityStyle;

        public PopupType Popup { get; private set; }

        public Func<ObjectType, ObjectType, bool> IsEqualDelegate { get; set; }

        #endregion

        protected virtual void SelectObject(ObjectType value) => OnSelectObject?.Invoke(value);

        #region POPUP

        public override void Update()
        {
            base.Update();
            OnUpdate();
        }
        protected virtual void OnUpdate() => CheckPopup();
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
            Popup.IsEqualDelegate = IsEqualDelegate;
            Popup.eventSizeChanged += OnPopupSizeChanged;
            Popup.eventKeyDown += OnPopupKeyDown;
            Popup.eventLeaveFocus += OnPopupLeaveFocus;
            Popup.OnSelect += OnSelect;

            Popup.StopRefresh();
            {
                var overridden = false;
                OnSetPopupStyle?.Invoke(Popup, ref overridden);
                if (!overridden)
                    SetPopupStyle();

                InitPopup();

                SetPopupPosition();
                Popup.parent.eventPositionChanged += SetPopupPosition;

                PopupOpening();
                OnPopupOpen?.Invoke(Popup);

                Popup.Focus();
            }
            Popup.StartRefresh();
        }

        protected abstract IEnumerable<ObjectType> Objects { get; }
        protected abstract Func<ObjectType, bool> Selector { get; }
        protected abstract Func<ObjectType, ObjectType, int> Sorter { get; }

        protected virtual void SetPopupStyle() { }
        protected virtual void SetEntityStyle(EntityType entity, ref bool overridden) => OnSetEntityStyle?.Invoke(entity, ref overridden);

        protected virtual void InitPopup() => Popup.Init(Objects, Selector, Sorter);
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
        private void OnSelect(ObjectType value)
        {
            SelectObject(value);
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

        #endregion
    }
}
