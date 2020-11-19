using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace ModsCommon.Utilities
{
    public class PropertyValue<T>
    {
        Action OnChanged { get; }
        T _value;

        public virtual T Value
        {
            get => _value;
            set
            {
                _value = value;
                OnChanged();
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
        public virtual XAttribute ToXml() => new XAttribute(Label, Value);
        public virtual void FromXml(XElement config, T defaultValue) => Value = config.GetAttrValue(Label, defaultValue);
        public override string ToString() => Value.ToString();
        public static implicit operator T(PropertyValue<T> property) => property.Value;
    }
    public class PropertyEnumValue<T> : PropertyValue<T>
        where T : Enum
    {
        public PropertyEnumValue(Action onChanged, T value = default) : base(onChanged, value) { }
        public PropertyEnumValue(string label, Action onChanged, T value = default) : base(label, onChanged, value) { }

        public override XAttribute ToXml() => new XAttribute(Label, (int)(object)Value);
        public override void FromXml(XElement config, T defaultValue) => Value = config.GetAttrValue(Label, defaultValue.ToInt()).ToEnum<T>();
    }
    public class PropertyBoolValue : PropertyValue<bool>
    {
        public PropertyBoolValue(Action onChanged, bool value = default) : base(onChanged, value) { }
        public PropertyBoolValue(string label, Action onChanged, bool value = default) : base(label, onChanged, value) { }

        public override XAttribute ToXml() => new XAttribute(Label, Value ? 1 : 0);
        public override void FromXml(XElement config, bool defaultValue) => Value = config.GetAttrValue(Label, defaultValue ? 1 : 0) == 1;
    }
    public class PropertyColorValue : PropertyValue<Color32>
    {
        public PropertyColorValue(Action onChanged, Color32 value = default) : base(onChanged, value) { }
        public PropertyColorValue(string label, Action onChanged, Color32 value = default) : base(label, onChanged, value) { }

        public override XAttribute ToXml() => new XAttribute(Label, (Value.r << 24) + (Value.g << 16) + (Value.b << 8) + Value.a);
        public override void FromXml(XElement config, Color32 defaultValue)
        {
            var color = config.GetAttrValue(Label, 0);
            Value = color != 0 ? new Color32((byte)(color >> 24), (byte)(color >> 16), (byte)(color >> 8), (byte)color) : defaultValue;
        }
    }
}
