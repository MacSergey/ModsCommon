using ColossalFramework.UI;
using System;
using System.Collections.Generic;
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

        public virtual void Init(Dictionary<Version, string> messages, Func<Version, string> toString = null, bool MaximizeFirst = true)
        {
            StopLayout();

            var first = default(VersionMessage);
            foreach (var message in messages)
            {
                var versionMessage = Panel.Content.AddUIComponent<VersionMessage>();
                versionMessage.Init(toString?.Invoke(message.Key) ?? message.Key.ToString(), message.Value);

                if (first == null)
                    first = versionMessage;
            }
            if (MaximizeFirst)
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
                    Button.text = $"{(value ? "►" : "▼")} {Label}";
                }
            }
            private CustomUIButton Button { get; set; }
            private UIAutoLayoutPanel Container { get; set; }
            private List<CustomUILabel> Lines { get; } = new List<CustomUILabel>();
            private string Label { get; set; }
            public VersionMessage()
            {
                autoLayoutDirection = LayoutDirection.Vertical;
                autoFitChildrenVertically = true;
                autoLayoutPadding = new RectOffset(0, 0, 0, 6);

                AddButton();
                AddLinesContainer();
            }
            private void AddButton()
            {
                Button = AddUIComponent<CustomUIButton>();
                Button.height = 20;
                Button.horizontalAlignment = UIHorizontalAlignment.Left;
                Button.color = Color.white;
                Button.textHorizontalAlignment = UIHorizontalAlignment.Left;
                Button.eventClick += (UIComponent component, UIMouseEventParameter eventParam) => IsMinimize = !IsMinimize;
            }

            private void AddLinesContainer()
            {
                Container = AddUIComponent<UIAutoLayoutPanel>();
                Container.autoLayoutDirection = LayoutDirection.Vertical;
                Container.autoFitChildrenVertically = true;
                Container.autoLayoutPadding = new RectOffset(0, 0, 0, 6);
                Container.backgroundSprite = "ContentManagerItemBackground";
                Container.verticalSpacing = 15;
            }

            public void Init(string version, string message)
            {
                Container.StopLayout();

                Label = version;
                SetMessage(message);
                IsMinimize = true;

                Container.StartLayout();
            }
            private void SetMessage(string message)
            {
                var lines = message.Split('\n');
                var max = Math.Max(lines.Length, Lines.Count);

                for (var i = 0; i < max; i += 1)
                {
                    if (i < lines.Length)
                    {
                        if (i >= Lines.Count)
                            AddLine();
                        Lines[i].isVisible = true;
                        Lines[i].text = lines[i];
                    }
                    else
                        Lines[i].isVisible = false;
                }
            }
            private void AddLine()
            {
                var line = Container.AddUIComponent<CustomUILabel>();
                line.textAlignment = UIHorizontalAlignment.Left;
                line.verticalAlignment = UIVerticalAlignment.Middle;
                line.textScale = 0.8f;
                line.wordWrap = true;
                line.autoHeight = true;
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

        public void Init(Dictionary<Version, string> messages, string betaText, Func<Version, string> toString = null)
        {
            StopLayout();

            var betaMessage = Panel.Content.AddUIComponent<CustomUILabel>();
            betaMessage.wordWrap = true;
            betaMessage.autoHeight = true;
            betaMessage.textColor = Color.red;
            betaMessage.text = betaText;

            StartLayout();

            base.Init(messages, toString);
        }
    }
}
