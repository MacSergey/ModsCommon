﻿using ColossalFramework.UI;
using System;
using System.Linq;
using UnityEngine;

namespace ModsCommon.UI
{
    public interface IAutoLayoutPanel
    {
        public Vector2 ItemSize { get; }
        public RectOffset LayoutPadding { get; }

        public int Level { get; }
        public void StopLayout();
        public void StartLayout(bool layoutNow = true, bool force = false);
        public void PauseLayout(Action action);
    }
    public class UIAutoLayoutPanel : CustomUIPanel, IAutoLayoutPanel
    {
        private int level;
        private bool fitVertically;
        private bool fitHorizontally;

        public new bool autoFitChildrenVertically
        {
            get => level == 0 ? base.autoFitChildrenVertically : fitVertically;
            set
            {
                if (level == 0)
                    base.autoFitChildrenVertically = value;
                else
                    fitVertically = value;
            }
        }
        public new bool autoFitChildrenHorizontally
        {
            get => level == 0 ? base.autoFitChildrenHorizontally : fitHorizontally;
            set
            {
                if (level == 0)
                    base.autoFitChildrenHorizontally = value;
                else
                    fitHorizontally = value;
            }
        }

        public int Level => level;

        public Vector2 ItemSize => new Vector2(width - autoLayoutPadding.horizontal - padding.horizontal, height - autoLayoutPadding.vertical - padding.vertical);
        public RectOffset LayoutPadding => autoLayoutPadding;

        public UIAutoLayoutPanel()
        {
            m_AutoLayout = true;
        }
        public virtual void StopLayout()
        {
            if (level == 0)
            {
                fitVertically = m_AutoFitChildrenVertically;
                fitHorizontally = m_AutoFitChildrenHorizontally;

                m_AutoLayout = false;
                m_AutoFitChildrenVertically = false;
                m_AutoFitChildrenHorizontally = false;
            }

            level += 1;
        }
        public virtual void StartLayout(bool layoutNow = true, bool force = false)
        {
            level = force ? 0 : Mathf.Max(level - 1, 0);

            if (level == 0)
            {
                m_AutoLayout = true;
                m_AutoFitChildrenVertically = fitVertically;
                m_AutoFitChildrenHorizontally = fitHorizontally;

                if (layoutNow)
                    Reset();
            }
        }
        public void PauseLayout(Action action)
        {
            if (action != null)
            {
                StopLayout();
                action();
                StartLayout();
            }
        }
    }
    public class UIAutoLayoutScrollablePanel : CustomUIScrollablePanel, IAutoLayoutPanel
    {
        private int level;
        public int Level => level;

        public Vector2 ItemSize => new Vector2(width - autoLayoutPadding.horizontal - scrollPadding.horizontal, height - autoLayoutPadding.vertical - scrollPadding.vertical);
        public RectOffset LayoutPadding => autoLayoutPadding;

        public UIAutoLayoutScrollablePanel()
        {
            m_AutoLayout = true;
        }
        public virtual void StopLayout()
        {
            if (level == 0)
                m_AutoLayout = false;

            level += 1;
        }
        public virtual void StartLayout(bool layoutNow = true, bool force = false)
        {
            level = force ? 0 : Mathf.Max(level - 1, 0);

            if (level == 0)
            {
                m_AutoLayout = true;
                if (layoutNow)
                    Reset();
            }
        }
        public void PauseLayout(Action action)
        {
            StopLayout();
            {
                action?.Invoke();
            }
            StartLayout();
        }
    }
    public abstract class BaseAdvancedScrollablePanel<TypeContent> : CustomUIPanel, IAutoLayoutPanel
        where TypeContent : UIAutoLayoutScrollablePanel
    {
        public int Level => Content.Level;
        public Vector2 ItemSize => Content.ItemSize;
        public RectOffset LayoutPadding => Content.LayoutPadding;

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
                        Content.verticalScrollbar.autoHide = true;
                    }
                    else
                    {
                        Content.verticalScrollbar.autoHide = false;
                        Content.verticalScrollbar.Hide();
                    }
                }
            }
        }

        public BaseAdvancedScrollablePanel()
        {
            clipChildren = true;

            Content = AddUIComponent<TypeContent>();
            Content.name = nameof(Content);

            Content.autoLayoutDirection = LayoutDirection.Vertical;
            Content.scrollWheelDirection = UIOrientation.Vertical;
            Content.builtinKeyNavigation = true;
            Content.clipChildren = true;
            Content.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            Content.autoReset = false;

            this.AddScrollbar(Content);

            Content.eventSizeChanged += ContentSizeChanged;
            Content.verticalScrollbar.eventVisibilityChanged += ScrollbarVisibilityChanged;
        }
        private void ContentSizeChanged(UIComponent component, Vector2 value)
        {
            foreach (var item in Content.components)
                item.width = Content.width - Content.autoLayoutPadding.horizontal - Content.scrollPadding.horizontal;
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

        private void SetContentSize() => Content.size = size - new Vector2(Content.verticalScrollbar.isVisible ? Content.verticalScrollbar.width : 0, 0);
        public virtual void StopLayout() => Content.StopLayout();
        public virtual void StartLayout(bool layoutNow = true, bool force = false) => Content.StartLayout(layoutNow, force);
        public void PauseLayout(Action action) => Content.PauseLayout(action);
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
            Content.verticalScrollbar.autoHide = false;
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
                if (autoLayout && parent != null)
                {
                    var height = 0f;
                    foreach (var component in components)
                    {
                        if (component.isVisibleSelf)
                            height = Mathf.Max(height, component.relativePosition.y + component.height);
                    }
                    if (components.Any())
                        height += autoLayoutPadding.bottom;

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

                    verticalScrollbar.isVisible = Mathf.CeilToInt(verticalScrollbar.scrollSize) < Mathf.CeilToInt(verticalScrollbar.maxValue - verticalScrollbar.minValue);
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
