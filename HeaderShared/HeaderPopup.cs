using ColossalFramework.UI;
using ModsCommon.Utilities;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class PopupPanel : CustomUIPanel
    {
        protected virtual Color32 Background => Color.black;
        public CustomUIScrollablePanel Content { get; private set; }
        private float Padding => 2f;

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
            color = Background;
            atlas = CommonTextures.Atlas;
            backgroundSprite = CommonTextures.FieldSingle;

            AddPanel();
        }

        private void AddPanel()
        {
            Content = AddUIComponent<CustomUIScrollablePanel>();
            Content.autoLayout = true;
            Content.autoLayoutDirection = LayoutDirection.Vertical;
            Content.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            Content.clipChildren = true;
            Content.builtinKeyNavigation = true;
            Content.scrollWheelDirection = UIOrientation.Vertical;
            Content.maximumSize = new Vector2(500, 500);
            Content.relativePosition = new Vector2(Padding, Padding);
            this.AddScrollbar(Content);
        }
        public void Refresh()
        {
            Content.FitChildrenVertically();
            Content.width = Content.verticalScrollbar.isVisible ? PopupWidth - Content.verticalScrollbar.width : PopupWidth;
            ContentSizeChanged();
        }
        private void ContentSizeChanged(UIComponent component = null, Vector2 value = default)
        {
            if (Content != null)
            {
                size = new Vector2(PopupWidth + Padding * 2, Content.height + Padding * 2);

                foreach (var item in Content.components)
                    item.width = Content.width;
            }
        }
    }


}
