using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class ColorPickerButton<PopupType> : BaseDropDown<PopupType>
        where PopupType : ColorPickerPopup
    {
        public event Action<Color32> OnSelectedColorChanged;

        private Color32 selectedColor;
        public Color32 SelectedColor
        {
            get => selectedColor;
            set
            {
                selectedColor = value;
                var sample = value;
                sample.a = 255;
                FgColors = sample;
                if (Popup != null)
                    Popup.SelectedColor = value;
            }
        }
        private ColorPickerStyle Style { get; set; }

        public ColorPickerButton() : base()
        {
            SpritePadding = new RectOffset(2, 2, 2, 2);
        }
        protected override void AfterPopupOpen()
        {
            base.AfterPopupOpen();
            Popup.SelectedColor = SelectedColor;
            Popup.OnSelectedColorChanged += PickerChanged;
            Popup.ColorPickerStyle = Style;
        }
        private void PickerChanged(Color32 color)
        {
            SelectedColor = color;
            OnSelectedColorChanged?.Invoke(color);
        }
        public ColorPickerStyle ColorPickerStyle
        {
            set
            {
                Style = value;

                BgAtlas = value.BgAtlas;
                FgAtlas = value.FgAtlas;

                BgSprites = value.BgSprites;
                BgColors = value.BgColors;

                FgSprites = value.FgSprites;
            }
        }
    }
    public class DefaultColorPickerButton : ColorPickerButton<ColorPickerPopup> { }
}
