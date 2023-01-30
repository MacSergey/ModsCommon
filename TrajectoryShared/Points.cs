using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static ColossalFramework.Math.VectorUtils;

namespace ModsCommon.Utilities
{
    public readonly struct TrajectoryPoints
    {
        ITrajectory Trajectory { get; }
        Vector2?[] Points { get; }
        public int Length => Points.Length;

        public Vector3 this[int index]
        {
            get
            {
                if (Points[index] == null)
                {
                    var t = 1f / Points.Length * index;
                    Points[index] = XZ(Trajectory.Position(t));
                }
                return Points[index].Value;
            }
        }

        public TrajectoryPoints(ITrajectory trajectory)
        {
            Trajectory = trajectory;

            var length = trajectory.Length;
            var count = Math.Max((int)(Mathf.Clamp(length, 0f, 200f) * 20), 2);

            Points = new Vector2?[count];
        }
        public float Find(int startIndex, float distance, out int findIndex)
        {
            distance *= distance;

            findIndex = startIndex;
            var endIndex = Points.Length - 1;
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
            return 1f / Points.Length * findIndex;
        }
    }
}
