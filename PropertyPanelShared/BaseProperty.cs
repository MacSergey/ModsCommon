using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class EditorItem : CustomUIPanel
    {
        public event Action<VisibleState> OnVisibleStateChanged;

        protected virtual float DefaultHeight => 30;
        protected virtual int ItemsPadding => 5;

        public virtual bool EnableControl { get; set; } = true;

        private CustomUISprite Even { get; }
        public virtual bool SupportEven => false;
        public bool IsEven
        {
            get => SupportEven && Even.isVisible;
            set
            {
                if (SupportEven)
                    Even.isVisible = value;
            }
        }

        private bool _canCollapse = true;
        public bool CanCollapse
        {
            get => _canCollapse;
            set
            {
                if (value != _canCollapse)
                {
                    _canCollapse = value;
                    if (!_canCollapse)
                        IsCollapsed = false;
                }
            }
        }

        private VisibleState _visibleState = VisibleState.Visible;
        private VisibleState VisibleState
        {
            get => _visibleState;
            set
            {
                if (value != _visibleState)
                {
                    _visibleState = value;
                    isVisible = _visibleState == VisibleState.Visible;
                    OnVisibleStateChanged?.Invoke(_visibleState);
                }
            }
        }
        public bool IsCollapsed
        {
            get => (VisibleState & VisibleState.Collapsed) != 0;
            set
            {
                if (CanCollapse && value)
                    VisibleState |= VisibleState.Collapsed;
                else
                    VisibleState &= ~VisibleState.Collapsed;
            }
        }
        public bool IsHidden
        {
            get => (VisibleState & VisibleState.Hidden) != 0;
            set
            {
                if (value)
                    VisibleState |= VisibleState.Hidden;
                else
                    VisibleState &= ~VisibleState.Hidden;
            }
        }

        public EditorItem()
        {
            if (SupportEven)
            {
                Even = AddUIComponent<CustomUISprite>();
                Even.atlas = CommonTextures.Atlas;
                Even.spriteName = CommonTextures.Empty;
                Even.color = new Color32(0, 0, 0, 48);
                IsEven = false;
            }
        }

        public virtual void DeInit()
        {
            IsEven = false;
            EnableControl = true;
            _visibleState = VisibleState.Visible;
            _canCollapse = true;
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
        private CustomUILabel LabelItem { get; set; }
        protected ContentPanel Content { get; set; }

        public string Label
        {
            get => LabelItem.text;
            set => LabelItem.text = value;
        }
        public override bool EnableControl
        {
            get => Content.isEnabled;
            set => Content.isEnabled = value;
        }
        public override bool SupportEven => true;

        public EditorPropertyPanel()
        {
            LabelItem = AddUIComponent<CustomUILabel>();
            LabelItem.textScale = 0.75f;
            LabelItem.autoSize = false;
            LabelItem.autoHeight = true;
            LabelItem.wordWrap = true;
            LabelItem.padding = new RectOffset(0, 0, 2, 0);
            LabelItem.disabledTextColor = new Color32(160, 160, 160, 255);
            LabelItem.name = nameof(LabelItem);
            LabelItem.eventTextChanged += (_, _) => SetLabel();

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
            Label = string.Empty;
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
            LabelItem.width = width - Content.width - ItemsPadding * 2;
            LabelItem.MakePixelPerfect(false);
            LabelItem.relativePosition = new Vector2(5, (height - LabelItem.height) / 2);
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
                    item.relativePosition = new Vector2(item.relativePosition.x, (height - item.height) * 0.5f);
            }
        }
    }

    public enum VisibleState
    {
        Visible = 0,
        Collapsed = 1,
        Hidden = 2,
    }
}
