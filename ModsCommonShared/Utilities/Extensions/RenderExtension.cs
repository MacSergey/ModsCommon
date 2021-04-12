using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModsCommon.Utilities
{
    public static class RenderExtension
    {
        public static RenderManager RenderManager => Singleton<RenderManager>.instance;

        private static float DefaultWidth => 0.2f;
        private static bool DefaultBlend => true;
        public static void RenderBezier(this Bezier3 bezier, OverlayData data)
        {
            var cutValue = (data.Width ?? DefaultWidth) / 2;
            RenderManager.OverlayEffect.DrawBezier(data.CameraInfo, data.Color ?? Colors.White, bezier, data.Width ?? DefaultWidth, data.CutStart == true ? cutValue : 0f, data.CutEnd == true ? cutValue : 0f, -1f, 1280f, false, data.AlphaBlend ?? DefaultBlend);
        }
        public static void RenderCircle(this Vector3 position, OverlayData data) =>
            RenderManager.OverlayEffect.DrawCircle(data.CameraInfo, data.Color ?? Colors.White, position, data.Width ?? DefaultWidth, -1f, 1280f, false, data.AlphaBlend ?? DefaultBlend);
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
