using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModsCommon.UI
{
    public class ButtonPanel : EditorItem, IReusable
    {
        bool IReusable.InCache { get; set; }
        protected CustomUIButton Button { get; set; }
        private float Height => 20f;

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
        public override bool SupportEven => true;

        public event Action OnButtonClick;

        public ButtonPanel()
        {
            Button = AddButton(this);
            Button.textScale = 0.8f;
            Button.textPadding = new RectOffset(0, 0, 3, 0);
            Button.isEnabled = EnableControl;
            Button.eventClick += ButtonClick;
        }
        protected override void Init(float? height)
        {
            base.Init(height);
            SetSize();
        }
        public override void DeInit()
        {
            base.DeInit();

            Text = string.Empty;
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
            Button.size = new Vector2(width - ItemsPadding * 2, Height);
            Button.relativePosition = new Vector3(ItemsPadding, (height - Height) / 2);
        }
    }
    public class ButtonsPanel : EditorItem, IReusable
    {
        public event Action<int> OnButtonClick;

        bool IReusable.InCache { get; set; }
        protected List<CustomUIButton> Buttons { get; } = new List<CustomUIButton>();
        public int Count => Buttons.Count;
        private float Space => 10f;
        private float Height => 20f;
        public override bool SupportEven => true;

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

        protected override void Init(float? height)
        {
            base.Init(height);
            SetSize();
        }
        public override void DeInit()
        {
            foreach (var button in Buttons)
            {
                button.parent.RemoveUIComponent(button);
                Destroy(button);
            }

            OnButtonClick = null;
            Buttons.Clear();

            base.DeInit();
        }

        public int AddButton(string text)
        {
            var button = AddButton(this);

            button.text = text;
            button.textScale = 0.8f;
            button.textPadding = new RectOffset(0, 0, 3, 0);
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
            var buttonWidth = (width - Space * (Count - 1) - ItemsPadding * 2) / Count;
            for (var i = 0; i < Count; i += 1)
            {
                Buttons[i].size = new Vector2(buttonWidth, Height);
                Buttons[i].relativePosition = new Vector2((buttonWidth + Space) * i + ItemsPadding, (height - Height) / 2);
            }
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
            get => Button.wordWrap;
            set => Button.wordWrap = value;
        }
        public bool AutoSize
        {
            get => Button.autoSize;
            set=> Button.autoSize = value;
        }

        public override bool EnableControl
        {
            get => Button.isEnabled;
            set => Button.isEnabled = value;
        }
        public override bool SupportEven => true;

        public event Action OnButtonClick;

        public ButtonPropertyPanel()
        {
            Button = AddButton(Content);
            Button.textScale = 0.8f;
            Button.textPadding = new RectOffset(0, 0, 3, 0);
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
            AutoSize = false;
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
    }
}
