using ColossalFramework.UI;
using System;
using UnityEngine;

namespace ModsCommon.UI
{
    public struct RenderOptions
    {
        public UITextureAtlas atlas;
        public UITextureAtlas.SpriteInfo spriteInfo;
        public Color32 color;
        public float pixelsToUnits;
        public Vector2 size;
        public UISpriteFlip flip;
        public bool invertFill;
        public UIFillDirection fillDirection;
        public float fillAmount;
        public Vector3 offset;
        public int baseIndex;
    }

    public static class Render
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

        private static Vector3[] vertices = new Vector3[16];
        private static Vector2[] uv = new Vector2[16];
        private static readonly int[][] kFillIndices = new int[][] { new int[4], new int[4], new int[4], new int[4] };

        public static void RenderSlicedSprite(UIRenderData renderData, RenderOptions options)
        {
            options.baseIndex = renderData.vertices.Count;
            RebuildSlicedTriangles(renderData, options);
            RebuildSlicedVertices(renderData, options);
            RebuildSlicedUV(renderData, options);
            RebuildSlicedColors(renderData, options);
            if (options.fillAmount < 1f)
                DoSlicedFill(renderData, options);
        }
        private static void RebuildSlicedTriangles(UIRenderData renderData, RenderOptions options)
        {
            var baseIndex = options.baseIndex;
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
            var sizeX = Mathf.Ceil(options.size.x);
            var sizeY = Mathf.Ceil(0f - options.size.y);
            var spriteInfo = options.spriteInfo;
            var left = spriteInfo.border.left;
            var top = spriteInfo.border.top;
            var right = spriteInfo.border.right;
            var bottom = spriteInfo.border.bottom;
            if (options.flip.IsFlagSet(UISpriteFlip.FlipHorizontal))
            {
                var num7 = right;
                right = left;
                left = num7;
            }
            if (options.flip.IsFlagSet(UISpriteFlip.FlipVertical))
            {
                var temp = bottom;
                bottom = top;
                top = temp;
            }

            var vertices = Render.vertices;
            vertices[0] = new Vector3(x, y, 0f) + options.offset;
            vertices[1] = vertices[0] + new Vector3(left, 0f, 0f);
            vertices[2] = vertices[0] + new Vector3(left, 0f - top, 0f);
            vertices[3] = vertices[0] + new Vector3(0f, 0f - top, 0f);
            vertices[4] = new Vector3(sizeX - right, y, 0f) + options.offset;
            vertices[5] = vertices[4] + new Vector3(right, 0f, 0f);
            vertices[6] = vertices[4] + new Vector3(right, 0f - top, 0f);
            vertices[7] = vertices[4] + new Vector3(0f, 0f - top, 0f);
            vertices[8] = new Vector3(x, sizeY + bottom, 0f) + options.offset;
            vertices[9] = vertices[8] + new Vector3(left, 0f, 0f);
            vertices[10] = vertices[8] + new Vector3(left, 0f - bottom, 0f);
            vertices[11] = vertices[8] + new Vector3(0f, 0f - bottom, 0f);
            vertices[12] = new Vector3(sizeX - right, sizeY + bottom, 0f) + options.offset;
            vertices[13] = vertices[12] + new Vector3(right, 0f, 0f);
            vertices[14] = vertices[12] + new Vector3(right, 0f - bottom, 0f);
            vertices[15] = vertices[12] + new Vector3(0f, 0f - bottom, 0f);

            for (int i = 0; i < vertices.Length; i++)
            {
                renderData.vertices.Add((vertices[i] * options.pixelsToUnits).Quantize(options.pixelsToUnits));
            }
        }
        private static void RebuildSlicedUV(UIRenderData renderData, RenderOptions options)
        {
            var atlas = options.atlas;
            var size = new Vector2(atlas.texture.width, atlas.texture.height);
            var spriteInfo = options.spriteInfo;
            var top = spriteInfo.border.top / size.y;
            var bottom = spriteInfo.border.bottom / size.y;
            var left = spriteInfo.border.left / size.x;
            var right = spriteInfo.border.right / size.x;
            var region = spriteInfo.region;

            var uv = Render.uv;
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


            if (options.flip != 0)
            {
                for (int i = 0; i < uv.Length; i += 4)
                {
                    var zero = Vector2.zero;
                    if (options.flip.IsFlagSet(UISpriteFlip.FlipHorizontal))
                    {
                        zero = uv[i];
                        uv[i] = uv[i + 1];
                        uv[i + 1] = zero;
                        zero = uv[i + 2];
                        uv[i + 2] = uv[i + 3];
                        uv[i + 3] = zero;
                    }
                    if (options.flip.IsFlagSet(UISpriteFlip.FlipVertical))
                    {
                        zero = uv[i];
                        uv[i] = uv[i + 3];
                        uv[i + 3] = zero;
                        zero = uv[i + 1];
                        uv[i + 1] = uv[i + 2];
                        uv[i + 2] = zero;
                    }
                }
                if (options.flip.IsFlagSet(UISpriteFlip.FlipHorizontal))
                {
                    var array = new Vector2[uv.Length];
                    Array.Copy(uv, array, uv.Length);
                    Array.Copy(uv, 0, uv, 4, 4);
                    Array.Copy(array, 4, uv, 0, 4);
                    Array.Copy(uv, 8, uv, 12, 4);
                    Array.Copy(array, 12, uv, 8, 4);
                }
                if (options.flip.IsFlagSet(UISpriteFlip.FlipVertical))
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
            var color = ((Color)options.color).linear;
            for (int i = 0; i < 16; i++)
            {
                renderData.colors.Add(color);
            }
        }
        private static void DoSlicedFill(UIRenderData renderData, RenderOptions options)
        {
            var baseIndex = options.baseIndex;
            var vertices = renderData.vertices;
            var uvs = renderData.uvs;
            var fillIndices = GetFillIndices(options.fillDirection, baseIndex);
            var invert = options.invertFill;
            if (options.invertFill)
            {
                for (int i = 0; i < fillIndices.Length; i++)
                {
                    Array.Reverse(fillIndices[i]);
                }
            }
            var index = ((options.fillDirection != 0) ? 1 : 0);
            var index1 = vertices[fillIndices[0][invert ? 3 : 0]][index];
            var index2 = vertices[fillIndices[0][(!invert) ? 3 : 0]][index];
            var index3 = Mathf.Abs(index2 - index1);
            var index4 = invert ? (index2 - options.fillAmount * index3) : (index1 + options.fillAmount * index3);

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


        public static void RenderSprite(UIRenderData data, RenderOptions options)
        {
            options.baseIndex = data.vertices.Count;
            RebuildTriangles(data, options);
            RebuildVertices(data, options);
            RebuildUV(data, options);
            RebuildColors(data, options);
            if (options.fillAmount < 1f)
                DoFill(data, options);
        }
        private static void RebuildTriangles(UIRenderData renderData, RenderOptions options)
        {
            var baseIndex = options.baseIndex;
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
            var baseIndex = options.baseIndex;
            var x = 0f;
            var y = 0f;
            var sizeX = Mathf.Ceil(options.size.x);
            var sizeY = Mathf.Ceil(0f - options.size.y);
            vertices.Add(new Vector3(x, y, 0f) * options.pixelsToUnits);
            vertices.Add(new Vector3(sizeX, y, 0f) * options.pixelsToUnits);
            vertices.Add(new Vector3(sizeX, sizeY, 0f) * options.pixelsToUnits);
            vertices.Add(new Vector3(x, sizeY, 0f) * options.pixelsToUnits);
            var vector = options.offset.RoundToInt() * options.pixelsToUnits;
            for (int i = 0; i < 4; i++)
            {
                vertices[baseIndex + i] = (vertices[baseIndex + i] + vector).Quantize(options.pixelsToUnits);
            }
        }
        private static void RebuildUV(UIRenderData renderData, RenderOptions options)
        {
            var region = options.spriteInfo.region;
            var uvs = renderData.uvs;
            uvs.Add(new Vector2(region.x, region.yMax));
            uvs.Add(new Vector2(region.xMax, region.yMax));
            uvs.Add(new Vector2(region.xMax, region.y));
            uvs.Add(new Vector2(region.x, region.y));
            var zero = Vector2.zero;
            if (options.flip.IsFlagSet(UISpriteFlip.FlipHorizontal))
            {
                zero = uvs[1];
                uvs[1] = uvs[0];
                uvs[0] = zero;
                zero = uvs[3];
                uvs[3] = uvs[2];
                uvs[2] = zero;
            }
            if (options.flip.IsFlagSet(UISpriteFlip.FlipVertical))
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
            var color = ((Color)options.color).linear;
            var colors = renderData.colors;
            for (int i = 0; i < 4; i++)
            {
                colors.Add(color);
            }
        }
        private static void DoFill(UIRenderData renderData, RenderOptions options)
        {
            var baseIndex = options.baseIndex;
            var vertices = renderData.vertices;
            var uvs = renderData.uvs;
            var index1 = baseIndex + 3;
            var index2 = baseIndex + 2;
            var index3 = baseIndex;
            var index4 = baseIndex + 1;
            if (options.invertFill)
            {
                if (options.fillDirection == UIFillDirection.Horizontal)
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
            if (options.fillDirection == UIFillDirection.Horizontal)
            {
                vertices[index2] = Vector3.Lerp(vertices[index2], vertices[index1], 1f - options.fillAmount);
                vertices[index4] = Vector3.Lerp(vertices[index4], vertices[index3], 1f - options.fillAmount);
                uvs[index2] = Vector2.Lerp(uvs[index2], uvs[index1], 1f - options.fillAmount);
                uvs[index4] = Vector2.Lerp(uvs[index4], uvs[index3], 1f - options.fillAmount);
            }
            else
            {
                vertices[index3] = Vector3.Lerp(vertices[index3], vertices[index1], 1f - options.fillAmount);
                vertices[index4] = Vector3.Lerp(vertices[index4], vertices[index2], 1f - options.fillAmount);
                uvs[index3] = Vector2.Lerp(uvs[index3], uvs[index1], 1f - options.fillAmount);
                uvs[index4] = Vector2.Lerp(uvs[index4], uvs[index2], 1f - options.fillAmount);
            }
        }
    }
}
