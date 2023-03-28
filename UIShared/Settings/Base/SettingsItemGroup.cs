using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using ColossalFramework.UI;
using UnityEngine;

namespace ModsCommon.UI
{
    public class SettingsItemGroup : BorderSettingsItem
    {
        public SettingsItemGroup() : base()
        {
            AutoLayout = AutoLayout.Vertical;
            autoChildrenVertically = AutoLayoutChildren.Fit;
            autoChildrenHorizontally = AutoLayoutChildren.Fill;

            CanHover = true;
        }
        protected override void OnComponentAdded(UIComponent child)
        {
            base.OnComponentAdded(child);

            if (child is BorderSettingsItem item)
            {
                item.Borders = SettingsItemBorder.None;
                item.CanHover = false;
            }
        }
    }
}
