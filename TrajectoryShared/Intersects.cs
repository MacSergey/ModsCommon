using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ColossalFramework.Math.VectorUtils;

namespace ModsCommon.Utilities
{
    public readonly struct Intersection
    {
        public static float DeltaAngle = 5f;
        public static float MaxLength = 1f;
        public static float MinLength = 0.5f;
        public static Comparer FirstComparer { get; } = new Comparer(true);
        public static Comparer SecondComparer { get; } = new Comparer(false);
        public static Intersection NotIntersect => new Intersection(false, default, default);

        public readonly ITrajectory first;
        public readonly ITrajectory second;

        public readonly float firstT;
        public readonly float secondT;

        public readonly bool isIntersect;
        public readonly bool inverted;

        private Intersection(bool isIntersect, float firstT, float secondT, ITrajectory first = default, ITrajectory second = default, bool inverted = false)
        {
            this.isIntersect = isIntersect;
            this.firstT = firstT;
            this.secondT = secondT;
            this.first = first;
            this.second = second;
            this.inverted = inverted;
        }
        public Intersection(float firstT, float secondT, ITrajectory first = default, ITrajectory second = default)
        {
            this.isIntersect = true;
            this.firstT = firstT;
            this.secondT = secondT;
            this.first = first;
            this.second = second;
            this.inverted = false;
        }

        public Intersection GetReverse() => !isIntersect ? this : new Intersection(isIntersect, secondT, firstT, second, first, inverted: !inverted);

        public static Intersection CalculateSingle(ITrajectory firstTrajectory, ITrajectory secondTrajectory)
        {
            var result = Calculate(firstTrajectory, secondTrajectory);
            return result.Count > 0 ? result[0] : NotIntersect;
        }
        public static bool CalculateSingle(ITrajectory firstTrajectory, ITrajectory secondTrajectory, out float firstT, out float secondT)
        {
            if (Calculate(firstTrajectory, secondTrajectory).FirstOrDefault() is Intersection intersection)
            {
                firstT = intersection.firstT;
                secondT = intersection.secondT;
                return true;
            }
            else
            {
                firstT = 0f;
                secondT = 0f;
                return false;
            }
        }
        public static bool Intersect(ITrajectory firstTrajectory, ITrajectory secondTrajectory) => Calculate(firstTrajectory, secondTrajectory).Any();

        public static List<Intersection> Calculate(ITrajectory firstTrajectory, ITrajectory secondTrajectory)
        {
            var result = new List<Intersection>();
            if (firstTrajectory.TrajectoryType == TrajectoryType.Line)
            {
                var firstStraight = (StraightTrajectory)firstTrajectory;

                if (secondTrajectory.TrajectoryType == TrajectoryType.Line)
                {
                    var secondStraight = (StraightTrajectory)secondTrajectory;
                    IntersectStraightWithStraight(result, in firstStraight, in secondStraight);
                }
                else
                    IntersectITrajectoryWithStraight(result, in firstStraight, secondTrajectory, false);
            }
            else
            {
                if (secondTrajectory.TrajectoryType == TrajectoryType.Line)
                {
                    var secondStraight = (StraightTrajectory)secondTrajectory;
                    IntersectITrajectoryWithStraight(result, in secondStraight, firstTrajectory, true);
                }
                else
                    IntersectITrajectoryWithITrajectory(result, firstTrajectory, secondTrajectory);
            }

            return result;
        }
        public static List<Intersection> Calculate(ITrajectory trajectory, IEnumerable<ITrajectory> otherTrajectories, bool onlyIntersect = false)
            => otherTrajectories.SelectMany(t => Calculate(trajectory, t)).Where(i => !onlyIntersect || i.isIntersect).ToList();

        #region STRAIGHT - STRAIGHT
        public static void IntersectStraightWithStraight(List<Intersection> results, in StraightTrajectory first, in StraightTrajectory second)
        {
            if (Line2.Intersect(XZ(first.StartPosition), XZ(first.EndPosition), XZ(second.StartPosition), XZ(second.EndPosition), out float firstT, out float secondT))
            {
                if (IsCorrectT(in first, firstT) && IsCorrectT(in second, secondT))
                {
                    var intersect = new Intersection(firstT, secondT, first, second);
                    results.Add(intersect);
                }
            }
        }
        public static Intersection GetIntersection(in StraightTrajectory first, in StraightTrajectory second)
        {
            if (Line2.Intersect(XZ(first.StartPosition), XZ(first.EndPosition), XZ(second.StartPosition), XZ(second.EndPosition), out float firstT, out float secondT))
            {
                if (IsCorrectT(in first, firstT) && IsCorrectT(second, secondT))
                    return new Intersection(firstT, secondT, first, second);
                else
                    return NotIntersect;
            }
            else
                return NotIntersect;
        }
        #endregion

        #region ITRAJECTORY - ITRAJECTORY
        private static bool IntersectITrajectoryWithITrajectory(List<Intersection> results, ITrajectory first, ITrajectory second, int firstIndex = 0, int firstTotal = 1, int secondIndex = 0, int secondTotal = 1)
        {
            var firstPoints = CalcTrajectoryParts(first, firstIndex, firstTotal, out int firstParts);
            var secondPoints = CalcTrajectoryParts(second, secondIndex, secondTotal, out int secondParts);

            if (firstParts == 1 && secondParts == 1)
            {
                IntersectSections(firstPoints[0].pos, firstPoints[1].pos, secondPoints[0].pos, secondPoints[1].pos, out float firstT, out float secondT);
                firstT = 1f / firstTotal * (firstIndex + firstT);
                secondT = 1f / secondTotal * (secondIndex + secondT);
                results.Add(new Intersection(firstT, secondT, first, second));
                return true;
            }

            for (var i = 0; i < firstParts; i += 1)
            {
                for (var j = 0; j < secondParts; j += 1)
                {
                    if (IntersectSections(firstPoints[i].pos, firstPoints[i + 1].pos, secondPoints[j].pos, secondPoints[j + 1].pos, out float firstT, out float secondT))
                    {
                        var needTryI = NeedTryParts(i, firstParts, firstT);
                        var needTryJ = NeedTryParts(j, secondParts, secondT);
                        for (int tryI = 0; tryI < needTryI.Length; tryI += 1)
                        {
                            var ii = needTryI[tryI];
                            for (int tryJ = 0; tryJ < needTryJ.Length; tryJ += 1)
                            {
                                var jj = needTryJ[tryJ];

                                if (IntersectITrajectoryWithITrajectory(results, first, second, firstIndex * firstParts + ii, firstTotal * firstParts, secondIndex * secondParts + jj, secondTotal * secondParts))
                                    return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
        #endregion

        #region ITRAJECTORY - STRAIGHT

        private static void IntersectITrajectoryWithStraight(List<Intersection> results, in StraightTrajectory line, ITrajectory trajectory, bool invert, int index = 0, int total = 1)
        {
            var points = CalcTrajectoryParts(trajectory, index, total, out int parts);

            if (parts > 1)
            {
                for (var i = 0; i < parts; i += 1)
                {
                    if (IntersectSectionAndRay(in line, points[i].pos, points[i + 1].pos, out _, out _))
                    {
                        IntersectITrajectoryWithStraight(results, in line, trajectory, invert, index * parts + i, total * parts);
                    }
                }
            }
            else if (parts == 1 && IntersectSectionAndRay(in line, points[0].pos, points[1].pos, out float firstT, out float secondT))
            {
                secondT = 1f / total * (index + secondT);
                if (!invert)
                {
                    var result = new Intersection(firstT, secondT, line, trajectory);
                    results.Add(result);
                }
                else
                {
                    var result = new Intersection(secondT, firstT, trajectory, line);
                    results.Add(result);
                }

            }
        }

        #endregion

        private static bool IntersectSections(Vector3 a, Vector3 b, Vector3 c, Vector3 d, out float p, out float q)
        {
            return Line2.Intersect(XZ(a), XZ(b), XZ(c), XZ(d), out p, out q) && IsCorrectT(p) && IsCorrectT(q);
        }
        private static int[] NeedTryParts(int i, int count, float p)
        {
            if (p < 0.1f && i != 0)
                return new int[] { i, i - 1 };
            else if (p > 0.9f && i + 1 < count)
                return new int[] { i, i + 1 };
            else
                return new int[] { i };
        }
        private static bool IntersectSectionAndRay(in StraightTrajectory line, Vector3 start, Vector3 end, out float p, out float q)
        {
            return Line2.Intersect(XZ(line.StartPosition), XZ(line.EndPosition), XZ(start), XZ(end), out p, out q) && IsCorrectT(in line, p) && IsCorrectT(q);
        }

        private static TrajectoryPoint[] CalcTrajectoryParts(ITrajectory trajectory, int index, int total, out int parts)
        {
            var startT = 1f / total * index;
            var endT = 1f / total * (index + 1);

            var start = trajectory.Position(startT);
            var end = trajectory.Position(endT);
            var middle = trajectory.Position((startT + endT) * 0.5f);

            var length = Mathf.Max((middle - start).magnitude + (end - middle).magnitude, 0f);
            parts = Math.Min((int)Math.Ceiling(length / MinLength), 10);

            var points = new TrajectoryPoint[parts + 1];
            points[0] = new TrajectoryPoint(start, startT);
            points[parts] = new TrajectoryPoint(end, endT);

            for (var i = 1; i < parts; i += 1)
            {
                var t = startT + 1f / (total * parts) * i;
                points[i] = new TrajectoryPoint(trajectory.Position(t), t);
            }

            return points;
        }
        public static bool IsCorrectT(float t) => 0f <= t && t <= 1f;
        public static bool IsCorrectT(in StraightTrajectory line, float t) => (line.StartLimited ? 0f : float.MinValue) <= t && t <= (line.EndLimited ? 1f : float.MaxValue);

        public static bool CanIntersect(Rect limits, in StraightTrajectory line, out Side side)
        {
            var dir = line.Direction.Turn90(true);
            var pos = line.StartPosition;

            side = GetSide(pos.x, pos.z, dir.x, dir.z, limits.xMin, limits.yMin);

            if (GetSide(pos.x, pos.z, dir.x, dir.z, limits.xMin, limits.yMax) != side)
                return true;
            else if (GetSide(pos.x, pos.z, dir.x, dir.z, limits.xMax, limits.yMin) != side)
                return true;
            else if (GetSide(pos.x, pos.z, dir.x, dir.z, limits.xMax, limits.yMax) != side)
                return true;
            else
                return false;
        }
        public static bool CanIntersect(Vector3[] points, in StraightTrajectory line, out Side side)
        {
            var dir = line.Direction.Turn90(true);
            var pos = line.StartPosition;

            if (points.Length == 0)
            {
                side = default;
                return false;
            }

            side = GetSide(pos.x, pos.z, dir.x, dir.z, points[0].x, points[0].z);
            for (var i = 1; i < points.Length; i += 1)
            {
                if (GetSide(pos.x, pos.z, dir.x, dir.z, points[i].x, points[i].z) != side)
                    return true;
            }
            return false;
        }
        public static bool CanIntersect(BezierTrajectory bezier, in StraightTrajectory line, out Side side)
        {
            var dir = line.Direction.Turn90(true);
            var pos = line.StartPosition;
            var trajectory = bezier.Trajectory;

            side = GetSide(pos.x, pos.z, dir.x, dir.z, trajectory.a.x, trajectory.a.z);

            if (GetSide(pos.x, pos.z, dir.x, dir.z, trajectory.b.x, trajectory.b.z) != side)
                return true;
            else if (GetSide(pos.x, pos.z, dir.x, dir.z, trajectory.c.x, trajectory.c.z) != side)
                return true;
            else if (GetSide(pos.x, pos.z, dir.x, dir.z, trajectory.d.x, trajectory.d.z) != side)
                return true;
            else
                return false;
        }

        public static Side GetSide(float posX, float posZ, float dirX, float dirZ, float pointX, float pointZ)
        {
            return dirX * (pointX - posX) + dirZ * (pointZ - posZ) >= 0 ? Side.Right : Side.Left;
        }
        public static Side GetSide(Vector3 direction, Vector3 toCheck)
        {
            return direction.z * toCheck.x - direction.x * toCheck.z >= 0 ? Side.Right : Side.Left;
        }

        public override string ToString()
        {
            if (isIntersect)
                return $"{firstT:0.###} ÷ {secondT:0.###}";
            else
                return "Not intersect";
        }

        public static bool operator ==(Intersection a, Intersection b) => a.firstT == b.firstT && a.secondT == b.secondT;
        public static bool operator !=(Intersection a, Intersection b) => a.firstT != b.firstT || a.secondT != b.secondT;

        public enum Side
        {
            Left,
            Right
        }
        public class Comparer : IComparer<Intersection>
        {
            private readonly bool isFirst;
            public Comparer(bool isFirst = true)
            {
                this.isFirst = isFirst;
            }
            public int Compare(Intersection x, Intersection y) => isFirst ? x.firstT.CompareTo(y.firstT) : x.secondT.CompareTo(y.secondT);
        }

        private readonly struct TrajectoryPoint
        {
            public readonly Vector3 pos;
            public readonly float t;

            public TrajectoryPoint(Vector3 pos, float t)
            {
                this.pos = pos;
                this.t = t;
            }

            public override string ToString() => $"{t:0.###}: {pos}";
        }
    }
    public struct IntersectionPair
    {
        public Intersection from;
        public Intersection to;

        public bool Inverted { get; private set; }
        public IntersectionPair Reverse => new IntersectionPair(to, from) { Inverted = !Inverted };

        public IntersectionPair(Intersection from, Intersection to)
        {
            this.from = from;
            this.to = to;
            Inverted = false;
        }

        public bool Contain(Intersection intersection) => from == intersection || to == intersection;

        public Intersection GetOther(Intersection intersection)
        {
            if (intersection == from)
                return to;
            else if (intersection == to)
                return from;
            else
                return Intersection.NotIntersect;
        }

        public override string ToString() => $"{from.secondT:0.###} ÷ [{from.firstT:0.###} ÷ {to.firstT:0.###}] ÷ {to.secondT:0.###}";
    }
}
