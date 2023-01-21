using System.Collections.Generic;

namespace ModsCommon.Utilities
{
    public interface INetObjectsMap
    {
        void AddSegment(ushort source, ushort target);
        void AddNode(ushort source, ushort target);
    }

    public abstract class NetObjectsMap<TypeObjectId> : BaseObjectsMap<TypeObjectId>, INetObjectsMap
        where TypeObjectId : NetObjectId, new()
    {
        public NetObjectsMap(bool isSimple = false) : base(isSimple) { }

        public bool TryGetNode(ushort nodeIdKey, out ushort nodeIdValue)
        {
            if (Map.TryGetValue(new TypeObjectId() { Node = nodeIdKey }, out TypeObjectId value))
            {
                nodeIdValue = value.Node;
                return true;
            }
            else
            {
                nodeIdValue = default;
                return false;
            }
        }
        public bool TryGetSegment(ushort segmentIdKey, out ushort segmentIdValue)
        {
            if (Map.TryGetValue(new TypeObjectId() { Segment = segmentIdKey }, out TypeObjectId value))
            {
                segmentIdValue = value.Segment;
                return true;
            }
            else
            {
                segmentIdValue = default;
                return false;
            }
        }

        public void AddSegment(ushort source, ushort target) => this[new TypeObjectId() { Segment = source }] = new TypeObjectId() { Segment = target };
        public void AddNode(ushort source, ushort target) => this[new TypeObjectId() { Node = source }] = new TypeObjectId() { Node = target };

        public void FromDictionary(Dictionary<InstanceID, InstanceID> sourceMap)
        {
            foreach (var source in sourceMap)
            {
                switch (source.Key.Type)
                {
                    case InstanceType.NetNode when source.Value.Type == InstanceType.NetNode:
                        AddNode(source.Key.NetNode, source.Value.NetNode);
                        break;
                    case InstanceType.NetSegment when source.Value.Type == InstanceType.NetSegment:
                        AddSegment(source.Key.NetSegment, source.Value.NetSegment);
                        break;
                }
            }
        }
    }
    public class NetObjectsMap : NetObjectsMap<NetObjectId>
    {
        public NetObjectsMap(bool isSimple = false) : base(isSimple) { }
    }

    public class NetObjectId : ObjectId
    {
        public static long NodeType = 1L << 32;
        public static long SegmentType = 2L << 32;

        public ushort Node
        {
            get => (Id & NodeType) == 0 ? (ushort)0 : (ushort)(Id & DataMask);
            set => Id = NodeType | value;
        }
        public ushort Segment
        {
            get => (Id & SegmentType) == 0 ? (ushort)0 : (ushort)(Id & DataMask);
            set => Id = SegmentType | value;
        }

        public override string ToString()
        {
            if (Type == NodeType)
                return $"{nameof(Node)}: {Node}";
            else if (Type == SegmentType)
                return $"{nameof(Segment)}: {Segment}";
            else
                return base.ToString();
        }
    }
}
