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
    public abstract class HeaderPanel : EditorItem, IReusable
    {
        public event Action OnDelete;
        protected override float DefaultHeight => HeaderButton.Size + 10;

        protected HeaderContent Content { get; set; }
        protected UIButton DeleteButton { get; set; }

        public HeaderPanel()
        {
            AddDeleteButton();
            AddContent();
        }

        public virtual void Init(float? height = null, bool isDeletable = true)
        {
            base.Init(height);
            DeleteButton.isVisible = isDeletable;
            SetSize();
        }
        public override void DeInit()
        {
            base.DeInit();
            OnDelete = null;
        }
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            SetSize();
        }
        private void SetSize()
        {
            Content.size = new Vector2((DeleteButton.isVisible ? width - DeleteButton.width - 10 : width) - ItemsPadding, height);
            Content.relativePosition = new Vector2(ItemsPadding, 0f);
            DeleteButton.relativePosition = new Vector2(width - DeleteButton.width - 5, (height - DeleteButton.height) / 2);
        }

        private void AddContent()
        {
            Content = AddUIComponent<HeaderContent>();
            Content.relativePosition = new Vector2(0, 0);
        }

        private void AddDeleteButton()
        {
            DeleteButton = AddUIComponent<UIButton>();
            DeleteButton.atlas = TextureHelper.CommonAtlas;
            DeleteButton.normalBgSprite = TextureHelper.DeleteNormal;
            DeleteButton.hoveredBgSprite = TextureHelper.DeleteHover;
            DeleteButton.pressedBgSprite = TextureHelper.DeletePressed;
            DeleteButton.size = new Vector2(20, 20);
            DeleteButton.eventClick += DeleteClick;
        }
        private void DeleteClick(UIComponent component, UIMouseEventParameter eventParam) => OnDelete?.Invoke();

    }
    public class HeaderContent : UIPanel
    {
        public HeaderContent()
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
            if(showText)
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
        public override void PerformLayout() { }

        public void PlaceChildren()
        {
            autoLayout = true;
            autoLayout = false;

            foreach (var item in components)
                item.relativePosition = new Vector2(item.relativePosition.x, (height - item.height) / 2);
        }
    }
}
