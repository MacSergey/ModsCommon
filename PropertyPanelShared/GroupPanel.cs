using ColossalFramework.UI;
using ModsCommon.Utilities;
using System.Linq;
using UnityEngine;

namespace ModsCommon.UI
{
    public class PropertyGroupPanel : UIAutoLayoutPanel, IReusable
    {
        protected static Color32 NormalColor { get; } = new Color32(82, 101, 117, 255);

        bool IReusable.InCache { get; set; }
        protected virtual Color32 DefaultColor => NormalColor;
        protected virtual string DefaultBackgroundSprite => CommonTextures.PanelBig;
        protected virtual UITextureAtlas DefaultAtlas => CommonTextures.Atlas;

        public PropertyGroupPanel()
        {
            Atlas = DefaultAtlas;
            BackgroundSprite = DefaultBackgroundSprite;
            color = disabledColor = DefaultColor;

            autoLayout = AutoLayout.Vertical;
            padding = new RectOffset(0, 0, 0, 0);
            autoFitChildrenVertically = true;
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
