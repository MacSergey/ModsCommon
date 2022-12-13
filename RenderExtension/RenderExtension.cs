using ColossalFramework;
using ColossalFramework.Math;
using System.Collections.Generic;
using UnityEngine;

namespace ModsCommon.Utilities
{
    public static class RenderExtension
    {
        public static int ID_PointX { get; }
        public static int ID_PointZ { get; }
        public static int ID_CutStart { get; }
        public static int ID_CutEnd { get; }
        public static int ID_Size { get; }
        public static int ID_LimitsY { get; }
        public static int ID_Color { get; }
        public static int ID_CenterPos { get; }
        public static int ID_CornersAC { get; }
        public static int ID_CornersBD { get; }

        public static Material ShapeMaterial { get; }
        public static Material BlendShapeMaterial { get; }

        private static Mesh Mesh { get; }

        static RenderExtension()
        {
            ID_PointX = Shader.PropertyToID("_PointX");
            ID_PointZ = Shader.PropertyToID("_PointZ");
            ID_CutStart = Shader.PropertyToID("_CutStart");
            ID_CutEnd = Shader.PropertyToID("_CutEnd");
            ID_Size = Shader.PropertyToID("_Size");
            ID_LimitsY = Shader.PropertyToID("_LimitsY");
            ID_Color = Shader.PropertyToID("_Color");
            ID_CenterPos = Shader.PropertyToID("_CenterPos");
            ID_CornersAC = Shader.PropertyToID("_CornersAC");
            ID_CornersBD = Shader.PropertyToID("_CornersBD");

            Mesh = CreateBoxMesh();
            ShapeMaterial = new Material(Shader.Find("Custom/Overlay/Shape"));
            BlendShapeMaterial = new Material(Shader.Find("Custom/Overlay/ShapeBlend"));
        }

        private static Mesh CreateBoxMesh()
        {
            Vector3[] array = new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f)
            };
            int[] triangles = new int[36];
            int index = 0;

            CreateQuad(triangles, ref index, 0, 2, 3, 1);
            CreateQuad(triangles, ref index, 4, 5, 7, 6);
            CreateQuad(triangles, ref index, 2, 6, 7, 3);
            CreateQuad(triangles, ref index, 0, 1, 5, 4);
            CreateQuad(triangles, ref index, 4, 6, 2, 0);
            CreateQuad(triangles, ref index, 5, 1, 3, 7);

            return new Mesh
            {
                hideFlags = HideFlags.DontSave,
                vertices = array,
                triangles = triangles
            };

            static void CreateQuad(int[] triangles, ref int index, int a, int b, int c, int d)
            {
                triangles[index++] = a;
                triangles[index++] = b;
                triangles[index++] = d;
                triangles[index++] = d;
                triangles[index++] = b;
                triangles[index++] = c;
            }
        }

        private static float DefaultWidth => 0.2f;
        private static bool DefaultBlend => true;
        private static bool DefaultLimit => false;
        private static float DefaultMinLimit => -1f;
        private static float DefaultMaxLimit => 1280f;
        private static Color32 DefaultColor => Colors.White;

        public static void RenderBezier(this Bezier3 bezier, OverlayData data)
        {
            var color = data.Color ?? DefaultColor;
            var width = data.Width ?? DefaultWidth;

            var cutValue = (data.Width ?? DefaultWidth) / 2;
            var startCut = data.CutStart == true ? cutValue : 0f;
            var endCut = data.CutEnd == true ? cutValue : 0f;

            var renderLimit = data.RenderLimit ?? DefaultLimit;
            var minLimit = renderLimit ? (bezier.a.y + bezier.d.y) * 0.5f - 0.01f : DefaultMinLimit;
            var maxLimit = renderLimit ? (bezier.a.y + bezier.d.y) * 0.5f + 0.01f : DefaultMaxLimit;

            var alphaBlend = data.AlphaBlend ?? DefaultBlend;

            DrawBezier(data.CameraInfo, color, bezier, width, startCut, endCut, minLimit, maxLimit, renderLimit, alphaBlend);
        }
        public static void RenderQuad(this Quad3 quad, OverlayData data)
        {
            var color = data.Color ?? DefaultColor;

            var renderLimit = data.RenderLimit ?? DefaultLimit;
            var minLimit = renderLimit ? (quad.a.y + quad.b.y + quad.c.y + quad.d.y) * 0.25f - 0.01f : DefaultMinLimit;
            var maxLimit = renderLimit ? (quad.a.y + quad.b.y + quad.c.y + quad.d.y) * 0.25f + 0.01f : DefaultMaxLimit;

            var alphaBlend = data.AlphaBlend ?? DefaultBlend;

            DrawQuad(data.CameraInfo, color, quad, minLimit, maxLimit, renderLimit, alphaBlend);
        }
        public static void RenderCircle(this Vector3 position, OverlayData data)
        {
            var color = data.Color ?? DefaultColor;
            var width = data.Width ?? DefaultWidth;

            var renderLimit = data.RenderLimit ?? DefaultLimit;
            var minLimit = renderLimit ? position.y - 0.01f : DefaultMinLimit;
            var maxLimit = renderLimit ? position.y + 0.01f : DefaultMaxLimit;

            var alphaBlend = data.AlphaBlend ?? DefaultBlend;

            DrawCircle(data.CameraInfo, color, position, width, minLimit, maxLimit, renderLimit, alphaBlend);
        }

        public static void RenderCircle(this Vector3 position, OverlayData data, float from, float to)
        {
            data.Width = Mathf.Max(from, to, 0f);
            to = Mathf.Max(Mathf.Min(from, to), 0f);

            do
            {
                position.RenderCircle(data);
                data.Width = Mathf.Max(data.Width.Value - 0.43f, to);
            }
            while (data.Width > to);
        }

        public static void RenderAngle(this Vector3 position, OverlayData data, Vector3 startDir, Vector3 endDir, float innerRadius, float outterRadius)
        {
            var startNormal = startDir.Turn90(true);
            var endNormal = endDir.Turn90(false);
            var invert = Vector3.Dot(startNormal, endDir) < 0f;
            if (invert)
            {
                startNormal = -startNormal;
                endNormal = -endNormal;
            }

            var shift = (outterRadius + innerRadius) / 2;
            var bezier = new Bezier3()
            {
                a = position + startDir * shift,
                d = position + endDir * shift,
            };

            var angle = Mathf.Acos(Mathf.Clamp(startDir.x * endDir.x + startDir.z * endDir.z, -1f, 1f));
            float d = Mathf.Tan(angle * 0.5f) * shift * 0.5522848f;
            bezier.b = bezier.a + startNormal * d;
            bezier.c = bezier.d + endNormal * d;

            data.Width = outterRadius - innerRadius;
            data.Cut = true;
            bezier.RenderBezier(data);
        }

        public static void RenderArea(this Vector3[] points, int[] triangles, OverlayData overlayData)
        {
            for (int i = 2; i < triangles.Length; i += 3)
            {
                var quad = new Quad3()
                {
                    a = points[triangles[i]],
                    b = points[triangles[i - 1]],
                    c = (points[triangles[i - 2]] + points[triangles[i - 1]]) * 0.5f,
                    d = points[triangles[i - 2]],
                };
                quad.RenderQuad(overlayData);
            }
        }

        private static void DrawBezier(RenderManager.CameraInfo cameraInfo, Color color, Bezier3 bezier, float size, float cutStart, float cutEnd, float minY, float maxY, bool renderLimits, bool alphaBlend)
        {
            var minVector = bezier.Min() - new Vector3(size * 0.5f, 0f, size * 0.5f);
            Vector3 maxVector = bezier.Max() + new Vector3(size * 0.5f, 0f, size * 0.5f);
            var distance = Vector2.Distance(cameraInfo.m_position, (minVector + maxVector) * 0.5f) * 0.001f + 1f;

            minVector.y = Mathf.Min(minVector.y, minY);
            maxVector.y = Mathf.Max(maxVector.y, maxY);
            var bounds = default(Bounds);
            var scale = new Vector3(distance, distance, distance);
            bounds.SetMinMax(minVector - scale, maxVector + scale);

            if (bounds.Intersects(cameraInfo.m_bounds))
            {
                var startCutSegment = new Segment3(bezier.a, bezier.a);

                var vector3 = VectorUtils.NormalizeXZ(bezier.b - bezier.a, out var len);
                if (len > 0.1f)
                {
                    startCutSegment.a.x += vector3.x * (cutStart - size) + vector3.z * size * 0.5f;
                    startCutSegment.a.z += vector3.z * (cutStart - size) - vector3.x * size * 0.5f;
                    startCutSegment.b.x += vector3.x * (cutStart - size) - vector3.z * size * 0.5f;
                    startCutSegment.b.z += vector3.z * (cutStart - size) + vector3.x * size * 0.5f;
                }
                else
                    startCutSegment = new Segment3(new Vector3(-100000f, -100000f, -100000f), new Vector3(-100000f, -100000f, -100000f));

                var endCutSegment = new Segment3(bezier.d, bezier.d);
                Vector3 vector4 = VectorUtils.NormalizeXZ(bezier.c - bezier.d, out len);
                if (len > 0.1f)
                {
                    endCutSegment.a.x += vector4.x * (cutEnd - size) + vector4.z * size * 0.5f;
                    endCutSegment.a.z += vector4.z * (cutEnd - size) - vector4.x * size * 0.5f;
                    endCutSegment.b.x += vector4.x * (cutEnd - size) - vector4.z * size * 0.5f;
                    endCutSegment.b.z += vector4.z * (cutEnd - size) + vector4.x * size * 0.5f;
                }
                else
                    endCutSegment = new Segment3(new Vector3(-100000f, -100000f, -100000f), new Vector3(-100000f, -100000f, -100000f));

                Material material = alphaBlend ? BlendShapeMaterial : ShapeMaterial;
                material.SetVector(ID_PointX, new Vector4(bezier.a.x, bezier.b.x, bezier.c.x, bezier.d.x));
                material.SetVector(ID_PointZ, new Vector4(bezier.a.z, bezier.b.z, bezier.c.z, bezier.d.z));
                material.SetVector(ID_CutStart, new Vector4(startCutSegment.a.x, startCutSegment.a.z, startCutSegment.b.x, startCutSegment.b.z));
                material.SetVector(ID_CutEnd, new Vector4(endCutSegment.a.x, endCutSegment.a.z, endCutSegment.b.x, endCutSegment.b.z));
                material.SetFloat(ID_Size, size * 0.5f);
                material.SetVector(ID_LimitsY, renderLimits ? new Vector4(minY, -100000f, 100000f, maxY) : new Vector4(-100000f, minY, maxY, 100000f));
                color = color.linear;
                material.SetVector(ID_Color, new Vector4(color.r, color.g, color.b, color.a));
                DrawEffect(cameraInfo, material, 3, bounds);
            }
        }
        private static void DrawQuad(RenderManager.CameraInfo cameraInfo, Color color, Quad3 quad, float minY, float maxY, bool renderLimits, bool alphaBlend)
        {
            var min = quad.Min();
            var max = quad.Max();
            var distance = Vector2.Distance(cameraInfo.m_position, (min + max) * 0.5f) * 0.001f + 1f;
            var cornerAC = new Vector4(quad.a.x, quad.a.z, quad.c.x, quad.c.z);
            var cornerBD = new Vector4(quad.b.x, quad.b.z, quad.d.x, quad.d.z);
            var limits = renderLimits ? new Vector4(minY, -100000f, 100000f, maxY) : new Vector4(-100000f, minY, maxY, 100000f);
            min.y = Mathf.Min(min.y, minY);
            max.y = Mathf.Max(max.y, maxY);
            var bounds = default(Bounds);
            var distance3 = new Vector3(distance, distance, distance);
            bounds.SetMinMax(min - distance3, max + distance3);
            if (bounds.Intersects(cameraInfo.m_bounds))
            {
                Material material = alphaBlend ? BlendShapeMaterial : ShapeMaterial;
                material.color = color.linear;
                material.SetVector(ID_CornersAC, cornerAC);
                material.SetVector(ID_CornersBD, cornerBD);
                material.SetVector(ID_LimitsY, limits);
                DrawEffect(cameraInfo, material, 0, bounds);
            }
        }
        private static void DrawCircle(RenderManager.CameraInfo cameraInfo, Color color, Vector3 center, float size, float minY, float maxY, bool renderLimits, bool alphaBlend)
        {
            float num = Vector2.Distance(cameraInfo.m_position, center) * 0.001f + 1f;
            Vector4 value = new Vector4(center.x, center.z, size * -0.5f, size * 0.5f);
            Vector4 value2 = renderLimits ? new Vector4(minY, -100000f, 100000f, maxY) : new Vector4(-100000f, minY, maxY, 100000f);
            Vector3 vector = center - new Vector3(size * 0.5f, 0f, size * 0.5f);
            Vector3 vector2 = center + new Vector3(size * 0.5f, 0f, size * 0.5f);
            vector.y = Mathf.Min(vector.y, minY);
            vector2.y = Mathf.Max(vector2.y, maxY);
            Bounds bounds = default(Bounds);
            Vector3 vector3 = new Vector3(num, num, num);
            bounds.SetMinMax(vector - vector3, vector2 + vector3);
            if (bounds.Intersects(cameraInfo.m_bounds))
            {
                Material material = alphaBlend ? BlendShapeMaterial : ShapeMaterial;
                material.color = color.linear;
                material.SetVector(ID_CenterPos, value);
                material.SetVector(ID_LimitsY, value2);
                DrawEffect(cameraInfo, material, 1, bounds);
            }
        }
        private static void DrawEffect(RenderManager.CameraInfo cameraInfo, Material material, int pass, Bounds bounds)
        {
            if (bounds.Intersects(cameraInfo.m_nearBounds))
            {
                if (material.SetPass(pass))
                {
                    var matrix = default(Matrix4x4);
                    matrix.SetTRS(cameraInfo.m_position + cameraInfo.m_forward * (cameraInfo.m_near + 1f), cameraInfo.m_rotation, new Vector3(100f, 100f, 1f));
                    Graphics.DrawMeshNow(Mesh, matrix);
                }
            }
            else if (material.SetPass(pass))
            {
                var matrix = default(Matrix4x4);
                matrix.SetTRS(bounds.center, Quaternion.identity, bounds.size);
                Graphics.DrawMeshNow(Mesh, matrix);
            }
        }
    }
}
