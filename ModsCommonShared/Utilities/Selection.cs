using ColossalFramework.Math;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ColossalFramework.Math.VectorUtils;

namespace ModsCommon.Utilities
{
    public abstract class Selection : IOverlay
    {
#if DEBUGMARKING
        protected static float BorderOverlayWidth => NodeMarkup.Settings.BorderOverlayWidth;
#else
        public static float BorderOverlayWidth => 3f;
#endif

        public ushort Id { get; }
        protected Data[] DataArray { get; }
        public IEnumerable<Data> Datas
        {
            get
            {
                foreach (var data in DataArray)
                    yield return data;
            }
        }
        public Vector3 Center { get; private set; }
        protected abstract Vector3 Position { get; }
        protected abstract float HalfWidth { get; }
        protected IEnumerable<ITrajectory> BorderLines
        {
            get
            {
                for (var i = 0; i < DataArray.Length; i += 1)
                {
                    yield return new StraightTrajectory(DataArray[i].leftPos, DataArray[i].rightPos);

                    var j = (i + 1) % DataArray.Length;
                    if (DataArray.Length != 1)
                        yield return new BezierTrajectory(GetBezier(DataArray[i].leftPos, DataArray[i].LeftDir, DataArray[j].rightPos, DataArray[j].RightDir));
                    else
                        yield return new BezierTrajectory(GetEndBezier(DataArray[i].leftPos, DataArray[i].LeftDir, DataArray[j].rightPos, DataArray[j].RightDir));
                }
            }
        }
        public Selection(ushort id)
        {
            Id = id;
            DataArray = Calculate().OrderBy(s => s.angle).ToArray();
            CalculateCenter();
            if (DataArray.Length > 1)
            {
                for (var i = 0; i < DataArray.Length; i += 1)
                {
                    var delta = 3 - (DataArray[i].Position - Center).magnitude;
                    if (delta > 0f)
                    {
                        DataArray[i].leftPos -= delta * DataArray[i].LeftDir;
                        DataArray[i].rightPos -= delta * DataArray[i].RightDir;
                    }
                }
            }
        }
        protected abstract IEnumerable<Data> Calculate();
        private void CalculateCenter()
        {
            if (DataArray.Length == 1)
                Center = DataArray[0].Position + Mathf.Min(1f, DataArray[0].halfWidth / 2) * DataArray[0].Direction;
            else
            {
                Vector3 center = new();
                for (var i = 0; i < DataArray.Length; i += 1)
                {
                    var j = (i + 1) % DataArray.Length;

                    var bezier = GetBezier(DataArray[i].Position, DataArray[i].Direction, DataArray[j].Position, DataArray[j].Direction);
                    center += bezier.Position(0.5f);
                }
                Center = center / DataArray.Length;
            }
        }
        public virtual bool Contains(Segment3 ray, out float t)
        {
            var line = new StraightTrajectory(GetHitPosition(ray, out t), Center);
            var contains = !BorderLines.Any(b => Intersection.Intersect(line, b));
            return contains;
        }
        public virtual Vector3 GetHitPosition(Segment3 ray, out float t) => ray.GetRayPosition(Center.y, out t);
        public virtual void Render(OverlayData overlayData)
        {
#if DEBUGMARKING
            if (NodeMarkup.Settings.RenderOverlayBorders)
                RenderBorders(new OverlayData(overlayData.CameraInfo) { Color = Colors.Green });
            if (NodeMarkup.Settings.RenderOverlayCentre)
                RenderCenter(new OverlayData(overlayData.CameraInfo) { Color = Colors.Red });
#endif
        }
        public void RenderBorders(OverlayData overlayData)
        {
            foreach (var border in BorderLines)
                border.Render(overlayData);
        }
        public void RenderCenter(OverlayData overlayData) => Center.RenderCircle(overlayData);

        protected void RenderCorner(OverlayData overlayData, Data data)
        {
            var cornerDelta = data.GetCornerDelta(Mathf.Min(BorderOverlayWidth / 2, data.halfWidth));
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
            var maxPossibleWidth = Math.Min(angle / 11.25f + 16f, Mathf.Max(BorderOverlayWidth, Mathf.Min(overlayWidth1, overlayWidth2)));

            if (Mathf.Abs(width1 - width2) < 0.001 && maxPossibleWidth >= Mathf.Max(width1, width2))
            {
                overlayData.Width = Mathf.Min(width1, width2);
                RenderMiddle(overlayData, data1, data2, 0f, 0f);
            }
            else
            {
                var overlayWidth = Mathf.Min(width1, width2, maxPossibleWidth);
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
            var halfOverlayWidth = Mathf.Min(BorderOverlayWidth / 2f, data.halfWidth);
            var effectiveWidth = Mathf.Max(data.halfWidth - BorderOverlayWidth, 0f);
            var step = count > 1 ? effectiveWidth / (count - 1) : 0f;
            for (var i = 0; i < count; i += 1)
            {
                var tPos = halfOverlayWidth + step * i;
                var tDir = tPos / (data.halfWidth * 2);
                var delta = data.GetCornerDelta(tPos);

                var startPos = data.leftPos + delta;
                var endPos = data.rightPos - delta;

                var startDir = data.GetDir(tDir) * data.GetDirLength(tDir);
                var endDir = data.GetDir(1 - tDir) * data.GetDirLength(1 - tDir);

                var bezier = GetEndBezier(startPos, startDir, endPos, endDir, halfOverlayWidth);
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
        private Bezier3 GetEndBezier(Vector3 leftPos, Vector3 leftDir, Vector3 rightPos, Vector3 rightDir, float halfWidth = 0f)
        {
            var length = Mathf.Min(LengthXZ(leftPos - rightPos) / 2 + halfWidth, 8f);
            length = (length - halfWidth) / 0.75f;
            var bezier = new Bezier3()
            {
                a = leftPos,
                b = leftPos + leftDir * length,
                c = rightPos + rightDir * length,
                d = rightPos,
            };
            return bezier;
        }

        public struct Data
        {
            public ushort Id { get; }
            public float angle;

            public Vector3 leftPos;
            public Vector3 rightPos;

            private Vector3 _leftDir;
            private Vector3 _rightDir;

            private float _leftDirLenght;
            private float _rightDirLenght;

            public float halfWidth;

            public Vector3 LeftDir
            {
                get => _leftDir;
                set
                {
                    _leftDir = value.normalized;
                    _leftDirLenght = LengthXZ(value);
                }
            }
            public Vector3 RightDir
            {
                get => _rightDir;
                set
                {
                    _rightDir = value.normalized;
                    _rightDirLenght = LengthXZ(value);
                }
            }

            public Data(ushort id)
            {
                Id = id;
                angle = 0f;

                leftPos = Vector3.zero;
                rightPos = Vector3.zero;

                _leftDir = Vector3.zero;
                _rightDir = Vector3.zero;

                _leftDirLenght = 1f;
                _rightDirLenght = 1f;

                halfWidth = 0f;
            }

            public float DeltaAngleCos => Mathf.Clamp01((2 * halfWidth) / CornerLength);
            public Vector3 CornerDir => (rightPos - leftPos).normalized;
            public float CornerLength => (rightPos - leftPos).XZ().magnitude;
            public StraightTrajectory Line => new StraightTrajectory(leftPos, rightPos);
            public Vector3 Position => (rightPos + leftPos) / 2;
            public Vector3 Direction => (_leftDir + _rightDir).normalized;

            public Vector3 GetDir(float t)
            {
                t = Mathf.Clamp01(t);
                return (_leftDir * t + _rightDir * (1 - t)).normalized;
            }
            public float GetDirLength(float t)
            {
                t = Mathf.Clamp01(t);
                return _leftDirLenght * t + _rightDirLenght * (1 - t);
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

            foreach (var segmentId in node.SegmentIds())
            {
                var segment = segmentId.GetSegment();
                var isStart = segment.IsStartNode(Id);
                var data = new Data(segmentId)
                {
                    halfWidth = segment.Info.m_halfWidth.RoundToNearest(0.1f),
                    angle = (isStart ? segment.m_startDirection : segment.m_endDirection).AbsoluteAngle(),
                };

                segment.CalculateCorner(segmentId, true, isStart, true, out data.leftPos, out var leftDir, out _);
                segment.CalculateCorner(segmentId, true, isStart, false, out data.rightPos, out var rightDir, out _);

                data.LeftDir = -leftDir;
                data.RightDir = -rightDir;

                if (node.m_flags.CheckFlags(NetNode.Flags.Middle))
                {
                    data.leftPos -= 3 * data.LeftDir;
                    data.rightPos -= 3 * data.RightDir;
                }

                yield return data;
            }
        }

        public override void Render(OverlayData overlayData)
        {
#if DEBUGMARKING
            overlayData.AlphaBlend = NodeMarkup.Settings.AlphaBlendOverlay;
#else
            overlayData.AlphaBlend = false;
#endif

            for (var i = 0; i < DataArray.Length; i += 1)
            {
                var data1 = DataArray[i];
                var data2 = DataArray[(i + 1) % DataArray.Length];
                overlayData.Width = Mathf.Min(BorderOverlayWidth, data1.halfWidth * 2, data2.halfWidth * 2);

                RenderCorner(overlayData, data1);
                if (DataArray.Length == 1)
                    RenderEnd(overlayData, DataArray[0]);
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
        public float Length => new BezierTrajectory(DataArray[0].Position, DataArray[0].Direction, DataArray[1].Position, DataArray[1].Direction).Length;
        public SegmentSelection(ushort id) : base(id) { }

        protected override IEnumerable<Data> Calculate()
        {
            var segment = Id.GetSegment();

            var startData = new Data(segment.m_startNode)
            {
                halfWidth = segment.Info.m_halfWidth.RoundToNearest(0.1f),
                angle = segment.m_startDirection.AbsoluteAngle(),
            };

            segment.CalculateCorner(Id, true, true, true, out startData.leftPos, out var startLeftDir, out _);
            segment.CalculateCorner(Id, true, true, false, out startData.rightPos, out var startRightDir, out _);
            startData.LeftDir = startLeftDir;
            startData.RightDir = startRightDir;

            yield return startData;

            var endData = new Data(segment.m_endNode)
            {
                halfWidth = segment.Info.m_halfWidth.RoundToNearest(0.1f),
                angle = segment.m_endDirection.AbsoluteAngle(),
            };

            segment.CalculateCorner(Id, true, false, true, out endData.leftPos, out var endLeftDir, out _);
            segment.CalculateCorner(Id, true, false, false, out endData.rightPos, out var endRightDir, out _);
            endData.LeftDir = endLeftDir;
            endData.RightDir = endRightDir;

            yield return endData;
        }
        public override Vector3 GetHitPosition(Segment3 ray, out float t) => GetHitPosition(ray, out t, out _);
        public Vector3 GetHitPosition(Segment3 ray, out float t, out Vector3 position) => Id.GetSegment().GetHitPosition(ray, out t, out position);

        public override void Render(OverlayData overlayData)
        {
            overlayData.Width = Mathf.Min(BorderOverlayWidth, DataArray[0].halfWidth * 2, DataArray[1].halfWidth * 2);
#if DEBUGMARKING
            overlayData.AlphaBlend = NodeMarkup.Settings.AlphaBlendOverlay;
#else
            overlayData.AlphaBlend = false;
#endif

            RenderCorner(overlayData, DataArray[0]);
            RenderCorner(overlayData, DataArray[1]);
            RenderBorder(overlayData, DataArray[0], DataArray[1]);
            RenderBorder(overlayData, DataArray[1], DataArray[0]);
            RenderMiddle(overlayData, DataArray[0], DataArray[1]);

            base.Render(overlayData);
        }
    }
}
