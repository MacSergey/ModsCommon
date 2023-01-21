using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModsCommon.Utilities
{
    public class TrajectoryBound : IOverlay
    {
        private static float Coef { get; } = Mathf.Sin(45 * Mathf.Deg2Rad);
        public ITrajectory Trajectory { get; }
        public float Size { get; }
        private List<Bounds> BoundsList { get; } = new List<Bounds>();
        public IEnumerable<Bounds> Bounds => BoundsList;
        public TrajectoryBound(ITrajectory trajectory, float size)
        {
            Trajectory = trajectory;
            Size = size;
            CalculateBounds();
        }

        private void CalculateBounds()
        {
            if (Trajectory == null)
                return;

            var size = Size * Coef;
            var t = 0f;
            while (t < 1f)
            {
                t = Trajectory.Travel(t, size / 2);
                BoundsList.Add(new Bounds(Trajectory.Position(t), Vector3.one * size));
            }
            BoundsList.Add(new Bounds(Trajectory.Position(0), Vector3.one * Size));
            BoundsList.Add(new Bounds(Trajectory.Position(1), Vector3.one * Size));
        }

        public bool IntersectRay(Ray ray) => BoundsList.Any(b => b.IntersectRay(ray));
        public bool Intersects(Bounds bounds) => BoundsList.Any(b => b.Intersects(bounds));

        public void Render(OverlayData data) => Trajectory.Render(data);
    }
}
