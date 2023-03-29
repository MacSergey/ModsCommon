using ColossalFramework.UI;
using System;
using UnityEngine;

namespace ModsCommon.UI
{
    [Flags]
    public enum AutoSize
    {
        None,
        Width = 1,
        Height = 2,
        All = Width | Height,
    }
    public class CustomUILabel : UITextComponent
    {
        private Vector3 positionBefore;
        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout()
        {
            if ((relativePosition - positionBefore).sqrMagnitude > 0.001)
                relativePosition = positionBefore;
        }

        public event Action<string> OnTextChanged;


        protected UITextureAtlas atlas;
        public UITextureAtlas Atlas
        {
            get => atlas ??= GetUIView()?.defaultAtlas;
            set
            {
                if (!Equals(value, atlas))
                {
                    atlas = value;
                    Invalidate();
                }
            }
        }

        protected UITextureAtlas atlasBackground;
        public UITextureAtlas AtlasBackground
        {
            get => atlasBackground ?? Atlas;
            set
            {
                if (!Equals(value, atlasBackground))
                {
                    atlasBackground = value;
                    Invalidate();
                }
            }
        }

        protected UITextureAtlas atlasIcons;
        public UITextureAtlas AtlasIcons
        {
            get => atlasIcons ?? Atlas;
            set
            {
                if (!Equals(value, atlasIcons))
                {
                    atlasIcons = value;
                    Invalidate();
                }
            }
        }

        string backgroundSprite;
        public string BackgroundSprite
        {
            get => backgroundSprite;
            set
            {
                if (value != backgroundSprite)
                {
                    backgroundSprite = value;
                    Invalidate();
                }
            }
        }

        Color32? bgColor;
        public Color32 BgColor
        {
            get => bgColor ?? base.color;
            set
            {
                if (!bgColor.Equals(value))
                {
                    bgColor = value;
                    Invalidate();
                }
            }
        }

        public LabelStyle LabelStyle
        {
            set
            {
                textColor = value.NormalTextColor;
                disabledTextColor = value.DisabledTextColor;
                Invalidate();
            }
        }

        public override string text
        {
            get => m_Text;
            set
            {
                if (!string.Equals(value, m_Text))
                {
                    m_Text = value;
                    OnTextChanged?.Invoke(m_Text);
                    Invalidate();
                }
            }
        }

        private bool bold;
        public bool Bold
        {
            get => bold;
            set
            {
                bold = value;
                font = value ? ComponentStyle.SemiBoldFont : ComponentStyle.RegularFont;
                Invalidate();
            }
        }

        private AutoSize _autoSize = AutoSize.All;

        [Obsolete]
        public override bool autoSize
        {
            get => AutoSize == AutoSize.All;
            set => AutoSize = value ? AutoSize.All : AutoSize.None;
        }
        public AutoSize AutoSize
        {
            get => _autoSize;
            set
            {
                if (value != _autoSize)
                {
                    _autoSize = value;
                    Invalidate();
                }
            }
        }

        protected bool wordWrap;
        public bool WordWrap
        {
            get => wordWrap;
            set
            {
                if (value != wordWrap)
                {
                    wordWrap = value;
                    Invalidate();
                }
            }
        }

        protected UIHorizontalAlignment horizontalAlignment;
        public UIHorizontalAlignment HorizontalAlignment
        {
            get => horizontalAlignment;
            set
            {
                if (value != horizontalAlignment)
                {
                    horizontalAlignment = value;
                    Invalidate();
                }
            }
        }


        protected UIVerticalAlignment verticalAlignment;
        public UIVerticalAlignment VerticalAlignment
        {
            get => verticalAlignment;
            set
            {
                if (value != verticalAlignment)
                {
                    verticalAlignment = value;
                    Invalidate();
                }
            }
        }

        protected RectOffset padding;
        public RectOffset Padding
        {
            get => padding ??= new RectOffset();
            set
            {
                value = value.ConstrainPadding();
                if (!Equals(value, padding))
                {
                    padding = value;
                    Invalidate();
                }
            }
        }


        protected int tabSize = 48;
        public int TabSize
        {
            get => tabSize;
            set
            {
                value = Mathf.Max(0, value);
                if (value != tabSize)
                {
                    tabSize = value;
                    Invalidate();
                }
            }
        }

        public override Vector2 CalculateMinimumSize()
        {
            if (font != null)
            {
                float num = font.size * textScale * 0.75f;
                return Vector2.Max(base.CalculateMinimumSize(), new Vector2(num, num));
            }

            return base.CalculateMinimumSize();
        }

        public override void Invalidate()
        {
            base.Invalidate();

            var size = this.size;

            if (font != null && font.isValid && !string.IsNullOrEmpty(text) && AutoSize != AutoSize.None)
            {
                using UIFontRenderer uIFontRenderer = ObtainTextRenderer();
                Vector2 measured = uIFontRenderer.MeasureString(text).RoundToInt();

                var width = measured.x + Padding.horizontal;
                var height = measured.y + Padding.vertical;

                switch (AutoSize)
                {
                    case AutoSize.Width:
                        size.x = width;
                        break;
                    case AutoSize.Height:
                        size.y = height;
                        break;
                    case AutoSize.All:
                        size = new Vector2(width, height);
                        break;
                }
            }

            this.size = size;
        }

        protected UIRenderData BgRenderData { get; set; }
        protected UIRenderData IconsRenderData { get; set; }
        protected UIRenderData TextRenderData { get; set; }

        public override void OnDisable()
        {
            BgRenderData = null;
            IconsRenderData = null;
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

            if (TextRenderData == null)
            {
                TextRenderData = UIRenderData.Obtain();
                m_RenderData.Add(TextRenderData);
            }
            else
                TextRenderData.Clear();

            if (IconsRenderData == null)
            {
                IconsRenderData = UIRenderData.Obtain();
                m_RenderData.Add(IconsRenderData);
            }
            else
                IconsRenderData.Clear();

            if (AtlasBackground is UITextureAtlas bgAtlas && AtlasIcons is UITextureAtlas iconsAtlas)
            {
                BgRenderData.material = bgAtlas.material;
                IconsRenderData.material = iconsAtlas.material;

                RenderBackground();
                RenderText();
            }
        }

        protected void RenderBackground()
        {
            if (AtlasBackground is not UITextureAtlas atlas)
                return;

            if (atlas[BackgroundSprite] is UITextureAtlas.SpriteInfo backgroundSprite)
            {
                var renderOptions = new RenderOptions()
                {
                    atlas = atlas,
                    color = BgColor,
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

        private void RenderText()
        {
            if (m_Font == null || !m_Font.isValid || string.IsNullOrEmpty(text))
                return;

            using UIFontRenderer uIFontRenderer = ObtainTextRenderer();
            if (uIFontRenderer is UIDynamicFont.DynamicFontRenderer dynamicFontRenderer)
            {
                dynamicFontRenderer.spriteAtlas = AtlasIcons;
                dynamicFontRenderer.spriteBuffer = TextRenderData;
            }
            uIFontRenderer.Render(text, TextRenderData);
        }

        private UIFontRenderer ObtainTextRenderer()
        {
            var maxSize = AutoSize switch
            {
                AutoSize.Width or AutoSize.All => new Vector2(width - Padding.horizontal, height),
                AutoSize.Height => new Vector2(width - Padding.horizontal, 4096f),
                _ => new Vector2(width - Padding.horizontal, height - Padding.vertical),
            };

            var ratio = PixelsToUnits();
            var offset = pivot.TransformToUpperLeft(size, arbitraryPivotOffset);
            offset.y -= Padding.top * ratio;
            if (HorizontalAlignment != UIHorizontalAlignment.Right)
                offset.x += Padding.left * ratio;

            var renderer = font.ObtainRenderer();
            renderer.wordWrap = wordWrap;
            renderer.multiLine = true;
            renderer.maxSize = maxSize;
            renderer.pixelRatio = ratio;
            renderer.textScale = textScale;
            renderer.vectorOffset = offset;
            renderer.textAlign = HorizontalAlignment;
            renderer.processMarkup = processMarkup;
            renderer.defaultColor = isEnabled ? textColor : disabledTextColor;
            renderer.bottomColor = null;
            renderer.overrideMarkupColors = false;
            renderer.opacity = CalculateOpacity();
            renderer.shadow = useDropShadow;
            renderer.shadowColor = dropShadowColor;
            renderer.shadowOffset = dropShadowOffset;
            renderer.outline = useOutline;
            renderer.outlineSize = outlineSize;
            renderer.outlineColor = outlineColor;
            if (VerticalAlignment != UIVerticalAlignment.Top)
                renderer.vectorOffset = GetVertAlignOffset(renderer);

            return renderer;
        }

        private Vector3 GetVertAlignOffset(UIFontRenderer fontRenderer)
        {
            var ratio = PixelsToUnits();
            var measure = fontRenderer.MeasureString(text) * ratio;
            var vectorOffset = fontRenderer.vectorOffset;
            var height = (this.height - Padding.vertical) * ratio;
            if (measure.y >= height)
                return vectorOffset;

            switch (VerticalAlignment)
            {
                case UIVerticalAlignment.Middle:
                    vectorOffset.y -= (height - measure.y) * 0.5f;
                    break;
                case UIVerticalAlignment.Bottom:
                    vectorOffset.y -= height - measure.y;
                    break;
            }

            return vectorOffset;
        }
    }
}
