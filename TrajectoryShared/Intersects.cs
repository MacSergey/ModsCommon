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
        public static ComponentComparer FirstComparer { get; } = new ComponentComparer(true, true);
        public static ComponentComparer SecondComparer { get; } = new ComponentComparer(false, true);

        public static ComponentComparer FirstApproxComparer { get; } = new ComponentComparer(true, false);
        public static ComponentComparer SecondApproxComparer { get; } = new ComponentComparer(false, false);

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
            var result = Calculate(firstTrajectory, secondTrajectory);
            if (result.Count > 0)
            {
                firstT = result[0].firstT;
                secondT = result[0].secondT;
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
                    IntersectITrajectoryWithStraight(result, in firstStraight, secondTrajectory, false, SplitData.Default);
            }
            else
            {
                if (secondTrajectory.TrajectoryType == TrajectoryType.Line)
                {
                    var secondStraight = (StraightTrajectory)secondTrajectory;
                    IntersectITrajectoryWithStraight(result, in secondStraight, firstTrajectory, true, SplitData.Default);
                }
                else
                    IntersectITrajectoryWithITrajectory(result, firstTrajectory, secondTrajectory, SplitData.Default, SplitData.Default);
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
        private static IntersectResult IntersectITrajectoryWithITrajectory(List<Intersection> results, ITrajectory first, ITrajectory second, SplitData firstData, SplitData secondData)
        {
            var firstPoints = CalcTrajectoryParts(first, firstData, out int firstParts);
            var secondPoints = CalcTrajectoryParts(second, secondData, out int secondParts);
            var result = new IntersectResult();

            if (firstParts == 1 && secondParts == 1)
            {
                if (IntersectSections(firstPoints[0].pos, firstPoints[1].pos, secondPoints[0].pos, secondPoints[1].pos, out float firstT, out float secondT))
                {
                    firstT = (1f / firstData.total) * (firstData.index + firstData.merge * firstT);
                    secondT = (1f / secondData.total) * (secondData.index + secondData.merge * secondT);
                    results.Add(new Intersection(firstT, secondT, first, second));
                    return IntersectResult.Positive;
                }

                result.Add(firstT, secondT);
            }
            else
            {
                for (var i = 0; i < firstParts; i += 1)
                {
                    for (var j = 0; j < secondParts; j += 1)
                    {
                        if (IntersectSections(firstPoints[i].pos, firstPoints[i + 1].pos, secondPoints[j].pos, secondPoints[j + 1].pos, out float firstT, out float secondT))
                        {
                            var nextFirstData = GetNext(firstData, i, firstParts, firstT);
                            var nextSecondData = GetNext(secondData, j, secondParts, secondT);

                            var nextResult = IntersectITrajectoryWithITrajectory(results, first, second, nextFirstData, nextSecondData);
                            if (nextResult.Intersect)
                                return IntersectResult.Positive;

                            var needI = NeedCheck(nextResult.firstDir, i, firstParts, out var nextI);
                            var needJ = NeedCheck(nextResult.secondDir, j, secondParts, out var nextJ);
                            if (!needI && !needJ)
                                continue;

                            nextFirstData = GetNext(firstData, nextI, firstParts, firstT);
                            nextSecondData = GetNext(secondData, nextJ, secondParts, secondT);

                            nextResult = IntersectITrajectoryWithITrajectory(results, first, second, nextFirstData, nextSecondData);
                            if (nextResult.Intersect)
                                return IntersectResult.Positive;
                        }

                        result.Add(firstT, secondT);
                    }
                }
            }

            return result;

            static SplitData GetNext(SplitData data, int i, int count, float t)
            {
                if (count == 1)
                {
                    return data;
                }
                if (t < 0.1f && i > 0)
                {
                    return new SplitData((data.index * count / data.merge + i) * 2 - 1, (data.total * count / data.merge) * 2, 2);
                }
                else if (t > 0.9f && i < count - 1)
                {
                    return new SplitData((data.index * count / data.merge + i) * 2 + 1, (data.total * count / data.merge) * 2, 2);
                }
                else
                {
                    return new SplitData(data.index * count / data.merge + i, data.total * count / data.merge, 1);
                }
            }
            static bool IntersectSections(Vector3 a, Vector3 b, Vector3 c, Vector3 d, out float p, out float q)
            {
                if(Line2.Intersect(XZ(a), XZ(b), XZ(c), XZ(d), out p, out q))
                {
                    if(IsCorrectT(p) && IsCorrectT(q))
                    {
                        var dot = Vector2.Dot((XZ(b) - XZ(a)).normalized, (XZ(d) - XZ(c)).normalized);
                        if (!Mathf.Approximately(Mathf.Abs(dot), 1f))
                            return true;
                    }
                }

                return false;
            }
            static bool NeedCheck(IntersectionDirection dir, int i, int count, out int nextI)
            {
                if (dir == IntersectionDirection.Before && i > 0)
                {
                    nextI = i - 1;
                    return true;
                }
                else if (dir == IntersectionDirection.After && i < count - 1)
                {
                    nextI = i + 1;
                    return true;
                }
                else
                {
                    nextI = i;
                    return false;
                }
            }
        }
        private readonly struct SplitData
        {
            public static SplitData Default = new SplitData(0, 1, 1);

            public readonly int index;
            public readonly int total;
            public readonly int merge;

            public SplitData(int index, int total, int merge = 1)
            {
                this.index = index;
                this.total = total;
                this.merge = merge;
            }

            public override string ToString() => merge <= 1 ? $"{index}/{total}" : $"{index}-{index + merge}/{total}";
        }
        private struct IntersectResult
        {
            public static IntersectResult Positive = new IntersectResult()
            {
                firstDir = IntersectionDirection.Middle,
                secondDir = IntersectionDirection.Middle
            };

            public IntersectionDirection firstDir;
            public IntersectionDirection secondDir;

            public bool Intersect => firstDir == IntersectionDirection.Middle && secondDir == IntersectionDirection.Middle;

            public void Add(float firstT, float secondT)
            {
                firstDir |= GetDirection(firstT);
                secondDir |= GetDirection(secondT);
            }
            private IntersectionDirection GetDirection(float t)
            {
                if (t < 0f)
                    return IntersectionDirection.Before;
                else if (t > 1f)
                    return IntersectionDirection.After;
                else
                    return IntersectionDirection.Middle;
            }
        }
        [Flags]
        private enum IntersectionDirection
        {
            None = 0,
            Before = 1,
            Middle = 2,
            After = 4,
        }

        #endregion

        #region ITRAJECTORY - STRAIGHT

        private static void IntersectITrajectoryWithStraight(List<Intersection> results, in StraightTrajectory line, ITrajectory trajectory, bool invert, SplitData data)
        {
            var points = CalcTrajectoryParts(trajectory, data, out int parts);

            if (parts > 1)
            {
                for (var i = 0; i < parts; i += 1)
                {
                    if (IntersectSectionAndRay(in line, points[i].pos, points[i + 1].pos, out _, out _))
                    {
                        var nextData = new SplitData(data.index * parts + i, data.total * parts);
                        IntersectITrajectoryWithStraight(results, in line, trajectory, invert, nextData);
                    }
                }
            }
            else if (parts == 1 && IntersectSectionAndRay(in line, points[0].pos, points[1].pos, out float firstT, out float secondT))
            {
                secondT = 1f / data.total * (data.index + secondT);
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

            static bool IntersectSectionAndRay(in StraightTrajectory line, Vector3 start, Vector3 end, out float p, out float q)
            {
                return Line2.Intersect(XZ(line.StartPosition), XZ(line.EndPosition), XZ(start), XZ(end), out p, out q) && IsCorrectT(in line, p) && IsCorrectT(q);
            }
        }

        #endregion

        private static TrajectoryPoint[] CalcTrajectoryParts(ITrajectory trajectory, SplitData data, out int parts)
        {
            var startT = 1f / data.total * data.index;
            var endT = 1f / data.total * (data.index + data.merge);

            var start = trajectory.Position(startT);
            var end = trajectory.Position(endT);
            var middle = trajectory.Position((startT + endT) * 0.5f);

            var length = Mathf.Max((middle - start).magnitude + (end - middle).magnitude, 0f);
            parts = Math.Min(Mathf.CeilToInt(length / MinLength), 10);
            if (parts > data.merge)
                parts = parts / data.merge * data.merge;

            var points = new TrajectoryPoint[parts + 1];
#if DEBUG
            if (parts == 1)
            {
                points[0] = new TrajectoryPoint(start, startT, data.index, data.total);
                points[parts] = new TrajectoryPoint(end, endT, data.index + data.merge, data.total);
            }
            else
            {
                points[0] = new TrajectoryPoint(start, startT, parts * data.index / data.merge, parts * data.total / data.merge);
                points[parts] = new TrajectoryPoint(end, endT, parts * data.index / data.merge + parts, parts * data.total / data.merge);
            }
#else
            points[0] = new TrajectoryPoint(start, startT);
            points[parts] = new TrajectoryPoint(end, endT);
#endif

            for (var i = 1; i < parts; i += 1)
            {
                var t = startT + 1f / (parts * data.total / data.merge) * i;
#if DEBUG
                points[i] = new TrajectoryPoint(trajectory.Position(t), t, data.index * parts / data.merge + i, parts * data.total / data.merge);
#else
                points[i] = new TrajectoryPoint(trajectory.Position(t), t);
#endif
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
        public class ComponentComparer : IComparer<Intersection>
        {
            private readonly bool isFirst;
            private readonly bool strict;
            public ComponentComparer(bool isFirst, bool strict)
            {
                this.isFirst = isFirst;
                this.strict = strict;
            }
            public int Compare(Intersection x, Intersection y)
            {
                if(isFirst)
                    return !strict && Approximately(x.firstT, y.firstT) ? 0 : x.firstT.CompareTo(y.firstT);
                else
                    return !strict && Approximately(x.secondT, y.secondT) ? 0 : x.secondT.CompareTo(y.secondT);
            }
            private bool Approximately(float a, float b)
            {
                return Mathf.Abs(b - a) < 0.001f * Mathf.Max(Mathf.Abs(a), Mathf.Abs(b));
            }
        }

        private readonly struct TrajectoryPoint
        {
            public readonly Vector3 pos;
            public readonly float t;
#if DEBUG
            public readonly int index;
            public readonly int total;
#endif

#if DEBUG
            public TrajectoryPoint(Vector3 pos, float t, int index, int total)
            {
                this.pos = pos;
                this.t = t;
                this.index = index;
                this.total = total;
            }

            public override string ToString() => $"{t:0.###}: {pos} ({index}/{total})";
#else
            public TrajectoryPoint(Vector3 pos, float t)
            {
                this.pos = pos;
                this.t = t;
            }

            public override string ToString() => $"{t:0.###}: {pos}";
#endif
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
