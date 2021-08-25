﻿using ColossalFramework;
using ColossalFramework.Math;
using ModsCommon.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace ModsCommon
{
    public abstract class BaseSelectToolMode<TypeTool> : BaseToolMode<TypeTool>
        where TypeTool : ITool
    {
        protected Segment3 Ray => SingletonTool<TypeTool>.Instance.Ray;

        protected virtual Color32 NodeColor => Colors.Orange;
        protected virtual Color32 SegmentColor => Colors.Purple;

        protected NodeSelection HoverNode { get; set; } = null;
        protected bool IsHoverNode => HoverNode != null;

        protected SegmentSelection HoverSegment { get; set; } = null;
        protected bool IsHoverSegment => HoverSegment != null;

        protected virtual bool SelectNodes { get; } = true;
        protected virtual bool SelectSegments { get; } = true;


        private HashSet<Selection> IgnoreList { get; } = new HashSet<Selection>(Selection.Comparer);
        private Dictionary<ushort, NodeSelection> NodeBuffer { get; } = new Dictionary<ushort, NodeSelection>(NetManager.MAX_NODE_COUNT);
        private Dictionary<ushort, SegmentSelection> SegmentBuffer { get; } = new Dictionary<ushort, SegmentSelection>(NetManager.MAX_SEGMENT_COUNT);

        bool _underground;
        protected bool Underground
        {
            get => _underground;
            set
            {
                if (value != _underground)
                {
                    _underground = value;
                    Singleton<InfoManager>.instance.SetCurrentMode(_underground ? InfoManager.InfoMode.Underground : InfoManager.InfoMode.None, InfoManager.SubInfoMode.Default);
                }
            }
        }
        protected virtual bool CheckUnderground => true;
        private bool NeedClear { get; set; }

        public override void Deactivate()
        {
            base.Deactivate();
            Clear();
        }
        protected override void Reset(IToolMode prevMode) => Clear();
        private void Clear()
        {
            Underground = false;
            ClearSelectionBuffer();
        }
        protected void ClearSelectionBuffer()
        {
            NeedClear = false;
            HoverNode = null;
            HoverSegment = null;
            IgnoreList.Clear();
            NodeBuffer.Clear();
            SegmentBuffer.Clear();
        }
        public void NeedClearSelectionBuffer() => NeedClear = true;
        public void IgnoreSelected()
        {
            if (IsHoverNode)
            {
                IgnoreList.Add(HoverNode);
                HoverNode = null;
            }
            else if (IsHoverSegment)
            {
                IgnoreList.Add(HoverSegment);
                HoverSegment = null;
            }
        }

        public override void OnToolUpdate()
        {
            if(NeedClear)
                ClearSelectionBuffer();

            if (SingletonTool<TypeTool>.Instance.MouseMoved)
                IgnoreList.Clear();

            var nodeSelection = default(NodeSelection);
            var segmentSelection = default(SegmentSelection);

            if (SingletonTool<TypeTool>.Instance.MouseRayValid)
            {
                if (IsHoverNode && HoverNode.Contains(Ray, out _))
                    nodeSelection = HoverNode;
                else if (!IsHoverSegment || !RayCast(HoverSegment.Id, ref nodeSelection, ref segmentSelection))
                {
                    RayCast(out nodeSelection, out segmentSelection);

                    if (IgnoreList.Count != 0 && nodeSelection == null && segmentSelection == null)
                    {
                        IgnoreList.Clear();
                        RayCast(out nodeSelection, out segmentSelection);
                    }
                }
            }

            HoverNode = nodeSelection;
            HoverSegment = segmentSelection;
        }

        private void RayCast(out NodeSelection nodeSelection, out SegmentSelection segmentSelection)
        {
            nodeSelection = null;
            segmentSelection = null;

            var hitPos = SingletonTool<TypeTool>.Instance.MouseWorldPosition;
            var gridMinX = Min(hitPos.x);
            var gridMinZ = Min(hitPos.z);
            var gridMaxX = Max(hitPos.x);
            var gridMaxZ = Max(hitPos.z);
            var segmentBuffer = Singleton<NetManager>.instance.m_segments.m_buffer;
            var ignoreNodes = new HashSet<ushort>();

            var priority = 1f;

            for (int i = gridMinZ; i <= gridMaxZ; i++)
            {
                for (int j = gridMinX; j <= gridMaxX; j++)
                {
                    var segmentId = NetManager.instance.m_segmentGrid[i * 270 + j];
                    int count = 0;

                    while (segmentId != 0u && count < 36864)
                    {
                        RayCast(segmentId, ignoreNodes, ref priority, ref nodeSelection, ref segmentSelection);
                        segmentId = segmentBuffer[segmentId].m_nextGridSegment;
                    }
                }
            }

            static int Min(float value) => Mathf.Max((int)((value - 16f) / 64f + 135f) - 1, 0);
            static int Max(float value) => Mathf.Min((int)((value + 16f) / 64f + 135f) + 1, 269);
        }
        private bool RayCast(ushort segmentId, HashSet<ushort> ignoreNodes, ref float priority, ref NodeSelection nodeSelection, ref SegmentSelection segmentSelection)
        {
            if (CheckSegment(segmentId))
            {
                ref var segment = ref segmentId.GetSegment();

                float thisPriority;
                if (SelectNodes && RayCastNode(ignoreNodes, segment.m_startNode, out NodeSelection startSelection, out thisPriority) && thisPriority < priority)
                {
                    nodeSelection = startSelection;
                    segmentSelection = null;
                    priority = thisPriority;
                    return true;
                }
                else if (SelectNodes && RayCastNode(ignoreNodes, segment.m_endNode, out NodeSelection endSelection, out thisPriority) && thisPriority < priority)
                {
                    nodeSelection = endSelection;
                    segmentSelection = null;
                    priority = thisPriority;
                    return true;
                }
                else if (SelectSegments && RayCastSegments(segmentId, out SegmentSelection selection, out thisPriority) && thisPriority < priority)
                {
                    segmentSelection = selection;
                    nodeSelection = null;
                    priority = thisPriority;
                    return true;
                }
            }

            return false;
        }
        private bool RayCast(ushort segmentId, ref NodeSelection nodeSelection, ref SegmentSelection segmentSelection)
        {
            var priority = 1f;
            return RayCast(segmentId, new HashSet<ushort>(), ref priority, ref nodeSelection, ref segmentSelection);
        }

        private bool RayCastNode(HashSet<ushort> ignoreNodes, ushort nodeId, out NodeSelection selection, out float t)
        {
            if (!ignoreNodes.Contains(nodeId))
            {
                ignoreNodes.Add(nodeId);
                if (IsValidNode(nodeId))
                {
                    if (!NodeBuffer.TryGetValue(nodeId, out selection))
                    {
                        selection = new NodeSelection(nodeId);
                        NodeBuffer[nodeId] = selection;
                    }

                    if (!IgnoreList.Contains(selection))
                        return selection.Contains(Ray, out t);
                }
            }

            selection = null;
            t = 0f;
            return false;

        }

        private bool RayCastSegments(ushort segmentId, out SegmentSelection selection, out float t)
        {
            if (IsValidSegment(segmentId))
            {
                if (!SegmentBuffer.TryGetValue(segmentId, out selection))
                {
                    selection = new SegmentSelection(segmentId);
                    SegmentBuffer[segmentId] = selection;
                }

                if (!IgnoreList.Contains(selection))
                    return selection.Contains(Ray, out t);
            }

            selection = null;
            t = 0f;
            return false;

        }
        protected virtual bool IsValidNode(ushort nodeId) => nodeId.GetNode().m_flags.IsSet(NetNode.Flags.Underground) ^ !Underground;
        protected virtual bool IsValidSegment(ushort segmentId) => true;

        protected virtual bool CheckSegment(ushort segmentId)
        {
            var segment = segmentId.GetSegment();

            if (!segment.m_flags.IsSet(NetSegment.Flags.Created))
                return false;

            if (CheckUnderground)
            {
                var startUndeground = segment.m_startNode.GetNode().m_flags.IsSet(NetNode.Flags.Underground);
                var endUndeground = segment.m_endNode.GetNode().m_flags.IsSet(NetNode.Flags.Underground);

                if (Underground && !startUndeground && !endUndeground)
                    return false;

                if (!Underground && startUndeground && endUndeground)
                    return false;
            }

            return CheckItemClass(segment.Info.GetConnectionClass());
        }
        protected virtual bool CheckItemClass(ItemClass itemClass) => true;

        public override void OnMouseUp(Event e) => OnPrimaryMouseClicked(e);
        public override void OnSecondaryMouseClicked() => Tool.Disable();
        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            if (IsHoverNode)
                HoverNode.Render(new OverlayData(cameraInfo) { Color = NodeColor, RenderLimit = Underground });
            else if (IsHoverSegment)
                HoverSegment.Render(new OverlayData(cameraInfo) { Color = SegmentColor, RenderLimit = Underground });
        }
    }
}
