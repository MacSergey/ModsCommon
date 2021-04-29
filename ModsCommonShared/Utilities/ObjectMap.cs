using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModsCommon.Utilities
{
    public interface IObjectsMap
    {
        void AddSegment(ushort source, ushort target);
        void AddNode(ushort source, ushort target);
    }
    public abstract class BaseObjectsMap<TypeObjectId> : IEnumerable<KeyValuePair<TypeObjectId, TypeObjectId>>, IObjectsMap
        where TypeObjectId : ObjectId, new()
    {
        public bool IsSimple { get; }
        public bool IsEmpty => !Map.Any();
        protected Dictionary<TypeObjectId, TypeObjectId> Map { get; } = new Dictionary<TypeObjectId, TypeObjectId>();
        public IEnumerable<TypeObjectId> Keys => Map.Keys;
        public IEnumerable<TypeObjectId> Values => Map.Values;

        public TypeObjectId this[TypeObjectId key]
        {
            get => Map[key];
            protected set
            {
                if (key == value)
                    return;

                Map[key] = value;
            }
        }

        public BaseObjectsMap(bool isSimple = false)
        {
            IsSimple = isSimple;
        }

        public bool TryGetValue(TypeObjectId key, out TypeObjectId value) => Map.TryGetValue(key, out value);
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

        public IEnumerator<KeyValuePair<TypeObjectId, TypeObjectId>> GetEnumerator() => Map.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
        public void AddSegment(ushort source, ushort target) => this[new TypeObjectId() { Segment = source }] = new TypeObjectId() { Segment = target };
        public void AddNode(ushort source, ushort target) => this[new TypeObjectId() { Node = source }] = new TypeObjectId() { Node = target };

        public void Remove(TypeObjectId key) => Map.Remove(key);

        public delegate bool TryGetDelegate<T>(T key, out T value);

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
    public class ObjectsMap : BaseObjectsMap<ObjectId>
    {
        public ObjectsMap(bool isSimple = false) : base(isSimple) { }
    }

    public class ObjectId
    {
        public static long DataMask = 0xFFFFFFFFL;
        public static long TypeMask = DataMask << 32;
        public static long NodeType = 1L << 32;
        public static long SegmentType = 2L << 32;

        public long Id;

        public ushort Node
        {
            get => (Id & NodeType) == 0 ? 0 : (ushort)(Id & DataMask);
            set => Id = NodeType | value;
        }
        public ushort Segment
        {
            get => (Id & SegmentType) == 0 ? 0 : (ushort)(Id & DataMask);
            set => Id = SegmentType | value;
        }
        public long Type => Id & TypeMask;

        public override bool Equals(object obj) => Equals(obj as ObjectId);
        public bool Equals(ObjectId other)
        {
            if (other is null)
                return false;
            else if (ReferenceEquals(this, other))
                return true;
            else
                return Id == other.Id;
        }
        public static bool operator ==(ObjectId x, ObjectId y) => x is null ? y is null : x.Equals(y);
        public static bool operator !=(ObjectId x, ObjectId y) => !(x == y);


        public override int GetHashCode() => Id.GetHashCode();
        public override string ToString()
        {
            if (Type == NodeType)
                return $"{nameof(Node)}: {Node}";
            else if (Type == SegmentType)
                return $"{nameof(Segment)}: {Segment}";
            else
                return $"{Type}: {Id}";
        }
    }
}
