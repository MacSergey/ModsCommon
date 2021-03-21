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

        public virtual void Init(float? width = null) 
        {
            if (width != null)
                this.width = width.Value;
            else if (parent is UIScrollablePanel scrollablePanel)
                this.width = scrollablePanel.width - scrollablePanel.autoLayoutPadding.horizontal - scrollablePanel.scrollPadding.horizontal;
            else if (parent is UIPanel panel)
                this.width = panel.width - panel.autoLayoutPadding.horizontal;
            else
                this.width = parent.width;
        }

        public virtual void DeInit()
        {
            StopLayout();

            var components = this.components.ToArray();
            foreach (var component in components)
                ComponentPool.Free(component);

            StartLayout();
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
            var supportEven = components.OfType<EditorItem>().Where(c => c.SupportEven && c.isVisible).ToArray();

            if (supportEven.Length > 1)
            {
                foreach (var item in supportEven)
                {
                    item.IsEven = even;
                    even = !even;
                }
            }
        }
    }
}
