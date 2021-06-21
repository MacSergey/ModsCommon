using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModsCommon.UI
{
    public class HeaderButton : CustomUIButton, IReusable
    {
        bool IReusable.InCache { get; set; }

        public static int Size => IconSize + 2 * IconPadding;
        public static int IconSize => 25;
        public static int IconPadding => 2;
        public CustomUIButton Icon { get; }

        public HeaderButton()
        {
            atlas = CommonTextures.Atlas;
            hoveredBgSprite = pressedBgSprite = focusedBgSprite = CommonTextures.HeaderHoverSprite;
            size = new Vector2(Size, Size);
            clipChildren = true;
            textPadding = new RectOffset(IconSize + 5, 5, 5, 0);
            textScale = 0.8f;
            textHorizontalAlignment = UIHorizontalAlignment.Left;
            minimumSize = size;

            Icon = AddUIComponent<CustomUIButton>();
            Icon.size = new Vector2(IconSize, IconSize);
            Icon.relativePosition = new Vector2(IconPadding, IconPadding);
        }
        public void SetIcon(UITextureAtlas atlas, string sprite)
        {
            Icon.atlas = atlas ?? TextureHelper.InGameAtlas;
            Icon.normalBgSprite = sprite;
            Icon.hoveredBgSprite = sprite;
            Icon.pressedBgSprite = sprite;
        }

        public override void Update()
        {
            base.Update();
            if (state == ButtonState.Focused)
                state = ButtonState.Normal;
        }

        public virtual void DeInit()
        {
            SetIcon(null, string.Empty);
        }
    }
    [Flags]
    public enum HeaderButtonState
    {
        Main = 1,
        Additional = 2,
        //Auto = Main | Additional,
    }
    public interface IHeaderButtonInfo
    {
        public event MouseEventHandler ClickedEvent;

        public HeaderButton Button { get; }

        public void AddButton(UIComponent parent, bool showText);
        public void RemoveButton();
        public HeaderButtonState State { get; }
        public bool Visible { get; set; }
    }
    public class HeaderButtonInfo<TypeButton> : IHeaderButtonInfo
        where TypeButton : HeaderButton
    {
        public event MouseEventHandler ClickedEvent
        {
            add => Button.eventClicked += value;
            remove => Button.eventClicked -= value;
        }

        HeaderButton IHeaderButtonInfo.Button => Button;
        public TypeButton Button { get; }

        public HeaderButtonState State { get; }
        public string Text { get; }
        private Action OnClick { get; }

        public bool Visible { get; set; } = true;
        public bool Enable
        {
            get => Button.isEnabled;
            set => Button.isEnabled = value;
        }

        public HeaderButtonInfo(HeaderButtonState state, UITextureAtlas atlas, string sprite, string text, Action onClick = null)
        {
            State = state;
            Text = text;
            OnClick = onClick;

            Button = new GameObject(typeof(TypeButton).Name).AddComponent<TypeButton>();
            Button.SetIcon(atlas, sprite);
            Button.eventClicked += ButtonClicked;
        }
        public HeaderButtonInfo(HeaderButtonState state, UITextureAtlas atlas, string sprite, string text, Shortcut shortcut) : this(state, atlas, sprite, GetText(text, shortcut), shortcut.Press) { }

        public void AddButton(UIComponent parent, bool showText)
        {
            RemoveButton();

            parent.AttachUIComponent(Button.gameObject);
            Button.transform.parent = parent.cachedTransform;

            Button.text = showText ? Text ?? string.Empty : string.Empty;
            Button.tooltip = showText ? string.Empty : Text;
        }
        public void RemoveButton()
        {
            Button.parent?.RemoveUIComponent(Button);
            Button.transform.parent = null;
        }

        private void ButtonClicked(UIComponent component, UIMouseEventParameter eventParam) => OnClick?.Invoke();

        protected static string GetText(string text, Shortcut shortcut) => $"{text} ({shortcut})";
    }
}
