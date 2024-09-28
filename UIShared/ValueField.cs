using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.ComponentModel;
using UnityEngine;

namespace ModsCommon.UI
{
    public interface IValueChanger<ValueType> : IReusable
    {
        event Action<ValueType> OnValueChanged;
        ValueType Value { get; set; }
        string Format { set; }
    }
    public interface IFieldRef { }
    public interface ITextField<ValueType>
    {
        ValueType Value { get; set; }
        //Color32 FieldTextColor { get; set; }
        string Format { set; }
        bool SubmitOnFocusLost { get; set; }
    }
    public abstract class UITextField<ValueType, RefType> : CustomUITextField, ITextField<ValueType>, IValueChanger<ValueType>, IReusable
        where RefType : IFieldRef, ITextField<ValueType>
    {
        private static string DefaultFormat => "{0}";
        public static float DefaultTextScale => 0.7f;

        public event Action<ValueType> OnValueChanged;

        private ValueType value;
        private string format;

        public RefType Ref { get; }
        bool IReusable.InCache { get; set; }
        Transform IReusable.CachedTransform { get => m_CachedTransform; set => m_CachedTransform = value; }

        private bool InProcess { get; set; } = false;
        public ValueType Value
        {
            get => value;
            set => ValueChanged(value, false);
        }
        public string Format
        {
            private get => !string.IsNullOrEmpty(format) ? format : DefaultFormat;
            set
            {
                format = value;
                RefreshText();
            }
        }

        public UITextField()
        {
            Ref = CreateRef();
        }
        protected abstract RefType CreateRef();

        public void SimulateEnterValue(ValueType value) => ValueChanged(value, true);

        protected virtual void ValueChanged(ValueType value, bool callEvent = true)
        {
            if (!InProcess)
            {
                InProcess = true;

                this.value = value;
                if (callEvent)
                    OnValueChanged?.Invoke(this.value);

                RefreshText();

                InProcess = false;
            }
        }
        protected void RefreshText() => text = hasFocus ? GetString(Value) : FormatString(Value);

        public virtual void DeInit()
        {
            OnValueChanged = null;
            Unfocus();
            SetDefault();
        }
        public virtual void SetDefault()
        {
            m_Text = string.Empty;
            value = default;
            format = null;
        }
        protected string FormatString(ValueType value) => string.Format(Format, GetString(value));
        protected virtual string GetString(ValueType value) => value?.ToString() ?? string.Empty;

        protected override void OnGotFocus(UIFocusEventParameter p)
        {
            RefreshText();
            base.OnGotFocus(p);
        }
        protected override void OnCancel(OnUnfocus onUnfocus)
        {
            base.OnCancel(onUnfocus);
            RefreshText();
        }
        protected override void OnSubmit(OnUnfocus onUnfocus)
        {
            var force = hasFocus;
            base.OnSubmit(onUnfocus);

            if (!force && text == GetString(Value))
            {
                RefreshText();
                return;
            }

            var newValue = default(ValueType);
            try
            {
                if (typeof(ValueType) == typeof(string))
                    newValue = (ValueType)(object)text;
                else if (!string.IsNullOrEmpty(text))
                    newValue = (ValueType)TypeDescriptor.GetConverter(typeof(ValueType)).ConvertFromString(text);
            }
            catch { }

            ValueChanged(newValue);
        }

        public override string ToString() => Value.ToString();
        public static implicit operator ValueType(UITextField<ValueType, RefType> field) => field.Value;

        public void SetDefaultStyle()
        {
            this.DefaultStyle();
            textScale = DefaultTextScale;
        }
    }
    public class FieldRef<ValueType, FieldType> : IFieldRef, ITextField<ValueType>
        where FieldType : ITextField<ValueType>
    {
        protected FieldType Field { get; }

        public FieldRef(FieldType field)
        {
            Field = field;
        }

        //public Color32 FieldTextColor
        //{
        //    get => Field.textColor;
        //    set => Field.textColor = value;
        //}
        public ValueType Value
        {
            get => Field.Value;
            set => Field.Value = value;
        }
        public string Format
        {
            set => Field.Format = value;
        }
        public bool SubmitOnFocusLost
        {
            get => Field.SubmitOnFocusLost;
            set => Field.SubmitOnFocusLost = value;
        }
    }

    public abstract class ComparableUITextField<ValueType, RefType> : UITextField<ValueType, RefType>, IComparableField<ValueType> 
        where ValueType : IComparable<ValueType>
        where RefType : IFieldRef, IComparableField<ValueType>
    {
        public ValueType MinValue { get; set; }
        public ValueType MaxValue { get; set; }
        public bool CheckMax { get; set; }
        public bool CheckMin { get; set; }
        public bool CyclicalValue { get; set; }
        public bool Limited => CheckMax && CheckMin;

        public bool UseWheel { get; set; }
        public ValueType WheelStep { get; set; }
        public bool WheelTip
        {
            set => tooltip = value ? CommonLocalize.FieldPanel_ScrollWheel : string.Empty;
        }
        public bool CanWheel { get; set; }
        private WheelMode Mode
        {
            get
            {
                if (Utility.ShiftIsPressed)
                    return WheelMode.High;
                else if (Utility.CtrlIsPressed)
                    return WheelMode.Low;
                else if (Utility.AltIsPressed)
                    return WheelMode.VeryLow;
                else
                    return WheelMode.Normal;
            }
        }

        public ComparableUITextField()
        {
            SetDefault();
        }

        protected override void ValueChanged(ValueType value, bool callEvent = true)
        {
            if (CheckMin && value.CompareTo(MinValue) < 0)
                value = MinValue;

            if (CheckMax && value.CompareTo(MaxValue) > 0)
                value = MaxValue;

            base.ValueChanged(value, callEvent);
        }
        public override void DeInit()
        {
            base.DeInit();
            SetDefault();
        }
        protected override void OnMouseMove(UIMouseEventParameter p)
        {
            base.OnMouseMove(p);
            CanWheel = true;
        }
        protected override void OnMouseLeave(UIMouseEventParameter p)
        {
            base.OnMouseLeave(p);
            CanWheel = false;
        }
        protected sealed override void OnMouseWheel(UIMouseEventParameter p)
        {
            m_TooltipShowing = true;
            tooltipBox.Hide();

            if (UseWheel && (CanWheel || Time.realtimeSinceStartup - m_HoveringStartTime >= UIHelper.PropertyScrollTimeout))
            {
                if (p.wheelDelta < 0)
                    ValueChanged(Decrement(Limited && CyclicalValue && Value.CompareTo(MinValue) == 0 ? MaxValue : Value, WheelStep, Mode));
                else
                    ValueChanged(Increment(Limited && CyclicalValue && Value.CompareTo(MaxValue) == 0 ? MinValue : Value, WheelStep, Mode));

                p.Use();
            }
        }
        protected override void OnTooltipEnter(UIMouseEventParameter p)
        {
            base.OnTooltipEnter(p);

            if (!isEnabled)
                m_TooltipShowing = true;
        }

        protected abstract ValueType Increment(ValueType value, ValueType step, WheelMode mode);
        protected abstract ValueType Decrement(ValueType value, ValueType step, WheelMode mode);

        public override void SetDefault()
        {
            base.SetDefault();

            MinValue = default;
            MaxValue = default;
            CheckMin = false;
            CheckMax = false;
            CyclicalValue = false;
            UseWheel = false;
            WheelTip = false;
            WheelStep = default;
        }

        protected enum WheelMode
        {
            High,
            Normal,
            Low,
            VeryLow,
        }

        
    }
    public interface IComparableField<ValueType> : ITextField<ValueType>
    {
        ValueType MinValue { get; set; }
        ValueType MaxValue { get; set; }
        bool CheckMin { get; set; }
        bool CheckMax { get; set; }
        bool CyclicalValue { get; set;}
        bool UseWheel {  get; set; }
        ValueType WheelStep { get; set; }
        bool WheelTip { set; }
    }
    public class ComparableFieldRef<ValueType, FieldType> : FieldRef<ValueType, FieldType>, IComparableField<ValueType>
        where FieldType : IComparableField<ValueType>
    {
        public ComparableFieldRef(FieldType field) : base(field) { }

        public ValueType MinValue
        {
            get => Field.MinValue;
            set => Field.MinValue = value;
        }
        public ValueType MaxValue
        {
            get => Field.MaxValue;
            set => Field.MaxValue = value;
        }
        public bool CheckMin
        {
            get => Field.CheckMin;
            set => Field.CheckMin = value;
        }
        public bool CheckMax
        {
            get => Field.CheckMax;
            set => Field.CheckMax = value;
        }
        public bool CyclicalValue
        {
            get => Field.CyclicalValue;
            set => Field.CyclicalValue = value;
        }

        public bool UseWheel
        {
            get => Field.UseWheel;
            set => Field.UseWheel = value;
        }
        public ValueType WheelStep
        {
            get => Field.WheelStep;
            set => Field.WheelStep = value;
        }
        public bool WheelTip
        {
            set => Field.WheelTip = value;
        }
    }

    public class FloatUITextField : ComparableUITextField<float, FloatUITextField.FloatFieldRef>
    {
        static string DefaultNumberFormat => "0.###";
        private string _numberFormat;
        public string NumberFormat
        {
            private get => !string.IsNullOrEmpty(_numberFormat) ? _numberFormat : DefaultNumberFormat;
            set
            {
                _numberFormat = value;
                RefreshText();
            }
        }

        public override string text
        {
            get => base.text.Replace(',', '.');
            set => base.text = value;
        }
        protected override FloatFieldRef CreateRef() => new(this);
        protected override float Decrement(float value, float step, WheelMode mode)
        {
            step = GetStep(step, mode);
            return (value - step).RoundToNearest(step);
        }
        protected override float Increment(float value, float step, WheelMode mode)
        {
            step = GetStep(step, mode);
            return (value + step).RoundToNearest(step);
        }

        private float GetStep(float step, WheelMode mode) => mode switch
        {
            WheelMode.High => step * 10,
            WheelMode.Low => step / 10,
            WheelMode.VeryLow => step / 100,
            _ => step,
        };

        public override void DeInit()
        {
            base.DeInit();
            _numberFormat = null;
        }
        protected override string GetString(float value) => value.ToString(NumberFormat);

        public class FloatFieldRef : ComparableFieldRef<float, FloatUITextField> 
        {
            public FloatFieldRef(FloatUITextField field) : base(field) { }
        }
    }
    public class IntUITextField : ComparableUITextField<int, IntUITextField.IntFieldRef>
    {
        protected override IntFieldRef CreateRef() => new(this);
        protected override int Decrement(int value, int step, WheelMode mode) => value == int.MinValue ? value : value - GetStep(step, mode);
        protected override int Increment(int value, int step, WheelMode mode) => value == int.MaxValue ? value : value + GetStep(step, mode);
        private int GetStep(int step, WheelMode mode) => mode switch
        {
            WheelMode.High => step * 10,
            WheelMode.Low => Math.Max(step / 10, 1),
            WheelMode.VeryLow => Math.Max(step / 100, 1),
            _ => step,
        };

        public class IntFieldRef : ComparableFieldRef<int, IntUITextField>
        {
            public IntFieldRef(IntUITextField field) : base(field) { }
        }
    }
    public class ByteUITextField : ComparableUITextField<byte, ByteUITextField.ByteFieldRef>
    {
        protected override ByteFieldRef CreateRef() => new(this);
        protected override byte Decrement(byte value, byte step, WheelMode mode)
        {
            step = GetStep(step, mode);
            return value < step ? byte.MinValue : (byte)(value - step);
        }
        protected override byte Increment(byte value, byte step, WheelMode mode)
        {
            step = GetStep(step, mode);
            return byte.MaxValue - value < step ? byte.MaxValue : (byte)(value + step);
        }

        private byte GetStep(byte step, WheelMode mode) => mode switch
        {
            WheelMode.High => (byte)Math.Min(step * 10, byte.MaxValue),
            WheelMode.Low => (byte)Math.Max(step / 10, 1),
            WheelMode.VeryLow => (byte)Math.Max(step / 100, 1),
            _ => step,
        };

        public class ByteFieldRef : ComparableFieldRef<byte, ByteUITextField>
        {
            public ByteFieldRef(ByteUITextField field) : base(field) { }
        }
    }
    public class StringUITextField : UITextField<string, StringUITextField.StringFieldRef> 
    {
        public Func<string, string> CheckValue { private get; set; }

        protected override StringFieldRef CreateRef() => new(this);

        protected override void ValueChanged(string value, bool callEvent = true)
        {
            if (CheckValue != null)
                value = CheckValue(value);

            base.ValueChanged(value, callEvent);
        }

        public class StringFieldRef : FieldRef<string, StringUITextField>
        {
            public StringFieldRef(StringUITextField field) : base(field) { }

            public bool Multiline
            {
                get => Field.Multiline;
                set => Field.Multiline = value;
            }
            public float TextScale
            {
                get => Field.textScale;
                set => Field.textScale = value;
            }
        }
    }
}
