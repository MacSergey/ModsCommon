using ColossalFramework;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ModsCommon.Utilities
{
    public static class NetExtension
    {
        private static NetManager NetManager => Singleton<NetManager>.instance;

        public static void UpdateOnceSegment(this NetManager instance, ushort segmentId)
        {
            instance.m_updatedSegments[segmentId >> 6] |= (ulong)(1L << segmentId);
            instance.m_segmentsUpdated = true;
        }
        public static void UpdateOnceNode(this NetManager instance, ushort nodeId)
        {
            instance.m_updatedNodes[nodeId >> 6] |= (ulong)(1L << nodeId);
            instance.m_nodesUpdated = true;
        }

        [Obsolete]
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

        [Obsolete]
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

        [Obsolete]
        public static IEnumerable<NetLane> GetLanes(this NetSegment segment)
        {
            NetLane lane;
            for (var laneId = segment.m_lanes; laneId != 0; laneId = lane.m_nextLane)
            {
                lane = GetLane(laneId);
                yield return lane;
            }
        }
        public static uint GetLaneId(this NetSegment segment, int index)
        {
            uint laneId = segment.m_lanes;
            for (int i = 0; i < index && laneId != 0; i += 1)
            {
                laneId = laneId.GetLane().m_nextLane;
            }
            return laneId;
        }
        public static IEnumerable<uint> GetLaneIds(this NetSegment segment)
        {
            for (var laneId = segment.m_lanes; laneId != 0; laneId = GetLane(laneId).m_nextLane)
                yield return laneId;
        }
        public static IEnumerable<uint> GetLaneIds(this uint laneId)
        {
            for (; laneId != 0; laneId = GetLane(laneId).m_nextLane)
                yield return laneId;
        }
        public static IEnumerable<uint> GetLaneIds(this NetSegment segment, bool? startNode = null, NetInfo.LaneType laneType = NetInfo.LaneType.All, VehicleInfo.VehicleType vehicleType = VehicleInfo.VehicleType.All)
        {
            var lanesInfo = segment.Info.m_lanes;
            var index = 0;
            var laneId = segment.m_lanes;
            for (; laneId != 0 && index < lanesInfo.Length; Next(ref index, ref laneId))
            {
                if (!lanesInfo[index].m_laneType.IsFlagSet(laneType))
                    continue;
                if (!lanesInfo[index].m_vehicleType.IsFlagSet(vehicleType))
                    continue;
                if (startNode != null && startNode.Value ^ (lanesInfo[index].m_finalDirection == NetInfo.Direction.Forward) ^ !segment.IsInvert())
                    continue;

                yield return laneId;
            }
            static void Next(ref int index, ref uint laneId)
            {
                index += 1;
                laneId = GetLane(laneId).m_nextLane;
            }
        }
        public static bool GetCommonSegment(ushort firstId, ushort secondId, out ushort segmentId)
        {
            var firstSegmentIds = firstId.GetNode().SegmentIds().ToArray();
            var secondSegmentIds = secondId.GetNode().SegmentIds().ToArray();

            foreach(var firstSegmentId in firstSegmentIds)
            {
                if(secondSegmentIds.Contains(firstSegmentId))
                {
                    segmentId = firstSegmentId;
                    return true;
                }
            }

            segmentId = 0;
            return false;
        }
        public static bool GetCommonNode(ushort firstId, ushort secondId, out ushort nodeId)
        {
            ref var first = ref firstId.GetSegment();
            ref var second = ref secondId.GetSegment();

            if(first.m_startNode == second.m_startNode)
            {
                nodeId = first.m_startNode;
                return true;
            }
            else if (first.m_startNode == second.m_endNode)
            {
                nodeId = first.m_startNode;
                return true;
            }
            else if (first.m_endNode == second.m_startNode)
            {
                nodeId = first.m_endNode;
                return true;
            }
            else if (first.m_endNode == second.m_endNode)
            {
                nodeId = first.m_endNode;
                return true;
            }
            else
            {
                nodeId = 0;
                return false;
            }
        }
        public static bool IsCommon(ushort firstId, ushort secondId)
        {
            ref var first = ref firstId.GetSegment();
            ref var second = ref secondId.GetSegment();

            return (first.m_startNode == second.m_startNode || first.m_startNode == second.m_endNode || first.m_endNode == second.m_startNode || first.m_endNode == second.m_endNode);
        }

        public static bool Contains(ref this NetNode node, ushort segmentId) => node.SegmentIds().Contains(segmentId);
        public static bool Contains(ref this NetSegment segment, ushort nodeId) => segment.NodeIds().Contains(nodeId); 

        public static bool IsInvert(ref this NetSegment segment) => (segment.m_flags & NetSegment.Flags.Invert) != 0;

        public static ref NetNode GetNode(this ushort nodeId) => ref NetManager.m_nodes.m_buffer[nodeId];
        public static ref NetSegment GetSegment(this ushort segmentId) => ref NetManager.m_segments.m_buffer[segmentId];
        public static ref NetLane GetLane(this uint laneId) => ref NetManager.m_lanes.m_buffer[laneId];
        public static ushort GetNode(ref this NetSegment segment, bool isStartNode) => isStartNode ? segment.m_startNode : segment.m_endNode;
        public static Vector3 GetDirection(ref this NetSegment segment, bool isStart) => isStart ? segment.m_startDirection : segment.m_endDirection;

        public static bool IsStartNode(ref this NetSegment segment, ushort nodeId) => segment.m_startNode == nodeId;
        public static bool IsValid(ref this NetNode node) => node.Info != null && node.m_flags.CheckFlags(required: NetNode.Flags.Created, forbidden: NetNode.Flags.Deleted);
        public static bool IsValid(ref this NetSegment segment) => segment.Info != null && segment.m_flags.CheckFlags(required: NetSegment.Flags.Created, forbidden: NetSegment.Flags.Deleted);


        public static bool ExistNode(this ushort nodeId) => (nodeId.GetNode().m_flags & NetNode.Flags.Created) != 0;
        public static bool ExistSegment(this ushort segmentId) => (segmentId.GetSegment().m_flags & NetSegment.Flags.Created) != 0;

        public static int PedestrianLanes(this NetInfo info) => info.m_lanes.Count(lane => lane.m_laneType == NetInfo.LaneType.Pedestrian);
        public static bool IsTwoWay(this NetInfo info) => info.m_forwardVehicleLaneCount > 0 && info.m_backwardVehicleLaneCount > 0;

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

        public static IEnumerable<ushort> NextNodes(this ushort nodeId, ushort segmentId, bool includeEnds = false, ushort maxCount = ushort.MaxValue)
        {
            var info = segmentId.GetSegment().Info;
            var nextSegmetId = segmentId;
            var nextNodeId = nextSegmetId.GetSegment().GetOtherNode(nodeId);
            var nodeSegments = nextNodeId.GetNode().SegmentIds().ToArray();
            var count = 0;

            while (nodeSegments.Length == 2 && nodeSegments.All(s => s.GetSegment().Info == info) && count < maxCount)
            {
                yield return nextNodeId;

                nextSegmetId = nextNodeId.GetNode().SegmentIds().FirstOrDefault(s => s != nextSegmetId);
                nextNodeId = nextSegmetId.GetSegment().GetOtherNode(nextNodeId);
                nodeSegments = nextNodeId.GetNode().SegmentIds().ToArray();

                count += 1;
            }
            if (includeEnds && count < maxCount)
                yield return nextNodeId;
        }
    }
}
