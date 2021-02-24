using ColossalFramework.UI;
using ModsCommon.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModsCommon.UI
{
    public class TextProperty : EditorItem, IReusable
    {
        protected static Color32 ErrorColor { get; } = new Color32(253, 77, 60, 255);
        protected static Color32 WarningColor { get; } = new Color32(253, 150, 62, 255);

        private UIPanel Panel { get; set; }
        private UILabel Label { get; set; }
        protected virtual Color32 Color { get; } = UnityEngine.Color.white;

        public string Text
        {
            get => Label.text;
            set => Label.text = value;
        }
        public override bool EnableControl
        {
            get => Label.isEnabled;
            set => Label.isEnabled = value;
        }

        public TextProperty()
        {
            autoLayout = true;
            autoLayoutPadding = new RectOffset(ItemsPadding, ItemsPadding, 0, 0);
            autoFitChildrenVertically = true;

            Panel = AddUIComponent<UIPanel>();
            Panel.atlas = TextureHelper.InGameAtlas;
            Panel.backgroundSprite = "ButtonWhite";
            Panel.color = Color;
            Panel.autoLayout = true;
            Panel.autoFitChildrenVertically = true;
            Panel.eventSizeChanged += PanelSizeChanged;

            Label = Panel.AddUIComponent<UILabel>();
            Label.textScale = 0.7f;
            Label.autoSize = false;
            Label.autoHeight = true;
            Label.wordWrap = true;
            Label.padding = new RectOffset(5, 5, 5, 5);
        }

        private void PanelSizeChanged(UIComponent component, Vector2 value) => Label.width = Panel.width;

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            if (Panel != null)
                Panel.width = width - ItemsPadding * 2;
        }
    }

    public class ErrorTextProperty : TextProperty
    {
        protected override Color32 Color => ErrorColor;
    }
    public class WarningTextProperty : TextProperty
    {
        protected override Color32 Color => WarningColor;
    }
}
