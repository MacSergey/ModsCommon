using ICities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class SliderPropertyPanel<ValueType, SliderType, FieldType> : EditorPropertyPanel, IReusable
        where SliderType : UIValueSlider<ValueType>
        where FieldType : UITextField<ValueType>
    {
        bool IReusable.InCache { get; set; }
        private bool InProcess { get; set; } = false;

        public event Action<ValueType> OnValueChanged;

        protected FieldType Field { get; set; }
        protected SliderType Slider { get; set; }
        protected CustomUILabel MinLabel { get; set; }
        protected CustomUILabel MaxLabel { get; set; }


        public virtual float FieldWidth
        {
            get => Field.width;
            set => Field.width = value;
        }
        public Color32 FieldTextColor
        {
            get => Field.textColor;
            set => Field.textColor = value;
        }
        public bool SubmitOnFocusLost
        {
            get => Field.submitOnFocusLost;
            set => Field.submitOnFocusLost = value;
        }
        public virtual float SliderWidth
        {
            get => Slider.width;
            set => Slider.width = value;
        }

        private ValueType value;
        public ValueType Value
        {
            get => value;
            set
            {
                this.value = value;
                Field.Value = value;
                Slider.Value = value;
            }
        }
        public string Format
        {
            set => Field.Format = value;
        }

        protected override void FillContent()
        {
            Slider = Content.AddUIComponent<SliderType>();
            Slider.SetDefaultStyle();
            Slider.name = nameof(Slider);
            Slider.OnChanged += SliderChanged;

            Field = Content.AddUIComponent<FieldType>();
            Field.SetDefaultStyle();
            Field.name = nameof(Field);
            Field.OnValueChanged += FieldChanged;
        }
        public override void DeInit()
        {
            base.DeInit();

            OnValueChanged = null;

            Format = null;
            SubmitOnFocusLost = true;
        }

        protected void ValueChanged(ValueType value, bool callEvent = true)
        {
            if (!InProcess)
            {
                InProcess = true;

                Value = value;
                if (callEvent)
                    OnValueChanged?.Invoke(value);

                InProcess = false;
            }
        }
        private void SliderChanged(ValueType value) => ValueChanged(value);
        private void FieldChanged(ValueType value) => ValueChanged(value);



        public override string ToString() => $"{base.ToString()}: {Value}";

        public static implicit operator ValueType(SliderPropertyPanel<ValueType, SliderType, FieldType> property) => property.Value;
    }

    public abstract class ComparableSliderPropertyPanel<ValueType, SliderType, FieldType> : SliderPropertyPanel<ValueType, SliderType, FieldType>
        where SliderType : ComparableUIValueSlider<ValueType>
        where FieldType : ComparableUITextField<ValueType>
        where ValueType : struct, IComparable<ValueType>
    {
        public ValueType MinValue
        {
            get => Field.MinValue;
            set
            {
                Field.MinValue = value;
                Slider.MinValue = value;
            }
        }
        public ValueType MaxValue
        {
            get => Field.MaxValue;
            set
            {
                Field.MaxValue = value;
                Slider.MaxValue = value;
            }
        }
        public ValueType MiddleValue
        {
            get => Slider.MiddleValue ?? default;
            set => Slider.MiddleValue = value;
        }
        public bool UseWheel
        {
            get => Field.UseWheel;
            set => Field.UseWheel = value;
        }
        public ValueType WheelStep
        {
            get => Field.WheelStep;
            set
            {
                Field.WheelStep = value;
                Slider.Step = value;
            }
        }
        public bool WheelTip
        {
            set => Field.WheelTip = value;
        }

        public ComparableSliderPropertyPanel()
        {
            Field.SetDefault();
            Field.CheckMin = true;
            Field.CheckMax = true;
        }

        public override void DeInit()
        {
            base.DeInit();
            Field.SetDefault();
        }
    }
    public class FloatSliderPropertyPanel : ComparableSliderPropertyPanel<float, FloatUISlider, FloatUITextField> { }
}
