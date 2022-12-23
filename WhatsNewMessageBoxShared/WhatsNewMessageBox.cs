using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        public virtual void Init(Dictionary<ModVersion, string> messages, string modName = null, bool maximizeFirst = true, CultureInfo culture = null)
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
            if (maximizeFirst)
                first.IsMinimize = false;

            StartLayout();
        }

        public class VersionMessage : UIAutoLayoutPanel
        {
            public bool IsMinimize
            {
                get => !Container.isVisible;
                set
                {
                    Container.isVisible = !value;
                    Button.text = value ? "►" : "▼";
                }
            }
            private CustomUILabel Title { get; set; }
            private CustomUILabel SubTitle { get; set; }
            private CustomUIButton Button { get; set; }
            private UIAutoLayoutPanel Container { get; set; }
            private List<UpdateMessage> Lines { get; } = new List<UpdateMessage>();
            public VersionMessage()
            {
                autoLayoutDirection = LayoutDirection.Vertical;
                autoFitChildrenVertically = true;
                padding = new RectOffset(5, 5, 5, 5);
                autoLayoutPadding = new RectOffset(0, 0, 0, 8);
                backgroundSprite = "TextFieldPanel";
                color = new Color32(64, 64, 64, 255);
                verticalSpacing = 5;

                AddTitle();
                AddLinesContainer();
            }
            private void AddTitle()
            {
                var titlePanel = AddUIComponent<UIAutoLayoutPanel>();
                titlePanel.autoLayoutDirection = LayoutDirection.Horizontal;
                titlePanel.autoFitChildrenVertically = true;
                titlePanel.autoFitChildrenHorizontally = true;
                titlePanel.autoLayoutPadding = new RectOffset(0, 10, 0, 0);
                titlePanel.eventClick += (UIComponent component, UIMouseEventParameter eventParam) => IsMinimize = !IsMinimize;

                var versionPanel = titlePanel.AddUIComponent<UIAutoLayoutPanel>();
                versionPanel.autoLayoutDirection = LayoutDirection.Vertical;
                versionPanel.autoFitChildrenVertically = true;
                versionPanel.autoFitChildrenHorizontally = true;
                versionPanel.atlas = TextureHelper.InGameAtlas;
                versionPanel.backgroundSprite = "TextFieldPanel";
                versionPanel.color = new Color32(180, 180, 180, 255);

                Title = versionPanel.AddUIComponent<CustomUILabel>();
                Title.autoSize = false;
                Title.width = 100f;
                Title.height = 30f;
                Title.textScale = 1.4f;
                Title.textAlignment = UIHorizontalAlignment.Center;
                Title.verticalAlignment = UIVerticalAlignment.Middle;
                Title.padding = new RectOffset(0, 0, 4, 0);

                SubTitle = versionPanel.AddUIComponent<CustomUILabel>();
                SubTitle.autoSize = false;
                SubTitle.width = 100f;
                SubTitle.height = 20f;
                SubTitle.textScale = 0.7f;
                SubTitle.textAlignment = UIHorizontalAlignment.Center;
                SubTitle.verticalAlignment = UIVerticalAlignment.Middle;

                Button = titlePanel.AddUIComponent<CustomUIButton>();
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
                Container.autoLayoutDirection = LayoutDirection.Vertical;
                Container.autoFitChildrenVertically = true;
                Container.autoLayoutPadding = new RectOffset(0, 0, 0, 6);
            }

            public void Init(ModVersion version, string message, CultureInfo culture)
            {
                Container.StopLayout();

                Title.text = version.Number.ToString();
                if (version.IsBeta)
                    SubTitle.text = "BETA";
                else
                    SubTitle.text = version.Date.ToString("d MMM yyyy", culture);
                SetMessage(message);
                IsMinimize = true;

                Container.StartLayout();
            }
            private void SetMessage(string message)
            {
                message = message.Replace("\r\n", "\n").Replace($"\n---", "---");
                var lines = message.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var max = Math.Max(lines.Length, Lines.Count);

                for (var i = 0; i < max; i += 1)
                {
                    if (i < lines.Length)
                    {
                        lines[i] = lines[i].Replace("---", $"\n\t");
                        var match = Regex.Match(lines[i], @"(\[(?<label>.+)\])?(?<text>(\n|.)+)");
                        var label = match.Groups["label"];
                        var text = match.Groups["text"];

                        if (text.Success)
                        {
                            if (i >= Lines.Count)
                                AddLine();
                            Lines[i].isVisible = true;
                            Lines[i].Init(label.Success ? label.Value : null, text.Value);
                        }
                        else
                            Lines[i].isVisible = false;
                    }
                    else
                        Lines[i].isVisible = false;
                }
            }
            private void AddLine()
            {
                var line = Container.AddUIComponent<UpdateMessage>();
                line.relativePosition = new Vector3(17, 7);
                Lines.Add(line);
            }

            protected override void OnSizeChanged()
            {
                base.OnSizeChanged();

                if (Button != null)
                    Button.width = width;
                if (Container != null)
                    Container.width = width;
                foreach (var line in Lines)
                    line.width = width;
            }
        }
        public class UpdateMessage : UIAutoLayoutPanel
        {
            private CustomUILabel Label { get; set; }
            private CustomUILabel Text { get; set; }

            public UpdateMessage()
            {
                autoLayoutDirection = LayoutDirection.Horizontal;
                autoFitChildrenVertically = true;
                autoLayoutPadding = new RectOffset(0, 0, 0, 0);
                autoLayoutPadding = new RectOffset(0, 10, 0, 0);

                Label = AddUIComponent<CustomUILabel>();
                Label.autoSize = false;
                Label.height = 20f;
                Label.width = 100f;
                Label.atlas = TextureHelper.InGameAtlas;
                Label.backgroundSprite = "TextFieldPanel";
                Label.textAlignment = UIHorizontalAlignment.Center;
                Label.verticalAlignment = UIVerticalAlignment.Middle;
                Label.textScale = 0.7f;
                Label.padding = new RectOffset(0, 0, 4, 0);

                Text = AddUIComponent<CustomUILabel>();
                Text.textAlignment = UIHorizontalAlignment.Left;
                Text.verticalAlignment = UIVerticalAlignment.Middle;
                Text.textScale = 0.8f;
                Text.wordWrap = true;
                Text.autoSize = false;
                Text.autoHeight = true;
                Text.relativePosition = new Vector3(17, 7);
                Text.minimumSize = new Vector2(100, 20);
                Text.padding = new RectOffset(0, 0, 4, 0);
            }
            public void Init(string label, string text)
            {
                if (!string.IsNullOrEmpty(label))
                {
                    Label.isVisible = true;
                    switch (label.Trim().ToUpper())
                    {
                        case "NEW":
                            Label.text = CommonLocalize.WhatsNew_NEW;
                            Label.color = new Color32(40, 178, 72, 255);
                            break;
                        case "FIXED":
                            Label.text = CommonLocalize.WhatsNew_FIXED;
                            Label.color = new Color32(255, 160, 0, 255);
                            break;
                        case "UPDATED":
                            Label.text = CommonLocalize.WhatsNew_UPDATED;
                            Label.color = new Color32(75, 228, 238, 255);
                            break;
                        case "REMOVED":
                            Label.text = CommonLocalize.WhatsNew_REMOVED;
                            Label.color = new Color32(225, 56, 225, 255);
                            break;
                        case "REVERTED":
                            Label.text = CommonLocalize.WhatsNew_REVERTED;
                            Label.color = new Color32(225, 56, 225, 255);
                            break;
                        case "TRANSLATION":
                            Label.text = CommonLocalize.WhatsNew_TRANSLATION;
                            Label.color = new Color32(0, 120, 255, 255);
                            break;
                        case "WARNING":
                            Label.text = CommonLocalize.WhatsNew_WARNING;
                            Label.color = new Color32(255, 34, 45, 255);
                            break;
                        default:
                            Label.text = label.ToUpper();
                            break;
                    }
                }
                else
                    Label.isVisible = false;

                Text.text = text.Trim();
                Label.textScale = Label.text.Length <= 11 ? 0.7f : 0.5f;
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
                    Text.width = width - (Label?.isVisible == true ? 100f + autoLayoutPadding.horizontal : 0f);
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
