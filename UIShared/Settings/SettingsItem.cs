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
            backgroundSprite = CommonTextures.BorderBottom;
            color = new Color32(60, 75, 76, 255);
            autoLayout = false;
            clipChildren = true;
        }
    }
    public abstract class ContentSettingsItem : SettingsItem
    {
        protected virtual int ItemsVerticalPadding => 5;
        protected virtual int ItemsHorizontalPadding => 20;

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
        protected void SetHeightBasedOn(UIComponent component) => SetHeight(component.height);
        protected void SetHeight(float height)
        {
            this.height = height + 2f + ItemsVerticalPadding * 2f;
            MakePixelPerfect(false);
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

            Content.height = height - 2f;
            Content.relativePosition = new Vector2(width - Content.width - ItemsHorizontalPadding, 2f);

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
            Label.width = width - Content.width - ItemsHorizontalPadding * 2;
            Label.MakePixelPerfect(false);
            Label.relativePosition = new Vector2(ItemsHorizontalPadding, (height - Label.height) * 0.5f + 2f);
        }
        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();

            if (isVisible)
                Refresh();
        }
    }
    public class LabelSettingsItem : ContentSettingsItem 
    {
        public LabelSettingsItem()
        {
            height = 30f;
        }
    }
}
