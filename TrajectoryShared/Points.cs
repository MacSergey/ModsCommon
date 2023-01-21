using System;
using System.Linq;
using UnityEngine;
using static ColossalFramework.Math.VectorUtils;

namespace ModsCommon.Utilities
{
    public struct TrajectoryPoints
    {
        ITrajectory[] Trajectories { get; }
        int[] Counts { get; }
        int[] EndIndixes { get; }
        Vector2?[] Points { get; }
        public int Length => Points.Length;

        public Vector3 this[int index]
        {
            get
            {
                if (Points[index] == null)
                {
                    var t = GetT(index);
                    var i = Math.Min((int)t, Trajectories.Length - 1);
                    Points[index] = XZ(Trajectories[i].Position(t - i));
                }
                return Points[index].Value;
            }
        }

        public TrajectoryPoints(params ITrajectory[] trajectories)
        {
            Trajectories = trajectories;
            Counts = new int[Trajectories.Length];
            EndIndixes = new int[Trajectories.Length];

            for (var i = 0; i < Trajectories.Length; i += 1)
            {
                var length = Trajectories[i].Length;
                Counts[i] = Math.Max((int)(Mathf.Clamp(length, 0f, 200f) * 20), 2);
                EndIndixes[i] = EndIndixes[Math.Max(i - 1, 0)] + Counts[i];
            }

            Points = new Vector2?[EndIndixes.Last()];
        }

        private float GetT(int index)
        {
            var i = 0;
            while (index > EndIndixes[i])
                i += 1;

            return 1f / Counts[i] * (Counts[i] - EndIndixes[i] + index) + i;
        }
        public float Find(int startIndex, float distance, out int findIndex)
        {
            distance *= distance;

            findIndex = startIndex;
            var endIndex = EndIndixes.Last() - 1;
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
            return GetT(findIndex);
        }
    }
}
