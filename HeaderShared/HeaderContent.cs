using ColossalFramework.UI;
using ModsCommon.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class BaseHeaderContent : CustomUIPanel
    {
        protected abstract Color32 ButtonHoveredColor { get; }
        protected abstract Color32 ButtonPressedColor { get; }
        protected abstract Color32 AdditionalButtonHoveredColor { get; }
        protected abstract Color32 AdditionalButtonPressedColor { get; }

        protected abstract Color32 IconNormalColor { get; }
        protected abstract Color32 IconHoverColor { get; }
        protected abstract Color32 IconPressedColor { get; }
        protected abstract Color32 IconDisabledColor { get; }

        public static int DefaultSize => DefaultIconSize + 2 * DefaultIconPadding;
        public static int DefaultIconSize => 25;
        public static int DefaultIconPadding => 2;

        protected virtual int MainIconSize { get; } = DefaultIconSize;
        protected virtual int MainIconPadding { get; } = DefaultIconPadding;
        protected virtual int MainButtonSize => MainIconSize + 2 * MainIconPadding;
        protected virtual int AdditionalIconSize { get; } = DefaultIconSize;
        protected virtual int AdditionalIconPadding { get; } = DefaultIconPadding;
        protected virtual int AdditionalButtonSize => AdditionalIconSize + 2 * AdditionalIconPadding;

        protected virtual UITextureAtlas AdditionalButtonAtlas => CommonTextures.Atlas;
        protected virtual string AdditionalButtonSprite => CommonTextures.HeaderAdditionalButton;

        private List<IHeaderButtonInfo> Infos { get; } = new List<IHeaderButtonInfo>();
        private List<IHeaderButtonInfo> MainInfos { get; set; }
        private List<IHeaderButtonInfo> AdditionalInfos { get; set; }
        private bool ShowAdditional => AdditionalInfos.Count > 0;

        private AdditionalHeaderButton Additional { get; }

        public BaseHeaderContent()
        {
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoLayoutPadding = new RectOffset(0, 0, 0, 0);

            Additional = AddUIComponent<AdditionalHeaderButton>();
            Additional.tooltip = CommonLocalize.Panel_Additional;
            Additional.SetIcon(AdditionalButtonAtlas, AdditionalButtonSprite);
            Additional.SetSize(MainButtonSize, MainIconSize);

            Additional.OnPopupOpening += PopupOpening;
            Additional.OnBeforePopupClose += BeforePopupClose;
            SetButtonColors(Additional);

            Refresh();
        }

        public void AddButton(IHeaderButtonInfo info, bool refresh = false)
        {
            Infos.Add(info);
            if (refresh)
                Refresh();
        }
        private void PopupOpening(AdditionalHeaderButton.AdditionalPopup popup)
        {
            foreach (var info in AdditionalInfos)
            {
                info.AddButton(popup.Content, true, AdditionalButtonSize, AdditionalIconSize);
                info.Button.autoSize = true;
                info.Button.autoSize = false;
                info.ClickedEvent += PopupClicked;
                SetAdditionalButtonColors(info.Button);
            }

            var maxwidth = AdditionalInfos.Max(i => i.Button.width);
            popup.PopupWidth = maxwidth;
        }
        private void BeforePopupClose(AdditionalHeaderButton.AdditionalPopup popup)
        {
            foreach (var info in AdditionalInfos)
            {
                info.RemoveButton();
                info.ClickedEvent -= PopupClicked;
            }
        }
        private void PopupClicked(UIComponent component, UIMouseEventParameter eventParam) => Additional.ClosePopup();

        public virtual void Refresh()
        {
            PlaceButtons();

            autoLayout = true;
            autoLayout = false;
            FitChildrenHorizontally();

            SetSize();
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            SetSize();
        }
        protected virtual void SetSize()
        {
            foreach (var item in components)
                item.relativePosition = new Vector2(item.relativePosition.x, (height - item.height) / 2);
        }

        private void PlaceButtons()
        {
            MainInfos = Infos.Where(i => i.Visible && i.State == HeaderButtonState.Main).ToList();
            AdditionalInfos = Infos.Where(i => i.Visible && i.State == HeaderButtonState.Additional).ToList();

            foreach (var info in Infos)
                info.RemoveButton();

            foreach (var info in MainInfos)
            {
                info.AddButton(this, false, MainButtonSize, MainIconSize);
                SetButtonColors(info.Button);
            }

            Additional.isVisible = ShowAdditional;
            Additional.zOrder = int.MaxValue;
        }
        private void SetButtonColors(IHeaderButton button)
        {
            button.SetBgColor(new ColorSet(Color.white, ButtonHoveredColor, ButtonPressedColor, ButtonPressedColor, Color.white));
            button.SetFgColor(new ColorSet(IconNormalColor, IconHoverColor, IconPressedColor, IconDisabledColor, IconNormalColor));
        }
        private void SetAdditionalButtonColors(IHeaderButton button)
        {
            button.SetBgColor(new ColorSet(Color.white, AdditionalButtonHoveredColor, AdditionalButtonPressedColor, AdditionalButtonPressedColor, Color.white));
            button.SetFgColor(new ColorSet(IconNormalColor, IconHoverColor, IconPressedColor, IconDisabledColor, IconNormalColor));
        }
    }
    public class HeaderContent : BaseHeaderContent
    {
        protected override Color32 ButtonHoveredColor => new Color32(32, 32, 32, 255);
        protected override Color32 ButtonPressedColor => Color.black;
        protected override Color32 AdditionalButtonHoveredColor => new Color32(112, 112, 112, 255);
        protected override Color32 AdditionalButtonPressedColor => new Color32(112, 112, 112, 255);

        protected override Color32 IconNormalColor => Color.white;
        protected override Color32 IconHoverColor => Color.white;
        protected override Color32 IconPressedColor => new Color32(224, 224, 224, 255);
        protected override Color32 IconDisabledColor => new Color32(144, 144, 144, 255);
    }

    public class PanelHeaderContent : BaseHeaderContent
    {
        protected override Color32 ButtonHoveredColor => new Color32(112, 112, 112, 255);
        protected override Color32 ButtonPressedColor => new Color32(144, 144, 144, 255);
        protected override Color32 AdditionalButtonHoveredColor => new Color32(112, 112, 112, 255);
        protected override Color32 AdditionalButtonPressedColor => new Color32(112, 112, 112, 255);

        protected override Color32 IconNormalColor => Color.white;
        protected override Color32 IconHoverColor => Color.white;
        protected override Color32 IconPressedColor => Color.white;
        protected override Color32 IconDisabledColor => new Color32(144, 144, 144, 255);
    }
}
