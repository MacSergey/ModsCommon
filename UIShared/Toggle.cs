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
        public event Action<bool> OnValueChanged;


        public bool Value
        {
            get => IsSelected;
            set
            {
                if (value != Value)
                {
                    IsSelected = value;
                    OnValueChanged?.Invoke(value);
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
            HorizontalAlignment = Value ? UIHorizontalAlignment.Right : UIHorizontalAlignment.Left;
            TextHorizontalAlignment = Value ? UIHorizontalAlignment.Left : UIHorizontalAlignment.Right;
            text = showMark ? (Value ? "I" : "O") : string.Empty;
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
            Value = !Value;
        }
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            SetCircle();
        }

        public ButtonStyle ToggleStyle { set => ButtonStyle = value; }

        protected override Color32 RenderTextColor
        {
            get
            {
                return State switch
                {
                    UIButton.ButtonState.Normal => Value ? SelNormalTextColor : NormalTextColor,
                    UIButton.ButtonState.Hovered => Value ? SelHoveredTextColor : HoveredTextColor,
                    UIButton.ButtonState.Pressed => Value ? SelPressedTextColor : PressedTextColor,
                    UIButton.ButtonState.Disabled => Value ? SelDisabledTextColor : DisabledTextColor,
                    UIButton.ButtonState.Focused => Value ? SelFocusedTextColor : FocusedTextColor,
                    _ => Color.white,
                };
            }
        }
        protected override Color32 RenderBackgroundColor
        {
            get
            {
                return State switch
                {
                    UIButton.ButtonState.Focused => Value ? SelFocusedBgColor : FocusedBgColor,
                    UIButton.ButtonState.Hovered => Value ? SelHoveredBgColor : HoveredBgColor,
                    UIButton.ButtonState.Pressed => Value ? SelPressedBgColor : PressedBgColor,
                    UIButton.ButtonState.Disabled => Value ? SelDisabledBgColor : DisabledBgColor,
                    _ => Value ? SelNormalBgColor : NormalBgColor,
                };
            }
        }
        protected override Color32 RenderForegroundColor
        {
            get
            {
                return State switch
                {
                    UIButton.ButtonState.Focused => Value ? SelFocusedFgColor : FocusedFgColor,
                    UIButton.ButtonState.Hovered => Value ? SelHoveredFgColor : HoveredFgColor,
                    UIButton.ButtonState.Pressed => Value ? SelPressedFgColor : PressedFgColor,
                    UIButton.ButtonState.Disabled => Value ? SelDisabledFgColor : DisabledFgColor,
                    _ => Value ? SelNormalFgColor : NormalFgColor,
                };
            }
        }
        protected override UITextureAtlas.SpriteInfo RenderBackgroundSprite
        {
            get
            {
                if (BgAtlas is not UITextureAtlas atlas)
                    return null;

                var spriteInfo = State switch
                {
                    UIButton.ButtonState.Normal => atlas[Value ? SelNormalBgSprite : NormalBgSprite],
                    UIButton.ButtonState.Focused => atlas[Value ? SelFocusedBgSprite : FocusedBgSprite],
                    UIButton.ButtonState.Hovered => atlas[Value ? SelHoveredBgSprite : HoveredBgSprite],
                    UIButton.ButtonState.Pressed => atlas[Value ? SelPressedBgSprite : PressedBgSprite],
                    UIButton.ButtonState.Disabled => atlas[Value ? SelDisabledBgSprite : DisabledBgSprite],
                    _ => null,
                };

                return spriteInfo ?? atlas[NormalBgSprite];
            }
        }

        protected override UITextureAtlas.SpriteInfo RenderForegroundSprite
        {
            get
            {
                if (FgAtlas is not UITextureAtlas atlas)
                    return null;

                var spriteInfo = State switch
                {
                    UIButton.ButtonState.Normal => atlas[Value ? SelNormalFgSprite : NormalFgSprite],
                    UIButton.ButtonState.Focused => atlas[Value ? SelFocusedFgSprite : FocusedFgSprite],
                    UIButton.ButtonState.Hovered => atlas[Value ? SelHoveredFgSprite : HoveredFgSprite],
                    UIButton.ButtonState.Pressed => atlas[Value ? SelPressedFgSprite : PressedFgSprite],
                    UIButton.ButtonState.Disabled => atlas[Value ? SelDisabledFgSprite : DisabledFgSprite],
                    _ => null,
                };

                return spriteInfo ?? atlas[NormalFgSprite];
            }
        }
    }
}
