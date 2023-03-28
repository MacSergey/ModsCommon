using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using UnityEngine;

namespace ModsCommon.UI
{
    public class ColorPropertyPanel<ColorPickerType, PopupType> : EditorPropertyPanel, IReusable
        where ColorPickerType : ColorPickerButton<PopupType>
        where PopupType : ColorPickerPopup
    {
        public event Action<Color32> OnValueChanged;

        bool IReusable.InCache { get; set; }
        private bool InProcess { get; set; } = false;

        protected ColorPickerType ColorPicker { get; set; }
        protected ByteUITextField RField { get; set; }
        protected ByteUITextField GField { get; set; }
        protected ByteUITextField BField { get; set; }
        protected ByteUITextField AField { get; set; }

        private CustomUILabel RLabel { get; set; }
        private CustomUILabel GLabel { get; set; }
        private CustomUILabel BLabel { get; set; }
        private CustomUILabel ALabel { get; set; }

        public bool WheelTip
        {
            set
            {
                RField.WheelTip = value;
                GField.WheelTip = value;
                BField.WheelTip = value;
                AField.WheelTip = value;
            }
        }

        public Color32 Value
        {
            get => new Color32(RField, GField, BField, AField);
            set => ValueChanged(value, false, OnChangedValue);
        }

        protected override void FillContent()
        {
            ColorPicker = Content.AddUIComponent<ColorPickerType>();
            ColorPicker.size = new Vector2(20f, 20f);

            ColorPicker.OnAfterPopupOpen += ColorPickerOpen;
            ColorPicker.OnSelectedColorChanged += PickerChanged;

            RLabel = AddLabel(Content, "R");
            RField = AddField(Content, RGBChanged);
            GLabel = AddLabel(Content, "G");
            GField = AddField(Content, RGBChanged);
            BLabel = AddLabel(Content, "B");
            BField = AddField(Content, RGBChanged);
            ALabel = AddLabel(Content, "A");
            AField = AddField(Content, AChanged);
        }

        private void ColorPickerOpen(ColorPickerPopup popup)
        {
            popup.SelectedColor = Value;
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

        private void RGBChanged(byte value)
        {
            if (Utility.AltIsPressed)
            {
                RField.Value = value;
                GField.Value = value;
                BField.Value = value;
            }
            if (!InProcess)
                ValueChanged(Value, action: OnChangedRGBA);
        }
        private void AChanged(byte value)
        {
            if (!InProcess)
                ValueChanged(Value, action: OnChangedRGBA);
        }
        private void PickerChanged(Color32 color)
        {
            if (!InProcess)
                ValueChanged(color, action: OnChangedPicker);
        }

        protected void OnChangedValue(Color32 color)
        {
            SetFields(color);
            SetPicker(color);
        }
        private void OnChangedRGBA(Color32 color)
        {
            SetFields(color);
            SetPicker(color);
        }
        private void OnChangedPicker(Color32 color)
        {
            SetFields(color);
        }

        private void SetFields(Color32 color)
        {
            RField.Value = color.r;
            GField.Value = color.g;
            BField.Value = color.b;
            AField.Value = color.a;
        }
        private void SetPicker(Color32 color)
        {
            ColorPicker.SelectedColor = color;
        }

        public override void DeInit()
        {
            base.DeInit();

            WheelTip = false;
            OnValueChanged = null;
        }
        private CustomUILabel AddLabel(UIComponent parent, string name)
        {
            var label = parent.AddUIComponent<CustomUILabel>();
            label.text = name;
            label.textScale = 0.7f;
            label.Padding = new RectOffset(0, 0, 2, 0);
            return label;
        }
        private ByteUITextField AddField(UIComponent parent, Action<byte> onChanged)
        {
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
            ColorPicker.ColorPickerStyle = style.ColorPicker;

            RField.TextFieldStyle = style.TextField;
            GField.TextFieldStyle = style.TextField;
            BField.TextFieldStyle = style.TextField;
            AField.TextFieldStyle = style.TextField;

            RLabel.LabelStyle = style.Label;
            GLabel.LabelStyle = style.Label;
            BLabel.LabelStyle = style.Label;
            ALabel.LabelStyle = style.Label;
        }
    }
}
