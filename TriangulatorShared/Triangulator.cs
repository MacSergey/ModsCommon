using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModsCommon.Utilities
{
    public class Triangulator
    {
        public static int[] TriangulateSimple(IEnumerable<ITrajectory> trajectories, out Vector3[] points, float minAngle = 10f, float minLength = 1f, float maxLength = 50f)
        {
            List<ITrajectory> split = new List<ITrajectory>();
            foreach (var trajectory in trajectories)
            {
                SplitTrajectory(0, trajectory, trajectory.DeltaAngle, minAngle, minLength, maxLength, split);
            }
            var direction = trajectories.GetDirection();
            points = split.Select(i => i.StartPosition).ToArray();
            return TriangulateSimple(points, direction);
        }
        private static void SplitTrajectory(int depth, ITrajectory trajectory, float deltaAngle, float minAngle, float minLength, float maxLength, List<ITrajectory> result)
        {
            var length = trajectory.Magnitude;

            var needDivide = (deltaAngle > minAngle && length >= minLength) || length > maxLength;
            if (depth < 5 && (needDivide || depth == 0))
            {
                trajectory.Divide(out ITrajectory first, out ITrajectory second);
                var firstDeltaAngle = first.DeltaAngle;
                var secondDeltaAngle = second.DeltaAngle;

                if (needDivide || deltaAngle > minAngle || (firstDeltaAngle + secondDeltaAngle) > minAngle)
                {
                    SplitTrajectory(depth + 1, first, firstDeltaAngle, minAngle, minLength, maxLength, result);
                    SplitTrajectory(depth + 1, second, secondDeltaAngle, minAngle, minLength, maxLength, result);

                    return;
                }
            }

            result.Add(trajectory);
        }

        public static int[] TriangulateSimple(IEnumerable<Vector3> points, TrajectoryHelper.Direction direction)
        {
            var triangulator = new Triangulator(points, direction);
            var triangles = triangulator.TriangulateSimple();
            return triangles.SelectMany(t => t.GetVertices(direction)).ToArray();
        }

        private TrajectoryHelper.Direction Direction { get; }
        private LinkedList<Vertex> Vertices { get; }
        private Dictionary<int, LinkedListNode<Vertex>> Ears { get; } = new Dictionary<int, LinkedListNode<Vertex>>();

        private Triangulator(IEnumerable<Vector3> points, TrajectoryHelper.Direction direction)
        {
            Vertices = new LinkedList<Vertex>(points.Select((p, i) => new Vertex(p, i)));
            Direction = direction;
        }
        private List<Triangle> TriangulateSimple()
        {
            foreach (var vertex in EnumerateVertex())
            {
                SetConvex(vertex);
                SetHeight(vertex);
                SetEar(vertex);
            }

            var triangles = new List<Triangle>();
            var iter = 0;
            while (true)
            {
                if (Ears.Count == 0)
                    break;

                var vertex = default(LinkedListNode<Vertex>);
                var minH = float.MaxValue;
                foreach(var ear in Ears.Values)
                {
                    if(ear.Value.deltaH < minH)
                    {
                        vertex = ear;
                        minH = ear.Value.deltaH;
                    }
                }

                var prev = vertex.GetPrevious();
                var next = vertex.GetNext();

                var triangle = new Triangle(next.Value.index, vertex.Value.index, prev.Value.index);
                triangles.Add(triangle);
                Ears.Remove(vertex.Value.index);
                Vertices.Remove(vertex);

                if (Vertices.Count < 3)
                    break;

                SetConvex(prev);
                SetConvex(next);
                SetHeight(prev);
                SetHeight(next);

                SetEar(prev);
                SetEar(next);
                iter += 1;
            }

            return triangles;
        }
        private void SetConvex(LinkedListNode<Vertex> vertex)
        {
            if (!vertex.Value.isConvex)
                vertex.Value = vertex.Value.SetConvex(vertex.GetPrevious().Value, vertex.GetNext().Value, Direction);
        }
        private void SetHeight(LinkedListNode<Vertex> vertex)
        {
            vertex.Value = vertex.Value.SetHeight(vertex.GetPrevious().Value, vertex.GetNext().Value);
        }

        private void SetEar(LinkedListNode<Vertex> vertex)
        {
            var prev = vertex.GetPrevious();
            var next = vertex.GetNext();

            if (vertex.Value.isConvex)
            {
                if (!EnumerateVertex(next, prev).Any(p => PointInTriangle(prev.Value.position, vertex.Value.position, next.Value.position, p.Value.position)))
                {
                    Ears[vertex.Value.index] = vertex;
                    return;
                }
            }

            Ears.Remove(vertex.Value.index);
        }

        private IEnumerable<LinkedListNode<Vertex>> EnumerateVertex() => EnumerateVertex(Vertices.First);
        private IEnumerable<LinkedListNode<Vertex>> EnumerateVertex(LinkedListNode<Vertex> startFrom)
        {
            if (startFrom != null)
                yield return startFrom;

            for (var vertex = startFrom.GetNext(); vertex != startFrom; vertex = vertex.GetNext())
                yield return vertex;
        }
        private IEnumerable<LinkedListNode<Vertex>> EnumerateVertex(LinkedListNode<Vertex> from, LinkedListNode<Vertex> to)
        {
            for (var vertex = from.GetNext(); vertex != to; vertex = vertex.GetNext())
                yield return vertex;
        }


        private bool PointInTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
        {
            float area = 0.5f * (-b.z * c.x + a.z * (-b.x + c.x) + a.x * (b.z - c.z) + b.x * c.z);
            float s = 1 / (2 * area) * (a.z * c.x - a.x * c.z + (c.z - a.z) * p.x + (a.x - c.x) * p.z);
            float t = 1 / (2 * area) * (a.x * b.z - a.z * b.x + (a.z - b.z) * p.x + (b.x - a.x) * p.z);
            return s >= 0 && t >= 0 && (s + t) <= 1;

        }

        public readonly struct Vertex
        {
            public readonly Vector3 position;
            public readonly int index;
            public readonly bool isConvex;
            public readonly float deltaH;

            private Vertex(Vector3 position, int index, bool isConvex, float deltaH)
            {
                this.position = position;
                this.index = index;
                this.isConvex = isConvex;
                this.deltaH = deltaH;
            }
            public Vertex(Vector3 position, int index) : this(position, index, default, default) { }

            public Vertex SetConvex(Vertex prev, Vertex next, TrajectoryHelper.Direction direction)
            {
                var a = position - prev.position;
                var b = next.position - position;

                var sign = (int)Mathf.Sign(a.x * b.z - a.z * b.x);
                var isConvex = sign >= 0 ^ direction == TrajectoryHelper.Direction.ClockWise;

                return new Vertex(position, index, isConvex, deltaH);
            }
            public Vertex SetHeight(Vertex prev, Vertex next)
            {
                var min = Mathf.Min(position.y, Mathf.Min(prev.position.y, next.position.y));
                var max = Mathf.Max(position.y, Mathf.Max(prev.position.y, next.position.y));
                return new Vertex(position, index, isConvex, max - min);
            }

            public override string ToString() => $"{index}:{position} ({(isConvex ? "Convex" : "Reflex")})";
        }
    }
}


