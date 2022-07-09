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
        private static readonly int[] kTriangleIndices = new int[6] { 0, 1, 3, 3, 1, 2 };
        private static readonly int[] kSlicedTriangleIndices = new int[54]
        {
            0, 1, 2, 2, 3, 0, 4, 5, 6, 6,
            7, 4, 8, 9, 10, 10, 11, 8, 12, 13,
            14, 14, 15, 12, 1, 4, 7, 7, 2, 1,
            9, 12, 15, 15, 10, 9, 3, 2, 9, 9,
            8, 3, 7, 6, 13, 13, 12, 7, 2, 7,
            12, 12, 9, 2
        };
        private static readonly int[][] kHorzFill = new int[4][]
        {
            new int[4] { 0, 1, 4, 5 },
            new int[4] { 3, 2, 7, 6 },
            new int[4] { 8, 9, 12, 13 },
            new int[4] { 11, 10, 15, 14 }
        };

        private static readonly int[][] kVertFill = new int[4][]
        {
            new int[4] { 11, 8, 3, 0 },
            new int[4] { 10, 9, 2, 1 },
            new int[4] { 15, 12, 7, 4 },
            new int[4] { 14, 13, 6, 5 }
        };

        private static Vector3[] _Vertices = new Vector3[16];
        private static Vector2[] _UV = new Vector2[16];
        private static readonly int[][] kFillIndices = new int[][] { new int[4], new int[4], new int[4], new int[4] };

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
                    RenderSlicedSprite(BgRenderData, renderOptions);
                else
                    RenderSprite(BgRenderData, renderOptions);
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
                    RenderSlicedSprite(FgRenderData, renderOptions);
                else
                    RenderSprite(FgRenderData, renderOptions);
            }
        }

        public static void RenderSprite(UIRenderData data, RenderOptions options)
        {
            options._baseIndex = data.vertices.Count;
            RebuildTriangles(data, options);
            RebuildVertices(data, options);
            RebuildUV(data, options);
            RebuildColors(data, options);
            if (options._fillAmount < 1f)
                DoFill(data, options);
        }
        private static void RebuildTriangles(UIRenderData renderData, RenderOptions options)
        {
            var baseIndex = options._baseIndex;
            var triangles = renderData.triangles;
            triangles.EnsureCapacity(triangles.Count + kTriangleIndices.Length);
            for (int i = 0; i < kTriangleIndices.Length; i++)
            {
                triangles.Add(baseIndex + kTriangleIndices[i]);
            }
        }
        private static void RebuildVertices(UIRenderData renderData, RenderOptions options)
        {
            var vertices = renderData.vertices;
            var baseIndex = options._baseIndex;
            var x = 0f;
            var y = 0f;
            var sizeX = Mathf.Ceil(options._size.x);
            var sizeY = Mathf.Ceil(0f - options._size.y);
            vertices.Add(new Vector3(x, y, 0f) * options._pixelsToUnits);
            vertices.Add(new Vector3(sizeX, y, 0f) * options._pixelsToUnits);
            vertices.Add(new Vector3(sizeX, sizeY, 0f) * options._pixelsToUnits);
            vertices.Add(new Vector3(x, sizeY, 0f) * options._pixelsToUnits);
            var vector = options._offset.RoundToInt() * options._pixelsToUnits;
            for (int i = 0; i < 4; i++)
            {
                vertices[baseIndex + i] = (vertices[baseIndex + i] + vector).Quantize(options._pixelsToUnits);
            }
        }
        private static void RebuildUV(UIRenderData renderData, RenderOptions options)
        {
            var region = options._spriteInfo.region;
            var uvs = renderData.uvs;
            uvs.Add(new Vector2(region.x, region.yMax));
            uvs.Add(new Vector2(region.xMax, region.yMax));
            uvs.Add(new Vector2(region.xMax, region.y));
            uvs.Add(new Vector2(region.x, region.y));
            var zero = Vector2.zero;
            if (options._flip.IsFlagSet(UISpriteFlip.FlipHorizontal))
            {
                zero = uvs[1];
                uvs[1] = uvs[0];
                uvs[0] = zero;
                zero = uvs[3];
                uvs[3] = uvs[2];
                uvs[2] = zero;
            }
            if (options._flip.IsFlagSet(UISpriteFlip.FlipVertical))
            {
                zero = uvs[0];
                uvs[0] = uvs[3];
                uvs[3] = zero;
                zero = uvs[1];
                uvs[1] = uvs[2];
                uvs[2] = zero;
            }
        }
        private static void RebuildColors(UIRenderData renderData, RenderOptions options)
        {
            var color = ((Color)options._color).linear;
            var colors = renderData.colors;
            for (int i = 0; i < 4; i++)
            {
                colors.Add(color);
            }
        }
        private static void DoFill(UIRenderData renderData, RenderOptions options)
        {
            var baseIndex = options._baseIndex;
            var vertices = renderData.vertices;
            var uvs = renderData.uvs;
            var index1 = baseIndex + 3;
            var index2 = baseIndex + 2;
            var index3 = baseIndex;
            var index4 = baseIndex + 1;
            if (options._invertFill)
            {
                if (options._fillDirection == UIFillDirection.Horizontal)
                {
                    index1 = baseIndex + 1;
                    index2 = baseIndex;
                    index3 = baseIndex + 2;
                    index4 = baseIndex + 3;
                }
                else
                {
                    index1 = baseIndex;
                    index2 = baseIndex + 1;
                    index3 = baseIndex + 3;
                    index4 = baseIndex + 2;
                }
            }
            if (options._fillDirection == UIFillDirection.Horizontal)
            {
                vertices[index2] = Vector3.Lerp(vertices[index2], vertices[index1], 1f - options._fillAmount);
                vertices[index4] = Vector3.Lerp(vertices[index4], vertices[index3], 1f - options._fillAmount);
                uvs[index2] = Vector2.Lerp(uvs[index2], uvs[index1], 1f - options._fillAmount);
                uvs[index4] = Vector2.Lerp(uvs[index4], uvs[index3], 1f - options._fillAmount);
            }
            else
            {
                vertices[index3] = Vector3.Lerp(vertices[index3], vertices[index1], 1f - options._fillAmount);
                vertices[index4] = Vector3.Lerp(vertices[index4], vertices[index2], 1f - options._fillAmount);
                uvs[index3] = Vector2.Lerp(uvs[index3], uvs[index1], 1f - options._fillAmount);
                uvs[index4] = Vector2.Lerp(uvs[index4], uvs[index2], 1f - options._fillAmount);
            }
        }

        public static void RenderSlicedSprite(UIRenderData renderData, RenderOptions options)
        {
            options._baseIndex = renderData.vertices.Count;
            RebuildSlicedTriangles(renderData, options);
            RebuildSlicedVertices(renderData, options);
            RebuildSlicedUV(renderData, options);
            RebuildSlicedColors(renderData, options);
            if (options._fillAmount < 1f)
                DoSlicedFill(renderData, options);
        }
        private static void RebuildSlicedTriangles(UIRenderData renderData, RenderOptions options)
        {
            var baseIndex = options._baseIndex;
            var triangles = renderData.triangles;
            for (int i = 0; i < kSlicedTriangleIndices.Length; i++)
            {
                triangles.Add(baseIndex + kSlicedTriangleIndices[i]);
            }
        }
        private static void RebuildSlicedVertices(UIRenderData renderData, RenderOptions options)
        {
            var x = 0f;
            var y = 0f;
            var sizeX = Mathf.Ceil(options._size.x);
            var sizeY = Mathf.Ceil(0f - options._size.y);
            var spriteInfo = options._spriteInfo;
            var left = spriteInfo.border.left;
            var top = spriteInfo.border.top;
            var right = spriteInfo.border.right;
            var bottom = spriteInfo.border.bottom;
            if (options._flip.IsFlagSet(UISpriteFlip.FlipHorizontal))
            {
                var num7 = right;
                right = left;
                left = num7;
            }
            if (options._flip.IsFlagSet(UISpriteFlip.FlipVertical))
            {
                var temp = bottom;
                bottom = top;
                top = temp;
            }

            var vertices = _Vertices;
            vertices[0] = new Vector3(x, y, 0f) + options._offset;
            vertices[1] = vertices[0] + new Vector3(left, 0f, 0f);
            vertices[2] = vertices[0] + new Vector3(left, 0f - top, 0f);
            vertices[3] = vertices[0] + new Vector3(0f, 0f - top, 0f);
            vertices[4] = new Vector3(sizeX - right, y, 0f) + options._offset;
            vertices[5] = vertices[4] + new Vector3(right, 0f, 0f);
            vertices[6] = vertices[4] + new Vector3(right, 0f - top, 0f);
            vertices[7] = vertices[4] + new Vector3(0f, 0f - top, 0f);
            vertices[8] = new Vector3(x, sizeY + bottom, 0f) + options._offset;
            vertices[9] = vertices[8] + new Vector3(left, 0f, 0f);
            vertices[10] = vertices[8] + new Vector3(left, 0f - bottom, 0f);
            vertices[11] = vertices[8] + new Vector3(0f, 0f - bottom, 0f);
            vertices[12] = new Vector3(sizeX - right, sizeY + bottom, 0f) + options._offset;
            vertices[13] = vertices[12] + new Vector3(right, 0f, 0f);
            vertices[14] = vertices[12] + new Vector3(right, 0f - bottom, 0f);
            vertices[15] = vertices[12] + new Vector3(0f, 0f - bottom, 0f);

            for (int i = 0; i < vertices.Length; i++)
            {
                renderData.vertices.Add((vertices[i] * options._pixelsToUnits).Quantize(options._pixelsToUnits));
            }
        }
        private static void RebuildSlicedUV(UIRenderData renderData, RenderOptions options)
        {
            var atlas = options._atlas;
            var size = new Vector2(atlas.texture.width, atlas.texture.height);
            var spriteInfo = options._spriteInfo;
            var top = (float)spriteInfo.border.top / size.y;
            var bottom = (float)spriteInfo.border.bottom / size.y;
            var left = (float)spriteInfo.border.left / size.x;
            var right = (float)spriteInfo.border.right / size.x;
            var region = spriteInfo.region;

            var uv = _UV;
            uv[0] = new Vector2(region.x, region.yMax);
            uv[1] = new Vector2(region.x + left, region.yMax);
            uv[2] = new Vector2(region.x + left, region.yMax - top);
            uv[3] = new Vector2(region.x, region.yMax - top);
            uv[4] = new Vector2(region.xMax - right, region.yMax);
            uv[5] = new Vector2(region.xMax, region.yMax);
            uv[6] = new Vector2(region.xMax, region.yMax - top);
            uv[7] = new Vector2(region.xMax - right, region.yMax - top);
            uv[8] = new Vector2(region.x, region.y + bottom);
            uv[9] = new Vector2(region.x + left, region.y + bottom);
            uv[10] = new Vector2(region.x + left, region.y);
            uv[11] = new Vector2(region.x, region.y);
            uv[12] = new Vector2(region.xMax - right, region.y + bottom);
            uv[13] = new Vector2(region.xMax, region.y + bottom);
            uv[14] = new Vector2(region.xMax, region.y);
            uv[15] = new Vector2(region.xMax - right, region.y);


            if (options._flip != 0)
            {
                for (int i = 0; i < uv.Length; i += 4)
                {
                    var zero = Vector2.zero;
                    if (options._flip.IsFlagSet(UISpriteFlip.FlipHorizontal))
                    {
                        zero = uv[i];
                        uv[i] = uv[i + 1];
                        uv[i + 1] = zero;
                        zero = uv[i + 2];
                        uv[i + 2] = uv[i + 3];
                        uv[i + 3] = zero;
                    }
                    if (options._flip.IsFlagSet(UISpriteFlip.FlipVertical))
                    {
                        zero = uv[i];
                        uv[i] = uv[i + 3];
                        uv[i + 3] = zero;
                        zero = uv[i + 1];
                        uv[i + 1] = uv[i + 2];
                        uv[i + 2] = zero;
                    }
                }
                if (options._flip.IsFlagSet(UISpriteFlip.FlipHorizontal))
                {
                    var array = new Vector2[uv.Length];
                    Array.Copy(uv, array, uv.Length);
                    Array.Copy(uv, 0, uv, 4, 4);
                    Array.Copy(array, 4, uv, 0, 4);
                    Array.Copy(uv, 8, uv, 12, 4);
                    Array.Copy(array, 12, uv, 8, 4);
                }
                if (options._flip.IsFlagSet(UISpriteFlip.FlipVertical))
                {
                    var array2 = new Vector2[uv.Length];
                    Array.Copy(uv, array2, uv.Length);
                    Array.Copy(uv, 0, uv, 8, 4);
                    Array.Copy(array2, 8, uv, 0, 4);
                    Array.Copy(uv, 4, uv, 12, 4);
                    Array.Copy(array2, 12, uv, 4, 4);
                }
            }
            for (int j = 0; j < uv.Length; j++)
            {
                renderData.uvs.Add(uv[j]);
            }
        }
        private static void RebuildSlicedColors(UIRenderData renderData, RenderOptions options)
        {
            var color = ((Color)options._color).linear;
            for (int i = 0; i < 16; i++)
            {
                renderData.colors.Add(color);
            }
        }
        private static void DoSlicedFill(UIRenderData renderData, RenderOptions options)
        {
            var baseIndex = options._baseIndex;
            var vertices = renderData.vertices;
            var uvs = renderData.uvs;
            var fillIndices = GetFillIndices(options._fillDirection, baseIndex);
            var invert = options._invertFill;
            if (options._invertFill)
            {
                for (int i = 0; i < fillIndices.Length; i++)
                {
                    Array.Reverse(fillIndices[i]);
                }
            }
            var index = ((options._fillDirection != 0) ? 1 : 0);
            var index1 = vertices[fillIndices[0][invert ? 3 : 0]][index];
            var index2 = vertices[fillIndices[0][(!invert) ? 3 : 0]][index];
            var index3 = Mathf.Abs(index2 - index1);
            var index4 = invert ? (index2 - options._fillAmount * index3) : (index1 + options._fillAmount * index3);

            for (int i = 0; i < fillIndices.Length; i += 1)
            {
                if (!invert)
                {
                    for (int j = 3; j > 0; j -= 1)
                    {
                        var vartex = vertices[fillIndices[i][j]][index];
                        if (vartex >= index4)
                        {
                            Vector3 value = vertices[fillIndices[i][j]];
                            value[index] = index4;
                            vertices[fillIndices[i][j]] = value;
                            float num7 = vertices[fillIndices[i][j - 1]][index];
                            if (num7 <= index4)
                            {
                                var num8 = vartex - num7;
                                var t = (index4 - num7) / num8;
                                var b = uvs[fillIndices[i][j]][index];
                                var a = uvs[fillIndices[i][j - 1]][index];
                                var value2 = uvs[fillIndices[i][j]];
                                value2[index] = Mathf.Lerp(a, b, t);
                                uvs[fillIndices[i][j]] = value2;
                            }
                        }
                    }
                    continue;
                }
                else
                {
                    for (int j = 1; j < 4; j += 1)
                    {
                        float vertex = vertices[fillIndices[i][j]][index];
                        if (vertex <= index4)
                        {
                            var value3 = vertices[fillIndices[i][j]];
                            value3[index] = index4;
                            vertices[fillIndices[i][j]] = value3;
                            var prevVertex = vertices[fillIndices[i][j - 1]][index];
                            if (prevVertex >= index4)
                            {
                                var num11 = vertex - prevVertex;
                                var t2 = (index4 - prevVertex) / num11;
                                var b2 = uvs[fillIndices[i][j]][index];
                                var a2 = uvs[fillIndices[i][j - 1]][index];
                                var value4 = uvs[fillIndices[i][j]];
                                value4[index] = Mathf.Lerp(a2, b2, t2);
                                uvs[fillIndices[i][j]] = value4;
                            }
                        }
                    }
                }
            }
        }
        private static int[][] GetFillIndices(UIFillDirection fillDirection, int baseIndex)
        {
            var array = (fillDirection == UIFillDirection.Horizontal) ? kHorzFill : kVertFill;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    kFillIndices[i][j] = baseIndex + array[i][j];
                }
            }
            return kFillIndices;
        }

        public struct RenderOptions
        {
            public UITextureAtlas _atlas;
            public UITextureAtlas.SpriteInfo _spriteInfo;
            public Color32 _color;
            public float _pixelsToUnits;
            public Vector2 _size;
            public UISpriteFlip _flip;
            public bool _invertFill;
            public UIFillDirection _fillDirection;
            public float _fillAmount;
            public Vector3 _offset;
            public int _baseIndex;
        }
    }
}
