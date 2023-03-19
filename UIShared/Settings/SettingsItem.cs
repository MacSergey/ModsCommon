using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using ColossalFramework.UI;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class BaseSettingItem : CustomUIPanel { }
    public class EmptySpaceSettingsItem : BaseSettingItem
    {
        public EmptySpaceSettingsItem() 
        {
            AutoLayout = AutoLayout.Disabled;
            height = 15f;
        }
    }
    public class SettingsContentItem : BaseSettingItem
    {
        protected virtual RectOffset ItemsPadding => new RectOffset(10, 40, 5, 5);

        private Border borders = Border.None;
        public Border Borders
        {
            get => borders;
            set
            {
                if (value != borders)
                {
                    var padding = Padding;
                    borders = value;
                    Padding = padding;

                    BackgroundSprite = borders switch
                    {
                        Border.Top => CommonTextures.BorderTop,
                        Border.Bottom => CommonTextures.BorderBottom,
                        Border.Both => CommonTextures.BorderBoth,
                        _ => string.Empty
                    };
                }
            }
        }
        public new RectOffset Padding
        {
            get
            {
                var padding = base.Padding;
                var top = Math.Max(padding.top - ((borders & Border.Top) != 0 ? 2 : 0), 0);
                var bottom = Math.Max(padding.bottom - ((borders & Border.Bottom) != 0 ? 2 : 0), 0);
                return new RectOffset(padding.left, padding.right, top, bottom);
            }
            set
            {
                var top = value.top + ((borders & Border.Top) != 0 ? 2 : 0);
                var bottom = value.bottom + ((borders & Border.Bottom) != 0 ? 2 : 0);
                var padding = new RectOffset(value.left, value.right, top, bottom);
                base.Padding = padding;
            }
        }

        public CustomUIPanel Content { get; private set; }

        public SettingsContentItem()
        {
            Atlas = CommonTextures.Atlas;
            color = ComponentStyle.NormalSettingsGray;
            clipChildren = true;

            PauseLayout(() =>
            {
                Content = AddUIComponent<CustomUIPanel>();
                Content.name = nameof(Content);

                Content.PauseLayout(InitContent);

                Content.AutoLayout = AutoLayout.Horizontal;
                Content.AutoFitChildrenVertically = true;
                Content.eventSizeChanged += RefreshContent;
            });

            AutoLayout = AutoLayout.Vertical;
            autoFitChildrenVertically = true;
            Borders = Border.Top;
            Padding = ItemsPadding;
        }

        protected virtual void InitContent() { }
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            Content.width = width - base.Padding.horizontal;
        }
        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();

            if (isVisible)
                RefreshContent(Content, Content.size);
        }
        private void RefreshContent(UIComponent component, Vector2 size) => Content.PauseLayout(RefreshItems);
        protected virtual void RefreshItems() { }


        [Flags]
        public enum Border
        {
            None = 0,
            Top = 1,
            Bottom = 2,
            Both = Top | Bottom,
        }
    }
    public class LabelSettingsItem : SettingsContentItem
    {
        public CustomUILabel LabelItem { get; private set; }

        public string Label
        {
            get => LabelItem.text;
            set => LabelItem.text = value;
        }

        protected override void InitContent()
        {
            LabelItem = Content.AddUIComponent<CustomUILabel>();
            LabelItem.name = nameof(Label);
            LabelItem.autoSize = false;
            LabelItem.autoHeight = true;
            LabelItem.wordWrap = true;
            LabelItem.verticalAlignment = UIVerticalAlignment.Middle;
            LabelItem.padding = new RectOffset(0, 0, 2, 0);
        }
        protected override void RefreshItems()
        {
            LabelItem.width = Content.width;
        }
    }
    public abstract class ControlSettingsItem<ControlType> : LabelSettingsItem
        where ControlType : UIComponent
    {
        public ControlType Control { get; private set; }

        protected sealed override void InitContent()
        {
            base.InitContent();

            Control = Content.AddUIComponent<ControlType>();
            Control.eventSizeChanged += (_, _) => RefreshItems();

            InitControl();
        }
        protected virtual void InitControl() { }

        protected override void RefreshItems()
        {
            LabelItem.minimumSize = new Vector2(0, Control.height);
            LabelItem.width = Content.width - Control.width;
        }
    }
}
