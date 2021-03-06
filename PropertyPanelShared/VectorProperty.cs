﻿using ModsCommon.UI;
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
        protected abstract uint Dimension { get; }

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
        float _fieldsWidth = 30f;
        public float FieldsWidth
        {
            get => _fieldsWidth;
            set
            {
                if(value != _fieldsWidth && value > 0)
                {
                    _fieldsWidth = value;
                    foreach (var field in Fields)
                        field.width = _fieldsWidth;
                }
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
            for(var i = 0; i < indexes.Length; i += 1)
            {
                Labels[indexes[i]].isVisible = true;
                Fields[indexes[i]].isVisible = true;

                Labels[indexes[i]].zOrder = i * 2;
                Fields[indexes[i]].zOrder = i * 2 + 1;
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
            field.width = FieldsWidth;
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
        protected override uint Dimension => 2;

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
        protected override uint Dimension => 3;

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
        protected override uint Dimension => 4;

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
