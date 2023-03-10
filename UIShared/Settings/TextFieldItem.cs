using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using ColossalFramework.UI;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class FieldSettingsItem<ValueType, FieldType> : ControlSettingsItem<FieldType>
        where FieldType : UITextField<ValueType>
    {
        protected override RectOffset ItemsPadding => new RectOffset(10, 40, 7, 7);

        public ValueType Value
        {
            get => Control.Value;
            set => Control.Value = value;
        }

        protected override void InitControl()
        {
            Control.CustomSettingsStyle();
            Control.size = new Vector2(150f, 28f);
            Control.textScale = 1.125f;
            Control.padding = new RectOffset(0, 0, 6, 0);
            Control.builtinKeyNavigation = true;
        }
    }

    public class FloatSettingsItem : FieldSettingsItem<float, FloatUITextField> { }
    public class IntSettingsItem : FieldSettingsItem<int, IntUITextField> { }
    public class StringSettingsItem : FieldSettingsItem<string, StringUITextField> { }
}
