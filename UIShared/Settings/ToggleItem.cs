using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using ColossalFramework.UI;
using UnityEngine;

namespace ModsCommon.UI
{
    public class ToggleSettingsItem : ContentSettingsItem
    {
        public CustomUIToggle Toggle { get; }

        public bool State
        {
            get => Toggle.State;
            set => Toggle.State = value;
        }

        public ToggleSettingsItem()
        {
            Toggle = Content.AddUIComponent<CustomUIToggle>();

            Toggle.OnColor = new Color32(124, 144, 53, 255);
            Toggle.OnHoverColor = new Color32(112, 130, 48, 255);
            Toggle.OnPressedColor = new Color32(101, 117, 43, 255);

            Toggle.OffColor = new Color32(148, 161, 166, 255);
            Toggle.OffHoverColor = new Color32(136, 148, 153, 255);
            Toggle.OffPressedColor = new Color32(125, 136, 140, 255);

            Toggle.ShowMark = true;

            SetHeightBasedOn(Toggle);
        }
    }
}
