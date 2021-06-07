using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModsCommon.Utilities
{
    public abstract class BaseObjectsMap<TypeObjectId> : IEnumerable<KeyValuePair<TypeObjectId, TypeObjectId>>
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

        public IEnumerator<KeyValuePair<TypeObjectId, TypeObjectId>> GetEnumerator() => Map.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Remove(TypeObjectId key) => Map.Remove(key);

        public delegate bool TryGetDelegate<T>(T key, out T value);
    }

    public class ObjectId
    {
        public static long DataMask = 0xFFFFFFFFL;
        public static long TypeMask = DataMask << 32;

        protected long Id;
       
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
        public override string ToString() => $"{Type}: {Id}";
    }
}
