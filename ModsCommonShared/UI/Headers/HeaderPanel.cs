using ColossalFramework.UI;
using System;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class BaseHeaderPanel<TypeContent> : EditorItem, IReusable
        where TypeContent : BaseHeaderContent
    {
        protected override float DefaultHeight => HeaderButton.Size + 10;

        protected TypeContent Content { get; set; }

        public BaseHeaderPanel()
        {
            AddContent();
        }
        private void AddContent()
        {
            Content = AddUIComponent<TypeContent>();
            Content.relativePosition = new Vector2(0, 0);
        }
    }
    public class BaseHeaderContent : CustomUIPanel
    {
        public BaseHeaderContent()
        {
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoLayoutPadding = new RectOffset(0, Math.Max(5 - 2 * HeaderButton.IconPadding, 0), 0, 0);
        }

        public ButtonType AddButton<ButtonType>(string sprite, string text, bool showText = false, MouseEventHandler onClick = null) where ButtonType : HeaderButton
            => AddButton<ButtonType>(this, sprite, text, showText, onClick);
        public ButtonType AddButton<ButtonType>(UIComponent parent, string sprite, string text, bool showText = false, MouseEventHandler onClick = null)
            where ButtonType : HeaderButton
        {
            var button = parent.AddUIComponent<ButtonType>();
            if (showText)
                button.text = text ?? string.Empty;
            else
                button.tooltip = text;
            button.SetIconSprite(sprite);

            if (onClick != null)
                button.eventClick += onClick;
            return button;
        }

        protected override void OnComponentAdded(UIComponent child)
        {
            base.OnComponentAdded(child);
            child.eventVisibilityChanged += ChildVisibilityChanged;
            child.eventSizeChanged += ChildSizeChanged;
        }
        protected override void OnComponentRemoved(UIComponent child)
        {
            base.OnComponentRemoved(child);
            child.eventVisibilityChanged -= ChildVisibilityChanged;
            child.eventSizeChanged -= ChildSizeChanged;
        }

        private void ChildVisibilityChanged(UIComponent component, bool value) => PlaceChildren();
        private void ChildSizeChanged(UIComponent component, Vector2 value) => PlaceChildren();

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            PlaceChildren();
        }

        public void PlaceChildren()
        {
            autoLayout = true;
            autoLayout = false;

            foreach (var item in components)
                item.relativePosition = new Vector2(item.relativePosition.x, (height - item.height) / 2);
        }
    }
}
