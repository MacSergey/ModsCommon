using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using ColossalFramework.UI;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class FieldSettingsItem<ValueType, FieldType> : ContentSettingsItem
        where FieldType : UITextField<ValueType>
    {
        protected override RectOffset ItemsPadding => new RectOffset(10, 30, 7, 7);

        public FieldType Field { get; }

        public ValueType Value
        {
            get => Field.Value;
            set => Field.Value = value;
        }

        public FieldSettingsItem()
        {
            Field = Content.AddUIComponent<FieldType>();
            Field.CustomSettingsStyle();
            Field.size = new Vector2(150f, 28f);
            Field.textScale = 1.125f;
            Field.padding = new RectOffset(0, 0, 6, 0);
            Field.builtinKeyNavigation = true;

            SetHeightBasedOn(Field);
        }
    }

    public class FloatSettingsItem : FieldSettingsItem<float, FloatUITextField> { }
    public class IntSettingsItem : FieldSettingsItem<int, IntUITextField> { }
    public class StringSettingsItem : FieldSettingsItem<string, StringUITextField> { }
}
