using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using ColossalFramework.UI;
using UnityEngine;

namespace ModsCommon.UI
{
    public class CustomUIToggle : CustomUIButton
    {
        public event Action<bool> OnStateChanged;

        private new bool state;
        public bool State
        {
            get => state;
            set
            {
                if (value != state)
                {
                    state = value;
                    OnStateChanged?.Invoke(state);
                    SetState();
                }
            }
        }
        private bool showMark;
        public bool ShowMark
        {
            get => showMark;
            set
            {
                if (value != showMark)
                {
                    showMark = value;
                    SetState();
                }
            }
        }

        private Color32 onColor;
        public Color32 OnColor
        {
            get => onColor;
            set
            {
                onColor = value;
                SetState();
            }
        }
        private Color32 onHoverColor;
        public Color32 OnHoverColor
        {
            get => onHoverColor;
            set
            {
                onHoverColor = value;
                SetState();
            }
        }
        private Color32 onPressedColor;
        public Color32 OnPressedColor
        {
            get => onPressedColor;
            set
            {
                onPressedColor = value;
                SetState();
            }
        }
        private Color32 onDisabledColor;
        public Color32 OnDisabledColor
        {
            get => onDisabledColor;
            set
            {
                onDisabledColor = value;
                SetState();
            }
        }


        private Color32 offColor;
        public Color32 OffColor
        {
            get => offColor;
            set
            {
                offColor = value;
                SetState();
            }
        }
        private Color32 offHoverColor;
        public Color32 OffHoverColor
        {
            get => offHoverColor;
            set
            {
                offHoverColor = value;
                SetState();
            }
        }
        private Color32 offPressedColor;
        public Color32 OffPressedColor
        {
            get => offPressedColor;
            set
            {
                offPressedColor = value;
                SetState();
            }
        }
        private Color32 offDisabledColor;
        public Color32 OffDisabledColor
        {
            get => offDisabledColor;
            set
            {
                offDisabledColor = value;
                SetState();
            }
        }

        public CustomUIToggle()
        {
            scaleFactor = 0.7f;
            canFocus = false;
            atlas = CommonTextures.Atlas;
            normalBgSprite = hoveredBgSprite = pressedBgSprite = CommonTextures.ToggleBackground;
            normalFgSprite = CommonTextures.ToggleCircle;
            foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            textPadding = new RectOffset(17, 13, 5, 0);
            size = new Vector2(60f, 30f);
        }

        private void SetState()
        {
            normalBgColor = state ? OnColor : OffColor;
            focusedBgColor = state ? OnColor : OffColor;
            hoveredBgColor = state ? OnHoverColor : OffHoverColor;
            pressedBgColor = state ? OnPressedColor : OffPressedColor;
            disabledBgColor = state ? OnDisabledColor : OffDisabledColor;

            horizontalAlignment = state ? UIHorizontalAlignment.Right : UIHorizontalAlignment.Left;
            textHorizontalAlignment = state ? UIHorizontalAlignment.Left : UIHorizontalAlignment.Right;
            text = showMark ? (state ? "I" : "O") : string.Empty;
        }
        private void SetSize()
        {
            var padding = Mathf.CeilToInt(height * (1f - scaleFactor) * 0.5f);
            spritePadding = new RectOffset(padding, padding, 0, 0);
        }

        protected override void OnClick(UIMouseEventParameter p)
        {
            base.OnClick(p);
            State = !State;
        }
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            SetSize();
        }
    }
}
