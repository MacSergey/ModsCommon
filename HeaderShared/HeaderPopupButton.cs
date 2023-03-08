using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Linq;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class BaseHeaderPopupButton<PopupType> : HeaderButton
        where PopupType : UIComponent
    {
        public event Action PopupOpenEvent;
        public event Action<PopupType> PopupOpenedEvent;
        public event Action<PopupType> PopupCloseEvent;
        public event Action PopupClosedEvent;
        public PopupType Popup { get; private set; }

        public override void Update()
        {
            base.Update();
            CheckPopup();
        }

        protected override void OnClick(UIMouseEventParameter p)
        {
            if (Popup == null)
                OpenPopup();
            else
                ClosePopup();
        }

        protected void OpenPopup()
        {
            OnPopupOpen();

            var root = GetRootContainer();
            Popup = root.AddUIComponent<PopupType>();
            Popup.eventLeaveFocus += OnPopupLeaveFocus;
            Popup.eventKeyDown += OnPopupKeyDown;
            Popup.Focus();

            OnPopupOpened();

            SetPopupPosition();
            Popup.parent.eventPositionChanged += SetPopupPosition;
        }
        public virtual void ClosePopup()
        {
            if (Popup != null)
            {
                OnPopupClose();

                Popup.eventLeaveFocus -= OnPopupLeaveFocus;
                Popup.eventKeyDown -= OnPopupKeyDown;

                foreach (var items in Popup.components.ToArray())
                    ComponentPool.Free(items);

                ComponentPool.Free(Popup);
                Popup = null;

                OnPopupClosed();
            }
        }
        private void CheckPopup()
        {
            if (Popup == null)
                return;

            if (!Popup.containsFocus && !containsFocus)
            {
                ClosePopup();
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                var ray = GetCamera().ScreenPointToRay(Input.mousePosition);
                if (!Popup.Raycast(ray) && !Raycast(ray))
                {
                    ClosePopup();
                    return;
                }
            }
        }

        protected virtual void OnPopupOpen() => PopupOpenEvent?.Invoke();
        protected virtual void OnPopupOpened() => PopupOpenedEvent?.Invoke(Popup);
        protected virtual void OnPopupClose() => PopupCloseEvent?.Invoke(Popup);
        protected virtual void OnPopupClosed() => PopupClosedEvent?.Invoke();

        private void OnPopupLeaveFocus(UIComponent component, UIFocusEventParameter eventParam) => CheckPopup();

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
        protected override void OnPopupOpened()
        {
            base.OnPopupOpened();
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

        protected override void OnPopupOpened()
        {
            base.OnPopupOpened();

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
            protected override Color32 Background => new Color32(64, 64, 64, 255);
        }
    }
}
