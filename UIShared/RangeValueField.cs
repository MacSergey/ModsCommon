using ColossalFramework;
using ColossalFramework.PlatformServices;
using ColossalFramework.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModsCommon.UI
{
    public interface IValueFieldRange<ValueType, FieldRefType>
        where ValueType : IComparable<ValueType>
        where FieldRefType : IFieldRef, IComparableField<ValueType>
    {
        RangeMode Mode { get; set; }
        bool AllowInvert { get; set; }
        float FieldWidth { get; set; }
        bool SubmitOnFocusLost { get; set; }
        ValueType ValueA { get; set; }
        ValueType ValueB { get; set; }
        string Format { set; }
        ValueType MinValue { get; set; }
        ValueType MaxValue { get; set; }
        bool CheckMin { get; set; }
        bool CheckMax { get; set; }
        bool CyclicalValue { get; set; }
        bool UseWheel { get; set; }
        ValueType WheelStep { set; }
        bool WheelTip { set; }
        FieldRefType FieldARef { get; }
        FieldRefType FieldBRef { get; }
    }
    public abstract class ValueFieldRange<ValueType, FieldType, FieldRefType, RefType> : CustomUIPanel, IValueFieldRange<ValueType, FieldRefType>, IReusable
        where ValueType : IComparable<ValueType>
        where FieldType : ComparableUITextField<ValueType, FieldRefType>
        where FieldRefType : IFieldRef, IComparableField<ValueType>
        where RefType : IFieldRef, IValueFieldRange<ValueType, FieldRefType>
    {
        public RefType Ref { get; }
        bool IReusable.InCache { get; set; }
        Transform IReusable.CachedTransform { get => m_CachedTransform; set => m_CachedTransform = value; }

        public event Action<ValueType, ValueType> OnValueChanged;

        protected FieldType FieldA { get; private set; }
        protected FieldType FieldB { get; private set; }

        public FieldRefType FieldARef => FieldA.Ref;
        public FieldRefType FieldBRef => FieldB.Ref;

        private const float defaultFieldWidth = 100f;
        private float fieldWidth = defaultFieldWidth;

        private RangeMode mode;
        private bool allowInvert;
        public RangeMode Mode
        {
            get => mode;
            set
            {
                if(value != mode)
                {
                    mode = value;
                    Refresh();
                }
            }
        }
        public bool AllowInvert
        {
            get => allowInvert;
            set
            {
                if (value != allowInvert)
                {
                    allowInvert = value;
                    Refresh();
                }
            }
        }

        public float FieldWidth
        {
            get => fieldWidth;
            set
            {
                if (value != fieldWidth)
                {
                    fieldWidth = value;
                }
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
            }
        }
        public ValueType ValueB
        {
            get => Mode switch
            {
                RangeMode.Range => FieldB,
                RangeMode.Single => FieldA,
                _ => default
            };
            set
            {
                FieldB.Value = value;
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

        private ValueType minValue;
        private ValueType maxValue;
        private bool checkMin;
        private bool checkMax;
        private bool cyclicalValue;

        public ValueType MinValue
        {
            get => minValue;
            set
            {
                if (value.CompareTo(minValue) != 0)
                {
                    minValue = value;
                }
            }
        }
        public ValueType MaxValue
        {
            get => maxValue;
            set
            {
                if (value.CompareTo(maxValue) != 0)
                {
                    maxValue = value;
                }
            }
        }
        public bool CheckMin
        {
            get => checkMin;
            set
            {
                if (value != checkMin)
                {
                    checkMin = value;
                }
            }
        }
        public bool CheckMax
        {
            get => checkMax;
            set
            {
                if (value != checkMax)
                {
                    checkMax = value;
                }
            }
        }
        public bool CyclicalValue
        {
            get => cyclicalValue;
            set
            {
                if (value != cyclicalValue)
                {
                    cyclicalValue = value;
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

        public ValueFieldRange()
        {
            Ref = CreateRef();

            autoLayout = AutoLayout.Horizontal;
            autoChildrenHorizontally = AutoLayoutChildren.Fit;
            autoChildrenVertically = AutoLayoutChildren.Fit;
            autoLayoutSpace = 5;

            PauseLayout(() =>
            {
                FieldA = AddUIComponent<FieldType>();
                FieldA.SetDefaultStyle();
                FieldA.name = nameof(FieldA);

                FieldB = AddUIComponent<FieldType>();
                FieldB.SetDefaultStyle();
                FieldB.name = nameof(FieldB);

                FieldA.OnValueChanged += ValueAChanged;
                FieldB.OnValueChanged += ValueBChanged;
            });
        }

        protected abstract RefType CreateRef();

        protected virtual void Refresh()
        {
            switch (Mode)
            {
                case RangeMode.Single:
                    {
                        FieldB.isVisible = false;
                        FieldA.width = FieldWidth;

                        FieldA.CheckMin = CheckMin;
                        FieldA.CheckMax = CheckMax;
                        FieldA.MinValue = MinValue;
                        FieldA.MaxValue = MaxValue;
                        FieldA.CyclicalValue = CyclicalValue;
                        FieldA.Value = FieldA.Value;
                        FieldB.Value = FieldA.Value;
                    }
                    break;
                case RangeMode.Range:
                    {
                        FieldB.isVisible = true;
                        FieldA.width = (FieldWidth - AutoLayoutSpace) * 0.5f;
                        FieldB.width = (FieldWidth - AutoLayoutSpace) * 0.5f;

                        if (AllowInvert)
                        {
                            FieldA.CheckMin = CheckMin;
                            FieldA.CheckMax = CheckMax;
                            FieldA.MinValue = MinValue;
                            FieldA.MaxValue = MaxValue;
                            FieldA.CyclicalValue = CyclicalValue;
                            FieldA.Value = FieldA.Value;

                            FieldB.CheckMin = CheckMin;
                            FieldB.CheckMax = CheckMax;
                            FieldB.MinValue = MinValue;
                            FieldB.MaxValue = MaxValue;
                            FieldB.CyclicalValue = CyclicalValue;
                            FieldB.Value = FieldB.Value;
                        }
                        else
                        {
                            FieldB.CheckMin = true;
                            FieldB.CheckMax = CheckMax;
                            FieldB.MinValue = FieldA.Value;
                            FieldB.MaxValue = MaxValue;
                            FieldB.CyclicalValue = false;
                            FieldB.Value = FieldB.Value;

                            FieldA.CheckMin = CheckMin;
                            FieldA.CheckMax = true;
                            FieldA.MinValue = MinValue;
                            FieldA.MaxValue = FieldB.Value;
                            FieldA.CyclicalValue = false;
                            FieldA.Value = FieldA.Value;
                        }
                    }
                    break;
            }
        }

        public virtual void DeInit()
        {
            OnValueChanged = null;
            SetDefault();
        }

        public virtual void SetDefault()
        {
            mode = RangeMode.Range;
            allowInvert = false;

            fieldWidth = defaultFieldWidth;
            checkMin = false;
            checkMax = false;
            minValue = default;
            maxValue = default;
            cyclicalValue = false;

            UseWheel = false;
            WheelStep = default;
            WheelTip = false;
            SubmitOnFocusLost = true;
            Format = null;

            FieldA.SetDefault();
            FieldB.SetDefault();
        }

        protected void ValueChanged(ValueType valueA, ValueType valueB) => OnValueChanged?.Invoke(valueA, valueB);

        private void ValueAChanged(ValueType value)
        {
            if (Mode == RangeMode.Single)
                ValueChanged(value, value);
            else
                ValueChanged(value, FieldB.Value);
        }
        private void ValueBChanged(ValueType value)
        {
            if (Mode == RangeMode.Single)
                ValueChanged(FieldA.Value, FieldA.Value);
            else
                ValueChanged(FieldA.Value, value);
        }

        public void SetValues(ValueType valueA, ValueType valueB)
        {
            FieldA.Value = valueA;
            FieldB.Value = valueB;
        }

        public void SetDefaultStyle()
        {
            FieldA.SetDefaultStyle();
            FieldB.SetDefaultStyle();
        }
        public void SetStyle(ControlStyle style)
        {
            FieldA.TextFieldStyle = style.TextField;
            FieldB.TextFieldStyle = style.TextField;
        }
    }

    public abstract class ValueFieldRangeRef<ValueType, RangeType, FieldRefType> : IFieldRef, IValueFieldRange<ValueType, FieldRefType>
        where ValueType : IComparable<ValueType>
        where FieldRefType : IFieldRef, IComparableField<ValueType>
        where RangeType : IValueFieldRange<ValueType, FieldRefType>
    {
        protected RangeType Range { get; }

        public ValueFieldRangeRef(RangeType range)
        {
            Range = range;
        }

        public RangeMode Mode 
        { 
            get => Range.Mode; 
            set => Range.Mode = value; 
        }
        public bool AllowInvert 
        { 
            get => Range.AllowInvert; 
            set => Range.AllowInvert = value; 
        }
        public float FieldWidth
        {
            get => Range.FieldWidth;
            set => Range.FieldWidth = value;
        }
        public bool SubmitOnFocusLost 
        { 
            get => Range.SubmitOnFocusLost; 
            set => Range.SubmitOnFocusLost = value; 
        }
        public ValueType ValueA 
        { 
            get => Range.ValueA; 
            set => Range.ValueA = value; 
        }
        public ValueType ValueB 
        { 
            get => Range.ValueB; 
            set => Range.ValueB = value; 
        }
        public string Format 
        { 
            set => Range.Format = value; 
        }
        public ValueType MinValue 
        { 
            get => Range.MinValue; 
            set => Range.MinValue = value; 
        }
        public ValueType MaxValue 
        { 
            get => Range.MaxValue; 
            set => Range.MaxValue = value; 
        }
        public bool CheckMin 
        { 
            get => Range.CheckMin; 
            set => Range.CheckMin = value; 
        }
        public bool CheckMax 
        { 
            get => Range.CheckMax; 
            set => Range.CheckMax = value; 
        }
        public bool CyclicalValue 
        { 
            get => Range.CyclicalValue; 
            set => Range.CyclicalValue = value; 
        }
        public bool UseWheel 
        { 
            get => Range.UseWheel; 
            set => Range.UseWheel = value; 
        }
        public ValueType WheelStep 
        { 
            set => Range.WheelStep = value; 
        }
        public bool WheelTip 
        { 
            set => Range.WheelTip = value; 
        }
        public FieldRefType FieldARef => Range.FieldARef;
        public FieldRefType FieldBRef => Range.FieldBRef;
    }

    public enum RangeMode
    {
        Range,
        Single,
    }

    public class IntRangeField : ValueFieldRange<int, IntUITextField, IntUITextField.IntFieldRef, IntRangeField.IntRangeFieldRef> 
    {
        protected override IntRangeFieldRef CreateRef() => new(this);

        public class IntRangeFieldRef : ValueFieldRangeRef<int, IntRangeField, IntUITextField.IntFieldRef>
        {
            public IntRangeFieldRef(IntRangeField range) : base(range) { }
        }
    }
    public class FloatRangeField : ValueFieldRange<float, FloatUITextField, FloatUITextField.FloatFieldRef, FloatRangeField.FloatRangeFieldRef> 
    {
        protected override FloatRangeFieldRef CreateRef() => new(this);

        public class FloatRangeFieldRef : ValueFieldRangeRef<float, FloatRangeField, FloatUITextField.FloatFieldRef>
        {
            public FloatRangeFieldRef(FloatRangeField range) : base(range) { }
        }
    }
}
