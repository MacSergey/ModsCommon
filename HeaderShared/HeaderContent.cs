using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
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
            Additional.SetIcon(CommonTextures.Atlas, CommonTextures.HeaderAdditional);
            Additional.PopupOpenedEvent += OnPopupOpened;
            Additional.PopupCloseEvent += OnPopupClose;
            SetButtonColors(Additional);

            Refresh();
        }

        public void AddButton(IHeaderButtonInfo info, bool refresh = false)
        {
            Infos.Add(info);
            if (refresh)
                Refresh();
        }
        private void OnPopupOpened(AdditionalHeaderButton.AdditionalPopup popup)
        {
            foreach (var info in AdditionalInfos)
            {
                info.AddButton(popup.Content, true);
                info.Button.autoSize = true;
                info.Button.autoSize = false;
                info.ClickedEvent += PopupClicked;
                SetAdditionalButtonColors(info.Button);
            }

            var maxwidth = AdditionalInfos.Max(i => i.Button.width);
            popup.Width = maxwidth;
        }
        private void OnPopupClose(AdditionalHeaderButton.AdditionalPopup popup)
        {
            foreach (var info in AdditionalInfos)
            {
                info.RemoveButton();
                info.ClickedEvent -= PopupClicked;
            }
        }
        private void PopupClicked(UIComponent component, UIMouseEventParameter eventParam) => Additional.ClosePopup();

        //Seem it leads to stack overflow and not needed.
        //protected override void OnSizeChanged()
        //{
        //    base.OnSizeChanged();
        //    Refresh();
        //}
        public virtual void Refresh()
        {
            PlaceButtons();

            autoLayout = true;
            autoLayout = false;
            FitChildrenHorizontally();

            foreach (var item in components)
                item.relativePosition = new Vector2(item.relativePosition.x, (height - item.height) / 2);
        }

        private void PlaceButtons()
        {
            //var visibleButtons = Infos.Where(i => i.Visible).ToArray();

            //var mainCount = visibleButtons.Count(i => i.State == HeaderButtonState.Main);
            //var additionalCount = visibleButtons.Count(i => i.State == HeaderButtonState.Additional);
            //var autoCount = visibleButtons.Count(i => i.State == HeaderButtonState.Auto);
            //var maxButtons = maximumSize.x > 0 ? ((int)maximumSize.x / HeaderButton.IconSize) : int.MaxValue;

            //var needAdditional = additionalCount > 0 || (autoCount > 0 && (mainCount + autoCount) > maxButtons);

            MainInfos = Infos.Where(i => i.Visible && i.State == HeaderButtonState.Main).ToList();
            AdditionalInfos = Infos.Where(i => i.Visible && i.State == HeaderButtonState.Additional).ToList();

            foreach (var info in Infos)
                info.RemoveButton();

            foreach (var info in MainInfos)
            {
                info.AddButton(this, false);
                SetButtonColors(info.Button);
            }

            Additional.isVisible = ShowAdditional;
            Additional.zOrder = int.MaxValue;
        }
        private void SetButtonColors(HeaderButton button)
        {
            button.hoveredColor = ButtonHoveredColor;
            button.pressedColor = ButtonPressedColor;
            button.focusedColor = ButtonPressedColor;

            button.Icon.color = IconNormalColor;
            button.Icon.hoveredColor = IconHoverColor;
            button.Icon.pressedColor = IconPressedColor;
            button.Icon.disabledColor = IconDisabledColor;
        }
        private void SetAdditionalButtonColors(HeaderButton button)
        {
            button.hoveredColor = AdditionalButtonHoveredColor;
            button.pressedColor = AdditionalButtonPressedColor;
            button.focusedColor = AdditionalButtonPressedColor;

            button.Icon.color = IconNormalColor;
            button.Icon.hoveredColor = IconHoverColor;
            button.Icon.pressedColor = IconPressedColor;
            button.Icon.disabledColor = IconDisabledColor;
        }
    }
    public class HeaderContent : BaseHeaderContent
    {
        protected override Color32 ButtonHoveredColor => new Color32(32, 32, 32, 255);
        protected override Color32 ButtonPressedColor => Color.black;
        protected override Color32 AdditionalButtonHoveredColor => new Color32(32, 32, 32, 255);
        protected override Color32 AdditionalButtonPressedColor => Color.black;

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
        protected override Color32 AdditionalButtonPressedColor => new Color32(144, 144, 144, 255);

        protected override Color32 IconNormalColor => Color.white;
        protected override Color32 IconHoverColor => Color.white;
        protected override Color32 IconPressedColor => Color.white;
        protected override Color32 IconDisabledColor => new Color32(144, 144, 144, 255);
    }
}
