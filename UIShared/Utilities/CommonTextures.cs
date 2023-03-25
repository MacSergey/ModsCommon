using ColossalFramework.UI;
using System.Collections.Generic;
using UnityEngine;

namespace ModsCommon.Utilities
{
    public static class CommonTextures
    {
        public static UITextureAtlas Atlas;

        public static string PanelSmall => nameof(PanelSmall);
        public static string PanelBig => nameof(PanelBig);
        public static string PanelLarge => nameof(PanelLarge);

        public static string BorderBottom => nameof(BorderBottom);
        public static string BorderTop => nameof(BorderTop);
        public static string BorderBoth => nameof(BorderBoth);
        public static string BorderSmall => nameof(BorderSmall);
        public static string BorderBig => nameof(BorderBig);
        public static string BorderLarge => nameof(BorderLarge);

        public static string ShadowVertical => nameof(ShadowVertical);
        public static string ShadowHorizontal => nameof(ShadowHorizontal);

        public static string FieldSingle => nameof(FieldSingle);
        public static string FieldLeft => nameof(FieldLeft);
        public static string FieldRight => nameof(FieldRight);
        public static string FieldMiddle => nameof(FieldMiddle);

        public static string FieldNormal => nameof(FieldNormal);
        public static string FieldHovered => nameof(FieldHovered);
        public static string FieldFocused => nameof(FieldFocused);
        //public static string FieldDisabled => nameof(FieldDisabled);

        //public static string FieldNormalLeft => nameof(FieldNormalLeft);
        //public static string FieldHoveredLeft => nameof(FieldHoveredLeft);
        //public static string FieldFocusedLeft => nameof(FieldFocusedLeft);
        //public static string FieldDisabledLeft => nameof(FieldDisabledLeft);

        //public static string FieldNormalRight => nameof(FieldNormalRight);
        //public static string FieldHoveredRight => nameof(FieldHoveredRight);
        //public static string FieldFocusedRight => nameof(FieldFocusedRight);
        //public static string FieldDisabledRight => nameof(FieldDisabledRight);

        //public static string FieldNormalMiddle => nameof(FieldNormalMiddle);
        //public static string FieldHoveredMiddle => nameof(FieldHoveredMiddle);
        //public static string FieldFocusedMiddle => nameof(FieldFocusedMiddle);
        //public static string FieldDisabledMiddle => nameof(FieldDisabledMiddle);

        public static string ToggleBackground { get; } = nameof(ToggleBackground);
        public static string ToggleBackgroundSmall { get; } = nameof(ToggleBackgroundSmall);
        public static string ToggleCircle { get; } = nameof(ToggleCircle);
        public static string Tab { get; } = nameof(Tab);

        public static string ArrowDown { get; } = nameof(ArrowDown);
        public static string ArrowUp { get; } = nameof(ArrowUp);
        public static string ArrowLeft { get; } = nameof(ArrowLeft);
        public static string ArrowRight { get; } = nameof(ArrowRight);

        public static string TabNormal { get; } = nameof(TabNormal);
        public static string TabHover { get; } = nameof(TabHover);
        public static string TabPressed { get; } = nameof(TabPressed);
        public static string TabFocused { get; } = nameof(TabFocused);
        public static string TabDisabled { get; } = nameof(TabDisabled);

        public static string Empty => nameof(Empty);
        public static string EmptyWithoutBorder => nameof(EmptyWithoutBorder);
        public static string Circle => nameof(Circle);

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

        public static string PlusMinusButton { get; } = nameof(PlusMinusButton);

        public static string VectorUp { get; } = nameof(VectorUp);
        public static string VectorDown { get; } = nameof(VectorDown);
        public static string VectorLeft { get; } = nameof(VectorLeft);
        public static string VectorRight { get; } = nameof(VectorRight);

        public static string Success { get; } = nameof(Success);

        public static string RadioChecked { get; } = nameof(RadioChecked);
        public static string RadioUnchecked { get; } = nameof(RadioUnchecked);

        static CommonTextures()
        {
            var spriteParams = new Dictionary<string, RectOffset>();

            spriteParams[PanelSmall] = new RectOffset(4, 4, 4, 4);
            spriteParams[PanelBig] = new RectOffset(6, 6, 6, 6);
            spriteParams[PanelLarge] = new RectOffset(10, 10, 10, 10);

            spriteParams[BorderBottom] = new RectOffset(4, 4, 4, 4);
            spriteParams[BorderTop] = new RectOffset(4, 4, 4, 4);
            spriteParams[BorderBoth] = new RectOffset(4, 4, 4, 4);
            spriteParams[BorderSmall] = new RectOffset(4, 4, 4, 4);
            spriteParams[BorderBig] = new RectOffset(6, 6, 6, 6);
            spriteParams[BorderLarge] = new RectOffset(10, 10, 10, 10);

            spriteParams[ShadowVertical] = new RectOffset();
            spriteParams[ShadowHorizontal] = new RectOffset();

            spriteParams[Success] = new RectOffset();

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
            spriteParams[FieldSingle] = new RectOffset(4, 4, 4, 4);
            spriteParams[FieldLeft] = new RectOffset(4, 4, 4, 4);
            spriteParams[FieldRight] = new RectOffset(4, 4, 4, 4);
            spriteParams[FieldMiddle] = new RectOffset(4, 4, 4, 4);

            spriteParams[FieldNormal] = new RectOffset(4, 4, 4, 4);
            spriteParams[FieldHovered] = new RectOffset(4, 4, 4, 4);
            spriteParams[FieldFocused] = new RectOffset(4, 4, 4, 4);
            //spriteParams[FieldDisabled] = new RectOffset(4, 4, 4, 4);

            //spriteParams[FieldNormalLeft] = new RectOffset(4, 4, 4, 4);
            //spriteParams[FieldHoveredLeft] = new RectOffset(4, 4, 4, 4);
            //spriteParams[FieldFocusedLeft] = new RectOffset(4, 4, 4, 4);
            //spriteParams[FieldDisabledLeft] = new RectOffset(4, 4, 4, 4);

            //spriteParams[FieldNormalRight] = new RectOffset(4, 4, 4, 4);
            //spriteParams[FieldHoveredRight] = new RectOffset(4, 4, 4, 4);
            //spriteParams[FieldFocusedRight] = new RectOffset(4, 4, 4, 4);
            //spriteParams[FieldDisabledRight] = new RectOffset(4, 4, 4, 4);

            //spriteParams[FieldNormalMiddle] = new RectOffset(4, 4, 4, 4);
            //spriteParams[FieldHoveredMiddle] = new RectOffset(4, 4, 4, 4);
            //spriteParams[FieldFocusedMiddle] = new RectOffset(4, 4, 4, 4);
            //spriteParams[FieldDisabledMiddle] = new RectOffset(4, 4, 4, 4);

            //Toggle
            spriteParams[ToggleBackground] = new RectOffset(17, 17, 0, 0);
            spriteParams[ToggleBackgroundSmall] = new RectOffset(11, 11, 0, 0);
            spriteParams[ToggleCircle] = new RectOffset();

            //Radio
            spriteParams[RadioChecked] = new RectOffset();
            spriteParams[RadioUnchecked] = new RectOffset();

            //Tab
            spriteParams[Tab] = new RectOffset(4, 4, 4, 4);
            spriteParams[TabNormal] = new RectOffset(4, 4, 4, 0);
            spriteParams[TabHover] = new RectOffset(4, 4, 4, 0);
            spriteParams[TabPressed] = new RectOffset(4, 4, 4, 0);
            spriteParams[TabFocused] = new RectOffset(4, 4, 4, 0);
            spriteParams[TabDisabled] = new RectOffset(4, 4, 4, 0);

            //Arrows
            spriteParams[ArrowDown] = new RectOffset();
            spriteParams[ArrowLeft] = new RectOffset();
            spriteParams[ArrowRight] = new RectOffset();
            spriteParams[ArrowUp] = new RectOffset();

            //Vectors
            spriteParams[VectorDown] = new RectOffset();
            spriteParams[VectorLeft] = new RectOffset();
            spriteParams[VectorRight] = new RectOffset();
            spriteParams[VectorUp] = new RectOffset();

            //OpacitySlider
            spriteParams[OpacitySliderBoard] = new RectOffset();
            spriteParams[OpacitySliderColor] = new RectOffset();

            //Header
            spriteParams[HeaderAdditionalButton] = new RectOffset();
            spriteParams[HeaderHover] = new RectOffset(4, 4, 4, 4);

            spriteParams[Empty] = new RectOffset(4, 4, 4, 4);
            spriteParams[EmptyWithoutBorder] = new RectOffset();
            spriteParams[Circle] = new RectOffset();
            spriteParams[Resize] = new RectOffset();
            spriteParams[PlusMinusButton] = new RectOffset();

            spriteParams["cs-CZ"] = new RectOffset();
            spriteParams["da-DK"] = new RectOffset();
            spriteParams["de-DE"] = new RectOffset();
            spriteParams["en-GB"] = new RectOffset();
            spriteParams["en-US"] = new RectOffset();
            spriteParams["es-ES"] = new RectOffset();
            spriteParams["fi-FI"] = new RectOffset();
            spriteParams["fr-FR"] = new RectOffset();
            spriteParams["hu-HU"] = new RectOffset();
            spriteParams["id-ID"] = new RectOffset();
            spriteParams["it-IT"] = new RectOffset();
            spriteParams["ja-JP"] = new RectOffset();
            spriteParams["ko-KR"] = new RectOffset();
            spriteParams["ms-MY"] = new RectOffset();
            spriteParams["nl-NL"] = new RectOffset();
            spriteParams["pl-PL"] = new RectOffset();
            spriteParams["pt-BR"] = new RectOffset();
            spriteParams["pt-PT"] = new RectOffset();
            spriteParams["ro-RO"] = new RectOffset();
            spriteParams["ru-RU"] = new RectOffset();
            spriteParams["tr-TR"] = new RectOffset();
            spriteParams["zh-CN"] = new RectOffset();
            spriteParams["zh-TW"] = new RectOffset();

            Atlas = TextureHelper.CreateAtlas(nameof(ModsCommon), spriteParams);
        }
    }
}
