﻿using ColossalFramework.UI;
using ModsCommon.Utilities;
using UnityEngine;

namespace ModsCommon.UI
{
    public class TextProperty : EditorItem, IReusable
    {
        bool IReusable.InCache { get; set; }
        private CustomUIPanel Panel { get; set; }
        private CustomUILabel Label { get; set; }
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
            Panel = AddUIComponent<CustomUIPanel>();
            Panel.relativePosition = new Vector2(ItemsPadding, ItemsPadding);
            Panel.atlas = TextureHelper.InGameAtlas;
            Panel.backgroundSprite = "ButtonWhite";
            Panel.color = Color;
            Panel.autoLayout = true;
            Panel.autoFitChildrenVertically = true;
            Panel.eventSizeChanged += PanelSizeChanged;

            Label = Panel.AddUIComponent<CustomUILabel>();
            Label.textScale = 0.7f;
            Label.autoSize = false;
            Label.autoHeight = true;
            Label.wordWrap = true;
            Label.padding = new RectOffset(5, 5, 5, 5);
        }

        private void PanelSizeChanged(UIComponent component, Vector2 value)
        {
            SetHeight();
            Label.width = Panel.width;
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            if (Panel != null)
            {
                Panel.width = width - ItemsPadding * 2;
                SetHeight();
            }
        }
        private void SetHeight() => height = Panel.height + 2 * ItemsPadding;
    }

    public class ErrorTextProperty : TextProperty
    {
        protected override Color32 Color => Colors.Error;
    }
    public class WarningTextProperty : TextProperty
    {
        protected override Color32 Color => Colors.Warning;
    }
}
