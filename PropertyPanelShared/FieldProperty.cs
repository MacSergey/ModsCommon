using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class FieldPropertyPanel<ValueType, FieldType, RefType> : EditorPropertyPanel, IReusable
        where FieldType : UITextField<ValueType>, RefType
        where RefType : ITextField<ValueType>
    {
        protected FieldType Field { get; private set; }
        public RefType FieldRef => Field;

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

        protected override void FillContent()
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

            Field.Format = null;
            Field.SubmitOnFocusLost = true;
        }
        public override void SetStyle(ControlStyle style)
        {
            Field.TextFieldStyle = style.TextField;
        }

        public void Edit() => Field.Focus();
        public override string ToString() => $"{base.ToString()}: {Field.Value}";

        public static implicit operator ValueType(FieldPropertyPanel<ValueType, FieldType, RefType> property) => property.Field.Value;
    }
    public abstract class ComparableFieldPropertyPanel<ValueType, FieldType, RefType> : FieldPropertyPanel<ValueType, FieldType, RefType>
                where ValueType : IComparable<ValueType>
        where FieldType : ComparableUITextField<ValueType>, RefType
        where RefType : IComparableField<ValueType>
    {
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
    public class FloatPropertyPanel : ComparableFieldPropertyPanel<float, FloatUITextField, IComparableField<float>> { }
    public class IntPropertyPanel : ComparableFieldPropertyPanel<int, IntUITextField, IComparableField<int>> { }
    public class StringPropertyPanel : FieldPropertyPanel<string, StringUITextField, ITextField<string>>
    {
        public override void DeInit()
        {
            base.DeInit();

            Field.Multiline = false;
            Field.textScale = StringUITextField.DefaultTextScale;
            Field.height = 20;
        }

        protected override void OnSizeChanged()
        {
            if (Field.Multiline)
                Field.height = height - ItemsPadding * 2f;
            else
                Field.height = 20;

            base.OnSizeChanged();
        }
    }

    public abstract class ComparableFieldRangePropertyPanel<ValueType, FieldType, RefType> : EditorPropertyPanel, IReusable
        where ValueType : IComparable<ValueType>
        where FieldType : ComparableUITextField<ValueType>, RefType
        where RefType : IComparableField<ValueType>
    {
        bool IReusable.InCache { get; set; }
        Transform IReusable.CachedTransform { get => m_CachedTransform; set => m_CachedTransform = value; }

        protected FieldType FieldA { get; private set; }
        protected FieldType FieldB { get; private set; }

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
            get => FieldA.SubmitOnFocusLost && FieldB.SubmitOnFocusLost;
            set
            {
                FieldA.SubmitOnFocusLost = value;
                FieldB.SubmitOnFocusLost = value;
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

        protected override void FillContent()
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
        public override void SetStyle(ControlStyle style)
        {
            FieldA.TextFieldStyle = style.TextField;
            FieldB.TextFieldStyle = style.TextField;
        }

        public override string ToString() => $"{base.ToString()}: from {ValueA} to {ValueB}";
    }

    public class FloatRangePropertyPanel : ComparableFieldRangePropertyPanel<float, FloatUITextField, IComparableField<float>> { }

    public abstract class InvertedFieldPropertyPanel<ValueType, FieldType, RefType> : ComparableFieldPropertyPanel<ValueType, FieldType, RefType>
        where ValueType : IComparable<ValueType>
        where FieldType : ComparableUITextField<ValueType>, RefType
        where RefType : IComparableField<ValueType>
    {
        protected CustomUIButton Invert { get; }

        public override float FieldWidth
        {
            get => base.FieldWidth + Content.Padding.horizontal + Invert.width;
            set => base.FieldWidth = value - Content.Padding.horizontal - Invert.width;
        }

        public InvertedFieldPropertyPanel()
        {
            Invert = Content.AddUIComponent<CustomUIButton>();
            Invert.SetDefaultStyle();
            Invert.width = 20;
            Invert.IconAtlas = CommonTextures.Atlas;
            Invert.AllIconSprites = CommonTextures.PlusMinusButton;
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

        public override void SetStyle(ControlStyle style)
        {
            base.SetStyle(style);

            Invert.ButtonStyle = style.SmallButton;
            Invert.IconAtlas = CommonTextures.Atlas;
            Invert.AllIconSprites = CommonTextures.PlusMinusButton;
        }
    }
    public class FloatInvertedPropertyPanel : InvertedFieldPropertyPanel<float, FloatUITextField, IComparableField<float>>
    {
        protected override float InvertValue(float value) => -value;
    }
}
