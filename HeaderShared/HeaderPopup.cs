using ColossalFramework.UI;
using ModsCommon.Utilities;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class PopupPanel : CustomUIPanel
    {
        protected virtual Color32 Background => Color.black;
        public CustomUIScrollablePanel Content { get; private set; }
        private float ContentPadding => 2f;

        private float popupWidth = 250f;
        public float PopupWidth
        {
            get => popupWidth;
            set
            {
                popupWidth = value;
                Refresh();
            }
        }
        public Vector2 MaxSize
        {
            get => Content.maximumSize;
            set => Content.maximumSize = value;
        }

        public PopupPanel()
        {
            isVisible = true;
            canFocus = true;
            isInteractive = true;
            BgColors = Background;
            Atlas = CommonTextures.Atlas;
            BackgroundSprite = CommonTextures.FieldSingle;

            AddPanel();
        }

        private void AddPanel()
        {
            Content = AddUIComponent<CustomUIScrollablePanel>();
            Content.AutoLayout = AutoLayout.Vertical;
            Content.AutoChildrenVertically = AutoLayoutChildren.Fit;
            Content.Padding = new RectOffset(0, 0, 0, 0);
            Content.clipChildren = true;
            Content.ScrollOrientation = UIOrientation.Vertical;
            Content.maximumSize = new Vector2(500, 500);
            Content.relativePosition = new Vector2(ContentPadding, ContentPadding);

            Content.Scrollbar.DefaultStyle();
            Content.ScrollbarSize = 12f;
        }
        public void Refresh()
        {
            Content.FitChildrenVertically();
            Content.width = Content.Scrollbar.isVisible ? PopupWidth - Content.Scrollbar.width : PopupWidth;
            ContentSizeChanged();
        }
        private void ContentSizeChanged(UIComponent component = null, Vector2 value = default)
        {
            if (Content != null)
            {
                size = new Vector2(PopupWidth + ContentPadding * 2, Content.height + ContentPadding * 2);

                foreach (var item in Content.components)
                    item.width = Content.width;
            }
        }
    }


}
