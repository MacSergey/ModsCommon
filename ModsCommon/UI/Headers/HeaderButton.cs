﻿using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class HeaderButton : UIButton
    {
        public static int Size => IconSize + 2 * IconPadding;
        public static int IconSize => 25;
        public static int IconPadding => 2;
        public UIButton Icon { get; }
        protected virtual Color32 HoveredColor => new Color32(32, 32, 32, 255);
        protected virtual Color32 PressedColor => Color.black;

        protected virtual Color32 IconColor => Color.white;
        protected virtual Color32 HoverIconColor => Color.white;
        protected virtual Color32 PressedIconColor => new Color32(224, 224, 224, 255);
        protected virtual Color32 DisabledIconColor => new Color32(144, 144, 144, 255);

        protected abstract UITextureAtlas IconAtlas { get; }

        public HeaderButton()
        {
            hoveredBgSprite = pressedBgSprite = focusedBgSprite = TextureHelper.HeaderHoverSprite;
            size = new Vector2(Size, Size);
            atlas = TextureHelper.CommonAtlas;
            hoveredColor = HoveredColor;
            pressedColor = focusedColor = PressedColor;
            clipChildren = true;
            textPadding = new RectOffset(IconSize + 5, 5, 5, 0);
            textScale = 0.8f;
            textHorizontalAlignment = UIHorizontalAlignment.Left;
            minimumSize = size;

            Icon = AddUIComponent<UIButton>();
            Icon.size = new Vector2(IconSize, IconSize);
            Icon.atlas = IconAtlas;
            Icon.relativePosition = new Vector2(IconPadding, IconPadding);

            Icon.color = IconColor;
            Icon.hoveredColor = HoverIconColor;
            Icon.pressedColor = PressedIconColor;
            Icon.disabledColor = DisabledIconColor;
        }

        public void SetIconSprite(string sprite)
        {
            Icon.normalBgSprite = sprite;
            Icon.hoveredBgSprite = sprite;
            Icon.pressedBgSprite = sprite;
        }
        public override void Update()
        {
            base.Update();
            if (state == ButtonState.Focused)
                state = ButtonState.Normal;
        }
    }

    public abstract class HeaderPopupButton<PopupType> : HeaderButton
        where PopupType : PopupPanel
    {
        public PopupType Popup { get; private set; }

        protected override void OnClick(UIMouseEventParameter p)
        {
            if (Popup == null)
                OpenPopup();
            else
                ClosePopup();
        }
        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();
            if (!isVisible)
                ClosePopup();
        }

        protected void OpenPopup()
        {
            var root = GetRootContainer();
            Popup = root.AddUIComponent<PopupType>();
            Popup.eventLostFocus += OnPopupLostFocus;
            Popup.eventKeyDown += OnPopupKeyDown;
            Popup.Focus();

            OnOpenPopup();
            Popup.Init();

            SetPopupPosition();
            Popup.parent.eventPositionChanged += SetPopupPosition;
        }

        protected virtual void OnOpenPopup() { }
        public virtual void ClosePopup()
        {
            if (Popup != null)
            {
                Popup.eventLostFocus -= OnPopupLostFocus;
                Popup.eventKeyDown -= OnPopupKeyDown;

                Popup.parent.RemoveUIComponent(Popup);
                Destroy(Popup.gameObject);
                Popup = null;
            }
        }
        private void OnPopupLostFocus(UIComponent component, UIFocusEventParameter eventParam)
        {
            var uiView = Popup.GetUIView();
            var mouse = uiView.ScreenPointToGUI(Input.mousePosition / uiView.inputScale);
            var popupRect = new Rect(Popup.absolutePosition, Popup.size);
            var buttonRect = new Rect(absolutePosition, size);
            if (!popupRect.Contains(mouse) && !buttonRect.Contains(mouse))
                ClosePopup();
            else
                Popup.Focus();
        }
        private void OnPopupKeyDown(UIComponent component, UIKeyEventParameter p)
        {
            if(p.keycode == KeyCode.Escape)
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
    }
}
