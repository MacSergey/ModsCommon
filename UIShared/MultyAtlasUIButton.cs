using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModsCommon.UI
{
    public class MultyAtlasUIButton : CustomUIButton
    {
        UITextureAtlas _fgAtlas;
        UITextureAtlas _bgAtlas;

        public UITextureAtlas FgAtlas
        {
            get => _fgAtlas ?? atlas;
            set
            {
                if (!Equals(value, _fgAtlas))
                {
                    _fgAtlas = value;
                    Invalidate();
                }
            }
        }
        public UITextureAtlas BgAtlas
        {
            get => _bgAtlas ?? atlas;
            set
            {
                if (!Equals(value, _bgAtlas))
                {
                    _bgAtlas = value;
                    Invalidate();
                }
            }
        }
        public UITextureAtlas Atlas
        {
            set
            {
                _fgAtlas = value;
                _bgAtlas = value;
                atlas = value;
            }
        }

        Color32? _bgColor;
        Color32? _bgFocusedColor;
        Color32? _bgHoveredColor;
        Color32? _bgPressedColor;
        Color32? _bgDisabledColor;

        Color32? _fgColor;
        Color32? _fgFocusedColor;
        Color32? _fgHoveredColor;
        Color32? _fgPressedColor;
        Color32? _fgDisabledColor;

        public Color32 BgColor
        {
            get => _bgColor ?? color;
            set
            {
                _bgColor = value;
                Invalidate();
            }
        }
        public Color32 BgFocusedColor
        {
            get => _bgFocusedColor ?? focusedColor;
            set
            {
                _bgFocusedColor = value;
                Invalidate();
            }
        }
        public Color32 BgHoveredColor
        {
            get => _bgHoveredColor ?? hoveredColor;
            set
            {
                _bgHoveredColor = value;
                Invalidate();
            }
        }
        public Color32 BgPressedColor
        {
            get => _bgPressedColor ?? pressedColor;
            set
            {
                _bgPressedColor = value;
                Invalidate();
            }
        }
        public Color32 BgDisabledColor
        {
            get => _bgDisabledColor ?? disabledColor;
            set
            {
                _bgDisabledColor = value;
                Invalidate();
            }
        }
        public Color32 FgColor
        {
            get => _fgColor ?? color;
            set
            {
                _fgColor = value;
                Invalidate();
            }
        }
        public Color32 FgFocusedColor
        {
            get => _fgFocusedColor ?? focusedColor;
            set
            {
                _bgFocusedColor = value;
                Invalidate();
            }
        }
        public Color32 FgHoveredColor
        {
            get => _fgHoveredColor ?? hoveredColor;
            set
            {
                _fgHoveredColor = value;
                Invalidate();
            }
        }
        public Color32 FgPressedColor
        {
            get => _fgPressedColor ?? pressedColor;
            set
            {
                _fgPressedColor = value;
                Invalidate();
            }
        }
        public Color32 FgDisabledColor
        {
            get => _fgDisabledColor ?? disabledColor;
            set
            {
                _fgDisabledColor = value;
                Invalidate();
            }
        }

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

                RenderBackground();
                RenderForeground();
                RenderText();
            }
        }
        private void RenderText()
        {
            if (m_Font == null || !m_Font.isValid)
                return;

            TextRenderData.material = BgAtlas.material;
            using UIFontRenderer uIFontRenderer = ObtainTextRenderer();
            if (uIFontRenderer is UIDynamicFont.DynamicFontRenderer dynamicFontRenderer)
            {
                dynamicFontRenderer.spriteAtlas = BgAtlas;
                dynamicFontRenderer.spriteBuffer = BgRenderData;
            }
            uIFontRenderer.Render(m_Text, TextRenderData);
        }
        private UIFontRenderer ObtainTextRenderer()
        {
            var vector = size - new Vector2(textPadding.horizontal, textPadding.vertical);
            var maxSize = (autoSize ? (Vector2.one * 2.14748365E+09f) : vector);
            var ratio = PixelsToUnits();
            var vectorOffset = (pivot.TransformToUpperLeft(size, arbitraryPivotOffset) + new Vector3(textPadding.left, -textPadding.top)) * ratio;
            GetTextScaleMultiplier();
            var defaultColor = ApplyOpacity(GetTextColorForState());
            var renderer = font.ObtainRenderer();
            renderer.wordWrap = wordWrap;
            renderer.multiLine = true;
            renderer.maxSize = maxSize;
            renderer.pixelRatio = ratio;
            renderer.textScale = textScale;
            renderer.vectorOffset = vectorOffset;
            renderer.textAlign = textHorizontalAlignment;
            renderer.processMarkup = processMarkup;
            renderer.defaultColor = defaultColor;
            renderer.bottomColor = null;
            renderer.overrideMarkupColors = false;
            renderer.opacity = CalculateOpacity();
            renderer.shadow = useDropShadow;
            renderer.shadowColor = dropShadowColor;
            renderer.shadowOffset = dropShadowOffset;
            renderer.outline = useOutline;
            renderer.outlineSize = outlineSize;
            renderer.outlineColor = outlineColor;
            if (!autoSize && m_TextVerticalAlign != 0)
                renderer.vectorOffset = GetVertAlignOffset(renderer);

            return renderer;
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
                ButtonState.Focused => BgFocusedColor,
                ButtonState.Hovered => BgHoveredColor,
                ButtonState.Pressed => BgPressedColor,
                ButtonState.Disabled => BgDisabledColor,
                _ => BgColor,
            };
        }
        private Color32 GetForegroundColor()
        {
            return state switch
            {
                ButtonState.Focused => FgFocusedColor,
                ButtonState.Hovered => FgHoveredColor,
                ButtonState.Pressed => FgPressedColor,
                ButtonState.Disabled => FgDisabledColor,
                _ => FgColor,
            };
        }
        private Vector3 GetVertAlignOffset(UIFontRenderer fontRenderer)
        {
            var num = PixelsToUnits();
            var vector = fontRenderer.MeasureString(m_Text) * num;
            var vectorOffset = fontRenderer.vectorOffset;
            var num2 = (height - textPadding.vertical) * num;
            if (vector.y >= num2)
                return vectorOffset;

            switch (m_TextVerticalAlign)
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

        protected override UITextureAtlas.SpriteInfo GetBackgroundSprite()
        {
            if (BgAtlas is not UITextureAtlas atlas)
                return null;

            var spriteInfo = state switch
            {
                ButtonState.Normal => atlas[normalBgSprite],
                ButtonState.Focused => atlas[focusedBgSprite],
                ButtonState.Hovered => atlas[hoveredBgSprite],
                ButtonState.Pressed => atlas[pressedBgSprite],
                ButtonState.Disabled => atlas[disabledBgSprite],
            };

            return spriteInfo ?? atlas[normalBgSprite];
        }
        protected override UITextureAtlas.SpriteInfo GetForegroundSprite()
        {
            if (FgAtlas is not UITextureAtlas atlas)
                return null;

            var spriteInfo = state switch
            {
                ButtonState.Normal => atlas[normalFgSprite],
                ButtonState.Focused => atlas[focusedFgSprite],
                ButtonState.Hovered => atlas[hoveredFgSprite],
                ButtonState.Pressed => atlas[pressedFgSprite],
                ButtonState.Disabled => atlas[disabledFgSprite],
            };

            return spriteInfo ?? atlas[normalFgSprite];
        }

        protected override void RenderBackground()
        {
            if (GetBackgroundSprite() is UITextureAtlas.SpriteInfo backgroundSprite)
            {
                var color = ApplyOpacity(GetBackgroundColor());

                var renderOptions = new RenderOptions()
                {
                    _atlas = BgAtlas,
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
                    _atlas = FgAtlas,
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
    }
}
