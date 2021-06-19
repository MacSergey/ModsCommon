using ColossalFramework;
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

        protected override void Reset(IToolMode prevMode)
        {
            HoverNode = null;
            HoverSegment = null;
            Underground = false;
        }

        public override void OnToolUpdate()
        {
            NodeSelection nodeSelection = null;
            SegmentSelection segmentSelection = null;

            if (SingletonTool<TypeTool>.Instance.MouseRayValid)
            {
                if (IsHoverNode && HoverNode.Contains(Ray, out _))
                    nodeSelection = HoverNode;
                else if (IsHoverSegment && RayCast(HoverSegment.Id, new HashSet<ushort>(), 1f, out _, ref nodeSelection, ref segmentSelection))
                    ;
                else
                    RayCast(out nodeSelection, out segmentSelection);
            }

            HoverNode = nodeSelection;
            HoverSegment = segmentSelection;
        }

        private void RayCast(out NodeSelection nodeSelection, out SegmentSelection segmentSelection)
        {
            nodeSelection = null;
            segmentSelection = null;

            var hitPos = SingletonTool<TypeTool>.Instance.MouseWorldPosition;
            var gridMinX = Max(hitPos.x);
            var gridMinZ = Max(hitPos.z);
            var gridMaxX = Min(hitPos.x);
            var gridMaxZ = Min(hitPos.z);
            var segmentBuffer = Singleton<NetManager>.instance.m_segments.m_buffer;
            var checkedNodes = new HashSet<ushort>();

            var priority = 1f;

            for (int i = gridMinZ; i <= gridMaxZ; i++)
            {
                for (int j = gridMinX; j <= gridMaxX; j++)
                {
                    var segmentId = NetManager.instance.m_segmentGrid[i * 270 + j];
                    int count = 0;

                    while (segmentId != 0u && count < 36864)
                    {
                        RayCast(segmentId, checkedNodes, priority, out priority, ref nodeSelection, ref segmentSelection);
                        segmentId = segmentBuffer[segmentId].m_nextGridSegment;
                    }
                }
            }

            static int Max(float value) => Mathf.Max((int)((value - 16f) / 64f + 135f) - 1, 0);
            static int Min(float value) => Mathf.Min((int)((value + 16f) / 64f + 135f) + 1, 269);
        }
        private bool RayCast(ushort segmentId, HashSet<ushort> checkedNodes, float priority, out float resultPriority, ref NodeSelection nodeSelection, ref SegmentSelection segmentSelection)
        {
            resultPriority = priority;
            if (CheckSegment(segmentId))
            {
                ref var segment = ref segmentId.GetSegment();

                if (SelectNodes && RayCastNode(checkedNodes, segment.m_startNode, out NodeSelection startSelection, out priority) && priority < resultPriority)
                {
                    nodeSelection = startSelection;
                    segmentSelection = null;
                    resultPriority = priority;
                    return true;
                }
                else if (SelectNodes && RayCastNode(checkedNodes, segment.m_endNode, out NodeSelection endSelection, out priority) && priority < resultPriority)
                {
                    nodeSelection = endSelection;
                    segmentSelection = null;
                    resultPriority = priority;
                    return true;
                }
                else if (SelectSegments && RayCastSegments(segmentId, out SegmentSelection selection, out priority) && priority < resultPriority)
                {
                    segmentSelection = selection;
                    nodeSelection = null;
                    resultPriority = priority;
                    return true;
                }
            }

            return false;
        }

        private bool RayCastNode(HashSet<ushort> checkedNodes, ushort nodeId, out NodeSelection selection, out float t)
        {
            if (!checkedNodes.Contains(nodeId))
            {
                checkedNodes.Add(nodeId);
                if (IsValidNode(nodeId))
                {
                    selection = new NodeSelection(nodeId);
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
                selection = new SegmentSelection(segmentId);
                return selection.Contains(Ray, out t);
            }
            else
            {
                selection = null;
                t = 0f;
                return false;
            }
        }
        protected virtual bool IsValidNode(ushort nodeId) => nodeId.GetNode().m_flags.IsSet(NetNode.Flags.Underground) ^ !Underground;
        protected virtual bool IsValidSegment(ushort segmentId) => true;

        protected virtual bool CheckSegment(ushort segmentId)
        {
            var segment = segmentId.GetSegment();

            if (!segment.m_flags.IsSet(NetSegment.Flags.Created))
                return false;

            var startUndeground = segment.m_startNode.GetNode().m_flags.IsSet(NetNode.Flags.Underground);
            var endUndeground = segment.m_endNode.GetNode().m_flags.IsSet(NetNode.Flags.Underground);

            if (Underground && !startUndeground && !endUndeground)
                return false;

            if (!Underground && startUndeground && endUndeground)
                return false;

            else
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
