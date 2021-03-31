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
        protected static float OverlayWidth => 2f;

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

        protected void Render(OverlayData overlayData, Data data1, Data data2, bool isEndBezier = false)
        {
            var count = Math.Max(Mathf.CeilToInt(2 * data1.halfWidth / (OverlayWidth * 0.75f)), Mathf.CeilToInt(2 * data2.halfWidth / (OverlayWidth * 0.75f)));

            var step1 = data1.GetStep(count);
            var step2 = data2.GetStep(count);

            var ratio1 = data1.Ratio;
            var ratio2 = data2.Ratio;

            var cornerDir1 = data1.CornerDir;
            var cornerDir2 = data2.CornerDir;

            for (var l = 0; l < count; l += 1)
            {
                var tPos1 = (OverlayWidth / 2 + l * step1);
                var tPos2 = (OverlayWidth / 2 + l * step2);

                var pos1 = data1.leftPos + cornerDir1 * tPos1 * ratio1;
                var pos2 = data2.rightPos - cornerDir2 * tPos2 * ratio2;

                var dir1 = data1.GetDir(tPos1 / (2 * data1.halfWidth));
                var dir2 = data2.GetDir(tPos2 / (2 * data2.halfWidth));

                Bezier3 bezier;
                if (!isEndBezier)
                    bezier = GetBezier(pos1, dir1, pos2, dir2);
                else
                {
                    var ratio = Mathf.Abs(count - 2 * l) / (float)count;
                    bezier = GetEndBezier(pos1, dir1, pos2, dir2, OverlayWidth / 2, ratio);
                }
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
        private Bezier3 GetEndBezier(Vector3 leftPos, Vector3 leftDir, Vector3 rightPos, Vector3 rightDir, float halfWidth = 0f, float ratio = 1f)
        {
            var length = (Mathf.Min((leftPos - rightPos).XZ().magnitude / 2, 8f) - halfWidth) * ratio / 0.75f;
            var bezier = new Bezier3()
            {
                a = leftPos,
                b = leftPos + leftDir * length,
                c = rightPos + rightDir * length,
                d = rightPos,
            };
            return bezier;
        }
        protected void Render(OverlayData overlayData, Data data)
        {
            var cornerDir = data.CornerDir * data.Ratio * (OverlayWidth / 2);
            var line = new StraightTrajectory(data.leftPos + cornerDir, data.rightPos - cornerDir);
            line.Render(overlayData);
        }
        public virtual void Render(OverlayData overlayData)
        {
            foreach (var border in BorderLines)
                border.Render(new OverlayData(overlayData.CameraInfo) { Color = Colors.Green });
            Center.RenderCircle(new OverlayData(overlayData.CameraInfo) { Color = Colors.Red });
        }

        protected struct Data
        {
            public float angle;
            public Vector3 rightPos;
            public Vector3 leftPos;
            public Vector3 rightDir;
            public Vector3 leftDir;
            public float halfWidth;

            public float Ratio => (rightPos - leftPos).XZ().magnitude / (2 * halfWidth);
            public Vector3 CornerDir => (rightPos - leftPos).normalized;
            public StraightTrajectory Line => new StraightTrajectory(leftPos, rightPos);
            public Vector3 Position => (rightPos + leftPos) / 2;
            public Vector3 Direction => (leftDir + rightDir).normalized;

            public float GetStep(int count) => (2 * halfWidth - OverlayWidth) / (count - 1);
            public Vector3 GetDir(float t)
            {
                t = Mathf.Clamp01(t);
                return (leftDir * t + rightDir * (1 - t)).normalized;
            }
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
            overlayData.Width = OverlayWidth;
            overlayData.AlphaBlend = false;

            for (var i = 0; i < Datas.Length; i += 1)
            {
                var data1 = Datas[i];
                var data2 = Datas[(i + 1) % Datas.Length];

                Render(overlayData, data1, data2, Datas.Length == 1);
                Render(overlayData, data1);
            }

            //base.Render(overlayData);
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
            overlayData.Width = OverlayWidth;
            overlayData.AlphaBlend = false;

            Render(overlayData, Datas[0], Datas[1]);
            Render(overlayData, Datas[0]);
            Render(overlayData, Datas[1]);

            //base.Render(overlayData);
        }
    }
}
