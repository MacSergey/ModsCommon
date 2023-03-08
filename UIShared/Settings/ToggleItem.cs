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

        public ToggleSettingsItem() : base()
        {
            Toggle = Content.AddUIComponent<CustomUIToggle>();
            Toggle.name = nameof(Toggle);
            Toggle.CustomSettingsStyle();

            SetHeightBasedOn(Toggle);
        }
    }
}
