using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ColossalFramework.Math.VectorUtils;
using static ModsCommon.Settings.Helper;

namespace ModsCommon.Utilities
{
    public abstract class Selection : IOverlay, IEquatable<Selection>
    {
#if DEBUG
        public static SavedBool AlphaBlendOverlay { get; } = new SavedBool(nameof(AlphaBlendOverlay), string.Empty, false);
        public static SavedFloat OverlayWidth { get; } = new SavedFloat(nameof(OverlayWidth), string.Empty, 3f);
        public static SavedBool RenderOverlayCentre { get; } = new SavedBool(nameof(RenderOverlayCentre), string.Empty, false);
        public static SavedBool RenderOverlayBorders { get; } = new SavedBool(nameof(RenderOverlayBorders), string.Empty, false);

        public static void AddAlphaBlendOverlay(UIComponent group) => group.AddToggle("Alpha blend overlay", AlphaBlendOverlay);
        public static void AddRenderOverlayCentre(UIComponent group) => group.AddToggle("Render overlay center", RenderOverlayCentre);
        public static void AddRenderOverlayBorders(UIComponent group) => group.AddToggle("Render overlay borders", RenderOverlayBorders);
        public static void AddBorderOverlayWidth(UIComponent group) => group.AddFloatField("Overlay width", OverlayWidth, 1f);

        public static float BorderOverlayWidth => OverlayWidth;
#else
        public static float BorderOverlayWidth => 3f;
#endif
        public static SelectionComparer Comparer { get; } = new SelectionComparer();

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

        private StraightTrajectory[] _dataLines;
        private BezierTrajectory[] _betweenDataLines;
        private Rect? _rect;
        public IEnumerable<StraightTrajectory> DataLines
        {
            get
            {
                if (_dataLines == null)
                {
                    _dataLines = new StraightTrajectory[DataArray.Length];
                    for (var i = 0; i < DataArray.Length; i += 1)
                        _dataLines[i] = new StraightTrajectory(DataArray[i].rightPos, DataArray[i].leftPos);
                }
                return _dataLines;
            }
        }
        public IEnumerable<BezierTrajectory> BetweenDataLines
        {
            get
            {
                if (_betweenDataLines == null)
                {
                    _betweenDataLines = new BezierTrajectory[DataArray.Length];
                    for (var i = 0; i < DataArray.Length; i += 1)
                    {
                        if (DataArray.Length != 1)
                        {
                            var j = i.NextIndex(DataArray.Length);
                            _betweenDataLines[i] = new BezierTrajectory(GetBezier(DataArray[i].leftPos, DataArray[i].LeftDir, DataArray[j].rightPos, DataArray[j].RightDir));
                        }
                        else
                            _betweenDataLines[i] = new BezierTrajectory(GetEndBezier(DataArray[i].leftPos, DataArray[i].LeftDir, DataArray[i].rightPos, DataArray[i].RightDir));
                    }
                }
                return _betweenDataLines;
            }
        }
        protected IEnumerable<ITrajectory> BorderLines
        {
            get
            {
                var data = DataLines.GetEnumerator();
                var between = BetweenDataLines.GetEnumerator();

                while (data.MoveNext() && between.MoveNext())
                {
                    yield return data.Current;
                    yield return between.Current;
                }
            }
        }
        protected Rect Rect
        {
            get
            {
                _rect ??= BorderLines.GetRect();
                return _rect.Value;
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
        public abstract bool Equals(Selection other);
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
            var position = GetHitPosition(ray, out t);
            if (!Rect.Contains(XZ(position)))
                return false;

            var line = new StraightTrajectory(position, position + 1000f * Vector3.right);

            var count = 0;
            foreach (var border in BorderLines)
            {
                foreach (var intersect in Intersection.Calculate(line, border))
                {
                    if (intersect.isIntersect)
                        count += 1;
                }
            }

            return count % 2 == 1;
        }
        public virtual Vector3 GetHitPosition(Segment3 ray, out float t) => ray.GetRayPosition(Center.y, out t);

        #region RENDER

        public virtual void Render(OverlayData overlayData)
        {
#if DEBUG
            overlayData.AlphaBlend = AlphaBlendOverlay;
#else
            overlayData.AlphaBlend = false;
#endif

            var t = new float[DataArray.Length];
            for (int i = 0; i < t.Length; i += 1)
            {
                t[i] = (BorderOverlayWidth * 0.5f) / (DataArray[i].halfWidth * 2f);
            }

            var contour = new List<ITrajectory>();
            for (int i = 0; i < t.Length; i += 1)
            {
                var dataLine = new StraightTrajectory(DataArray[i].GetPos(t[i]), DataArray[i].GetPos(1f - t[i]));
                contour.Add(dataLine);

                var j = DataArray.Length != 1 ? i.NextIndex(DataArray.Length) : i;
                var startPos = DataArray[i].GetPos(1 - t[i]);
                var startDir = DataArray[i].GetDir(1 - t[i]);
                var endPos = DataArray[j].GetPos(t[j]);
                var endDir = DataArray[j].GetDir(t[j]);

                if (DataArray.Length != 1)
                {
                    var betweenLine = new BezierTrajectory(GetBezier(startPos, startDir, endPos, endDir));
                    contour.Add(betweenLine);
                }
                else
                {
                    var betweenLine = new BezierTrajectory(GetEndBezier(startPos, startDir, endPos, endDir, BorderOverlayWidth * 0.5f));
                    contour.Add(betweenLine);
                }
            }

            {
                var borderOverlay = overlayData;
                borderOverlay.Width = BorderOverlayWidth;
                foreach (var trajectory in contour)
                    trajectory.Render(borderOverlay);
            }

            {
                List<ITrajectory> split = new List<ITrajectory>();
                foreach (var trajectory in contour)
                {
                    SplitTrajectory(0, trajectory, trajectory.DeltaAngle, 20f, 1f, 50f, split);
                }
                var direction = contour.GetDirection();
                var points = split.Select(i => i.StartPosition).ToArray();
                var triangles = Triangulator.TriangulateSimple(points, direction);

                if (triangles == null)
                    return;

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

#if DEBUG
            if (RenderOverlayBorders)
            {
                var borderOverlay = new OverlayData(overlayData.CameraInfo) { Color = Colors.Green };
                foreach (var border in BorderLines)
                    border.Render(borderOverlay);
            }
            if (RenderOverlayCentre)
                Center.RenderCircle(new OverlayData(overlayData.CameraInfo) { Color = Colors.Red });
#endif
        }
        private void SplitTrajectory(int depth, ITrajectory trajectory, float deltaAngle, float minAngle, float minLength, float maxLength, List<ITrajectory> result)
        {
            var length = trajectory.Magnitude;

            var needDivide = (deltaAngle > minAngle && length >= minLength) || length > maxLength;
            if (depth < 5 && (needDivide || depth == 0))
            {
                trajectory.Divide(out ITrajectory first, out ITrajectory second);
                var firstDeltaAngle = first.DeltaAngle;
                var secondDeltaAngle = second.DeltaAngle;

                if (needDivide || deltaAngle > minAngle || (firstDeltaAngle + secondDeltaAngle) > minAngle)
                {
                    SplitTrajectory(depth + 1, first, firstDeltaAngle, minAngle, minLength, maxLength, result);
                    SplitTrajectory(depth + 1, second, secondDeltaAngle, minAngle, minLength, maxLength, result);

                    return;
                }
            }

            result.Add(trajectory);
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
        private Bezier3 GetEndBezier(Vector3 leftPos, Vector3 leftDir, Vector3 rightPos, Vector3 rightDir, float shift = 0f)
        {
            var length = Mathf.Max(Id.GetNode().Info.m_netAI.GetEndRadius() - shift, 0f) / 0.75f;
            var bezier = new Bezier3()
            {
                a = leftPos,
                b = leftPos + leftDir * length,
                c = rightPos + rightDir * length,
                d = rightPos,
            };
            return bezier;
        }

        #endregion

        public struct Data
        {
            public ushort Id { get; }
            public float angle;

            public Vector3 leftPos;
            public Vector3 rightPos;

            private Vector3 _leftDir;
            private Vector3 _rightDir;

            private float _leftDirLength;
            private float _rightDirLength;

            public float halfWidth;

            public Vector3 LeftDir
            {
                get => _leftDir;
                set
                {
                    _leftDir = value.normalized;
                    _leftDirLength = LengthXZ(value);
                }
            }
            public Vector3 RightDir
            {
                get => _rightDir;
                set
                {
                    _rightDir = value.normalized;
                    _rightDirLength = LengthXZ(value);
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

                _leftDirLength = 1f;
                _rightDirLength = 1f;

                halfWidth = 0f;
            }

            public float DeltaAngleCos => Mathf.Clamp01((2 * halfWidth) / CornerLength);
            public Vector3 CornerDir => (rightPos - leftPos).normalized;
            public float CornerLength => LengthXZ(rightPos - leftPos);
            public StraightTrajectory Line => new StraightTrajectory(leftPos, rightPos);
            public Vector3 Position => (rightPos + leftPos) / 2;
            public Vector3 Direction => (_leftDir + _rightDir).normalized;

            public Vector3 GetPos(float t)
            {
                t = Mathf.Clamp01(t);
                return leftPos * t + rightPos * (1 - t);
            }
            public Vector3 GetDir(float t)
            {
                t = Mathf.Clamp01(t);
                return (_leftDir * t + _rightDir * (1 - t)).normalized;
            }
            public float GetDirLength(float t)
            {
                t = Mathf.Clamp01(t);
                return _leftDirLength * t + _rightDirLength * (1 - t);
            }
            public Vector3 GetCornerDelta(float width) => CornerDir * (width / DeltaAngleCos);
        }

        public class SelectionComparer : IEqualityComparer<Selection>
        {
            public bool Equals(Selection x, Selection y)
            {
                if (x == null)
                    return y == null;
                else
                    return x.Equals(y);
            }

            public int GetHashCode(Selection obj) => obj.Id;
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

        public override bool Equals(Selection other) => other is NodeSelection selection && selection.Id == Id;
        public override string ToString() => $"Node #{Id}";
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

        public override bool Equals(Selection other) => other is SegmentSelection selection && selection.Id == Id;
        public override string ToString() => $"Segment #{Id}";
    }
}
