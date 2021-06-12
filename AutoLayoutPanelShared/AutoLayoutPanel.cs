using ColossalFramework.UI;
using System.Linq;
using UnityEngine;

namespace ModsCommon.UI
{
    public interface IAutoLayoutPanel
    {
        public void StopLayout();
        public void StartLayout(bool layoutNow = true);
    }
    public class UIAutoLayoutPanel : CustomUIPanel, IAutoLayoutPanel
    {
        public UIAutoLayoutPanel()
        {
            m_AutoLayout = true;
        }
        public virtual void StopLayout() => m_AutoLayout = false;
        public virtual void StartLayout(bool layoutNow = true)
        {
            m_AutoLayout = true;
            if (layoutNow)
                Reset();
        }
    }
    public class UIAutoLayoutScrollablePanel : CustomUIScrollablePanel, IAutoLayoutPanel
    {
        public UIAutoLayoutScrollablePanel()
        {
            m_AutoLayout = true;
        }
        public virtual void StopLayout() => m_AutoLayout = false;
        public virtual void StartLayout(bool layoutNow = true)
        {
            m_AutoLayout = true;
            if (layoutNow)
                Reset();
        }
    }
    public abstract class BaseAdvancedScrollablePanel<TypeContent> : CustomUIPanel, IAutoLayoutPanel
        where TypeContent : UIAutoLayoutScrollablePanel
    {
        public TypeContent Content { get; private set; }
        public BaseAdvancedScrollablePanel()
        {
            clipChildren = true;

            Content = AddUIComponent<TypeContent>();

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
                item.width = Content.width - Content.autoLayoutPadding.horizontal;
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
        public void StopLayout() => Content.StopLayout();
        public void StartLayout(bool layoutNow = true) => Content.StartLayout(layoutNow);
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
            public override void StartLayout(bool layoutNow = true)
            {
                base.StartLayout(layoutNow);
                if (layoutNow)
                    FitContentChildren();
            }
        }
    }
}
