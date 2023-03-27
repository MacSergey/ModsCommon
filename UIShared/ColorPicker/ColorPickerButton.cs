using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class ColorPickerButton<PopupType> : BaseDropDown<PopupType>
        where PopupType: ColorPickerPopup
    {
        public event Action<Color32> OnSelectedColorChanged;
    }
    public class DefaultColorPickerButton : ColorPickerButton<ColorPickerPopup> { }
}
