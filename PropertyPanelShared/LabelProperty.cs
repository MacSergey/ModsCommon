﻿using ColossalFramework.UI;
using ModsCommon.Utilities;
using UnityEngine;

namespace ModsCommon.UI
{
    public class LabelProperty : BaseEditorPanel, IReusable
    {
        bool IReusable.InCache { get; set; }
        private CustomUILabel Label { get; set; }
        protected virtual Color32 DefaultColor { get; } = Color.white;
        protected virtual float DefaultTextScale { get; } = 0.7f;

        public string Text
        {
            get => Label.text;
            set => Label.text = value;
        }
        public Color32 BackgroundColor
        {
            get => Label.color;
            set => Label.color = value;
        }
        public Color32 TextColor
        {
            get => Label.textColor;
            set => Label.textColor = value;
        }
        public float TextScale
        {
            get => Label.textScale;
            set => Label.textScale = value;
        }

        public override bool EnableControl
        {
            get => Label.isEnabled;
            set => Label.isEnabled = value;
        }

        public LabelProperty() : base()
        {
            PauseLayout(() =>
            {
                autoLayout = AutoLayout.Vertical;
                autoFitChildrenVertically = true;

                Label = AddUIComponent<CustomUILabel>();
                Label.atlas = CommonTextures.Atlas;
                Label.backgroundSprite = CommonTextures.PanelBig;
                Label.color = DefaultColor;
                Label.textScale = DefaultTextScale;
                Label.autoSize = false;
                Label.autoHeight = true;
                Label.wordWrap = true;
                Label.padding = new RectOffset(5, 5, 5, 5);
            });
        }
        public override void DeInit()
        {
            base.DeInit();

            PauseLayout(() =>
            {
                Label.color = DefaultColor;
                Label.textScale = DefaultTextScale;
            }, false);
        }
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            Label.width = width - Padding.horizontal;
        }
    }

    public class ErrorTextProperty : LabelProperty
    {
        protected override Color32 DefaultColor => ComponentStyle.ErrorFocusedColor;
    }
    public class WarningTextProperty : LabelProperty
    {
        protected override Color32 DefaultColor => ComponentStyle.WarningColor;
    }
}
