using ColossalFramework.UI;
using ModsCommon.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class FieldPropertyPanel<ValueType, FieldType> : EditorPropertyPanel, IReusable
        where FieldType : UITextField<ValueType>
    {
        protected FieldType Field { get; set; }

        public event Action<ValueType> OnValueChanged;

        public float FieldWidth
        {
            get => Field.width;
            set => Field.width = value;
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

        public FieldPropertyPanel()
        {
            Field = Control.AddUIComponent<FieldType>();
            Field.SetDefaultStyle();

            Field.OnValueChanged += ValueChanged;
        }

        private void ValueChanged(ValueType value) => OnValueChanged?.Invoke(value);

        public override void DeInit()
        {
            base.DeInit();

            SubmitOnFocusLost = true;

            OnValueChanged = null;
        }
        public void Edit() => Field.Focus();
        public override string ToString() => Value.ToString();

        public static implicit operator ValueType(FieldPropertyPanel<ValueType, FieldType> property) => property.Value;
    }
    public abstract class ComparableFieldPropertyPanel<ValueType, FieldType> : FieldPropertyPanel<ValueType, FieldType>, IWheelChangeable
        where FieldType : ComparableUITextField<ValueType>
        where ValueType : IComparable<ValueType>
    {
        public event Action OnStartWheel;
        public event Action OnStopWheel;

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
        public bool CheckMax
        {
            get => Field.CheckMax;
            set => Field.CheckMax = value;
        }
        public bool CheckMin
        {
            get => Field.CheckMin;
            set => Field.CheckMin = value;
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
        public string WheelTip
        {
            get => Field.WheelTip;
            set => Field.WheelTip = value;
        }

        public ComparableFieldPropertyPanel()
        {
            Field.SetDefault();

            Field.eventMouseHover += FieldHover;
            Field.eventMouseLeave += FieldLeave;
        }

        private void FieldHover(UIComponent component, UIMouseEventParameter eventParam) => OnStartWheel?.Invoke();
        private void FieldLeave(UIComponent component, UIMouseEventParameter eventParam) => OnStartWheel?.Invoke();

        public override void DeInit()
        {
            base.DeInit();

            UseWheel = false;
            WheelStep = default;
            WheelTip = string.Empty;
            CyclicalValue = false;
            Field.SetDefault();

            OnStartWheel = null;
            OnStopWheel = null;
        }
    }
    public class FloatPropertyPanel : ComparableFieldPropertyPanel<float, FloatUITextField> { }
    public class IntPropertyPanel : ComparableFieldPropertyPanel<int, IntUITextField> { }
    public class StringPropertyPanel : FieldPropertyPanel<string, StringUITextField> { }

}
