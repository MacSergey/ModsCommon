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
        private CustomUIPanel MessagePanel { get; set; }
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
                Label.wordWrap = true;
                Label.autoHeight = true;
                Label.textColor = ComponentStyle.DarkPrimaryColor100;
                Label.textAlignment = UIHorizontalAlignment.Center;
                Label.verticalAlignment = UIVerticalAlignment.Middle;

                Label.atlas = CommonTextures.Atlas;
                Label.backgroundSprite = CommonTextures.PanelBig;
                Label.color = ComponentStyle.WarningColor;
                Label.padding = new RectOffset(15, 15, 10, 10);


                MessagePanel = Content.AddUIComponent<CustomUIPanel>();
                MessagePanel.name = nameof(MessagePanel);
                MessagePanel.AutoLayout = AutoLayout.Vertical;
                MessagePanel.AutoChildrenHorizontally = AutoLayoutChildren.Fill;
                MessagePanel.AutoChildrenVertically = AutoLayoutChildren.Fit;
                MessagePanel.AutoLayoutSpace = 10;
                MessagePanel.Padding = new RectOffset(0, 0, 10, 0);

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

        public PluginMessage AddMessage()
        {
            var message = MessagePanel.AddUIComponent<PluginMessage>();
            return message;
        }
        public void RemoveMessage(PluginMessage message)
        {
            MessagePanel.RemoveUIComponent(message);
            Destroy(message.gameObject);
            Destroy(message);
        }
    }
    public class PluginMessage : CustomUIPanel
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

        public PluginMessage()
        {
            PauseLayout(() =>
            {
                AutoLayout = AutoLayout.Horizontal;
                AutoChildrenVertically = AutoLayoutChildren.Fit;
                AutoLayoutStart = UI.LayoutStart.MiddleLeft;
                Padding = new RectOffset(10, 10, 10, 10);

                Atlas = CommonTextures.Atlas;
                BackgroundSprite = CommonTextures.PanelBig;
                color = ComponentStyle.DarkPrimaryColor20;
                ForegroundSprite = CommonTextures.BorderBig;
                NormalFgColor = ComponentStyle.DarkPrimaryColor10;

                Label = AddUIComponent<CustomUILabel>();
                Label.autoSize = false;
                Label.autoHeight = true;
                Label.wordWrap = true;
                Label.padding = new RectOffset(10, 10, 5, 0);

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
                Resolved.atlas = CommonTextures.Atlas;
                Resolved.SetBgSprite(new SpriteSet(CommonTextures.PanelBig));
                Resolved.SetBgColor(new ColorSet(ComponentStyle.WellColor));
                Resolved.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
                Resolved.SetFgSprite(new SpriteSet(CommonTextures.Success));
                Resolved.SetFgColor(new ColorSet(ComponentStyle.DarkPrimaryColor100));
                Resolved.autoSize = false;
                Resolved.size = new Vector2(150f, 30f);
                Resolved.horizontalAlignment = UIHorizontalAlignment.Left;
                Resolved.textHorizontalAlignment = UIHorizontalAlignment.Center;
                Resolved.textVerticalAlignment = UIVerticalAlignment.Middle;
                Resolved.textPadding = new RectOffset(20, 0, 3, 0);
                Resolved.spritePadding = new RectOffset(2, 0, 0, 0);

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