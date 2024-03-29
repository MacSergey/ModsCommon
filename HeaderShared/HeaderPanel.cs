﻿using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class BaseHeaderPanel<TypeContent> : EditorItem, IReusable
        where TypeContent : BaseHeaderContent
    {
        bool IReusable.InCache { get; set; }
        Transform IReusable.CachedTransform { get => m_CachedTransform; set => m_CachedTransform = value; }

        protected override float DefaultHeight => BaseHeaderContent.DefaultSize + 10;

        protected TypeContent Content { get; set; }
        public HeaderStyle ContentStyle { set => Content.HeaderStyle = value; }

        public BaseHeaderPanel() : base()
        {
            PauseLayout(() =>
            {
                AutoLayoutStart = LayoutStart.MiddleLeft;

                Content = AddUIComponent<TypeContent>();
                Content.PauseLayout(FillContent);

                Fill();
            });
        }
        protected virtual void Fill() { }
        protected abstract void FillContent();

        protected override void Init(float? height)
        {
            base.Init(height);
            Refresh();
        }

        public virtual void Refresh() => Content.Refresh();

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            PauseLayout(SetSize);
        }
        protected virtual void SetSize()
        {
            Content.size = new Vector2(width - Padding.horizontal, height);
        }
    }
    public abstract class BaseDeletableHeaderPanel<TypeContent> : BaseHeaderPanel<TypeContent>
        where TypeContent : BaseHeaderContent
    {
        public event Action OnDelete;

        protected CustomUIButton DeleteButton { get; private set; }

        protected override void Fill()
        {
            base.Fill();

            DeleteButton = AddUIComponent<CustomUIButton>();
            DeleteButton.name = nameof(DeleteButton);
            DeleteButton.Atlas = CommonTextures.Atlas;
            DeleteButton.BgSprites = new SpriteSet(CommonTextures.CloseButtonNormal, CommonTextures.CloseButtonHovered, CommonTextures.CloseButtonPressed, CommonTextures.CloseButtonNormal, string.Empty);
            DeleteButton.size = new Vector2(20, 20);
            DeleteButton.eventClick += DeleteClick;
        }
        public virtual void Init(float? height = null, bool isDeletable = true)
        {
            base.Init(height);
            DeleteButton.isVisible = isDeletable;
            PauseLayout(SetSize);
        }
        public override void DeInit()
        {
            base.DeInit();
            OnDelete = null;
        }
        protected override void SetSize()
        {
            if (DeleteButton.isVisible)
                Content.size = new Vector2(width - Content.relativePosition.x - AutoLayoutSpace - DeleteButton.width - PaddingRight, height);
            else
                base.SetSize();
        }

        private void DeleteClick(UIComponent component, UIMouseEventParameter eventParam) => OnDelete?.Invoke();
    }
}
