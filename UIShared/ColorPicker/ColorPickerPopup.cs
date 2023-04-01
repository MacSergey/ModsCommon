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

        public event Action<Color32> OnSelectedColorChanged;

        private static Texture2D BlankTexture { get; } = TextureHelper.CreateTexture(16, 16, Color.white);

        private bool InProcess { get; set; } = false;
        public bool AutoClose { get; protected set; } = true;
        private CustomUITextureSprite HSBField { get; set; }
        private CustomUITextureSprite HueField { get; set; }
        private CustomUISlider HueSlider { get; set; }
        private CustomUISlider OpacitySlider { get; set; }
        private CustomUIButton HSBIndicator { get; set; }
        private CustomUIButton HueIndicator { get; set; }
        private CustomUIButton OpacityIndicator { get; set; }


        private ByteUITextField RField { get; set; }
        private ByteUITextField GField { get; set; }
        private ByteUITextField BField { get; set; }
        private ByteUITextField AField { get; set; }

        private StringUITextField HEXField { get; set; }

        private RectOffset HSBPadding { get; } = new RectOffset(6, 6, 6, 6);

        public virtual ColorPickerStyle ColorPickerStyle
        {
            set
            {
                RField.BgColors = value.TextField.BgColors;
                RField.textColor = value.TextField.TextColors.normal;
                RField.selectionBackgroundColor = value.TextField.SelectionColor;

                GField.BgColors = value.TextField.BgColors;
                GField.textColor = value.TextField.TextColors.normal;
                GField.selectionBackgroundColor = value.TextField.SelectionColor;

                BField.BgColors = value.TextField.BgColors;
                BField.textColor = value.TextField.TextColors.normal;
                BField.selectionBackgroundColor = value.TextField.SelectionColor;

                AField.BgColors = value.TextField.BgColors;
                AField.textColor = value.TextField.TextColors.normal;
                AField.selectionBackgroundColor = value.TextField.SelectionColor;

                HEXField.BgColors = value.TextField.BgColors;
                HEXField.textColor = value.TextField.TextColors.normal;
                HEXField.selectionBackgroundColor = value.TextField.SelectionColor;
            }
        }


        public Color32 SelectedColor
        {
            get => RGBAValue;
            set => ColorChanged(value, false, OnValueChanged);
        }
        private Color HueColor { get; set; }
        private Color32 RGBAValue
        {
            get => new Color32(RField, GField, BField, AField);
        }
        private Color HSBAValue
        {
            get
            {
                var position = (Vector2)HSBIndicator.relativePosition.RoundToInt() - new Vector2(HSBPadding.left, HSBPadding.top) + HSBIndicator.size * 0.5f;
                var xRatio = Mathf.Clamp01(position.x / (HSBField.width - HSBPadding.horizontal));
                var yRatio = Mathf.Clamp01(position.y / (HSBField.height - HSBPadding.vertical));
                var color = Color.Lerp(Color.white, HueColor, xRatio) * (1f - yRatio);
                color.a = OpacitySlider.Value;

                return color;
            }
        }

        public ColorPickerPopup()
        {
            builtinKeyNavigation = true;

            PauseLayout(() =>
            {
                AutoLayout = AutoLayout.Vertical;
                AutoChildrenHorizontally = AutoLayoutChildren.Fit;
                AutoChildrenVertically = AutoLayoutChildren.Fit;
                AutoLayoutStart = LayoutStart.TopCentre;
                Padding = new RectOffset(10, 10, 10, 10);
                autoLayoutSpace = 15;

                Atlas = CommonTextures.Atlas;
                BackgroundSprite = CommonTextures.PanelLarge;
                BgColors = ComponentStyle.DarkPrimaryColor15;

                FillPopup();
            });
        }
        protected virtual void FillPopup()
        {
            var pickerPanel = AddUIComponent<CustomUIPanel>();
            pickerPanel.PauseLayout(() =>
            {
                pickerPanel.AutoLayout = AutoLayout.Horizontal;
                pickerPanel.AutoChildrenHorizontally = AutoLayoutChildren.Fit;
                pickerPanel.AutoChildrenVertically = AutoLayoutChildren.Fit;
                pickerPanel.AutoLayoutSpace = 15;

                HSBField = pickerPanel.AddUIComponent<CustomUITextureSprite>();
                HSBField.name = nameof(HSBField);
                HSBField.material = new Material(Shader.Find("UI/ColorPicker HSB"));
                HSBField.texture = BlankTexture;
                HSBField.size = new Vector2(200f, 200f);
                HSBField.canFocus = true;
                HSBField.eventMouseDown += IndicatorDown;
                HSBField.eventMouseMove += IndicatorMove;

                var hsbMask = HSBField.AddUIComponent<CustomUISlicedSprite>();
                hsbMask.atlas = CommonTextures.Atlas;
                hsbMask.spriteName = CommonTextures.OpacitySliderMask;
                hsbMask.color = ComponentStyle.DarkPrimaryColor15;
                hsbMask.size = HSBField.size;
                hsbMask.relativePosition = Vector3.zero;

                HSBIndicator = HSBField.AddUIComponent<CustomUIButton>();
                HSBIndicator.name = nameof(HSBIndicator);
                HSBIndicator.isInteractive = false;
                HSBIndicator.Atlas = CommonTextures.Atlas;
                HSBIndicator.BgSprites = CommonTextures.Circle;
                HSBIndicator.FgSprites = CommonTextures.Circle;
                HSBIndicator.size = new Vector2(16f, 16f);
                HSBIndicator.SpritePadding = new RectOffset(2, 2, 2, 2);

                HueField = pickerPanel.AddUIComponent<CustomUITextureSprite>();
                HueField.name = nameof(HueField);
                HueField.material = new Material(Shader.Find("UI/ColorPicker Hue"));
                HueField.texture = BlankTexture;
                HueField.size = new Vector2(12f, 200f);

                HueSlider = HueField.AddUIComponent<CustomUISlider>();
                HueSlider.name = nameof(HueSlider);
                HueSlider.size = HueField.size;
                HueSlider.Orientation = UIOrientation.Vertical;
                HueSlider.relativePosition = Vector3.zero;
                HueSlider.MinValue = 0f;
                HueSlider.MaxValue = 1f;
                HueSlider.StepSize = 0.01f;
                HueSlider.ScrollWheelAmount = 0.02f;
                HueSlider.OnSliderValueChanged += HueChanged;

                HueSlider.FgAtlas = CommonTextures.Atlas;
                HueSlider.FgSprite = CommonTextures.OpacitySliderMask;
                HueSlider.FgColor = ComponentStyle.DarkPrimaryColor15;

                HueSlider.ThumbPadding = new RectOffset(0, 0, 6, 6);
                HueSlider.ThumbSize = new Vector2(16f, 16f);
                HueSlider.OnThumbPositionChanged += (pos) => HueIndicator.relativePosition = pos;

                HueIndicator = HueSlider.AddUIComponent<CustomUIButton>();
                HueIndicator.name = nameof(HSBIndicator);
                HueIndicator.isInteractive = false;
                HueIndicator.Atlas = CommonTextures.Atlas;
                HueIndicator.BgSprites = CommonTextures.Circle;
                HueIndicator.FgSprites = CommonTextures.Circle;
                HueIndicator.size = new Vector2(16f, 16f);
                HueIndicator.SpritePadding = new RectOffset(2, 2, 2, 2);
                HueIndicator.relativePosition = HueSlider.ThumbPosition;


                var opacityField = pickerPanel.AddUIComponent<CustomUISprite>();
                opacityField.atlas = CommonTextures.Atlas;
                opacityField.spriteName = CommonTextures.OpacitySliderBoard;
                opacityField.size = new Vector2(12f, 200f);

                OpacitySlider = opacityField.AddUIComponent<CustomUISlider>();
                OpacitySlider.name = nameof(OpacitySlider);
                OpacitySlider.size = new Vector2(12f, 200f);
                OpacitySlider.Orientation = UIOrientation.Vertical;
                OpacitySlider.relativePosition = Vector3.zero;
                OpacitySlider.MinValue = 0f;
                OpacitySlider.MaxValue = 1f;
                OpacitySlider.StepSize = 0.01f;
                OpacitySlider.ScrollWheelAmount = 0.02f;
                OpacitySlider.OnSliderValueChanged += OpacityChanged;

                OpacitySlider.BgAtlas = CommonTextures.Atlas;
                OpacitySlider.BgSprite = CommonTextures.OpacitySliderColor;
                OpacitySlider.BgColor = Color.white;

                OpacitySlider.FgAtlas = CommonTextures.Atlas;
                OpacitySlider.FgSprite = CommonTextures.OpacitySliderMask;
                OpacitySlider.FgColor = ComponentStyle.DarkPrimaryColor15;

                OpacitySlider.ThumbPadding = new RectOffset(0, 0, 6, 6);
                OpacitySlider.ThumbSize = new Vector2(16f, 16f);
                OpacitySlider.OnThumbPositionChanged += (pos) => OpacityIndicator.relativePosition = pos;

                OpacityIndicator = OpacitySlider.AddUIComponent<CustomUIButton>();
                OpacityIndicator.name = nameof(HSBIndicator);
                OpacityIndicator.isInteractive = false;
                OpacityIndicator.Atlas = CommonTextures.Atlas;
                OpacityIndicator.BgSprites = CommonTextures.Circle;
                OpacityIndicator.FgSprites = CommonTextures.Circle;
                OpacityIndicator.size = new Vector2(16f, 16f);
                OpacityIndicator.SpritePadding = new RectOffset(2, 2, 2, 2);
                OpacityIndicator.relativePosition = OpacitySlider.ThumbPosition;
            });

            var valuePanel = AddUIComponent<CustomUIPanel>();
            valuePanel.PauseLayout(() =>
            {
                valuePanel.AutoLayout = AutoLayout.Horizontal;
                valuePanel.AutoChildrenHorizontally = AutoLayoutChildren.Fit;
                valuePanel.AutoChildrenVertically = AutoLayoutChildren.Fit;
                valuePanel.AutoLayoutStart = LayoutStart.MiddleLeft;

                RField = AddField<byte, ByteUITextField>(valuePanel, "R", RGBChanged);
                RField.BgSprites = CommonTextures.FieldLeft;
                SetField(RField);

                GField = AddField<byte, ByteUITextField>(valuePanel, "G", RGBChanged);
                GField.BgSprites = CommonTextures.FieldMiddle;
                SetField(GField);

                BField = AddField<byte, ByteUITextField>(valuePanel, "B", RGBChanged);
                BField.BgSprites = CommonTextures.FieldMiddle;
                SetField(BField);

                AField = AddField<byte, ByteUITextField>(valuePanel, "A", AChanged);
                AField.BgSprites = CommonTextures.FieldRight;
                SetField(AField);

                HEXField = AddField<string, StringUITextField>(valuePanel, "HEX", HEXChanged);
                HEXField.SetDefaultStyle();
                HEXField.Format = "#{0}";
                HEXField.horizontalAlignment = UIHorizontalAlignment.Center;
                HEXField.width = 72f;
                HEXField.CheckValue = CheckHEXValue;
                HEXField.OnValueChanged += HEXChanged;

                valuePanel.SetItemMargin(HEXField.parent, new RectOffset(10, 0, 0, 0));
            });
        }
        private FieldType AddField<ValueType, FieldType>(UIComponent parent, string name, Action<ValueType> onChanged)
            where FieldType : UITextField<ValueType>
        {
            FieldType field = null;

            var panel = parent.AddUIComponent<CustomUIPanel>();
            panel.PauseLayout(() =>
            {
                panel.AutoLayout = AutoLayout.Vertical;
                panel.AutoChildrenHorizontally = AutoLayoutChildren.Fit;
                panel.AutoChildrenVertically = AutoLayoutChildren.Fit;
                panel.AutoLayoutStart = LayoutStart.TopCentre;
                panel.AutoLayoutSpace = 3;

                field = panel.AddUIComponent<FieldType>();
                field.SetDefaultStyle();
                field.OnValueChanged += onChanged;
                field.eventGotFocus += FieldGotFocus;
                field.eventLostFocus += FieldLostFocus;

                var label = panel.AddUIComponent<CustomUILabel>();
                label.text = name;
                label.textScale = 0.65f;
                label.textColor = ComponentStyle.DarkPrimaryColor80;
                label.Bold = false;
                label.Padding = new RectOffset(0, 0, 2, 0);
            });

            return field;
        }
        private void FieldGotFocus(UIComponent component, UIFocusEventParameter eventParam)
        {
            AutoClose = false;
        }
        private void FieldLostFocus(UIComponent component, UIFocusEventParameter eventParam)
        {
            AutoClose = true; 
            Focus();
        }
        private void SetField(ByteUITextField field)
        {
            field.MinValue = byte.MinValue;
            field.MaxValue = byte.MaxValue;
            field.CheckMax = true;
            field.CheckMin = true;
            field.UseWheel = true;
            field.WheelStep = 10;
            field.width = 40;
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

        public virtual void DeInit()
        {
            OnSelectedColorChanged = null;
            SelectedColor = Color.white;
            AutoClose = true;
        }

        #region HANDLERS

        protected void ColorChanged(Color color, bool callEvent = true, Action<Color> action = null)
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
            if (HSBField.GetHitPosition(p.ray, out var hitPos))
            {
                hitPos.x = Mathf.Clamp(hitPos.x, HSBPadding.left, HSBField.width - HSBPadding.right);
                hitPos.y = Mathf.Clamp(hitPos.y, HSBPadding.top, HSBField.height - HSBPadding.bottom);
                HSBIndicator.relativePosition = hitPos - HSBIndicator.size * 0.5f;

                var color = HSBAValue;
                ColorChanged(color, action: OnChangedIndicator);

                HSBField.Focus();
            }
        }
        private void HueChanged(float value)
        {
            if (!InProcess)
            {
                HueColor = new HSBColor(HueSlider.Value, 1f, 1f, 1f);
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

        protected void OnValueChanged(Color color)
        {
            SetHSB(color);
            SetHue(color);
            SetOpacity(color);
            SetIndicators(color);
            SetRGBValue(color);
            SetHEXValue(color);
        }
        private void OnChangedIndicator(Color color)
        {
            SetOpacity(color);
            SetIndicators(color);
            SetRGBValue(color);
            SetHEXValue(color);
        }
        private void OnChangedHue(Color color)
        {
            SetHSB(color);
            SetOpacity(color);
            SetIndicators(color);
            SetRGBValue(color);
            SetHEXValue(color);
        }
        private void OnChangedOpacity(Color color)
        {
            SetRGBValue(color);
        }
        private void OnChangedRGBAValue(Color color)
        {
            SetHSB(color);
            SetHue(color);
            SetOpacity(color);
            SetIndicators(color);
            SetHEXValue(color);
        }
        private void OnChangedHEXValue(Color color)
        {
            SetHSB(color);
            SetHue(color);
            SetIndicators(color);
            SetRGBValue(color);
        }

        private void SetHSB(Color color)
        {
            var hsbColor = HSBColor.FromColor(color);
            var x = hsbColor.s * (HSBField.width - HSBPadding.horizontal);
            var y = (1f - hsbColor.b) * (HSBField.height - HSBPadding.vertical);
            HSBIndicator.relativePosition = new Vector2(x, y) + new Vector2(HSBPadding.left, HSBPadding.top) - HSBIndicator.size * 0.5f;

            if (HSBField.renderMaterial != null)
            {
                var hue = HueColor;
                HSBField.renderMaterial.color = hue.gamma;
            }
        }
        private void SetHue(Color color)
        {
            var hsbColor = HSBColor.FromColor(color);
            HueColor = new HSBColor(hsbColor.h, 1f, 1f, 1f);
            HueSlider.Value = hsbColor.h;
        }
        private void SetOpacity(Color color)
        {
            OpacitySlider.Value = color.a;
            color.a = byte.MaxValue;
            OpacitySlider.BgColor = color;
        }
        private void SetIndicators(Color color)
        {
            color.a = 255;
            HSBIndicator.FgColors = color;
            HueIndicator.FgColors = HueColor;
            OpacityIndicator.FgColors = color;
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
