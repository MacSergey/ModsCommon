using ColossalFramework.UI;
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

        public static string EmptySprite => nameof(EmptySprite);

        public static string OpacitySliderBoard { get; } = nameof(OpacitySliderBoard);
        public static string OpacitySliderColor { get; } = nameof(OpacitySliderColor);

        public static string ColorPickerNormal { get; } = nameof(ColorPickerNormal);
        public static string ColorPickerHover { get; } = nameof(ColorPickerHover);
        public static string ColorPickerDisable { get; } = nameof(ColorPickerDisable);
        public static string ColorPickerColor { get; } = nameof(ColorPickerColor);
        public static string ColorPickerBoard { get; } = nameof(ColorPickerBoard);

        public static string ResizeSprite { get; } = nameof(ResizeSprite);

        public static string HeaderHoverSprite { get; } = nameof(HeaderHoverSprite);

        public static string DeleteNormal { get; } = nameof(DeleteNormal);
        public static string DeleteHover { get; } = nameof(DeleteHover);
        public static string DeletePressed { get; } = nameof(DeletePressed);

        private static Dictionary<string, TextureHelper.SpriteParamsGetter> Files { get; } = new Dictionary<string, TextureHelper.SpriteParamsGetter>
        {
            {nameof(CloseButton), CloseButton},
            {nameof(ColorPicker), ColorPicker},
            {nameof(DefaultTabButtons), DefaultTabButtons},
            {nameof(Empty), Empty},
            {nameof(HeaderHover), HeaderHover},
            {nameof(OpacitySlider), OpacitySlider},
            {nameof(Resize), Resize},
            {nameof(TabButton), TabButton},
            {nameof(TextFieldPanel), TextFieldPanel},
        };

        static CommonTextures()
        {
            Atlas = TextureHelper.CreateAtlas(nameof(ModsCommon), Files);
        }

        private static UITextureAtlas.SpriteInfo[] TextFieldPanel(int texWidth, int texHeight, Rect rect)
           => TextureHelper.GetSpritesRowsInfo(texWidth, texHeight, rect, 32, 32, new RectOffset(4, 4, 4, 4), 2, 4,
       FieldNormal, FieldHovered, FieldFocused, FieldDisabled,
       FieldNormalLeft, FieldHoveredLeft, FieldFocusedLeft, FieldDisabledLeft,
       FieldNormalRight, FieldHoveredRight, FieldFocusedRight, FieldDisabledRight,
       FieldNormalMiddle, FieldHoveredMiddle, FieldFocusedMiddle, FieldDisabledMiddle).ToArray();
        private static UITextureAtlas.SpriteInfo[] TabButton(int texWidth, int texHeight, Rect rect) => TextureHelper.GetSpritesInfo(texWidth, texHeight, rect, new RectOffset(4, 4, 4, 0), 1, Tab).ToArray();

        private static UITextureAtlas.SpriteInfo[] DefaultTabButtons(int texWidth, int texHeight, Rect rect) => TextureHelper.GetSpritesInfo(texWidth, texHeight, rect, 58, 25, new RectOffset(4, 4, 4, 0), 2, TabNormal, TabHover, TabPressed, TabFocused, TabDisabled).ToArray();

        private static UITextureAtlas.SpriteInfo[] Empty(int texWidth, int texHeight, Rect rect) => TextureHelper.GetSpritesInfo(texWidth, texHeight, rect, 28, 28, new RectOffset(2, 2, 2, 2), 2, EmptySprite).ToArray();

        private static UITextureAtlas.SpriteInfo[] OpacitySlider(int texWidth, int texHeight, Rect rect) => TextureHelper.GetSpritesInfo(texWidth, texHeight, rect, 18, 200, new RectOffset(), 2, OpacitySliderBoard, OpacitySliderColor).ToArray();

        private static UITextureAtlas.SpriteInfo[] ColorPicker(int texWidth, int texHeight, Rect rect) => TextureHelper.GetSpritesInfo(texWidth, texHeight, rect, 43, 49, ColorPickerNormal, ColorPickerHover, ColorPickerDisable, ColorPickerColor, ColorPickerBoard).ToArray();

        private static UITextureAtlas.SpriteInfo[] Resize(int texWidth, int texHeight, Rect rect) => TextureHelper.GetSpritesInfo(texWidth, texHeight, rect, ResizeSprite).ToArray();
        private static UITextureAtlas.SpriteInfo[] HeaderHover(int texWidth, int texHeight, Rect rect) => TextureHelper.GetSpritesInfo(texWidth, texHeight, rect, new RectOffset(4, 4, 4, 4), 0, HeaderHoverSprite).ToArray();
        private static UITextureAtlas.SpriteInfo[] CloseButton(int texWidth, int texHeight, Rect rect) => TextureHelper.GetSpritesInfo(texWidth, texHeight, rect, 32, 32, DeleteNormal, DeleteHover, DeletePressed).ToArray();
    }
}
