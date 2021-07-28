using ColossalFramework.Math;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ColossalFramework.Math.VectorUtils;

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
        public static Vector3 MakeFlat(this Vector3 v) => new Vector3(v.x, 0f, v.z);
        public static Vector3 MakeFlatNormalized(this Vector3 v) => new Vector3(v.x, 0f, v.z).normalized;
        public static Vector3 Turn90(this Vector3 v, bool isClockWise) => isClockWise ? new Vector3(v.z, v.y, -v.x) : new Vector3(-v.z, v.y, v.x);
        public static Vector3 TurnDeg(this Vector3 vector, float turnAngle, bool isClockWise) => vector.TurnRad(turnAngle * Mathf.Deg2Rad, isClockWise);
        public static Vector3 TurnRad(this Vector3 vector, float turnAngle, bool isClockWise)
        {
            turnAngle = isClockWise ? -turnAngle : turnAngle;
            var newX = vector.x * Mathf.Cos(turnAngle) - vector.z * Mathf.Sin(turnAngle);
            var newZ = vector.x * Mathf.Sin(turnAngle) + vector.z * Mathf.Cos(turnAngle);
            return new Vector3(newX, vector.y, newZ);
        }

        public static Vector4 Turn90(this Vector4 v, bool isClockWise) => isClockWise ? new Vector4(v.z, v.y, -v.x, v.w) : new Vector4(-v.z, v.y, v.x, v.w);
        public static Vector4 TurnDeg(this Vector4 vector, float turnAngle, bool isClockWise) => vector.TurnRad(turnAngle * Mathf.Deg2Rad, isClockWise);
        public static Vector4 TurnRad(this Vector4 vector, float turnAngle, bool isClockWise)
        {
            turnAngle = isClockWise ? -turnAngle : turnAngle;
            var newX = vector.x * Mathf.Cos(turnAngle) - vector.z * Mathf.Sin(turnAngle);
            var newZ = vector.x * Mathf.Sin(turnAngle) + vector.z * Mathf.Cos(turnAngle);
            return new Vector4(newX, vector.y, newZ, vector.w);
        }

        public static float Length(this Bezier3 bezier, float minAngleDelta = 10, int depth = 5)
        {
            var start = bezier.b - bezier.a;
            var end = bezier.c - bezier.d;
            if (start.sqrMagnitude < Vector3.kEpsilon || end.sqrMagnitude < Vector3.kEpsilon)
                return 0;

            var angle = Vector3.Angle(start, end);
            if (depth > 0 && 180 - angle > minAngleDelta)
            {
                bezier.Divide(out Bezier3 first, out Bezier3 second);
                var firstLength = first.Length(minAngleDelta, depth - 1);
                var secondLength = second.Length(minAngleDelta, depth - 1);
                return firstLength + secondLength;
            }
            else
            {
                var length = (bezier.d - bezier.a).magnitude;
                return length;
            }
        }

        public static float Travel(this Bezier3 bezier, float distance, int depth = 5)
        {
            var length = (bezier.b - bezier.a).magnitude + (bezier.c - bezier.b).magnitude + (bezier.d - bezier.c).magnitude;
            if (distance > length)
                return 1f;
            else
            {
                bezier.Travel(distance, depth, out _, out var t);
                return t;
            }
        }
        private static void Travel(this Bezier3 bezier, float distance, int depth, out float length, out float t, int idx = 0, int of = 1)
        {
            if (depth > 0)
            {
                bezier.Divide(out Bezier3 first, out Bezier3 second);
                first.Travel(distance, depth - 1, out length, out t, idx * 2, of * 2);
                if (t == -1f)
                {
                    second.Travel(distance - length, depth - 1, out var secondLength, out t, idx * 2 + 1, of * 2);
                    length += secondLength;
                }
            }
            else
            {
                length = (bezier.d - bezier.a).magnitude;
                if (distance < length)
                    t = 1f / of * (idx + distance / length);
                else
                    t = -1;
            }
        }
        public static Vector3 ClosestPosition(this Bezier3 bezier, Vector3 point)
        {
            bezier.ClosestPositionAndDirection(point, out var position, out _, out _);
            return position;
        }
        public static Vector3 ClosestDirection(this Bezier3 bezier, Vector3 point)
        {
            bezier.ClosestPositionAndDirection(point, out _, out var direction, out _);
            return direction;
        }
        public static void ClosestPositionAndDirection(this Bezier3 bezier, Vector3 point, out Vector3 position, out Vector3 direction, out float t)
        {
            var distance = 1E+11f;
            t = 0f;
            var prevPosition = bezier.a;
            for (var i = 1; i <= 16; i += 1)
            {
                var currentPosition = bezier.Position(i / 16f);
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
                var minPosition = bezier.Position(Mathf.Max(0f, t - delta));
                var currentPosition = bezier.Position(t);
                var maxPosition = bezier.Position(Mathf.Min(1f, t + delta));

                var minDistance = Segment3.DistanceSqr(minPosition, currentPosition, point, out var minU);
                var maxDistance = Segment3.DistanceSqr(currentPosition, maxPosition, point, out var maxU);

                t = minDistance >= maxDistance ? Mathf.Min(1f, t + delta * maxU) : Mathf.Max(0f, t - delta * (1f - minU));
                delta *= 0.5f;
            }

            position = bezier.Position(t);
            direction = NormalizeXZ(bezier.Tangent(t));
        }
        public static Vector3 GetHitPosition(this Bezier3 bezier, Segment3 ray, out float rayT, out float bezierT, out Vector3 position)
        {
            var hitPos = ray.GetRayPosition(bezier.Position(0.5f).y, out rayT);
            bezier.ClosestPositionAndDirection(hitPos, out position, out _, out bezierT);

            for (var i = 0; i < 3 && Mathf.Abs(hitPos.y - position.y) > 1f; i += 1)
            {
                hitPos = ray.GetRayPosition(position.y, out rayT);
                bezier.ClosestPositionAndDirection(hitPos, out position, out _, out bezierT);
            }

            return hitPos;
        }

        public static Vector3 GetHitPosition(this NetSegment segment, Segment3 ray, out float rayT, out Vector3 position)
        {
            var hitPos = ray.GetRayPosition(segment.m_middlePosition.y, out rayT);
            position = segment.GetClosestPosition(hitPos);

            for (var i = 0; i < 3 && Mathf.Abs(hitPos.y - position.y) > 1f; i += 1)
            {
                hitPos = ray.GetRayPosition(position.y, out rayT);
                position = segment.GetClosestPosition(hitPos);
            }

            return hitPos;
        }

        public static float AbsoluteAngle(this Vector3 vector) => Mathf.Atan2(vector.z, vector.x);
        public static float DeltaAngle(this Bezier3 bezier) => 180 - Vector3.Angle(bezier.b - bezier.a, bezier.c - bezier.d);
        public static Vector3 Direction(this float absoluteAngle) => Vector3.right.TurnRad(absoluteAngle, false).normalized;
        public static float Magnitude(this Bounds bounds) => bounds.size.magnitude / Mathf.Sqrt(3);
        public static float GetAngle(Vector3 firstDir, Vector3 secondDir)
        {
            var first = NormalizeXZ(firstDir);
            var second = NormalizeXZ(secondDir);

            var sign = -Mathf.Sign(VectorUtilsExtensions.CrossXZ(first, second));
            var angle = Mathf.Acos(Mathf.Clamp(DotXZ(first, second), -1f, 1f));

            return sign * angle;
        }

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
    public static class VectorUtilsExtensions
    {
        public static float CrossXZ(Vector3 a, Vector3 b) => a.z * b.x - a.x * b.z;
        public static float NormalizeCrossXZ(Vector3 a, Vector3 b) => CrossXZ(NormalizeXZ(a), NormalizeXZ(b));
        public static float NormalizeDotXZ(Vector3 a, Vector3 b) => DotXZ(NormalizeXZ(a), NormalizeXZ(b));
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
