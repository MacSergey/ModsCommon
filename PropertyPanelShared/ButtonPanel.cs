using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using ModsCommon.Utilities;

namespace ModsCommon.UI
{
    public class ButtonPanel : BaseEditorPanel, IReusable
    {
        bool IReusable.InCache { get; set; }
        protected CustomUIButton Button { get; set; }
        protected float DefaultHeight => 20f;

        public string Text
        {
            get => Button.text;
            set => Button.text = value;
        }
        public override bool EnableControl
        {
            get => Button.isEnabled;
            set => Button.isEnabled = value;
        }
        public float ButtonHeight
        {
            get => Button.height;
            set => Button.height = value;
        }
        public bool WordWrap
        {
            get => Button.WordWrap;
            set => Button.WordWrap = value;
        }
        public UIHorizontalAlignment TextAlignment
        {
            get => Button.TextHorizontalAlignment;
            set => Button.TextHorizontalAlignment = value;
        }
        public RectOffset TextPadding
        {
            get => Button.TextPadding;
            set => Button.TextPadding = value;
        }
        public AutoSize AutoSize
        {
            get => Button.AutoSize;
            set => Button.AutoSize = value;
        }

        public event Action OnButtonClick;

        public ButtonPanel() : base()
        {
            PauseLayout(() =>
            {
                Button = AddUIComponent<CustomUIButton>();
                Button.SetDefaultStyle();
                Button.height = DefaultHeight;
                Button.textScale = 0.8f;
                Button.TextPadding = new RectOffset(0, 0, 3, 0);
                Button.isEnabled = EnableControl;
                Button.eventClick += ButtonClick;
            });
        }
        public override void DeInit()
        {
            base.DeInit();

            Button.height = DefaultHeight;
            Text = string.Empty;
            OnButtonClick = null;
        }

        private void ButtonClick(UIComponent component, UIMouseEventParameter eventParam) => OnButtonClick?.Invoke();

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            Button.width = width - Padding.horizontal;
        }

        public override void SetStyle(ControlStyle style)
        {
            Button.ButtonStyle = style.Button;
        }
    }
    public class ButtonsPanel : BaseEditorPanel, IReusable
    {
        public event Action<int> OnButtonClick;

        bool IReusable.InCache { get; set; }
        protected List<CustomUIButton> Buttons { get; } = new List<CustomUIButton>();
        public int Count => Buttons.Count;
        protected float DefaultHeight => 20f;

        public override bool EnableControl
        {
            get => base.EnableControl;
            set
            {
                base.EnableControl = value;
                foreach (var button in Buttons)
                    button.isEnabled = value;
            }
        }
        public CustomUIButton this[int index] => Buttons[index];

        public ButtonsPanel() : base()
        {
            autoLayout = AutoLayout.Horizontal;
            autoLayoutSpace = 10;
        }
        protected override void Init(float? height)
        {
            base.Init(height);
            SetSize();
        }
        public override void DeInit()
        {
            foreach (var button in Buttons)
            {
                ComponentPool.Free(button);
            }

            OnButtonClick = null;
            Buttons.Clear();

            base.DeInit();
        }

        public int AddButton(string text)
        {
            var button = AddUIComponent<CustomUIButton>();
            button.SetDefaultStyle();
            button.ButtonStyle = Style;
            button.height = DefaultHeight;
            button.text = text;
            button.textScale = 0.8f;
            button.TextPadding = new RectOffset(0, 0, 3, 0);
            button.isEnabled = EnableControl;
            button.eventClick += ButtonClick;

            Buttons.Add(button);

            return Count - 1;
        }

        private void ButtonClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (component is CustomUIButton button)
            {
                var index = Buttons.IndexOf(button);
                if (index != -1)
                    OnButtonClick?.Invoke(index);
            }
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            SetSize();
        }
        private void SetSize()
        {
            PauseLayout(() =>
            {
                var buttonWidth = (width - AutoLayoutSpace * (Count - 1) - Padding.horizontal) / Count;

                for (var i = 0; i < Count; i += 1)
                    Buttons[i].width = buttonWidth;
            });
        }

        private ButtonStyle Style { get; set; } = ComponentStyle.Default.Button;
        public override void SetStyle(ControlStyle style)
        {
            Style = style.Button;

            foreach (var button in Buttons)
                button.ButtonStyle = Style;
        }
    }

    public class ButtonPropertyPanel : EditorPropertyPanel, IReusable
    {
        bool IReusable.InCache { get; set; }
        CustomUIButton Button { get; set; }

        public string ButtonText
        {
            get => Button.text;
            set => Button.text = value;
        }
        public float Width
        {
            get => Button.width;
            set => Button.width = value;
        }
        public bool WordWrap
        {
            get => Button.WordWrap;
            set => Button.WordWrap = value;
        }
        public UIHorizontalAlignment TextAlignment
        {
            get => Button.TextHorizontalAlignment;
            set => Button.TextHorizontalAlignment = value;
        }
        public RectOffset TextPadding
        {
            get => Button.TextPadding;
            set => Button.TextPadding = value;
        }
        public AutoSize AutoSize
        {
            get => Button.AutoSize;
            set => Button.AutoSize = value;
        }

        public override bool EnableControl
        {
            get => Button.isEnabled;
            set => Button.isEnabled = value;
        }

        public event Action OnButtonClick;

        protected override void FillContent()
        {
            Button = Content.AddUIComponent<CustomUIButton>();
            Button.SetDefaultStyle();
            Button.textScale = 0.8f;
            Button.TextPadding = new RectOffset(0, 0, 3, 0);
            Button.TextHorizontalAlignment = UIHorizontalAlignment.Center;
            Button.isEnabled = EnableControl;
            Button.eventClick += ButtonClick;
        }
        public new void Init(float? height)
        {
            base.Init(height);
            SetSize();
        }
        public override void DeInit()
        {
            base.DeInit();

            ButtonText = string.Empty;
            WordWrap = false;
            AutoSize = AutoSize.None;
            OnButtonClick = null;
        }

        private void ButtonClick(UIComponent component, UIMouseEventParameter eventParam) => OnButtonClick?.Invoke();

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            SetSize();
        }
        protected virtual void SetSize()
        {
            Button.height = Content.height - ItemsPadding * 2;
            Button.relativePosition = new Vector3(ItemsPadding, ItemsPadding);
        }

        public override void SetStyle(ControlStyle style)
        {
            Button.ButtonStyle = style.Button;
        }
    }
}
