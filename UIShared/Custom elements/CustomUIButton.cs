using ColossalFramework.UI;
using System;
using UnityEngine;

namespace ModsCommon.UI
{
    public class CustomUIButton : UIButton
    {
        UITextureAtlas _atlasForeground;
        UITextureAtlas _atlasBackground;

        private bool _isSelected;
        public bool isSelected
        {
            get => _isSelected;
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    Invalidate();
                }
            }
        }

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
        public new UITextureAtlas atlas
        {
            get => base.atlas;
            set
            {
                _atlasForeground = null;
                _atlasBackground = null;
                base.atlas = value;
            }
        }

        public new Color32 color
        {
            get => base.color;
            set
            {
                bgColors.normal = null;
                base.color = value;
            }
        }
        public new Color32 hoveredColor
        {
            get => base.hoveredColor;
            set
            {
                bgColors.hovered = null;
                base.hoveredColor = value;
            }
        }
        public new Color32 pressedColor
        {
            get => base.pressedColor;
            set
            {
                bgColors.pressed = null;
                base.pressedColor = value;
            }
        }
        public new Color32 focusedColor
        {
            get => base.focusedColor;
            set
            {
                bgColors.focused = null;
                base.focusedColor = value;
            }
        }
        public new Color32 disabledColor
        {
            get => base.disabledColor;
            set
            {
                bgColors.disabled = null;
                base.disabledColor = value;
            }
        }

        public void SetBgSprite(UI.SpriteSet sprites)
        {
            m_BackgroundSprites.m_Normal = sprites.normal;
            m_BackgroundSprites.m_Hovered = sprites.hovered;
            m_BackgroundSprites.m_Focused = sprites.focused;
            m_BackgroundSprites.m_Disabled = sprites.disabled;
            m_PressedBgSprite = sprites.pressed;
            Invalidate();
        }
        public void SetFgSprite(UI.SpriteSet sprites)
        {
            m_ForegroundSprites.m_Normal = sprites.normal;
            m_ForegroundSprites.m_Hovered = sprites.hovered;
            m_ForegroundSprites.m_Focused = sprites.focused;
            m_ForegroundSprites.m_Disabled = sprites.disabled;
            m_PressedFgSprite = sprites.pressed;
            Invalidate();
        }
        public void SetTextColor(ColorSet colors)
        {
            m_TextColor = colors.normal ?? new Color32(255, 255, 255, 255);
            m_HoveredTextColor = colors.hovered ?? new Color32(255, 255, 255, 255);
            m_FocusedTextColor = colors.focused ?? new Color32(255, 255, 255, 255);
            m_DisabledTextColor = colors.disabled ?? new Color32(255, 255, 255, 255);
            m_PressedTextColor = colors.pressed ?? new Color32(255, 255, 255, 255);
            Invalidate();
        }

        #region BACKGROUND COLOR

        ColorSet bgColors;

        public Color32 normalBgColor
        {
            get => bgColors.normal ?? base.color;
            set
            {
                if (!bgColors.normal.Equals(value))
                {
                    bgColors.normal = value;
                    Invalidate();
                }
            }
        }
        public Color32 focusedBgColor
        {
            get => bgColors.focused ?? base.focusedColor;
            set
            {
                if (!bgColors.focused.Equals(value))
                {
                    bgColors.focused = value;
                    Invalidate();
                }
            }
        }
        public Color32 hoveredBgColor
        {
            get => bgColors.hovered ?? base.hoveredColor;
            set
            {
                if (!bgColors.hovered.Equals(value))
                {
                    bgColors.hovered = value;
                    Invalidate();
                }
            }
        }
        public Color32 pressedBgColor
        {
            get => bgColors.pressed ?? base.pressedColor;
            set
            {
                if (!bgColors.pressed.Equals(value))
                {
                    bgColors.pressed = value;
                    Invalidate();
                }
            }
        }
        public Color32 disabledBgColor
        {
            get => bgColors.disabled ?? base.disabledColor;
            set
            {
                if (!bgColors.disabled.Equals(value))
                {
                    bgColors.disabled = value;
                    Invalidate();
                }
            }
        }

        public void SetBgColor(ColorSet colors)
        {
            bgColors = colors;
            Invalidate();
        }

        #endregion

        #region FOREGROUND COLOR

        ColorSet fgColors;

        public Color32 normalFgColor
        {
            get => fgColors.normal ?? base.textColor;
            set
            {
                if (!fgColors.normal.Equals(value))
                {
                    fgColors.normal = value;
                    Invalidate();
                }
            }
        }
        public Color32 focusedFgColor
        {
            get => fgColors.focused ?? base.focusedTextColor;
            set
            {
                if (!fgColors.focused.Equals(value))
                {
                    fgColors.focused = value;
                    Invalidate();
                }
            }
        }
        public Color32 hoveredFgColor
        {
            get => fgColors.hovered ?? base.hoveredTextColor;
            set
            {
                if (!fgColors.hovered.Equals(value))
                {
                    fgColors.hovered = value;
                    Invalidate();
                }
            }
        }
        public Color32 pressedFgColor
        {
            get => fgColors.pressed ?? base.pressedTextColor;
            set
            {
                if (!fgColors.pressed.Equals(value))
                {
                    fgColors.pressed = value;
                    Invalidate();
                }
            }
        }
        public Color32 disabledFgColor
        {
            get => fgColors.disabled ?? base.disabledTextColor;
            set
            {
                if (!fgColors.disabled.Equals(value))
                {
                    fgColors.disabled = value;
                    Invalidate();
                }
            }
        }

        public void SetFgColor(ColorSet colors)
        {
            fgColors = colors;
            Invalidate();
        }

        #endregion

        #region SELECTED BACKGROUND SPRITE

        UI.SpriteSet selectedBgSprites;

        public string selectedNormalBgSprite
        {
            get => string.IsNullOrEmpty(selectedBgSprites.normal) ? focusedBgSprite : selectedBgSprites.normal;
            set
            {
                if (value != selectedBgSprites.normal)
                {
                    selectedBgSprites.normal = value;
                    Invalidate();
                }
            }
        }
        public string selectedHoveredBgSprite
        {
            get => string.IsNullOrEmpty(selectedBgSprites.hovered) ? hoveredBgSprite : selectedBgSprites.hovered;
            set
            {
                if (value != selectedBgSprites.hovered)
                {
                    selectedBgSprites.hovered = value;
                    Invalidate();
                }
            }
        }
        public string selectedPressedBgSprite
        {
            get => string.IsNullOrEmpty(selectedBgSprites.pressed) ? pressedBgSprite : selectedBgSprites.pressed;
            set
            {
                if (value != selectedBgSprites.pressed)
                {
                    selectedBgSprites.pressed = value;
                    Invalidate();
                }
            }
        }
        public string selectedFocusedBgSprite
        {
            get => string.IsNullOrEmpty(selectedBgSprites.focused) ? focusedBgSprite : selectedBgSprites.focused;
            set
            {
                if (value != selectedBgSprites.focused)
                {
                    selectedBgSprites.focused = value;
                    Invalidate();
                }
            }
        }
        public string selectedDisabledBgSprite
        {
            get => string.IsNullOrEmpty(selectedBgSprites.disabled) ? disabledBgSprite : selectedBgSprites.disabled;
            set
            {
                if (value != selectedBgSprites.disabled)
                {
                    selectedBgSprites.disabled = value;
                    Invalidate();
                }
            }
        }

        public void SetSelectedBgSprite(UI.SpriteSet sprites)
        {
            selectedBgSprites = sprites;
            Invalidate();
        }

        #endregion

        #region SELECTED FOREGROUND SPRITE

        UI.SpriteSet selectedFgSprites;

        public string selectedNormalFgSprite
        {
            get => string.IsNullOrEmpty(selectedFgSprites.normal) ? focusedFgSprite : selectedFgSprites.normal;
            set
            {
                if (value != selectedFgSprites.normal)
                {
                    selectedFgSprites.normal = value;
                    Invalidate();
                }
            }
        }
        public string selectedHoveredFgSprite
        {
            get => string.IsNullOrEmpty(selectedFgSprites.hovered) ? hoveredFgSprite : selectedFgSprites.hovered;
            set
            {
                if (value != selectedFgSprites.hovered)
                {
                    selectedFgSprites.hovered = value;
                    Invalidate();
                }
            }
        }
        public string selectedPressedFgSprite
        {
            get => string.IsNullOrEmpty(selectedFgSprites.pressed) ? pressedFgSprite : selectedFgSprites.pressed;
            set
            {
                if (value != selectedFgSprites.pressed)
                {
                    selectedFgSprites.pressed = value;
                    Invalidate();
                }
            }
        }
        public string selectedFocusedFgSprite
        {
            get => string.IsNullOrEmpty(selectedFgSprites.focused) ? focusedFgSprite : selectedFgSprites.focused;
            set
            {
                if (value != selectedFgSprites.focused)
                {
                    selectedFgSprites.focused = value;
                    Invalidate();
                }
            }
        }
        public string selectedDisabledFgSprite
        {
            get => string.IsNullOrEmpty(selectedFgSprites.disabled) ? disabledFgSprite : selectedFgSprites.disabled;
            set
            {
                if (value != selectedFgSprites.disabled)
                {
                    selectedFgSprites.disabled = value;
                    Invalidate();
                }
            }
        }

        public void SetSelectedFgSprite(UI.SpriteSet sprites)
        {
            selectedFgSprites = sprites;
            Invalidate();
        }

        #endregion

        #region SELECTED BACKGROUND COLOR

        ColorSet selectedBgColors;

        public Color32 selectedNormalBgColor
        {
            get => selectedBgColors.normal ?? focusedBgColor;
            set
            {
                if (!selectedBgColors.normal.Equals(value))
                {
                    selectedBgColors.normal = value;
                    Invalidate();
                }
            }
        }
        public Color32 selectedFocusedBgColor
        {
            get => selectedBgColors.focused ?? focusedBgColor;
            set
            {
                if (!selectedBgColors.focused.Equals(value))
                {
                    selectedBgColors.focused = value;
                    Invalidate();
                }
            }
        }
        public Color32 selectedHoveredBgColor
        {
            get => selectedBgColors.hovered ?? hoveredBgColor;
            set
            {
                if (!selectedBgColors.hovered.Equals(value))
                {
                    selectedBgColors.hovered = value;
                    Invalidate();
                }
            }
        }
        public Color32 selectedPressedBgColor
        {
            get => selectedBgColors.pressed ?? pressedBgColor;
            set
            {
                if (!selectedBgColors.pressed.Equals(value))
                {
                    selectedBgColors.pressed = value;
                    Invalidate();
                }
            }
        }
        public Color32 selectedDisabledBgColor
        {
            get => selectedBgColors.disabled ?? disabledBgColor;
            set
            {
                if (!selectedBgColors.disabled.Equals(value))
                {
                    selectedBgColors.disabled = value;
                    Invalidate();
                }
            }
        }

        public void SetSelectedBgColor(ColorSet colors)
        {
            selectedBgColors = colors;
            Invalidate();
        }

        #endregion

        #region SELECTED FOREGROUND COLOR

        ColorSet selectedFgColors;

        public Color32 selectedNormalFgColor
        {
            get => selectedFgColors.normal ?? base.focusedTextColor;
            set
            {
                if (!selectedFgColors.normal.Equals(value))
                {
                    selectedFgColors.normal = value;
                    Invalidate();
                }
            }
        }
        public Color32 selectedFocusedFgColor
        {
            get => selectedFgColors.focused ?? base.focusedTextColor;
            set
            {
                if (!selectedFgColors.focused.Equals(value))
                {
                    selectedFgColors.focused = value;
                    Invalidate();
                }
            }
        }
        public Color32 selectedHoveredFgColor
        {
            get => selectedFgColors.hovered ?? base.hoveredTextColor;
            set
            {
                if (!selectedFgColors.hovered.Equals(value))
                {
                    selectedFgColors.hovered = value;
                    Invalidate();
                }
            }
        }
        public Color32 selectedPressedFgColor
        {
            get => selectedFgColors.pressed ?? base.pressedTextColor;
            set
            {
                if (!selectedFgColors.pressed.Equals(value))
                {
                    selectedFgColors.pressed = value;
                    Invalidate();
                }
            }
        }
        public Color32 selectedDisabledFgColor
        {
            get => selectedFgColors.disabled ?? base.disabledTextColor;
            set
            {
                if (!selectedFgColors.disabled.Equals(value))
                {
                    selectedFgColors.disabled = value;
                    Invalidate();
                }
            }
        }

        public void SetSelectedFgColor(ColorSet colors)
        {
            selectedFgColors = colors;
            Invalidate();
        }

        #endregion

        #region SELECTED TEXT COLOR

        ColorSet selectedTextColors;

        public Color32 selectedNormalTextColor
        {
            get => selectedTextColors.normal ?? base.focusedTextColor;
            set
            {
                if (!selectedTextColors.normal.Equals(value))
                {
                    selectedTextColors.normal = value;
                    Invalidate();
                }
            }
        }
        public Color32 selectedFocusedTextColor
        {
            get => selectedTextColors.focused ?? base.focusedTextColor;
            set
            {
                if (!selectedTextColors.focused.Equals(value))
                {
                    selectedTextColors.focused = value;
                    Invalidate();
                }
            }
        }
        public Color32 selectedHoveredTextColor
        {
            get => selectedTextColors.hovered ?? base.hoveredTextColor;
            set
            {
                if (!selectedTextColors.hovered.Equals(value))
                {
                    selectedTextColors.hovered = value;
                    Invalidate();
                }
            }
        }
        public Color32 selectedPressedTextColor
        {
            get => selectedTextColors.pressed ?? base.pressedTextColor;
            set
            {
                if (!selectedTextColors.pressed.Equals(value))
                {
                    selectedTextColors.pressed = value;
                    Invalidate();
                }
            }
        }
        public Color32 selectedDisabledTextColor
        {
            get => selectedTextColors.disabled ?? base.disabledTextColor;
            set
            {
                if (!selectedTextColors.disabled.Equals(value))
                {
                    selectedTextColors.disabled = value;
                    Invalidate();
                }
            }
        }

        public void SetSelectedTextColor(ColorSet colors)
        {
            selectedTextColors = colors;
            Invalidate();
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
                        Vector2 vector = uIFontRenderer.MeasureString(m_Text);
                        size = new Vector2(vector.x + textPadding.horizontal, vector.y + textPadding.vertical);
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
                    ButtonState.Normal => isSelected ? selectedNormalTextColor : textColor,
                    ButtonState.Focused => isSelected ? selectedFocusedTextColor : focusedTextColor,
                    ButtonState.Hovered => isSelected ? selectedHoveredTextColor : hoveredTextColor,
                    ButtonState.Pressed => isSelected ? selectedPressedTextColor : pressedTextColor,
                    ButtonState.Disabled => isSelected ? selectedDisabledTextColor : disabledTextColor,
                    _ => Color.white,
                };
            }
        }
        private Color32 GetBackgroundColor()
        {
            return state switch
            {
                ButtonState.Focused => isSelected ? selectedFocusedBgColor : focusedBgColor,
                ButtonState.Hovered => isSelected ? selectedHoveredBgColor : hoveredBgColor,
                ButtonState.Pressed => isSelected ? selectedPressedBgColor : pressedBgColor,
                ButtonState.Disabled => isSelected ? selectedDisabledBgColor : disabledBgColor,
                _ => isSelected ? selectedNormalBgColor : normalBgColor,
            };
        }
        private Color32 GetForegroundColor()
        {
            return state switch
            {
                ButtonState.Focused => isSelected ? selectedFocusedFgColor : focusedFgColor,
                ButtonState.Hovered => isSelected ? selectedHoveredFgColor : hoveredFgColor,
                ButtonState.Pressed => isSelected ? selectedPressedFgColor : pressedFgColor,
                ButtonState.Disabled => isSelected ? selectedDisabledFgColor : disabledFgColor,
                _ => isSelected ? selectedNormalFgColor : normalFgColor,
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
                ButtonState.Normal => isSelected ? atlas[selectedNormalBgSprite] : atlas[normalBgSprite],
                ButtonState.Focused => isSelected ? atlas[selectedFocusedBgSprite] : atlas[focusedBgSprite],
                ButtonState.Hovered => isSelected ? atlas[selectedHoveredBgSprite] : atlas[hoveredBgSprite],
                ButtonState.Pressed => isSelected ? atlas[selectedPressedBgSprite] : atlas[pressedBgSprite],
                ButtonState.Disabled => isSelected ? atlas[selectedDisabledBgSprite] : atlas[disabledBgSprite],
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
                ButtonState.Normal => isSelected ? atlas[normalFgSprite] : atlas[normalFgSprite],
                ButtonState.Focused => isSelected ? atlas[selectedFocusedFgSprite] : atlas[focusedFgSprite],
                ButtonState.Hovered => isSelected ? atlas[selectedHoveredFgSprite] : atlas[hoveredFgSprite],
                ButtonState.Pressed => isSelected ? atlas[selectedPressedFgSprite] : atlas[pressedFgSprite],
                ButtonState.Disabled => isSelected ? atlas[selectedDisabledFgSprite] : atlas[disabledFgSprite],
                _ => null,
            };

            return spriteInfo ?? atlas[normalFgSprite];
        }

        protected override void RenderBackground()
        {
            if (GetBackgroundSprite() is UITextureAtlas.SpriteInfo backgroundSprite)
            {
                var renderOptions = new RenderOptions()
                {
                    atlas = atlasBackground,
                    color = GetBackgroundColor(),
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
        protected override void RenderForeground()
        {
            if (GetForegroundSprite() is UITextureAtlas.SpriteInfo foregroundSprite)
            {
                var foregroundRenderSize = GetForegroundRenderSize(foregroundSprite);
                var foregroundRenderOffset = GetForegroundRenderOffset(foregroundRenderSize);

                var renderOptions = new RenderOptions()
                {
                    atlas = atlasForeground,
                    color = GetForegroundColor(),
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
        protected override Vector2 GetForegroundRenderSize(UITextureAtlas.SpriteInfo spriteInfo)
        {
            if (spriteInfo == null)
                return Vector2.zero;

            if (m_ForegroundSpriteMode == UIForegroundSpriteMode.Stretch)
            {
                return new Vector2(width - spritePadding.horizontal, height - spritePadding.vertical) * m_ScaleFactor;
            }
            else if (m_ForegroundSpriteMode == UIForegroundSpriteMode.Fill)
            {
                return spriteInfo.pixelSize;
            }
            else if (m_ForegroundSpriteMode == UIForegroundSpriteMode.Scale)
            {
                var widthRatio = Mathf.Max(width - spritePadding.horizontal, 0f) / spriteInfo.width;
                var heightRatio = Mathf.Max(height - spritePadding.vertical, 0f) / spriteInfo.height;
                var ratio = Mathf.Min(widthRatio, heightRatio);
                return new Vector2(ratio * spriteInfo.width, ratio * spriteInfo.height) * m_ScaleFactor;
            }
            else
            {
                return Vector2.zero;
            }
        }
        protected override Vector2 GetForegroundRenderOffset(Vector2 renderSize)
        {
            Vector2 result = pivot.TransformToUpperLeft(size, arbitraryPivotOffset);
            if (horizontalAlignment == UIHorizontalAlignment.Left)
            {
                result.x += spritePadding.left;
            }
            else if (horizontalAlignment == UIHorizontalAlignment.Center)
            {
                result.x += (width - renderSize.x) * 0.5f;
                result.x += spritePadding.left - spritePadding.right;
            }
            else if (horizontalAlignment == UIHorizontalAlignment.Right)
            {
                result.x += width - renderSize.x;
                result.x -= spritePadding.right;
            }

            if (verticalAlignment == UIVerticalAlignment.Bottom)
            {
                result.y -= height - renderSize.y;
                result.y += spritePadding.bottom;
            }
            else if (verticalAlignment == UIVerticalAlignment.Middle)
            {
                result.y -= (height - renderSize.y) * 0.5f;
                result.y -= spritePadding.top - spritePadding.bottom;
            }
            else if (verticalAlignment == UIVerticalAlignment.Top)
            {
                result.y -= spritePadding.top;
            }

            return result;
        }

        public void AutoWidth()
        {
            if (m_Font != null && m_Font.isValid && !string.IsNullOrEmpty(m_Text))
            {
                var minSize = minimumSize;

                using (UIFontRenderer uIFontRenderer = ObtainTextRenderer())
                {
                    var textSize = uIFontRenderer.MeasureString(m_Text);
                    minSize.x = textSize.x + textPadding.horizontal;
                }

                var sprite = GetForegroundSprite();
                var spriteSize = GetForegroundRenderSize(sprite);
                width = Mathf.Max(spriteSize.x, minSize.x);
            }
        }
    }
}
