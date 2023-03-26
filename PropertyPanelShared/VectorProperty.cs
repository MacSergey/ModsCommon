using System;
using System.Linq;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class BaseVectorPropertyPanel<TypeVector> : EditorPropertyPanel, IReusable
        where TypeVector : struct
    {
        public struct Field
        {
            BaseVectorPropertyPanel<TypeVector> Property { get; }
            int Index { get; }

            public float Value
            {
                get => Property.Fields[Index].Value;
                set => Property.Fields[Index].Value = value;
            }
            public float Width
            {
                get => Property.Fields[Index].width;
                set => Property.Fields[Index].width = value;
            }
            public string Format
            {
                set => Property.Fields[Index].Format = value;
            }
            public string NumberFormat
            {
                set => Property.Fields[Index].NumberFormat = value;
            }
            public Color32 FieldTextColor
            {
                set => Property.Fields[Index].textColor = value;
            }
            public bool SubmitOnFocusLost
            {
                get => Property.Fields[Index].submitOnFocusLost;
                set => Property.Fields[Index].submitOnFocusLost = value;
            }
            public float MinValue
            {
                get => Property.Fields[Index].MinValue;
                set => Property.Fields[Index].MinValue = value;
            }
            public float MaxValue
            {
                get => Property.Fields[Index].MaxValue;
                set => Property.Fields[Index].MaxValue = value;
            }
            public bool CheckMin
            {
                get => Property.Fields[Index].CheckMin;
                set => Property.Fields[Index].CheckMin = value;
            }
            public bool CheckMax
            {
                get => Property.Fields[Index].CheckMax;
                set => Property.Fields[Index].CheckMax = value;
            }
            public bool CyclicalValue
            {
                get => Property.Fields[Index].CyclicalValue;
                set => Property.Fields[Index].CyclicalValue = value;
            }
            public bool UseWheel
            {
                get => Property.Fields[Index].UseWheel;
                set => Property.Fields[Index].UseWheel = value;
            }
            public float WheelStep
            {
                get => Property.Fields[Index].WheelStep;
                set => Property.Fields[Index].WheelStep = value;
            }
            public bool WheelTip
            {
                set => Property.Fields[Index].WheelTip = value;
            }


            public Field(BaseVectorPropertyPanel<TypeVector> property, int index)
            {
                Property = property;
                Index = index;
            }
        }
        public event Action<TypeVector> OnValueChanged;

        bool IReusable.InCache { get; set; }
        public abstract uint Dimension { get; }

        public Field this[int index]
        {
            get
            {
                if (index < 0 && index >= Dimension)
                    throw new IndexOutOfRangeException();
                else
                    return new Field(this, index);
            }
        }

        TypeVector _value;
        public TypeVector Value
        {
            get => _value;
            set
            {
                _value = value;
                for (var i = 0; i < Dimension; i += 1)
                    Fields[i].Value = Get(ref _value, i);
            }
        }
        public string Format
        {
            set
            {
                for (var i = 0; i < Dimension; i += 1)
                    Fields[i].Format = value;
            }
        }
        public string NumberFormat
        {
            set
            {
                for (var i = 0; i < Dimension; i += 1)
                    Fields[i].NumberFormat = value;
            }
        }

        float _fieldsWidth = 30f;
        public float FieldsWidth
        {
            get => _fieldsWidth;
            set
            {
                if (value != _fieldsWidth && value > 0)
                {
                    _fieldsWidth = value;
                    foreach (var field in Fields)
                        field.width = _fieldsWidth;
                }
            }
        }

        protected CustomUILabel[] Labels { get; private set; }
        protected FloatUITextField[] Fields { get; private set; }

        public Color32 FieldTextColor
        {
            set
            {
                for (var i = 0; i < Dimension; i += 1)
                    Fields[i].textColor = value;
            }
        }
        public bool SubmitOnFocusLost
        {
            get => Fields.All(f => f.submitOnFocusLost);
            set
            {
                for (var i = 0; i < Dimension; i += 1)
                    Fields[i].submitOnFocusLost = value;
            }
        }

        public TypeVector MinValue
        {
            get
            {
                var value = default(TypeVector);
                for (var i = 0; i < Dimension; i += 1)
                    Set(ref value, i, Fields[i].MinValue);
                return value;
            }
            set
            {
                for (var i = 0; i < Dimension; i += 1)
                    Fields[i].MinValue = Get(ref value, i);
            }
        }
        public TypeVector MaxValue
        {
            get
            {
                var value = default(TypeVector);
                for (var i = 0; i < Dimension; i += 1)
                    Set(ref value, i, Fields[i].MaxValue);
                return value;
            }
            set
            {
                for (var i = 0; i < Dimension; i += 1)
                    Fields[i].MaxValue = Get(ref value, i);
            }
        }
        public bool CheckMin
        {
            get => Fields.All(f => f.CheckMin);
            set
            {
                for (var i = 0; i < Dimension; i += 1)
                    Fields[i].CheckMin = value;
            }
        }
        public bool CheckMax
        {
            get => Fields.All(f => f.CheckMax);
            set
            {
                for (var i = 0; i < Dimension; i += 1)
                    Fields[i].CheckMax = value;
            }
        }
        public bool CyclicalValue
        {
            get => Fields.All(f => f.CyclicalValue);
            set
            {
                for (var i = 0; i < Dimension; i += 1)
                    Fields[i].CyclicalValue = value;
            }
        }
        public bool UseWheel
        {
            get => Fields.All(f => f.UseWheel);
            set
            {
                for (var i = 0; i < Dimension; i += 1)
                    Fields[i].UseWheel = value;
            }
        }
        public TypeVector WheelStep
        {
            get
            {
                var value = default(TypeVector);
                for (var i = 0; i < Dimension; i += 1)
                    Set(ref value, i, Fields[i].WheelStep);
                return value;
            }
            set
            {
                for (var i = 0; i < Dimension; i += 1)
                    Fields[i].WheelStep = Get(ref value, i);
            }
        }
        public bool WheelTip
        {
            set
            {
                for (var i = 0; i < Dimension; i += 1)
                    Fields[i].WheelTip = value;
            }
        }

        protected override void FillContent()
        {
            Labels = new CustomUILabel[Dimension];
            Fields = new FloatUITextField[Dimension];

            for (var i = 0; i < Dimension; i += 1)
                AddField(i, out Labels[i], out Fields[i]);
        }
        public void Init(params int[] indexes)
        {
            for (var i = 0; i < indexes.Length; i += 1)
            {
                if (indexes[i] < Dimension)
                {
                    Labels[indexes[i]].isVisible = true;
                    Fields[indexes[i]].isVisible = true;

                    Labels[indexes[i]].zOrder = i * 2;
                    Fields[indexes[i]].zOrder = i * 2 + 1;
                }
            }

            base.Init();
        }
        public void SetLabels(params string[] labels)
        {
            for (var i = 0; i < labels.Length && i < Dimension; i += 1)
                Labels[i].text = labels[i];
        }
        public override void DeInit()
        {
            base.DeInit();

            for (var i = 0; i < Dimension; i += 1)
            {
                Labels[i].isVisible = false;
                Labels[i].text = GetName(i);
                Fields[i].isVisible = false;
                Fields[i].SetDefault();
            }

            OnValueChanged = null;
        }

        protected abstract string GetName(int index);
        protected abstract float Get(ref TypeVector vector, int index);
        protected abstract void Set(ref TypeVector vector, int index, float value);

        protected void AddField(int index, out CustomUILabel label, out FloatUITextField field)
        {
            label = Content.AddUIComponent<CustomUILabel>();
            label.isVisible = false;
            label.text = GetName(index);
            label.textScale = 0.7f;
            label.Padding = new RectOffset(0, 0, 2, 0);

            field = Content.AddUIComponent<FloatUITextField>();
            field.isVisible = false;
            field.SetDefaultStyle();
            field.UseWheel = true;
            field.WheelStep = 1;
            field.width = FieldsWidth;
            field.NumberFormat = "0.##";
            field.OnValueChanged += (value) => FieldChanged(index, value);
        }
        private void FieldChanged(int index, float value)
        {
            Set(ref _value, index, value);
            OnValueChanged?.Invoke(_value);
        }
        public override void SetStyle(ControlStyle style)
        {
            foreach (var field in Fields)
                field.TextFieldStyle = style.TextField;
        }
    }

    public class Vector2PropertyPanel : BaseVectorPropertyPanel<Vector2>
    {
        public override uint Dimension => 2;

        protected override float Get(ref Vector2 vector, int index) => vector[index];
        protected override void Set(ref Vector2 vector, int index, float value) => vector[index] = value;
        protected override string GetName(int index) => index switch
        {
            0 => "X",
            1 => "Y",
            _ => "?",
        };
    }
    public class Vector3PropertyPanel : BaseVectorPropertyPanel<Vector3>
    {
        public override uint Dimension => 3;

        protected override float Get(ref Vector3 vector, int index) => vector[index];
        protected override void Set(ref Vector3 vector, int index, float value) => vector[index] = value;
        protected override string GetName(int index) => index switch
        {
            0 => "X",
            1 => "Y",
            2 => "Z",
            _ => "?",
        };
    }
    public class Vector4PropertyPanel : BaseVectorPropertyPanel<Vector4>
    {
        public override uint Dimension => 4;

        protected override float Get(ref Vector4 vector, int index) => vector[index];
        protected override void Set(ref Vector4 vector, int index, float value) => vector[index] = value;
        protected override string GetName(int index) => index switch
        {
            0 => "X",
            1 => "Y",
            2 => "Z",
            3 => "W",
            _ => "?",
        };
    }
}
