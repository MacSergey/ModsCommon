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
            backgroundSprite = CommonTextures.BorderTop;
            color = ComponentStyle.NormalSettingsGray;
            autoLayout = false;
            clipChildren = true;
        }
    }
    public abstract class ContentSettingsItem : SettingsItem
    {
        protected virtual RectOffset ItemsPadding => new RectOffset(10, 30, 5, 5);

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
            Label.name = nameof(Label);
            Label.autoSize = false;
            Label.autoHeight = true;
            Label.wordWrap = true;
            Label.padding = new RectOffset(0, 0, 2, 0);
            Label.name = nameof(Label);
            Label.eventTextChanged += (_, _) => SetLabel();

            Content = AddUIComponent<CustomUIPanel>();
            Content.name = nameof(Content);
            Content.autoLayoutDirection = LayoutDirection.Horizontal;
            Content.autoFitChildrenHorizontally = true;
            Content.autoLayoutPadding = new RectOffset(5, 0, 0, 0);
            Content.eventSizeChanged += (_, _) => RefreshContent();
        }
        protected void SetHeightBasedOn(UIComponent component) => SetHeight(component.height);
        protected void SetHeight(float height)
        {
            this.height = height + 2f + ItemsPadding.vertical;
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
            Content.relativePosition = new Vector2(width - Content.width - ItemsPadding.right, 2f);

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
            Label.width = width - Content.width - ItemsPadding.horizontal;
            Label.MakePixelPerfect(false);
            Label.relativePosition = new Vector2(ItemsPadding.left, (height - Label.height) * 0.5f + 2f);
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
