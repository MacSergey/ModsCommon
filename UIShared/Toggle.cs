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
        private float circleScale = 1f;
        public float CircleScale
        {
            get => circleScale;
            set
            {
                if (value != circleScale)
                {
                    circleScale = value;
                    SetCircle();
                }
            }
        }

        public CustomUIToggle()
        {
            canFocus = false;
            ForegroundSpriteMode = UIForegroundSpriteMode.Scale;
        }

        private void SetState()
        {
            NormalBgColor = state ? OnColor : OffColor;
            FocusedBgColor = state ? OnColor : OffColor;
            HoveredBgColor = state ? OnHoverColor : OffHoverColor;
            PressedBgColor = state ? OnPressedColor : OffPressedColor;
            DisabledBgColor = state ? OnDisabledColor : OffDisabledColor;

            HorizontalAlignment = state ? UIHorizontalAlignment.Right : UIHorizontalAlignment.Left;
            TextHorizontalAlignment = state ? UIHorizontalAlignment.Left : UIHorizontalAlignment.Right;
            text = showMark ? (state ? "I" : "O") : string.Empty;
        }
        private void SetCircle()
        {
            var padding = Mathf.RoundToInt(height * (1f - CircleScale) * 0.5f);
            SpritePadding = new RectOffset(padding, padding, 0, 0);
            ScaleFactor = 1f - SpritePadding.horizontal / height;
        }

        protected override void OnClick(UIMouseEventParameter p)
        {
            base.OnClick(p);
            State = !State;
        }
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            SetCircle();
        }

        public void SetStyle(ToggleStyle style)
        {
            onColor = style.OnColors.normal;
            onHoverColor = style.OnColors.hovered;
            onPressedColor = style.OnColors.pressed;
            onDisabledColor = style.OnColors.disabled;

            offColor = style.OffColors.normal;
            offHoverColor = style.OffColors.hovered;
            offPressedColor = style.OffColors.pressed;
            offDisabledColor = style.OffColors.disabled;

            SetState();
        }
    }
}
