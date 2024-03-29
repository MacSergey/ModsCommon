﻿using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using ColossalFramework.UI;
using UnityEngine;

namespace ModsCommon.UI
{
    public class ToggleSettingsItem : ControlSettingsItem<CustomUIToggle>
    {
        public bool State
        {
            get => Control.Value;
            set => Control.Value = value;
        }

        protected override void InitControl()
        {
            Control.SettingsStyle();
        }
    }
}
