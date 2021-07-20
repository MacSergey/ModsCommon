using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ColossalFramework.Math.VectorUtils;

namespace ModsCommon.Utilities
{
    public class Intersection
    {
        public static float DeltaAngle = 5f;
        public static float MaxLength = 1f;
        public static float MinLength = 0.5f;
        public static Comparer FirstComparer { get; } = new Comparer(true);
        public static Comparer SecondComparer { get; } = new Comparer(false);
        public static Intersection NotIntersect => new Intersection();

        public ITrajectory First { get; protected set; }
        public ITrajectory Second { get; protected set; }

        public float FirstT { get; protected set; }
        public float SecondT { get; protected set; }
        public bool IsIntersect { get; }

        public Intersection(float firstT, float secondT)
        {
            IsIntersect = true;
            FirstT = firstT;
            SecondT = secondT;
        }
        protected Intersection()
        {
            IsIntersect = false;
        }

        public static Intersection CalculateSingle(ITrajectory firstTrajectory, ITrajectory secondTrajectory) => Calculate(firstTrajectory, secondTrajectory).FirstOrDefault() ?? NotIntersect;
        public static bool CalculateSingle(ITrajectory firstTrajectory, ITrajectory secondTrajectory, out float firstT, out float secondT)
        {
            if (Calculate(firstTrajectory, secondTrajectory).FirstOrDefault() is Intersection intersection)
            {
                firstT = intersection.FirstT;
                secondT = intersection.SecondT;
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
            var result = default(List<Intersection>);
            if (firstTrajectory is BezierTrajectory firstBezier)
            {
                if (secondTrajectory is BezierTrajectory secondBezier)
                    result = Calculate(firstBezier, secondBezier);
                else if (secondTrajectory is StraightTrajectory secondStraight)
                    result = Calculate(firstBezier, secondStraight);
            }
            else if (firstTrajectory is StraightTrajectory firstStraight)
            {
                if (secondTrajectory is BezierTrajectory secondBezier)
                    result = Calculate(firstStraight, secondBezier);
                else if (secondTrajectory is StraightTrajectory secondStraight)
                    result = Calculate(firstStraight, secondStraight);
            }

            if(result == null)
                return new List<Intersection>();
            else
            {
                foreach(var item in result)
                {
                    item.First = firstTrajectory;
                    item.Second = secondTrajectory;
                }    

                return result;
            }
        }
        public static List<Intersection> Calculate(ITrajectory trajectory, IEnumerable<ITrajectory> otherTrajectories, bool onlyIntersect = false)
            => otherTrajectories.SelectMany(t => Calculate(trajectory, t)).Where(i => !onlyIntersect || i.IsIntersect).ToList();

        #region BEZIER - BEZIER
        private static List<Intersection> Calculate(BezierTrajectory firstBezier, BezierTrajectory secondBezier)
        {
            var intersects = new List<Intersection>();
            Intersect(intersects, firstBezier, secondBezier);
            return intersects;
        }
        private static bool Intersect(List<Intersection> results, Bezier3 first, Bezier3 second, int fIdx = 0, int fOf = 1, int sIdx = 0, int sOf = 1)
        {
            CalcParts(first, out int fParts, out float[] fPoints, out Vector3[] fPos);
            CalcParts(second, out int sParts, out float[] sPoints, out Vector3[] sPos);

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
                        foreach (var ii in WillTryParts(i, fParts, p))
                        {
                            foreach (var jj in WillTryParts(j, sParts, q))
                            {
                                var firstCut = first.Cut(fPoints[ii], fPoints[ii + 1]);
                                var secondCut = second.Cut(sPoints[jj], sPoints[jj + 1]);

                                if (Intersect(results, firstCut, secondCut, fIdx * fParts + ii, fOf * fParts, sIdx * sParts + jj, sOf * sParts))
                                    return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
        private static bool IntersectSections(Vector3 a, Vector3 b, Vector3 c, Vector3 d, out float p, out float q)
            => Line2.Intersect(XZ(a), XZ(b), XZ(c), XZ(d), out p, out q) && CorrectT(p) && CorrectT(q);
        private static IEnumerable<int> WillTryParts(int i, int count, float p)
        {
            yield return i;
            if (p < 0.1f && i != 0)
                yield return i - 1;
            if (0.9f < p && i + 1 < count)
                yield return i + 1;
        }

        #endregion

        #region BEZIER - STRAIGHT

        private static List<Intersection> Calculate(StraightTrajectory straight, BezierTrajectory bezier)
        {
            var intersects = new List<Intersection>();
            Intersect(intersects, straight, bezier, false);
            return intersects;
        }
        private static List<Intersection> Calculate(BezierTrajectory bezier, StraightTrajectory straight)
        {
            var intersects = new List<Intersection>();
            Intersect(intersects, straight, bezier, true);
            return intersects;
        }

        private static void Intersect(List<Intersection> results, StraightTrajectory line, BezierTrajectory bezier, bool invert, int idx = 0, int of = 1)
        {
            CalcParts(bezier, out int parts, out float[] points, out Vector3[] pos);

            if (parts > 1)
            {
                for (var i = 0; i < parts; i += 1)
                {
                    if (IntersectSectionAndRay(line, pos[i], pos[i + 1], out _, out _))
                    {
                        var cut = bezier.Cut(points[i], points[i + 1]);
                        Intersect(results, line, cut, invert, idx * parts + i, of * parts);
                    }
                }
            }
            else if (IntersectSectionAndRay(line, bezier.StartPosition, bezier.EndPosition, out float p, out float q))
            {
                q = 1f / of * (idx + q);
                var result = new Intersection(invert ? q : p, invert ? p : q);
                results.Add(result);
            }
        }
        private static bool IntersectSectionAndRay(StraightTrajectory line, Vector3 start, Vector3 end, out float p, out float q) =>
            Line2.Intersect(XZ(line.StartPosition), XZ(line.EndPosition), XZ(start), XZ(end), out p, out q) && (!line.IsSection || CorrectT(p)) && CorrectT(q);

        #endregion

        #region STRAIGHT - STRAIGHT
        private static List<Intersection> Calculate(StraightTrajectory firstStraight, StraightTrajectory secondStraight)
        {
            var intersects = new List<Intersection>();
            var trajectory1 = firstStraight.Trajectory;
            var trajectory2 = secondStraight.Trajectory;
            if (Line2.Intersect(XZ(trajectory1.a), XZ(trajectory1.b), XZ(trajectory2.a), XZ(trajectory2.b), out float p, out float q) && (!firstStraight.IsSection || CorrectT(p)) && (!secondStraight.IsSection || CorrectT(q)))
            {
                var intersect = new Intersection(p, q);
                intersects.Add(intersect);
            }
            return intersects;
        }
        #endregion


        protected static void CalcParts(Bezier3 bezier, out int parts, out float[] points, out Vector3[] positons)
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
        public static bool CorrectT(float t) => 0f <= t && t <= 1f;

        public override string ToString() => $"{IsIntersect}:{FirstT};{SecondT}";


        public class Comparer : IComparer<Intersection>
        {
            private readonly bool _isFirst;
            public Comparer(bool isFirst = true)
            {
                _isFirst = isFirst;
            }
            public int Compare(Intersection x, Intersection y) => _isFirst ? x.FirstT.CompareTo(y.FirstT) : x.SecondT.CompareTo(y.SecondT);
        }
    }
}
