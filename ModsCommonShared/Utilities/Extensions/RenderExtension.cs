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
    }
}
