using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.ComponentModel;
using UnityEngine;

namespace ModsCommon.UI
{
    public interface IValueChanger<TypeValue>
    {
        event Action<TypeValue> OnValueChanged;
        TypeValue Value { get; set; }
    }
    public abstract class UITextField<TypeValue> : CustomUITextField, IValueChanger<TypeValue>
    {
        public event Action<TypeValue> OnValueChanged;

        private bool InProcess { get; set; } = false;
        public TypeValue Value
        {
            get
            {
                try
                {
                    if (typeof(TypeValue) == typeof(string))
                        return (TypeValue)(object)text;
                    else if (string.IsNullOrEmpty(text))
                        return default;
                    else
                        return (TypeValue)TypeDescriptor.GetConverter(typeof(TypeValue)).ConvertFromString(text);
                }
                catch
                {
                    return default;
                }
            }
            set => ValueChanged(value, false);
        }
        protected virtual void ValueChanged(TypeValue value, bool callEvent = true, Action<TypeValue> action = null)
        {
            if (!InProcess)
            {
                InProcess = true;

                action?.Invoke(value);
                if (callEvent)
                    OnValueChanged?.Invoke(value);
                text = GetString(value);

                InProcess = false;
            }
        }

        protected virtual string GetString(TypeValue value) => value?.ToString() ?? string.Empty;

        protected override void OnSubmit()
        {
            base.OnSubmit();
            ValueChanged(Value);
        }

        public override string ToString() => Value.ToString();
        public static implicit operator TypeValue(UITextField<TypeValue> field) => field.Value;

        public void SetDefaultStyle()
        {
            atlas = CommonTextures.Atlas;
            normalBgSprite = CommonTextures.FieldNormal;
            hoveredBgSprite = CommonTextures.FieldHovered;
            focusedBgSprite = CommonTextures.FieldNormal;
            disabledBgSprite = CommonTextures.FieldDisabled;
            selectionSprite = CommonTextures.EmptySprite;

            allowFloats = true;
            isInteractive = true;
            enabled = true;
            readOnly = false;
            builtinKeyNavigation = true;
            cursorWidth = 1;
            cursorBlinkTime = 0.45f;
            selectOnFocus = true;

            textScale = 0.7f;
            verticalAlignment = UIVerticalAlignment.Middle;
            padding = new RectOffset(0, 0, 6, 0);
        }
    }
    public abstract class ComparableUITextField<ValueType> : UITextField<ValueType>
        where ValueType : IComparable<ValueType>
    {
        public ValueType MinValue { get; set; }
        public ValueType MaxValue { get; set; }
        public bool CheckMax { get; set; }
        public bool CheckMin { get; set; }
        public bool CyclicalValue { get; set; }
        public bool Limited => CheckMax && CheckMin;

        public bool UseWheel { get; set; }
        public ValueType WheelStep { get; set; }
        public string WheelTip
        {
            get => tooltip;
            set => tooltip = value;
        }
        public bool CanWheel { get; set; }

        protected override void ValueChanged(ValueType value, bool callEvent = true, Action<ValueType> action = null)
        {
            if (CheckMin && value.CompareTo(MinValue) < 0)
                value = MinValue;

            if (CheckMax && value.CompareTo(MaxValue) > 0)
                value = MaxValue;

            base.ValueChanged(value, callEvent, action);
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
            if (!CanWheel && Time.realtimeSinceStartup - m_HoveringStartTime < 1f)
                base.OnMouseWheel(p);
            else if (UseWheel)
            {
                var mode = InputExtension.ShiftIsPressed ? WheelMode.High : InputExtension.CtrlIsPressed ? WheelMode.Low : WheelMode.Normal;
                if (p.wheelDelta < 0)
                    ValueChanged(Decrement(Limited && CyclicalValue && Value.CompareTo(MinValue) == 0 ? MaxValue : Value, WheelStep, mode));
                else
                    ValueChanged(Increment(Limited && CyclicalValue && Value.CompareTo(MaxValue) == 0 ? MinValue : Value, WheelStep, mode));

                p.Use();
            }
        }
        protected abstract ValueType Increment(ValueType value, ValueType step, WheelMode mode);
        protected abstract ValueType Decrement(ValueType value, ValueType step, WheelMode mode);

        public ComparableUITextField() => SetDefault();

        public void SetDefault()
        {
            MinValue = default;
            MaxValue = default;
            CheckMin = false;
            CheckMax = false;
        }

        protected enum WheelMode
        {
            Normal,
            Low,
            High
        }
    }
    public class FloatUITextField : ComparableUITextField<float>
    {
        public override string text
        {
            get => base.text.Replace(',', '.');
            set => base.text = value;
        }
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
            WheelMode.Low => step / 10,
            WheelMode.High => step * 10,
            _ => step,
        };

        protected override string GetString(float value) => value.ToString("0.###");
    }
    public class IntUITextField : ComparableUITextField<int>
    {
        protected override int Decrement(int value, int step, WheelMode mode) => value == int.MinValue ? value : value - GetStep(step, mode);
        protected override int Increment(int value, int step, WheelMode mode) => value == int.MaxValue ? value : value + GetStep(step, mode);
        private int GetStep(int step, WheelMode mode) => mode switch
        {
            WheelMode.Low => Math.Max(step / 10, 1),
            WheelMode.High => step * 10,
            _ => step,
        };
    }
    public class ByteUITextField : ComparableUITextField<byte>
    {
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
            WheelMode.Low => (byte)Math.Max(step / 10, 1),
            WheelMode.High => (byte)Math.Min(step * 10, byte.MaxValue),
            _ => step,
        };
    }
    public class StringUITextField : UITextField<string> { }
}
