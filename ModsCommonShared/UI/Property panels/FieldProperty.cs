using System;

namespace ModsCommon.UI
{
    public abstract class FieldPropertyPanel<ValueType, FieldType> : EditorPropertyPanel, IReusable
        where FieldType : UITextField<ValueType>
    {
        bool IReusable.InCache { get; set; }
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
            Field = Content.AddUIComponent<FieldType>();
            Field.SetDefaultStyle();
            Field.name = nameof(Field);

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
        }

        public override void DeInit()
        {
            base.DeInit();

            UseWheel = false;
            WheelStep = default;
            WheelTip = string.Empty;
            CyclicalValue = false;
            Field.SetDefault();
        }
    }
    public class FloatPropertyPanel : ComparableFieldPropertyPanel<float, FloatUITextField> { }
    public class IntPropertyPanel : ComparableFieldPropertyPanel<int, IntUITextField> { }
    public class StringPropertyPanel : FieldPropertyPanel<string, StringUITextField> { }

}
