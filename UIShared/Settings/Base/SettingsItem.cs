using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using ColossalFramework.UI;
using UnityEngine;

namespace ModsCommon.UI
{
    [Flags]
    public enum SettingsItemBorder
    {
        None = 0,
        Top = 1,
        Bottom = 2,
        Both = Top | Bottom,
    }

    public abstract class BaseSettingItem : CustomUIPanel { }
    public class EmptySpaceSettingsItem : BaseSettingItem
    {
        public EmptySpaceSettingsItem() : base()
        {
            AutoLayout = AutoLayout.Disabled;
            height = 15f;
        }
    }

}
