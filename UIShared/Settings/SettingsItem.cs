using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using ColossalFramework.UI;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class SettingsItem : CustomUIPanel
    {
        public SettingsItem()
        {
            atlas = CommonTextures.Atlas;
            backgroundSprite = CommonTextures.Panel;
            color = new Color32(128, 0, 0, 255);
            autoLayout = false;
        }
    }
    public abstract class ContentSettingsItem : SettingsItem
    {
        protected virtual int ItemsPadding => 5;

        private CustomUILabel Label { get; set; }
        protected CustomUIPanel Content { get; set; }

        public string Text
        {
            get => Label.text;
            set => Label.text = value;
        }

        public ContentSettingsItem()
        {
            Label = AddUIComponent<CustomUILabel>();
            Label.autoSize = false;
            Label.autoHeight = true;
            Label.wordWrap = true;
            Label.padding = new RectOffset(0, 0, 2, 0);
            Label.name = nameof(Label);
            Label.eventTextChanged += (_, _) => SetLabel();

            Content = AddUIComponent<CustomUIPanel>();
            Content.autoLayoutDirection = LayoutDirection.Horizontal;
            Content.autoFitChildrenHorizontally = true;
            Content.autoLayoutPadding = new RectOffset(5, 0, 0, 0);
            Content.name = nameof(Content);
            Content.eventSizeChanged += (_, _) => RefreshContent();
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            Refresh(false);
        }
        protected void Refresh(bool refreshContent = true)
        {
            if (refreshContent)
                RefreshContent();

            Content.height = height;
            Content.relativePosition = new Vector2(width - Content.width - ItemsPadding, 0f);

            SetLabel();
        }
        private void RefreshContent()
        {
            Content.autoLayout = true;
            Content.autoLayout = false;

            foreach (var item in Content.components)
                item.relativePosition = new Vector2(item.relativePosition.x, (Content.height - item.height) * 0.5f);
        }

        private void SetLabel()
        {
            Label.width = width - Content.width - ItemsPadding * 2;
            Label.MakePixelPerfect(false);
            Label.relativePosition = new Vector2(5, (height - Label.height) * 0.5f);
        }
        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();

            if (isVisible)
                Refresh();
        }
    }
}
