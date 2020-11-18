using ColossalFramework.Importers;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ModsCommon.Utilities
{
    public static class TextureHelper
    {
        public static UITextureAtlas InGameAtlas { get; } = GetAtlas("Ingame");

        public static UITextureAtlas CommonAtlas;

        static Dictionary<string, Action<int, int, Rect>> Files { get; } = new Dictionary<string, Action<int, int, Rect>>
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

        static TextureHelper()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var textures = Files.Select(f => assembly.LoadTextureFromAssembly(f.Key)).ToArray();
            CommonAtlas = CreateAtlas(textures, nameof(CommonAtlas), out Rect[] rects);
            var actions = Files.Values.ToArray();

            for (var i = 0; i < actions.Length; i += 1)
                actions[i](textures[i].width, textures[i].height, rects[i]);
        }

        static UITextureAtlas GetAtlas(string name)
        {
            UITextureAtlas[] atlases = Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) as UITextureAtlas[];
            for (int i = 0; i < atlases.Length; i++)
            {
                if (atlases[i].name == name)
                    return atlases[i];
            }
            return UIView.GetAView().defaultAtlas;
        }
        public static UITextureAtlas CreateAtlas(Texture2D[] textures, string name, out Rect[] rects)
        {
            var atlas = ScriptableObject.CreateInstance<UITextureAtlas>();
            atlas.material = UnityEngine.Object.Instantiate(UIView.GetAView().defaultAtlas.material);
            atlas.material.mainTexture = CreateTexture(1, 1, Color.white);
            atlas.name = name;

            rects = atlas.texture.PackTextures(textures, atlas.padding, 4096, false);

            return atlas;
        }

        public static Texture2D LoadTextureFromAssembly(this Assembly assembly, string textureFile)
        {
            var search = $".{textureFile}.";
            var path = assembly.GetManifestResourceNames().FirstOrDefault(n => n.Contains(search));
            var manifestResourceStream = assembly.GetManifestResourceStream(path);
            var data = new byte[manifestResourceStream.Length];
            manifestResourceStream.Read(data, 0, data.Length);

            var texture = new Image(data).CreateTexture();
            return texture;
        }

        public static Texture2D CreateTexture(int height, int width, Color color)
        {
            var texture = new Texture2D(height, width) { name = "Markup" };
            for (var i = 0; i < width; i += 1)
            {
                for (var j = 0; j < height; j += 1)
                    texture.SetPixel(i, j, color);
            }
            texture.Apply();
            return texture;
        }

        static void TextFieldPanel(int texWidth, int texHeight, Rect rect)
            => CommonAtlas.AddSpritesRows(texWidth, texHeight, rect, 32, 32, new RectOffset(4, 4, 4, 4), 2, 4,
        FieldNormal, FieldHovered, FieldFocused, FieldDisabled,
        FieldNormalLeft, FieldHoveredLeft, FieldFocusedLeft, FieldDisabledLeft,
        FieldNormalRight, FieldHoveredRight, FieldFocusedRight, FieldDisabledRight,
        FieldNormalMiddle, FieldHoveredMiddle, FieldFocusedMiddle, FieldDisabledMiddle);
        static void TabButton(int texWidth, int texHeight, Rect rect) => CommonAtlas.AddSprites(texWidth, texHeight, rect, new RectOffset(4, 4, 4, 0), 1, Tab);

        static void DefaultTabButtons(int texWidth, int texHeight, Rect rect)
            => CommonAtlas.AddSprites(texWidth, texHeight, rect, 58, 25, new RectOffset(4, 4, 4, 0), 2, TabNormal, TabHover, TabPressed, TabFocused, TabDisabled);

        static void Empty(int texWidth, int texHeight, Rect rect)
            => CommonAtlas.AddSprites(texWidth, texHeight, rect, 32, 32, EmptySprite);

        static void OpacitySlider(int texWidth, int texHeight, Rect rect) => CommonAtlas.AddSprites(texWidth, texHeight, rect, 18, 200, new RectOffset(), 2, OpacitySliderBoard, OpacitySliderColor);

        static void ColorPicker(int texWidth, int texHeight, Rect rect)
            => CommonAtlas.AddSprites(texWidth, texHeight, rect, 43, 49, ColorPickerNormal, ColorPickerHover, ColorPickerDisable, ColorPickerColor, ColorPickerBoard);

        static void Resize(int texWidth, int texHeight, Rect rect) => CommonAtlas.AddSprites(texWidth, texHeight, rect, ResizeSprite);
        static void HeaderHover(int texWidth, int texHeight, Rect rect) => CommonAtlas.AddSprites(texWidth, texHeight, rect, HeaderHoverSprite);
        static void CloseButton(int texWidth, int texHeight, Rect rect) => CommonAtlas.AddSprites(texWidth, texHeight, rect, 32, 32, DeleteNormal, DeleteHover, DeletePressed);


        public static void AddSprites(this UITextureAtlas atlas, int texWidth, int texHeight, Rect rect, string sprite)
            => atlas.AddSprites(texWidth, texHeight, rect, new RectOffset(), 0, sprite);

        public static void AddSprites(this UITextureAtlas atlas, int texWidth, int texHeight, Rect rect, RectOffset border, int space, string sprite)
            => atlas.AddSprites(texWidth, texHeight, rect, texWidth - 2 * space, texHeight - 2 * space, border, space, sprite);

        public static void AddSprites(this UITextureAtlas atlas, int texWidth, int texHeight, Rect rect, int spriteWidth, int spriteHeight, params string[] sprites)
            => atlas.AddSprites(texWidth, texHeight, rect, spriteWidth, spriteHeight, new RectOffset(), 0, sprites);

        public static void AddSpritesRows(this UITextureAtlas atlas, int texWidth, int texHeight, Rect rect, int spriteWidth, int spriteHeight, RectOffset border, int space, int inRow, params string[] sprites)
        {
            var rows = sprites.Length / inRow + (sprites.Length % inRow == 0 ? 0 : 1);

            var rowHeight = rect.height / texHeight * spriteHeight;
            var spaceHeight = rect.height / texHeight * space;

            for (var i = 0; i < rows; i += 1)
            {
                var rowRect = new Rect(rect.x, rect.y + (spaceHeight + rowHeight) * (rows - i - 1), rect.width, rowHeight + 2 * spaceHeight);
                var rowSprites = sprites.Skip(inRow * i).Take(inRow).ToArray();
                atlas.AddSprites(texWidth, spriteHeight + 2 * space, rowRect, spriteWidth, spriteHeight, border, space, rowSprites);
            }
        }
        public static void AddSprites(this UITextureAtlas atlas, int texWidth, int texHeight, Rect rect, int spriteWidth, int spriteHeight, RectOffset border, int space, params string[] sprites)
        {
            var width = spriteWidth / (float)texWidth * rect.width;
            var height = spriteHeight / (float)texHeight * rect.height;
            var spaceWidth = space / (float)texWidth * rect.width;
            var spaceHeight = space / (float)texHeight * rect.height;

            for (int i = 0; i < sprites.Length; i += 1)
            {
                var x = rect.x + i * width + (i + 1) * spaceWidth;
                var y = rect.y + spaceHeight;
                atlas.AddSprite(sprites[i], new Rect(x, y, width, height), border);

            }
        }
        public static void AddSprite(this UITextureAtlas atlas, string name, Rect region, RectOffset border = null)
        {
            UITextureAtlas.SpriteInfo spriteInfo = new UITextureAtlas.SpriteInfo
            {
                name = name,
                texture = atlas.material.mainTexture as Texture2D,
                region = region,
                border = border ?? new RectOffset()
            };
            atlas.AddSprite(spriteInfo);
        }

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
    }
}
