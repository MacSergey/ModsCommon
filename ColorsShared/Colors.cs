using System;
using UnityEngine;

namespace ModsCommon.Utilities
{
    public static class Colors
    {
        private const byte Alpha = 224;
        public static Color32 White { get; } = new Color32(255, 255, 255, 255);
        public static Color32 Green { get; } = new Color32(0, 200, 81, 255);
        public static Color32 Red { get; } = new Color32(255, 68, 68, 255);
        public static Color32 Blue { get; } = new Color32(0, 180, 255, 255);
        public static Color32 Orange { get; } = new Color32(255, 136, 0, 255);
        public static Color32 Yellow { get; } = new Color32(255, 187, 51, 255);
        public static Color32 Gray { get; } = new Color32(192, 192, 192, 255);
        public static Color32 Purple { get; } = new Color32(148, 87, 255, 255);
        public static Color32 Hover { get; } = new Color32(217, 251, 255, 255);
        public static Color32 Error { get; } = new Color32(253, 77, 60, 255);
        public static Color32 Warning { get; } = new Color32(253, 150, 62, 255);

        public static Color32[] OverlayColors { get; } = new Color32[]
        {
            new Color32(218, 33, 40, Alpha), //Red
            new Color32(72, 184, 94, Alpha), //Green
            new Color32(0, 120, 191, Alpha), //Blue

            new Color32(245, 130, 32, Alpha), //Orange
            new Color32(142, 71, 155, Alpha), //Purple
            new Color32(180, 212, 69, Alpha), //Lime

            new Color32(0, 193, 243, Alpha), //SkyBlue
            new Color32(255, 198, 26, Alpha), //Yellow
            new Color32(230, 106, 192, Alpha), //Pink

            new Color32(53, 201, 159, Alpha), //Turquoise
        };
        public enum Overlay
        {
            Red,
            Green,
            Blue,
            Orange,
            Purple,
            Lime,
            SkyBlue,
            Yellow,
            Pink,
            Turquoise,
        }

        public static Color32 GetOverlayColor(int index, byte alpha = Alpha, byte hue = 255)
        {
            var color = OverlayColors[index % OverlayColors.Length];
            color.a = alpha;
            return hue == 255 ? color : color.SetHue(hue);
        }
        public static Color32 GetOverlayColor(Overlay index, byte alpha = Alpha, byte hue = 255) => GetOverlayColor((int)index, alpha, hue);

        public static Color32 SetHue(this Color32 color, byte hue) => new Color32(SetHue(color.r, hue), SetHue(color.g, hue), SetHue(color.b, hue), color.a);
        private static byte SetHue(byte value, byte hue) => (byte)(byte.MaxValue - ((byte.MaxValue - value) / 255f * hue));
        public static Color32 SetAlpha(this Color32 color, byte alpha) => new Color32(color.r, color.g, color.b, alpha);

        public static Color32 GetStyleIconColor(this Color32 color)
        {
            var ratio = 255 / (float)Math.Max(Math.Max(color.r, color.g), color.b);
            var styleColor = new Color32((byte)(color.r * ratio), (byte)(color.g * ratio), (byte)(color.b * ratio), 255);
            return styleColor == Color.black ? (Color32)Color.white : styleColor;
        }
        public static Vector4 ToX3Vector(this Color32 c) => ToX3Vector((Color)c);
        public static Vector4 ToX3Vector(this Color c) => new Vector4(ColorChange(c.r), ColorChange(c.g), ColorChange(c.b), Mathf.Pow(c.a, 2)/* c.a == 0 ? 0 : ColorChange(c.a) * 0.985f + 0.015f*/);
        private static float ColorChange(float c) => Mathf.Pow(c, 4);
    }
}
