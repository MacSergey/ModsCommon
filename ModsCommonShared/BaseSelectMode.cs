using ColossalFramework.Math;
using ModsCommon.Utilities;
using System.Collections.Generic;
using UnityEngine;
using ColossalFramework;

namespace ModsCommon
{
    public abstract class BaseSelectToolMode<TypeMod, TypeTool> : BaseToolMode<TypeMod, TypeTool>
        where TypeMod : BaseMod<TypeMod>
        where TypeTool : BaseTool<TypeMod, TypeTool>
    {
        protected Segment3 Ray => SingletonTool<TypeTool>.Instance.Ray;

        protected NodeSelection HoverNode { get; set; } = null;
        protected bool IsHoverNode => HoverNode != null;

        protected SegmentSelection HoverSegment { get; set; } = null;
        protected bool IsHoverSegment => HoverSegment != null;

        protected virtual bool SelectNodes { get; } = true;
        protected virtual bool SelectSegments { get; } = true;
        protected virtual bool SelectMiddleNodes { get; } = false;

        protected override void Reset(IToolMode prevMode)
        {
            HoverNode = null;
            HoverSegment = null;
        }

        public override void OnToolUpdate()
        {
            NodeSelection nodeSelection = null;
            SegmentSelection segmentSelection = null;

            if (SingletonTool<TypeTool>.Instance.MouseRayValid)
            {
                if (IsHoverNode && HoverNode.Contains(Ray, out _))
                    nodeSelection = HoverNode;
                else if (IsHoverSegment && HoverSegment.Contains(Ray, out _))
                    segmentSelection = HoverSegment;
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

            var selectNodes = SelectNodes;
            var selectSegments = SelectSegments;
            var selectMiddleNodes = SelectMiddleNodes;

            for (int i = gridMinZ; i <= gridMaxZ; i++)
            {
                for (int j = gridMinX; j <= gridMaxX; j++)
                {
                    var segmentId = NetManager.instance.m_segmentGrid[i * 270 + j];
                    int count = 0;

                    while (segmentId != 0u && count < 36864)
                    {
                        if (CheckSegment(segmentId))
                        {
                            var segment = segmentId.GetSegment();
                            float t;

                            if (selectNodes && RayCastNode(checkedNodes, segment.m_startNode, selectMiddleNodes, out NodeSelection startSelection, out t) && t < priority)
                            {
                                nodeSelection = startSelection;
                                segmentSelection = null;
                                priority = t;
                            }
                            else if (selectNodes && RayCastNode(checkedNodes, segment.m_endNode, selectMiddleNodes, out NodeSelection endSelection, out t) && t < priority)
                            {
                                nodeSelection = endSelection;
                                segmentSelection = null;
                                priority = t;
                            }
                            else if (selectSegments && RayCastSegments(segmentId, out SegmentSelection selection, out t) && t < priority)
                            {
                                segmentSelection = selection;
                                nodeSelection = null;
                                priority = t;
                            }
                        }

                        segmentId = segmentBuffer[segmentId].m_nextGridSegment;
                    }
                }
            }

            static int Max(float value) => Mathf.Max((int)((value - 16f) / 64f + 135f) - 1, 0);
            static int Min(float value) => Mathf.Min((int)((value + 16f) / 64f + 135f) + 1, 269);
        }
        bool RayCastNode(HashSet<ushort> checkedNodes, ushort nodeId, bool includeMiddle, out NodeSelection selection, out float t)
        {
            if (!checkedNodes.Contains(nodeId))
            {
                checkedNodes.Add(nodeId);
                selection = new NodeSelection(nodeId);
                return selection.Contains(Ray, includeMiddle, out t);
            }
            else
            {
                selection = null;
                t = 0f;
                return false;
            }
        }
        bool RayCastSegments(ushort segmentId, out SegmentSelection selection, out float t)
        {
            selection = new SegmentSelection(segmentId);
            return selection.Contains(Ray, out t);
        }


        private bool CheckSegment(ushort segmentId)
        {
            var segment = segmentId.GetSegment();
            var connect = segment.Info.GetConnectionClass();

            if ((segment.m_flags & NetSegment.Flags.Created) == 0)
                return false;

            if ((connect.m_layer & ItemClass.Layer.Default) == 0)
                return false;

            if (connect.m_service != ItemClass.Service.Road && (connect.m_service != ItemClass.Service.PublicTransport || connect.m_subService != ItemClass.SubService.PublicTransportPlane))
                return false;

            return true;
        }

        public override void OnMouseUp(Event e) => OnPrimaryMouseClicked(e);
        public override void OnSecondaryMouseClicked() => Tool.Disable();
        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            if (IsHoverNode)
                HoverNode.Render(new OverlayData(cameraInfo) { Color = Colors.Orange });
            else if (IsHoverSegment)
                HoverSegment.Render(new OverlayData(cameraInfo) { Color = Colors.Purple });
        }
    }
}
