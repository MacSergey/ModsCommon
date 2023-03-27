using ColossalFramework.Math;
using ColossalFramework.UI;
using ModsCommon.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace ModsCommon.UI
{
    public class ColorPickerPopup : CustomUIPanel, IReusable
    {
        bool IReusable.InCache { get; set; }

        public event Action<Color> OnSelectedColorChanged;

        private static Texture2D BlankTexture { get; } = TextureHelper.CreateTexture(16, 16, Color.white);

        private bool InProcess { get; set; } = false;
        private CustomUITextureSprite HSBField { get; set; }
        private CustomUITextureSprite HueField { get; set; }
        private CustomUISlider HueSlider { get; set; }
        private CustomUISlider OpacitySlider { get; set; }
        private CustomUIButton Indicator { get; set; }


        private ByteUITextField RField { get; set; }
        private ByteUITextField GField { get; set; }
        private ByteUITextField BField { get; set; }
        private ByteUITextField AField { get; set; }

        private StringUITextField HEXField { get; set; }


        public Color32 Value
        {
            get => RGBAValue;
            set => ColorChanged(value, false, OnValueChanged);
        }
        private Color32 RGBAValue
        {
            get => new Color32(RField, GField, BField, AField);
        }
        private Color32 HSBAValue
        {
            get
            {
                var position = (Vector2)Indicator.relativePosition + Indicator.size * 0.5f;
                var xRatio = Mathf.Clamp01(position.x / HSBField.width);
                var yRatio = Mathf.Clamp01(position.y / HSBField.height);
                var hue = new HSBColor(HueSlider.Value, 1f, 1f, 1f).ToColor();
                var color = Color.Lerp(Color.white, hue, xRatio) * (1f - yRatio);
                color.a = OpacitySlider.Value;

                return color;
            }
        }

        public ColorPickerPopup()
        {
            PauseLayout(() =>
            {
                AutoLayout = AutoLayout.Vertical;
                AutoChildrenHorizontally = AutoLayoutChildren.Fit;
                AutoChildrenVertically = AutoLayoutChildren.Fit;

                Atlas = CommonTextures.Atlas;
                BackgroundSprite = CommonTextures.PanelLarge;
                BgColors = ComponentStyle.DarkPrimaryColor15;

                var pickerPanel = AddUIComponent<CustomUIPanel>();
                pickerPanel.PauseLayout(() =>
                {
                    pickerPanel.AutoLayout = AutoLayout.Horizontal;
                    pickerPanel.AutoChildrenHorizontally = AutoLayoutChildren.Fit;
                    pickerPanel.AutoChildrenVertically = AutoLayoutChildren.Fit;
                    pickerPanel.AutoLayoutSpace = 20;
                    pickerPanel.Padding = new RectOffset(10, 10, 10, 10);

                    HSBField = pickerPanel.AddUIComponent<CustomUITextureSprite>();
                    HSBField.name = nameof(HSBField);
                    HSBField.material = new Material(Shader.Find("UI/ColorPicker HSB"));
                    HSBField.texture = BlankTexture;
                    HSBField.size = new Vector2(200f, 200f);
                    HSBField.eventMouseDown += IndicatorDown;
                    HSBField.eventMouseMove += IndicatorMove;

                    Indicator = HSBField.AddUIComponent<CustomUIButton>();
                    Indicator.name = nameof(Indicator);
                    Indicator.isInteractive = false;
                    Indicator.Atlas = CommonTextures.Atlas;
                    Indicator.BgSprites = CommonTextures.Circle;
                    Indicator.FgSprites = CommonTextures.Circle;
                    Indicator.size = new Vector2(16f, 16f);
                    Indicator.SpritePadding = new RectOffset(2, 2, 2, 2);

                    HueField = pickerPanel.AddUIComponent<CustomUITextureSprite>();
                    HueField.name = nameof(HueField);
                    HueField.material = new Material(Shader.Find("UI/ColorPicker Hue"));
                    HueField.texture = BlankTexture;
                    HueField.size = new Vector2(18f, 200f);

                    HueSlider = HueField.AddUIComponent<CustomUISlider>();
                    HueSlider.name = nameof(HueSlider);
                    HueSlider.size = HueField.size;
                    HueSlider.Orientation = UIOrientation.Vertical;
                    HueSlider.relativePosition = Vector3.zero;
                    HueSlider.MinValue = 0f;
                    HueSlider.MaxValue = 1f;
                    HueSlider.StepSize = 0.01f;
                    HueSlider.OnSliderValueChanged += HueChanged;

                    HueSlider.ThumbAtlas = CommonTextures.Atlas;
                    HueSlider.ThumbSprites = CommonTextures.PanelSmall;
                    HueSlider.ThumbColors = ComponentStyle.FieldNormalColor;
                    HueSlider.ThumbSize = new Vector2(30f, 8f);


                    OpacitySlider = pickerPanel.AddUIComponent<CustomUISlider>();
                    OpacitySlider.name = nameof(OpacitySlider);
                    OpacitySlider.size = new Vector2(18f, 200f);
                    OpacitySlider.Orientation = UIOrientation.Vertical;
                    OpacitySlider.MinValue = 0f;
                    OpacitySlider.MaxValue = 1f;
                    OpacitySlider.StepSize = 0.01f;
                    OpacitySlider.OnSliderValueChanged += OpacityChanged;

                    OpacitySlider.BgAtlas = CommonTextures.Atlas;
                    OpacitySlider.BgSprite = CommonTextures.OpacitySliderBoard;
                    OpacitySlider.BgColor = Color.white;

                    OpacitySlider.FgAtlas = CommonTextures.Atlas;
                    OpacitySlider.FgSprite = CommonTextures.OpacitySliderColor;

                    OpacitySlider.ThumbAtlas = CommonTextures.Atlas;
                    OpacitySlider.ThumbSprites = CommonTextures.PanelSmall;
                    OpacitySlider.ThumbColors = ComponentStyle.FieldNormalColor;
                    OpacitySlider.ThumbSize = new Vector2(30f, 8f);
                });

                var valuePanel = AddUIComponent<CustomUIPanel>();
                valuePanel.PauseLayout(() =>
                {
                    valuePanel.AutoLayout = AutoLayout.Horizontal;
                    valuePanel.AutoLayoutSpace = 4;
                    valuePanel.AutoChildrenHorizontally = AutoLayoutChildren.Fit;
                    valuePanel.AutoChildrenVertically = AutoLayoutChildren.Fit;
                    valuePanel.AutoLayoutStart = LayoutStart.MiddleLeft;
                    valuePanel.Padding = new RectOffset(10, 10, 10, 10);

                    RField = AddField(valuePanel, "R", RGBChanged);
                    GField = AddField(valuePanel, "G", RGBChanged);
                    BField = AddField(valuePanel, "B", RGBChanged);
                    AField = AddField(valuePanel, "A", AChanged);

                    HEXField = valuePanel.AddUIComponent<StringUITextField>();
                    HEXField.SetDefaultStyle();
                    HEXField.Format = "#{0}";
                    HEXField.horizontalAlignment = UIHorizontalAlignment.Center;
                    HEXField.width = 72f;
                    HEXField.CheckValue = CheckHEXValue;
                    HEXField.OnValueChanged += HEXChanged;
                    //HEXField.eventGotFocus += FieldGotFocus;
                    //HEXField.eventLostFocus += FieldLostFocus;
                });
            });
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
        private string CheckHEXValue(string value)
        {
            byte r = 0;
            byte g = 0;
            byte b = 0;

            if (value.Length > 0)
            {
                if (value.StartsWith("#"))
                    value = value.Substring(1);

                if (value.Length < 2 || !byte.TryParse(value.Substring(0, 2), NumberStyles.HexNumber, null, out r))
                    r = 0;

                if (value.Length < 4 || !byte.TryParse(value.Substring(2, 2), NumberStyles.HexNumber, null, out g))
                    g = 0;

                if (value.Length < 6 || !byte.TryParse(value.Substring(4, 2), NumberStyles.HexNumber, null, out b))
                    b = 0;
            }

            return $"{r:X2}{g:X2}{b:X2}";
        }

        public void DeInit()
        {

        }

        #region HANDLERS

        protected void ColorChanged(Color32 color, bool callEvent = true, Action<Color32> action = null)
        {
            if (!InProcess)
            {
                InProcess = true;

                action(color);
                if (callEvent)
                    OnSelectedColorChanged?.Invoke(color);

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
                ColorChanged(RGBAValue, action: OnChangedRGBAValue);
        }
        private void AChanged(byte value)
        {
            if (!InProcess)
                ColorChanged(RGBAValue, action: OnChangedRGBAValue);
        }
        private void HEXChanged(string value)
        {
            if (!InProcess)
            {
                var r = byte.Parse(value.Substring(0, 2), NumberStyles.HexNumber, null);
                var g = byte.Parse(value.Substring(2, 2), NumberStyles.HexNumber, null);
                var b = byte.Parse(value.Substring(4, 2), NumberStyles.HexNumber, null);
                ColorChanged(new Color32(r, g, b, AField), action: OnChangedHEXValue);
            }
        }
        private void IndicatorDown(UIComponent comp, UIMouseEventParameter p) => IndicatorMoved(p);
        private void IndicatorMove(UIComponent comp, UIMouseEventParameter p)
        {
            if (p.buttons == UIMouseButton.Left)
                IndicatorMoved(p);
        }
        private void IndicatorMoved(UIMouseEventParameter p)
        {
            if (HSBField.GetHitPosition(p.ray, out var position))
            {
                Indicator.relativePosition = position - Indicator.size * 0.5f;
                var color = HSBAValue;
                ColorChanged(color, action: OnChangedIndicator);
            }
        }
        private void HueChanged(float value)
        {
            if (!InProcess)
            {
                var color = HSBAValue;
                ColorChanged(color, action: OnChangedHue);
            }
        }
        private void OpacityChanged(float value)
        {
            if (!InProcess)
            {
                var color = RGBAValue;
                color.a = (byte)Mathf.Clamp(value * byte.MaxValue, byte.MinValue, byte.MaxValue);

                ColorChanged(color, action: OnChangedOpacity);
            }
        }

        protected void OnValueChanged(Color32 color)
        {
            SetRGBValue(color);
            SetHEXValue(color);
            SetIndicator(color);
            SetIndicatorColor(color);
            SetHue(color);
            SetOpacity(color);
        }
        private void OnChangedIndicator(Color32 color)
        {
            SetIndicatorColor(color);
            SetOpacity(color);
            SetRGBValue(color);
            SetHEXValue(color);
        }
        private void OnChangedHue(Color32 color)
        {
            SetIndicator(color);
            SetIndicatorColor(color);
            SetOpacity(color);
            SetRGBValue(color);
            SetHEXValue(color);
        }
        private void OnChangedOpacity(Color32 color)
        {
            SetRGBValue(color);
        }
        private void OnChangedRGBAValue(Color32 color)
        {
            SetIndicator(color);
            SetIndicatorColor(color);
            SetHue(color);
            SetOpacity(color);
            SetHEXValue(color);
        }
        private void OnChangedHEXValue(Color32 color)
        {
            SetIndicator(color);
            SetIndicatorColor(color);
            SetHue(color);
            SetRGBValue(color);
        }

        private void SetIndicator(Color32 color)
        {
            var hsbColor = HSBColor.FromColor(color);
            var position = new Vector2(hsbColor.s * HSBField.width, (1f - hsbColor.b) * HSBField.height);
            Indicator.relativePosition = position - Indicator.size * 0.5f;

            if (HSBField.renderMaterial != null)
            {
                var hue = HSBColor.GetHue(color);
                HSBField.renderMaterial.color = hue.gamma;
            }
        }
        private void SetIndicatorColor(Color32 color)
        {
            color.a = 255;
            Indicator.FgColors = color;
        }
        private void SetHue(Color32 color)
        {
            var hsbColor = HSBColor.FromColor(color);
            HueSlider.Value = hsbColor.h;
        }
        private void SetOpacity(Color32 color)
        {
            OpacitySlider.Value = ((Color)color).a;
            color.a = byte.MaxValue;
            OpacitySlider.FgColor = color;
        }
        private void SetRGBValue(Color32 color)
        {
            RField.Value = color.r;
            GField.Value = color.g;
            BField.Value = color.b;
            AField.Value = color.a;
        }
        private void SetHEXValue(Color32 color)
        {
            HEXField.Value = $"{color.r:X2}{color.g:X2}{color.b:X2}";
        }

        #endregion
    }
}
