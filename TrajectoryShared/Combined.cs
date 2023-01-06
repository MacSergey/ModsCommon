using ColossalFramework.Math;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ColossalFramework.Math.VectorUtils;

namespace ModsCommon.Utilities
{
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
                if ((Trajectories[i - 1].EndPosition - Trajectories[i].StartPosition).magnitude > 0.01f)
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
            var minT = ToPartT(Mathf.Min(t0, t1), out var startI);
            var maxT = ToPartT(Mathf.Max(t0, t1), out var endI);

            if (startI == endI)
            {
                if (t0 <= t1)
                    return Trajectories[startI].Cut(minT, maxT);
                else
                    return Trajectories[startI].Cut(maxT, minT);
            }
            else
            {
                var trajectories = new List<ITrajectory>();

                if (t0 <= t1)
                {
                    var start = Trajectories[startI].Cut(minT, 1f);
                    //if (start.Magnitude >= 0.01f)
                        trajectories.Add(start);

                    for (var i = startI + 1; i < endI; i += 1)
                    {
                        //if (Trajectories[i].Magnitude >= 0.01f)
                            trajectories.Add(Trajectories[i]);
                    }

                    var end = Trajectories[endI].Cut(0f, maxT);
                    //if (end.Magnitude >= 0.01f)
                        trajectories.Add(end);

                    if(trajectories.Count == 0)
                        trajectories.Add(start);
                }
                else
                {
                    var start = Trajectories[endI].Cut(maxT, 0f);
                    //if (start.Magnitude >= 0.01f)
                        trajectories.Add(start);

                    for (var i = endI - 1; i > startI; i -= 1)
                    {
                        //if (Trajectories[i].Magnitude >= 0.01f)
                            trajectories.Add(Trajectories[i].Invert());
                    }

                    var end = Trajectories[startI].Cut(1f, minT);
                    //if (end.Magnitude >= 0.01f)
                        trajectories.Add(end);

                    if (trajectories.Count == 0)
                        trajectories.Add(start);
                }

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
        public CombinedTrajectory Shift(float start, float end)
        {
            var trajectories = new List<ITrajectory>();
            var parts = Parts;
            for(var i = 0; i < parts.Length; i += 1)
            {
                var startI = Mathf.Lerp(start, end, parts[i]);
                var endI = Mathf.Lerp(start, end, i + 1 < parts.Length ? parts[i + 1] : 1f);
                trajectories.Add(Trajectories[i].Shift(startI, endI));
            }
            return new CombinedTrajectory(trajectories);
        }
        ITrajectory ITrajectory.Shift(float start, float end) => Shift(start, end);

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

            for (int i = 0; i < Count; i += 1)
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
}
