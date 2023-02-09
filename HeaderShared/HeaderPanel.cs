using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class BaseHeaderPanel<TypeContent> : EditorItem, IReusable
        where TypeContent : BaseHeaderContent
    {
        bool IReusable.InCache { get; set; }
        protected override float DefaultHeight => BaseHeaderContent.DefaultSize + 10;

        protected TypeContent Content { get; set; }

        public BaseHeaderPanel()
        {
            AddContent();
        }
        protected override void Init(float? height)
        {
            base.Init(height);
            Refresh();
        }
        private void AddContent()
        {
            Content = AddUIComponent<TypeContent>();
            Content.relativePosition = new Vector2(0, 0);
        }

        public virtual void Refresh()
        {
            Content.Refresh();
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            SetSize();
        }
        protected virtual void SetSize()
        {
            Content.size = new Vector2(width - ItemsPadding, height);
            Content.relativePosition = new Vector2(ItemsPadding, 0f);
        }
    }
    public abstract class BaseDeletableHeaderPanel<TypeContent> : BaseHeaderPanel<TypeContent>
        where TypeContent : BaseHeaderContent
    {
        public event Action OnDelete;

        protected CustomUIButton DeleteButton { get; set; }

        public BaseDeletableHeaderPanel() : base()
        {
            AddDeleteButton();
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
        protected override void SetSize()
        {
            Content.size = new Vector2((DeleteButton.isVisible ? width - DeleteButton.width - 10 : width) - ItemsPadding, height);
            Content.relativePosition = new Vector2(ItemsPadding, 0f);
            DeleteButton.relativePosition = new Vector2(width - DeleteButton.width - 5, (height - DeleteButton.height) / 2);
        }

        private void AddDeleteButton()
        {
            DeleteButton = AddUIComponent<CustomUIButton>();
            DeleteButton.zOrder = 0;
            DeleteButton.atlas = CommonTextures.Atlas;
            DeleteButton.normalBgSprite = CommonTextures.CloseButtonNormal;
            DeleteButton.hoveredBgSprite = CommonTextures.CloseButtonHovered;
            DeleteButton.pressedBgSprite = CommonTextures.CloseButtonPressed;
            DeleteButton.size = new Vector2(20, 20);
            DeleteButton.eventClick += DeleteClick;
        }
        private void DeleteClick(UIComponent component, UIMouseEventParameter eventParam) => OnDelete?.Invoke();
    }
}
