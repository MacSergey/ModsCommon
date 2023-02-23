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
        protected virtual Color32 Color => NormalColor;
        protected virtual string BackgroundSprite => "ButtonWhite";
        protected virtual UITextureAtlas Atlas => TextureHelper.InGameAtlas;

        public PropertyGroupPanel()
        {
            atlas = Atlas;
            backgroundSprite = BackgroundSprite;
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
            isVisible = true;

            StartLayout(false);
        }

        protected override void OnSizeChanged()
        {
            StopLayout();
            foreach (var item in components)
                item.width = width - autoLayoutPadding.horizontal;
            StartLayout();

            base.OnSizeChanged();
        }
        protected override void OnComponentAdded(UIComponent child)
        {
            base.OnComponentAdded(child);

            if (child is EditorItem item && item.SupportEven)
            {
                item.eventVisibilityChanged += ItemVisibilityChanged;
                SetEven();
            }
        }
        protected override void OnComponentRemoved(UIComponent child)
        {
            base.OnComponentRemoved(child);

            if (child is EditorItem item && item.SupportEven)
            {
                item.eventVisibilityChanged -= ItemVisibilityChanged;
                SetEven();
            }
        }

        private void ItemVisibilityChanged(UIComponent component, bool value) => SetEven();

        public void SetEven()
        {
            if (!autoLayout)
                return;

            var supportEven = components.OfType<EditorItem>().Where(c => c.SupportEven && c.isVisible).ToArray();
            var even = supportEven.Length > 1;

            foreach (var item in supportEven)
            {
                item.IsEven = even;
                even = !even;
            }
        }

        public override void StartLayout(bool layoutNow = true)
        {
            base.StartLayout(layoutNow);
            SetEven();
        }
    }
}
