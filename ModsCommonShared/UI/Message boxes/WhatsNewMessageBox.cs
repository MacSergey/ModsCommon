using ColossalFramework.UI;
using ModsCommon.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NodeMarkup.UI
{
    public class WhatsNewMessageBox : MessageBoxBase
    {
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

        public virtual void Init(Dictionary<Version, string> messages, Func<Version, string> toString = null)
        {
            StopLayout();

            var first = default(VersionMessage);
            foreach (var message in messages)
            {
                var versionMessage = Panel.Content.AddUIComponent<VersionMessage>();
                //versionMessage.width = ScrollableContent.width;
                versionMessage.Init(toString?.Invoke(message.Key) ?? message.Key.ToString(), message.Value);

                if (first == null)
                    first = versionMessage;
            }
            first.IsMinimize = false;

            StartLayout();
        }

        public class VersionMessage : CustomUIPanel
        {
            public bool IsMinimize
            {
                get => !Message.isVisible;
                set => Message.isVisible = !value;
            }
            CustomUIButton Button { get; set; }
            CustomUILabel Message { get; set; }
            string Label { get; set; }
            public VersionMessage()
            {
                autoLayout = true;
                autoLayoutDirection = LayoutDirection.Vertical;
                autoFitChildrenVertically = true;
                autoLayoutPadding = new RectOffset(0, 0, Padding / 2, Padding / 2);

                AddButton();
                AddText();
            }

            public void AddButton()
            {
                Button = AddUIComponent<CustomUIButton>();
                Button.height = 20;
                Button.horizontalAlignment = UIHorizontalAlignment.Left;
                Button.color = Color.white;
                Button.textHorizontalAlignment = UIHorizontalAlignment.Left;
                Button.eventClick += (UIComponent component, UIMouseEventParameter eventParam) => IsMinimize = !IsMinimize;
            }

            public void AddText()
            {
                Message = AddUIComponent<CustomUILabel>();
                Message.textAlignment = UIHorizontalAlignment.Left;
                Message.verticalAlignment = UIVerticalAlignment.Middle;
                Message.textScale = 0.8f;
                Message.wordWrap = true;
                Message.autoHeight = true;
                Message.relativePosition = new Vector3(17, 7);
                Message.eventVisibilityChanged += (UIComponent component, bool value) => SetLabel();
            }

            public void Init(string version, string message)
            {
                Label = version;
                Message.text = message;
                IsMinimize = true;

                SetLabel();
            }
            private void SetLabel() => Button.text = $"{(IsMinimize ? "►" : "▼")} {Label}";

            protected override void OnSizeChanged()
            {
                base.OnSizeChanged();
                if (Button != null)
                    Button.width = width;
                if (Message != null)
                    Message.width = width;
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
            var betaMessage = Panel.Content.AddUIComponent<CustomUILabel>();
            betaMessage.wordWrap = true;
            betaMessage.autoHeight = true;
            betaMessage.textColor = Color.red;
            betaMessage.text = betaText;

            base.Init(messages, toString);
        }
    }
}
