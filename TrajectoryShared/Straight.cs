using ColossalFramework.Math;
using UnityEngine;

namespace ModsCommon.Utilities
{
    public readonly struct StraightTrajectory : ITrajectory
    {
        public TrajectoryType TrajectoryType => TrajectoryType.Line;
        public Line3 Trajectory { get; }
        public bool StartLimited { get; }
        public bool EndLimited { get; }
        public bool IsSection => StartLimited && EndLimited;
        public float Length { get; }
        public float Magnitude => Length;
        public float DeltaAngle => 0f;
        public Vector3 Direction { get; }
        public Vector3 StartDirection => Direction;
        public Vector3 EndDirection => -Direction;
        public Vector3 StartPosition => Trajectory.a;
        public Vector3 EndPosition => Trajectory.b;
        public bool IsZero => Trajectory.a == Trajectory.b;
#if DEBUG
        public string Table => $"{Trajectory.a.x};{Trajectory.a.z}\n{Trajectory.b.x};{Trajectory.b.z}";
#endif

        private StraightTrajectory(Line3 trajectory, bool startLimited, bool endLimited, float length, Vector3 direction)
        {
            Trajectory = trajectory;
            StartLimited = startLimited;
            EndLimited = endLimited;

            Length = length;
            Direction = direction;
        }
        public StraightTrajectory(Line3 trajectory, bool startLimited, bool endLimited)
        {
            Trajectory = trajectory;
            StartLimited = startLimited;
            EndLimited = endLimited;

            Length = (Trajectory.b - Trajectory.a).magnitude;
            Direction = (Trajectory.b - Trajectory.a).normalized;
        }
        public StraightTrajectory(Line3 trajectory, bool isSection = true) : this(trajectory, isSection, isSection) { }

        public StraightTrajectory(Vector3 start, Vector3 end, bool startLimited, bool endLimited) : this(new Line3(start, end), startLimited, endLimited) { }
        public StraightTrajectory(Vector3 start, Vector3 end, bool isSection = true) : this(new Line3(start, end), isSection, isSection) { }

        public StraightTrajectory(ITrajectory trajectory, bool startLimited, bool endLimited) : this(new Line3(trajectory.StartPosition, trajectory.EndPosition), startLimited, endLimited) { }
        public StraightTrajectory(ITrajectory trajectory, bool isSection = true) : this(new Line3(trajectory.StartPosition, trajectory.EndPosition), isSection, isSection) { }

        public StraightTrajectory Cut(float t0, float t1) => new StraightTrajectory(Position(t0), Position(t1));
        ITrajectory ITrajectory.Cut(float t0, float t1) => Cut(t0, t1);
        public StraightTrajectory Cut(float t0, float t1, bool isSection) => new StraightTrajectory(Position(t0), Position(t1), isSection);

        public void Divide(out ITrajectory trajectory1, out ITrajectory trajectory2)
        {
            var middle = (Trajectory.a + Trajectory.b) / 2;
            trajectory1 = new StraightTrajectory(Trajectory.a, middle);
            trajectory2 = new StraightTrajectory(middle, Trajectory.b);
        }
        public Vector3 Tangent(float t) => Direction;
        public Vector3 Position(float t) => Trajectory.a + (Trajectory.b - Trajectory.a) * t;
        public float Travel(float distance) => distance / Length;
        public float Travel(float start, float distance) => start + Travel(distance);
        public float Distance(float from = 0f, float to = 1f) => Length * (to - from);
        public StraightTrajectory Invert() => new StraightTrajectory(new Line3(Trajectory.b, Trajectory.a), EndLimited, StartLimited, Length, -Direction);
        ITrajectory ITrajectory.Invert() => Invert();
        public StraightTrajectory Shift(float start, float end)
        {
            var startNormal = StartDirection.MakeFlatNormalized().Turn90(true);
            var endNormal = EndDirection.MakeFlatNormalized().Turn90(false);
            return new StraightTrajectory(StartPosition + startNormal * start, EndPosition + endNormal * end);
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
        public float GetLength(float minAngleDelta, int depth) => Length;

        public void Render(OverlayData data) => Trajectory.GetBezier().RenderBezier(data);
        public override string ToString() => $"Straight: {StartPosition} - {EndPosition}";

        public static implicit operator Line3(StraightTrajectory trajectory) => trajectory.Trajectory;
        public static explicit operator StraightTrajectory(Line3 line) => new StraightTrajectory(line);

        public override bool Equals(object obj) => obj is StraightTrajectory trajectory && Equals(trajectory);
        public bool Equals(StraightTrajectory other) => Trajectory.a == other.Trajectory.a && Trajectory.b == other.Trajectory.b;

        public static bool operator ==(StraightTrajectory first, StraightTrajectory second) => first.Equals(second);
        public static bool operator !=(StraightTrajectory first, StraightTrajectory second) => !first.Equals(second);

        public override int GetHashCode() => Trajectory.GetHashCode();
    }
}
