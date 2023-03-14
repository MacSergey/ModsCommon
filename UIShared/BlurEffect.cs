using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModsCommon.UI
{
    public class BlurEffect : CustomUITextureSprite
    {
        private static Material BlurMaterial { get; }
        private static Texture2D BlurTexture { get; }

        static BlurEffect()
        {
            BlurMaterial = new Material(Shader.Find("UI/ModalEffect"))
            {
                name = nameof(BlurEffect),
                color = new Color(0f, 0f, 0f, 0f),
                renderQueue = 4000,
            };
            BlurMaterial.EnableKeyword("_LIGHTMAPPINGREALTIME");

            BlurTexture = new Texture2D(1, 1);
            BlurTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 255f));
            BlurTexture.Apply();
        }
        public BlurEffect()
        {
            material = BlurMaterial;
            texture = BlurTexture;
        }
    }
}
