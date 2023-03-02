using ColossalFramework.Math;
using System.Collections.Generic;
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
        public static Vector3 SetHeight(this Vector3 v, float height) => new Vector3(v.x, height, v.z);
        public static Vector3 AddHeight(this Vector3 v, float deltaHeight) => new Vector3(v.x, v.y + deltaHeight, v.z);
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
                return t == -1f ? 1f : t;
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
                    t = -1f;
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
        public static Vector3 ClosestPosition(this Line3 line, Vector3 point)
        {
            line.ClosestPositionAndDirection(point, out var position, out _, out _);
            return position;
        }
        public static Vector3 ClosestDirection(this Line3 line, Vector3 point)
        {
            line.ClosestPositionAndDirection(point, out _, out var direction, out _);
            return direction;
        }
        public static void ClosestPositionAndDirection(this Line3 line, Vector3 point, out Vector3 position, out Vector3 direction, out float t)
        {
            var normal = (line.b - line.a).MakeFlatNormalized().Turn90(true);
            Line2.Intersect(XZ(line.a), XZ(line.b), XZ(point), XZ(point + normal), out t, out _);
            t = Mathf.Clamp01(t);
            direction = (line.b - line.a).normalized;
            position = line.a + (line.b - line.a) * t;
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
        public static Vector3 GetHitPosition(this Line3 line, Segment3 ray, out float rayT, out float bezierT, out Vector3 position)
        {
            var hitPos = ray.GetRayPosition((line.a.y + line.b.y) / 2f, out rayT);
            line.ClosestPositionAndDirection(hitPos, out position, out _, out bezierT);

            for (var i = 0; i < 3 && Mathf.Abs(hitPos.y - position.y) > 1f; i += 1)
            {
                hitPos = ray.GetRayPosition(position.y, out rayT);
                line.ClosestPositionAndDirection(hitPos, out position, out _, out bezierT);
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
            var dir = line.b - line.a;
            var bezier = new Bezier3
            {
                a = line.a,
                b = line.a + dir * 0.3f,
                c = line.b - dir * 0.3f,
                d = line.b,
            };

            return bezier;
        }
        public static Vector3 GetRayPosition(this Segment3 ray, float height, out float t)
        {
            Segment1.Intersect(ray.a.y, ray.b.y, height, out t);
            return ray.Position(t);
        }

        public static int NextIndex(this int i, int count, int shift = 1) => (i + shift) % count;
        public static int PrevIndex(this int i, int count, int shift = 1) => shift > i ? i + count - (shift % count) : i - shift;

        public static LinkedListNode<T> GetPrevious<T>(this LinkedListNode<T> item) => item.Previous ?? item.List.Last;
        public static LinkedListNode<T> GetNext<T>(this LinkedListNode<T> item) => item.Next ?? item.List.First;
    }
    public static class VectorUtilsExtensions
    {
        public static float CrossXZ(Vector3 a, Vector3 b) => a.z * b.x - a.x * b.z;
        public static float NormalizeCrossXZ(Vector3 a, Vector3 b) => CrossXZ(NormalizeXZ(a), NormalizeXZ(b));
        public static float NormalizeDotXZ(Vector3 a, Vector3 b) => DotXZ(NormalizeXZ(a), NormalizeXZ(b));


        public static Vector2 XX(this Vector3 vector) => new Vector2(vector.x, vector.x);
        public static Vector2 XY(this Vector3 vector) => new Vector2(vector.x, vector.y);
        public static Vector2 XZ(this Vector3 vector) => new Vector2(vector.x, vector.z);

        public static Vector2 YX(this Vector3 vector) => new Vector2(vector.y, vector.x);
        public static Vector2 YY(this Vector3 vector) => new Vector2(vector.y, vector.y);
        public static Vector2 YZ(this Vector3 vector) => new Vector2(vector.y, vector.z);

        public static Vector2 ZX(this Vector3 vector) => new Vector2(vector.z, vector.x);
        public static Vector2 ZY(this Vector3 vector) => new Vector2(vector.z, vector.y);
        public static Vector2 ZZ(this Vector3 vector) => new Vector2(vector.z, vector.z);



        public static Vector2 XX(this Vector4 vector) => new Vector2(vector.x, vector.x);
        public static Vector2 XY(this Vector4 vector) => new Vector2(vector.x, vector.y);
        public static Vector2 XZ(this Vector4 vector) => new Vector2(vector.x, vector.z);
        public static Vector2 XW(this Vector4 vector) => new Vector2(vector.x, vector.w);

        public static Vector2 YX(this Vector4 vector) => new Vector2(vector.y, vector.x);
        public static Vector2 YY(this Vector4 vector) => new Vector2(vector.y, vector.y);
        public static Vector2 YZ(this Vector4 vector) => new Vector2(vector.y, vector.z);
        public static Vector2 YW(this Vector4 vector) => new Vector2(vector.y, vector.w);

        public static Vector2 ZX(this Vector4 vector) => new Vector2(vector.z, vector.x);
        public static Vector2 ZY(this Vector4 vector) => new Vector2(vector.z, vector.y);
        public static Vector2 ZZ(this Vector4 vector) => new Vector2(vector.z, vector.z);
        public static Vector2 ZW(this Vector4 vector) => new Vector2(vector.z, vector.w);

        public static Vector2 WX(this Vector4 vector) => new Vector2(vector.w, vector.x);
        public static Vector2 WY(this Vector4 vector) => new Vector2(vector.w, vector.y);
        public static Vector2 WZ(this Vector4 vector) => new Vector2(vector.w, vector.z);
        public static Vector2 WW(this Vector4 vector) => new Vector2(vector.w, vector.w);



        public static Vector3 XXX(this Vector4 vector) => new Vector3(vector.x, vector.x, vector.x);
        public static Vector3 XXY(this Vector4 vector) => new Vector3(vector.x, vector.x, vector.y);
        public static Vector3 XXZ(this Vector4 vector) => new Vector3(vector.x, vector.x, vector.z);
        public static Vector3 XXW(this Vector4 vector) => new Vector3(vector.x, vector.x, vector.w);

        public static Vector3 XYX(this Vector4 vector) => new Vector3(vector.x, vector.y, vector.x);
        public static Vector3 XYY(this Vector4 vector) => new Vector3(vector.x, vector.y, vector.y);
        public static Vector3 XYZ(this Vector4 vector) => new Vector3(vector.x, vector.y, vector.z);
        public static Vector3 XYW(this Vector4 vector) => new Vector3(vector.x, vector.y, vector.w);

        public static Vector3 XZX(this Vector4 vector) => new Vector3(vector.x, vector.z, vector.x);
        public static Vector3 XZY(this Vector4 vector) => new Vector3(vector.x, vector.z, vector.y);
        public static Vector3 XZZ(this Vector4 vector) => new Vector3(vector.x, vector.z, vector.z);
        public static Vector3 XZW(this Vector4 vector) => new Vector3(vector.x, vector.z, vector.w);

        public static Vector3 XWX(this Vector4 vector) => new Vector3(vector.x, vector.w, vector.x);
        public static Vector3 XWY(this Vector4 vector) => new Vector3(vector.x, vector.w, vector.y);
        public static Vector3 XWZ(this Vector4 vector) => new Vector3(vector.x, vector.w, vector.z);
        public static Vector3 XWW(this Vector4 vector) => new Vector3(vector.x, vector.w, vector.w);

        public static Vector3 YXX(this Vector4 vector) => new Vector3(vector.y, vector.x, vector.x);
        public static Vector3 YXY(this Vector4 vector) => new Vector3(vector.y, vector.x, vector.y);
        public static Vector3 YXZ(this Vector4 vector) => new Vector3(vector.y, vector.x, vector.z);
        public static Vector3 YXW(this Vector4 vector) => new Vector3(vector.y, vector.x, vector.w);

        public static Vector3 YYX(this Vector4 vector) => new Vector3(vector.y, vector.y, vector.x);
        public static Vector3 YYY(this Vector4 vector) => new Vector3(vector.y, vector.y, vector.y);
        public static Vector3 YYZ(this Vector4 vector) => new Vector3(vector.y, vector.y, vector.z);
        public static Vector3 YYW(this Vector4 vector) => new Vector3(vector.y, vector.y, vector.w);

        public static Vector3 YZX(this Vector4 vector) => new Vector3(vector.y, vector.z, vector.x);
        public static Vector3 YZY(this Vector4 vector) => new Vector3(vector.y, vector.z, vector.y);
        public static Vector3 YZZ(this Vector4 vector) => new Vector3(vector.y, vector.z, vector.z);
        public static Vector3 YZW(this Vector4 vector) => new Vector3(vector.y, vector.z, vector.w);

        public static Vector3 YWX(this Vector4 vector) => new Vector3(vector.y, vector.w, vector.x);
        public static Vector3 YWY(this Vector4 vector) => new Vector3(vector.y, vector.w, vector.y);
        public static Vector3 YWZ(this Vector4 vector) => new Vector3(vector.y, vector.w, vector.z);
        public static Vector3 YWW(this Vector4 vector) => new Vector3(vector.y, vector.w, vector.w);

        public static Vector3 ZXX(this Vector4 vector) => new Vector3(vector.z, vector.x, vector.x);
        public static Vector3 ZXY(this Vector4 vector) => new Vector3(vector.z, vector.x, vector.y);
        public static Vector3 ZXZ(this Vector4 vector) => new Vector3(vector.z, vector.x, vector.z);
        public static Vector3 ZXW(this Vector4 vector) => new Vector3(vector.z, vector.x, vector.w);

        public static Vector3 ZYX(this Vector4 vector) => new Vector3(vector.z, vector.y, vector.x);
        public static Vector3 ZYY(this Vector4 vector) => new Vector3(vector.z, vector.y, vector.y);
        public static Vector3 ZYZ(this Vector4 vector) => new Vector3(vector.z, vector.y, vector.z);
        public static Vector3 ZYW(this Vector4 vector) => new Vector3(vector.z, vector.y, vector.w);

        public static Vector3 ZZX(this Vector4 vector) => new Vector3(vector.z, vector.z, vector.x);
        public static Vector3 ZZY(this Vector4 vector) => new Vector3(vector.z, vector.z, vector.y);
        public static Vector3 ZZZ(this Vector4 vector) => new Vector3(vector.z, vector.z, vector.z);
        public static Vector3 ZZW(this Vector4 vector) => new Vector3(vector.z, vector.z, vector.w);

        public static Vector3 ZWX(this Vector4 vector) => new Vector3(vector.z, vector.w, vector.x);
        public static Vector3 ZWY(this Vector4 vector) => new Vector3(vector.z, vector.w, vector.y);
        public static Vector3 ZWZ(this Vector4 vector) => new Vector3(vector.z, vector.w, vector.z);
        public static Vector3 ZWW(this Vector4 vector) => new Vector3(vector.z, vector.w, vector.w);

        public static Vector3 WXX(this Vector4 vector) => new Vector3(vector.w, vector.x, vector.x);
        public static Vector3 WXY(this Vector4 vector) => new Vector3(vector.w, vector.x, vector.y);
        public static Vector3 WXZ(this Vector4 vector) => new Vector3(vector.w, vector.x, vector.z);
        public static Vector3 WXW(this Vector4 vector) => new Vector3(vector.w, vector.x, vector.w);

        public static Vector3 WYX(this Vector4 vector) => new Vector3(vector.w, vector.y, vector.x);
        public static Vector3 WYY(this Vector4 vector) => new Vector3(vector.w, vector.y, vector.y);
        public static Vector3 WYZ(this Vector4 vector) => new Vector3(vector.w, vector.y, vector.z);
        public static Vector3 WYW(this Vector4 vector) => new Vector3(vector.w, vector.y, vector.w);

        public static Vector3 WZX(this Vector4 vector) => new Vector3(vector.w, vector.z, vector.x);
        public static Vector3 WZY(this Vector4 vector) => new Vector3(vector.w, vector.z, vector.y);
        public static Vector3 WZZ(this Vector4 vector) => new Vector3(vector.w, vector.z, vector.z);
        public static Vector3 WZW(this Vector4 vector) => new Vector3(vector.w, vector.z, vector.w);

        public static Vector3 WWX(this Vector4 vector) => new Vector3(vector.w, vector.w, vector.x);
        public static Vector3 WWY(this Vector4 vector) => new Vector3(vector.w, vector.w, vector.y);
        public static Vector3 WWZ(this Vector4 vector) => new Vector3(vector.w, vector.w, vector.z);
        public static Vector3 WWW(this Vector4 vector) => new Vector3(vector.w, vector.w, vector.w);
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
