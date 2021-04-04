using ColossalFramework.Importers;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ModsCommon.Utilities
{
    public static class TextureHelper
    {
        public delegate SpriteParams[] SpriteParamsGetter(int texWidth, int texHeight, Rect rect);
        public static UITextureAtlas InGameAtlas { get; } = GetAtlas("Ingame");

        private static UITextureAtlas GetAtlas(string name)
        {
            UITextureAtlas[] atlases = Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) as UITextureAtlas[];
            for (int i = 0; i < atlases.Length; i++)
            {
                if (atlases[i].name == name)
                    return atlases[i];
            }
            return UIView.GetAView().defaultAtlas;
        }
        public static UITextureAtlas CreateAtlas(string atlasName, Dictionary<string, SpriteParamsGetter> files)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var textures = files.Select(f => assembly.LoadTextureFromAssembly(f.Key)).ToArray();
            var atlas = CreateAtlas(textures, atlasName, out Rect[] rects);
            var paramsGetters = files.Values.ToArray();

            for (var i = 0; i < paramsGetters.Length; i += 1)
                paramsGetters[i](textures[i].width, textures[i].height, rects[i]);

            return atlas;
        }
        private static UITextureAtlas CreateAtlas(Texture2D[] textures, string name, out Rect[] rects)
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

        public static IEnumerable<SpriteParams> GetSpritesParams(int texWidth, int texHeight, Rect rect, string name)
            => GetSpritesParams(texWidth, texHeight, rect, new RectOffset(), 0, name);

        public static IEnumerable<SpriteParams> GetSpritesParams(int texWidth, int texHeight, Rect rect, RectOffset border, int space, string name)
            => GetSpritesParams(texWidth, texHeight, rect, texWidth - 2 * space, texHeight - 2 * space, border, space, name);

        public static IEnumerable<SpriteParams> GetSpritesParams(int texWidth, int texHeight, Rect rect, int spriteWidth, int spriteHeight, params string[] names)
            => GetSpritesParams(texWidth, texHeight, rect, spriteWidth, spriteHeight, new RectOffset(), 0, names);

        public static IEnumerable<SpriteParams> GetSpritesRowsParams(int texWidth, int texHeight, Rect rect, int spriteWidth, int spriteHeight, RectOffset border, int space, int inRow, params string[] names)
        {
            var rows = names.Length / inRow + (names.Length % inRow == 0 ? 0 : 1);

            var rowHeight = rect.height / texHeight * spriteHeight;
            var spaceHeight = rect.height / texHeight * space;

            for (var i = 0; i < rows; i += 1)
            {
                var rowRect = new Rect(rect.x, rect.y + (spaceHeight + rowHeight) * (rows - i - 1), rect.width, rowHeight + 2 * spaceHeight);
                var rowNames = names.Skip(inRow * i).Take(inRow).ToArray();
                foreach (var sprite in GetSpritesParams(texWidth, spriteHeight + 2 * space, rowRect, spriteWidth, spriteHeight, border, space, rowNames))
                    yield return sprite;
            }
        }
        public static IEnumerable<SpriteParams> GetSpritesParams(int texWidth, int texHeight, Rect rect, int spriteWidth, int spriteHeight, RectOffset border, int space, params string[] names)
        {
            var width = spriteWidth / (float)texWidth * rect.width;
            var height = spriteHeight / (float)texHeight * rect.height;
            var spaceWidth = space / (float)texWidth * rect.width;
            var spaceHeight = space / (float)texHeight * rect.height;

            for (int i = 0; i < names.Length; i += 1)
            {
                var x = rect.x + i * width + (i + 1) * spaceWidth;
                var y = rect.y + spaceHeight;
                yield return new SpriteParams()
                {
                    name = names[i],
                    region = new Rect(x, y, width, height),
                    border = border,
                };
            }
        }
        public static void AddSprite(this UITextureAtlas atlas, string name, Rect region, RectOffset border)
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
    }
    public struct SpriteParams
    {
        public string name;
        public Rect region;
        public RectOffset border;
    }
}
