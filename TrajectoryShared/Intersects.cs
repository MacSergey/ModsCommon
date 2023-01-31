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
            if (firstTrajectory.TrajectoryType == TrajectoryType.Bezier)
            {
                var firstBezier = (BezierTrajectory)firstTrajectory;

                if (secondTrajectory.TrajectoryType == TrajectoryType.Bezier)
                {
                    var secondBezier = (BezierTrajectory)secondTrajectory;
                    IntersectBezierWithBezier(result, firstBezier.Trajectory, secondBezier.Trajectory);
                }
                else if (secondTrajectory.TrajectoryType == TrajectoryType.Line)
                {
                    var secondStraight = (StraightTrajectory)secondTrajectory;
                    IntersectBezierWithStraight(result, in secondStraight, firstBezier, true);
                }
                else if (secondTrajectory.TrajectoryType == TrajectoryType.Combined)
                    IntersectITrajectoryWithITrajectory(result, in firstTrajectory, in secondTrajectory);
            }
            else if (firstTrajectory.TrajectoryType == TrajectoryType.Line)
            {
                var firstStraight = (StraightTrajectory)firstTrajectory;

                if (secondTrajectory.TrajectoryType == TrajectoryType.Bezier)
                {
                    var secondBezier = (BezierTrajectory)secondTrajectory;
                    IntersectBezierWithStraight(result, in firstStraight, secondBezier, false);
                }
                else if (secondTrajectory.TrajectoryType == TrajectoryType.Line)
                {
                    var secondStraight = (StraightTrajectory)secondTrajectory;
                    IntersectStraightWithStraight(result, in firstStraight, in secondStraight);
                }
                else if (secondTrajectory.TrajectoryType == TrajectoryType.Combined)
                    IntersectITrajectoryWithStraight(result, in firstStraight, in secondTrajectory, false);
            }
            else if (firstTrajectory.TrajectoryType == TrajectoryType.Combined)
            {
                if (secondTrajectory is BezierTrajectory)
                    IntersectITrajectoryWithITrajectory(result, in firstTrajectory, in secondTrajectory);
                else if (secondTrajectory.TrajectoryType == TrajectoryType.Line)
                {
                    var secondStraight = (StraightTrajectory)secondTrajectory;
                    IntersectITrajectoryWithStraight(result, in secondStraight, in secondTrajectory, true);
                }
                else if (secondTrajectory is CombinedTrajectory)
                    IntersectITrajectoryWithITrajectory(result, in firstTrajectory, in secondTrajectory);
            }

            for (int i = 0; i < result.Count; i += 1)
            {
                var item = result[i];
                result[i] = new Intersection(true, item.firstT, item.secondT, firstTrajectory, secondTrajectory);
            }

            return result;
        }
        public static List<Intersection> Calculate(ITrajectory trajectory, IEnumerable<ITrajectory> otherTrajectories, bool onlyIntersect = false)
            => otherTrajectories.SelectMany(t => Calculate(trajectory, t)).Where(i => !onlyIntersect || i.isIntersect).ToList();

        #region BEZIER - BEZIER
        private static bool IntersectBezierWithBezier(List<Intersection> results, Bezier3 first, Bezier3 second, int fIdx = 0, int fOf = 1, int sIdx = 0, int sOf = 1)
        {
            CalcBezierParts(first, out int fParts, out float[] fPoints, out Vector3[] fPos);
            CalcBezierParts(second, out int sParts, out float[] sPoints, out Vector3[] sPos);

            if (fParts == 1 && sParts == 1)
            {
                IntersectSections(first.a, first.d, second.a, second.d, out float firstT, out float secondT);
                firstT = 1f / fOf * (fIdx + firstT);
                secondT = 1f / sOf * (sIdx + secondT);
                results.Add(new Intersection(firstT, secondT));
                return true;
            }

            for (var i = 0; i < fParts; i += 1)
            {
                for (var j = 0; j < sParts; j += 1)
                {
                    if (IntersectSections(fPos[i], fPos[i + 1], sPos[j], sPos[j + 1], out float p, out float q))
                    {
                        var needTryI = NeedTryParts(i, fParts, p);
                        var needTryJ = NeedTryParts(j, sParts, q);
                        for (int tryI = 0; tryI < needTryI.Length; tryI += 1)
                        {
                            var ii = needTryI[tryI];
                            for (int tryJ = 0; tryJ < needTryJ.Length; tryJ += 1)
                            {
                                var jj = needTryJ[tryJ];

                                var firstCut = first.Cut(fPoints[ii], fPoints[ii + 1]);
                                var secondCut = second.Cut(sPoints[jj], sPoints[jj + 1]);

                                if (IntersectBezierWithBezier(results, firstCut, secondCut, fIdx * fParts + ii, fOf * fParts, sIdx * sParts + jj, sOf * sParts))
                                    return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        #endregion

        #region BEZIER - STRAIGHT

        private static void IntersectBezierWithStraight(List<Intersection> results, in StraightTrajectory line, BezierTrajectory bezier, bool invert, int idx = 0, int of = 1)
        {
            CalcBezierParts(bezier, out int parts, out float[] points, out Vector3[] pos);

            if (parts > 1)
            {
                for (var i = 0; i < parts; i += 1)
                {
                    if (IntersectSectionAndRay(in line, pos[i], pos[i + 1], out _, out _))
                    {
                        var cut = bezier.Cut(points[i], points[i + 1]);
                        IntersectBezierWithStraight(results, in line, cut, invert, idx * parts + i, of * parts);
                    }
                }
            }
            else if (IntersectSectionAndRay(in line, bezier.StartPosition, bezier.EndPosition, out float p, out float q))
            {
                q = 1f / of * (idx + q);
                var result = new Intersection(invert ? q : p, invert ? p : q);
                results.Add(result);
            }
        }

        #endregion

        #region STRAIGHT - STRAIGHT
        public static void IntersectStraightWithStraight(List<Intersection> results, in StraightTrajectory firstStraight, in StraightTrajectory secondStraight)
        {
            var trajectory1 = firstStraight.Trajectory;
            var trajectory2 = secondStraight.Trajectory;
            if (Line2.Intersect(XZ(trajectory1.a), XZ(trajectory1.b), XZ(trajectory2.a), XZ(trajectory2.b), out float p, out float q) && IsCorrectT(in firstStraight, p) && IsCorrectT(in secondStraight, q))
            {
                var intersect = new Intersection(p, q);
                results.Add(intersect);
            }
        }
        public static Intersection GetIntersection(in StraightTrajectory firstStraight, in StraightTrajectory secondStraight)
        {
            var trajectory1 = firstStraight.Trajectory;
            var trajectory2 = secondStraight.Trajectory;
            if (Line2.Intersect(XZ(trajectory1.a), XZ(trajectory1.b), XZ(trajectory2.a), XZ(trajectory2.b), out float p, out float q) && IsCorrectT(in firstStraight, p) && IsCorrectT(secondStraight, q))
                return new Intersection(p, q);
            else
                return NotIntersect;
        }
        #endregion

        #region ITRAJECTORY - ITRAJECTORY
        private static bool IntersectITrajectoryWithITrajectory(List<Intersection> results, in ITrajectory first, in ITrajectory second, int fIdx = 0, int fOf = 1, int sIdx = 0, int sOf = 1)
        {
            CalcTrajectoryParts(in first, out int fParts, out float[] fPoints, out Vector3[] fPos);
            CalcTrajectoryParts(in second, out int sParts, out float[] sPoints, out Vector3[] sPos);

            if (fParts == 1 && sParts == 1)
            {
                IntersectSections(first.StartPosition, first.EndPosition, second.StartPosition, second.EndPosition, out float firstT, out float secondT);
                firstT = 1f / fOf * (fIdx + firstT);
                secondT = 1f / sOf * (sIdx + secondT);
                results.Add(new Intersection(firstT, secondT));
                return true;
            }

            for (var i = 0; i < fParts; i += 1)
            {
                for (var j = 0; j < sParts; j += 1)
                {
                    if (IntersectSections(fPos[i], fPos[i + 1], sPos[j], sPos[j + 1], out float p, out float q))
                    {
                        var needTryI = NeedTryParts(i, fParts, p);
                        var needTryJ = NeedTryParts(j, sParts, q);
                        for (int tryI = 0; tryI < needTryI.Length; tryI += 1)
                        {
                            var ii = needTryI[tryI];
                            for (int tryJ = 0; tryJ < needTryJ.Length; tryJ += 1)
                            {
                                var jj = needTryJ[tryJ];

                                var firstCut = first.Cut(fPoints[ii], fPoints[ii + 1]);
                                var secondCut = second.Cut(sPoints[jj], sPoints[jj + 1]);

                                if (IntersectITrajectoryWithITrajectory(results, in firstCut, in secondCut, fIdx * fParts + ii, fOf * fParts, sIdx * sParts + jj, sOf * sParts))
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

        private static void IntersectITrajectoryWithStraight(List<Intersection> results, in StraightTrajectory line, in ITrajectory trajectory, bool invert, int idx = 0, int of = 1)
        {
            CalcTrajectoryParts(in trajectory, out int parts, out float[] points, out Vector3[] pos);

            if (parts > 1)
            {
                for (var i = 0; i < parts; i += 1)
                {
                    if (IntersectSectionAndRay(in line, pos[i], pos[i + 1], out _, out _))
                    {
                        var cut = trajectory.Cut(points[i], points[i + 1]);
                        IntersectITrajectoryWithStraight(results, in line, in cut, invert, idx * parts + i, of * parts);
                    }
                }
            }
            else if (IntersectSectionAndRay(in line, trajectory.StartPosition, trajectory.EndPosition, out float p, out float q))
            {
                q = 1f / of * (idx + q);
                var result = new Intersection(invert ? q : p, invert ? p : q);
                results.Add(result);
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
            else if (0.9f < p && i + 1 < count)
                return new int[] { i, i + 1 };
            else
                return new int[] { i };
        }
        private static bool IntersectSectionAndRay(in StraightTrajectory line, Vector3 start, Vector3 end, out float p, out float q)
        {
            return Line2.Intersect(XZ(line.StartPosition), XZ(line.EndPosition), XZ(start), XZ(end), out p, out q) && IsCorrectT(in line, p) && IsCorrectT(q);
        }

        private static void CalcBezierParts(Bezier3 bezier, out int parts, out float[] points, out Vector3[] positons)
        {
            bezier.Divide(out Bezier3 b1, out Bezier3 b2);
            var length = Mathf.Max((b1.d - b1.a).magnitude + (b2.d - b2.a).magnitude, 0f);
            parts = Math.Min((int)Math.Ceiling(length / MinLength), 10);

            points = new float[parts + 1];
            points[0] = 0f;
            points[parts] = 1f;

            positons = new Vector3[parts + 1];
            positons[0] = bezier.a;
            positons[parts] = bezier.d;

            for (var i = 1; i < parts; i += 1)
            {
                points[i] = (1f / parts) * i;
                positons[i] = bezier.Position(points[i]);
            }
        }
        private static void CalcTrajectoryParts(in ITrajectory trajectory, out int parts, out float[] points, out Vector3[] positons)
        {
            trajectory.Divide(out ITrajectory b1, out ITrajectory b2);
            var length = Mathf.Max(b1.Magnitude + b2.Magnitude, 0f);
            parts = Math.Min((int)Math.Ceiling(length / MinLength), 10);

            points = new float[parts + 1];
            points[0] = 0f;
            points[parts] = 1f;

            positons = new Vector3[parts + 1];
            positons[0] = trajectory.StartPosition;
            positons[parts] = trajectory.EndPosition;

            for (var i = 1; i < parts; i += 1)
            {
                points[i] = (1f / parts) * i;
                positons[i] = trajectory.Position(points[i]);
            }
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
