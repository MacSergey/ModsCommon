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
        protected override int ContentSpacing => 10;

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
            PauseLayout(() =>
            {
                //if (!string.IsNullOrEmpty(modName))
                //{
                //    var donate = Panel.Content.AddUIComponent<DonatePanel>();
                //    donate.Init(modName, new Vector2(200f, 50f));
                //}

                var first = default(VersionMessage);
                foreach (var message in messages)
                {
                    var versionMessage = Content.AddUIComponent<VersionMessage>();
                    versionMessage.Init(message.Key, message.Value, culture);

                    if (first == null)
                        first = versionMessage;
                }
                if (expandFirst)
                    first.IsExpand = true;
            });
        }

        public class VersionMessage : CustomUIPanel
        {
            public bool IsExpand
            {
                get => Container.isVisible;
                set
                {
                    Container.isVisible = value;
                    Button.SetFgSprite(new SpriteSet(value ? CommonTextures.VectorDown : CommonTextures.VectorRight));

                    Container.PauseLayout(() =>
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
                    });
                }
            }
            private CustomUILabel Title { get; set; }
            private CustomUILabel SubTitle { get; set; }
            private CustomUIButton Button { get; set; }
            private CustomUIPanel Container { get; set; }
            private List<MessageText> Messages { get; } = new List<MessageText>();

            public VersionMessage()
            {
                autoLayout = AutoLayout.Vertical;
                autoChildrenVertically = AutoLayoutChildren.Fit;
                autoChildrenHorizontally = AutoLayoutChildren.Fill;
                Atlas = CommonTextures.Atlas;
                BackgroundSprite = CommonTextures.PanelBig;
                color = ComponentStyle.DarkPrimaryColor20;
                ForegroundSprite = CommonTextures.BorderBig;
                NormalFgColor = ComponentStyle.DarkPrimaryColor10;

                PauseLayout(() =>
                {
                    AddTitle();
                    AddLinesContainer();
                });
            }
            private void AddTitle()
            {
                var titlePanel = AddUIComponent<CustomUIPanel>();
                titlePanel.PauseLayout(() =>
                {
                    titlePanel.name = "TitlePanel";
                    titlePanel.AutoLayout = AutoLayout.Horizontal;
                    titlePanel.AutoChildrenVertically = AutoLayoutChildren.Fit;
                    titlePanel.AutoLayoutStart = LayoutStart.MiddleLeft;
                    titlePanel.Padding = new RectOffset(10, 20, 10, 10);
                    titlePanel.Atlas = CommonTextures.Atlas;
                    titlePanel.eventClick += (UIComponent component, UIMouseEventParameter eventParam) => IsExpand = !IsExpand;

                    var versionPanel = titlePanel.AddUIComponent<CustomUIPanel>();
                    versionPanel.PauseLayout(() =>
                    {
                        versionPanel.name = "VersionPanel";
                        versionPanel.AutoLayout = AutoLayout.Vertical;
                        versionPanel.AutoChildrenVertically = AutoLayoutChildren.Fit;
                        versionPanel.AutoChildrenHorizontally = AutoLayoutChildren.Fit;
                        versionPanel.Atlas = CommonTextures.Atlas;
                        versionPanel.BackgroundSprite = CommonTextures.PanelBig;
                        versionPanel.color = ComponentStyle.DarkPrimaryColor60;

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
                    });

                    var buttonPanel = titlePanel.AddUIComponent<CustomUIPanel>();
                    buttonPanel.name = "ButtonPanel";
                    buttonPanel.PauseLayout(() =>
                    {
                        buttonPanel.AutoLayout = AutoLayout.Horizontal;
                        buttonPanel.AutoLayoutStart = LayoutStart.MiddleRight;

                        Button = buttonPanel.AddUIComponent<CustomUIButton>();
                        Button.atlas = CommonTextures.Atlas;
                        Button.SetBgSprite(new SpriteSet(CommonTextures.Circle));
                        Button.SetBgColor(new ColorSet(ComponentStyle.DarkPrimaryColor30, ComponentStyle.DarkPrimaryColor35, ComponentStyle.DarkPrimaryColor40, ComponentStyle.DarkPrimaryColor30, ComponentStyle.DarkPrimaryColor30));
                        Button.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
                        Button.scaleFactor = 0.6f;
                        Button.autoSize = false;
                        Button.height = 40f;
                        Button.width = 40f;
                        Button.horizontalAlignment = UIHorizontalAlignment.Center;
                        Button.textScale = 1.5f;
                        Button.textHorizontalAlignment = UIHorizontalAlignment.Left;
                        Button.textVerticalAlignment = UIVerticalAlignment.Middle;
                    });

                    titlePanel.eventSizeChanged += (_, _) => buttonPanel.size = titlePanel.ItemSize - new Vector2(titlePanel.AutoLayoutSpace + versionPanel.width, 0f);
                });
            }
            private void AddLinesContainer()
            {
                Container = AddUIComponent<CustomUIPanel>();
                Container.name = nameof(Container);
                Container.PauseLayout(() =>
                {
                    Container.AutoLayout = AutoLayout.Vertical;
                    Container.AutoChildrenVertically = AutoLayoutChildren.Fit;
                    Container.AutoChildrenHorizontally = AutoLayoutChildren.Fill;
                    Container.AutoLayoutSpace = -2;
                    Container.Padding = new RectOffset(0, 0, 10, 5);
                });
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
        public class UpdateMessage : CustomUIPanel, IReusable
        {
            bool IReusable.InCache { get; set; }

            private CustomUILabel Tag { get; set; }
            private CustomUILabel Text { get; set; }

            public UpdateMessage()
            {
                autoLayout = AutoLayout.Horizontal;
                autoChildrenVertically = AutoLayoutChildren.Fit;
                autoLayoutSpace = 10;
                Padding = new RectOffset(10, 10, 0, 7);

                Atlas = CommonTextures.Atlas;
                ForegroundSprite = CommonTextures.PanelSmall;
                NormalFgColor = new Color32(0, 0, 0, 0);
                HoveredFgColor = ComponentStyle.DarkPrimaryColor30;
                SpritePadding = new RectOffset(5, 5, 0, 0);

                PauseLayout(() =>
                {
                    Tag = AddUIComponent<CustomUILabel>();
                    Tag.name = nameof(Tag);
                    Tag.autoSize = false;
                    Tag.height = 20f;
                    Tag.width = 100f;
                    Tag.atlas = CommonTextures.Atlas;
                    Tag.backgroundSprite = CommonTextures.PanelSmall;
                    Tag.textAlignment = UIHorizontalAlignment.Center;
                    Tag.verticalAlignment = UIVerticalAlignment.Middle;
                    Tag.textScale = 0.7f;
                    Tag.padding = new RectOffset(0, 0, 4, 0);
                    SetItemPadding(Tag, new RectOffset(0, 0, 8, 0));

                    Text = AddUIComponent<CustomUILabel>();
                    Text.name = nameof(Text);
                    Text.textAlignment = UIHorizontalAlignment.Left;
                    Text.verticalAlignment = UIVerticalAlignment.Middle;
                    Text.textScale = 0.8f;
                    Text.wordWrap = true;
                    Text.autoSize = false;
                    Text.autoHeight = true;
                    Text.relativePosition = new Vector3(17, 7);
                    Text.minimumSize = new Vector2(100, 20);
                    Text.padding = new RectOffset(10, 10, 10, 0);
                    Text.atlas = CommonTextures.Atlas;
                    Text.backgroundSprite = CommonTextures.BorderTop;
                    Text.color = ComponentStyle.DarkPrimaryColor30;
                });
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
                            Tag.color = new Color32(42, 185, 48, 255);
                            break;
                        case "FIXED":
                            Tag.text = CommonLocalize.WhatsNew_FIXED;
                            Tag.color = new Color32(237, 149, 38, 255);
                            break;
                        case "UPDATED":
                            Tag.text = CommonLocalize.WhatsNew_UPDATED;
                            Tag.color = new Color32(80, 150, 241, 255);
                            break;
                        case "REMOVED":
                            Tag.text = CommonLocalize.WhatsNew_REMOVED;
                            Tag.color = new Color32(243, 198, 0, 255);
                            break;
                        case "REVERTED":
                            Tag.text = CommonLocalize.WhatsNew_REVERTED;
                            Tag.color = new Color32(243, 198, 0, 255);
                            break;
                        case "TRANSLATION":
                            Tag.text = CommonLocalize.WhatsNew_TRANSLATION;
                            Tag.color = new Color32(0, 79, 153, 255);
                            break;
                        case "WARNING":
                            Tag.text = CommonLocalize.WhatsNew_WARNING;
                            Tag.color = new Color32(245, 65, 61, 255);
                            break;
                        default:
                            Tag.text = tag.ToUpper();
                            Tag.color = new Color32(189, 195, 199, 255);
                            break;
                    }
                }
                else
                    Tag.isVisible = false;

                Text.text = message.text.Trim();
                Tag.textScale = Tag.text.Length <= 11 ? 0.7f : 0.5f;
                SetSize();
            }
            protected override void OnSizeChanged()
            {
                base.OnSizeChanged();
                SetSize();
            }
            protected override void OnVisibilityChanged()
            {
                base.OnVisibilityChanged();
                SetSize();
            }
            private void SetSize()
            {
                if (Text != null)
                    Text.width = width - (Tag?.isVisible == true ? Tag.width + Padding.horizontal + AutoLayoutSpace : 0f);
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
            PauseLayout(() =>
            {
                var betaMessage = Content.AddUIComponent<CustomUILabel>();
                betaMessage.wordWrap = true;
                betaMessage.autoHeight = true;
                betaMessage.textColor = new Color32(255, 160, 0, 255);
                betaMessage.text = betaText;
                betaMessage.atlas = CommonTextures.Atlas;
                betaMessage.backgroundSprite = CommonTextures.PanelBig;
                betaMessage.color = ComponentStyle.DarkPrimaryColor30;
                betaMessage.padding = new RectOffset(7, 7, 7, 7);
            });

            base.Init(messages, modName, maximizeFirst, culture);
        }
    }
}
