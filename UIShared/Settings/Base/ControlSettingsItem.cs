using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using ColossalFramework.UI;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class ControlSettingsItem<ControlType> : LabelSettingsItem
        where ControlType : UIComponent
    {
        public ControlType Control { get; private set; }

        protected sealed override void InitContent()
        {
            base.InitContent();

            Control = Content.AddUIComponent<ControlType>();
            Control.eventSizeChanged += (_, _) => RefreshItems();

            InitControl();
        }
        protected virtual void InitControl() { }

        protected override void RefreshItems()
        {
            LabelItem.minimumSize = new Vector2(0, Control.height);
            LabelItem.width = Content.width - Control.width;
        }
    }
}
