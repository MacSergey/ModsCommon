using ColossalFramework.Math;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModsCommon.Utilities
{
    public static class MathExtention
    {
        public static Vector2 Turn90(this Vector2 v, bool isClockWise) => isClockWise ? new Vector2(v.y, -v.x) : new Vector2(-v.y, v.x);
        public static Vector2 TurnDeg(this Vector2 vector, float turnAngle, bool isClockWise) => vector.TurnRad(turnAngle * Mathf.Deg2Rad, isClockWise);
        public static Vector2 TurnRad(this Vector2 vector, float turnAngle, bool isClockWise)
        {
            turnAngle = isClockWise ? -turnAngle : turnAngle;
            var newX = vector.x * Mathf.Cos(turnAngle) - vector.y * Mathf.Sin(turnAngle);
            var newY = vector.x * Mathf.Sin(turnAngle) + vector.y * Mathf.Cos(turnAngle);
            return new Vector2(newX, newY);
        }

        public static Vector3 Turn90(this Vector3 v, bool isClockWise) => isClockWise ? new Vector3(v.z, v.y, -v.x) : new Vector3(-v.z, v.y, v.x);
        public static Vector3 TurnDeg(this Vector3 vector, float turnAngle, bool isClockWise) => vector.TurnRad(turnAngle * Mathf.Deg2Rad, isClockWise);
        public static Vector3 TurnRad(this Vector3 vector, float turnAngle, bool isClockWise)
        {
            turnAngle = isClockWise ? -turnAngle : turnAngle;
            var newX = vector.x * Mathf.Cos(turnAngle) - vector.z * Mathf.Sin(turnAngle);
            var newZ = vector.x * Mathf.Sin(turnAngle) + vector.z * Mathf.Cos(turnAngle);
            return new Vector3(newX, vector.y, newZ);
        }

        public static float Length(this Bezier3 bezier, float minAngleDelta = 10, int depth = 0, int maxDepth = 5)
        {
            var start = bezier.b - bezier.a;
            var end = bezier.c - bezier.d;
            if (start.magnitude < Vector3.kEpsilon || end.magnitude < Vector3.kEpsilon)
                return 0;

            var angle = Vector3.Angle(start, end);
            if (depth < maxDepth && 180 - angle > minAngleDelta)
            {
                bezier.Divide(out Bezier3 first, out Bezier3 second);
                var firstLength = first.Length(depth: depth + 1, maxDepth: maxDepth);
                var secondLength = second.Length(depth: depth + 1, maxDepth: maxDepth);
                return firstLength + secondLength;
            }
            else
            {
                var length = (bezier.d - bezier.a).magnitude;
                return length;
            }
        }
        public static float Length(this Bezier3 bezier, out List<BezierPoint> bezierPoints, float minAngleDelta = 10, int depth = 0)
        {
            bezierPoints = new List<BezierPoint>();

            var start = bezier.b - bezier.a;
            var end = bezier.c - bezier.d;
            if (start.magnitude < Vector3.kEpsilon || end.magnitude < Vector3.kEpsilon)
                return 0;

            var angle = Vector3.Angle(start, end);
            if (depth < 5 && 180 - angle > minAngleDelta)
            {
                bezier.Divide(out Bezier3 first, out Bezier3 second);
                var firstLength = first.Length(out List<BezierPoint> firstPoints, depth: depth + 1);
                var secondLength = second.Length(out List<BezierPoint> secondPoints, depth: depth + 1);
                var length = firstLength + secondLength;
                if (length == 0)
                    return 0;

                var firstPart = firstLength / length;
                var secondPart = secondLength / length;

                foreach (var point in firstPoints)
                {
                    bezierPoints.Add(new BezierPoint(point.T * firstPart, point.Length));
                }
                foreach (var point in secondPoints.Skip(1))
                {
                    bezierPoints.Add(new BezierPoint(point.T * secondPart + firstPart, point.Length + firstLength));
                }
                return length;
            }
            else
            {
                var length = (bezier.d - bezier.a).magnitude;
                bezierPoints.Add(new BezierPoint(0, 0));
                bezierPoints.Add(new BezierPoint(1, length));
                return length;
            }
        }
        public static Vector2 XZ(this Vector3 vector) => VectorUtils.XZ(vector);
        public static float AbsoluteAngle(this Vector3 vector) => Mathf.Atan2(vector.z, vector.x);
        public static float DeltaAngle(this Bezier3 bezier) => 180 - Vector3.Angle(bezier.b - bezier.a, bezier.c - bezier.d);
        public static Vector3 Direction(this float absoluteAngle) => Vector3.right.TurnRad(absoluteAngle, false).normalized;
        public static float Magnitude(this Bounds bounds) => bounds.size.magnitude / Mathf.Sqrt(3);

        public static Bezier3 GetBezier(this Line3 line)
        {
            var bezier = new Bezier3
            {
                a = line.a,
                d = line.b,
            };
            var dir = line.b - line.a;
            NetSegment.CalculateMiddlePoints(bezier.a, dir, bezier.d, -dir, true, true, out bezier.b, out bezier.c);

            return bezier;
        }
        public static Vector3 GetRayPosition(this Segment3 ray, float height, out float t)
        {
            Segment1.Intersect(ray.a.y, ray.b.y, height, out t);
            return ray.Position(t);
        }

        public static int NextIndex(this int i, int count, int shift = 1) => (i + shift) % count;
        public static int PrevIndex(this int i, int count, int shift = 1) => shift > i ? i + count - (shift % count) : i - shift;
    }
    public struct BezierPoint
    {
        public float Length;
        public float T;
        public BezierPoint(float t, float length)
        {
            T = t;
            Length = length;
        }
        public override string ToString() => $"{T} - {Length}";
    }
    public class BezierPointComparer : IComparer<BezierPoint>
    {
        public static BezierPointComparer Instance { get; } = new BezierPointComparer();
        public int Compare(BezierPoint x, BezierPoint y) => x.Length.CompareTo(y.Length);
    }
}
