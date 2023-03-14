using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModsCommon.UI
{
    public class BaseDropDown<PopupType> : CustomUIButton
        where PopupType : UIComponent
    {
        public event Action OnBeforePopupOpen;
        public event Action<PopupType> OnPopupOpening;
        public event Action<PopupType> OnAfterPopupOpen;

        public event Action<PopupType> OnBeforePopupClose;
        public event Action<PopupType> OnPopupClosing;
        public event Action OnAfterPopupClose;

        public PopupType Popup { get; private set; }

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

        protected virtual void OpenPopup()
        {
            BeforePopupOpen();
            OnBeforePopupOpen?.Invoke();

            Popup = GetRootContainer().AddUIComponent<PopupType>();

            SetPopupProperties();
            AddPopupEventHandlers();

            WhilePopupOpening();
            OnPopupOpening?.Invoke(Popup);

            SetPopupPosition();
            Popup.parent.eventPositionChanged += SetPopupPosition;
            Popup.Focus();

            AfterPopupOpen();
            OnAfterPopupOpen?.Invoke(Popup);
        }
        protected virtual void SetPopupProperties()
        {
            Popup.canFocus = true;
        }
        protected virtual void AddPopupEventHandlers()
        {
            Popup.eventSizeChanged += OnPopupSizeChanged;
            Popup.eventKeyDown += OnPopupKeyDown;
            Popup.eventLeaveFocus += OnPopupLeaveFocus;
        }
        public virtual void ClosePopup()
        {
            if (Popup != null)
            {
                BeforePopupClose();
                OnBeforePopupClose?.Invoke(Popup);

                Popup.eventLeaveFocus -= OnPopupLeaveFocus;
                Popup.eventKeyDown -= OnPopupKeyDown;

                WhilePopupClosing();
                OnPopupClosing?.Invoke(Popup);

                ComponentPool.Free(Popup);
                Popup = null;

                AfterPopupClose();
                OnAfterPopupClose?.Invoke();
            }
        }

        protected virtual void BeforePopupOpen() { }
        protected virtual void WhilePopupOpening() { }
        protected virtual void AfterPopupOpen() { }

        protected virtual void BeforePopupClose() { }
        protected virtual void WhilePopupClosing() { }
        protected virtual void AfterPopupClose() { }

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
    }
}
