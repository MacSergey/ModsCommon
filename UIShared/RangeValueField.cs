using ColossalFramework;
using ColossalFramework.PlatformServices;
using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModsCommon.UI
{
    public interface IValueFieldRange<ValueType>
        where ValueType : IComparable<ValueType>
    {
        RangeMode Mode { get; set; }
        bool AllowReverse { get; set; }
        bool CanInvert { get; set; }
        bool CanMirror { get; set; }
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
    }
    public abstract class ValueFieldRange<ValueType, FieldType, RefType> : CustomUIPanel, IValueFieldRange<ValueType>, IReusable
        where ValueType : IComparable<ValueType>
        where FieldType : ComparableUITextField<ValueType>
        where RefType : IValueFieldRange<ValueType>
    {
        bool IReusable.InCache { get; set; }
        Transform IReusable.CachedTransform { get => m_CachedTransform; set => m_CachedTransform = value; }

        public event Action<ValueType, ValueType> OnValueChanged;

        protected FieldType FieldA { get; private set; }
        protected FieldType FieldB { get; private set; }
        protected CustomUIButton Invert { get; private set; }
        protected CustomUIButton Mirror { get; private set; }

        private const float defaultFieldWidth = 100f;
        private float fieldWidth = defaultFieldWidth;

        private RangeMode mode;
        private bool allowReverse;
        private bool canInvert;
        private bool canMirror;

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
        public bool AllowReverse
        {
            get => allowReverse;
            set
            {
                if (value != allowReverse)
                {
                    allowReverse = value;
                    Refresh();
                }
            }
        }
        public bool CanInvert
        {
            get => canInvert;
            set
            {
                if (value != canInvert)
                {
                    canInvert = value;
                    Refresh();
                }
            }
        }
        public bool CanMirror
        {
            get => canMirror;
            set
            {
                if (value != canMirror)
                {
                    canMirror = value;
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

                Invert = AddUIComponent<CustomUIButton>();
                Invert.width = 20;
                Invert.eventClick += InvertClick;

                Mirror = AddUIComponent<CustomUIButton>();
                Mirror.width = 20;
                Mirror.eventClick += MirrorClick;

                FieldA.OnValueChanged += ValueAChanged;
                FieldB.OnValueChanged += ValueBChanged;
            });
        }

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

                        Invert.isVisible = CanInvert;
                        Mirror.isVisible = false;
                    }
                    break;
                case RangeMode.Range:
                    {
                        FieldB.isVisible = true;
                        FieldA.width = (FieldWidth - AutoLayoutSpace) * 0.5f;
                        FieldB.width = (FieldWidth - AutoLayoutSpace) * 0.5f;

                        if (AllowReverse)
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

                        Invert.isVisible = CanInvert;
                        Mirror.isVisible = CanMirror && AllowReverse;
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
            allowReverse = false;
            canInvert = false;
            canMirror = false;

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
                FieldB.Value = value;

            ValueChanged(FieldA.Value, FieldB.Value);
        }
        private void ValueBChanged(ValueType value)
        {
            if (Mode == RangeMode.Single)
                FieldA.Value = value;

            ValueChanged(FieldA.Value, FieldB.Value);
        }

        public void SetValues(ValueType valueA, ValueType valueB)
        {
            FieldA.Value = valueA;
            FieldB.Value = valueB;
        }

        private void InvertClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            ValueA = InvertValue(ValueA);
            ValueB = InvertValue(ValueB);

            switch(Mode)
            {
                case RangeMode.Single:
                    ValueChanged(ValueA, ValueA);
                    break;
                case RangeMode.Range:
                    ValueChanged(ValueA, ValueB);
                    break;
            }
        }

        protected abstract ValueType InvertValue(ValueType value);

        private void MirrorClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            if(Mode == RangeMode.Range)
            {
                SetValues(ValueB, ValueA);
                ValueChanged(ValueA, ValueB);
            }
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            SetSize();
        }
        protected virtual void SetSize()
        {
            if (Invert != null)
                Invert.height = height;

            if (Mirror != null)
                Mirror.height = height;
        }

        public void SetDefaultStyle()
        {
            FieldA.SetDefaultStyle();
            FieldB.SetDefaultStyle();
            Invert.SetDefaultStyle();
            Mirror.SetDefaultStyle();
        }
        public void SetStyle(ControlStyle style)
        {
            FieldA.TextFieldStyle = style.TextField;
            FieldB.TextFieldStyle = style.TextField;

            Invert.ButtonStyle = style.SmallButton;
            Invert.IconAtlas = CommonTextures.Atlas;
            Invert.AllIconSprites = CommonTextures.PlusMinusButton;

            Mirror.ButtonStyle = style.SmallButton;
            Mirror.IconAtlas = CommonTextures.Atlas;
            Mirror.AllIconSprites = CommonTextures.MirrorButton;
        }
    }

    public enum RangeMode
    {
        Range,
        Single,
    }

    public class IntRangeField : ValueFieldRange<int, IntUITextField, IValueFieldRange<int>> 
    {
        protected override int InvertValue(int value) => -value;
    }
    public class FloatRangeField : ValueFieldRange<float, FloatUITextField, IValueFieldRange<float>> 
    {
        protected override float InvertValue(float value) => -value;
    }
}
