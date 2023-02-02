using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ColossalFramework.Math.VectorUtils;

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

        public static int[] TriangulateSimple(IEnumerable<Vector3> points, TrajectoryHelper.Direction direction) => TriangulateSimple(points.Select(p => XZ(p)), direction);
        public static int[] TriangulateSimple(IEnumerable<Vector2> points, TrajectoryHelper.Direction direction)
        {
            var triangulator = new Triangulator(points, direction);
            var triangles = triangulator.TriangulateSimple();
            return triangles.SelectMany(t => t.GetVertices(direction)).ToArray();
        }
        public static int[] Triangulate(IEnumerable<Vector3> points, TrajectoryHelper.Direction direction) => Triangulate(points.Select(p => XZ(p)), direction);
        public static int[] Triangulate(IEnumerable<Vector2> points, TrajectoryHelper.Direction direction)
        {
            var triangulator = new Triangulator(points, direction);
            var triangles = triangulator.Triangulate();
            return triangles.SelectMany(t => t.GetVertices(direction)).ToArray();
        }

        private TrajectoryHelper.Direction Direction { get; }
        private LinkedList<Vertex2> Vertices { get; }
        private HashSet<LinkedListNode<Vertex2>> Ears { get; } = new HashSet<LinkedListNode<Vertex2>>();

        private Triangulator(IEnumerable<Vector2> points, TrajectoryHelper.Direction direction)
        {
            Vertices = new LinkedList<Vertex2>(points.Select((p, i) => new Vertex2(p, i)));
            Direction = direction;
        }
        private List<Triangle> TriangulateSimple()
        {
            foreach (var vertex in EnumerateVertex())
            {
                SetConvex(vertex);
                SetEar(vertex);
            }

            var triangles = new List<Triangle>();
            var iter = 0;
            while (true)
            {
                if ((iter % 2 == 0 ? Ears.FirstOrDefault() : Ears.LastOrDefault()) is not LinkedListNode<Vertex2> vertex)
                    break;

                var prev = vertex.GetPrevious();
                var next = vertex.GetNext();

                var triangle = new Triangle(next.Value.index, vertex.Value.index, prev.Value.index);
                triangles.Add(triangle);
                Ears.Remove(vertex);
                Vertices.Remove(vertex);

                if (Vertices.Count < 3)
                    break;

                SetConvex(prev);
                SetConvex(next);

                SetEar(prev);
                SetEar(next);
                iter += 1;
            }

            return triangles;
        }
        private List<Triangle> Triangulate()
        {
            foreach (var vertex in EnumerateVertex())
            {
                SetConvex(vertex);
                SetEar(vertex);
            }

            var triangles = new List<Triangle>();
            while (true)
            {
                if (Ears.Count == 0)
                    return null;

                var vertex = default(LinkedListNode<Vertex2>);
                var min = float.MaxValue;
                foreach (var current in Ears)
                {
                    var prev = current.GetPrevious();
                    var next = current.GetNext();

                    var dist12 = (prev.Value.position - current.Value.position).magnitude;
                    var dist23 = (current.Value.position - next.Value.position).magnitude;
                    var dist31 = (next.Value.position - prev.Value.position).magnitude;

                    var avg = (dist12 + dist23 + dist31) / 3f;

                    dist12 = Mathf.Abs(dist12 - avg);
                    dist23 = Mathf.Abs(dist23 - avg);
                    dist31 = Mathf.Abs(dist31 - avg);

                    var dist = dist12 * dist12 + dist23 * dist23 + dist31 * dist31;

                    if (dist < min)
                    {
                        min = dist;
                        vertex = current;
                    }
                }

                {
                    var prev = vertex.GetPrevious();
                    var next = vertex.GetNext();

                    var triangle = new Triangle(next.Value.index, vertex.Value.index, prev.Value.index);
                    triangles.Add(triangle);
                    Ears.Remove(vertex);
                    Vertices.Remove(vertex);

                    if (Vertices.Count < 3)
                        return triangles;

                    SetConvex(prev);
                    SetConvex(next);

                    SetEar(prev);
                    SetEar(next);
                }
            }
        }
        private void SetConvex(LinkedListNode<Vertex2> vertex)
        {
            if (!vertex.Value.isConvex)
                vertex.Value = vertex.Value.SetConvex(vertex.GetPrevious().Value, vertex.GetNext().Value, Direction);
        }

        private void SetEar(LinkedListNode<Vertex2> vertex)
        {
            if (vertex.Value.isConvex)
            {
                var prev = vertex.GetPrevious();
                var next = vertex.GetNext();
                if (!EnumerateVertex(next, prev).Any(p => PointInTriangle(prev.Value.position, vertex.Value.position, next.Value.position, p.Value.position)))
                {
                    Ears.Add(vertex);
                    return;
                }
            }

            Ears.Remove(vertex);
        }

        private IEnumerable<LinkedListNode<Vertex2>> EnumerateVertex() => EnumerateVertex(Vertices.First);
        private IEnumerable<LinkedListNode<Vertex2>> EnumerateVertex(LinkedListNode<Vertex2> startFrom)
        {
            if (startFrom != null)
                yield return startFrom;

            for (var vertex = startFrom.GetNext(); vertex != startFrom; vertex = vertex.GetNext())
                yield return vertex;
        }
        private IEnumerable<LinkedListNode<Vertex2>> EnumerateVertex(LinkedListNode<Vertex2> from, LinkedListNode<Vertex2> to)
        {
            for (var vertex = from.GetNext(); vertex != to; vertex = vertex.GetNext())
                yield return vertex;
        }


        private bool PointInTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
        {
            float area = 0.5f * (-b.y * c.x + a.y * (-b.x + c.x) + a.x * (b.y - c.y) + b.x * c.y);
            float s = 1 / (2 * area) * (a.y * c.x - a.x * c.y + (c.y - a.y) * p.x + (a.x - c.x) * p.y);
            float t = 1 / (2 * area) * (a.x * b.y - a.y * b.x + (a.y - b.y) * p.x + (b.x - a.x) * p.y);
            return s >= 0 && t >= 0 && (s + t) <= 1;

        }
    }
}


