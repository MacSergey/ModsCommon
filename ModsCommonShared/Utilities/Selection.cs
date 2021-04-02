using ColossalFramework.Math;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModsCommon.Utilities
{
    public abstract class Selection : IOverlay
    {
#if DEBUG
        protected static float BorderOverlayWidth => NodeMarkup.Settings.BorderOverlayWidth;
#else
        protected static float BorderOverlayWidth => 3f;
        protected static float MaxOverlayWidth => 16f;
#endif

        public ushort Id { get; }
        protected Data[] Datas { get; }
        protected Vector3 Center { get; set; }
        protected abstract Vector3 Position { get; }
        protected abstract float HalfWidth { get; }
        protected IEnumerable<ITrajectory> BorderLines
        {
            get
            {
                for (var i = 0; i < Datas.Length; i += 1)
                {
                    yield return new StraightTrajectory(Datas[i].leftPos, Datas[i].rightPos);

                    var j = (i + 1) % Datas.Length;
                    if (Datas.Length != 1)
                        yield return new BezierTrajectory(GetBezier(Datas[i].leftPos, Datas[i].leftDir, Datas[j].rightPos, Datas[j].rightDir));
                    else
                        yield return new BezierTrajectory(GetEndBezier(Datas[i].leftPos, Datas[i].leftDir, Datas[j].rightPos, Datas[j].rightDir));
                }
            }
        }
        public Selection(ushort id)
        {
            Id = id;
            Datas = Calculate().OrderBy(s => s.angle).ToArray();
            CalculateCenter();
        }
        protected abstract IEnumerable<Data> Calculate();
        private void CalculateCenter()
        {
            if (Datas.Length == 1)
                Center = Datas[0].Position + Datas[0].Direction;
            else
            {
                Vector3 center = new();
                for (var i = 0; i < Datas.Length; i += 1)
                {
                    var j = (i + 1) % Datas.Length;

                    var bezier = GetBezier(Datas[i].Position, Datas[i].Direction, Datas[j].Position, Datas[j].Direction);
                    center += bezier.Position(0.5f);
                }
                Center = center / Datas.Length;
            }
        }
        public virtual bool Contains(Segment3 ray, out float t)
        {
            var line = new StraightTrajectory(ray.GetRayPosition(Center.y, out t), Center);
            var contains = !BorderLines.Any(b => Intersection.CalculateSingle(line, b).IsIntersect);
            return contains;
        }
        public virtual void Render(OverlayData overlayData)
        {
#if DEBUG
            if (NodeMarkup.Settings.RenderOverlayBorders)
            {
                foreach (var border in BorderLines)
                    border.Render(new OverlayData(overlayData.CameraInfo) { Color = Colors.Green });
            }
            if (NodeMarkup.Settings.RenderOverlayCentre)
                Center.RenderCircle(new OverlayData(overlayData.CameraInfo) { Color = Colors.Red });
#endif
        }
        protected void RenderCorner(OverlayData overlayData, Data data)
        {
            var cornerDelta = data.GetCornerDelta(BorderOverlayWidth / 2);
            var line = new StraightTrajectory(data.leftPos + cornerDelta, data.rightPos - cornerDelta);
            line.Render(overlayData);
        }
        protected void RenderBorder(OverlayData overlayData, Data data1, Data data2)
        {
            var cornerDelta1 = data1.GetCornerDelta(BorderOverlayWidth / 2);
            var cornerDelta2 = data2.GetCornerDelta(BorderOverlayWidth / 2);

            var position1 = data1.leftPos + cornerDelta1;
            var position2 = data2.rightPos - cornerDelta2;

            var direction1 = data1.GetDir(1 - cornerDelta1.magnitude / data1.CornerLength);
            var direction2 = data2.GetDir(cornerDelta2.magnitude / data2.CornerLength);

            var bezier = GetBezier(position1, direction1, position2, direction2);
            bezier.RenderBezier(overlayData);
        }
        protected void RenderMiddle(OverlayData overlayData, Data data1, Data data2)
        {
            overlayData.Cut = true;

            var overlayWidth1 = GetWidth(data1.DeltaAngleCos);
            var overlayWidth2 = GetWidth(data2.DeltaAngleCos);

            var width1 = data1.halfWidth * 2 - BorderOverlayWidth;
            var width2 = data2.halfWidth * 2 - BorderOverlayWidth;

            var angle = Vector3.Angle(data1.Direction, data2.Direction);
            var maxPossibleWidth = angle / 11.25f + 16f;
            var overlayWidth = Mathf.Max(BorderOverlayWidth, Mathf.Min(overlayWidth1, overlayWidth2));

            if (Mathf.Abs(width1 - width2) < 0.001 && maxPossibleWidth >= Mathf.Max(width1, width2) && overlayWidth >= maxPossibleWidth)
            {
                overlayData.Width = Mathf.Min(width1, width2, maxPossibleWidth);
                RenderMiddle(overlayData, data1, data2, 0f, 0f);
            }
            else
            {
                overlayWidth = Mathf.Min(overlayWidth, width1, width2, maxPossibleWidth);
                overlayData.Width = overlayWidth;

                var effectiveWidth = overlayWidth - Mathf.Max(overlayWidth * ((180 - angle) / 720f), 1f);
                var count = Math.Max(Mathf.CeilToInt(width1 / effectiveWidth), Mathf.CeilToInt(width2 / effectiveWidth));

                var step1 = GetStep(width1, overlayWidth, count);
                var step2 = GetStep(width2, overlayWidth, count);

                for (var l = 0; l < count; l += 1)
                    RenderMiddle(overlayData, data1, data2, l * step1, l * step2);
            }

            static float GetWidth(float cos)
            {
                if (Mathf.Abs(cos - 1f) < 0.001f)
                    return float.MaxValue;
                else
                    return (BorderOverlayWidth * 0.9f) / Mathf.Sqrt(1 - Mathf.Pow(cos, 2));
            }
            static float GetStep(float width, float overlayWidth, int count) => count > 1 ? (width - overlayWidth) / (count - 1) : 0f;
        }
        private void RenderMiddle(OverlayData overlayData, Data data1, Data data2, float shift1, float shift2)
        {
            var beginPos = (overlayData.Width.Value + BorderOverlayWidth) / 2;
            var tPos1 = beginPos + shift1;
            var tPos2 = beginPos + shift2;

            var pos1 = data1.leftPos + data1.GetCornerDelta(tPos1);
            var pos2 = data2.rightPos - data2.GetCornerDelta(tPos2);

            var dir1 = data1.GetDir(tPos1 / (data1.halfWidth * 2));
            var dir2 = data2.GetDir(tPos2 / (data2.halfWidth * 2));

            var bezier = GetBezier(pos1, dir1, pos2, dir2);
            bezier.RenderBezier(overlayData);
        }
        protected void RenderEnd(OverlayData overlayData, Data data)
        {
            var count = Mathf.CeilToInt(data.halfWidth / BorderOverlayWidth);
            var halfOverlayWidth = BorderOverlayWidth / 2f;
            var effectiveWidth = data.halfWidth - BorderOverlayWidth;
            var step = effectiveWidth / (count - 1);
            for (var i = 0; i < count; i += 1)
            {
                var tPos = halfOverlayWidth + step * i;
                var tDir = tPos / (data.halfWidth * 2);
                var delta = data.GetCornerDelta(tPos);

                var startPos = data.leftPos + delta;
                var endPos = data.rightPos - delta;

                var startDir = data.GetDir(tDir);
                var endDir = data.GetDir(1 - tDir);

                var bezier = GetEndBezier(startPos, startDir, endPos, endDir, halfOverlayWidth, (tPos - halfOverlayWidth) / effectiveWidth);
                bezier.RenderBezier(overlayData);
            }
        }

        private Bezier3 GetBezier(Vector3 leftPos, Vector3 leftDir, Vector3 rightPos, Vector3 rightDir)
        {
            var bezier = new Bezier3()
            {
                a = leftPos,
                d = rightPos,
            };

            NetSegment.CalculateMiddlePoints(bezier.a, leftDir, bezier.d, rightDir, true, true, out bezier.b, out bezier.c);
            return bezier;
        }
        private Bezier3 GetEndBezier(Vector3 leftPos, Vector3 leftDir, Vector3 rightPos, Vector3 rightDir, float halfWidth = 0f, float ratio = 0f)
        {
            var length = Mathf.Min((leftPos - rightPos).XZ().magnitude / 2 + halfWidth, 8f);
            length = (length - halfWidth) /** (1 - ratio)*/ / 0.75f;
            var bezier = new Bezier3()
            {
                a = leftPos,
                b = leftPos + leftDir * length,
                c = rightPos + rightDir * length,
                d = rightPos,
            };
            return bezier;
        }

        protected struct Data
        {
            public float angle;
            public Vector3 rightPos;
            public Vector3 leftPos;
            public Vector3 rightDir;
            public Vector3 leftDir;
            public float halfWidth;

            public float DeltaAngleCos => (2 * halfWidth) / CornerLength;
            public Vector3 CornerDir => (rightPos - leftPos).normalized;
            public float CornerLength => (rightPos - leftPos).XZ().magnitude;
            public StraightTrajectory Line => new StraightTrajectory(leftPos, rightPos);
            public Vector3 Position => (rightPos + leftPos) / 2;
            public Vector3 Direction => (leftDir + rightDir).normalized;

            public Vector3 GetDir(float t)
            {
                t = Mathf.Clamp01(t);
                return (leftDir * t + rightDir * (1 - t)).normalized;
            }
            public Vector3 GetCornerDelta(float width) => CornerDir * (width / DeltaAngleCos);
        }
    }
    public class NodeSelection : Selection
    {
        protected override Vector3 Position => Id.GetNode().m_position;
        protected override float HalfWidth => Id.GetNode().Info.m_halfWidth;
        public NodeSelection(ushort id) : base(id) { }

        protected override IEnumerable<Data> Calculate()
        {
            var node = Id.GetNode();

            foreach (var segmentId in node.SegmentsId())
            {
                var segment = segmentId.GetSegment();
                var isStart = segment.m_startNode == Id;
                var data = new Data()
                {
                    halfWidth = segment.Info.m_halfWidth.RoundToNearest(0.1f),
                    angle = (isStart ? segment.m_startDirection : segment.m_endDirection).AbsoluteAngle(),
                };

                segment.CalculateCorner(segmentId, true, isStart, true, out data.leftPos, out data.leftDir, out _);
                segment.CalculateCorner(segmentId, true, isStart, false, out data.rightPos, out data.rightDir, out _);

                data.leftDir = (-data.leftDir).normalized;
                data.rightDir = (-data.rightDir).normalized;

                yield return data;
            }
        }
        public override bool Contains(Segment3 ray, out float t)
        {
            var node = Id.GetNode();

            if ((node.m_flags & NetNode.Flags.Middle) == 0)
                return base.Contains(ray, out t);
            else
            {
                t = 0;
                return false;
            }
        }

        public override void Render(OverlayData overlayData)
        {
            overlayData.Width = BorderOverlayWidth;
#if DEBUG
            overlayData.AlphaBlend = NodeMarkup.Settings.AlphaBlendOverlay;
#else
            overlayData.AlphaBlend = false;
#endif

            for (var i = 0; i < Datas.Length; i += 1)
            {
                var data1 = Datas[i];
                var data2 = Datas[(i + 1) % Datas.Length];

                RenderCorner(overlayData, data1);
                if (Datas.Length == 1)
                    RenderEnd(overlayData, Datas[0]);
                else
                {
                    RenderBorder(overlayData, data1, data2);
                    RenderMiddle(overlayData, data1, data2);
                }
            }

            base.Render(overlayData);
        }
    }
    public class SegmentSelection : Selection
    {
        protected override Vector3 Position => Id.GetSegment().m_middlePosition;
        protected override float HalfWidth => Id.GetSegment().Info.m_halfWidth;
        public SegmentSelection(ushort id) : base(id) { }

        protected override IEnumerable<Data> Calculate()
        {
            var segment = Id.GetSegment();

            var startData = new Data()
            {
                halfWidth = segment.Info.m_halfWidth.RoundToNearest(0.1f),
                angle = segment.m_startDirection.AbsoluteAngle(),
            };

            segment.CalculateCorner(Id, true, true, true, out startData.leftPos, out startData.leftDir, out _);
            segment.CalculateCorner(Id, true, true, false, out startData.rightPos, out startData.rightDir, out _);

            yield return startData;

            var endData = new Data()
            {
                halfWidth = segment.Info.m_halfWidth.RoundToNearest(0.1f),
                angle = segment.m_endDirection.AbsoluteAngle(),
            };

            segment.CalculateCorner(Id, true, false, true, out endData.leftPos, out endData.leftDir, out _);
            segment.CalculateCorner(Id, true, false, false, out endData.rightPos, out endData.rightDir, out _);

            yield return endData;
        }

        public override void Render(OverlayData overlayData)
        {
            overlayData.Width = BorderOverlayWidth;
#if DEBUG
            overlayData.AlphaBlend = NodeMarkup.Settings.AlphaBlendOverlay;
#else
            overlayData.AlphaBlend = false;
#endif

            RenderCorner(overlayData, Datas[0]);
            RenderCorner(overlayData, Datas[1]);
            RenderBorder(overlayData, Datas[0], Datas[1]);
            RenderBorder(overlayData, Datas[1], Datas[0]);
            RenderMiddle(overlayData, Datas[0], Datas[1]);

            base.Render(overlayData);
        }
    }
}
