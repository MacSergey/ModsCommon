using ModsCommon.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class BaseVectorPropertyPanel<TypeVector> : EditorPropertyPanel, IReusable
        where TypeVector : struct
    {
        public event Action<TypeVector> OnValueChanged;

        bool IReusable.InCache { get; set; }
        protected abstract int Dimension { get; }

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

        protected CustomUILabel[] Labels { get; }
        protected FloatUITextField[] Fields { get; }
        public bool WheelTip
        {
            set
            {
                for (var i = 0; i < Dimension; i += 1)
                    Fields[i].WheelTip = value;
            }
        }

        public BaseVectorPropertyPanel()
        {
            Labels = new CustomUILabel[Dimension];
            Fields = new FloatUITextField[Dimension];

            for (var i = 0; i < Dimension; i += 1)
                AddField(i, out Labels[i], out Fields[i]);
        }

        public void Init(params int[] indexes)
        {
            foreach(var index in indexes)
            {
                Labels[index].isVisible = true;
                Fields[index].isVisible = true;
            }

            base.Init();
        }
        public override void DeInit()
        {
            base.DeInit();

            for (var i = 0; i < Dimension; i += 1)
            {
                Labels[i].isVisible = false;
                Fields[i].isVisible = false;
            }

            WheelTip = false;
            OnValueChanged = null;
        }

        protected abstract string GetName(int index);
        protected abstract float Get(ref TypeVector vector, int index);
        protected abstract void Set(ref TypeVector vector, int index, float value);

        protected void AddField(int index,  out CustomUILabel label, out FloatUITextField field)
        {
            label = Content.AddUIComponent<CustomUILabel>();
            label.isVisible = false;
            label.text = GetName(index);
            label.textScale = 0.7f;
            label.padding = new RectOffset(0, 0, 2, 0);

            field = Content.AddUIComponent<FloatUITextField>();
            field.isVisible = false;
            field.SetDefaultStyle();
            field.UseWheel = true;
            field.WheelStep = 1;
            field.width = 30;
            field.NumberFormat = "0.##";
            field.OnValueChanged += (value) => FieldChanged(index, value);
        }
        private void FieldChanged(int index, float value)
        {
            Set(ref _value, index, value);
            OnValueChanged?.Invoke(_value);
        }
    }

    public class Vector2PropertyPanel : BaseVectorPropertyPanel<Vector2>
    {
        protected override int Dimension => 2;

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
        protected override int Dimension => 3;

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
        protected override int Dimension => 4;

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
