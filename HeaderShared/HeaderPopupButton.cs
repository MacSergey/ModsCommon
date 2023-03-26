using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Linq;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class BaseHeaderPopupButton<PopupType> : BaseDropDown<PopupType>, IHeaderButton
        where PopupType : UIComponent
    {
        public BaseHeaderPopupButton()
        {
            AtlasBackground = CommonTextures.Atlas;
            BgSprites = new SpriteSet(string.Empty, CommonTextures.HeaderHover, CommonTextures.HeaderHover, CommonTextures.HeaderHover, string.Empty);

            clipChildren = true;
            textScale = 0.8f;
            TextHorizontalAlignment = UIHorizontalAlignment.Left;
            ForegroundSpriteMode = UIForegroundSpriteMode.Fill;
        }

        protected override void WhilePopupClosing()
        {
            foreach (var items in Popup.components.ToArray())
                ComponentPool.Free(items);
        }

        public void SetIcon(UITextureAtlas atlas, string sprite)
        {
            AtlasForeground = atlas ?? TextureHelper.InGameAtlas;
            FgSprites = sprite;
        }

        public void SetSize(int buttonSize, int iconSize)
        {
            size = new Vector2(buttonSize, buttonSize);
            minimumSize = size;
            TextPadding = new RectOffset(iconSize + 5, 5, 5, 0);
        }
        protected override void OnUpdate()
        {
            base.OnUpdate();
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
    public abstract class HeaderPopupButton<PopupType> : BaseHeaderPopupButton<PopupType>
        where PopupType : PopupPanel
    {
        protected override void WhilePopupOpening()
        {
            Popup.Refresh();
        }
    }

    [Obsolete]
    public abstract class BaseHeaderDropDown<TypeItem> : BaseHeaderPopupButton<CustomUIListBox>
    {
        public event Action<TypeItem> OnSelect;
        protected TypeItem[] Items { get; set; } = new TypeItem[0];

        public float ListWidth { get; set; }
        public float MinListWidth { get; set; }

        public void Init(TypeItem[] items)
        {
            Items = items;
        }

        protected override void WhilePopupOpening()
        {
            Popup.atlas = CommonTextures.Atlas;
            Popup.normalBgSprite = CommonTextures.FieldHovered;
            Popup.itemHeight = 20;
            Popup.itemHover = CommonTextures.FieldNormal;
            Popup.itemHighlight = CommonTextures.FieldFocused;

            Popup.textScale = 0.7f;
            Popup.color = Color.white;
            Popup.itemTextColor = Color.black;
            Popup.itemPadding = new RectOffset(5, 5, 5, 0);
            Popup.items = Items.Select(i => GetItemLabel(i)).ToArray();

            var width = GetPopupWidth();
            var height = Popup.items.Length * Popup.itemHeight + Popup.listPadding.vertical;
            Popup.size = new Vector2(width, height);

            Popup.eventSelectedIndexChanged += PopupSelectedIndexChanged;
        }
        public float GetPopupWidth()
        {
            if (ListWidth == 0f)
            {
                var autoWidth = 0f;
                var pixelRatio = PixelsToUnits();

                for (int i = 0; i < Popup.items.Length; i++)
                {
                    using UIFontRenderer uIFontRenderer = Popup.font.ObtainRenderer();
                    uIFontRenderer.wordWrap = false;
                    uIFontRenderer.pixelRatio = pixelRatio;
                    uIFontRenderer.textScale = Popup.textScale;
                    uIFontRenderer.characterSpacing = Popup.characterSpacing;
                    uIFontRenderer.multiLine = false;
                    uIFontRenderer.textAlign = UIHorizontalAlignment.Left;
                    uIFontRenderer.processMarkup = Popup.processMarkup;
                    uIFontRenderer.colorizeSprites = Popup.colorizeSprites;
                    uIFontRenderer.overrideMarkupColors = false;
                    var itemWidth = uIFontRenderer.MeasureString(Popup.items[i]);
                    if (itemWidth.x > autoWidth)
                        autoWidth = itemWidth.x;
                }

                autoWidth += Popup.listPadding.horizontal + Popup.itemPadding.horizontal;
                return Mathf.Max(autoWidth, MinListWidth);
            }
            else
                return Mathf.Max(ListWidth, MinListWidth);
        }

        private void PopupSelectedIndexChanged(UIComponent component, int index)
        {
            OnSelect?.Invoke(Items[index]);
            ClosePopup();
        }

        protected abstract string GetItemLabel(TypeItem item);
    }

    public class AdditionalHeaderButton : HeaderPopupButton<AdditionalHeaderButton.AdditionalPopup>
    {
        public class AdditionalPopup : PopupPanel
        {
            protected override Color32 Background => ComponentStyle.DarkPrimaryColor15;
        }
    }
}
