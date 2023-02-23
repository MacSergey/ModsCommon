using IMT.Utilities;
using System;
using System.Xml.Linq;
using UnityEngine;

namespace ModsCommon.Utilities
{
    public abstract class BasePropertyValue<T>
    {
        private Action OnChanged { get; }

        public abstract T Value { get; set; }
        public string Label { get; }

        public BasePropertyValue(Action onChanged) : this(string.Empty, onChanged) { }
        public BasePropertyValue(string label, Action onChanged)
        {
            Label = label;
            OnChanged = onChanged;
        }

        protected void OnValueChanged() => OnChanged?.Invoke();

        protected abstract bool Equals(T x, T y);

        public virtual void ToXml(XElement element, bool replaceValue = false)
        {
            ToXml(out var label, out var value);
            if (element.Attribute(label) is not XAttribute attribute)
                element.AddAttr(label, value);
            else if (replaceValue)
                attribute.Value = value.ToString();
        }

        protected virtual void ToXml(out string label, out object value)
        {
            label = Label;
            value = Value;
        }
        public void FromXml(XElement config) => FromXml(config, Value);
        public virtual void FromXml(XElement config, T defaultValue) => Value = config.GetAttrValue(Label, defaultValue);

        public override string ToString() => Value.ToString();
        public static implicit operator T(BasePropertyValue<T> property) => property.Value;
    }
    public abstract class PropertyValue<T> : BasePropertyValue<T>
    {
        private T _value;

        public override T Value
        {
            get => _value;
            set
            {
                if (!Equals(value, _value))
                {
                    _value = value;
                    OnValueChanged();
                }
            }
        }

        public PropertyValue(Action onChanged, T value = default) : base(onChanged)
        {
            _value = value;
        }
        public PropertyValue(string label, Action onChanged, T value = default) : base(label, onChanged)
        {
            _value = value;
        }
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
    public class PropertyNullableStructValue<T, PT> : BasePropertyValue<T?>
        where T : struct
        where PT : PropertyStructValue<T>
    {
        private bool _hasValue;
        public override T? Value
        {
            get
            {
                if (HasValue)
                    return StructProperty.Value;
                else
                    return null;
            }
            set
            {
                if (!Equals(value, Value))
                {
                    SetValue(value);
                    OnValueChanged();
                }
            }
        }
        public bool HasValue => _hasValue;
        private PT StructProperty { get; set; }

        public PropertyNullableStructValue(PT structProperty, Action onChanged, T? value = default) : base(onChanged)
        {
            StructProperty = structProperty;
            SetValue(value);
        }
        public PropertyNullableStructValue(PT structProperty, string label, Action onChanged, T? value = default) : base(label, onChanged)
        {
            StructProperty = structProperty;
            SetValue(value);
        }

        private void SetValue(T? value)
        {
            if (value.HasValue)
            {
                _hasValue = true;
                StructProperty.Value = value.Value;
            }
            else
            {
                _hasValue = false;
                StructProperty.Value = default;
            }
        }

        public override void ToXml(XElement element, bool replaceValue = false)
        {
            if (HasValue)
                StructProperty.ToXml(element, replaceValue);
            else
            {
                ToXml(out var label, out var value);
                if (element.Attribute(label) is not XAttribute attribute)
                    element.AddAttr(label, value);
                else if (replaceValue)
                    attribute.Value = value.ToString();
            }
        }
        protected override void ToXml(out string label, out object value)
        {
            label = Label;
            value = string.Empty;
        }
        public override void FromXml(XElement config, T? defaultValue)
        {
            if (config.TryGetAttrValue<string>(Label, out var stringValue) && stringValue == string.Empty)
                SetValue(null);
            else
            {
                _hasValue = true;
                StructProperty.FromXml(config);
            }
        }

        protected override bool Equals(T? x, T? y) => x.Equals(y);

        public static implicit operator T?(PropertyNullableStructValue<T, PT> property) => property.HasValue ? property.Value : null;
    }

    public class PropertyEnumValue<T> : PropertyStructValue<T>
        where T : struct, Enum
    {
        public PropertyEnumValue(Action onChanged, T value = default) : base(onChanged, value) { }
        public PropertyEnumValue(string label, Action onChanged, T value = default) : base(label, onChanged, value) { }

        protected override bool Equals(T x, T y) => x.ToInt() == y.ToInt();
        protected override void ToXml(out string label, out object value)
        {
            label = Label;
            value = Value.ToInt();
        }
        public override void FromXml(XElement config, T defaultValue) => Value = config.GetAttrValue(Label, defaultValue.ToInt()).ToEnum<T>();
    }
    public class PropertyLongEnumValue<T> : PropertyStructValue<T>
        where T : struct, Enum
    {
        public PropertyLongEnumValue(Action onChanged, T value = default) : base(onChanged, value) { }
        public PropertyLongEnumValue(string label, Action onChanged, T value = default) : base(label, onChanged, value) { }

        protected override bool Equals(T x, T y) => x.ToLong() == y.ToLong();
        protected override void ToXml(out string label, out object value)
        {
            label = Label;
            value = Value.ToLong();
        }
        public override void FromXml(XElement config, T defaultValue) => Value = config.GetAttrValue(Label, defaultValue.ToLong()).ToEnum<T>();
    }
    public class PropertyULongEnumValue<T> : PropertyStructValue<T>
        where T : struct, Enum
    {
        public PropertyULongEnumValue(Action onChanged, T value = default) : base(onChanged, value) { }
        public PropertyULongEnumValue(string label, Action onChanged, T value = default) : base(label, onChanged, value) { }

        protected override bool Equals(T x, T y) => x.ToULong() == y.ToULong();
        protected override void ToXml(out string label, out object value)
        {
            label = Label;
            value = Value.ToULong();
        }
        public override void FromXml(XElement config, T defaultValue) => Value = config.GetAttrValue(Label, defaultValue.ToULong()).ToEnum<T>();
    }
    public class PropertyBoolValue : PropertyStructValue<bool>
    {
        public PropertyBoolValue(Action onChanged, bool value = default) : base(onChanged, value) { }
        public PropertyBoolValue(string label, Action onChanged, bool value = default) : base(label, onChanged, value) { }

        protected override void ToXml(out string label, out object value)
        {
            label = Label;
            value = Value ? 1 : 0;
        }
        public override void FromXml(XElement config, bool defaultValue) => Value = config.GetAttrValue(Label, defaultValue ? 1 : 0) == 1;
    }
    public class PropertyColorValue : PropertyStructValue<Color32>
    {
        public PropertyColorValue(Action onChanged, Color32 value = default) : base(onChanged, value) { }
        public PropertyColorValue(string label, Action onChanged, Color32 value = default) : base(label, onChanged, value) { }

        protected override bool Equals(Color32 x, Color32 y) => x.r == y.r && x.g == y.g && x.b == y.b && x.a == y.a;
        protected override void ToXml(out string label, out object value)
        {
            label = Label;
            value = (Value.r << 24) + (Value.g << 16) + (Value.b << 8) + Value.a;
        }
        public override void FromXml(XElement config, Color32 defaultValue)
        {
            var color = config.GetAttrValue(Label, 0);
            Value = color != 0 ? new Color32((byte)(color >> 24), (byte)(color >> 16), (byte)(color >> 8), (byte)color) : defaultValue;
        }
    }
    public abstract class PropertyVectorValue<T> : PropertyStructValue<T>
            where T : struct
    {
        protected abstract uint Dimension { get; }

        protected string[] Labels { get; }
        protected PropertyVectorValue(Action onChanged, T value, params string[] labels) : base(onChanged, value)
        {
            Labels = labels;
        }

        protected abstract float Get(ref T vector, int index);
        protected abstract void Set(ref T vector, int index, float value);

        public override void ToXml(XElement element, bool replaceValue = false)
        {
            var value = Value;
            for (var i = 0; i < Dimension; i += 1)
            {
                if (element.Attribute(Labels[i]) is not XAttribute attribute)
                    element.AddAttr(Labels[i], Get(ref value, i));
                else if (replaceValue)
                    attribute.Value = Get(ref value, i).ToString();
            }
        }
        public override void FromXml(XElement config, T defaultValue)
        {
            for (var i = 0; i < Dimension; i += 1)
                Set(ref defaultValue, i, config.GetAttrValue(Labels[i], Get(ref defaultValue, i)));

            Value = defaultValue;
        }
    }
    public class PropertyVector2Value : PropertyVectorValue<Vector2>
    {
        protected override uint Dimension => 2;

        public PropertyVector2Value(Action onChanged, Vector2 value = default, string labelX = "X", string labelY = "Y") : base(onChanged, value, labelX, labelY) { }

        protected override float Get(ref Vector2 vector, int index) => vector[index];
        protected override void Set(ref Vector2 vector, int index, float value) => vector[index] = value;
    }
    public class PropertyVector3Value : PropertyVectorValue<Vector3>
    {
        protected override uint Dimension => 3;

        public PropertyVector3Value(Action onChanged, Vector3 value = default, string labelX = "X", string labelY = "Y", string labelZ = "Z") : base(onChanged, value, labelX, labelY, labelZ) { }

        protected override float Get(ref Vector3 vector, int index) => vector[index];
        protected override void Set(ref Vector3 vector, int index, float value) => vector[index] = value;
    }
    public class PropertyVector4Value : PropertyVectorValue<Vector4>
    {
        protected override uint Dimension => 4;

        public PropertyVector4Value(Action onChanged, Vector4 value = default, string labelX = "X", string labelY = "Y", string labelZ = "Z", string labelW = "W") : base(onChanged, value, labelX, labelY, labelZ, labelW) { }

        protected override float Get(ref Vector4 vector, int index) => vector[index];
        protected override void Set(ref Vector4 vector, int index, float value) => vector[index] = value;
    }
    public class PropertyStringValue : PropertyClassValue<string>
    {
        public PropertyStringValue(Action onChanged, string value = default) : base(onChanged, value) { }
        public PropertyStringValue(string label, Action onChanged, string value = default) : base(label, onChanged, value) { }

        protected override bool Equals(string x, string y) => x == y;
        protected override void ToXml(out string label, out object value)
        {
            label = Label;
            value = Value;
        }
        public override void FromXml(XElement config, string defaultValue) => Value = config.GetAttrValue(Label, defaultValue);
    }

    public class PropertyPrefabValue<PrefabType> : PropertyClassValue<PrefabType>
        where PrefabType : PrefabInfo
    {
        public string RawName { get; private set; } = string.Empty;
        public bool HasName => !string.IsNullOrEmpty(RawName);

        public override PrefabType Value
        {
            set
            {
                base.Value = value;
                RawName = value?.name ?? string.Empty;
            }
        }

        public PropertyPrefabValue(Action onChanged, PrefabType value = default) : base(onChanged, value) { }
        public PropertyPrefabValue(string label, Action onChanged, PrefabType value = default) : base(label, onChanged, value) { }

        public PropertyPrefabValue(Action onChanged, string name) : this(onChanged, PrefabCollection<PrefabType>.FindLoaded(name)) { }
        public PropertyPrefabValue(string label, Action onChanged, string name) : this(label, onChanged, PrefabCollection<PrefabType>.FindLoaded(name)) { }

        protected override void ToXml(out string label, out object value)
        {
            label = Label;
            value = Value?.name ?? RawName;
        }
        public override void FromXml(XElement config, PrefabType defaultValue)
        {
            RawName = config.GetAttrValue(Label, string.Empty);
            Value = PrefabCollection<PrefabType>.FindLoaded(RawName) ?? defaultValue;
        }
    }

    public class PropertyThemeValue : PropertyClassValue<ThemeHelper.IThemeData>
    {
        public string RawName { get; private set; } = string.Empty;
        public bool HasName => !string.IsNullOrEmpty(RawName);

        public override ThemeHelper.IThemeData Value 
        {
            set
            {
                base.Value = value;
                RawName = value?.Id ?? string.Empty;
            }
        }

        public PropertyThemeValue(Action onChanged, ThemeHelper.IThemeData value = default) : base(onChanged, value) { }
        public PropertyThemeValue(string label, Action onChanged, ThemeHelper.IThemeData value = default) : base(label, onChanged, value) { }

        public PropertyThemeValue(Action onChanged, string name) : this(onChanged, ThemeHelper.TryGetTheme(name, out var theme) ? theme : null) { }
        public PropertyThemeValue(string label, Action onChanged, string name) : this(label, onChanged, ThemeHelper.TryGetTheme(name, out var theme) ? theme : null) { }

        protected override void ToXml(out string label, out object value)
        {
            label = Label;
            value = Value?.Id ?? RawName;
        }
        public override void FromXml(XElement config, ThemeHelper.IThemeData defaultValue)
        {
            RawName = config.GetAttrValue(Label, string.Empty);
            Value = ThemeHelper.TryGetTheme(RawName, out var theme) ? theme : defaultValue;
        }
    }
}
