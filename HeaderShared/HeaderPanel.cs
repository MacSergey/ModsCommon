using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
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

    public abstract class BaseHeaderContent : CustomUIPanel
    {
        private List<IHeaderButtonInfo> Infos { get; }
        private List<IHeaderButtonInfo> MainInfos { get; set; }
        private List<IHeaderButtonInfo> AdditionalInfos { get; set; }
        private bool ShowAdditional => AdditionalInfos.Count > 0;

        private BaseAdditionallyHeaderButton Additional { get; }

        public BaseHeaderContent()
        {
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoLayoutPadding = new RectOffset(0, 0, 0, 0);

            Additional = AddUIComponent<BaseAdditionallyHeaderButton>();
            Additional.tooltip = CommonLocalize.Panel_Additional;
            Additional.SetIcon(CommonTextures.Atlas, CommonTextures.HeaderAdditional);
            Additional.PopupOpenedEvent += OnPopupOpened;
            Additional.PopupCloseEvent += OnPopupClose;

            Infos = GetInfos().ToList();

            Refresh();
        }

        protected abstract IEnumerable<IHeaderButtonInfo> GetInfos();
        private void OnPopupOpened(AdditionallyPopup popup)
        {
            foreach (var info in AdditionalInfos)
            {
                info.AddButton(popup.Content, true);
                info.ClickedEvent += PopupClicked;
            }
        }
        private void OnPopupClose(AdditionallyPopup popup)
        {
            foreach (var info in AdditionalInfos)
            {
                info.RemoveButton();
                info.ClickedEvent -= PopupClicked;
            }
        }
        private void PopupClicked(UIComponent component, UIMouseEventParameter eventParam) => Additional.ClosePopup();

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            Refresh();
        }
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
                info.AddButton(this, false);

            Additional.isVisible = ShowAdditional;
            Additional.zOrder = int.MaxValue; 
        }
    }

    public class AdditionallyPopup : PopupPanel
    {
        protected override Color32 Background => new Color32(64, 64, 64, 255);
    }
    public class BasePanelHeaderButton : HeaderButton
    {
        protected override Color32 HoveredColor => new Color32(112, 112, 112, 255);
        protected override Color32 PressedColor => new Color32(144, 144, 144, 255);
        protected override Color32 PressedIconColor => Color.white;
    }
    public class BaseAdditionallyHeaderButton : HeaderPopupButton<AdditionallyPopup>
    {
        protected override Color32 HoveredColor => new Color32(112, 112, 112, 255);
        protected override Color32 PressedColor => new Color32(144, 144, 144, 255);
        protected override Color32 PressedIconColor => Color.white;
    }
}
