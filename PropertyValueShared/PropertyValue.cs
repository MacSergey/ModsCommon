using System;
using System.Xml.Linq;
using UnityEngine;

namespace ModsCommon.Utilities
{
    public abstract class PropertyValue<T>
    {
        private Action OnChanged { get; }

        private T _value;

        public virtual T Value
        {
            get => _value;
            set
            {
                if (!Equals(value, _value))
                {
                    _value = value;
                    OnChanged();
                }
            }
        }
        public string Label { get; }

        public PropertyValue(Action onChanged, T value = default) : this(string.Empty, onChanged, value) { }
        public PropertyValue(string label, Action onChanged, T value = default)
        {
            Label = label;
            OnChanged = onChanged;
            _value = value;
        }

        protected abstract bool Equals(T x, T y);

        public void ToXml(XElement element) => element.Add(ToXml());
        protected virtual XAttribute ToXml() => new XAttribute(Label, Value);
        public virtual void FromXml(XElement config) => FromXml(config, Value);
        public virtual void FromXml(XElement config, T defaultValue) => Value = config.GetAttrValue(Label, defaultValue);

        public override string ToString() => Value.ToString();
        public static implicit operator T(PropertyValue<T> property) => property.Value;
    }
    public class PropertyClassValue<T> : PropertyValue<T>
        where T : class
    {
        public PropertyClassValue(Action onChanged, T value = default) : base(onChanged, value) { }
        public PropertyClassValue(string label, Action onChanged, T value = default) : base(label, onChanged, value) { }

        protected override bool Equals(T x, T y) => object.Equals(x, y);
    }
    public class PropertyStructValue<T> : PropertyValue<T>
    where T : struct
    {
        public PropertyStructValue(Action onChanged, T value = default) : base(onChanged, value) { }
        public PropertyStructValue(string label, Action onChanged, T value = default) : base(label, onChanged, value) { }

        protected override bool Equals(T x, T y) => x.Equals(y);
    }

    public class PropertyEnumValue<T> : PropertyStructValue<T>
        where T : struct, Enum
    {
        public PropertyEnumValue(Action onChanged, T value = default) : base(onChanged, value) { }
        public PropertyEnumValue(string label, Action onChanged, T value = default) : base(label, onChanged, value) { }

        protected override bool Equals(T x, T y) => x.ToInt() == y.ToInt();
        protected override XAttribute ToXml() => new XAttribute(Label, Value.ToInt());
        public override void FromXml(XElement config, T defaultValue) => Value = config.GetAttrValue(Label, defaultValue.ToInt()).ToEnum<T>();
    }
    public class PropertyBoolValue : PropertyStructValue<bool>
    {
        public PropertyBoolValue(Action onChanged, bool value = default) : base(onChanged, value) { }
        public PropertyBoolValue(string label, Action onChanged, bool value = default) : base(label, onChanged, value) { }

        protected override XAttribute ToXml() => new XAttribute(Label, Value ? 1 : 0);
        public override void FromXml(XElement config, bool defaultValue) => Value = config.GetAttrValue(Label, defaultValue ? 1 : 0) == 1;
    }
    public class PropertyColorValue : PropertyStructValue<Color32>
    {
        public PropertyColorValue(Action onChanged, Color32 value = default) : base(onChanged, value) { }
        public PropertyColorValue(string label, Action onChanged, Color32 value = default) : base(label, onChanged, value) { }

        protected override bool Equals(Color32 x, Color32 y) => x.r == y.r && x.g == y.g && x.b == y.b && x.a == y.a;
        protected override XAttribute ToXml() => new XAttribute(Label, (Value.r << 24) + (Value.g << 16) + (Value.b << 8) + Value.a);
        public override void FromXml(XElement config, Color32 defaultValue)
        {
            var color = config.GetAttrValue(Label, 0);
            Value = color != 0 ? new Color32((byte)(color >> 24), (byte)(color >> 16), (byte)(color >> 8), (byte)color) : defaultValue;
        }
    }
}
