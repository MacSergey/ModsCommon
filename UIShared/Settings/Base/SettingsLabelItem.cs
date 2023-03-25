using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using ColossalFramework.UI;
using UnityEngine;

namespace ModsCommon.UI
{
    public class LabelSettingsItem : ContentSettingsItem
    {
        public CustomUILabel LabelItem { get; private set; }

        public string Label
        {
            get => LabelItem.text;
            set => LabelItem.text = value;
        }

        protected override void InitContent()
        {
            LabelItem = Content.AddUIComponent<CustomUILabel>();
            LabelItem.name = nameof(Label);
            LabelItem.autoSize = false;
            LabelItem.autoHeight = true;
            LabelItem.wordWrap = true;
            LabelItem.verticalAlignment = UIVerticalAlignment.Middle;
            LabelItem.padding = new RectOffset(0, 0, 2, 0);
        }
        protected override void RefreshItems()
        {
            LabelItem.width = Content.width;
        }
    }
}
