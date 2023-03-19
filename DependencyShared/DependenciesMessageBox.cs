using ColossalFramework.UI;
using ModsCommon.UI;
using System;
using System.Linq;
using UnityEngine;

namespace ModsCommon.Utilities
{
    public class DependenciesMessageBox : MessageBoxBase
    {
        private CustomUILabel Label { get; set; }
        private CustomUIButton Button { get; set; }
        private CustomUIPanel Space { get; set; }
        protected override int ContentSpacing => 5;

        public string MessageText
        {
            get => Label.text;
            set => Label.text = value;
        }
        public string ButtonText
        {
            get => Button.text;
            set => Button.text = value;
        }
        public Func<bool> OnButtonClick { private get; set; }

        public DependenciesMessageBox()
        {
            Content.Padding = new RectOffset(ButtonsSpace, ButtonsSpace, ContentSpacing, ContentSpacing);

            Content.PauseLayout(() =>
            {
                AddLabel();
                AddButton();
            });
            Content.StartLayout();

            Content.eventComponentAdded += ContentComponentChanged;
            Content.eventComponentRemoved += ContentComponentChanged;
        }
        private void AddLabel()
        {
            Label = Content.AddUIComponent<CustomUILabel>();
            Label.textAlignment = UIHorizontalAlignment.Center;
            Label.verticalAlignment = UIVerticalAlignment.Middle;
            Label.textScale = 1.1f;
            Label.wordWrap = true;
            Label.autoHeight = true;
            Label.minimumSize = new Vector2(0, 79);

            Space = Content.AddUIComponent<CustomUIPanel>();
            Space.BackgroundSprite = "ContentManagerItemBackground";
            Space.height = 5f;
        }

        private void AddButton()
        {
            Button = AddButton(OkClick);
            ButtonText = CommonLocalize.MessageBox_OK;
        }

        private void OkClick()
        {
            if (OnButtonClick?.Invoke() != false)
                Close();
        }
        public override void OnDestroy()
        {
            Content.eventComponentAdded -= ContentComponentChanged;
            Content.eventComponentRemoved -= ContentComponentChanged;
            base.OnDestroy();
        }
        private void ContentComponentChanged(UIComponent container, UIComponent child)
        {
            Space.isVisible = Content.components.Any(c => c is PluginMessage);
        }

    }
    public class PluginMessage : CustomUIPanel
    {
        private CustomUILabel Label { get; set; }
        private CustomUIButton Button { get; set; }
        private CustomUIProgressBar Progress { get; set; }

        private bool _inProgress;
        public bool InProgress
        {
            get => _inProgress;
            set
            {
                if (value != _inProgress)
                {
                    _inProgress = value;
                    Button.isVisible = !_inProgress;
                    Progress.isVisible = _inProgress;
                }
            }
        }

        public Action OnButtonClick { private get; set; }
        public Func<float> GetProgress { private get; set; }

        public string Text { set => Label.text = value; }
        public string ButtonText { set => Button.text = value; }

        public PluginMessage()
        {
            AddLabel();
            AddButton();
            AddProgress();

            height = 30f;
        }
        private void AddLabel()
        {
            Label = AddUIComponent<CustomUILabel>();
            Label.eventTextChanged += (_, _) => SetLabel();
        }
        private void AddButton()
        {
            Button = AddUIComponent<CustomUIButton>();
            Button.ButtonMessageBoxStyle();
            Button.size = new Vector2(150f, 30f);
            Button.eventClick += ButtonClick;
        }
        private void AddProgress()
        {
            Progress = AddUIComponent<CustomUIProgressBar>();
            Progress.minValue = 0f;
            Progress.maxValue = 1f;
            Progress.value = 0f;
            Progress.size = new Vector2(150f, 16f);
            Progress.backgroundSprite = "ScrollbarTrack";
            Progress.progressSprite = "SliderFill";
            Progress.isVisible = false;
        }

        private void ButtonClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (OnButtonClick != null)
            {
                OnButtonClick.Invoke();
                Button.isEnabled = false;
            }
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            SetLabel();
            SetButton();
            SetProgress();
        }
        private void SetLabel()
        {
            Label.relativePosition = new Vector2(0f, (height - Label.height) / 2f);
        }
        private void SetButton()
        {
            Button.relativePosition = new Vector2(width - Button.width, (height - Button.height) / 2f);
        }
        private void SetProgress()
        {
            Progress.relativePosition = new Vector2(width - Progress.width, (height - Progress.height) / 2f);
        }
        public override void Update()
        {
            base.Update();

            if (InProgress)
                Progress.value = GetProgress?.Invoke() ?? 0f;
        }
    }
}