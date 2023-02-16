using ColossalFramework.Math;
using UnityEngine;

namespace ModsCommon.Utilities
{
    public enum TrajectoryType
    {
        Line,
        Bezier,
        Combined
    }
    public interface ITrajectory : IOverlay
    {
        TrajectoryType TrajectoryType { get; }
        float Length { get; }
        float Magnitude { get; }
        float DeltaAngle { get; }
        Vector3 Direction { get; }
        Vector3 StartDirection { get; }
        Vector3 EndDirection { get; }
        Vector3 StartPosition { get; }
        Vector3 EndPosition { get; }
        ITrajectory Cut(float t0, float t1);
        void Divide(out ITrajectory trajectory1, out ITrajectory trajectory2);
        Vector3 Tangent(float t);
        Vector3 Position(float t);
        float Travel(float distance);
        float Travel(float start, float distance);
        float Distance(float from = 0f, float to = 1f);
        ITrajectory Invert();
        ITrajectory Shift(float start, float end);

        public bool IsZero { get; }

        public Vector3 GetHitPosition(Segment3 ray, out float rayT, out float trajectoryT, out Vector3 position);

        public Vector3 GetClosestPosition(Vector3 hitPos, out float closestT);
        public Vector3 GetDirectionPosition(Vector3 hitPos, out float closestT);
        public void GetClosestPositionAndDirection(Vector3 hitPos, out Vector3 position, out Vector3 direction, out float closestT);
        public float GetLength(float minAngleDelta = 10, int depth = 5);
#if DEBUG
        public string Table { get; }
#endif
    }
}
