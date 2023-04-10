using ColossalFramework.UI;
using ModsCommon.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModsCommon.UI
{
    public class BaseHeaderContent : CustomUIPanel
    {
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

        private AdditionalHeaderButton Additional { get; set; }

        private HeaderStyle headerStyle;
        public HeaderStyle HeaderStyle
        {
            get => headerStyle ?? ComponentStyle.Default.HeaderContent;
            set
            {
                if(value != headerStyle)
                {
                    headerStyle = value;

                    foreach (var info in MainInfos)
                    {
                        info.Button.BgColors = value.MainBgColors;
                        info.Button.IconColors = value.MainIconColors;
                    }

                    Additional.BgColors = value.MainBgColors;
                    Additional.IconColors = value.MainIconColors;
                }
            }
        }

        public BaseHeaderContent()
        {
            PauseLayout(() =>
            {
                autoLayout = AutoLayout.Horizontal;
                padding = new RectOffset(0, 0, 5, 5);
                autoChildrenVertically = AutoLayoutChildren.Fit;
                autoLayoutStart = LayoutStart.MiddleLeft;

                Additional = AddUIComponent<AdditionalHeaderButton>();
                Additional.name = nameof(Additional);
                Additional.tooltip = CommonLocalize.Panel_Additional;
                Additional.SetIcon(AdditionalButtonAtlas, AdditionalButtonSprite);
                Additional.SetSize(MainButtonSize, MainIconSize);

                Additional.OnPopupOpening += PopupOpening;
                Additional.OnBeforePopupClose += BeforePopupClose;
                Additional.BgColors = HeaderStyle.MainBgColors;
                Additional.IconColors = HeaderStyle.MainIconColors;
            });

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
                info.Button.PerformAutoWidth();
                info.ClickedEvent += PopupClicked;
                info.Button.BgColors = HeaderStyle.AdditionalBgColors;
                info.Button.IconColors = HeaderStyle.AdditionalIconColors;
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

        public virtual void Refresh() => PauseLayout(PlaceButtons);

        private void PlaceButtons()
        {
            MainInfos = Infos.Where(i => i.Visible && i.State == HeaderButtonState.Main).ToList();
            AdditionalInfos = Infos.Where(i => i.Visible && i.State == HeaderButtonState.Additional).ToList();

            foreach (var info in Infos)
                info.RemoveButton();

            foreach (var info in MainInfos)
            {
                info.AddButton(this, false, MainButtonSize, MainIconSize);
                info.Button.BgColors = HeaderStyle.MainBgColors;
                info.Button.IconColors = HeaderStyle.MainIconColors;
            }

            Additional.isVisible = ShowAdditional;
            Additional.zOrder = int.MaxValue;
        }   
    }
}
