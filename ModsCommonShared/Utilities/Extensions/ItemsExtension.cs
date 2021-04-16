using ColossalFramework;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace ModsCommon.Utilities
{
    public static class ItemsExtension
    {
        public static NetManager NetManager => Singleton<NetManager>.instance;
        public static PropManager PropManager => Singleton<PropManager>.instance;
        public static TerrainManager TerrainManager => Singleton<TerrainManager>.instance;
        public static IEnumerable<NetSegment> Segments(this NetNode node)
        {
            for (var i = 0; i < 8; i += 1)
            {
                var segment = node.GetSegment(i);
                if (segment != 0)
                    yield return GetSegment(segment);
            }
        }
        public static IEnumerable<ushort> SegmentIds(this NetNode node)
        {
            for (var i = 0; i < 8; i += 1)
            {
                var segment = node.GetSegment(i);
                if (segment != 0)
                    yield return segment;
            }
        }
        public static IEnumerable<NetNode> Nodes(this NetSegment segment)
        {
            yield return segment.m_startNode.GetNode();
            yield return segment.m_endNode.GetNode();
        }
        public static IEnumerable<ushort> NodeIds(this NetSegment segment)
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
                yield return laneId;
        }
        public static IEnumerable<uint> GetLanesId(this uint laneId)
        {
            for (; laneId != 0; laneId = GetLane(laneId).m_nextLane)
                yield return laneId;
        }

        public static bool IsInvert(this NetSegment segment) => (segment.m_flags & NetSegment.Flags.Invert) != 0;

        public static VehicleInfo.VehicleType DriveType { get; } =
                    VehicleInfo.VehicleType.Car |
                    VehicleInfo.VehicleType.Bicycle |
                    VehicleInfo.VehicleType.Tram |
                    VehicleInfo.VehicleType.Trolleybus |
                    VehicleInfo.VehicleType.Plane;
        public static bool IsDriveLane(this NetInfo.Lane info) => (info.m_vehicleType & DriveType) != VehicleInfo.VehicleType.None;

        public static NetNode GetNode(this ushort nodeId) => NetManager.m_nodes.m_buffer[nodeId];
        public static ref NetNode GetNodeRef(this ushort nodeId) => ref NetManager.m_nodes.m_buffer[nodeId];
        public static NetSegment GetSegment(this ushort segmentId) => NetManager.m_segments.m_buffer[segmentId];
        public static ref NetSegment GetSegmentRef(this ushort segmentId) => ref NetManager.m_segments.m_buffer[segmentId];
        public static NetLane GetLane(this uint laneId) => NetManager.m_lanes.m_buffer[laneId];
        public static ref NetLane GetLaneRef(this uint laneId) => ref NetManager.m_lanes.m_buffer[laneId];
        public static ushort GetNode(this NetSegment segment, bool isStartNode) => isStartNode ? segment.m_startNode : segment.m_endNode;

        public static bool IsStartNode(this NetSegment segment, ushort nodeId) => segment.m_startNode == nodeId;
        public static bool IsValid(this NetNode node) => node.Info != null && node.m_flags.CheckFlags(required: NetNode.Flags.Created, forbidden: NetNode.Flags.Deleted);
        public static bool IsValid(this NetSegment segment) => segment.Info != null && segment.m_flags.CheckFlags(required: NetSegment.Flags.Created, forbidden: NetSegment.Flags.Deleted);


        public static bool ExistNode(this ushort nodeId) => (nodeId.GetNode().m_flags & NetNode.Flags.Created) != 0;
        public static bool ExistSegment(this ushort segmentId) => (segmentId.GetSegment().m_flags & NetSegment.Flags.Created) != 0;

        public static bool InGame => !OnStartup && !InMenu;
        public static bool InMenu => SceneManager.GetActiveScene().name is string scene && scene == "IntroScreen";
        public static bool OnStartup => SceneManager.GetActiveScene().name is string scene && scene == "Startup";

        public static IEnumerable<ushort> GetUpdateNodes(this NetManager netManager) => GetItems(netManager.m_updatedNodes);
        public static IEnumerable<ushort> GetUpdateSegments(this NetManager netManager) => GetItems(netManager.m_updatedSegments);
        private static IEnumerable<ushort> GetItems(ulong[] updated)
        {
            for (int j = 0; j < updated.Length; j++)
            {
                var num = updated[j];
                if (num != 0)
                {
                    for (int k = 0; k < 64; k++)
                    {
                        if ((num & (ulong)(1L << k)) != 0)
                            yield return (ushort)((j << 6) | k);
                    }
                }
            }
        }
    }
}
