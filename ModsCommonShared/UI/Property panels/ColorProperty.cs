using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using UnityEngine;

namespace ModsCommon.UI
{
    public class ColorPropertyPanel : EditorPropertyPanel, IReusable
    {
        public event Action<Color32> OnValueChanged;
        public event Action OnStartWheel;
        public event Action OnStopWheel;

        private bool InProcess { get; set; } = false;

        private ByteUITextField R { get; set; }
        private ByteUITextField G { get; set; }
        private ByteUITextField B { get; set; }
        private ByteUITextField A { get; set; }
        private UIColorField ColorSample { get; set; }
        protected UIColorPicker Popup { get; private set; }
        private UISlider Opacity { get; set; }
        public string WheelTip
        {
            set
            {
                R.WheelTip = value;
                G.WheelTip = value;
                B.WheelTip = value;
                A.WheelTip = value;
            }
        }

        public Color32 Value
        {
            get => new Color32(R, G, B, A);
            set => ValueChanged(value, (c) =>
            {
                SetFields(c);
                SetSample(c);
                SetOpacity(c);
            });
        }

        public ColorPropertyPanel()
        {
            R = AddField(nameof(R));
            G = AddField(nameof(G));
            B = AddField(nameof(B));
            A = AddField(nameof(A));

            AddColorSample();
        }
        private void ValueChanged(Color32 color, Action<Color32> action)
        {
            if (!InProcess)
            {
                InProcess = true;

                action(color);
                OnValueChanged?.Invoke(Value);

                InProcess = false;
            }
        }

        private void FieldChanged(byte value) => ValueChanged(Value, (c) =>
        {
            SetSample(c);
            SetOpacity(c);
        });
        private void SelectedColorChanged(UIComponent component, Color value)
        {
            var color = (Color32)value;
            color.a = A;

            ValueChanged(color, (c) =>
            {
                SetFields(c);
                SetOpacity(color);
            });
        }
        private void OpacityChanged(UIComponent component, float value) => A.Value = (byte)value;

        private void SetFields(Color32 color)
        {
            R.Value = color.r;
            G.Value = color.g;
            B.Value = color.b;
            A.Value = color.a;
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

            WheelTip = string.Empty;

            OnValueChanged = null;
            OnStartWheel = null;
            OnStopWheel = null;
        }
        private ByteUITextField AddField(string name)
        {
            var lable = Content.AddUIComponent<CustomUILabel>();
            lable.text = name;
            lable.textScale = 0.7f;

            var field = Content.AddUIComponent<ByteUITextField>();
            field.SetDefaultStyle();
            field.MinValue = byte.MinValue;
            field.MaxValue = byte.MaxValue;
            field.CheckMax = true;
            field.CheckMin = true;
            field.UseWheel = true;
            field.WheelStep = 10;
            field.width = 30;
            field.OnValueChanged += FieldChanged;
            field.eventMouseHover += FieldHover;
            field.eventMouseLeave += FieldLeave;

            return field;
        }

        private void AddColorSample()
        {
            if (!(UITemplateManager.Get("LineTemplate") is UIComponent template))
                return;

            var panel = Content.AddUIComponent<CustomUIPanel>();
            panel.atlas = TextureHelper.CommonAtlas;
            panel.backgroundSprite = TextureHelper.ColorPickerBoard;

            ColorSample = Instantiate(template.Find<UIColorField>("LineColor").gameObject).GetComponent<UIColorField>();
            panel.AttachUIComponent(ColorSample.gameObject);
            ColorSample.size = panel.size = new Vector2(26f, 28f);
            ColorSample.relativePosition = new Vector2(0, 0);
            ColorSample.anchor = UIAnchorStyle.None;

            ColorSample.atlas = TextureHelper.CommonAtlas;
            ColorSample.normalBgSprite = TextureHelper.ColorPickerNormal;
            ColorSample.normalFgSprite = TextureHelper.ColorPickerColor;
            ColorSample.hoveredBgSprite = TextureHelper.ColorPickerHover;
            ColorSample.hoveredFgSprite = TextureHelper.ColorPickerColor;
            ColorSample.disabledBgSprite = TextureHelper.ColorPickerDisable;
            ColorSample.disabledFgSprite = TextureHelper.ColorPickerColor;

            ColorSample.eventSelectedColorChanged += SelectedColorChanged;
            ColorSample.eventColorPickerOpen += ColorPickerOpen;
            ColorSample.eventColorPickerClose += ColorPickerClose;
        }

        protected virtual void ColorPickerOpen(UIColorField dropdown, UIColorPicker popup, ref bool overridden)
        {
            dropdown.triggerButton.isInteractive = false;

            Popup = popup;

            Popup.component.size += new Vector2(31, 31);
            Popup.component.relativePosition -= new Vector3(dropdown.width + 31, Math.Max(Popup.component.absolutePosition.y - dropdown.absolutePosition.y, 0));

            if (Popup.component is UIPanel panel)
            {
                panel.atlas = TextureHelper.InGameAtlas;
                panel.backgroundSprite = "ButtonWhite";
                panel.color = new Color32(201, 211, 216, 255);
            }

            Opacity = AddOpacitySlider(popup.component);
            Opacity.value = A;
        }
        private void ColorPickerClose(UIColorField dropdown, UIColorPicker popup, ref bool overridden)
        {
            dropdown.triggerButton.isInteractive = true;

            Popup = null;
            Opacity = null;
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
            opacityBoard.atlas = TextureHelper.CommonAtlas;
            opacityBoard.spriteName = TextureHelper.OpacitySliderBoard;
            opacityBoard.relativePosition = Vector2.zero;
            opacityBoard.size = opacitySlider.size;
            opacityBoard.fillDirection = UIFillDirection.Vertical;

            var opacityColor = opacitySlider.AddUIComponent<UISlicedSprite>();
            opacityColor.name = "color";
            opacityColor.atlas = TextureHelper.CommonAtlas;
            opacityColor.spriteName = TextureHelper.OpacitySliderColor;
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

        private void FieldHover(UIComponent component, UIMouseEventParameter eventParam) => OnStartWheel?.Invoke();
        private void FieldLeave(UIComponent component, UIMouseEventParameter eventParam) => OnStopWheel?.Invoke();

        public override string ToString() => $"{base.ToString()}: {Value}";
        public static implicit operator Color32(ColorPropertyPanel property) => property.Value;
    }
}

