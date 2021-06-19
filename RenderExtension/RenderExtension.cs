using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace ModsCommon.Utilities
{
    public static class RenderExtension
    {
        public static RenderManager RenderManager => Singleton<RenderManager>.instance;

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
            var minLimit = renderLimit ? (bezier.a.y + bezier.d.y) / 2f - 0.01f : DefaultMinLimit;
            var maxLimit = renderLimit ? (bezier.a.y + bezier.d.y) / 2f + 0.01f : DefaultMaxLimit;

            var alphaBlend = data.AlphaBlend ?? DefaultBlend;

            RenderManager.OverlayEffect.DrawBezier(data.CameraInfo, color, bezier, width, startCut, endCut, minLimit, maxLimit, renderLimit, alphaBlend);
        }
        public static void RenderCircle(this Vector3 position, OverlayData data)
        {
            var color = data.Color ?? DefaultColor;
            var width = data.Width ?? DefaultWidth;

            var renderLimit = data.RenderLimit ?? DefaultLimit;
            var minLimit = renderLimit ? position.y - 0.01f : DefaultMinLimit;
            var maxLimit = renderLimit ? position.y + 0.01f : DefaultMaxLimit;

            var alphaBlend = data.AlphaBlend ?? DefaultBlend;

            RenderManager.OverlayEffect.DrawCircle(data.CameraInfo, color, position, width, minLimit, maxLimit, renderLimit, alphaBlend);
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
        public static void RenderQuad(this Quad3 quad, OverlayData data) => RenderManager.OverlayEffect.DrawQuad(data.CameraInfo, data.Color ?? Colors.White, quad, -1f, 1280f, false, data.AlphaBlend ?? DefaultBlend);

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
    }
}
