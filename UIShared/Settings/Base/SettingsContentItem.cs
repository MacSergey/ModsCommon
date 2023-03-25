using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using ColossalFramework.UI;
using UnityEngine;

namespace ModsCommon.UI
{
    public class ContentSettingsItem : BorderSettingsItem
    {
        protected virtual RectOffset ItemsPadding => new RectOffset(20, 20, 7, 7);
        public CustomUIPanel Content { get; private set; }

        public ContentSettingsItem() : base()
        {
            AutoLayout = AutoLayout.Vertical;
            autoChildrenVertically = AutoLayoutChildren.Fit;
            autoChildrenHorizontally = AutoLayoutChildren.Fill;

            PauseLayout(() =>
            {
                Content = AddUIComponent<CustomUIPanel>();
                Content.name = nameof(Content);

                Content.PauseLayout(InitContent);

                Content.AutoLayout = AutoLayout.Horizontal;
                Content.AutoChildrenVertically = AutoLayoutChildren.Fit;
                Content.eventSizeChanged += RefreshContent;
            });

            Padding = ItemsPadding;
        }

        protected virtual void InitContent() { }
        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();

            if (isVisible)
                RefreshContent(Content, Content.size);
        }
        private void RefreshContent(UIComponent component, Vector2 size) => Content.PauseLayout(RefreshItems);
        protected virtual void RefreshItems() { }
    }
}
