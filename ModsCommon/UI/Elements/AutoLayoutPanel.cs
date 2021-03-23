using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModsCommon.UI
{
    public interface IAutoLayoutPanel
    {
        public void StopLayout();
        public void StartLayout(bool layoutNow = true);
    }
    public class UIAutoLayoutPanel : UIPanel, IAutoLayoutPanel
    {
        public UIAutoLayoutPanel()
        {
            m_AutoLayout = true;
        }
        public void StopLayout() => m_AutoLayout = false;
        public void StartLayout(bool layoutNow = true)
        {
            m_AutoLayout = true;
            if (layoutNow)
                Reset();
        }
        public override void PerformLayout() { }
    }
    public class UIAutoLayoutScrollablePanel : UIScrollablePanel, IAutoLayoutPanel
    {
        public UIAutoLayoutScrollablePanel()
        {
            m_AutoLayout = true;
        }
        public void StopLayout() => m_AutoLayout = false;
        public void StartLayout(bool layoutNow = true)
        {
            m_AutoLayout = true;
            if (layoutNow)
                Reset();
        }
        public override void PerformLayout() { }
    }
    public class AdvancedScrollablePanel : UIPanel, IAutoLayoutPanel
    {
        public UIAutoLayoutScrollablePanel Content { get; private set; }
        public AdvancedScrollablePanel()
        {
            AddPanel();
        }

        private void AddPanel()
        {
            Content = AddUIComponent<UIAutoLayoutScrollablePanel>();

            Content.autoLayoutDirection = LayoutDirection.Vertical;
            Content.scrollWheelDirection = UIOrientation.Vertical;
            Content.builtinKeyNavigation = true;
            Content.clipChildren = true;
            Content.autoLayoutPadding = new RectOffset(0, 0, 0, 0);

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
            //var swAll = Stopwatch.StartNew();
            base.OnSizeChanged();
            SetContentSize();
            //UnityEngine.Debug.Log($"Advanced panel {name} content size changed: {swAll.ElapsedTicks}");
        }

        private void SetContentSize() => Content.size = size - new Vector2(Content.verticalScrollbar.isVisible ? Content.verticalScrollbar.width :0, 0);
        public void StopLayout() => Content.StopLayout();
        public void StartLayout(bool layoutNow = true) => Content.StartLayout(layoutNow);
        public override void PerformLayout() { }
    }
}
