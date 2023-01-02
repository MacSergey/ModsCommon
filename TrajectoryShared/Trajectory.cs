using ColossalFramework.Math;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ColossalFramework.Math.VectorUtils;
using static UnityEngine.ParticleSystem;

namespace ModsCommon.Utilities
{
    public enum TrajectoryType
    {
        Line,
        Bezier,
        Combined
    }
    public interface ITrajectory : IOverlay
    {
        TrajectoryType TrajectoryType { get; }
        float Length { get; }
        float Magnitude { get; }
        float DeltaAngle { get; }
        Vector3 Direction { get; }
        Vector3 StartDirection { get; }
        Vector3 EndDirection { get; }
        Vector3 StartPosition { get; }
        Vector3 EndPosition { get; }
        ITrajectory Cut(float t0, float t1);
        void Divide(out ITrajectory trajectory1, out ITrajectory trajectory2);
        Vector3 Tangent(float t);
        Vector3 Position(float t);
        float Travel(float distance);
        float Travel(float start, float distance);
        float Distance(float from = 0f, float to = 1f);
        ITrajectory Invert();

        public bool IsZero { get; }

        public Vector3 GetHitPosition(Segment3 ray, out float rayT, out float trajectoryT, out Vector3 position);

        public Vector3 GetClosestPosition(Vector3 hitPos, out float closestT);
        public Vector3 GetDirectionPosition(Vector3 hitPos, out float closestT);
        public void GetClosestPositionAndDirection(Vector3 hitPos, out Vector3 position, out Vector3 direction, out float closestT);
        public float GetLength(float minAngleDelta = 10, int depth = 5);
    }
    public struct BezierTrajectory : ITrajectory
    {
        public TrajectoryType TrajectoryType => TrajectoryType.Bezier;
        public Bezier3 Trajectory { get; }
        public bool? Smooth { get; private set; }

        private float? _length;
        public float Length => _length ??= Trajectory.Length();
        public float Magnitude { get; }
        public float DeltaAngle { get; }
        public Vector3 Direction { get; }
        public Vector3 StartDirection { get; }
        public Vector3 EndDirection { get; }
        public Vector3 StartPosition => Trajectory.a;
        public Vector3 EndPosition => Trajectory.d;
        public bool IsZero => Trajectory.Max() == Trajectory.Min();

        private BezierTrajectory(Bezier3 trajectory, float? length, float magnitude, float deltaAngle, Vector3 direction, Vector3 startDirection, Vector3 endDirection, bool? smooth)
        {
            Trajectory = trajectory;
            _length = length;

            Magnitude = magnitude;
            DeltaAngle = deltaAngle;
            Direction = direction;
            StartDirection = startDirection;
            EndDirection = endDirection;
            Smooth = smooth;
        }
        public BezierTrajectory(Bezier3 trajectory, bool? smooth = null)
        {
            Trajectory = trajectory;
            _length = null;

            Magnitude = (Trajectory.d - Trajectory.a).magnitude;
            DeltaAngle = Trajectory.DeltaAngle();
            Direction = (Trajectory.d - Trajectory.a).normalized;
            StartDirection = (Trajectory.b - Trajectory.a).normalized;
            EndDirection = (Trajectory.c - Trajectory.d).normalized;
            Smooth = smooth;
        }
        public BezierTrajectory(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir, bool normalize = true, bool? smooth = null) : this(GetBezier(startPos, startDir, endPos, endDir, normalize, smooth == true)) 
        {
            Smooth = smooth;
        }
        public BezierTrajectory(Vector3 startPos, Vector3 startDir, Vector3 endPos, bool? smooth = null) : this(GetBezier(startPos, startDir, endPos, smooth == true))
        {
            Smooth = smooth;
        }

        public BezierTrajectory(BezierTrajectory trajectory) : this(trajectory.Trajectory) { }
        public BezierTrajectory(ITrajectory trajectory) : this(trajectory.StartPosition, trajectory.StartDirection, trajectory.EndPosition, trajectory.EndDirection) { }
        public BezierTrajectory(ref NetSegment segment) : this(segment.m_startNode.GetNode().m_position, segment.m_startDirection, segment.m_endNode.GetNode().m_position, segment.m_endDirection) { }
        public BezierTrajectory(ushort segmentId) : this(ref segmentId.GetSegment()) { }

        private static Bezier3 GetBezier(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir, bool normalize, bool smooth)
        {
            var bezier = new Bezier3()
            {
                a = startPos,
                d = endPos,
            };
            GetMiddlePoints(bezier.a, normalize ? startDir.normalized : startDir, bezier.d, normalize ? endDir.normalized : endDir, smooth, out bezier.b, out bezier.c);
            return bezier;
        }
        private static Bezier3 GetBezier(Vector3 startPos, Vector3 startDir, Vector3 endPos, bool smooth)
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
                GetMiddlePoints(bezier.a, dir, bezier.d, -dir, smooth, out bezier.b, out bezier.c);
            else
                GetMiddlePoints(bezier.a, startDir, bezier.d, endAngle.Direction(), smooth, out bezier.b, out bezier.c);

            return bezier;
        }
        private static void GetMiddlePoints(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir, bool smooth, out Vector3 middlePos1, out Vector3 middlePos2)
        {
            if (NetSegment.IsStraight(startPos, startDir, endPos, endDir, out var distance))
            {
                middlePos1 = startPos + startDir * (distance * 0.3f);
                middlePos2 = endPos + endDir * (distance * 0.3f);
                return;
            }
            var dot = startDir.x * endDir.x + startDir.z * endDir.z;
            if (dot >= -0.999f && Line2.Intersect(XZ(startPos), XZ(startPos + startDir), XZ(endPos), XZ(endPos + endDir), out var u, out var v))
            {
                if (smooth)
                {
                    u = Mathf.Abs(u);
                    v = Mathf.Abs(v);
                }
                u = Mathf.Clamp(u, distance * 0.1f, distance);
                v = Mathf.Clamp(v, distance * 0.1f, distance);
                distance = u + v;
                middlePos1 = startPos + startDir * Mathf.Min(u, distance * 0.3f);
                middlePos2 = endPos + endDir * Mathf.Min(v, distance * 0.3f);
            }
            else
            {
                //distance *= smooth ? 0.75f : 0.3f;
                distance *= 0.3f;
                middlePos1 = startPos + startDir * distance;
                middlePos2 = endPos + endDir * distance;
            }
        }

        public BezierTrajectory Cut(float t0, float t1) => new BezierTrajectory(Trajectory.Cut(t0, t1), Smooth);
        ITrajectory ITrajectory.Cut(float t0, float t1) => Cut(t0, t1);
        public void Divide(out ITrajectory trajectory1, out ITrajectory trajectory2)
        {
            Trajectory.Divide(out Bezier3 bezier1, out Bezier3 bezier2);
            trajectory1 = new BezierTrajectory(bezier1, Smooth);
            trajectory2 = new BezierTrajectory(bezier2, Smooth);
        }
        public Vector3 Tangent(float t) => Trajectory.Tangent(t);
        public Vector3 Position(float t) => Trajectory.Position(t);
        public float Travel(float distance) => Trajectory.Travel(distance);
        public float Travel(float start, float distance) => start + Trajectory.Cut(start, 1f).Travel(distance) * (1f - start);
        public float Distance(float from = 0f, float to = 1f) => Trajectory.Cut(from, to).Length();
        public BezierTrajectory Invert() => new BezierTrajectory(Trajectory.Invert(), _length, Magnitude, DeltaAngle, -Direction, EndDirection, StartDirection, Smooth);

            ITrajectory ITrajectory.Invert() => Invert();
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
        public static explicit operator BezierTrajectory(Bezier3 bezier) => new BezierTrajectory(bezier, null);

        public override bool Equals(object obj) => obj is BezierTrajectory trajectory && Equals(trajectory);
        public bool Equals(BezierTrajectory other) => Trajectory.a == other.Trajectory.a && Trajectory.b == other.Trajectory.b && Trajectory.c == other.Trajectory.c && Trajectory.d == other.Trajectory.d;

        public static bool operator ==(BezierTrajectory first, BezierTrajectory second) => first.Equals(second);
        public static bool operator !=(BezierTrajectory first, BezierTrajectory second) => !first.Equals(second);

        public override int GetHashCode() => Trajectory.GetHashCode();
    }
    public struct StraightTrajectory : ITrajectory
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
    public struct CombinedTrajectory : ITrajectory, IEnumerable<ITrajectory>
    {
        public TrajectoryType TrajectoryType => TrajectoryType.Combined;
        private ITrajectory[] Trajectories { get; }

        private float? _length;
        private float[] _parts;

        public int Count => Trajectories.Length;
        public float Length => _length ??= Trajectories.Sum(t => t.Length);
        public float[] Parts
        {
            get
            {
                var parts = _parts;

                if (parts == null)
                {
                    var totalLength = Length;
                    parts = new float[Trajectories.Length];

                    var sum = 0f;
                    for (var i = 0; i < parts.Length; i += 1)
                    {
                        parts[i] = sum;
                        sum += Trajectories[i].Length / totalLength;
                    }

                    _parts = parts;
                }

                return parts;
            }
        }
        public ITrajectory this[int i] => Trajectories[i];

        public float Magnitude { get; }
        public float DeltaAngle { get; }
        public Vector3 Direction { get; }
        public Vector3 StartDirection { get; }
        public Vector3 EndDirection { get; }
        public Vector3 StartPosition => Trajectories.First().StartPosition;
        public Vector3 EndPosition => Trajectories.Last().EndPosition;
        public bool IsZero => Trajectories.All(t => t.IsZero);

        private CombinedTrajectory(ITrajectory[] trajectories, float? length, float[] parts, float magnitude, float deltaAngle, Vector3 direction, Vector3 startDirection, Vector3 endDirection)
        {
            Trajectories = trajectories;

            _length = length;
            _parts = parts;

            Magnitude = magnitude;
            DeltaAngle = deltaAngle;
            Direction = direction;
            StartDirection = startDirection;
            EndDirection = endDirection;
        }
        public CombinedTrajectory(params ITrajectory[] trajectories)
        {
            Trajectories = trajectories;

            if (Trajectories.Length == 0)
                throw new ArgumentException("Trajectories are empty", nameof(trajectories));

            for (int i = 1; i < Trajectories.Length; i += 1)
            {
                if (Trajectories[i - 1].EndPosition != Trajectories[i].StartPosition)
                    throw new ArgumentException($"Trajectories should connect each other. trajectories {i - 1} and {i} are not connected", nameof(trajectories));
            }

            _length = null;
            _parts = null;

            var first = Trajectories[0];
            var last = Trajectories[Trajectories.Length - 1];
            Magnitude = (last.EndPosition - first.StartPosition).magnitude;
            DeltaAngle = 180 - Vector3.Angle(first.StartDirection, last.EndDirection);
            Direction = (last.EndPosition - first.StartPosition).normalized;
            StartDirection = first.StartDirection;
            EndDirection = last.EndDirection;
        }
        public CombinedTrajectory(IEnumerable<ITrajectory> trajectories) : this(trajectories.ToArray()) { }

        public IEnumerator<ITrajectory> GetEnumerator() => ((IEnumerable<ITrajectory>)Trajectories).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Trajectories.GetEnumerator();

        private int GetIndex(float t)
        {
            var parts = Parts;
            for (var i = Count - 1; i >= 0; i -= 1)
            {
                if (t >= parts[i])
                    return i;
            }

            return 0;
        }
        public void GetBounds(int i, out float t0, out float t1)
        {
            t0 = Parts[i];
            t1 = i < Count - 1 ? Parts[i + 1] : 1f;
        }
        public float GetRelativeLength(int i) => Trajectories[i].Length / Length;

        public float ToPartT(float t, out int i)
        {
            i = GetIndex(t);
            return ToPartT(i, t);
        }
        public float ToPartT(int i, float t)
        {
            t = Mathf.Clamp01(t - Parts[i]) / GetRelativeLength(i);
            return t;
        }
        public float FromPartT(int i, float t)
        {
            t = Mathf.Clamp01(t * GetRelativeLength(i) + Parts[i]);
            return t;
        }

        public ITrajectory Cut(float t0, float t1)
        {
            t0 = ToPartT(t0, out var startI);
            t1 = ToPartT(t1, out var endI);

            if (startI == endI)
                return Trajectories[startI].Cut(t0, t1);
            else
            {
                var trajectories = new List<ITrajectory>();

                trajectories.Add(Trajectories[startI].Cut(t0, 1f));

                for (var i = startI + 1; i < endI; i += 1)
                    trajectories.Add(Trajectories[i]);

                trajectories.Add(Trajectories[endI].Cut(0f, t1));

                return new CombinedTrajectory(trajectories);
            }
        }
        ITrajectory ITrajectory.Cut(float t0, float t1) => Cut(t0, t1);

        public float Distance(float from = 0, float to = 1)
        {
            from = ToPartT(from, out var startI);
            to = ToPartT(to, out var endI);

            if (startI == endI)
                return Trajectories[startI].Distance(from, to);
            else
            {
                var distance = 0f;

                distance += Trajectories[startI].Distance(from, 1f);

                for (var i = startI + 1; i < endI - 1; i += 1)
                    distance += Trajectories[i].Length;

                distance += Trajectories[endI].Distance(0f, to);

                return distance;
            }
        }

        public void Divide(out ITrajectory trajectory1, out ITrajectory trajectory2)
        {
            var firstHalf = new List<ITrajectory>();
            var secondHalf = new List<ITrajectory>();

            for (var i = 0; i < Count; i += 1)
            {
                GetBounds(i, out var t0, out var t1);

                if (t1 <= 0.5f)
                    firstHalf.Add(Trajectories[i]);
                else if (t0 >= 0.5f)
                    secondHalf.Add(Trajectories[i]);
                else
                {
                    var t = (0.5f - t0) / GetRelativeLength(i);
                    firstHalf.Add(Trajectories[i].Cut(0f, t));
                    secondHalf.Add(Trajectories[i].Cut(t, 1f));
                }
            }

            trajectory1 = firstHalf.Count == 1 ? firstHalf[0] : new CombinedTrajectory(firstHalf);
            trajectory2 = secondHalf.Count == 1 ? secondHalf[0] : new CombinedTrajectory(secondHalf);
        }

        public CombinedTrajectory Invert() => new CombinedTrajectory(Trajectories.Select(t => t.Invert()).Reverse().ToArray(), _length, _parts == null ? null : _parts.Reverse().ToArray(), Magnitude, DeltaAngle, -Direction, EndDirection, StartDirection);
        ITrajectory ITrajectory.Invert() => Invert();

        public Vector3 Position(float t)
        {
            t = ToPartT(t, out var i);
            return Trajectories[i].Position(t);
        }

        public Vector3 Tangent(float t)
        {
            t = ToPartT(t, out var i);
            return Trajectories[i].Tangent(t);
        }

        public float Travel(float distance) => Travel(0f, distance);
        public float Travel(float start, float distance)
        {
            for (var i = 0; i < Count; i += 1)
            {
                var lenght = Trajectories[i].Length;

                if (lenght <= distance)
                    distance -= lenght;
                else
                {
                    var t = Trajectories[i].Travel(distance);
                    t = FromPartT(i, t);
                    return t;
                }
            }

            return 1f;
        }
        public Vector3 GetHitPosition(Segment3 ray, out float rayT, out float trajectoryT, out Vector3 position)
        {
            rayT = 1f;
            trajectoryT = 0f;
            position = default;
            var result = default(Vector3);
            var delta = float.MaxValue;

            for (var i = 0; i < Count; i += 1)
            {
                var thisResult = Trajectories[i].GetHitPosition(ray, out var thisRayT, out var thisTrajectoryT, out var thisPosition);
                var thisDelta = (thisResult - thisPosition).magnitude;
                if (thisDelta < delta)
                {
                    delta = thisDelta;
                    result = thisResult;
                    rayT = thisRayT;
                    trajectoryT = FromPartT(i, thisTrajectoryT);
                    position = thisPosition;
                }
            }

            return result;
        }
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
        public void GetClosestPositionAndDirection(Vector3 hitPos, out Vector3 position, out Vector3 direction, out float closestT) => TrajectoryHelper.ClosestPositionAndDirection(this, hitPos, out position, out direction, out closestT);

        public float GetLength(float minAngleDelta, int depth) => Trajectories.Sum(t => t.GetLength(minAngleDelta, depth));

        public void Render(OverlayData data)
        {
            foreach (var trajectory in Trajectories)
                trajectory.Render(data);
        }

        public override bool Equals(object obj) => obj is CombinedTrajectory trajectory && Equals(trajectory);
        public bool Equals(CombinedTrajectory other)
        {
            if (Count != other.Count)
                return false;

            for(int i = 0; i < Count; i += 1)
            {
                if (!this[i].Equals(other[i]))
                    return false;
            }

            return true;
        }

        public static bool operator ==(CombinedTrajectory first, CombinedTrajectory second) => first.Equals(second);
        public static bool operator !=(CombinedTrajectory first, CombinedTrajectory second) => !first.Equals(second);
        public override int GetHashCode() => Trajectories.GetHashCode();

        public override string ToString() => string.Join("\n", Trajectories.Select(t => t.ToString()).ToArray());
    }
    public class TrajectoryBound : IOverlay
    {
        private static float Coef { get; } = Mathf.Sin(45 * Mathf.Deg2Rad);
        public ITrajectory Trajectory { get; }
        public float Size { get; }
        private List<Bounds> BoundsList { get; } = new List<Bounds>();
        public IEnumerable<Bounds> Bounds => BoundsList;
        public TrajectoryBound(ITrajectory trajectory, float size)
        {
            Trajectory = trajectory;
            Size = size;
            CalculateBounds();
        }

        private void CalculateBounds()
        {
            if (Trajectory == null)
                return;

            var size = Size * Coef;
            var t = 0f;
            while (t < 1f)
            {
                t = Trajectory.Travel(t, size / 2);
                BoundsList.Add(new Bounds(Trajectory.Position(t), Vector3.one * size));
            }
            BoundsList.Add(new Bounds(Trajectory.Position(0), Vector3.one * Size));
            BoundsList.Add(new Bounds(Trajectory.Position(1), Vector3.one * Size));
        }

        public bool IntersectRay(Ray ray) => BoundsList.Any(b => b.IntersectRay(ray));
        public bool Intersects(Bounds bounds) => BoundsList.Any(b => b.Intersects(bounds));

        public void Render(OverlayData data) => Trajectory.Render(data);
    }
    public struct TrajectoryPoints
    {
        ITrajectory[] Trajectories { get; }
        int[] Counts { get; }
        int[] EndIndixes { get; }
        Vector2?[] Points { get; }
        public int Length => Points.Length;

        public Vector3 this[int index]
        {
            get
            {
                if (Points[index] == null)
                {
                    var t = GetT(index);
                    var i = Math.Min((int)t, Trajectories.Length - 1);
                    Points[index] = XZ(Trajectories[i].Position(t - i));
                }
                return Points[index].Value;
            }
        }

        public TrajectoryPoints(params ITrajectory[] trajectories)
        {
            Trajectories = trajectories;
            Counts = new int[Trajectories.Length];
            EndIndixes = new int[Trajectories.Length];

            for (var i = 0; i < Trajectories.Length; i += 1)
            {
                var length = Trajectories[i].Length;
                Counts[i] = Math.Max((int)(Mathf.Clamp(length, 0f, 200f) * 20), 2);
                EndIndixes[i] = EndIndixes[Math.Max(i - 1, 0)] + Counts[i];
            }

            Points = new Vector2?[EndIndixes.Last()];
        }

        private float GetT(int index)
        {
            var i = 0;
            while (index > EndIndixes[i])
                i += 1;

            return 1f / Counts[i] * (Counts[i] - EndIndixes[i] + index) + i;
        }
        public float Find(int startIndex, float distance, out int findIndex)
        {
            distance *= distance;

            findIndex = startIndex;
            var endIndex = EndIndixes.Last() - 1;
            var position = this[findIndex];

            while (findIndex <= endIndex)
            {
                var currentIndex = findIndex + (endIndex - findIndex >> 1);
                var diffirent = (this[currentIndex] - position).sqrMagnitude - distance;

                if (diffirent < 0)
                    findIndex = currentIndex + 1;
                else
                    endIndex = currentIndex - 1;
            }
            return GetT(findIndex);
        }
    }

    public static class TrajectoryHelper
    {
        public static Direction GetDirection(this IEnumerable<ITrajectory> trajectories)
        {
            var isClockWise = 0;
            var contour = trajectories.ToArray();
            for (var i = 0; i < contour.Length; i += 1)
                isClockWise += (Vector3.Cross(-contour[i].Direction, contour[(i + 1) % contour.Length].Direction).y < 0) ? 1 : -1;

            return isClockWise >= 0 ? Direction.ClockWise : Direction.CounterClockWise;
        }
        public static IEnumerable<ITrajectory> GetTrajectories(this Rect rect, float height = 0f)
        {
            yield return new StraightTrajectory(new Vector3(rect.xMin, height, rect.yMin), new Vector3(rect.xMax, height, rect.yMin));
            yield return new StraightTrajectory(new Vector3(rect.xMin, height, rect.yMax), new Vector3(rect.xMax, height, rect.yMax));
            yield return new StraightTrajectory(new Vector3(rect.xMin, height, rect.yMin), new Vector3(rect.xMin, height, rect.yMax));
            yield return new StraightTrajectory(new Vector3(rect.xMax, height, rect.yMin), new Vector3(rect.xMax, height, rect.yMax));
        }
        public static Rect GetRect(this IEnumerable<ITrajectory> contour)
        {
            var firstPos = contour.FirstOrDefault(t => t != null)?.StartPosition ?? default;
            var rect = Rect.MinMaxRect(firstPos.x, firstPos.z, firstPos.x, firstPos.z);

            foreach (var trajectory in contour)
            {
                switch (trajectory)
                {
                    case BezierTrajectory bezierTrajectory:
                        Set(bezierTrajectory.Trajectory.a);
                        Set(bezierTrajectory.Trajectory.b);
                        Set(bezierTrajectory.Trajectory.c);
                        Set(bezierTrajectory.Trajectory.d);
                        break;
                    case StraightTrajectory straightTrajectory:
                        Set(straightTrajectory.Trajectory.a);
                        Set(straightTrajectory.Trajectory.b);
                        break;
                }
            }

            return rect;

            void Set(Vector3 pos)
            {
                if (pos.x < rect.xMin)
                    rect.xMin = pos.x;
                else if (pos.x > rect.xMax)
                    rect.xMax = pos.x;

                if (pos.z < rect.yMin)
                    rect.yMin = pos.z;
                else if (pos.z > rect.yMax)
                    rect.yMax = pos.z;
            }
        }
        public static Vector3 ClosestPosition(this ITrajectory trajectory, Vector3 point)
        {
            ClosestPositionAndDirection(trajectory, point, out var position, out _, out _);
            return position;
        }
        public static Vector3 ClosestDirection(this ITrajectory trajectory, Vector3 point)
        {
            ClosestPositionAndDirection(trajectory, point, out _, out var direction, out _);
            return direction;
        }
        public static void ClosestPositionAndDirection(this ITrajectory trajectory, Vector3 point, out Vector3 position, out Vector3 direction, out float t)
        {
            var distance = 1E+11f;
            t = 0f;
            var prevPosition = trajectory.StartPosition;
            for (var i = 1; i <= 16; i += 1)
            {
                var currentPosition = trajectory.Position(i / 16f);
                var currentDistance = Segment3.DistanceSqr(prevPosition, currentPosition, point, out var u);
                if (currentDistance < distance)
                {
                    distance = currentDistance;
                    t = (i - 1f + u) / 16f;
                }
                prevPosition = currentPosition;
            }

            float delta = 0.03125f;
            for (var i = 0; i < 4; i += 1)
            {
                var minPosition = trajectory.Position(Mathf.Max(0f, t - delta));
                var currentPosition = trajectory.Position(t);
                var maxPosition = trajectory.Position(Mathf.Min(1f, t + delta));

                var minDistance = Segment3.DistanceSqr(minPosition, currentPosition, point, out var minU);
                var maxDistance = Segment3.DistanceSqr(currentPosition, maxPosition, point, out var maxU);

                t = minDistance >= maxDistance ? Mathf.Min(1f, t + delta * maxU) : Mathf.Max(0f, t - delta * (1f - minU));
                delta *= 0.5f;
            }

            position = trajectory.Position(t);
            direction = NormalizeXZ(trajectory.Tangent(t));
        }

        public enum Direction
        {
            ClockWise,
            CounterClockWise
        }
    }
}
