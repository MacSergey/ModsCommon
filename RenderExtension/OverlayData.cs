using UnityEngine;

namespace ModsCommon.Utilities
{
    public interface IOverlay
    {
        void Render(OverlayData data);
    }
    public struct OverlayData
    {
        public RenderManager.CameraInfo CameraInfo { get; }
        public Color? Color;
        public float? Width;
        public bool? CutStart;
        public bool? CutEnd;
        public bool? RenderLimit;
        public bool? AlphaBlend;
        public bool SplitPoint;

        public bool Cut
        {
            set
            {
                CutStart = value;
                CutEnd = value;
            }
        }

        public OverlayData(RenderManager.CameraInfo cameraInfo)
        {
            CameraInfo = cameraInfo;
            Color = null;
            Width = null;
            RenderLimit = null;
            CutStart = null;
            CutEnd = null;
            AlphaBlend = null;
            SplitPoint = false;
        }

        public OverlayData Copy()
        {
            var copy = new OverlayData(CameraInfo)
            {
                Color = Color,
                Width = Width,
                AlphaBlend = AlphaBlend,
                CutStart = CutStart,
                CutEnd = CutEnd,
                SplitPoint = SplitPoint,
            };

            return copy;
        }
    }
}
