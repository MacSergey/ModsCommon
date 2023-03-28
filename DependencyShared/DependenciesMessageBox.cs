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
        private CustomUIPanel RequestPanel { get; set; }
        private CustomUIButton Button { get; set; }
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

        public DependenciesMessageBox() : base()
        {
            Content.PauseLayout(() =>
            {
                Label = Content.AddUIComponent<CustomUILabel>();
                Label.name = "Message";
                Label.Bold = true;
                Label.WordWrap = true;
                Label.AutoSize = AutoSize.Height;
                Label.textColor = ComponentStyle.DarkPrimaryColor100;
                Label.HorizontalAlignment = UIHorizontalAlignment.Center;
                Label.VerticalAlignment = UIVerticalAlignment.Middle;

                Label.Atlas = CommonTextures.Atlas;
                Label.BackgroundSprite = CommonTextures.PanelBig;
                Label.color = ComponentStyle.WarningColor;
                Label.Padding = new RectOffset(15, 15, 10, 10);


                RequestPanel = Content.AddUIComponent<CustomUIPanel>();
                RequestPanel.name = nameof(RequestPanel);
                RequestPanel.AutoLayout = AutoLayout.Vertical;
                RequestPanel.AutoChildrenHorizontally = AutoLayoutChildren.Fill;
                RequestPanel.AutoChildrenVertically = AutoLayoutChildren.Fit;
                RequestPanel.AutoLayoutSpace = 10;
                RequestPanel.Padding = new RectOffset(0, 0, 10, 0);

                Button = AddButton(OkClick);
                ButtonText = CommonLocalize.MessageBox_OK;
            });
            Content.StartLayout();
        }

        private void OkClick()
        {
            if (OnButtonClick?.Invoke() != false)
                Close();
        }

        public PluginRequest AddRequest()
        {
            var request = RequestPanel.AddUIComponent<PluginRequest>();
            return request;
        }
        public void RemoveRequest(PluginRequest message)
        {
            ComponentPool.Free(message);
        }
    }
    public class PluginRequest : CustomUIPanel
    {
        private CustomUILabel Label { get; set; }
        private CustomUIButton Required { get; set; }
        private CustomUIProgressBar Progress { get; set; }
        private CustomUIButton Resolved { get; set; }


        private DependencyMessageState state;
        public DependencyMessageState State
        {
            get => state;
            set
            {
                if (value != state)
                {
                    state = value;
                    StateChanged();
                }
            }
        }

        public Action OnButtonClick { private get; set; }
        public Func<float> GetProgress { private get; set; }

        public string Text { set => Label.text = value; }
        public string RequiredText { set => Required.text = value; }
        public string ResolvedText { set => Resolved.text = value; }

        public PluginRequest()
        {
            PauseLayout(() =>
            {
                AutoLayout = AutoLayout.Horizontal;
                AutoChildrenVertically = AutoLayoutChildren.Fit;
                AutoLayoutStart = UI.LayoutStart.MiddleLeft;
                Padding = new RectOffset(10, 10, 10, 10);

                Atlas = CommonTextures.Atlas;
                BackgroundSprite = CommonTextures.PanelLarge;
                BgColors = ComponentStyle.DarkPrimaryColor20;
                ForegroundSprite = CommonTextures.BorderLarge;
                FgColors = ComponentStyle.DarkPrimaryColor10;

                Label = AddUIComponent<CustomUILabel>();
                Label.AutoSize = AutoSize.Height;
                Label.WordWrap = true;
                Label.Padding = new RectOffset(10, 10, 5, 0);

                Required = AddUIComponent<CustomUIButton>();
                Required.ButtonMessageBoxStyle();
                Required.size = new Vector2(150f, 30f);
                Required.eventClick += ButtonClick;

                Progress = AddUIComponent<CustomUIProgressBar>();
                Progress.minValue = 0f;
                Progress.maxValue = 1f;
                Progress.value = 0f;
                Progress.size = new Vector2(150f, 30f);
                Progress.atlas = CommonTextures.Atlas;
                Progress.backgroundSprite = CommonTextures.PanelBig;
                Progress.progressSprite = CommonTextures.PanelSmall;
                Progress.color = ComponentStyle.DarkPrimaryColor10;
                Progress.progressColor = ComponentStyle.NormalBlue;
                Progress.isVisible = false;
                Progress.padding = new RectOffset(3, 3, 3, 3);

                Resolved = AddUIComponent<CustomUIButton>();
                Resolved.Atlas = CommonTextures.Atlas;
                Resolved.BgSprites = CommonTextures.PanelBig;
                Resolved.BgColors = ComponentStyle.WellColor;
                Resolved.ForegroundSpriteMode = UIForegroundSpriteMode.Scale;
                Resolved.FgSprites = CommonTextures.Success;
                Resolved.FgColors = ComponentStyle.DarkPrimaryColor100;
                Resolved.AutoSize = AutoSize.None;
                Resolved.size = new Vector2(150f, 30f);
                Resolved.HorizontalAlignment = UIHorizontalAlignment.Left;
                Resolved.TextHorizontalAlignment = UIHorizontalAlignment.Center;
                Resolved.TextVerticalAlignment = UIVerticalAlignment.Middle;
                Resolved.TextPadding = new RectOffset(20, 0, 3, 0);
                Resolved.SpritePadding = new RectOffset(2, 0, 0, 0);

                StateChanged();
            });
        }
        private void ButtonClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            OnButtonClick?.Invoke();
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            StateChanged();
        }
        private void StateChanged()
        {
            PauseLayout(() =>
            {
                Required.isVisible = state == DependencyMessageState.Required;
                Progress.isVisible = state == DependencyMessageState.InProgress;
                Resolved.isVisible = state == DependencyMessageState.Resolved;

                switch (State)
                {
                    case DependencyMessageState.Required:
                        Label.width = ItemSize.x - Required.width;
                        break;
                    case DependencyMessageState.InProgress:
                        Label.width = ItemSize.x - Progress.width;
                        break;
                    case DependencyMessageState.Resolved:
                        Label.width = ItemSize.x - Resolved.width;
                        break;
                }
            });
        }
        public override void Update()
        {
            base.Update();

            if (State == DependencyMessageState.InProgress)
                Progress.value = GetProgress?.Invoke() ?? 0f;
        }
    }

    public enum DependencyMessageState
    {
        None,
        Required,
        InProgress,
        Resolved
    }
}