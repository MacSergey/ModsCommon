using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using ColossalFramework.UI;
using UnityEngine;
using System.Linq;

namespace ModsCommon.UI
{
    public class SettingsItemSection : CustomUIPanel
    {
        private bool customSection;
        public bool CustomSection
        {
            get => customSection;
            set
            {
                if(value != customSection)
                {
                    customSection = value;
                    if (!value)
                        SetBorders();
                }
            }
        }
        public SettingsItemSection()
        {
            name = "Content";
            AutoLayout = AutoLayout.Vertical;
            AutoChildrenVertically = AutoLayoutChildren.Fit;
            autoChildrenHorizontally = AutoLayoutChildren.Fill;
            AutoLayoutSpace = -2;
            Padding = new RectOffset(30, 30, 0, 0);
        }
        protected override void ChildChanged(UIComponent child)
        {
            PauseLayout(SetBorders, false);
            base.ChildChanged(child);
        }
        private void SetBorders()
        {
            if (!CustomSection)
            {
                var items = components.OfType<BorderSettingsItem>().Where(i => i.isVisible).ToArray();

                for (var i = 0; i < items.Length; i += 1)
                {
                    if (i == 0 && i == items.Length - 1)
                        items[i].Borders = SettingsItemBorder.None;
                    else if (i == 0)
                        items[i].Borders = SettingsItemBorder.Bottom;
                    else if (i == items.Length - 1)
                        items[i].Borders = SettingsItemBorder.Top;
                    else
                        items[i].Borders = SettingsItemBorder.Both;
                }
            }
        }
    }
}
