using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using UnityEngine;

namespace ModsCommon.UI
{
    public interface IHeaderButton
    {
        AutoSize AutoSize { get; set; }
        float height { get; set; }
        float width { get; set; }


        void SetIcon(UITextureAtlas atlas, string sprite);
        void SetSize(int buttonSize, int iconSize);
        void PerformAutoWidth();
        ColorSet BgColors {set;}
        ColorSet FgColors { set; }
    }
    public class HeaderButton : CustomUIButton, IHeaderButton, IReusable
    {
        bool IReusable.InCache { get; set; }

        public HeaderButton()
        {
            BgAtlas = CommonTextures.Atlas;
            BgSprites = new SpriteSet(string.Empty, CommonTextures.HeaderHover, CommonTextures.HeaderHover, CommonTextures.HeaderHover, string.Empty);
            clipChildren = true;
            textScale = 0.8f;
            TextHorizontalAlignment = UIHorizontalAlignment.Left;
            ForegroundSpriteMode = SpriteMode.Fill;
        }

        public void Init(UITextureAtlas atlas, string sprite, int size, int iconSize)
        {
            SetIcon(atlas, sprite);
            SetSize(size, iconSize);
        }
        public void SetSize(int buttonSize, int iconSize)
        {
            size = new Vector2(buttonSize, buttonSize);
            minimumSize = size;
            TextPadding = new RectOffset(iconSize + 5, 5, 5, 0);
        }
        public void SetIcon(UITextureAtlas atlas, string sprite)
        {
            FgAtlas = atlas ?? TextureHelper.InGameAtlas;
            FgSprites = sprite;
        }

        public override void Update()
        {
            base.Update();
            if (State == UIButton.ButtonState.Focused)
                State = UIButton.ButtonState.Normal;
        }

        public virtual void DeInit()
        {
            SetIcon(null, string.Empty);
        }

        protected override void OnClick(UIMouseEventParameter p)
        {
            p.Use();
            base.OnClick(p);
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

        public IHeaderButton Button { get; }

        public void AddButton(UIComponent parent, bool showText, int size, int iconSize);
        public void RemoveButton();
        public HeaderButtonState State { get; }
        public bool Visible { get; set; }
    }
    public class HeaderButtonInfo<TypeButton> : IHeaderButtonInfo
        where TypeButton : CustomUIButton, IHeaderButton
    {
        public event MouseEventHandler ClickedEvent
        {
            add => Button.eventClicked += value;
            remove => Button.eventClicked -= value;
        }

        IHeaderButton IHeaderButtonInfo.Button => Button;
        public TypeButton Button { get; }

        public HeaderButtonState State { get; }
        public string Text { get; set; }
        public Shortcut Shortcut { get; set; }
        private Action OnClick { get; }

        public bool Visible { get; set; } = true;
        public bool Enable
        {
            get => Button.isEnabled;
            set => Button.isEnabled = value;
        }
        private HeaderButtonInfo(HeaderButtonState state, UITextureAtlas atlas, string sprite, Action onClick)
        {
            State = state;
            OnClick = onClick;

            Button = new GameObject(typeof(TypeButton).Name).AddComponent<TypeButton>();
            Button.SetIcon(atlas, sprite);
            Button.eventClicked += ButtonClicked;
        }
        public HeaderButtonInfo(HeaderButtonState state, UITextureAtlas atlas, string sprite, string text, Action onClick = null) : this(state, atlas, sprite, onClick) 
        {
            Text = text;
        }
        public HeaderButtonInfo(HeaderButtonState state, UITextureAtlas atlas, string sprite, string text, Shortcut shortcut) : this(state, atlas, sprite, shortcut.Press)
        {
            Text = text;
            Shortcut = shortcut;
        }

        public void AddButton(UIComponent parent, bool showText, int size, int iconSize)
        {
            RemoveButton();

            parent.AttachUIComponent(Button.gameObject);
            Button.transform.parent = parent.cachedTransform;

            Button.text = showText ? GetText() : string.Empty;
            Button.tooltip = showText ? string.Empty : GetText();
            Button.HorizontalAlignment = showText ? UIHorizontalAlignment.Left : UIHorizontalAlignment.Center;
            Button.SetSize(size, iconSize);
        }
        public void RemoveButton()
        {
            Button.parent?.RemoveUIComponent(Button);
            Button.transform.parent = null;
        }

        private void ButtonClicked(UIComponent component, UIMouseEventParameter eventParam) => OnClick?.Invoke();

        private string GetText()
        {
            if (Shortcut == null || Shortcut.NotSet)
                return Text;
            else
                return $"{Text} ({Shortcut})";
        }
    }
}
