using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class EditorItem : CustomUIPanel
    {
        protected virtual float DefaultHeight => 34;
        protected static int ItemsPadding => 7;

        public virtual bool EnableControl { get; set; } = true;

        public EditorItem()
        {
            autoLayout = AutoLayout.Horizontal;
        }

        public virtual void DeInit()
        {
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
            else if (parent is CustomUIPanel customPanel)
                return customPanel.width - customPanel.Padding.horizontal;
            else if (parent is UIPanel panel)
                return panel.width - panel.autoLayoutPadding.horizontal;
            else
                return parent.width;
        }

        public override string ToString() => name;
    }
    public abstract class BaseEditorPanel : EditorItem
    {
        public event Action<VisibleState> OnVisibleStateChanged;

        private bool canCollapse = true;
        public bool CanCollapse
        {
            get => canCollapse;
            set
            {
                if (value != canCollapse)
                {
                    canCollapse = value;
                    if (!canCollapse)
                        IsCollapsed = false;
                }
            }
        }

        private VisibleState visibleState = VisibleState.Visible;
        private VisibleState VisibleState
        {
            get => visibleState;
            set
            {
                if (value != visibleState)
                {
                    visibleState = value;
                    isVisible = visibleState == VisibleState.Visible;
                    OnVisibleStateChanged?.Invoke(visibleState);
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
        private PropertyBorder borders = PropertyBorder.None;
        public PropertyBorder Borders
        {
            get => borders;
            set
            {
                if (value != borders)
                {
                    var padding = Padding;
                    borders = value;
                    Padding = padding;

                    ForegroundSprite = borders switch
                    {
                        PropertyBorder.Top => CommonTextures.BorderTop,
                        PropertyBorder.Bottom => CommonTextures.BorderBottom,
                        PropertyBorder.Both => CommonTextures.BorderBoth,
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
                var top = Math.Max(padding.top - ((borders & PropertyBorder.Top) != 0 ? 2 : 0), 0);
                var bottom = Math.Max(padding.bottom - ((borders & PropertyBorder.Bottom) != 0 ? 2 : 0), 0);
                return new RectOffset(padding.left, padding.right, top, bottom);
            }
            set
            {
                var top = value.top + ((borders & PropertyBorder.Top) != 0 ? 2 : 0);
                var bottom = value.bottom + ((borders & PropertyBorder.Bottom) != 0 ? 2 : 0);
                var padding = new RectOffset(value.left, value.right, top, bottom);
                base.Padding = padding;
            }
        }

        private bool isEven;
        public bool IsEven
        {
            get => isEven;
            set
            {
                isEven = value;
                BackgroundSprite = value ? CommonTextures.Empty : string.Empty;
            }
        }

        public BaseEditorPanel()
        {
            Padding = new RectOffset(10, 10, 7, 7);
        }

        public override void DeInit()
        {
            base.DeInit();

            visibleState = VisibleState.Visible;
            canCollapse = true;
            IsEven = false;
        }
    }
    public abstract class EditorPropertyPanel : BaseEditorPanel
    {
        private CustomUILabel LabelItem { get; set; }
        protected CustomUIPanel Content { get; set; }

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

        public EditorPropertyPanel() : base()
        {
            PauseLayout(() =>
            {
                autoFitChildrenVertically = true;
                autoLayout = AutoLayout.Horizontal;

                Atlas = CommonTextures.Atlas;
                NormalFgColor = new Color32(0, 0, 0, 96);
                NormalBgColor = new Color32(0, 0, 0, 48);
                SpritePadding.left = 10;
                SpritePadding.right = 10;
                Borders = PropertyBorder.Top;
                Padding = new RectOffset(15, 15, 0, 0);

                LabelItem = AddUIComponent<CustomUILabel>();
                LabelItem.name = "Label";
                LabelItem.relativePosition = Vector3.zero;
                LabelItem.textScale = 0.8f;
                LabelItem.autoSize = false;
                LabelItem.autoHeight = false;
                LabelItem.wordWrap = true;
                LabelItem.padding = new RectOffset(0, 5, 5, 0);
                LabelItem.disabledTextColor = new Color32(160, 160, 160, 255);
                LabelItem.verticalAlignment = UIVerticalAlignment.Middle;

                Content = AddUIComponent<CustomUIPanel>();
                Content.name = nameof(Content);
                Content.PauseLayout(() =>
                {
                    Content.AutoLayout = AutoLayout.Horizontal;
                    Content.AutoFitChildrenHorizontally = true;
                    Content.AutoFitChildrenVertically = true;
                    Content.AutoLayoutSpace = 5;
                    Content.AutoLayoutCenter = true;
                    Content.Padding = new RectOffset(0, 0, 7, 7);
                    Content.eventSizeChanged += ContentSizeChanged;

                    FillContent();
                });
            });
        }
        protected abstract void FillContent();

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            SetLabel();
        }
        private void ContentSizeChanged(UIComponent component, Vector2 value) => SetLabel();

        private void SetLabel()
        {
            PauseLayout(() =>
            {
                LabelItem.size = new Vector2(width - Content.width - Padding.horizontal, Content.height);
                LabelItem.MakePixelPerfect(false);
            }, false);
        }
    }

    public enum VisibleState
    {
        Visible = 0,
        Collapsed = 1,
        Hidden = 2,
    }
    public enum PropertyBorder
    {
        None = 0,
        Top = 1,
        Bottom = 2,
        Both = Top | Bottom,
    }
}
