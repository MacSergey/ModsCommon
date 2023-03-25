using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModsCommon.UI
{
    public class CustomUICheckBox : UITextComponent
    {
        public event Action<string> OnLabelChanged;
        public event Action<bool> OnStateChanged;

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

        protected UITextureAtlas atlasMark;
        public UITextureAtlas AtlasMark
        {
            get => atlasMark ?? Atlas;
            set
            {
                if (!Equals(value, atlasMark))
                {
                    atlasMark = value;
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

        string uncheckedSprite;
        public string UncheckedSprite
        {
            get => uncheckedSprite;
            set
            {
                if (value != uncheckedSprite)
                {
                    uncheckedSprite = value;
                    Invalidate();
                }
            }
        }

        string checkedSprite;
        public string CheckedSprite
        {
            get => checkedSprite;
            set
            {
                if (value != checkedSprite)
                {
                    checkedSprite = value;
                    Invalidate();
                }
            }
        }

        private bool isChecked;
        public bool IsChecked
        {
            get => isChecked;
            set
            {
                if (value != isChecked)
                {
                    isChecked = value;
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

        Color32? uncheckedNormalColor;
        public Color32 UncheckedNormalColor
        {
            get => uncheckedNormalColor ?? base.color;
            set
            {
                if (!uncheckedNormalColor.Equals(value))
                {
                    uncheckedNormalColor = value;
                    OnColorChanged();
                }
            }
        }


        Color32? uncheckedHoveredColor;
        public Color32 UncheckedHoveredColor
        {
            get => uncheckedHoveredColor ?? UncheckedNormalColor;
            set
            {
                if (!uncheckedHoveredColor.Equals(value))
                {
                    uncheckedHoveredColor = value;
                    OnColorChanged();
                }
            }
        }


        Color32? uncheckedDisabledColor;
        public Color32 UncheckedDisabledColor
        {
            get => uncheckedDisabledColor ?? UncheckedNormalColor;
            set
            {
                if (!uncheckedDisabledColor.Equals(value))
                {
                    uncheckedDisabledColor = value;
                    OnColorChanged();
                }
            }
        }


        Color32? checkedNormalColor;
        public Color32 CheckedNormalColor
        {
            get => checkedNormalColor ?? UncheckedNormalColor;
            set
            {
                if (!checkedNormalColor.Equals(value))
                {
                    checkedNormalColor = value;
                    OnColorChanged();
                }
            }
        }


        Color32? checkedHoveredColor;
        public Color32 CheckedHoveredColor
        {
            get => checkedHoveredColor ?? CheckedNormalColor;
            set
            {
                if (!checkedHoveredColor.Equals(value))
                {
                    checkedHoveredColor = value;
                    OnColorChanged();
                }
            }
        }


        Color32? checkedDisabledColor;
        public Color32 CheckedDisabledColor
        {
            get => checkedDisabledColor ?? CheckedNormalColor;
            set
            {
                if (!checkedDisabledColor.Equals(value))
                {
                    checkedDisabledColor = value;
                    OnColorChanged();
                }
            }
        }


        protected string label;
        public string Label
        {
            get => label;
            set
            {
                if (!string.Equals(value, label))
                {
                    label = value;
                    OnLabelChanged?.Invoke(label);
                }
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


        protected Vector2 markSize;
        public Vector2 MarkSize
        {
            get => markSize;
            set
            {
                if (value != markSize)
                {
                    markSize = value;
                    Invalidate();
                }
            }
        }

        protected new bool autoSize = true;
        public bool AutoSize
        {
            get => autoSize;
            set
            {
                if (value != autoSize)
                {
                    if (value)
                        autoHeight = false;

                    autoSize = value;
                    Invalidate();
                }
            }
        }


        protected bool autoHeight;
        public bool AutoHeight
        {
            get => autoHeight;
            set
            {
                if (value != autoHeight)
                {
                    if (value)
                        autoSize = false;

                    autoHeight = value;
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


        protected RectOffset textPadding;
        public RectOffset TextPadding
        {
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

        protected override void OnClick(UIMouseEventParameter p)
        {
            base.OnClick(p);

            IsChecked = !IsChecked;
            OnStateChanged?.Invoke(IsChecked);
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

            if (AutoSize)
            {
                if (font != null && font.isValid && !string.IsNullOrEmpty(Label))
                {
                    using UIFontRenderer uIFontRenderer = ObtainTextRenderer();
                    Vector2 vector = uIFontRenderer.MeasureString(Label).RoundToInt();
                    var width = markSize.x + vector.x + Padding.horizontal + TextPadding.horizontal;
                    var height = Math.Max(markSize.y, vector.y + TextPadding.vertical) + Padding.vertical;
                    size = new Vector2(width, height);
                }
                else
                    size = MarkSize + new Vector2(Padding.horizontal, Padding.vertical);
            }
            else if (AutoHeight)
            {
                if (font != null && font.isValid && !string.IsNullOrEmpty(Label))
                {
                    using UIFontRenderer uIFontRenderer = ObtainTextRenderer();
                    var vector = uIFontRenderer.MeasureString(Label).RoundToInt();
                    var height = Math.Max(markSize.y, vector.y + TextPadding.vertical) + Padding.vertical;
                    size = new Vector2(size.x, height);
                }
                else
                {
                    size = new Vector2(size.x, MarkSize.y + Padding.vertical);
                }
            }
        }


        protected UIRenderData BgRenderData { get; set; }
        protected UIRenderData MarkRenderData { get; set; }
        protected UIRenderData TextRenderData { get; set; }

        public override void OnDisable()
        {
            BgRenderData = null;
            MarkRenderData = null;
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

            if (MarkRenderData == null)
            {
                MarkRenderData = UIRenderData.Obtain();
                m_RenderData.Add(MarkRenderData);
            }
            else
                MarkRenderData.Clear();

            if (TextRenderData == null)
            {
                TextRenderData = UIRenderData.Obtain();
                m_RenderData.Add(TextRenderData);
            }
            else
                TextRenderData.Clear();

            if (AtlasBackground is UITextureAtlas bgAtlas && AtlasMark is UITextureAtlas markAtlas)
            {
                BgRenderData.material = bgAtlas.material;
                MarkRenderData.material = markAtlas.material;
                TextRenderData.material = bgAtlas.material;

                RenderBackground();
                RenderMark();
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

        private Color32 RenderColor
        {
            get
            {
                if (!isEnabled)
                    return isChecked ? CheckedDisabledColor : UncheckedDisabledColor;
                else if (m_IsMouseHovering)
                    return isChecked ? CheckedHoveredColor : UncheckedHoveredColor;
                else
                    return isChecked ? CheckedNormalColor : UncheckedNormalColor;
            }
        }

        protected void RenderMark()
        {
            if (AtlasMark is not UITextureAtlas atlas)
                return;

            if (atlas[isChecked ? CheckedSprite : UncheckedSprite] is UITextureAtlas.SpriteInfo foregroundSprite)
            {
                var markRenderSize = MarkSize;
                var markRenderOffset = pivot.TransformToUpperLeft(markRenderSize, arbitraryPivotOffset);

                if (horizontalAlignment != UIHorizontalAlignment.Right)
                {
                    markRenderOffset.x += Padding.left;
                }
                else
                {
                    markRenderOffset.x += width - markRenderSize.x;
                    markRenderOffset.x -= Padding.right;
                }

                if (verticalAlignment == UIVerticalAlignment.Bottom)
                {
                    markRenderOffset.y -= height - markRenderSize.y;
                    markRenderOffset.y += Padding.bottom;
                }
                else if (verticalAlignment == UIVerticalAlignment.Middle)
                {
                    markRenderOffset.y -= (height - markRenderSize.y) * 0.5f;
                    markRenderOffset.y -= Padding.top - Padding.bottom;
                }
                else if (verticalAlignment == UIVerticalAlignment.Top)
                {
                    markRenderOffset.y -= Padding.top;
                }

                var renderOptions = new RenderOptions()
                {
                    atlas = atlas,
                    color = RenderColor,
                    fillAmount = 1f,
                    flip = UISpriteFlip.None,
                    offset = markRenderOffset,
                    pixelsToUnits = PixelsToUnits(),
                    size = markRenderSize,
                    spriteInfo = foregroundSprite,
                };

                if (foregroundSprite.isSliced)
                    Render.RenderSlicedSprite(MarkRenderData, renderOptions);
                else
                    Render.RenderSprite(MarkRenderData, renderOptions);
            }
        }
        private void RenderText()
        {
            if (m_Font == null || !m_Font.isValid || string.IsNullOrEmpty(Label))
                return;

            using UIFontRenderer uIFontRenderer = ObtainTextRenderer();
            if (uIFontRenderer is UIDynamicFont.DynamicFontRenderer dynamicFontRenderer)
            {
                dynamicFontRenderer.spriteAtlas = AtlasBackground;
                dynamicFontRenderer.spriteBuffer = BgRenderData;
            }
            uIFontRenderer.Render(Label, TextRenderData);
        }
        private UIFontRenderer ObtainTextRenderer()
        {
            Vector2 maxSize;
            if (AutoSize)
                maxSize = Vector2.one * int.MaxValue;
            else if (AutoHeight)
                maxSize = new Vector2(width - MarkSize.x - Padding.horizontal - TextPadding.horizontal, 4096f);
            else
                maxSize = new Vector2(width - MarkSize.x - Padding.horizontal - TextPadding.horizontal, height - Padding.vertical - TextPadding.vertical);

            var ratio = PixelsToUnits();
            var offset = pivot.TransformToUpperLeft(size, arbitraryPivotOffset);
            offset.y -= (Padding.top + TextPadding.top) * ratio;
            if (horizontalAlignment != UIHorizontalAlignment.Right)
                offset.x += (MarkSize.x + Padding.left + TextPadding.left) * ratio;

            var renderer = font.ObtainRenderer();
            renderer.wordWrap = wordWrap;
            renderer.multiLine = true;
            renderer.maxSize = maxSize;
            renderer.pixelRatio = ratio;
            renderer.textScale = textScale;
            renderer.vectorOffset = offset;
            renderer.textAlign = horizontalAlignment;
            renderer.processMarkup = processMarkup;
            renderer.defaultColor = textColor;
            renderer.bottomColor = null;
            renderer.overrideMarkupColors = false;
            renderer.opacity = CalculateOpacity();
            renderer.shadow = useDropShadow;
            renderer.shadowColor = dropShadowColor;
            renderer.shadowOffset = dropShadowOffset;
            renderer.outline = useOutline;
            renderer.outlineSize = outlineSize;
            renderer.outlineColor = outlineColor;
            if (!autoSize && verticalAlignment != UIVerticalAlignment.Top)
                renderer.vectorOffset = GetVertAlignOffset(renderer);

            return renderer;
        }
        private Vector3 GetVertAlignOffset(UIFontRenderer fontRenderer)
        {
            var ratio = PixelsToUnits();
            var measure = fontRenderer.MeasureString(Label) * ratio;
            var vectorOffset = fontRenderer.vectorOffset;
            var height = (this.height - Padding.vertical - TextPadding.vertical) * ratio;
            if (measure.y >= height)
                return vectorOffset;

            switch (verticalAlignment)
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
