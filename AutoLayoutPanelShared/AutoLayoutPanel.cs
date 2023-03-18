﻿using ColossalFramework.UI;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace ModsCommon.UI
{
    public interface IAutoLayoutPanel
    {
        public Vector2 ItemSize { get; }
        public RectOffset LayoutPadding { get; }

        public bool IsLayoutSuspended { get; }
        public void StopLayout();
        public void StartLayout(bool layoutNow = true, bool force = false);
        public void PauseLayout(Action action, bool layoutNow = true, bool force = false);
    }
    public class UIAutoLayoutPanel : CustomUIPanel
    {
        public UIAutoLayoutPanel()
        {
            autoLayout = Utilities.AutoLayout.Horizontal;
        }
    }
    public class UIAutoLayoutScrollablePanel : CustomUIScrollablePanel, IAutoLayoutPanel
    {
        public UIAutoLayoutScrollablePanel()
        {
            m_AutoLayout = true;
        }
    }
    public abstract class BaseAdvancedScrollablePanel<TypeContent> : CustomUIPanel, IAutoLayoutPanel
        where TypeContent : UIAutoLayoutScrollablePanel
    {
        public new bool IsLayoutSuspended => Content.IsLayoutSuspended;
        public new Vector2 ItemSize => Content.ItemSize;
        public new RectOffset LayoutPadding => Content.LayoutPadding;

        public TypeContent Content { get; private set; }

        private bool showScroll = true;
        public bool ShowScroll
        {
            get => showScroll;
            set
            {
                if (value != showScroll)
                {
                    showScroll = value;
                    if (value)
                    {
                        Content.VerticalScrollbar.AutoHide = true;
                    }
                    else
                    {
                        Content.VerticalScrollbar.AutoHide = false;
                        Content.VerticalScrollbar.Hide();
                    }
                }
            }
        }

        public BaseAdvancedScrollablePanel()
        {
            clipChildren = true;

            Content = AddUIComponent<TypeContent>();
            Content.name = nameof(Content);

            Content.AutoLayoutDirection = LayoutDirection.Vertical;
            Content.ScrollWheelDirection = UIOrientation.Vertical;
            Content.builtinKeyNavigation = true;
            Content.clipChildren = true;
            Content.AutoLayoutPadding = new RectOffset(0, 0, 0, 0);
            Content.AutoReset = false;

            this.AddScrollbar(Content);

            Content.eventSizeChanged += ContentSizeChanged;

            if (Content.VerticalScrollbar != null)
                Content.VerticalScrollbar.eventVisibilityChanged += ScrollbarVisibilityChanged;
        }
        private void ContentSizeChanged(UIComponent component, Vector2 value)
        {
            foreach (var item in Content.components)
                item.width = Content.width - Content.AutoLayoutPadding.horizontal - Content.ScrollPadding.horizontal;
        }

        private bool InProgress { get; set; } = false;
        private void ScrollbarVisibilityChanged(UIComponent component, bool value)
        {
            if (InProgress && !value)
                return;

            InProgress = true;
            SetContentSize();
            InProgress = false;
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            SetContentSize();
        }

        private void SetContentSize()
        {
            if (Content.VerticalScrollbar != null && Content.VerticalScrollbar.isVisible)
                Content.width = width - Content.VerticalScrollbar.width;
            else
                Content.width = width;

            if (Content.HorizontalScrollbar != null && Content.HorizontalScrollbar.isVisible)
                Content.height = height - Content.HorizontalScrollbar.height;
            else
                Content.height = height;
        }
        public override void StopLayout() => Content.StopLayout();
        public override void StartLayout(bool layoutNow = true, bool force = false) => Content.StartLayout(layoutNow, force);
        public override void PauseLayout(Action action, bool layoutNow = true, bool force = false) => Content.PauseLayout(action, layoutNow, force);
    }

    public class AdvancedScrollablePanel : BaseAdvancedScrollablePanel<UIAutoLayoutScrollablePanel> { }
    public class AutoSizeAdvancedScrollablePanel : BaseAdvancedScrollablePanel<AutoSizeAdvancedScrollablePanel.AutoSizeScrollablePanel>
    {
        public Vector2 MaxSize
        {
            get => maximumSize;
            set
            {
                maximumSize = value;
                Content.maximumSize = value;
            }
        }

        public AutoSizeAdvancedScrollablePanel()
        {
            Content.VerticalScrollbar.AutoHide = false;
        }

        public class AutoSizeScrollablePanel : UIAutoLayoutScrollablePanel
        {
            protected override void OnComponentAdded(UIComponent child)
            {
                base.OnComponentAdded(child);
                FitContentChildren();

                child.eventVisibilityChanged += OnChildVisibilityChanged;
                child.eventSizeChanged += OnChildSizeChanged;
            }

            protected override void OnComponentRemoved(UIComponent child)
            {
                base.OnComponentRemoved(child);
                FitContentChildren();

                child.eventVisibilityChanged -= OnChildVisibilityChanged;
                child.eventSizeChanged -= OnChildSizeChanged;
            }

            private void OnChildVisibilityChanged(UIComponent component, bool value) => FitContentChildren();
            private void OnChildSizeChanged(UIComponent component, Vector2 value) => FitContentChildren();

            private void FitContentChildren()
            {
                if (AutoLayout && parent != null)
                {
                    var height = 0f;
                    foreach (var component in components)
                    {
                        if (component.isVisibleSelf)
                            height = Mathf.Max(height, component.relativePosition.y + component.height);
                    }
                    if (components.Any())
                        height += AutoLayoutPadding.bottom;

                    if (height < this.height)
                    {
                        this.height = height;
                        parent.height = height;
                    }
                    else
                    {
                        parent.height = height;
                        this.height = height;
                    }

                    VerticalScrollbar.isVisible = Mathf.CeilToInt(VerticalScrollbar.ScrollSize) < Mathf.CeilToInt(VerticalScrollbar.MaxValue - VerticalScrollbar.MinValue);
                }
            }
            public override void StartLayout(bool layoutNow = true, bool force = false)
            {
                base.StartLayout(layoutNow, force);
                if (layoutNow)
                    FitContentChildren();
            }
        }
    }
}
