using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Linq;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class BaseHeaderPanel<TypeContent> : EditorItem, IReusable
        where TypeContent : BaseHeaderContent
    {
        bool IReusable.InCache { get; set; }
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

        private void AddDeleteButton()
        {
            DeleteButton = AddUIComponent<CustomUIButton>();
            DeleteButton.zOrder = 0;
            DeleteButton.atlas = CommonTextures.Atlas;
            DeleteButton.normalBgSprite = CommonTextures.DeleteNormal;
            DeleteButton.hoveredBgSprite = CommonTextures.DeleteHover;
            DeleteButton.pressedBgSprite = CommonTextures.DeletePressed;
            DeleteButton.size = new Vector2(20, 20);
            DeleteButton.eventClick += DeleteClick;
        }
        private void DeleteClick(UIComponent component, UIMouseEventParameter eventParam) => OnDelete?.Invoke();
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

        private void ChildVisibilityChanged(UIComponent component, bool value) => Refresh();
        private void ChildSizeChanged(UIComponent component, Vector2 value) => Refresh();

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            Refresh();
        }
        public virtual void Refresh()
        {
            autoLayout = true;
            autoLayout = false;
            FitChildrenHorizontally();

            foreach (var item in components)
                item.relativePosition = new Vector2(item.relativePosition.x, (height - item.height) / 2);
        }
    }

    public abstract class BasePanelHeaderContent<TypeButton, TypeAdditionallyButton> : BaseHeaderContent
        where TypeButton : HeaderButton
        where TypeAdditionallyButton : BaseAdditionallyHeaderButton
    {
        private TypeAdditionallyButton Additionally { get; }

        public BasePanelHeaderContent()
        {
            AddButtons();
            if (GetAdditionally() is TypeAdditionallyButton additional)
            {
                Additionally = additional;
                Additionally.OpenPopupEvent += OnAdditionallyPopup;
            }
        }
        private void OnAdditionallyPopup(AdditionallyPopup popup)
        {
            AddAdditionallyButtons(popup.Content);
            var buttons = popup.Content.components.OfType<TypeButton>().ToArray();
            foreach (var button in buttons)
            {
                button.autoSize = true;
                button.autoSize = false;
            }

            popup.Width = buttons.Max(b => b.width);
        }

        protected virtual void AddButtons() { }
        protected virtual void AddAdditionallyButtons(UIComponent parent) { }
        protected virtual TypeAdditionallyButton GetAdditionally() => null;

        protected TypeButton AddButton(string sprite, string text, Shortcut shortcut) => AddButton<TypeButton>(sprite, GetText(text, shortcut), onClick: (_, _) => shortcut.Press());
        protected TypeButton AddButton(string sprite, string text, MouseEventHandler onClick) => AddButton<TypeButton>(sprite, text, onClick: onClick);

        protected TypeButton AddPopupButton(UIComponent parent, string sprite, string text, Shortcut shortcut)
        {
            return AddButton<TypeButton>(parent, sprite, GetText(text, shortcut), true, action);

            void action(UIComponent component, UIMouseEventParameter eventParam)
            {
                Additionally.ClosePopup();
                shortcut.Press();
            }
        }
        protected TypeButton AddPopupButton(UIComponent parent, string sprite, string text, MouseEventHandler onClick)
        {
            return AddButton<TypeButton>(parent, sprite, text, true, action);

            void action(UIComponent component, UIMouseEventParameter eventParam)
            {
                Additionally.ClosePopup();
                onClick(component, eventParam);
            }
        }

        protected string GetText(string text, Shortcut shortcut) => $"{text} ({shortcut})";
    }
    public class AdditionallyPopup : PopupPanel
    {
        protected override Color32 Background => new Color32(64, 64, 64, 255);
    }
    public abstract class BasePanelHeaderButton : HeaderButton
    {
        protected override Color32 HoveredColor => new Color32(112, 112, 112, 255);
        protected override Color32 PressedColor => new Color32(144, 144, 144, 255);
        protected override Color32 PressedIconColor => Color.white;
    }
    public abstract class BaseAdditionallyHeaderButton : HeaderPopupButton<AdditionallyPopup>
    {
        protected override Color32 HoveredColor => new Color32(112, 112, 112, 255);
        protected override Color32 PressedColor => new Color32(144, 144, 144, 255);
        protected override Color32 PressedIconColor => Color.white;
    }
}
