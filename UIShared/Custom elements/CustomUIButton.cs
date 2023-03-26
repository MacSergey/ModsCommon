using ColossalFramework.UI;
using System;
using UnityEngine;

namespace ModsCommon.UI
{
    public class CustomUIButton : UITextComponent
    {
        public event Action<UIButton.ButtonState> OnStateChanged;

        #region HANDLERS
        public override void OnEnable()
        {
            base.OnEnable();

            if (m_Font == null || !m_Font.isValid)
                m_Font = GetUIView().defaultFont;
        }
        protected override void OnEnterFocus(UIFocusEventParameter p)
        {
            if (State != UIButton.ButtonState.Pressed)
                State = UIButton.ButtonState.Focused;

            base.OnEnterFocus(p);
        }
        protected override void OnLeaveFocus(UIFocusEventParameter p)
        {
            State = containsMouse ? UIButton.ButtonState.Hovered : UIButton.ButtonState.Normal;
            base.OnLeaveFocus(p);
        }
        protected override void OnKeyPress(UIKeyEventParameter p)
        {
            if (builtinKeyNavigation && (p.keycode == KeyCode.Space || p.keycode == KeyCode.Return))
                OnClick(new UIMouseEventParameter(this, UIMouseButton.Left, 1, default, Vector2.zero, Vector2.zero, 0f));
            else
                base.OnKeyPress(p);
        }
        protected override void OnMouseDown(UIMouseEventParameter p)
        {
            if ((p.buttons & ButtonsMask) != 0)
            {
                State = UIButton.ButtonState.Pressed;
                base.OnMouseDown(p);
            }
        }
        protected override void OnMouseUp(UIMouseEventParameter p)
        {
            if (m_IsMouseHovering)
                State = UIButton.ButtonState.Hovered;
            else if (hasFocus)
                State = UIButton.ButtonState.Focused;
            else
                State = UIButton.ButtonState.Normal;

            base.OnMouseUp(p);
        }
        protected override void OnMouseEnter(UIMouseEventParameter p)
        {
            State = UIButton.ButtonState.Hovered;

            base.OnMouseEnter(p);
            Invalidate();
        }
        protected override void OnMouseLeave(UIMouseEventParameter p)
        {
            if (containsFocus)
                State = UIButton.ButtonState.Focused;
            else
                State = UIButton.ButtonState.Normal;

            base.OnMouseLeave(p);
            Invalidate();
        }
        protected override void OnIsEnabledChanged()
        {
            if (!isEnabled)
                State = UIButton.ButtonState.Disabled;
            else
                State = UIButton.ButtonState.Normal;

            base.OnIsEnabledChanged();
        }
        protected virtual void OnButtonStateChanged(UIButton.ButtonState value)
        {
            if (isEnabled || value == UIButton.ButtonState.Disabled)
            {
#pragma warning disable CS0612
                state = value;
#pragma warning restore CS0612
                OnStateChanged?.Invoke(value);

                Invalidate();
            }
        }
        protected override void OnGotFocus(UIFocusEventParameter p)
        {
            base.OnGotFocus(p);
            Invalidate();
        }
        protected override void OnLostFocus(UIFocusEventParameter p)
        {
            base.OnLostFocus(p);
            Invalidate();
        }

        #endregion

        [Obsolete]
        protected UIButton.ButtonState state;
        public UIButton.ButtonState State
        {
#pragma warning disable CS0612
            get => state;
            set
            {
                if (value != state)
                {
                    OnButtonStateChanged(value);
                    Invalidate();
                }
            }
#pragma warning restore CS0612
        }


        [Obsolete]
        protected bool wordWrap;
        public bool WordWrap
        {
#pragma warning disable CS0612
            get => wordWrap;
            set
            {
                if (value != wordWrap)
                {
                    wordWrap = value;
                    Invalidate();
                }
            }
#pragma warning restore CS0612
        }


        [Obsolete]
        protected UIHorizontalAlignment textHorizontalAlign = UIHorizontalAlignment.Center;
        public UIHorizontalAlignment TextHorizontalAlignment
        {
#pragma warning disable CS0612
            get => (AutoSize & AutoSize.Width) != 0 ? UIHorizontalAlignment.Left : textHorizontalAlign;
            set
            {
                if (value != textHorizontalAlign)
                {
                    textHorizontalAlign = value;
                    Invalidate();
                }
            }
#pragma warning restore CS0612
        }


        [Obsolete]
        protected UIVerticalAlignment textVerticalAlign = UIVerticalAlignment.Middle;
        public virtual UIVerticalAlignment TextVerticalAlignment
        {
#pragma warning disable CS0612
            get => (AutoSize & AutoSize.Height) != 0 ? UIVerticalAlignment.Top : textVerticalAlign;
            set
            {
                if (value != textVerticalAlign)
                {
                    textVerticalAlign = value;
                    Invalidate();
                }
            }
#pragma warning restore CS0612
        }


        [Obsolete]
        protected UIHorizontalAlignment horizontalAlignment = UIHorizontalAlignment.Center;
        public UIHorizontalAlignment HorizontalAlignment
        {
#pragma warning disable CS0612
            get => (AutoSize & AutoSize.Width) != 0 ? UIHorizontalAlignment.Left : horizontalAlignment;
            set
            {
                if (value != horizontalAlignment)
                {
                    horizontalAlignment = value;
                    Invalidate();
                }
            }
#pragma warning restore CS0612
        }


        [Obsolete]
        protected UIVerticalAlignment verticalAlignment = UIVerticalAlignment.Middle;
        public UIVerticalAlignment VerticalAlignment
        {
#pragma warning disable CS0612
            get => (AutoSize & AutoSize.Height) != 0 ? UIVerticalAlignment.Top : verticalAlignment;
            set
            {
                if (value != verticalAlignment)
                {
                    verticalAlignment = value;
                    Invalidate();
                }
            }
#pragma warning restore CS0612
        }


        [Obsolete]
        protected RectOffset textPadding;
        public RectOffset TextPadding
        {
#pragma warning disable CS0612
            get => textPadding ??= new RectOffset();
            set
            {
                value = value.ConstrainPadding();
                if (!Equals(value, textPadding))
                {
                    textPadding = value;
                    Invalidate();
                }
            }
#pragma warning restore CS0612
        }



        private AutoSize _autoSize;
        [Obsolete]
        public override bool autoSize
        {
            get => AutoSize == AutoSize.All;
            set => AutoSize = value ? AutoSize.All : AutoSize.None;
        }
        public AutoSize AutoSize
        {
#pragma warning disable CS0612
            get => _autoSize;
            set
            {
                if (value != _autoSize)
                {
                    _autoSize = value;

                    if ((value & AutoSize.Width) != 0)
                    {
                        horizontalAlignment = UIHorizontalAlignment.Left;
                        textHorizontalAlign = UIHorizontalAlignment.Left;
                    }

                    if ((value & AutoSize.Height) != 0)
                    {
                        verticalAlignment = UIVerticalAlignment.Top;
                        textVerticalAlign = UIVerticalAlignment.Top;
                    }

                    Invalidate();
                }
            }
#pragma warning restore CS0612
        }


        protected UIMouseButton buttonsMask = UIMouseButton.Left;
        public UIMouseButton ButtonsMask
        {
#pragma warning disable CS0612
            get => buttonsMask;
            set => buttonsMask = value;
#pragma warning restore CS0612
        }



        #region Style

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    Invalidate();
                }
            }
        }


        protected UITextureAtlas fgAtlas;
        public UITextureAtlas FgAtlas
        {
            get => fgAtlas ?? Atlas;
            set
            {
                if (!Equals(value, fgAtlas))
                {
                    fgAtlas = value;
                    Invalidate();
                }
            }
        }


        protected UITextureAtlas bgAtlas;
        public UITextureAtlas BgAtlas
        {
            get => bgAtlas ?? Atlas;
            set
            {
                if (!Equals(value, bgAtlas))
                {
                    bgAtlas = value;
                    Invalidate();
                }
            }
        }


        protected UITextureAtlas atlas;
        public UITextureAtlas Atlas
        {
            get => atlas ??= GetUIView()?.defaultAtlas;
            set
            {
                if (value != atlas)
                {
                    atlas = value;
                    Invalidate();
                }
            }
        }


        protected UIForegroundSpriteMode foregroundSpriteMode;
        public UIForegroundSpriteMode ForegroundSpriteMode
        {
            get => foregroundSpriteMode;
            set
            {
                if (value != foregroundSpriteMode)
                {
                    foregroundSpriteMode = value;
                    Invalidate();
                }
            }
        }

        protected float scaleFactor = 1f;
        public float ScaleFactor
        {
            get => scaleFactor;
            set
            {
                if (!Mathf.Approximately(value, scaleFactor))
                {
                    scaleFactor = value;
                    Invalidate();
                }
            }
        }


        protected RectOffset spritePadding;
        public RectOffset SpritePadding
        {
            get => spritePadding ??= new RectOffset();
            set
            {
                if (!Equals(value, spritePadding))
                {
                    spritePadding = value;
                    Invalidate();
                }
            }
        }

        public override bool canFocus => isEnabled && isVisible ? true : base.canFocus;

        [Obsolete]
        public new Color32 color
        {
            get => base.color;
            set
            {
                bgColors = value;
                fgColors = value;
                selBgColors = value;
                selFgColors = value;
                base.color = value;
            }
        }

        [Obsolete]
        public new Color32 textColor
        {
            get => base.textColor;
            set
            {
                textColors = value;
                selTextColors = value;
                base.textColor = value;
            }
        }

        [Obsolete]
        public new Color32 disabledColor
        {
            get => base.disabledColor;
            set
            {
                bgColors.disabled = value;
                fgColors.disabled = value;
                base.disabledColor = value;
            }
        }

        [Obsolete]
        public new Color32 disabledTextColor
        {
            get => base.disabledTextColor;
            set
            {
                textColors.disabled = value;
                selTextColors.disabled = value;
                base.disabledTextColor = value;
            }
        }

        public SpriteSet AllBgSprites
        {
            set
            {
                bgSprites = value;
                selBgSprites = value;
                Invalidate();
            }
        }
        public SpriteSet AllFgSprites
        {
            set
            {
                fgSprites = value;
                selFgSprites = value;
                Invalidate();
            }
        }
        public ColorSet AllBgColors
        {
            set
            {
                bgColors = value;
                selBgColors = value;
                Invalidate();
            }
        }
        public ColorSet AllFgColors
        {
            set
            {
                fgColors = value;
                selFgColors = value;
                Invalidate();
            }
        }
        public ColorSet AllTextColors
        {
            set
            {
                textColors = value;
                selTextColors = value;
                Invalidate();
            }
        }

        #region BACKGROUND SPRITE

        protected SpriteSet bgSprites;
        public SpriteSet BgSprites
        {
            get => bgSprites;
            set
            {
                bgSprites = value;
                Invalidate();
            }
        }
        public string NormalBgSprite
        {
            get => bgSprites.normal;
            set
            {
                if (value != bgSprites.normal)
                {
                    bgSprites.normal = value;
                    Invalidate();
                }
            }
        }
        public string HoveredBgSprite
        {
            get => bgSprites.hovered;
            set
            {
                if (value != bgSprites.hovered)
                {
                    bgSprites.hovered = value;
                    Invalidate();
                }
            }
        }
        public string PressedBgSprite
        {
            get => bgSprites.pressed;
            set
            {
                if (value != bgSprites.pressed)
                {
                    bgSprites.pressed = value;
                    Invalidate();
                }
            }
        }
        public string FocusedBgSprite
        {
            get => bgSprites.focused;
            set
            {
                if (value != bgSprites.focused)
                {
                    bgSprites.focused = value;
                    Invalidate();
                }
            }
        }
        public string DisabledBgSprite
        {
            get => bgSprites.disabled;
            set
            {
                if (value != bgSprites.disabled)
                {
                    bgSprites.disabled = value;
                    Invalidate();
                }
            }
        }

        #endregion

        #region FOREGROUND SPRITE

        protected SpriteSet fgSprites;
        public SpriteSet FgSprites
        {
            get => fgSprites;
            set
            {
                fgSprites = value;
                Invalidate();
            }
        }

        public string NormalFgSprite
        {
            get => fgSprites.normal;
            set
            {
                if (value != fgSprites.normal)
                {
                    fgSprites.normal = value;
                    Invalidate();
                }
            }
        }
        public string HoveredFgSprite
        {
            get => fgSprites.hovered;
            set
            {
                if (value != fgSprites.hovered)
                {
                    fgSprites.hovered = value;
                    Invalidate();
                }
            }
        }
        public string PressedFgSprite
        {
            get => fgSprites.pressed;
            set
            {
                if (value != fgSprites.pressed)
                {
                    fgSprites.pressed = value;
                    Invalidate();
                }
            }
        }
        public string FocusedFgSprite
        {
            get => fgSprites.focused;
            set
            {
                if (value != fgSprites.focused)
                {
                    fgSprites.focused = value;
                    Invalidate();
                }
            }
        }
        public string DisabledFgSprite
        {
            get => fgSprites.disabled;
            set
            {
                if (value != fgSprites.disabled)
                {
                    fgSprites.disabled = value;
                    Invalidate();
                }
            }
        }

        #endregion

        #region SELECTED BACKGROUND SPRITE

        protected SpriteSet selBgSprites;
        public SpriteSet SelBgSprites
        {
            get => selBgSprites;
            set
            {
                selBgSprites = value;
                Invalidate();
            }
        }

        public string SelNormalBgSprite
        {
            get => string.IsNullOrEmpty(selBgSprites.normal) ? FocusedBgSprite : selBgSprites.normal;
            set
            {
                if (value != selBgSprites.normal)
                {
                    selBgSprites.normal = value;
                    Invalidate();
                }
            }
        }
        public string SelHoveredBgSprite
        {
            get => string.IsNullOrEmpty(selBgSprites.hovered) ? HoveredBgSprite : selBgSprites.hovered;
            set
            {
                if (value != selBgSprites.hovered)
                {
                    selBgSprites.hovered = value;
                    Invalidate();
                }
            }
        }
        public string SelPressedBgSprite
        {
            get => string.IsNullOrEmpty(selBgSprites.pressed) ? PressedBgSprite : selBgSprites.pressed;
            set
            {
                if (value != selBgSprites.pressed)
                {
                    selBgSprites.pressed = value;
                    Invalidate();
                }
            }
        }
        public string SelFocusedBgSprite
        {
            get => string.IsNullOrEmpty(selBgSprites.focused) ? FocusedBgSprite : selBgSprites.focused;
            set
            {
                if (value != selBgSprites.focused)
                {
                    selBgSprites.focused = value;
                    Invalidate();
                }
            }
        }
        public string SelDisabledBgSprite
        {
            get => string.IsNullOrEmpty(selBgSprites.disabled) ? DisabledBgSprite : selBgSprites.disabled;
            set
            {
                if (value != selBgSprites.disabled)
                {
                    selBgSprites.disabled = value;
                    Invalidate();
                }
            }
        }

        #endregion

        #region SELECTED FOREGROUND SPRITE

        protected SpriteSet selFgSprites;
        public SpriteSet SelFgSprites
        {
            get => selFgSprites;
            set
            {
                selFgSprites = value;
                Invalidate();
            }
        }

        public string SelNormalFgSprite
        {
            get => string.IsNullOrEmpty(selFgSprites.normal) ? FocusedFgSprite : selFgSprites.normal;
            set
            {
                if (value != selFgSprites.normal)
                {
                    selFgSprites.normal = value;
                    Invalidate();
                }
            }
        }
        public string SelHoveredFgSprite
        {
            get => string.IsNullOrEmpty(selFgSprites.hovered) ? HoveredFgSprite : selFgSprites.hovered;
            set
            {
                if (value != selFgSprites.hovered)
                {
                    selFgSprites.hovered = value;
                    Invalidate();
                }
            }
        }
        public string SelPressedFgSprite
        {
            get => string.IsNullOrEmpty(selFgSprites.pressed) ? PressedFgSprite : selFgSprites.pressed;
            set
            {
                if (value != selFgSprites.pressed)
                {
                    selFgSprites.pressed = value;
                    Invalidate();
                }
            }
        }
        public string SelFocusedFgSprite
        {
            get => string.IsNullOrEmpty(selFgSprites.focused) ? FocusedFgSprite : selFgSprites.focused;
            set
            {
                if (value != selFgSprites.focused)
                {
                    selFgSprites.focused = value;
                    Invalidate();
                }
            }
        }
        public string SelDisabledFgSprite
        {
            get => string.IsNullOrEmpty(selFgSprites.disabled) ? DisabledFgSprite : selFgSprites.disabled;
            set
            {
                if (value != selFgSprites.disabled)
                {
                    selFgSprites.disabled = value;
                    Invalidate();
                }
            }
        }

        #endregion

        #region BACKGROUND COLOR

        protected ColorSet bgColors = new ColorSet(Color.white);
        public ColorSet BgColors
        {
            get => bgColors;
            set
            {
                bgColors = value;
                OnColorChanged();
            }
        }

        public Color32 NormalBgColor
        {
            get => bgColors.normal;
            set
            {
                if (!bgColors.normal.Equals(value))
                {
                    bgColors.normal = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 FocusedBgColor
        {
            get => bgColors.focused;
            set
            {
                if (!bgColors.focused.Equals(value))
                {
                    bgColors.focused = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 HoveredBgColor
        {
            get => bgColors.hovered;
            set
            {
                if (!bgColors.hovered.Equals(value))
                {
                    bgColors.hovered = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 PressedBgColor
        {
            get => bgColors.pressed;
            set
            {
                if (!bgColors.pressed.Equals(value))
                {
                    bgColors.pressed = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 DisabledBgColor
        {
            get => bgColors.disabled;
            set
            {
                if (!bgColors.disabled.Equals(value))
                {
                    bgColors.disabled = value;
                    OnColorChanged();
                }
            }
        }

        #endregion

        #region FOREGROUND COLOR

        protected ColorSet fgColors = new ColorSet(Color.white);
        public ColorSet FgColors
        {
            get => fgColors;
            set
            {
                fgColors = value;
                OnColorChanged();
            }
        }

        public Color32 NormalFgColor
        {
            get => fgColors.normal;
            set
            {
                if (!fgColors.normal.Equals(value))
                {
                    fgColors.normal = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 FocusedFgColor
        {
            get => fgColors.focused;
            set
            {
                if (!fgColors.focused.Equals(value))
                {
                    fgColors.focused = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 HoveredFgColor
        {
            get => fgColors.hovered;
            set
            {
                if (!fgColors.hovered.Equals(value))
                {
                    fgColors.hovered = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 PressedFgColor
        {
            get => fgColors.pressed;
            set
            {
                if (!fgColors.pressed.Equals(value))
                {
                    fgColors.pressed = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 DisabledFgColor
        {
            get => fgColors.disabled;
            set
            {
                if (!fgColors.disabled.Equals(value))
                {
                    fgColors.disabled = value;
                    OnColorChanged();
                }
            }
        }

        #endregion

        #region TEXT COLOR

        protected ColorSet textColors = new ColorSet(Color.white);
        public ColorSet TextColors
        {
            get => textColors;
            set
            {
                textColors = value;
                OnColorChanged();
            }
        }

        public Color32 NormalTextColor
        {
            get => textColors.normal;
            set
            {
                if (!textColors.normal.Equals(value))
                {
                    textColors.normal = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 FocusedTextColor
        {
            get => textColors.focused;
            set
            {
                if (!textColors.focused.Equals(value))
                {
                    textColors.focused = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 HoveredTextColor
        {
            get => textColors.hovered;
            set
            {
                if (!textColors.hovered.Equals(value))
                {
                    textColors.hovered = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 PressedTextColor
        {
            get => textColors.pressed;
            set
            {
                if (!textColors.pressed.Equals(value))
                {
                    textColors.pressed = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 DisabledTextColor
        {
            get => textColors.disabled;
            set
            {
                if (!textColors.disabled.Equals(value))
                {
                    textColors.disabled = value;
                    OnColorChanged();
                }
            }
        }

        #endregion

        #region SELECTED BACKGROUND COLOR

        protected ColorSet selBgColors = new ColorSet(Color.white);
        public ColorSet SelBgColors
        {
            get => selBgColors;
            set
            {
                selBgColors = value;
                OnColorChanged();
            }
        }

        public Color32 SelNormalBgColor
        {
            get => string.IsNullOrEmpty(selBgSprites.normal) ? NormalBgColor : selBgColors.normal;
            set
            {
                if (!selBgColors.normal.Equals(value))
                {
                    selBgColors.normal = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 SelHoveredBgColor
        {
            get => string.IsNullOrEmpty(selBgSprites.hovered) ? HoveredBgColor : selBgColors.hovered;
            set
            {
                if (!selBgColors.hovered.Equals(value))
                {
                    selBgColors.hovered = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 SelPressedBgColor
        {
            get => string.IsNullOrEmpty(selBgSprites.pressed) ? PressedBgColor : selBgColors.pressed;
            set
            {
                if (!selBgColors.pressed.Equals(value))
                {
                    selBgColors.pressed = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 SelFocusedBgColor
        {
            get => string.IsNullOrEmpty(selBgSprites.hovered) ? FocusedBgColor : selBgColors.focused;
            set
            {
                if (!selBgColors.focused.Equals(value))
                {
                    selBgColors.focused = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 SelDisabledBgColor
        {
            get => string.IsNullOrEmpty(selBgSprites.disabled) ? DisabledBgColor : selBgColors.disabled;
            set
            {
                if (!selBgColors.disabled.Equals(value))
                {
                    selBgColors.disabled = value;
                    OnColorChanged();
                }
            }
        }

        #endregion

        #region SELECTED FOREGROUND COLOR

        protected ColorSet selFgColors = new ColorSet(Color.white);
        public ColorSet SelFgColors
        {
            get => selFgColors;
            set
            {
                selFgColors = value;
                OnColorChanged();
            }
        }

        public Color32 SelNormalFgColor
        {
            get => string.IsNullOrEmpty(selFgSprites.normal) ? NormalFgColor : selFgColors.normal;
            set
            {
                if (!selFgColors.normal.Equals(value))
                {
                    selFgColors.normal = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 SelHoveredFgColor
        {
            get => string.IsNullOrEmpty(selFgSprites.hovered) ? FocusedFgColor : selFgColors.hovered;
            set
            {
                if (!selFgColors.hovered.Equals(value))
                {
                    selFgColors.hovered = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 SelPressedFgColor
        {
            get => string.IsNullOrEmpty(selFgSprites.pressed) ? PressedFgColor : selFgColors.pressed;
            set
            {
                if (!selFgColors.pressed.Equals(value))
                {
                    selFgColors.pressed = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 SelFocusedFgColor
        {
            get => string.IsNullOrEmpty(selFgSprites.focused) ? FocusedFgColor : selFgColors.focused;
            set
            {
                if (!selFgColors.focused.Equals(value))
                {
                    selFgColors.focused = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 SelDisabledFgColor
        {
            get => string.IsNullOrEmpty(selFgSprites.disabled) ? DisabledFgColor : selFgColors.disabled;
            set
            {
                if (!selFgColors.disabled.Equals(value))
                {
                    selFgColors.disabled = value;
                    OnColorChanged();
                }
            }
        }

        #endregion

        #region SELECTED TEXT COLOR

        protected ColorSet selTextColors = new ColorSet(Color.white);
        public ColorSet SelTextColors
        {
            get => selTextColors;
            set
            {
                selTextColors = value;
                OnColorChanged();
            }
        }

        public Color32 SelNormalTextColor
        {
            get => string.IsNullOrEmpty(selBgSprites.normal) ? NormalTextColor : selTextColors.normal;
            set
            {
                if (!selTextColors.normal.Equals(value))
                {
                    selTextColors.normal = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 SelHoveredTextColor
        {
            get => string.IsNullOrEmpty(selBgSprites.hovered) ? HoveredTextColor : selTextColors.hovered;
            set
            {
                if (!selTextColors.hovered.Equals(value))
                {
                    selTextColors.hovered = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 SelPressedTextColor
        {
            get => string.IsNullOrEmpty(selBgSprites.pressed) ? PressedTextColor : selTextColors.pressed;
            set
            {
                if (!selTextColors.pressed.Equals(value))
                {
                    selTextColors.pressed = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 SelFocusedTextColor
        {
            get => string.IsNullOrEmpty(selBgSprites.hovered) ? FocusedTextColor : selTextColors.focused;
            set
            {
                if (!selTextColors.focused.Equals(value))
                {
                    selTextColors.focused = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 SelDisabledTextColor
        {
            get => string.IsNullOrEmpty(selBgSprites.disabled) ? DisabledTextColor : selTextColors.disabled;
            set
            {
                if (!selTextColors.disabled.Equals(value))
                {
                    selTextColors.disabled = value;
                    OnColorChanged();
                }
            }
        }

        #endregion

        public ButtonStyle ButtonStyle
        {
            set
            {
                bgAtlas = value.BgAtlas;
                fgAtlas = value.FgAtlas;

                bgSprites = value.BgSprites;
                selBgSprites = value.SelBgSprites;

                fgSprites = value.FgSprites;
                selFgSprites = value.SelFgSprites;

                bgColors = value.BgColors;
                selBgColors = value.SelBgColors;

                fgColors = value.FgColors;
                selFgColors = value.SelFgColors;

                textColors = value.TextColors;
                selTextColors = value.SelTextColors;

                OnColorChanged();
            }
        }

        private bool bold;
        public bool Bold
        {
            get => bold;
            set
            {
                if (value != bold)
                {
                    bold = value;
                    font = value ? ComponentStyle.SemiBoldFont : ComponentStyle.RegularFont;
                    Invalidate();
                }
            }
        }

        #endregion

        public Vector2 MinimumAutoSize
        {
            get
            {
                var size = minimumSize;

                if (m_Font != null && m_Font.isValid && !string.IsNullOrEmpty(m_Text))
                {
                    using (UIFontRenderer uIFontRenderer = ObtainTextRenderer())
                    {
                        Vector2 measure = uIFontRenderer.MeasureString(m_Text);
                        size = new Vector2(measure.x + TextPadding.horizontal, measure.y + TextPadding.vertical);
                    }
                }

                return size;
            }
        }

        private Vector3 positionBefore;
        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout()
        {
            if ((relativePosition - positionBefore).sqrMagnitude > 0.001)
                relativePosition = positionBefore;
        }

        public override void Invalidate()
        {
            base.Invalidate();

            switch (AutoSize)
            {
                case AutoSize.Width:
                    PerformAutoWidth();
                    break;
                case AutoSize.Height:
                    PerformAutoHeight();
                    break;
                case AutoSize.All:
                    PerformAutoSize();
                    break;
            }
        }

        public void PerformAutoSize()
        {
            if (font != null && font.isValid && !string.IsNullOrEmpty(text))
            {
                using UIFontRenderer uIFontRenderer = ObtainTextRenderer();
                var measured = uIFontRenderer.MeasureString(text).RoundToInt();

                var width = Mathf.Max(measured.x + TextPadding.horizontal, minimumSize.x);
                var height = Mathf.Max(measured.y + TextPadding.vertical, minimumSize.y);
                size = new Vector2(width, height);
            }
        }
        public void PerformAutoWidth()
        {
            if (m_Font != null && m_Font.isValid && !string.IsNullOrEmpty(m_Text))
            {
                using UIFontRenderer uIFontRenderer = ObtainTextRenderer();
                var measured = uIFontRenderer.MeasureString(text).RoundToInt();
                width = Mathf.Max(measured.x + TextPadding.horizontal, minimumSize.x);
            }
        }
        public void PerformAutoHeight()
        {
            if (m_Font != null && m_Font.isValid && !string.IsNullOrEmpty(m_Text))
            {
                using UIFontRenderer uIFontRenderer = ObtainTextRenderer();
                var measured = uIFontRenderer.MeasureString(text).RoundToInt();
                height = Mathf.Max(measured.y + TextPadding.vertical, minimumSize.y);
            }
        }

        #region RENDER

        protected UIRenderData BgRenderData { get; set; }
        protected UIRenderData FgRenderData { get; set; }
        protected UIRenderData TextRenderData { get; set; }

        public override void OnDisable()
        {
            BgRenderData = null;
            FgRenderData = null;
            TextRenderData = null;
            base.OnDisable();
        }
        protected override void OnRebuildRenderData()
        {
            if (BgRenderData == null)
            {
                BgRenderData = UIRenderData.Obtain();
                m_RenderData.Add(BgRenderData);
            }
            else
                BgRenderData.Clear();

            if (FgRenderData == null)
            {
                FgRenderData = UIRenderData.Obtain();
                m_RenderData.Add(FgRenderData);
            }
            else
                FgRenderData.Clear();

            if (TextRenderData == null)
            {
                TextRenderData = UIRenderData.Obtain();
                m_RenderData.Add(TextRenderData);
            }
            else
                TextRenderData.Clear();

            if (BgAtlas is UITextureAtlas bgAtlas && FgAtlas is UITextureAtlas fgAtlas)
            {
                BgRenderData.material = bgAtlas.material;
                FgRenderData.material = fgAtlas.material;
                TextRenderData.material = bgAtlas.material;

                RenderBackground();
                RenderForeground();
                RenderText();
            }
        }
        private void RenderText()
        {
            if (font == null || !font.isValid || string.IsNullOrEmpty(text))
                return;

            using UIFontRenderer uIFontRenderer = ObtainTextRenderer();
            if (uIFontRenderer is UIDynamicFont.DynamicFontRenderer dynamicFontRenderer)
            {
                dynamicFontRenderer.spriteAtlas = BgAtlas;
                dynamicFontRenderer.spriteBuffer = BgRenderData;
            }
            uIFontRenderer.Render(text, TextRenderData);
        }
        private UIFontRenderer ObtainTextRenderer()
        {
            var maxSize = AutoSize switch
            {
                AutoSize.Width or AutoSize.All => new Vector2(width - TextPadding.horizontal, height),
                AutoSize.Height => new Vector2(width - TextPadding.horizontal, 4096f),
                _ => new Vector2(width - TextPadding.horizontal, height - TextPadding.vertical),
            };

            var ratio = PixelsToUnits();
            var offset = pivot.TransformToUpperLeft(size, arbitraryPivotOffset);
            offset.x += TextPadding.left * ratio;
            offset.y -= TextPadding.top * ratio;

            var renderer = font.ObtainRenderer();
            renderer.wordWrap = WordWrap;
            renderer.multiLine = true;
            renderer.maxSize = maxSize;
            renderer.pixelRatio = ratio;
            renderer.textScale = textScale;
            renderer.vectorOffset = offset;
            renderer.textAlign = TextHorizontalAlignment;
            renderer.processMarkup = processMarkup;
            renderer.defaultColor = ApplyOpacity(RenderTextColor);
            renderer.bottomColor = null;
            renderer.overrideMarkupColors = false;
            renderer.opacity = CalculateOpacity();
            renderer.shadow = useDropShadow;
            renderer.shadowColor = dropShadowColor;
            renderer.shadowOffset = dropShadowOffset;
            renderer.outline = useOutline;
            renderer.outlineSize = outlineSize;
            renderer.outlineColor = outlineColor;
            if (TextVerticalAlignment != UIVerticalAlignment.Top)
                renderer.vectorOffset = GetVertAlignOffset(renderer);

            return renderer;
        }

        protected virtual Color32 RenderTextColor
        {
            get
            {
                if (!isEnabled)
                    return DisabledTextColor;
                else
                {
                    return State switch
                    {
                        UIButton.ButtonState.Normal => IsSelected ? SelNormalTextColor : NormalTextColor,
                        UIButton.ButtonState.Hovered => IsSelected ? SelHoveredTextColor : HoveredTextColor,
                        UIButton.ButtonState.Pressed => IsSelected ? SelPressedTextColor : PressedTextColor,
                        UIButton.ButtonState.Focused => IsSelected ? SelFocusedTextColor : FocusedTextColor,
                        UIButton.ButtonState.Disabled => IsSelected ? SelDisabledTextColor : DisabledTextColor,
                        _ => Color.white,
                    };
                }
            }
        }
        protected virtual Color32 RenderBackgroundColor
        {
            get
            {
                return State switch
                {
                    UIButton.ButtonState.Focused => IsSelected ? SelFocusedBgColor : FocusedBgColor,
                    UIButton.ButtonState.Hovered => IsSelected ? SelHoveredBgColor : HoveredBgColor,
                    UIButton.ButtonState.Pressed => IsSelected ? SelPressedBgColor : PressedBgColor,
                    UIButton.ButtonState.Disabled => IsSelected ? SelDisabledBgColor : DisabledBgColor,
                    _ => IsSelected ? SelNormalBgColor : NormalBgColor,
                };
            }
        }
        protected virtual Color32 RenderForegroundColor
        {
            get
            {
                return State switch
                {
                    UIButton.ButtonState.Focused => IsSelected ? SelFocusedFgColor : FocusedFgColor,
                    UIButton.ButtonState.Hovered => IsSelected ? SelHoveredFgColor : HoveredFgColor,
                    UIButton.ButtonState.Pressed => IsSelected ? SelPressedFgColor : PressedFgColor,
                    UIButton.ButtonState.Disabled => IsSelected ? SelDisabledFgColor : DisabledFgColor,
                    _ => IsSelected ? SelNormalFgColor : NormalFgColor,
                };
            }
        }

        private Vector3 GetVertAlignOffset(UIFontRenderer fontRenderer)
        {
            var num = PixelsToUnits();
            var vector = fontRenderer.MeasureString(text) * num;
            var vectorOffset = fontRenderer.vectorOffset;
            var num2 = (height - TextPadding.vertical) * num;
            if (vector.y >= num2)
                return vectorOffset;

            switch (TextVerticalAlignment)
            {
                case UIVerticalAlignment.Middle:
                    vectorOffset.y -= (num2 - vector.y) * 0.5f;
                    break;
                case UIVerticalAlignment.Bottom:
                    vectorOffset.y -= num2 - vector.y;
                    break;
            }
            return vectorOffset;
        }

        protected virtual UITextureAtlas.SpriteInfo RenderBackgroundSprite
        {
            get
            {
                if (BgAtlas is not UITextureAtlas atlas)
                    return null;

                var spriteInfo = State switch
                {
                    UIButton.ButtonState.Normal => atlas[IsSelected ? SelNormalBgSprite : NormalBgSprite],
                    UIButton.ButtonState.Focused => atlas[IsSelected ? SelFocusedBgSprite : FocusedBgSprite],
                    UIButton.ButtonState.Hovered => atlas[IsSelected ? SelHoveredBgSprite : HoveredBgSprite],
                    UIButton.ButtonState.Pressed => atlas[IsSelected ? SelPressedBgSprite : PressedBgSprite],
                    UIButton.ButtonState.Disabled => atlas[IsSelected ? SelDisabledBgSprite : DisabledBgSprite],
                    _ => null,
                };

                return spriteInfo ?? atlas[NormalBgSprite];
            }
        }

        protected virtual UITextureAtlas.SpriteInfo RenderForegroundSprite
        {
            get
            {
                if (FgAtlas is not UITextureAtlas atlas)
                    return null;

                var spriteInfo = State switch
                {
                    UIButton.ButtonState.Normal => atlas[IsSelected ? SelNormalFgSprite : NormalFgSprite],
                    UIButton.ButtonState.Focused => atlas[IsSelected ? SelFocusedFgSprite : FocusedFgSprite],
                    UIButton.ButtonState.Hovered => atlas[IsSelected ? SelHoveredFgSprite : HoveredFgSprite],
                    UIButton.ButtonState.Pressed => atlas[IsSelected ? SelPressedFgSprite : PressedFgSprite],
                    UIButton.ButtonState.Disabled => atlas[IsSelected ? SelDisabledFgSprite : DisabledFgSprite],
                    _ => null,
                };

                return spriteInfo ?? atlas[NormalFgSprite];
            }
        }

        protected void RenderBackground()
        {
            if (RenderBackgroundSprite is UITextureAtlas.SpriteInfo backgroundSprite)
            {
                var renderOptions = new RenderOptions()
                {
                    atlas = BgAtlas,
                    color = RenderBackgroundColor,
                    fillAmount = 1f,
                    flip = UISpriteFlip.None,
                    offset = pivot.TransformToUpperLeft(size, arbitraryPivotOffset),
                    pixelsToUnits = PixelsToUnits(),
                    size = size,
                    spriteInfo = backgroundSprite,
                };

                if (backgroundSprite.isSliced)
                    Render.RenderSlicedSprite(BgRenderData, renderOptions);
                else
                    Render.RenderSprite(BgRenderData, renderOptions);
            }
        }
        protected void RenderForeground()
        {
            if (RenderForegroundSprite is UITextureAtlas.SpriteInfo foregroundSprite)
            {
                var foregroundRenderSize = GetForegroundRenderSize(foregroundSprite);
                var foregroundRenderOffset = GetForegroundRenderOffset(foregroundRenderSize);

                var renderOptions = new RenderOptions()
                {
                    atlas = FgAtlas,
                    color = RenderForegroundColor,
                    fillAmount = 1f,
                    flip = UISpriteFlip.None,
                    offset = foregroundRenderOffset,
                    pixelsToUnits = PixelsToUnits(),
                    size = foregroundRenderSize,
                    spriteInfo = foregroundSprite,
                };

                if (foregroundSprite.isSliced)
                    Render.RenderSlicedSprite(FgRenderData, renderOptions);
                else
                    Render.RenderSprite(FgRenderData, renderOptions);
            }
        }
        protected Vector2 GetForegroundRenderSize(UITextureAtlas.SpriteInfo spriteInfo)
        {
            if (spriteInfo == null)
                return Vector2.zero;

            if (ForegroundSpriteMode == UIForegroundSpriteMode.Stretch)
            {
                return new Vector2(width - SpritePadding.horizontal, height - SpritePadding.vertical) * scaleFactor;
            }
            else if (ForegroundSpriteMode == UIForegroundSpriteMode.Fill)
            {
                return spriteInfo.pixelSize;
            }
            else if (ForegroundSpriteMode == UIForegroundSpriteMode.Scale)
            {
                var widthRatio = Mathf.Max(width - SpritePadding.horizontal, 0f) / spriteInfo.width;
                var heightRatio = Mathf.Max(height - SpritePadding.vertical, 0f) / spriteInfo.height;
                var ratio = Mathf.Min(widthRatio, heightRatio);
                return new Vector2(ratio * spriteInfo.width, ratio * spriteInfo.height) * scaleFactor;
            }
            else
            {
                return Vector2.zero;
            }
        }
        protected Vector2 GetForegroundRenderOffset(Vector2 renderSize)
        {
            Vector2 result = pivot.TransformToUpperLeft(size, arbitraryPivotOffset);
            if (HorizontalAlignment == UIHorizontalAlignment.Left)
            {
                result.x += SpritePadding.left;
            }
            else if (HorizontalAlignment == UIHorizontalAlignment.Center)
            {
                result.x += (width - renderSize.x) * 0.5f;
                result.x += SpritePadding.left - SpritePadding.right;
            }
            else if (HorizontalAlignment == UIHorizontalAlignment.Right)
            {
                result.x += width - renderSize.x;
                result.x -= SpritePadding.right;
            }

            if (VerticalAlignment == UIVerticalAlignment.Bottom)
            {
                result.y -= height - renderSize.y;
                result.y += SpritePadding.bottom;
            }
            else if (VerticalAlignment == UIVerticalAlignment.Middle)
            {
                result.y -= (height - renderSize.y) * 0.5f;
                result.y -= SpritePadding.top - SpritePadding.bottom;
            }
            else if (VerticalAlignment == UIVerticalAlignment.Top)
            {
                result.y -= SpritePadding.top;
            }

            return result;
        }

        #endregion
    }
}
