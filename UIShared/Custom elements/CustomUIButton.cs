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
            State = containsMouse && CanHover ? UIButton.ButtonState.Hovered : UIButton.ButtonState.Normal;
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
                State = CanPress ? UIButton.ButtonState.Pressed : UIButton.ButtonState.Normal;
                base.OnMouseDown(p);
            }
        }
        protected override void OnMouseUp(UIMouseEventParameter p)
        {
            if (m_IsMouseHovering)
                State = CanHover ? UIButton.ButtonState.Hovered : UIButton.ButtonState.Normal;
            else if (hasFocus)
                State = UIButton.ButtonState.Focused;
            else
                State = UIButton.ButtonState.Normal;

            base.OnMouseUp(p);
        }
        protected override void OnMouseEnter(UIMouseEventParameter p)
        {
            State = CanHover ? UIButton.ButtonState.Hovered : UIButton.ButtonState.Normal;

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
        protected override void OnVisibilityChanged()
        {
            if (hasFocus && !isVisible)
            {
                if (this.GetRoot() is UIComponent root)
                    root.Focus();
                else
                    Unfocus();
            }
            base.OnVisibilityChanged();
        }

        #endregion

        [Obsolete]
        protected UIButton.ButtonState state = UIButton.ButtonState.Normal;
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


        protected bool canHover = true;
        public bool CanHover
        {
            get => canHover;
            set
            {
                if (value != canHover)
                {
                    canHover = value;
                    Invalidate();
                }
            }
        }


        protected bool canPress = true;
        public bool CanPress
        {
            get => canPress;
            set
            {
                if (value != canPress)
                {
                    canPress = value;
                    Invalidate();
                }
            }
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


        protected UITextureAtlas iconAtlas;
        public UITextureAtlas IconAtlas
        {
            get => iconAtlas ?? Atlas;
            set
            {
                if (!Equals(value, iconAtlas))
                {
                    iconAtlas = value;
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


        protected SpriteMode iconMode = SpriteMode.Stretch;
        public SpriteMode IconMode
        {
            get => iconMode;
            set
            {
                if (value != iconMode)
                {
                    iconMode = value;
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


        private RectOffset backgroundPadding;
        public RectOffset BackgroundPadding
        {
            get => backgroundPadding ??= new RectOffset();
            set
            {
                if (!Equals(value, backgroundPadding))
                {
                    backgroundPadding = value;
                    Invalidate();
                }
            }
        }


        protected RectOffset foregroundPadding;
        public RectOffset ForegroundPadding
        {
            get => foregroundPadding ??= new RectOffset();
            set
            {
                if (!Equals(value, foregroundPadding))
                {
                    foregroundPadding = value;
                    Invalidate();
                }
            }
        }


        protected RectOffset iconPadding;
        public RectOffset IconPadding
        {
            get => iconPadding ??= new RectOffset();
            set
            {
                if (!Equals(value, iconPadding))
                {
                    iconPadding = value;
                    Invalidate();
                }
            }
        }


        protected Vector2 iconSize;
        public Vector2 IconSize
        {
            get => iconSize;
            set
            {
                if (value != iconSize)
                {
                    iconSize = value;
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
        public SpriteSet AllIconSprites
        {
            set
            {
                iconSprites = value;
                selIconSprites = value;
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
        public ColorSet AllIconColors
        {
            set
            {
                iconColors = value;
                selIconColors = value;
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

        #region ICON SPRITE

        protected SpriteSet iconSprites;
        public SpriteSet IconSprites
        {
            get => iconSprites;
            set
            {
                iconSprites = value;
                Invalidate();
            }
        }

        public string NormalIconSprite
        {
            get => iconSprites.normal;
            set
            {
                if (value != iconSprites.normal)
                {
                    iconSprites.normal = value;
                    Invalidate();
                }
            }
        }
        public string HoveredIconSprite
        {
            get => iconSprites.hovered;
            set
            {
                if (value != iconSprites.hovered)
                {
                    iconSprites.hovered = value;
                    Invalidate();
                }
            }
        }
        public string PressedIconSprite
        {
            get => iconSprites.pressed;
            set
            {
                if (value != iconSprites.pressed)
                {
                    iconSprites.pressed = value;
                    Invalidate();
                }
            }
        }
        public string FocusedIconSprite
        {
            get => iconSprites.focused;
            set
            {
                if (value != iconSprites.focused)
                {
                    iconSprites.focused = value;
                    Invalidate();
                }
            }
        }
        public string DisabledIconSprite
        {
            get => iconSprites.disabled;
            set
            {
                if (value != iconSprites.disabled)
                {
                    iconSprites.disabled = value;
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
            get => selBgSprites.normal;
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
            get => selBgSprites.hovered;
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
            get => selBgSprites.pressed;
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
            get => selBgSprites.focused;
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
            get => selBgSprites.disabled;
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
            get => selFgSprites.normal;
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
            get => selFgSprites.hovered;
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
            get => selFgSprites.pressed;
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
            get => selFgSprites.focused;
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
            get => selFgSprites.disabled;
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

        #region SELECTED ICON SPRITE

        protected SpriteSet selIconSprites;
        public SpriteSet SelIconSprites
        {
            get => selIconSprites;
            set
            {
                selIconSprites = value;
                Invalidate();
            }
        }

        public string SelNormalIconSprite
        {
            get => selIconSprites.normal;
            set
            {
                if (value != selIconSprites.normal)
                {
                    selIconSprites.normal = value;
                    Invalidate();
                }
            }
        }
        public string SelHoveredIconSprite
        {
            get => selIconSprites.hovered;
            set
            {
                if (value != selIconSprites.hovered)
                {
                    selIconSprites.hovered = value;
                    Invalidate();
                }
            }
        }
        public string SelPressedIconSprite
        {
            get => selIconSprites.pressed;
            set
            {
                if (value != selIconSprites.pressed)
                {
                    selIconSprites.pressed = value;
                    Invalidate();
                }
            }
        }
        public string SelFocusedIconSprite
        {
            get => selIconSprites.focused;
            set
            {
                if (value != selIconSprites.focused)
                {
                    selIconSprites.focused = value;
                    Invalidate();
                }
            }
        }
        public string SelDisabledIconSprite
        {
            get => selIconSprites.disabled;
            set
            {
                if (value != selIconSprites.disabled)
                {
                    selIconSprites.disabled = value;
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

        #region ICON COLOR

        protected ColorSet iconColors = new ColorSet(Color.white);
        public ColorSet IconColors
        {
            get => iconColors;
            set
            {
                iconColors = value;
                OnColorChanged();
            }
        }

        public Color32 NormalIconColor
        {
            get => iconColors.normal;
            set
            {
                if (!iconColors.normal.Equals(value))
                {
                    iconColors.normal = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 FocusedIconColor
        {
            get => iconColors.focused;
            set
            {
                if (!iconColors.focused.Equals(value))
                {
                    iconColors.focused = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 HoveredIconColor
        {
            get => iconColors.hovered;
            set
            {
                if (!iconColors.hovered.Equals(value))
                {
                    iconColors.hovered = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 PressedIconColor
        {
            get => iconColors.pressed;
            set
            {
                if (!iconColors.pressed.Equals(value))
                {
                    iconColors.pressed = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 DisabledIconColor
        {
            get => iconColors.disabled;
            set
            {
                if (!iconColors.disabled.Equals(value))
                {
                    iconColors.disabled = value;
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

        #region SELECTED ICON COLOR

        protected ColorSet selIconColors = new ColorSet(Color.white);
        public ColorSet SelIconColors
        {
            get => selIconColors;
            set
            {
                selIconColors = value;
                OnColorChanged();
            }
        }

        public Color32 SelNormalIconColor
        {
            get => string.IsNullOrEmpty(selIconSprites.normal) ? NormalIconColor : selIconColors.normal;
            set
            {
                if (!selIconColors.normal.Equals(value))
                {
                    selIconColors.normal = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 SelHoveredIconColor
        {
            get => string.IsNullOrEmpty(selIconSprites.hovered) ? FocusedIconColor : selIconColors.hovered;
            set
            {
                if (!selIconColors.hovered.Equals(value))
                {
                    selIconColors.hovered = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 SelPressedIconColor
        {
            get => string.IsNullOrEmpty(selIconSprites.pressed) ? PressedIconColor : selIconColors.pressed;
            set
            {
                if (!selIconColors.pressed.Equals(value))
                {
                    selIconColors.pressed = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 SelFocusedIconColor
        {
            get => string.IsNullOrEmpty(selIconSprites.focused) ? FocusedIconColor : selIconColors.focused;
            set
            {
                if (!selIconColors.focused.Equals(value))
                {
                    selIconColors.focused = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 SelDisabledIconColor
        {
            get => string.IsNullOrEmpty(selIconSprites.disabled) ? DisabledIconColor : selIconColors.disabled;
            set
            {
                if (!selIconColors.disabled.Equals(value))
                {
                    selIconColors.disabled = value;
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
            get => selTextColors.normal;
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
            get => selTextColors.hovered;
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
            get => selTextColors.pressed;
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
            get => selTextColors.focused;
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
            get => selTextColors.disabled;
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
                iconAtlas = value.IconAtlas;

                bgSprites = value.BgSprites;
                selBgSprites = value.SelBgSprites;

                fgSprites = value.FgSprites;
                selFgSprites = value.SelFgSprites;

                iconSprites = value.IconSprites;
                selIconSprites = value.SelIconSprites;

                bgColors = value.BgColors;
                selBgColors = value.SelBgColors;

                fgColors = value.FgColors;
                selFgColors = value.SelFgColors;

                iconColors = value.IconColors;
                selIconColors = value.SelIconColors;

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
                size = MeasuredSize;
        }
        public void PerformAutoWidth()
        {
            if (m_Font != null && m_Font.isValid && !string.IsNullOrEmpty(m_Text))
                width = MeasuredSize.x;
        }
        public void PerformAutoHeight()
        {
            if (m_Font != null && m_Font.isValid && !string.IsNullOrEmpty(m_Text))
                height = MeasuredSize.y;
        }
        public Vector2 MeasuredSize
        {
            get
            {
                using UIFontRenderer uIFontRenderer = ObtainTextRenderer();
                var measured = uIFontRenderer.MeasureString(text).RoundToInt();
                measured += new Vector2(TextPadding.horizontal, TextPadding.vertical);
                measured = Vector2.Max(measured, minimumSize);
                return measured;
            }
        }

        #region RENDER

        protected UIRenderData BgRenderData { get; set; }
        protected UIRenderData FgRenderData { get; set; }
        protected UIRenderData IconRenderData { get; set; }
        protected UIRenderData TextRenderData { get; set; }

        public override void OnDisable()
        {
            BgRenderData = null;
            FgRenderData = null;
            IconRenderData = null;
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

            if (IconRenderData == null)
            {
                IconRenderData = UIRenderData.Obtain();
                m_RenderData.Add(IconRenderData);
            }
            else
                IconRenderData.Clear();

            if (TextRenderData == null)
            {
                TextRenderData = UIRenderData.Obtain();
                m_RenderData.Add(TextRenderData);
            }
            else
                TextRenderData.Clear();

            RenderBackground();
            RenderForeground();
            RenderIcon();
            RenderText();
        }

        protected virtual Color32 RenderTextColor
        {
            get
            {
                if (!isEnabled)
                    return IsSelected ? SelDisabledTextColor : DisabledTextColor;
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
        private void RenderText()
        {
            if (font != null && font.isValid && !string.IsNullOrEmpty(text) && IconAtlas is UITextureAtlas iconAtlas)
            {
                TextRenderData.material = iconAtlas.material;
                using UIFontRenderer uIFontRenderer = ObtainTextRenderer();
                if (uIFontRenderer is UIDynamicFont.DynamicFontRenderer dynamicFontRenderer)
                {
                    dynamicFontRenderer.spriteAtlas = iconAtlas;
                    dynamicFontRenderer.spriteBuffer = IconRenderData;
                }
                uIFontRenderer.Render(text, TextRenderData);
            }
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

                return State switch
                {
                    UIButton.ButtonState.Normal => atlas[IsSelected ? SelNormalBgSprite : NormalBgSprite],
                    UIButton.ButtonState.Focused => atlas[IsSelected ? SelFocusedBgSprite : FocusedBgSprite],
                    UIButton.ButtonState.Hovered => atlas[IsSelected ? SelHoveredBgSprite : HoveredBgSprite],
                    UIButton.ButtonState.Pressed => atlas[IsSelected ? SelPressedBgSprite : PressedBgSprite],
                    UIButton.ButtonState.Disabled => atlas[IsSelected ? SelDisabledBgSprite : DisabledBgSprite],
                    _ => null,
                };
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
        protected void RenderBackground()
        {
            if (BgAtlas is UITextureAtlas bgAtlas && RenderBackgroundSprite is UITextureAtlas.SpriteInfo backgroundSprite)
            {
                BgRenderData.material = bgAtlas.material;

                var backgroundRenderSize = GetBackgroundRenderSize(backgroundSprite);
                var backgroundRenderOffset = GetBackfroundRenderOffset(backgroundRenderSize);

                var renderOptions = new RenderOptions()
                {
                    atlas = bgAtlas,
                    color = RenderBackgroundColor,
                    fillAmount = 1f,
                    flip = UISpriteFlip.None,
                    offset = backgroundRenderOffset,
                    pixelsToUnits = PixelsToUnits(),
                    size = backgroundRenderSize,
                    spriteInfo = backgroundSprite,
                };

                if (backgroundSprite.isSliced)
                    Render.RenderSlicedSprite(BgRenderData, renderOptions);
                else
                    Render.RenderSprite(BgRenderData, renderOptions);
            }
        }
        protected virtual Vector2 GetBackgroundRenderSize(UITextureAtlas.SpriteInfo spriteInfo)
        {
            if (spriteInfo == null)
                return Vector2.zero;
            else
                return new Vector2(width - BackgroundPadding.horizontal, height - BackgroundPadding.vertical);
        }
        protected virtual Vector2 GetBackfroundRenderOffset(Vector2 renderSize)
        {
            Vector2 result = pivot.TransformToUpperLeft(size, arbitraryPivotOffset);

            result.x += BackgroundPadding.left;
            result.y -= BackgroundPadding.top;

            return result;
        }


        protected virtual UITextureAtlas.SpriteInfo RenderForegroundSprite
        {
            get
            {
                if (FgAtlas is not UITextureAtlas atlas)
                    return null;

                return State switch
                {
                    UIButton.ButtonState.Normal => atlas[IsSelected ? SelNormalFgSprite : NormalFgSprite],
                    UIButton.ButtonState.Focused => atlas[IsSelected ? SelFocusedFgSprite : FocusedFgSprite],
                    UIButton.ButtonState.Hovered => atlas[IsSelected ? SelHoveredFgSprite : HoveredFgSprite],
                    UIButton.ButtonState.Pressed => atlas[IsSelected ? SelPressedFgSprite : PressedFgSprite],
                    UIButton.ButtonState.Disabled => atlas[IsSelected ? SelDisabledFgSprite : DisabledFgSprite],
                    _ => null,
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
        protected void RenderForeground()
        {
            if (FgAtlas is UITextureAtlas fgAtlas && RenderForegroundSprite is UITextureAtlas.SpriteInfo foregroundSprite)
            {
                FgRenderData.material = fgAtlas.material;

                var foregroundRenderSize = GetForegroundRenderSize(foregroundSprite);
                var foregroundRenderOffset = GetForegroundRenderOffset(foregroundRenderSize);

                var renderOptions = new RenderOptions()
                {
                    atlas = fgAtlas,
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
            else
                return new Vector2(width - ForegroundPadding.horizontal, height - ForegroundPadding.vertical);
            //switch (IconMode)
            //{
            //    case SpriteMode.Stretch:
            //        return new Vector2(width - ForegroundPadding.horizontal, height - ForegroundPadding.vertical) * scaleFactor;
            //    case SpriteMode.Fill:
            //        return spriteInfo.pixelSize;
            //    case SpriteMode.Scale:
            //        var widthRatio = Mathf.Max(width - ForegroundPadding.horizontal, 0f) / spriteInfo.width;
            //        var heightRatio = Mathf.Max(height - ForegroundPadding.vertical, 0f) / spriteInfo.height;
            //        var ratio = Mathf.Min(widthRatio, heightRatio);
            //        return new Vector2(ratio * spriteInfo.width, ratio * spriteInfo.height) * scaleFactor;
            //    case SpriteMode.FixedSize:
            //        return IconSize;
            //    default:
            //        return Vector2.zero;
            //}
        }
        protected Vector2 GetForegroundRenderOffset(Vector2 renderSize)
        {
            Vector2 result = pivot.TransformToUpperLeft(size, arbitraryPivotOffset);
            if (HorizontalAlignment == UIHorizontalAlignment.Left)
            {
                result.x += ForegroundPadding.left;
            }
            else if (HorizontalAlignment == UIHorizontalAlignment.Center)
            {
                result.x += (width - renderSize.x) * 0.5f;
                result.x += ForegroundPadding.left - ForegroundPadding.right;
            }
            else if (HorizontalAlignment == UIHorizontalAlignment.Right)
            {
                result.x += width - renderSize.x;
                result.x -= ForegroundPadding.right;
            }

            if (VerticalAlignment == UIVerticalAlignment.Bottom)
            {
                result.y -= height - renderSize.y;
                result.y += ForegroundPadding.bottom;
            }
            else if (VerticalAlignment == UIVerticalAlignment.Middle)
            {
                result.y -= (height - renderSize.y) * 0.5f;
                result.y -= ForegroundPadding.top - ForegroundPadding.bottom;
            }
            else if (VerticalAlignment == UIVerticalAlignment.Top)
            {
                result.y -= ForegroundPadding.top;
            }

            return result;
        }


        protected virtual UITextureAtlas.SpriteInfo RenderIconSprite
        {
            get
            {
                if (IconAtlas is not UITextureAtlas atlas)
                    return null;

                return State switch
                {
                    UIButton.ButtonState.Normal => atlas[IsSelected ? SelNormalIconSprite : NormalIconSprite],
                    UIButton.ButtonState.Focused => atlas[IsSelected ? SelFocusedIconSprite : FocusedIconSprite],
                    UIButton.ButtonState.Hovered => atlas[IsSelected ? SelHoveredIconSprite : HoveredIconSprite],
                    UIButton.ButtonState.Pressed => atlas[IsSelected ? SelPressedIconSprite : PressedIconSprite],
                    UIButton.ButtonState.Disabled => atlas[IsSelected ? SelDisabledIconSprite : DisabledIconSprite],
                    _ => null,
                };
            }
        }
        protected virtual Color32 RenderIconColor
        {
            get
            {
                return State switch
                {
                    UIButton.ButtonState.Focused => IsSelected ? SelFocusedIconColor : FocusedIconColor,
                    UIButton.ButtonState.Hovered => IsSelected ? SelHoveredIconColor : HoveredIconColor,
                    UIButton.ButtonState.Pressed => IsSelected ? SelPressedIconColor : PressedIconColor,
                    UIButton.ButtonState.Disabled => IsSelected ? SelDisabledIconColor : DisabledIconColor,
                    _ => IsSelected ? SelNormalIconColor : NormalIconColor,
                };
            }
        }
        protected void RenderIcon()
        {
            if (IconAtlas is UITextureAtlas iconAtlas && RenderIconSprite is UITextureAtlas.SpriteInfo iconSprite)
            {
                IconRenderData.material = iconAtlas.material;

                var iconRenderSize = GetIconRenderSize(iconSprite);
                var iconRenderOffset = GetIconRenderOffset(iconRenderSize);

                var renderOptions = new RenderOptions()
                {
                    atlas = iconAtlas,
                    color = RenderIconColor,
                    fillAmount = 1f,
                    flip = UISpriteFlip.None,
                    offset = iconRenderOffset,
                    pixelsToUnits = PixelsToUnits(),
                    size = iconRenderSize,
                    spriteInfo = iconSprite,
                };

                if (iconSprite.isSliced)
                    Render.RenderSlicedSprite(IconRenderData, renderOptions);
                else
                    Render.RenderSprite(IconRenderData, renderOptions);
            }
        }
        protected Vector2 GetIconRenderSize(UITextureAtlas.SpriteInfo spriteInfo)
        {
            if (spriteInfo == null)
                return Vector2.zero;

            switch (IconMode)
            {
                case SpriteMode.Stretch:
                    return new Vector2(width - IconPadding.horizontal, height - IconPadding.vertical) * scaleFactor;
                case SpriteMode.Fill:
                    return spriteInfo.pixelSize;
                case SpriteMode.Scale:
                    var widthRatio = Mathf.Max(width - IconPadding.horizontal, 0f) / spriteInfo.width;
                    var heightRatio = Mathf.Max(height - IconPadding.vertical, 0f) / spriteInfo.height;
                    var ratio = Mathf.Min(widthRatio, heightRatio);
                    return new Vector2(ratio * spriteInfo.width, ratio * spriteInfo.height) * scaleFactor;
                case SpriteMode.FixedSize:
                    return IconSize;
                default:
                    return Vector2.zero;
            }
        }
        protected Vector2 GetIconRenderOffset(Vector2 renderSize)
        {
            Vector2 result = pivot.TransformToUpperLeft(size, arbitraryPivotOffset);
            if (HorizontalAlignment == UIHorizontalAlignment.Left)
            {
                result.x += IconPadding.left;
            }
            else if (HorizontalAlignment == UIHorizontalAlignment.Center)
            {
                result.x += (width - renderSize.x) * 0.5f;
                result.x += IconPadding.left - IconPadding.right;
            }
            else if (HorizontalAlignment == UIHorizontalAlignment.Right)
            {
                result.x += width - renderSize.x;
                result.x -= IconPadding.right;
            }

            if (VerticalAlignment == UIVerticalAlignment.Bottom)
            {
                result.y -= height - renderSize.y;
                result.y += IconPadding.bottom;
            }
            else if (VerticalAlignment == UIVerticalAlignment.Middle)
            {
                result.y -= (height - renderSize.y) * 0.5f;
                result.y -= IconPadding.top - IconPadding.bottom;
            }
            else if (VerticalAlignment == UIVerticalAlignment.Top)
            {
                result.y -= IconPadding.top;
            }

            return result;
        }

        #endregion
    }
}
