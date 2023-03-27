using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Globalization;
using UnityEngine;

namespace ModsCommon.UI
{
    public class NewColorPropertyPanel : EditorPropertyPanel, IReusable
    {
        public event Action<Color32> OnValueChanged;

        bool IReusable.InCache { get; set; }
        private bool InProcess { get; set; } = false;

        private DefaultColorPickerButton ColorPicker { get; set; }
        private ByteUITextField RProperty { get; set; }
        private ByteUITextField GProperty { get; set; }
        private ByteUITextField BProperty { get; set; }
        private ByteUITextField AProperty { get; set; }

        public bool WheelTip
        {
            set
            {
                RProperty.WheelTip = value;
                GProperty.WheelTip = value;
                BProperty.WheelTip = value;
                AProperty.WheelTip = value;
            }
        }

        public Color32 Value
        {
            get => new Color32(RProperty, GProperty, BProperty, AProperty);
            set => ValueChanged(value, false, OnChangedValue);
        }

        protected override void FillContent()
        {
            ColorPicker = Content.AddUIComponent<DefaultColorPickerButton>();
            ColorPicker.size = new Vector2(20f, 20f);
            ColorPicker.Atlas = CommonTextures.Atlas;
            ColorPicker.BgSprites = CommonTextures.FieldSingle;


            RProperty = AddField(Content, "R", PropertyChangedRGB);
            GProperty = AddField(Content, "G", PropertyChangedRGB);
            BProperty = AddField(Content, "B", PropertyChangedRGB);
            AProperty = AddField(Content, "A", PropertyChangedA);
        }

        protected void ValueChanged(Color32 color, bool callEvent = true, Action<Color32> action = null)
        {
            if (!InProcess)
            {
                InProcess = true;

                action(color);
                if (callEvent)
                    OnValueChanged?.Invoke(Value);

                InProcess = false;
            }
        }

        private void PropertyChangedRGB(byte value)
        {
            if (Utility.AltIsPressed)
            {
                RProperty.Value = value;
                GProperty.Value = value;
                BProperty.Value = value;
            }
            if (!InProcess)
                ValueChanged(Value, action: OnChangedProperty);
        }
        private void PropertyChangedA(byte value)
        {
            if (!InProcess)
                ValueChanged(Value, action: OnChangedProperty);
        }

        protected void OnChangedValue(Color32 color)
        {
            SetProperties(color);
            //SetSample(color);
        }
        private void OnChangedProperty(Color32 color)
        {
            //SetSample(color);
        }

        private void SetProperties(Color32 color)
        {
            RProperty.Value = color.r;
            GProperty.Value = color.g;
            BProperty.Value = color.b;
            AProperty.Value = color.a;
        }

        public override void DeInit()
        {
            base.DeInit();

            WheelTip = false;
            OnValueChanged = null;
        }
        private ByteUITextField AddField(UIComponent parent, string name, Action<byte> onChanged)
        {
            var label = parent.AddUIComponent<CustomUILabel>();
            label.text = name;
            label.textScale = 0.7f;
            label.Padding = new RectOffset(0, 0, 2, 0);

            var field = parent.AddUIComponent<ByteUITextField>();
            field.SetDefaultStyle();
            field.MinValue = byte.MinValue;
            field.MaxValue = byte.MaxValue;
            field.CheckMax = true;
            field.CheckMin = true;
            field.UseWheel = true;
            field.WheelStep = 10;
            field.width = 30;
            field.OnValueChanged += onChanged;
            //field.eventGotFocus += FieldGotFocus;
            //field.eventLostFocus += FieldLostFocus;

            return field;
        }

        public override void SetStyle(ControlStyle style)
        {
            RProperty.TextFieldStyle = style.TextField;
            GProperty.TextFieldStyle = style.TextField;
            BProperty.TextFieldStyle = style.TextField;
            AProperty.TextFieldStyle = style.TextField;
        }
    }
}
