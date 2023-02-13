using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModsCommon.Utilities
{
    public readonly struct Triangle
    {
        public readonly int a;
        public readonly int b;
        public readonly int c;

        public Triangle(int a, int b, int c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public IEnumerable<int> GetVertices(TrajectoryHelper.Direction direction)
        {
            if (direction == TrajectoryHelper.Direction.ClockWise)
            {
                yield return c;
                yield return b;
                yield return a;
            }
            else
            {
                yield return a;
                yield return b;
                yield return c;
            }
        }

        public override string ToString() => $"{a}-{b}-{c}";
    }

    public class Polygon : IEnumerable<Area>
    {
        private Vector3[] Vertices { get; }
        private List<Area> Areas { get; }
        public int Count => Areas.Count;
        public Area this[int index] => Areas[index];

        public Polygon(Vector3[] vertices, List<Triangle> triangles)
        {
            Vertices = vertices;
            Areas = new List<Area>(triangles.Count);

            for (var i = 0; i < triangles.Count; i += 1)
            {
                Areas.Add(new Area(this, Count, triangles[i]));
            }
        }
        public Polygon(Vector3[] vertices, int[] triangles)
        {
            Vertices = vertices;
            Areas = new List<Area>(triangles.Length / 3);

            for (var i = 2; i < triangles.Length; i += 3)
            {
                Areas.Add(new Area(this, Count, triangles[i - 2], triangles[i - 1], triangles[i]));
            }
        }

        public Vector3 GetPosition(int index) => Vertices[index];

        public void Arrange(int maxCount, float maxDeltaH)
        {
            for (var i = 0; i < Count - 1; i += 1)
            {
                var max1 = Areas[i].Max;
                var min1 = Areas[i].Min;

                for (var j = i + 1; j < Count; j += 1)
                {
                    if (Areas[i].Count + Areas[j].Count > maxCount)
                        continue;

                    var max2 = Areas[j].Max;
                    var min2 = Areas[j].Min;

                    var max = Math.Max(max1.y, max2.y);
                    var min = Math.Min(min1.y, min2.y);
                    if (max - min > maxDeltaH)
                        continue;

                    if (!IsConnected(i, j, out int side1Start, out int side1End, out int side2Start, out int side2End))
                        continue;

                    Connect(i, j, side1Start, side1End, side2Start, side2End);
                    i -= 1;
                    break;
                }
            }
        }
        public bool IsConnected(int area1, int area2, out int side1Start, out int side1End, out int side2Start, out int side2End)
        {
            var side1 = new List<int>();
            var side2 = new List<int>();

            for (var i = 0; i < Areas[area1].Sides.Length; i += 1)
            {
                for (var j = 0; j < Areas[area2].Sides.Length; j += 1)
                {
                    if (Areas[area1].Sides[i] == Areas[area2].Sides[j])
                    {
                        Areas[area1].Sides[i].ConnectedTo = Areas[area2].Index;
                        Areas[area2].Sides[j].ConnectedTo = Areas[area1].Index;

                        side1.Add(i);
                        side2.Add(j);
                    }
                }
            }

            if (side1.Count != 0 && side2.Count != 0)
            {
                side1Start = side1[0];
                side1End = side1[side1.Count - 1];
                side2Start = side2[side2.Count - 1];
                side2End = side2[0];
                return true;
            }
            else
            {
                side1Start = default;
                side1End = default;
                side2Start = default;
                side2End = default;
                return false;
            }
        }
        public void Connect(int area1, int area2, int side1Start, int side1End, int side2Start, int side2End)
        {
            var sides = new List<Side>();

            for (var i = (side1End + 1) % Areas[area1].Count; i != side1Start; i = (i + 1) % Areas[area1].Count)
                sides.Add(Areas[area1].Sides[i]);

            for (var i = (side2End + 1) % Areas[area2].Count; i != side2Start; i = (i + 1) % Areas[area2].Count)
                sides.Add(Areas[area2].Sides[i]);

            if (area1 < area2)
            {
                Areas.RemoveAt(area2);
                Areas.RemoveAt(area1);
            }
            else
            {
                Areas.RemoveAt(area1);
                Areas.RemoveAt(area2);
            }

            var index = Count == 0 ? 0 : Areas.Max(a => a.Index) + 1;
            var newArea = new Area(this, index, sides.ToArray());
            Areas.Add(newArea);
        }

        public IEnumerator<Area> GetEnumerator() => Areas.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Areas.GetEnumerator();

        public override string ToString() => $"{Count} Areas";
    }

    public struct Area
    {
        public Polygon Polygon { get; }
        public int Index { get; }
        public Side[] Sides { get; }
        public Side this[int index] => Sides[index];
        public int Count => Sides.Length;
        public IEnumerable<int> ConnectedTo
        {
            get
            {
                foreach (var side in Sides)
                {
                    if (side.ConnectedTo != null)
                        yield return side.ConnectedTo.Value;
                }
            }
        }

        public Vector3 Center => (Min + Max) * 0.5f;
        public Vector3 Min
        {
            get
            {
                if (Count == 0)
                    return Vector3.zero;
                else
                {
                    var min = Sides[0].Start.Position;
                    for (var i = 1; i < Count; i += 1)
                        min = Vector3.Min(min, Sides[i].Start.Position);
                    return min;
                }
            }
        }
        public Vector3 Max
        {
            get
            {
                if (Count == 0)
                    return Vector3.zero;
                else
                {
                    var max = Sides[0].Start.Position;
                    for (var i = 1; i < Count; i += 1)
                        max = Vector3.Max(max, Sides[i].Start.Position);
                    return max;
                }
            }
        }
        public IEnumerable<Vertex3> Vertices => Sides.Select(s => s.Start);
        public IEnumerable<Vector3> Positions => Sides.Select(s => s.Start.Position);

        public Area(Polygon polygon, int index, int a, int b, int c)
        {
            Polygon = polygon;
            Index = index;
            Sides = new Side[]
            {
                new Side(polygon, index, 0, a, b),
                new Side(polygon, index, 1, b, c),
                new Side(polygon, index, 2, c, a),
            };
        }
        public Area(Polygon polygon, int index, Triangle triangle) : this(polygon, index, triangle.a, triangle.b, triangle.c)
        {

        }
        public Area(Polygon polygon, int index, Side[] sides)
        {
            Polygon = polygon;
            Index = index;
            Sides = sides;
            Reindex();
        }

        public void Reindex()
        {
            for (var i = 0; i < Sides.Length; i += 1)
                Sides[i].Index = i;
        }

        public override string ToString() => $"Area #{Index}: {Count} sides";
    }
    public struct Side
    {
        public Polygon Polygon { get; }
        public int AreaIndex { get; }
        public int Index { get; set; }
        public Vertex3 Start { get; }
        public Vertex3 End { get; }
        public int? ConnectedTo { get; set; }

        public Side(Polygon polygon, int areaIndex, int index, int startIndex, int endIndex, int? connectedTo = null)
        {
            Polygon = polygon;
            AreaIndex = areaIndex;
            Index = index;
            Start = new Vertex3(polygon, startIndex);
            End = new Vertex3(polygon, endIndex);
            ConnectedTo = connectedTo;
        }

        public static bool operator ==(Side a, Side b)
        {
            if (a.AreaIndex == b.AreaIndex)
                return a.Index == b.Index;
            else
                return a.Start.Index == b.End.Index && a.End.Index == b.Start.Index;
        }
        public static bool operator !=(Side a, Side b)
        {
            if (a.AreaIndex == b.AreaIndex)
                return a.Index != b.Index;
            else
                return a.Start.Index != b.End.Index || a.End.Index != b.Start.Index;
        }

        public override string ToString() => $"Side #{Index} of Area #{AreaIndex}: start={Start}, end={End}";
    }
    public struct Vertex3
    {
        public Polygon Polygon { get; }
        public int Index { get; set; }

        public Vector3 Position => Polygon.GetPosition(Index);

        public Vertex3(Polygon polygon, int index)
        {
            Polygon = polygon;
            Index = index;
        }
        public override string ToString() => $"Vertex #{Index}: pos={Position}";
    }
}
