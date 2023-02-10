using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class UIValueSlider<TypeValue> : CustomUISlider, IReusable
    {
        public event Action<TypeValue> OnChanged;

        bool IReusable.InCache { get; set; }
        private bool InProcess { get; set; } = false;


        private new TypeValue minValue;
        private new TypeValue maxValue;
        public TypeValue Value
        {
            get => GetValue(m_RawValue);
            set => ValueChanged(value, false);
        }
        public TypeValue MinValue
        {
            get => minValue;
            set
            {
                var oldValue = Value;
                minValue = value;
                Value = oldValue;
            }
        }
        public TypeValue MaxValue
        {
            get => maxValue;
            set
            {
                var oldValue = Value;
                maxValue = value;
                Value = oldValue;
            }
        }
        public TypeValue Step
        {
            get => GetValue(stepSize);
            set => GetRawValue(value);
        }

        public UIValueSlider()
        {
            thumbObject = AddUIComponent<UISprite>();
            thumbObject.size = new Vector2(10f, 10f);
            SetDefaultStyle();
        }

        public virtual void DeInit()
        {
            OnChanged = null;
            MinValue = default;
            MaxValue = default;
            Value = default;

            SetDefaultStyle();
        }

        protected virtual void ValueChanged(TypeValue value, bool callEvent = true)
        {
            if (!InProcess)
            {
                InProcess = true;

                base.value = GetRawValue(value);
                if (callEvent)
                    OnChanged?.Invoke(value);

                InProcess = false;
            }
        }

        protected override void OnValueChanged()
        {
            base.OnValueChanged();

            var newValue = GetValue(m_RawValue);
            ValueChanged(newValue);
        }

        protected abstract TypeValue GetValue(float rawValue);
        protected abstract float GetRawValue(TypeValue value);

        public void SetDefaultStyle()
        {
            base.minValue = 0;
            base.maxValue = 1;
            base.stepSize = 0.001f;
        }
    }
    public abstract class ComparableUIValueSlider<TypeValue> : UIValueSlider<TypeValue>
        where TypeValue : struct, IComparable<TypeValue>
    {
        public TypeValue? MiddleValue { get; set; }

        public override void DeInit()
        {
            base.DeInit();
            MiddleValue = null;
        }

        protected sealed override TypeValue GetValue(float rawValue)
        {
            if (MiddleValue == null)
                return GetValue(rawValue, MinValue, MaxValue);
            if (rawValue < 0.5f)
                return GetValue(rawValue * 2f, MinValue, MiddleValue.Value);
            else
                return GetValue(rawValue * 2f - 1f, MiddleValue.Value, MaxValue);
        }
        protected abstract TypeValue GetValue(float rawValue, TypeValue min, TypeValue max);

        protected override float GetRawValue(TypeValue value)
        {
            if (MiddleValue == null)
                return Mathf.Clamp01(GetRawValue(value, MinValue, MaxValue));
            else if (value.CompareTo(MiddleValue.Value) < 0)
                return Mathf.Clamp01(GetRawValue(value, MinValue, MiddleValue.Value)) * 0.5f;
            else
                return Mathf.Clamp01(GetRawValue(value, MiddleValue.Value, MaxValue)) * 0.5f + 0.5f;
        }
        protected abstract float GetRawValue(TypeValue value, TypeValue min, TypeValue max);
    }

    public class FloatUISlider : ComparableUIValueSlider<float>
    {
        protected override float GetValue(float rawValue, float min, float max) => Mathf.Lerp(min, max, value);
        protected override float GetRawValue(float value, float min, float max) => (value - min) / (max - min);
    }
}
