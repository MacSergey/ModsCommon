using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ModsCommon.UI
{
    public class WhatsNewMessageBox : MessageBoxBase
    {
        protected override int ContentSpacing => 5;

        private CustomUIButton OkButton { get; }
        public Func<bool> OnButtonClick { get; set; }
        public string OkText { set => OkButton.text = value; }

        public WhatsNewMessageBox()
        {
            OkButton = AddButton(OkClick);
        }
        protected virtual void OkClick()
        {
            if (OnButtonClick?.Invoke() != false)
                Close();
        }

        public virtual void Init(Dictionary<ModVersion, string> messages, string modName = null, bool expandFirst = true, CultureInfo culture = null)
        {
            StopLayout();

            //if (!string.IsNullOrEmpty(modName))
            //{
            //    var donate = Panel.Content.AddUIComponent<DonatePanel>();
            //    donate.Init(modName, new Vector2(200f, 50f));
            //}

            var first = default(VersionMessage);
            foreach (var message in messages)
            {
                var versionMessage = Panel.Content.AddUIComponent<VersionMessage>();
                versionMessage.Init(message.Key, message.Value, culture);

                if (first == null)
                    first = versionMessage;
            }
            if (expandFirst)
                first.IsExpand = true;

            StartLayout();
        }

        public class VersionMessage : UIAutoLayoutPanel
        {
            public bool IsExpand
            {
                get => Container.isVisible;
                set
                {
                    Container.isVisible = value;
                    Button.SetFgSprite(new SpriteSet(value ? CommonTextures.ArrowDown : CommonTextures.ArrowRight));

                    Container.StopLayout();
                    {
                        if (value)
                        {
                            foreach (var message in Messages)
                            {
                                var line = ComponentPool.Get<UpdateMessage>(Container);
                                line.relativePosition = new Vector3(17, 7);
                                line.Init(message);
                            }
                        }
                        else
                        {
                            var components = Container.components.ToArray();
                            foreach (var component in components)
                            {
                                ComponentPool.Free(component);
                            }
                        }
                    }
                    Container.StartLayout();
                }
            }
            private CustomUILabel Title { get; set; }
            private CustomUILabel SubTitle { get; set; }
            private CustomUIButton Button { get; set; }
            private UIAutoLayoutPanel Container { get; set; }
            private List<MessageText> Messages { get; } = new List<MessageText>();

            public VersionMessage()
            {
                StopLayout();
                {
                    autoLayoutDirection = LayoutDirection.Vertical;
                    autoFitChildrenVertically = true;
                    padding = new RectOffset(5, 5, 5, 5);
                    autoLayoutPadding = new RectOffset(0, 0, 0, 8);
                    atlas = CommonTextures.Atlas;
                    backgroundSprite = CommonTextures.PanelBig;
                    color = ComponentStyle.DisabledSettingsGray;
                    verticalSpacing = 5;
                    AddTitle();
                    AddLinesContainer();
                }
                StartLayout();
            }
            private void AddTitle()
            {
                var titlePanel = AddUIComponent<UIAutoLayoutPanel>();
                titlePanel.name = "TitlePanel";
                titlePanel.autoLayoutDirection = LayoutDirection.Horizontal;
                titlePanel.autoFitChildrenVertically = true;
                titlePanel.autoFitChildrenHorizontally = true;
                titlePanel.autoLayoutPadding = new RectOffset(0, 10, 0, 0);
                titlePanel.eventClick += (UIComponent component, UIMouseEventParameter eventParam) => IsExpand = !IsExpand;

                var versionPanel = titlePanel.AddUIComponent<UIAutoLayoutPanel>();
                versionPanel.name = "VersionPanel";
                versionPanel.autoLayoutDirection = LayoutDirection.Vertical;
                versionPanel.autoFitChildrenVertically = true;
                versionPanel.autoFitChildrenHorizontally = true;
                versionPanel.atlas = CommonTextures.Atlas;
                versionPanel.backgroundSprite = CommonTextures.PanelBig;
                versionPanel.color = new Color32(122, 138, 153, 255);

                Title = versionPanel.AddUIComponent<CustomUILabel>();
                Title.name = "Title";
                Title.autoSize = false;
                Title.width = 100f;
                Title.height = 30f;
                Title.textScale = 1.4f;
                Title.textAlignment = UIHorizontalAlignment.Center;
                Title.verticalAlignment = UIVerticalAlignment.Middle;
                Title.padding = new RectOffset(0, 0, 4, 0);

                SubTitle = versionPanel.AddUIComponent<CustomUILabel>();
                SubTitle.name = "SubTitle";
                SubTitle.autoSize = false;
                SubTitle.width = 100f;
                SubTitle.height = 20f;
                SubTitle.textScale = 0.7f;
                SubTitle.textAlignment = UIHorizontalAlignment.Center;
                SubTitle.verticalAlignment = UIVerticalAlignment.Middle;

                Button = titlePanel.AddUIComponent<CustomUIButton>();
                Button.atlas = CommonTextures.Atlas;
                Button.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
                Button.scaleFactor = 0.5f;
                Button.spritePadding.left = 10;
                Button.autoSize = false;
                Button.height = 50f;
                Button.width = 50f;
                Button.horizontalAlignment = UIHorizontalAlignment.Left;
                Button.color = Color.white;
                Button.textScale = 1.5f;
                Button.textHorizontalAlignment = UIHorizontalAlignment.Left;
                Button.textVerticalAlignment = UIVerticalAlignment.Middle;
            }
            private void AddLinesContainer()
            {
                Container = AddUIComponent<UIAutoLayoutPanel>();
                Container.name = "Container";
                Container.autoLayoutDirection = LayoutDirection.Vertical;
                Container.autoFitChildrenVertically = true;
                Container.autoLayoutPadding = new RectOffset(0, 0, 0, 6);
            }

            public void Init(ModVersion version, string message, CultureInfo culture)
            {
                IsExpand = false;
                Title.text = version.Number.ToString();
                if (version.IsBeta)
                    SubTitle.text = "BETA";
                else
                    SubTitle.text = version.Date.ToString("d MMM yyyy", culture);

                GetMessages(message);
            }
            private void GetMessages(string message)
            {
                message = message.Replace("\r\n", "\n").Replace($"\n---", "---");
                var lines = message.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                for (var i = 0; i < lines.Length; i += 1)
                {
                    lines[i] = lines[i].Replace("---", $"\n\t");
                    var match = Regex.Match(lines[i], @"(\[(?<tag>.+)\])?(?<text>(\n|.)+)");
                    var tag = match.Groups["tag"];
                    var text = match.Groups["text"];

                    if (text.Success)
                        Messages.Add(new MessageText(tag.Success ? tag.Value : null, text.Value));
                }
            }

            protected override void OnSizeChanged()
            {
                base.OnSizeChanged();

                if (Button != null)
                    Button.width = width - padding.horizontal;
                if (Container != null)
                    Container.width = width - padding.horizontal;
                foreach (var line in Container.components)
                    line.width = width - padding.horizontal;
            }
        }
        public readonly struct MessageText
        {
            public readonly string tag;
            public readonly string text;

            public MessageText(string tag, string text)
            {
                this.tag = tag;
                this.text = text;
            }
        }
        public class UpdateMessage : UIAutoLayoutPanel, IReusable
        {
            bool IReusable.InCache { get; set; }

            private CustomUILabel Tag { get; set; }
            private CustomUILabel Text { get; set; }

            public UpdateMessage()
            {
                autoLayoutDirection = LayoutDirection.Horizontal;
                autoFitChildrenVertically = true;
                autoLayoutPadding = new RectOffset(0, 0, 0, 0);
                autoLayoutPadding = new RectOffset(0, 10, 0, 0);

                Tag = AddUIComponent<CustomUILabel>();
                Tag.autoSize = false;
                Tag.height = 20f;
                Tag.width = 100f;
                Tag.atlas = CommonTextures.Atlas;
                Tag.backgroundSprite = CommonTextures.PanelSmall;
                Tag.textAlignment = UIHorizontalAlignment.Center;
                Tag.verticalAlignment = UIVerticalAlignment.Middle;
                Tag.textScale = 0.7f;
                Tag.padding = new RectOffset(0, 0, 4, 0);

                Text = AddUIComponent<CustomUILabel>();
                Text.textAlignment = UIHorizontalAlignment.Left;
                Text.verticalAlignment = UIVerticalAlignment.Middle;
                Text.textScale = 0.8f;
                Text.wordWrap = true;
                Text.autoSize = false;
                Text.autoHeight = true;
                Text.relativePosition = new Vector3(17, 7);
                Text.minimumSize = new Vector2(100, 20);
                Text.padding = new RectOffset(10, 10, 8, 0);
                Text.atlas = CommonTextures.Atlas;
                Text.backgroundSprite = CommonTextures.BorderTop;
                Text.color = ComponentStyle.NormalSettingsGray;
            }
            public void Init(MessageText message)
            {
                if (!string.IsNullOrEmpty(message.text))
                {
                    Tag.isVisible = true;
                    var tag = string.IsNullOrEmpty(message.tag) ? string.Empty : message.tag.Trim().ToUpper();
                    switch (tag)
                    {
                        case "NEW":
                            Tag.text = CommonLocalize.WhatsNew_NEW;
                            Tag.color = new Color32(49, 170, 77, 255);
                            break;
                        case "FIXED":
                            Tag.text = CommonLocalize.WhatsNew_FIXED;
                            Tag.color = new Color32(255, 102, 0, 255);
                            break;
                        case "UPDATED":
                            Tag.text = CommonLocalize.WhatsNew_UPDATED;
                            Tag.color = new Color32(57, 147, 249, 255);
                            break;
                        case "REMOVED":
                            Tag.text = CommonLocalize.WhatsNew_REMOVED;
                            Tag.color = new Color32(251, 185, 4, 255);
                            break;
                        case "REVERTED":
                            Tag.text = CommonLocalize.WhatsNew_REVERTED;
                            Tag.color = new Color32(251, 185, 4, 255);
                            break;
                        case "TRANSLATION":
                            Tag.text = CommonLocalize.WhatsNew_TRANSLATION;
                            Tag.color = new Color32(0, 79, 153, 255);
                            break;
                        case "WARNING":
                            Tag.text = CommonLocalize.WhatsNew_WARNING;
                            Tag.color = new Color32(217, 38, 38, 255);
                            break;
                        default:
                            Tag.text = tag.ToUpper();
                            Tag.color = new Color32(192, 192, 192, 255);
                            break;
                    }
                }
                else
                    Tag.isVisible = false;

                Text.text = message.text.Trim();
                Tag.textScale = Tag.text.Length <= 11 ? 0.7f : 0.5f;
                Fit();
            }
            protected override void OnSizeChanged()
            {
                base.OnSizeChanged();
                Fit();
            }
            protected override void OnVisibilityChanged()
            {
                base.OnVisibilityChanged();
                Fit();
            }
            private void Fit()
            {
                if (Text != null)
                    Text.width = width - (Tag?.isVisible == true ? 100f + autoLayoutPadding.horizontal : 0f);
            }

            void IReusable.DeInit()
            {
                Tag.text = string.Empty;
                Tag.color = Color.white;
                Text.text = string.Empty;
            }
        }
    }
    public class BetaWhatsNewMessageBox : WhatsNewMessageBox
    {
        private CustomUIButton GetStableButton { get; }
        public Func<bool> OnGetStableClick { get; set; }
        public string GetStableText { set => GetStableButton.text = value; }

        public BetaWhatsNewMessageBox()
        {
            GetStableButton = AddButton(OnGetStable, 2);
        }
        private void OnGetStable()
        {
            if (OnGetStableClick?.Invoke() != false)
                Close();
        }

        public void Init(Dictionary<ModVersion, string> messages, string betaText, string modName = null, bool maximizeFirst = true, CultureInfo culture = null)
        {
            StopLayout();

            var betaMessage = Panel.Content.AddUIComponent<CustomUILabel>();
            betaMessage.wordWrap = true;
            betaMessage.autoHeight = true;
            betaMessage.textColor = new Color32(255, 160, 0, 255);
            betaMessage.text = betaText;
            betaMessage.atlas = TextureHelper.InGameAtlas;
            betaMessage.backgroundSprite = "TextFieldPanel";
            betaMessage.color = new Color32(96, 96, 96, 255);
            betaMessage.padding = new RectOffset(7, 7, 7, 7);

            StartLayout(false);

            base.Init(messages, modName, maximizeFirst, culture);
        }
    }
}
