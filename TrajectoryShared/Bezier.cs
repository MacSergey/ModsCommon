using ColossalFramework.Math;
using UnityEngine;
using static ColossalFramework.Math.VectorUtils;

namespace ModsCommon.Utilities
{
    public struct BezierTrajectory : ITrajectory
    {
        public const float curveT = 0.3f;
        public const float straightT = 0.15f;

        public TrajectoryType TrajectoryType => TrajectoryType.Bezier;
        public Bezier3 Trajectory { get; }
        public float? StartT { get; }
        public float? EndT { get; }

        private float? _length;
        public float Length => _length ??= (Magnitude <= 0.01f ? 0.01f : Trajectory.Length());
        public float Magnitude { get; }
        public float DeltaAngle { get; }
        public Vector3 Direction { get; }
        public Vector3 StartDirection { get; }
        public Vector3 EndDirection { get; }
        public Vector3 StartPosition => Trajectory.a;
        public Vector3 EndPosition => Trajectory.d;
        public bool IsZero => Trajectory.Max() == Trajectory.Min();
#if DEBUG
        public string Table => $"{Trajectory.a.x};{Trajectory.a.z}\n{Trajectory.b.x};{Trajectory.b.z}\n{Trajectory.c.x};{Trajectory.c.z}\n{Trajectory.d.x};{Trajectory.d.z}";
#endif

        private BezierTrajectory(Bezier3 trajectory, float? length, float magnitude, float deltaAngle, Vector3 direction, Vector3 startDirection, Vector3 endDirection, float? startT, float? endT)
        {
            Trajectory = trajectory;
            _length = length;

            Magnitude = magnitude;
            DeltaAngle = deltaAngle;
            Direction = direction;
            StartDirection = startDirection;
            EndDirection = endDirection;
            StartT = startT;
            EndT = endT;
        }
        public BezierTrajectory(Bezier3 trajectory, float? startT = null, float? endT = null)
        {
            Trajectory = trajectory;
            _length = null;

            Magnitude = (Trajectory.d - Trajectory.a).magnitude;
            DeltaAngle = Trajectory.DeltaAngle();
            Direction = (Trajectory.d - Trajectory.a).normalized;
            StartDirection = (Trajectory.b - Trajectory.a).normalized;
            EndDirection = (Trajectory.c - Trajectory.d).normalized;
            StartT = startT;
            EndT = endT;
        }
        public BezierTrajectory(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir, bool normalize, bool smoothStart, bool smoothEnd)
        {
            var startStraightT = smoothStart ? curveT : straightT;
            var endStraightT = smoothEnd ? curveT : straightT;
            var startCurveT = curveT;
            var endCurveT = curveT;
            if (normalize)
            {
                startDir = startDir.normalized;
                endDir = endDir.normalized;
            }
            var bezier = GetBezier(startPos, startDir, endPos, endDir, startStraightT, endStraightT, startCurveT, endCurveT, out var startT, out var endT);
            this = new BezierTrajectory(bezier, startT, endT);
        }
        public BezierTrajectory(Vector3 startPos, Vector3 startDir, Vector3 endPos, bool smoothStart, bool smoothEnd)
        {
            var startStraightT = smoothStart ? curveT : straightT;
            var endStraightT = smoothEnd ? curveT : straightT;
            var bezier = GetBezier(startPos, startDir, endPos, startStraightT, endStraightT, curveT, curveT, out var startT, out var endT);
            this = new BezierTrajectory(bezier, startT, endT);
        }
        public BezierTrajectory(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir, float? startT = curveT, float? endT = curveT, bool normalize = true)
        {
            if (normalize)
            {
                startDir = startDir.normalized;
                endDir = endDir.normalized;
            }
            var bezier = GetBezier(startPos, startDir, endPos, endDir, startT ?? straightT, endT ?? straightT, startT ?? curveT, endT ?? curveT, out var startResultT, out var endResultT);
            this = new BezierTrajectory(bezier, startResultT, endResultT);
        }
        public BezierTrajectory(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir, float? startStraightT, float? endStraightT, float? startCurveT, float? endCurveT, bool normalize = true)
        {
            if (normalize)
            {
                startDir = startDir.normalized;
                endDir = endDir.normalized;
            }
            var bezier = GetBezier(startPos, startDir, endPos, endDir, startStraightT ?? straightT, endStraightT ?? straightT, startCurveT ?? curveT, endCurveT ?? curveT, out var startT, out var endT);
            this = new BezierTrajectory(bezier, startT, endT);
        }
        public BezierTrajectory(Vector3 startPos, Vector3 startDir, Vector3 endPos, float? startStraightT = straightT, float? endStraightT = straightT, float? startCurveT = curveT, float? endCurveT = curveT)
        {
            var bezier = GetBezier(startPos, startDir, endPos, startStraightT ?? straightT, endStraightT ?? straightT, startCurveT ?? curveT, endCurveT ?? curveT, out var startT, out var endT);
            this = new BezierTrajectory(bezier, startT, endT);
        }

        public BezierTrajectory(BezierTrajectory trajectory)
        {
            this = new BezierTrajectory(trajectory.Trajectory, trajectory.StartT, trajectory.EndT);
        }
        public BezierTrajectory(ITrajectory trajectory)
        {
            this = new BezierTrajectory(trajectory.StartPosition, trajectory.StartDirection, trajectory.EndPosition, trajectory.EndDirection, true, true, true);
        }
        public BezierTrajectory(ref NetSegment segment)
        {
            this = new BezierTrajectory(segment.m_startNode.GetNode().m_position, segment.m_startDirection, segment.m_endNode.GetNode().m_position, segment.m_endDirection, true, true, true);
        }
        public BezierTrajectory(ushort segmentId) : this(ref segmentId.GetSegment()) { }

        public static Bezier3 GetBezier(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir, float startStraightT, float endStraightT, float startCurveT, float endCurveT, out float startT, out float endT)
        {
            var bezier = new Bezier3()
            {
                a = startPos,
                d = endPos,
            };

            GetMiddlePoints(bezier.a, startDir, bezier.d, endDir, startStraightT, endStraightT, startCurveT, endCurveT, out bezier.b, out bezier.c, out startT, out endT);
            return bezier;
        }
        public static Bezier3 GetBezier(Vector3 startPos, Vector3 startDir, Vector3 endPos, float startStraightT, float endStraightT, float startCurveT, float endCurveT, out float startT, out float endT)
        {
            var startAngle = startDir.AbsoluteAngle();
            var dir = endPos - startPos;
            var strangeAngle = dir.AbsoluteAngle();
            var endAngle = strangeAngle + Mathf.PI + (strangeAngle - startAngle);

            var bezier = new Bezier3()
            {
                a = startPos,
                d = endPos,
            };

            if (Vector3.Dot(startDir, dir) < 0)
                GetMiddlePoints(bezier.a, dir, bezier.d, -dir, startStraightT, endStraightT, startCurveT, endCurveT, out bezier.b, out bezier.c, out startT, out endT);
            else
                GetMiddlePoints(bezier.a, startDir, bezier.d, endAngle.Direction(), startStraightT, endStraightT, startCurveT, endCurveT, out bezier.b, out bezier.c, out startT, out endT);

            return bezier;
        }
        public static void GetMiddlePoints(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir, float startStraightT, float endStraightT, float startCurveT, float endCurveT, out Vector3 middlePos1, out Vector3 middlePos2, out float startT, out float endT)
        {
            GetMiddleDistance(startPos, startDir, endPos, endDir, startStraightT, endStraightT, startCurveT, endCurveT, out var startDis, out var endDis, out startT, out endT);
            middlePos1 = startPos + startDir * startDis;
            middlePos2 = endPos + endDir * endDis;
        }
        public static void GetMiddleDistance(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir, float startStraightT, float endStraightT, float startCurveT, float endCurveT, out float startDis, out float endDis, out float startT, out float endT)
        {
            if (NetSegment.IsStraight(startPos, startDir, endPos, endDir, out var distance))
            {
                startT = startStraightT;
                endT = endStraightT;
                startDis = distance * startT;
                endDis = distance * endT;
            }
            else
            {
                startT = startCurveT;
                endT = endCurveT;
                var dot = startDir.x * endDir.x + startDir.z * endDir.z;
                if (dot >= -0.999f && Line2.Intersect(XZ(startPos), XZ(startPos + startDir), XZ(endPos), XZ(endPos + endDir), out var u, out var v))
                {
                    //if (smooth)
                    //{
                    //    u = Mathf.Abs(u);
                    //    v = Mathf.Abs(v);
                    //}
                    u = Mathf.Clamp(u, distance * 0.1f, distance);
                    v = Mathf.Clamp(v, distance * 0.1f, distance);
                    distance = u + v;
                    startDis = Mathf.Min(u, distance * startT);
                    endDis = Mathf.Min(v, distance * endT);

                }
                else
                {
                    startDis = distance * startT;
                    endDis = distance * endT;
                }
            }
        }

        public BezierTrajectory Cut(float t0, float t1) => new BezierTrajectory(Trajectory.Cut(t0, t1), StartT, EndT);
        ITrajectory ITrajectory.Cut(float t0, float t1) => Cut(t0, t1);
        public void Divide(out ITrajectory trajectory1, out ITrajectory trajectory2)
        {
            Trajectory.Divide(out Bezier3 bezier1, out Bezier3 bezier2);
            trajectory1 = new BezierTrajectory(bezier1, StartT, EndT);
            trajectory2 = new BezierTrajectory(bezier2, StartT, EndT);
        }
        public Vector3 Tangent(float t) => Trajectory.Tangent(t);
        public Vector3 Position(float t) => Trajectory.Position(t);
        public float Travel(float distance) => Trajectory.Travel(distance);
        public float Travel(float start, float distance) => start + Trajectory.Cut(start, 1f).Travel(distance) * (1f - start);
        public float Distance(float from = 0f, float to = 1f) => Trajectory.Cut(from, to).Length();
        public BezierTrajectory Invert() => new BezierTrajectory(Trajectory.Invert(), _length, Magnitude, DeltaAngle, -Direction, EndDirection, StartDirection, EndT, StartT);
        ITrajectory ITrajectory.Invert() => Invert();
        public BezierTrajectory Shift(float start, float end)
        {
            var startNormal = StartDirection.MakeFlatNormalized().Turn90(true);
            var endNormal = EndDirection.MakeFlatNormalized().Turn90(false);
            return new BezierTrajectory(StartPosition + startNormal * start, StartDirection, EndPosition + endNormal * end, EndDirection, StartT, EndT);
        }
        ITrajectory ITrajectory.Shift(float start, float end) => Shift(start, end);

        public Vector3 GetHitPosition(Segment3 ray, out float rayT, out float trajectoryT, out Vector3 position) => Trajectory.GetHitPosition(ray, out rayT, out trajectoryT, out position);
        public Vector3 GetClosestPosition(Vector3 hitPos, out float closestT)
        {
            GetClosestPositionAndDirection(hitPos, out var position, out _, out closestT);
            return position;
        }
        public Vector3 GetDirectionPosition(Vector3 hitPos, out float closestT)
        {
            GetClosestPositionAndDirection(hitPos, out _, out var direction, out closestT);
            return direction;
        }
        public void GetClosestPositionAndDirection(Vector3 hitPos, out Vector3 position, out Vector3 direction, out float closestT) => Trajectory.ClosestPositionAndDirection(hitPos, out position, out direction, out closestT);
        public float GetLength(float minAngleDelta, int depth) => Trajectory.Length(minAngleDelta, depth);

        public void Render(OverlayData data) => Trajectory.RenderBezier(data);
        public override string ToString() => $"Bezier: {StartPosition} - {EndPosition}";


        public static implicit operator Bezier3(BezierTrajectory trajectory) => trajectory.Trajectory;
        public static explicit operator BezierTrajectory(Bezier3 bezier) => new BezierTrajectory(bezier, null, null);

        public override bool Equals(object obj) => obj is BezierTrajectory trajectory && Equals(trajectory);
        public bool Equals(BezierTrajectory other) => Trajectory.a == other.Trajectory.a && Trajectory.b == other.Trajectory.b && Trajectory.c == other.Trajectory.c && Trajectory.d == other.Trajectory.d;

        public static bool operator ==(BezierTrajectory first, BezierTrajectory second) => first.Equals(second);
        public static bool operator !=(BezierTrajectory first, BezierTrajectory second) => !first.Equals(second);

        public override int GetHashCode() => Trajectory.GetHashCode();
    }
}
