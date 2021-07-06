using ColossalFramework.UI;
using ModsCommon.Utilities;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class EditorItem : CustomUIPanel
    {
        protected virtual float DefaultHeight => 30;
        protected virtual int ItemsPadding => 5;

        public virtual bool EnableControl { get; set; } = true;

        private CustomUIPanel Even { get; }
        public virtual bool SupportEven => false;
        public bool IsEven
        {
            get => Even.isVisible;
            set => Even.isVisible = value;
        }

        public EditorItem()
        {
            Even = AddUIComponent<CustomUIPanel>();
            Even.atlas = CommonTextures.Atlas;
            Even.backgroundSprite = CommonTextures.EmptySprite;
            Even.color = new Color32(0, 0, 0, 48);
            IsEven = false;
        }

        public virtual void DeInit()
        {
            IsEven = false;
            EnableControl = true;
        }
        public virtual void Init() => Init(null);
        protected virtual void Init(float? height)
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

        protected CustomUIButton AddButton(UIComponent parent)
        {
            var button = parent.AddUIComponent<CustomUIButton>();
            button.SetDefaultStyle();
            return button;
        }
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            if (Even != null)
                Even.size = size;
        }
        public override string ToString() => name;
    }
    public abstract class EditorPropertyPanel : EditorItem
    {
        private CustomUILabel Label { get; set; }
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
            Label = AddUIComponent<CustomUILabel>();
            Label.textScale = 0.75f;
            Label.autoSize = false;
            Label.autoHeight = true;
            Label.wordWrap = true;
            Label.padding = new RectOffset(0, 0, 2, 0);
            Label.disabledTextColor = new Color32(160, 160, 160, 255);
            Label.name = nameof(Label);
            Label.eventTextChanged += (_, _) => SetLabel();

            Content = AddUIComponent<ContentPanel>();
        }

        protected override void Init(float? height)
        {
            base.Init(height);
            Refresh();
        }
        public override void DeInit()
        {
            base.DeInit();
            Text = string.Empty;
        }
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            Refresh(false);
        }
        protected void Refresh(bool refreshContent = true)
        {
            if (refreshContent)
                Content.Refresh();

            Content.height = height;
            Content.relativePosition = new Vector2(width - Content.width - ItemsPadding, 0f);

            SetLabel();
        }
        private void SetLabel()
        {
            Label.width = width - Content.width - ItemsPadding * 2;
            Label.MakePixelPerfect(false);
            Label.relativePosition = new Vector2(5, (height - Label.height) / 2);
        }
        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();

            if (isVisible)
                Refresh();
        }

        protected class ContentPanel : CustomUIPanel
        {
            public ContentPanel()
            {
                autoLayoutDirection = LayoutDirection.Horizontal;
                autoFitChildrenHorizontally = true;
                autoLayoutPadding = new RectOffset(5, 0, 0, 0);
                name = nameof(Content);
            }

            protected override void OnSizeChanged()
            {
                base.OnSizeChanged();
                Refresh();
            }
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
