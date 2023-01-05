using ColossalFramework.Math;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ColossalFramework.Math.VectorUtils;

namespace ModsCommon.Utilities
{
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
