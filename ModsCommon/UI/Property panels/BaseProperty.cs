using ColossalFramework.UI;
using ModsCommon.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class EditorItem : UIPanel
    {
        protected virtual float DefaultHeight => 30;
        protected virtual int ItemsPadding => 5;

        public virtual bool EnableControl { get; set; } = true;

        private UIPanel Even { get; }
        public virtual bool SupportEven => false;
        public bool IsEven
        {
            get => Even.isVisible;
            set => Even.isVisible = value;
        }

        public EditorItem()
        {
            Even = AddUIComponent<UIPanel>();
            Even.atlas = TextureHelper.CommonAtlas;
            Even.backgroundSprite = TextureHelper.EmptySprite;
            Even.color = new Color32(0, 0, 0, 48);
            IsEven = false;
        }

        public virtual void Init() => Init(null);
        public virtual void DeInit()
        {
            isEnabled = true;
            isVisible = true;
            IsEven = false;
            EnableControl = true;
        }
        public void Init(float? height = null)
        {
            size = new Vector2(GetWidth(), height ?? DefaultHeight);
        }
        private float GetWidth()
        {
            if (parent is UIScrollablePanel scrollablePanel)
                return scrollablePanel.width - scrollablePanel.autoLayoutPadding.horizontal - scrollablePanel.scrollPadding.horizontal;
            else if (parent is UIPanel panel)
                return panel.width - panel.autoLayoutPadding.horizontal;
            else
                return parent.width;
        }

        protected UIButton AddButton(UIComponent parent)
        {
            var button = parent.AddUIComponent<UIButton>();
            button.SetDefaultStyle();
            return button;
        }
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            if (Even != null)
                Even.size = size;
        }
        public override void PerformLayout() { }

        public override string ToString() => name;
    }
    public abstract class EditorPropertyPanel : EditorItem
    {
        private UILabel Label { get; set; }
        protected ContentPanel Content { get; set; }

        public string Text
        {
            get => Label.text;
            set => Label.text = value;
        }
        public override bool EnableControl
        {
            get => Content.isEnabled;
            set => Content.isEnabled = value;
        }
        public override bool SupportEven => true;

        public EditorPropertyPanel()
        {
            Label = AddUIComponent<UILabel>();
            Label.textScale = 0.8f;
            Label.disabledTextColor = new Color32(160, 160, 160, 255);
            Label.name = nameof(Label);

            Content = AddUIComponent<ContentPanel>();
        }
        public override void Init()
        {
            base.Init();
            Content.Refresh();
        }
        public override void DeInit()
        {
            base.DeInit();
            Text = string.Empty;
        }
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            Label.relativePosition = new Vector2(5, (height - Label.height) / 2);
            Content.size = size - new Vector2(ItemsPadding * 2, 0);
            Content.relativePosition = new Vector2(ItemsPadding, 0f);
        }
        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();

            if (isVisible)
                Content.Refresh();
        }

        protected class ContentPanel : UIPanel
        {
            public ContentPanel()
            {
                autoLayoutDirection = LayoutDirection.Horizontal;
                autoLayoutStart = LayoutStart.TopRight;
                autoLayoutPadding = new RectOffset(5, 0, 0, 0);
                name = nameof(Content);
            }

            protected override void OnSizeChanged()
            {
                base.OnSizeChanged();
                Refresh();
            }
            public override void PerformLayout() { }
            public void Refresh()
            {
                autoLayout = true;
                autoLayout = false;

                foreach (var item in components)
                    item.relativePosition = new Vector2(item.relativePosition.x, (height - item.height) / 2);
            }
        }
    }
}
