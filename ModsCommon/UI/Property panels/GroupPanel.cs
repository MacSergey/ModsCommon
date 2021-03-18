using ColossalFramework.UI;
using ModsCommon.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModsCommon.UI
{
    public class PropertyGroupPanel : UIAutoLayoutPanel, IReusable
    {
        private static Color32 NormalColor { get; } = new Color32(82, 101, 117, 255);

        protected virtual Color32 Color => NormalColor;

        public PropertyGroupPanel()
        {
            atlas = TextureHelper.InGameAtlas;
            backgroundSprite = "ButtonWhite";
            color = Color;

            autoLayoutDirection = LayoutDirection.Vertical;
            autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            autoFitChildrenVertically = true;
        }

        public virtual void Init() => SetSize();

        public virtual void DeInit()
        {
            StopLayout();

            var components = this.components.ToArray();
            foreach (var component in components)
                ComponentPool.Free(component);

            StartLayout();
        }

        private void SetSize()
        {
            if (parent is UIScrollablePanel scrollablePanel)
                width = scrollablePanel.width - scrollablePanel.autoLayoutPadding.horizontal - scrollablePanel.scrollPadding.horizontal;
            else if (parent is UIPanel panel)
                width = panel.width - panel.autoLayoutPadding.horizontal;
            else
                width = parent.width;
        }
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            foreach (var item in components)
                item.width = width - autoLayoutPadding.horizontal;
        }
        protected override void OnComponentAdded(UIComponent child)
        {
            base.OnComponentAdded(child);

            if(child is EditorItem item && item.SupportEven)
            {
                item.eventVisibilityChanged += ItemVisibilityChanged;
                SetEven();
            }
        }
        protected override void OnComponentRemoved(UIComponent child)
        {
            if (child is EditorItem item && item.SupportEven)
            {
                item.eventVisibilityChanged -= ItemVisibilityChanged;
                SetEven();
            }
        }

        private void ItemVisibilityChanged(UIComponent component, bool value) => SetEven();

        public void SetEven()
        {
            var even = true;
            foreach (var item in components.OfType<EditorItem>().Where(c => c.SupportEven && c.isVisible))
            {
                item.IsEven = even;
                even = !even;
            }
        }
    }
}
