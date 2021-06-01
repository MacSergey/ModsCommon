using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class HeaderButton : CustomUIButton
    {
        public static int Size => IconSize + 2 * IconPadding;
        public static int IconSize => 25;
        public static int IconPadding => 2;
        public CustomUIButton Icon { get; }
        protected virtual Color32 HoveredColor => new Color32(32, 32, 32, 255);
        protected virtual Color32 PressedColor => Color.black;

        protected virtual Color32 IconColor => Color.white;
        protected virtual Color32 HoverIconColor => Color.white;
        protected virtual Color32 PressedIconColor => new Color32(224, 224, 224, 255);
        protected virtual Color32 DisabledIconColor => new Color32(144, 144, 144, 255);

        protected abstract UITextureAtlas IconAtlas { get; }

        public HeaderButton()
        {
            hoveredBgSprite = pressedBgSprite = focusedBgSprite = CommonTextures.HeaderHoverSprite;
            size = new Vector2(Size, Size);
            atlas = CommonTextures.Atlas;
            hoveredColor = HoveredColor;
            pressedColor = focusedColor = PressedColor;
            clipChildren = true;
            textPadding = new RectOffset(IconSize + 5, 5, 5, 0);
            textScale = 0.8f;
            textHorizontalAlignment = UIHorizontalAlignment.Left;
            minimumSize = size;

            Icon = AddUIComponent<CustomUIButton>();
            Icon.size = new Vector2(IconSize, IconSize);
            Icon.atlas = IconAtlas;
            Icon.relativePosition = new Vector2(IconPadding, IconPadding);

            Icon.color = IconColor;
            Icon.hoveredColor = HoverIconColor;
            Icon.pressedColor = PressedIconColor;
            Icon.disabledColor = DisabledIconColor;
        }

        public void SetIconSprite(string sprite)
        {
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
    }

    public abstract class BaseHeaderPopupButton<PopupType> : HeaderButton
        where PopupType : UIComponent
    {
        public event Action<PopupType> OpenPopupEvent;
        public PopupType Popup { get; private set; }

        protected override void OnClick(UIMouseEventParameter p)
        {
            if (Popup == null)
                OpenPopup();
            else
                ClosePopup();
        }
        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();
            if (!isVisible)
                ClosePopup();
        }

        protected void OpenPopup()
        {
            var root = GetRootContainer();
            Popup = root.AddUIComponent<PopupType>();
            Popup.eventLostFocus += OnPopupLostFocus;
            Popup.eventKeyDown += OnPopupKeyDown;
            Popup.Focus();

            OnOpenPopup();

            SetPopupPosition();
            Popup.parent.eventPositionChanged += SetPopupPosition;
        }

        protected virtual void OnOpenPopup() => OpenPopupEvent?.Invoke(Popup);
        public virtual void ClosePopup()
        {
            if (Popup != null)
            {
                Popup.eventLostFocus -= OnPopupLostFocus;
                Popup.eventKeyDown -= OnPopupKeyDown;

                foreach (var items in Popup.components.ToArray())
                    ComponentPool.Free(items);

                ComponentPool.Free(Popup);
                Popup = null;
            }
        }
        private void OnPopupLostFocus(UIComponent component, UIFocusEventParameter eventParam)
        {
            var uiView = Popup.GetUIView();
            var mouse = uiView.ScreenPointToGUI(Input.mousePosition / uiView.inputScale);
            var popupRect = new Rect(Popup.absolutePosition, Popup.size);
            var buttonRect = new Rect(absolutePosition, size);
            if (!popupRect.Contains(mouse) && !buttonRect.Contains(mouse))
                ClosePopup();
            else
                Popup.Focus();
        }
        private void OnPopupKeyDown(UIComponent component, UIKeyEventParameter p)
        {
            if (p.keycode == KeyCode.Escape)
            {
                ClosePopup();
                p.Use();
            }
        }

        private void SetPopupPosition(UIComponent component = null, Vector2 value = default)
        {
            if (Popup != null)
            {
                UIView uiView = Popup.GetUIView();
                var screen = uiView.GetScreenResolution();
                var position = absolutePosition + new Vector3(0, height);
                position.x = MathPos(position.x, Popup.width, screen.x);
                position.y = MathPos(position.y, Popup.height, screen.y);

                Popup.relativePosition = position - Popup.parent.absolutePosition;
            }

            static float MathPos(float pos, float size, float screen) => pos + size > screen ? (screen - size < 0 ? 0 : screen - size) : Mathf.Max(pos, 0);
        }
    }
    public abstract class HeaderPopupButton<PopupType> : BaseHeaderPopupButton<PopupType>
        where PopupType : PopupPanel
    {
        protected override void OnOpenPopup()
        {
            base.OnOpenPopup();
            Popup.Init();
        }
    }
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

        protected override void OnOpenPopup()
        {
            base.OnOpenPopup();

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
}
