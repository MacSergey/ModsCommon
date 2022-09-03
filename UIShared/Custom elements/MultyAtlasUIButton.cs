﻿using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModsCommon.UI
{
    public class MultyAtlasUIButton : CustomUIButton
    {
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

        [Obsolete]
        public new Color32 hoveredColor
        {
            get => hoveredBgColor;
            set => hoveredBgColor = value;
        }
        [Obsolete]
        public new Color32 pressedColor
        {
            get => pressedBgColor;
            set => pressedBgColor = value;
        }
        [Obsolete]
        public new Color32 focusedColor
        {
            get => focusedBgColor;
            set => focusedBgColor = value;
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
        private void RenderText()
        {
            if (m_Font == null || !m_Font.isValid)
                return;

            using UIFontRenderer uIFontRenderer = ObtainTextRenderer();
            if (uIFontRenderer is UIDynamicFont.DynamicFontRenderer dynamicFontRenderer)
            {
                dynamicFontRenderer.spriteAtlas = atlasBackground;
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

            var renderer = font.ObtainRenderer();
            renderer.wordWrap = wordWrap;
            renderer.multiLine = true;
            renderer.maxSize = maxSize;
            renderer.pixelRatio = ratio;
            renderer.textScale = textScale;
            renderer.vectorOffset = vectorOffset;
            renderer.textAlign = textHorizontalAlignment;
            renderer.processMarkup = processMarkup;
            renderer.defaultColor = ApplyOpacity(GetTextColorForState());
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
            if (atlasBackground is not UITextureAtlas atlas)
                return null;

            var spriteInfo = state switch
            {
                ButtonState.Normal => atlas[normalBgSprite],
                ButtonState.Focused => atlas[focusedBgSprite],
                ButtonState.Hovered => atlas[hoveredBgSprite],
                ButtonState.Pressed => atlas[pressedBgSprite],
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
                ButtonState.Pressed => atlas[pressedFgSprite],
                ButtonState.Disabled => atlas[disabledFgSprite],
                _ => null,
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
    }
}
