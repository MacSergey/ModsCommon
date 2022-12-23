using ColossalFramework.UI;
using UnityEngine;
using static ColossalFramework.UI.UIButton;

namespace ModsCommon.UI
{
    public class CustomUIDropDown : UIDropDown
    {
        public event PropertyChangedEventHandler<ButtonState> eventButtonStateChanged;

        private Vector3 positionBefore;

        protected ButtonState m_State;
        public ButtonState state
        {
            get => m_State;
            set
            {
                if (value != m_State)
                {
                    OnButtonStateChanged(value);
                    Invalidate();
                }
            }
        }

        UITextureAtlas _atlasForeground;
        UITextureAtlas _atlasBackground;

        public UITextureAtlas atlasForeground
        {
            get => _atlasForeground ?? atlas;
            set
            {
                if (!Equals(value, _atlasForeground))
                {
                    _atlasForeground = value;
                    Invalidate();
                }
            }
        }
        public UITextureAtlas atlasBackground
        {
            get => _atlasBackground ?? atlas;
            set
            {
                if (!Equals(value, _atlasBackground))
                {
                    _atlasBackground = value;
                    Invalidate();
                }
            }
        }
        public UITextureAtlas Atlas
        {
            set
            {
                _atlasForeground = value;
                _atlasBackground = value;
                atlas = value;
            }
        }

        Color32? _normalBgColor;
        Color32? _focusedBgColor;
        Color32? _hoveredBgColor;
        Color32? _pressedBgColor;
        Color32? _disabledBgColor;

        Color32? _normalFgColor;
        Color32? _focusedFgColor;
        Color32? _hoveredFgColor;
        Color32? _pressedFgColor;
        Color32? _disabledFgColor;

        Color32? _focusedTextColor;
        Color32? _hoveredTextColor;
        Color32? _pressedTextColor;

        public Color32 normalBgColor
        {
            get => _normalBgColor ?? color;
            set
            {
                _normalBgColor = value;
                Invalidate();
            }
        }
        public Color32 focusedBgColor
        {
            get => _focusedBgColor ?? color;
            set
            {
                _focusedBgColor = value;
                Invalidate();
            }
        }
        public Color32 hoveredBgColor
        {
            get => _hoveredBgColor ?? color;
            set
            {
                _hoveredBgColor = value;
                Invalidate();
            }
        }
        public Color32 pressedBgColor
        {
            get => _pressedBgColor ?? color;
            set
            {
                _pressedBgColor = value;
                Invalidate();
            }
        }
        public Color32 disabledBgColor
        {
            get => _disabledBgColor ?? disabledColor;
            set
            {
                _disabledBgColor = value;
                Invalidate();
            }
        }

        public Color32 normalFgColor
        {
            get => _normalFgColor ?? color;
            set
            {
                _normalFgColor = value;
                Invalidate();
            }
        }
        public Color32 focusedFgColor
        {
            get => _focusedFgColor ?? color;
            set
            {
                _focusedFgColor = value;
                Invalidate();
            }
        }
        public Color32 hoveredFgColor
        {
            get => _hoveredFgColor ?? color;
            set
            {
                _hoveredFgColor = value;
                Invalidate();
            }
        }
        public Color32 pressedFgColor
        {
            get => _pressedFgColor ?? color;
            set
            {
                _pressedFgColor = value;
                Invalidate();
            }
        }
        public Color32 disabledFgColor
        {
            get => _disabledFgColor ?? disabledColor;
            set
            {
                _disabledFgColor = value;
                Invalidate();
            }
        }

        public Color32 focusedTextColor
        {
            get => _focusedTextColor ?? textColor;
            set
            {
                _focusedTextColor = value;
                Invalidate();
            }
        }
        public Color32 hoveredTextColor
        {
            get => _hoveredTextColor ?? textColor;
            set
            {
                _hoveredTextColor = value;
                Invalidate();
            }
        }
        public Color32 pressedTextColor
        {
            get => _pressedTextColor ?? textColor;
            set
            {
                _pressedTextColor = value;
                Invalidate();
            }
        }

        protected UIRenderData BgRenderData { get; set; }
        protected UIRenderData FgRenderData { get; set; }
        protected UIRenderData TextRenderData { get; set; }

        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout()
        {
            if ((relativePosition - positionBefore).sqrMagnitude > 0.001)
                relativePosition = positionBefore;
        }

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

            if (atlasBackground is UITextureAtlas bgAtlas && atlasForeground is UITextureAtlas fgAtlas)
            {
                BgRenderData.material = bgAtlas.material;
                FgRenderData.material = fgAtlas.material;
                TextRenderData.material = bgAtlas.material;

                RenderBackground();
                RenderForeground();
                RenderText();
            }
        }

        protected override void RenderBackground()
        {
            if (GetBackgroundSprite() is UITextureAtlas.SpriteInfo backgroundSprite)
            {
                var color = ApplyOpacity(GetBackgroundColor());
                RenderOptions renderOptions = new RenderOptions()
                {
                    _atlas = atlasBackground,
                    _color = color,
                    _fillAmount = 1f,
                    _flip = UISpriteFlip.None,
                    _offset = pivot.TransformToUpperLeft(size, arbitraryPivotOffset),
                    _pixelsToUnits = PixelsToUnits(),
                    _size = size,
                    _spriteInfo = backgroundSprite,
                };

                if (backgroundSprite.isSliced)
                    Render.RenderSlicedSprite(BgRenderData, renderOptions);
                else
                    Render.RenderSprite(BgRenderData, renderOptions);
            }
        }
        protected override void RenderForeground()
        {
            if (GetForegroundSprite() is UITextureAtlas.SpriteInfo foregroundSprite)
            {
                var foregroundRenderSize = GetForegroundRenderSize(foregroundSprite);
                var foregroundRenderOffset = GetForegroundRenderOffset(foregroundRenderSize);
                var color = ApplyOpacity(GetForegroundColor());

                var renderOptions = new RenderOptions()
                {
                    _atlas = atlasForeground,
                    _color = color,
                    _fillAmount = 1f,
                    _flip = UISpriteFlip.None,
                    _offset = foregroundRenderOffset,
                    _pixelsToUnits = PixelsToUnits(),
                    _size = foregroundRenderSize,
                    _spriteInfo = foregroundSprite,
                };

                if (foregroundSprite.isSliced)
                    Render.RenderSlicedSprite(FgRenderData, renderOptions);
                else
                    Render.RenderSprite(FgRenderData, renderOptions);
            }
        }
        private void RenderText()
        {
            if (m_Font == null || !m_Font.isValid)
                return;

            if (selectedIndex < 0 || selectedIndex >= items.Length)
                return;

            using UIFontRenderer uIFontRenderer = ObtainTextRenderer();
            if (uIFontRenderer is UIDynamicFont.DynamicFontRenderer dynamicFontRenderer)
            {
                dynamicFontRenderer.spriteAtlas = atlasBackground;
                dynamicFontRenderer.spriteBuffer = BgRenderData;
            }
            uIFontRenderer.Render(items[selectedIndex], TextRenderData);
        }
        private UIFontRenderer ObtainTextRenderer()
        {
            var num = PixelsToUnits();
            var maxSize = new Vector2(size.x - textFieldPadding.horizontal, size.y - textFieldPadding.vertical);
            var vector = pivot.TransformToUpperLeft(size, arbitraryPivotOffset);
            var vectorOffset = new Vector3(vector.x + textFieldPadding.left, vector.y - textFieldPadding.top, 0f) * num;

            var renderer = font.ObtainRenderer();
            renderer.wordWrap = false;
            renderer.maxSize = maxSize;
            renderer.pixelRatio = num;
            renderer.textScale = textScale;
            renderer.characterSpacing = characterSpacing;
            renderer.vectorOffset = vectorOffset;
            renderer.multiLine = false;
            renderer.textAlign = UIHorizontalAlignment.Left;
            renderer.processMarkup = processMarkup;
            renderer.colorizeSprites = colorizeSprites;
            renderer.defaultColor = ApplyOpacity(GetTextColorForState());
            renderer.bottomColor = useGradient ? new Color32?(bottomColor) : null;
            renderer.overrideMarkupColors = false;
            renderer.opacity = CalculateOpacity();
            renderer.outline = useOutline;
            renderer.outlineSize = outlineSize;
            renderer.outlineColor = outlineColor;
            renderer.shadow = useDropShadow;
            renderer.shadowColor = dropShadowColor;
            renderer.shadowOffset = dropShadowOffset;

            return renderer;
        }

        protected virtual void OnButtonStateChanged(ButtonState value)
        {
            if (isEnabled || value == ButtonState.Disabled)
            {
                m_State = value;
                if (eventButtonStateChanged != null)
                    eventButtonStateChanged(this, value);

                Invoke("OnButtonStateChanged", value);
                Invalidate();
            }
        }

        protected override void OnEnterFocus(UIFocusEventParameter p)
        {
            if (state != ButtonState.Pressed)
                state = ButtonState.Focused;

            base.OnEnterFocus(p);
        }
        protected override void OnLeaveFocus(UIFocusEventParameter p)
        {
            state = containsMouse ? ButtonState.Hovered : ButtonState.Normal;
            base.OnLeaveFocus(p);
        }
        protected override void OnMouseDown(UIMouseEventParameter p)
        {
            if (state != ButtonState.Focused)
                state = ButtonState.Pressed;

            base.OnMouseDown(p);
        }
        protected override void OnMouseUp(UIMouseEventParameter p)
        {
            if (m_IsMouseHovering)
            {
                if (containsFocus)
                    state = ButtonState.Focused;
                else
                    state = ButtonState.Hovered;
            }
            else if (hasFocus)
                state = ButtonState.Focused;
            else
                state = ButtonState.Normal;

            base.OnMouseUp(p);
        }
        protected override void OnMouseEnter(UIMouseEventParameter p)
        {
            if (state != ButtonState.Focused)
                state = ButtonState.Hovered;

            base.OnMouseEnter(p);
        }
        protected override void OnMouseLeave(UIMouseEventParameter p)
        {
            if (containsFocus)
                state = ButtonState.Focused;
            else
                state = ButtonState.Normal;

            base.OnMouseLeave(p);
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            listWidth = (int)width;
            if (triggerButton is UIComponent button && button != this)
                button.size = size;
        }
        protected override void OnIsEnabledChanged()
        {
            if (!isEnabled)
                state = ButtonState.Disabled;
            else
                state = ButtonState.Normal;

            base.OnIsEnabledChanged();
        }

        private Color32 GetTextColorForState()
        {
            if (!isEnabled)
                return disabledTextColor;
            else
            {
                return state switch
                {
                    ButtonState.Normal => textColor,
                    ButtonState.Focused => focusedTextColor,
                    ButtonState.Hovered => hoveredTextColor,
                    ButtonState.Pressed => pressedTextColor,
                    ButtonState.Disabled => disabledTextColor,
                    _ => Color.white,
                };
            }
        }
        private Color32 GetBackgroundColor()
        {
            return state switch
            {
                ButtonState.Focused => focusedBgColor,
                ButtonState.Hovered => hoveredBgColor,
                ButtonState.Pressed => pressedBgColor,
                ButtonState.Disabled => disabledBgColor,
                _ => normalBgColor,
            };
        }
        private Color32 GetForegroundColor()
        {
            return state switch
            {
                ButtonState.Focused => focusedFgColor,
                ButtonState.Hovered => hoveredFgColor,
                ButtonState.Pressed => pressedFgColor,
                ButtonState.Disabled => disabledFgColor,
                _ => normalFgColor,
            };
        }

        protected override UITextureAtlas.SpriteInfo GetBackgroundSprite()
        {
            if (atlasBackground is not UITextureAtlas atlas)
                return null;

            var spriteInfo = state switch
            {
                ButtonState.Normal => atlas[normalBgSprite],
                ButtonState.Focused => atlas[focusedBgSprite],
                ButtonState.Hovered => atlas[hoveredBgSprite],
                //ButtonState.Pressed => atlas[pressedBgSprite],
                ButtonState.Disabled => atlas[disabledBgSprite],
                _ => null,
            };

            return spriteInfo ?? atlas[normalBgSprite];
        }
        protected override UITextureAtlas.SpriteInfo GetForegroundSprite()
        {
            if (atlasForeground is not UITextureAtlas atlas)
                return null;

            var spriteInfo = state switch
            {
                ButtonState.Normal => atlas[normalFgSprite],
                ButtonState.Focused => atlas[focusedFgSprite],
                ButtonState.Hovered => atlas[hoveredFgSprite],
                //ButtonState.Pressed => atlas[pressedFgSprite],
                ButtonState.Disabled => atlas[disabledFgSprite],
                _ => null,
            };

            return spriteInfo ?? atlas[normalFgSprite];
        }
    }
}
