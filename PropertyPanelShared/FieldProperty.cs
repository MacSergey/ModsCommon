using System;
using UnityEngine;
using ColossalFramework.UI;
using ModsCommon.Utilities;

namespace ModsCommon.UI
{
    public abstract class FieldPropertyPanel<ValueType, FieldType> : EditorPropertyPanel, IReusable
        where FieldType : UITextField<ValueType>
    {
        bool IReusable.InCache { get; set; }
        protected FieldType Field { get; set; }

        public event Action<ValueType> OnValueChanged;

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

        public ValueType Value
        {
            get => Field;
            set => Field.Value = value;
        }
        public string Format
        {
            set => Field.Format = value;
        }

        public FieldPropertyPanel()
        {
            Field = Content.AddUIComponent<FieldType>();
            Field.SetDefaultStyle();
            Field.name = nameof(Field);

            Field.OnValueChanged += ValueChanged;
        }

        public void SimulateEnterValue(ValueType value) => Field.SimulateEnterValue(value);
        private void ValueChanged(ValueType value) => OnValueChanged?.Invoke(value);

        public override void DeInit()
        {
            base.DeInit();

            OnValueChanged = null;

            Format = null;
            SubmitOnFocusLost = true;
        }
        public void Edit() => Field.Focus();
        public override string ToString() => $"{base.ToString()}: {Value}";

        public static implicit operator ValueType(FieldPropertyPanel<ValueType, FieldType> property) => property.Value;
    }
    public abstract class ComparableFieldPropertyPanel<ValueType, FieldType> : FieldPropertyPanel<ValueType, FieldType>
        where FieldType : ComparableUITextField<ValueType>
        where ValueType : IComparable<ValueType>
    {
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

        public ComparableFieldPropertyPanel()
        {
            Field.SetDefault();
        }

        public override void DeInit()
        {
            base.DeInit();
            Field.SetDefault();
        }
    }
    public class FloatPropertyPanel : ComparableFieldPropertyPanel<float, FloatUITextField> { }
    public class IntPropertyPanel : ComparableFieldPropertyPanel<int, IntUITextField> { }
    public class StringPropertyPanel : FieldPropertyPanel<string, StringUITextField>
    {
        public bool Multyline
        {
            get => Field.multiline;
            set => Field.multiline = value;
        }
        public float TextScale
        {
            get => Field.textScale;
            set => Field.textScale = value;
        }
        public override void DeInit()
        {
            base.DeInit();

            Multyline = false;
            TextScale = StringUITextField.DefaultTextScale;
            Field.height = 20;
        }

        protected override void OnSizeChanged()
        {
            if (Multyline)
                Field.height = height - ItemsPadding * 2f;
            else
                Field.height = 20;

            base.OnSizeChanged();
        }
    }

    public abstract class ComparableFieldRangePropertyPanel<ValueType, FieldType> : EditorPropertyPanel, IReusable
        where FieldType : ComparableUITextField<ValueType>
        where ValueType : IComparable<ValueType>
    {
        bool IReusable.InCache { get; set; }
        protected FieldType FieldA { get; set; }
        protected FieldType FieldB { get; set; }

        public event Action<ValueType, ValueType> OnValueChanged;

        public float FieldWidth
        {
            get => (FieldA.width + FieldB.width) * 0.5f;
            set
            {
                FieldA.width = value;
                FieldB.width = value;
            }
        }
        public bool SubmitOnFocusLost
        {
            get => FieldA.submitOnFocusLost && FieldB.submitOnFocusLost;
            set
            {
                FieldA.submitOnFocusLost = value;
                FieldB.submitOnFocusLost = value;
            }
        }

        public ValueType ValueA
        {
            get => FieldA;
            set
            {
                FieldA.Value = value;
                SetLimits();
            }
        }
        public ValueType ValueB
        {
            get => FieldB;
            set
            {
                FieldB.Value = value;
                SetLimits();
            }
        }
        public string Format
        {
            set
            {
                FieldA.Format = value;
                FieldB.Format = value;
            }
        }

        public ValueType MinValue
        {
            get => FieldA.MinValue;
            set
            {
                FieldA.MinValue = value;
                SetLimits();
            }
        }
        public ValueType MaxValue
        {
            get => FieldB.MaxValue;
            set
            {
                FieldB.MaxValue = value;
                SetLimits();
            }
        }
        public bool CheckMin
        {
            get => FieldA.CheckMin;
            set
            {
                FieldA.CheckMin = value;
                SetLimits();
            }
        }
        public bool CheckMax
        {
            get => FieldB.CheckMax;
            set
            {
                FieldB.CheckMax = value;
                SetLimits();
            }
        }

        private bool _allowInvert;
        public bool AllowInvert
        {
            get => _allowInvert;
            set
            {
                if (value != _allowInvert)
                {
                    _allowInvert = value;
                    SetLimits();
                }
            }
        }

        public bool UseWheel
        {
            get => FieldA.UseWheel && FieldB.UseWheel;
            set
            {
                FieldA.UseWheel = value;
                FieldB.UseWheel = value;
            }
        }
        public ValueType WheelStep
        {
            set
            {
                FieldA.WheelStep = value;
                FieldB.WheelStep = value;
            }
        }
        public bool WheelTip
        {
            set
            {
                FieldA.WheelTip = value;
                FieldB.WheelTip = value;
            }
        }

        public ComparableFieldRangePropertyPanel()
        {
            FieldA = Content.AddUIComponent<FieldType>();
            FieldA.SetDefaultStyle();
            FieldA.name = nameof(FieldA);
            FieldA.CheckMax = true;

            FieldB = Content.AddUIComponent<FieldType>();
            FieldB.SetDefaultStyle();
            FieldB.name = nameof(FieldB);
            FieldB.CheckMin = true;

            FieldA.OnValueChanged += ValueAChanged;
            FieldB.OnValueChanged += ValueBChanged;
        }

        public override void DeInit()
        {
            base.DeInit();

            OnValueChanged = null;

            AllowInvert = false;
            UseWheel = false;
            WheelStep = default;
            WheelTip = false;
            SubmitOnFocusLost = true;
            Format = null;

            FieldA.SetDefault();
            FieldB.SetDefault();
        }

        public void SetValues(ValueType valueA, ValueType valueB)
        {
            FieldA.Value = valueA;
            FieldB.Value = valueB;
            SetLimits();
        }
        private void ValueAChanged(ValueType value)
        {
            SetLimits();
            OnValueChanged?.Invoke(value, FieldB.Value);
        }
        private void ValueBChanged(ValueType value)
        {
            SetLimits();
            OnValueChanged?.Invoke(FieldA.Value, value);
        }

        private void SetLimits()
        {
            if (!AllowInvert)
            {
                FieldA.MaxValue = FieldB.Value;
                FieldA.CheckMax = true;

                FieldB.MinValue = FieldA.Value;
                FieldB.CheckMin = true;
            }
            else
            {
                FieldA.MaxValue = FieldB.MaxValue;
                FieldA.CheckMax = FieldB.CheckMax;

                FieldB.MinValue = FieldA.MinValue;
                FieldB.CheckMin = FieldB.CheckMin;
            }
        }

        public override string ToString() => $"{base.ToString()}: from {ValueA} to {ValueB}";
    }

    public class FloatRangePropertyPanel : ComparableFieldRangePropertyPanel<float, FloatUITextField> { }


    public abstract class InvertedFieldPropertyPanel<ValueType, FieldType> : ComparableFieldPropertyPanel<ValueType, FieldType>
        where FieldType : ComparableUITextField<ValueType>
        where ValueType : IComparable<ValueType>
    {
        protected MultyAtlasUIButton Invert { get; }

        public override float FieldWidth
        {
            get => base.FieldWidth + Content.autoLayoutPadding.horizontal + Invert.width;
            set => base.FieldWidth = value - Content.autoLayoutPadding.horizontal - Invert.width;
        }

        public InvertedFieldPropertyPanel()
        {
            Invert = Content.AddUIComponent<MultyAtlasUIButton>();
            Invert.SetDefaultStyle();
            Invert.width = 20;
            Invert.atlasForeground = CommonTextures.Atlas;
            Invert.normalFgSprite = CommonTextures.PlusMinusButton;
            Invert.eventClick += InvertClick;
        }

        private void InvertClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            Field.SimulateEnterValue(InvertValue(Field.Value));
        }
        protected override void Init(float? height)
        {
            base.Init(height);
            SetSize();
        }

        protected abstract ValueType InvertValue(ValueType value);

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            SetSize();
        }
        protected virtual void SetSize()
        {
            if (Invert != null)
                Invert.height = Content.height - ItemsPadding * 2;
        }
    }
    public class FloatInvertedPropertyPanel : InvertedFieldPropertyPanel<float, FloatUITextField>
    {
        protected override float InvertValue(float value) => -value;
    }
}
