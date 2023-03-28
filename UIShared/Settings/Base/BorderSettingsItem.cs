using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using ColossalFramework.UI;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class BorderSettingsItem : BaseSettingItem
    {
        private SettingsItemBorder borders = SettingsItemBorder.None;
        public SettingsItemBorder Borders
        {
            get => borders;
            set
            {
                if (!BorderEnabled)
                    value = SettingsItemBorder.None;

                if (value != borders)
                {
                    var padding = Padding;
                    borders = value;
                    Padding = padding;

                    ForegroundSprite = value switch
                    {
                        SettingsItemBorder.Top => CommonTextures.BorderTop,
                        SettingsItemBorder.Bottom => CommonTextures.BorderBottom,
                        SettingsItemBorder.Both => CommonTextures.BorderBoth,
                        _ => string.Empty
                    };
                }
            }
        }
        private bool borderEnabled = true;
        public bool BorderEnabled
        {
            get => borderEnabled;
            set
            {
                if (value != borderEnabled)
                {
                    borderEnabled = value;
                    Borders = Borders;
                }
            }
        }

        public new RectOffset Padding
        {
            get
            {
                var padding = base.Padding;
                var top = Math.Max(padding.top - ((borders & SettingsItemBorder.Top) != 0 ? 2 : 0), 0);
                var bottom = Math.Max(padding.bottom - ((borders & SettingsItemBorder.Bottom) != 0 ? 2 : 0), 0);
                return new RectOffset(padding.left, padding.right, top, bottom);
            }
            set
            {
                var top = value.top + ((borders & SettingsItemBorder.Top) != 0 ? 2 : 0);
                var bottom = value.bottom + ((borders & SettingsItemBorder.Bottom) != 0 ? 2 : 0);
                var padding = new RectOffset(value.left, value.right, top, bottom);
                base.Padding = padding;
            }
        }

        private bool canHover;
        public bool CanHover
        {
            get => canHover;
            set
            {
                canHover = value;
                BackgroundSprite = canHover ? CommonTextures.PanelBig : string.Empty;
            }
        }

        public BorderSettingsItem() : base()
        {
            Atlas = CommonTextures.Atlas;
            FgColors = ComponentStyle.SettingsColor20;
            BgColors = new ColorSet(default, ComponentStyle.SettingsColor20, default, default, default);
            CanHover = true;
            Borders = SettingsItemBorder.Both;
            SpritePadding = new RectOffset(10, 10, 0, 0);
        }
    }
}
