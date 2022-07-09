﻿using ColossalFramework.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModsCommon.Utilities
{
    public static class CommonTextures
    {
        public static UITextureAtlas Atlas;

        public static string FieldNormal => nameof(FieldNormal);
        public static string FieldHovered => nameof(FieldHovered);
        public static string FieldFocused => nameof(FieldFocused);
        public static string FieldDisabled => nameof(FieldDisabled);
        public static string FieldNormalLeft => nameof(FieldNormalLeft);
        public static string FieldHoveredLeft => nameof(FieldHoveredLeft);
        public static string FieldFocusedLeft => nameof(FieldFocusedLeft);
        public static string FieldDisabledLeft => nameof(FieldDisabledLeft);
        public static string FieldNormalRight => nameof(FieldNormalRight);
        public static string FieldHoveredRight => nameof(FieldHoveredRight);
        public static string FieldFocusedRight => nameof(FieldFocusedRight);
        public static string FieldDisabledRight => nameof(FieldDisabledRight);
        public static string FieldNormalMiddle => nameof(FieldNormalMiddle);
        public static string FieldHoveredMiddle => nameof(FieldHoveredMiddle);
        public static string FieldFocusedMiddle => nameof(FieldFocusedMiddle);
        public static string FieldDisabledMiddle => nameof(FieldDisabledMiddle);

        public static string Tab { get; } = nameof(Tab);

        public static string TabNormal { get; } = nameof(TabNormal);
        public static string TabHover { get; } = nameof(TabHover);
        public static string TabPressed { get; } = nameof(TabPressed);
        public static string TabFocused { get; } = nameof(TabFocused);
        public static string TabDisabled { get; } = nameof(TabDisabled);

        public static string Empty => nameof(Empty);

        public static string OpacitySliderBoard { get; } = nameof(OpacitySliderBoard);
        public static string OpacitySliderColor { get; } = nameof(OpacitySliderColor);

        public static string ColorPickerNormal { get; } = nameof(ColorPickerNormal);
        public static string ColorPickerHovered { get; } = nameof(ColorPickerHovered);
        public static string ColorPickerDisabled { get; } = nameof(ColorPickerDisabled);
        public static string ColorPickerColor { get; } = nameof(ColorPickerColor);
        public static string ColorPickerBoard { get; } = nameof(ColorPickerBoard);

        public static string Resize { get; } = nameof(Resize);

        public static string HeaderHover { get; } = nameof(HeaderHover);

        public static string CloseButtonNormal { get; } = nameof(CloseButtonNormal);
        public static string CloseButtonHovered { get; } = nameof(CloseButtonHovered);
        public static string CloseButtonPressed { get; } = nameof(CloseButtonPressed);

        public static string HeaderAdditionalButton { get; } = nameof(HeaderAdditionalButton);

        static CommonTextures()
        {
            var spriteParams = new Dictionary<string, RectOffset>();

            //UUIButton
            spriteParams[CloseButtonNormal] = new RectOffset();
            spriteParams[CloseButtonHovered] = new RectOffset();
            spriteParams[CloseButtonPressed] = new RectOffset();

            //ColorPicker
            spriteParams[ColorPickerNormal] = new RectOffset();
            spriteParams[ColorPickerHovered] = new RectOffset();
            spriteParams[ColorPickerDisabled] = new RectOffset();
            spriteParams[ColorPickerColor] = new RectOffset();
            spriteParams[ColorPickerBoard] = new RectOffset();

            //Field
            spriteParams[FieldNormal] = new RectOffset(4, 4, 4, 4);
            spriteParams[FieldHovered] = new RectOffset(4, 4, 4, 4);
            spriteParams[FieldFocused] = new RectOffset(4, 4, 4, 4);
            spriteParams[FieldDisabled] = new RectOffset(4, 4, 4, 4);

            spriteParams[FieldNormalLeft] = new RectOffset(4, 4, 4, 4);
            spriteParams[FieldHoveredLeft] = new RectOffset(4, 4, 4, 4);
            spriteParams[FieldFocusedLeft] = new RectOffset(4, 4, 4, 4);
            spriteParams[FieldDisabledLeft] = new RectOffset(4, 4, 4, 4);

            spriteParams[FieldNormalRight] = new RectOffset(4, 4, 4, 4);
            spriteParams[FieldHoveredRight] = new RectOffset(4, 4, 4, 4);
            spriteParams[FieldFocusedRight] = new RectOffset(4, 4, 4, 4);
            spriteParams[FieldDisabledRight] = new RectOffset(4, 4, 4, 4);

            spriteParams[FieldNormalMiddle] = new RectOffset(4, 4, 4, 4);
            spriteParams[FieldHoveredMiddle] = new RectOffset(4, 4, 4, 4);
            spriteParams[FieldFocusedMiddle] = new RectOffset(4, 4, 4, 4);
            spriteParams[FieldDisabledMiddle] = new RectOffset(4, 4, 4, 4);

            //Tab
            spriteParams[Tab] = new RectOffset(4, 4, 4, 4);
            spriteParams[TabNormal] = new RectOffset(4, 4, 4, 0);
            spriteParams[TabHover] = new RectOffset(4, 4, 4, 0);
            spriteParams[TabPressed] = new RectOffset(4, 4, 4, 0);
            spriteParams[TabFocused] = new RectOffset(4, 4, 4, 0);
            spriteParams[TabDisabled] = new RectOffset(4, 4, 4, 0);

            //OpacitySlider
            spriteParams[OpacitySliderBoard] = new RectOffset();
            spriteParams[OpacitySliderColor] = new RectOffset();

            //Header
            spriteParams[HeaderAdditionalButton] = new RectOffset();
            spriteParams[HeaderHover] = new RectOffset(4, 4, 4, 4);

            spriteParams[Empty] = new RectOffset();
            spriteParams[Resize] = new RectOffset();

            Atlas = TextureHelper.CreateAtlas(nameof(ModsCommon), spriteParams);
        }
    }
}
