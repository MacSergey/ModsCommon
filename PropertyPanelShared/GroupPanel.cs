using ColossalFramework.UI;
using ModsCommon.Utilities;
using System.Linq;
using UnityEngine;

namespace ModsCommon.UI
{
    public class PropertyGroupPanel : CustomUIPanel, IReusable
    {
        bool IReusable.InCache { get; set; }
        Transform IReusable.CachedTransform { get => m_CachedTransform; set => m_CachedTransform = value; }

        public virtual PropertyPanelStyle PanelStyle
        {
            set
            {
                atlas = value.BgAtlas;
                backgroundSprite = value.BgSprites.normal;
                bgColors = value.BgColors;

                Invalidate();
            }
        }

        public PropertyGroupPanel()
        {
            autoLayout = AutoLayout.Vertical;
            padding = new RectOffset(0, 0, 0, 0);
            autoChildrenVertically = AutoLayoutChildren.Fit;
        }

        public virtual void Init(float? width = null)
        {
            if (width != null)
                this.width = width.Value;
            else if (parent is UIScrollablePanel scrollablePanel)
                this.width = scrollablePanel.width - scrollablePanel.autoLayoutPadding.horizontal - scrollablePanel.scrollPadding.horizontal;
            else if (parent is CustomUIPanel customPanel)
                this.width = customPanel.ItemSize.x;
            else if (parent is UIPanel panel)
                this.width = panel.width - panel.autoLayoutPadding.horizontal;
            else
                this.width = parent.width;
        }

        public virtual void DeInit()
        {
            PauseLayout(() =>
            {
                var components = this.components.ToArray();
                foreach (var component in components)
                    ComponentPool.Free(component);
                isVisible = true;
            }, false);

            PanelStyle = ComponentStyle.Default.PropertyPanel;
        }

        protected override void OnSizeChanged()
        {
            PauseLayout(() =>
            {
                foreach (var item in components)
                    item.width = width - Padding.horizontal;
            });

            base.OnSizeChanged();
        }
        protected override void OnComponentAdded(UIComponent child)
        {
            base.OnComponentAdded(child);

            if (child is EditorPropertyPanel property)
            {
                property.eventVisibilityChanged += ItemVisibilityChanged;
                if (!IsLayoutSuspended)
                    SetBorder();
            }
        }
        protected override void OnComponentRemoved(UIComponent child)
        {
            base.OnComponentRemoved(child);

            if (child is EditorPropertyPanel property)
            {
                property.eventVisibilityChanged -= ItemVisibilityChanged;
                if (!IsLayoutSuspended)
                    SetBorder();
            }
        }

        private void ItemVisibilityChanged(UIComponent component, bool value)
        {
            if (!IsLayoutSuspended)
                SetBorder();
        }

        protected virtual void SetBorder()
        {
            var items = components.Where(p => p.isVisible).ToArray();
            for (int i = 0; i < items.Length; i += 1)
            {
                if (items[i] is EditorPropertyPanel property)
                    property.Borders = i == 0 ? PropertyBorder.None : PropertyBorder.Top;
            }
        }

        public override void StartLayout(bool layoutNow = true, bool force = false)
        {
            SetBorder();
            base.StartLayout(layoutNow, force);
        }


    }
}
