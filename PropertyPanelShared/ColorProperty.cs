using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Globalization;
using UnityEngine;

namespace ModsCommon.UI
{
    public class ColorPropertyPanel : EditorPropertyPanel, IReusable
    {
        public event Action<Color32> OnValueChanged;

        bool IReusable.InCache { get; set; }
        private bool InProcess { get; set; } = false;
        protected virtual Color32 PopupColor => new Color32(201, 211, 216, 255);

        private ByteUITextField RProperty { get; set; }
        private ByteUITextField GProperty { get; set; }
        private ByteUITextField BProperty { get; set; }
        private ByteUITextField AProperty { get; set; }


        private ByteUITextField RPicker { get; set; }
        private ByteUITextField GPicker { get; set; }
        private ByteUITextField BPicker { get; set; }
        private ByteUITextField APicker { get; set; }
        private StringUITextField HEXPicker { get; set; }

        private UIColorField ColorSample { get; set; }
        protected UIColorPicker Popup { get; private set; }
        private UISlider Opacity { get; set; }
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
        private Color32 PickerValue
        {
            get => Popup != null ? new Color32(RPicker, GPicker, BPicker, APicker) : Value;
            set => ValueChanged(value, false, OnChangedValue);
        }

        public ColorPropertyPanel()
        {
            RProperty = AddField(Content, "R", PropertyChanged);
            GProperty = AddField(Content, "G", PropertyChanged);
            BProperty = AddField(Content, "B", PropertyChanged);
            AProperty = AddField(Content, "A", PropertyChanged);

            AddColorSample();
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

        private void PropertyChanged(byte value)
        {
            if (!InProcess)
                ValueChanged(Value, action: OnChangedProperty);
        }
        private void ColorChanged(UIComponent component, Color value)
        {
            if (!InProcess)
            {
                var color = (Color32)value;
                color.a = AProperty;

                ValueChanged(color, action: OnChangedColor);
            }
        }
        private void OpacityChanged(UIComponent component, float value)
        {
            if (!InProcess)
            {
                var color = Value;
                color.a = (byte)value;

                ValueChanged(color, action: OnChangedOpacity);
            }
        }
        private void PickerRGBChanged(byte value)
        {
            if (!InProcess)
                ValueChanged(PickerValue, action: OnChangedPickerRGB);
        }
        private void PickerHEXChanged(string value)
        {
            if (!InProcess)
            {
                var r = byte.Parse(value.Substring(0, 2), NumberStyles.HexNumber, null);
                var g = byte.Parse(value.Substring(2, 2), NumberStyles.HexNumber, null);
                var b = byte.Parse(value.Substring(4, 2), NumberStyles.HexNumber, null);
                ValueChanged(new Color32(r, g, b, AProperty), action: OnChangedPickerHEX);
            }
        }

        protected void OnChangedValue(Color32 color)
        {
            SetProperties(color);
            SetPickerRGB(color);
            SetPickerHEX(color);
            SetSample(color);
            SetOpacity(color);
        }
        private void OnChangedProperty(Color32 color)
        {
            SetSample(color);
        }
        private void OnChangedColor(Color32 color)
        {
            SetProperties(color);
            SetPickerRGB(color);
            SetPickerHEX(color);
            SetOpacity(color);
        }
        private void OnChangedOpacity(Color32 color)
        {
            SetProperties(color);
            SetPickerRGB(color);
            SetPickerHEX(color);
            SetSample(color);
        }
        private void OnChangedPickerRGB(Color32 color)
        {
            SetProperties(color);
            SetPickerHEX(color);
            SetSample(color);
            SetOpacity(color);
        }
        private void OnChangedPickerHEX(Color32 color)
        {
            SetProperties(color);
            SetPickerRGB(color);
            SetSample(color);
            SetOpacity(color);
        }

        private void SetProperties(Color32 color)
        {
            RProperty.Value = color.r;
            GProperty.Value = color.g;
            BProperty.Value = color.b;
            AProperty.Value = color.a;
        }
        private void SetPickerRGB(Color32 color)
        {
            if (Popup != null)
            {
                RPicker.Value = color.r;
                GPicker.Value = color.g;
                BPicker.Value = color.b;
                APicker.Value = color.a;
            }
        }
        private void SetPickerHEX(Color32 color)
        {
            if (Popup != null)
            {
                HEXPicker.Value = $"{color.r:X2}{color.g:X2}{color.b:X2}";
            }
        }

        private void SetSample(Color32 color)
        {
            color.a = byte.MaxValue;

            if (ColorSample != null)
                ColorSample.selectedColor = color;
            if (Popup != null)
                Popup.color = color;
        }
        private void SetOpacity(Color32 color)
        {
            if (Opacity != null)
            {
                Opacity.value = color.a;
                color.a = byte.MaxValue;
                Opacity.Find<UISlicedSprite>("color").color = color;
            }
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
            label.padding = new RectOffset(0, 0, 2, 0);

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

            return field;
        }

        private void AddColorSample()
        {
            if (UITemplateManager.Get("LineTemplate") is not UIComponent template)
                return;

            var panel = Content.AddUIComponent<CustomUIPanel>();
            panel.atlas = CommonTextures.Atlas;
            panel.backgroundSprite = CommonTextures.ColorPickerBoard;

            ColorSample = Instantiate(template.Find<UIColorField>("LineColor").gameObject).GetComponent<UIColorField>();
            panel.AttachUIComponent(ColorSample.gameObject);
            ColorSample.size = panel.size = new Vector2(20f, 20f);
            ColorSample.relativePosition = new Vector2(0, 0);
            ColorSample.anchor = UIAnchorStyle.None;

            ColorSample.atlas = CommonTextures.Atlas;
            ColorSample.normalBgSprite = CommonTextures.ColorPickerNormal;
            ColorSample.normalFgSprite = CommonTextures.ColorPickerColor;
            ColorSample.hoveredBgSprite = CommonTextures.ColorPickerHovered;
            ColorSample.hoveredFgSprite = CommonTextures.ColorPickerColor;
            ColorSample.disabledBgSprite = CommonTextures.ColorPickerDisabled;
            ColorSample.disabledFgSprite = CommonTextures.ColorPickerColor;

            ColorSample.eventSelectedColorChanged += ColorChanged;
            ColorSample.eventColorPickerOpen += ColorPickerOpen;
            ColorSample.eventColorPickerClose += ColorPickerClose;
        }

        protected virtual void ColorPickerOpen(UIColorField dropdown, UIColorPicker popup, ref bool overridden)
        {
            dropdown.triggerButton.isInteractive = false;

            Popup = popup;

            Popup.component.size += new Vector2(31, 81);
            Popup.component.relativePosition -= new Vector3(dropdown.width + 31, Math.Max(Popup.component.absolutePosition.y - dropdown.absolutePosition.y, 0));

            if (Popup.component is UIPanel panel)
            {
                panel.atlas = TextureHelper.InGameAtlas;
                panel.backgroundSprite = "ButtonWhite";
                panel.color = PopupColor;
            }

            Opacity = AddOpacitySlider(popup.component);
            AddRGBPanel(popup.component);
            AddHEXPanel(popup.component);

            SetOpacity(Value);
        }
        private void ColorPickerClose(UIColorField dropdown, UIColorPicker popup, ref bool overridden)
        {
            dropdown.triggerButton.isInteractive = true;

            Popup = null;
            Opacity = null;
            RPicker = null;
            GPicker = null;
            BPicker = null;
            APicker = null;
            HEXPicker = null;
        }
        private UISlider AddOpacitySlider(UIComponent parent)
        {
            var opacitySlider = parent.AddUIComponent<UISlider>();

            opacitySlider.atlas = TextureHelper.InGameAtlas;
            opacitySlider.size = new Vector2(18, 200);
            opacitySlider.relativePosition = new Vector3(254, 12);
            opacitySlider.orientation = UIOrientation.Vertical;
            opacitySlider.minValue = 0f;
            opacitySlider.maxValue = 255f;
            opacitySlider.stepSize = 1f;
            opacitySlider.eventValueChanged += OpacityChanged;

            var opacityBoard = opacitySlider.AddUIComponent<UISlicedSprite>();
            opacityBoard.atlas = CommonTextures.Atlas;
            opacityBoard.spriteName = CommonTextures.OpacitySliderBoard;
            opacityBoard.relativePosition = Vector2.zero;
            opacityBoard.size = opacitySlider.size;
            opacityBoard.fillDirection = UIFillDirection.Vertical;

            var opacityColor = opacitySlider.AddUIComponent<UISlicedSprite>();
            opacityColor.name = "color";
            opacityColor.atlas = CommonTextures.Atlas;
            opacityColor.spriteName = CommonTextures.OpacitySliderColor;
            opacityColor.relativePosition = Vector2.zero;
            opacityColor.size = opacitySlider.size;
            opacityColor.fillDirection = UIFillDirection.Vertical;

            UISlicedSprite thumbSprite = opacitySlider.AddUIComponent<UISlicedSprite>();
            thumbSprite.relativePosition = Vector2.zero;
            thumbSprite.fillDirection = UIFillDirection.Horizontal;
            thumbSprite.size = new Vector2(29, 7);
            thumbSprite.spriteName = "ScrollbarThumb";

            opacitySlider.thumbObject = thumbSprite;

            return opacitySlider;
        }
        private void AddRGBPanel(UIComponent parent)
        {
            var rgbPanel = parent.AddUIComponent<CustomUIPanel>();
            rgbPanel.relativePosition = new Vector2(10, 223);
            rgbPanel.autoLayoutDirection = LayoutDirection.Horizontal;
            rgbPanel.autoLayoutPadding = new RectOffset(5, 0, 0, 0);
            rgbPanel.autoFitChildrenHorizontally = true;
            rgbPanel.autoFitChildrenVertically = true;

            RPicker = AddField(rgbPanel, "R", PickerRGBChanged);
            GPicker = AddField(rgbPanel, "G", PickerRGBChanged);
            BPicker = AddField(rgbPanel, "B", PickerRGBChanged);
            APicker = AddField(rgbPanel, "A", PickerRGBChanged);

            rgbPanel.autoLayout = true;
            rgbPanel.autoLayout = false;

            foreach (var item in rgbPanel.components)
                item.relativePosition = new Vector2(item.relativePosition.x, (rgbPanel.height - item.height) / 2);
        }

        private void AddHEXPanel(UIComponent parent)
        {
            var hexPanel = parent.AddUIComponent<CustomUIPanel>();
            hexPanel.relativePosition = new Vector2(10, 248);
            hexPanel.autoLayoutDirection = LayoutDirection.Horizontal;
            hexPanel.autoLayoutPadding = new RectOffset(5, 0, 0, 0);
            hexPanel.autoFitChildrenHorizontally = true;
            hexPanel.autoFitChildrenVertically = true;

            var label = hexPanel.AddUIComponent<CustomUILabel>();
            label.text = "HEX";
            label.textScale = 0.7f;
            label.padding = new RectOffset(0, 0, 2, 0);

            HEXPicker = hexPanel.AddUIComponent<StringUITextField>();
            HEXPicker.SetDefaultStyle();
            HEXPicker.Format = "#{0}";
            HEXPicker.horizontalAlignment = UIHorizontalAlignment.Left;
            HEXPicker.width = 100f;
            HEXPicker.padding.left = 5;
            HEXPicker.CheckValue = CheckHEXValue;
            HEXPicker.OnValueChanged += PickerHEXChanged;

            hexPanel.autoLayout = true;
            hexPanel.autoLayout = false;

            foreach (var item in hexPanel.components)
                item.relativePosition = new Vector2(item.relativePosition.x, (hexPanel.height - item.height) / 2);
        }
        private string CheckHEXValue(string value)
        {
            byte r = 0;
            byte g = 0;
            byte b = 0;

            if(value.Length > 0)
            {
                if(value.StartsWith("#"))
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

        public override string ToString() => $"{base.ToString()}: {Value}";
        public static implicit operator Color32(ColorPropertyPanel property) => property.Value;
    }
}

