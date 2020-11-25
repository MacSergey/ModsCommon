using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.SceneManagement;

namespace ModsCommon.Utilities
{
    public static class ItemsExtension
    {
        public static NetManager NetManager => Singleton<NetManager>.instance;
        public static PropManager PropManager => Singleton<PropManager>.instance;
        public static IEnumerable<NetSegment> Segments(this NetNode node)
        {
            for (var i = 0; i < 8; i += 1)
            {
                var segment = node.GetSegment(i);
                if (segment != 0)
                    yield return GetSegment(segment);
            }
        }
        public static IEnumerable<ushort> SegmentsId(this NetNode node)
        {
            for (var i = 0; i < 8; i += 1)
            {
                var segment = node.GetSegment(i);
                if (segment != 0)
                    yield return segment;
            }
        }
        public static IEnumerable<ushort> NodesID(this NetSegment segment)
        {
            yield return segment.m_startNode;
            yield return segment.m_endNode;
        }
        public static IEnumerable<NetLane> GetLanes(this NetSegment segment)
        {
            NetLane lane;
            for (var laneId = segment.m_lanes; laneId != 0; laneId = lane.m_nextLane)
            {
                lane = GetLane(laneId);
                yield return lane;
            }
        }
        public static IEnumerable<uint> GetLanesId(this NetSegment segment)
        {
            for (var laneId = segment.m_lanes; laneId != 0; laneId = GetLane(laneId).m_nextLane)
            {
                yield return laneId;
            }
        }

        public static bool IsInvert(this NetSegment segment) => (segment.m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.Invert;

        public static VehicleInfo.VehicleType DriveType { get; } =
            VehicleInfo.VehicleType.Car |
            VehicleInfo.VehicleType.Bicycle |
            VehicleInfo.VehicleType.Tram |
            VehicleInfo.VehicleType.Trolleybus |
            VehicleInfo.VehicleType.Plane;
        public static bool IsDriveLane(this NetInfo.Lane info) => (info.m_vehicleType & DriveType) != VehicleInfo.VehicleType.None;

        public static NetNode GetNode(this ushort nodeId) => NetManager.m_nodes.m_buffer[nodeId];
        public static NetSegment GetSegment(this ushort segmentId) => NetManager.m_segments.m_buffer[segmentId];
        public static NetLane GetLane(this uint laneId) => NetManager.m_lanes.m_buffer[laneId];

        public static bool InGame => SceneManager.GetActiveScene().name is string scene && scene != "MainMenu" && scene != "IntroScreen";
    }
}
