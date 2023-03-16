using ColossalFramework.UI;
using UnityEngine;

namespace ModsCommon.UI
{
    public class CustomUIPanel : UIPanel
    {
        private Vector3 positionBefore;
        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout()
        {
            if ((relativePosition - positionBefore).sqrMagnitude > 0.001)
                relativePosition = positionBefore;
        }


        UITextureAtlas _atlasForeground;
        UITextureAtlas _atlasBackground;

        public UITextureAtlas atlasForeground
        {
            get => _atlasForeground ?? atlas;
            set
            {
                if (!Equals(value, _atlasForeground))
                {
                    _atlasForeground = value;
                    Invalidate();
                }
            }
        }
        public UITextureAtlas atlasBackground
        {
            get => _atlasBackground ?? atlas;
            set
            {
                if (!Equals(value, _atlasBackground))
                {
                    _atlasBackground = value;
                    Invalidate();
                }
            }
        }

        Color32? _normalBgColor;
        public Color32 normalBgColor
        {
            get => _normalBgColor ?? base.color;
            set
            {
                _normalBgColor = value;
                Invalidate();
            }
        }

        Color32? _disabledBgColor;
        public Color32 disabledBgColor
        {
            get => _disabledBgColor ?? base.disabledColor;
            set
            {
                _disabledBgColor = value;
                Invalidate();
            }
        }

        Color32? _normalFgColor;
        public Color32 normalFgColor
        {
            get => _normalFgColor ?? base.color;
            set
            {
                _normalFgColor = value;
                Invalidate();
            }
        }

        Color32? _disabledFgColor;
        public Color32 disabledFgColor
        {
            get => _disabledFgColor ?? base.disabledColor;
            set
            {
                _disabledFgColor = value;
                Invalidate();
            }
        }

        string _foregroundSprite;
        public string foregroundSprite
        {
            get => _foregroundSprite;
            set
            {
                if (value != _foregroundSprite)
                {
                    _foregroundSprite = value;
                    Invalidate();
                }
            }
        }

        private RectOffset _spritePadding;
        public RectOffset spritePadding
        {
            get => _spritePadding ??= new RectOffset();
            set
            {
                if (!Equals(value, _spritePadding))
                {
                    _spritePadding = value;
                    Invalidate();
                }
            }
        }

        protected UIRenderData BgRenderData { get; set; }
        protected UIRenderData FgRenderData { get; set; }

        public override void OnDisable()
        {
            BgRenderData = null;
            FgRenderData = null;
            base.OnDisable();
        }
        protected override void OnRebuildRenderData()
        {
            if (BgRenderData == null)
            {
                BgRenderData = UIRenderData.Obtain();
                m_RenderData.Add(BgRenderData);
            }
            else
                BgRenderData.Clear();

            if (FgRenderData == null)
            {
                FgRenderData = UIRenderData.Obtain();
                m_RenderData.Add(FgRenderData);
            }
            else
                FgRenderData.Clear();

            if (atlasBackground is UITextureAtlas bgAtlas && atlasForeground is UITextureAtlas fgAtlas)
            {
                BgRenderData.material = bgAtlas.material;
                FgRenderData.material = fgAtlas.material;

                RenderBackground();
                RenderForeground();
            }
        }

        private void RenderBackground()
        {
            if (atlasBackground[this.backgroundSprite] is UITextureAtlas.SpriteInfo backgroundSprite)
            {
                var renderOptions = new RenderOptions()
                {
                    atlas = atlasBackground,
                    color = isEnabled ? normalBgColor : disabledBgColor,
                    fillAmount = 1f,
                    flip = m_Flip,
                    offset = pivot.TransformToUpperLeft(size, arbitraryPivotOffset),
                    pixelsToUnits = PixelsToUnits(),
                    size = size,
                    spriteInfo = backgroundSprite,
                };

                if (backgroundSprite.isSliced)
                    Render.RenderSlicedSprite(BgRenderData, renderOptions);
                else
                    Render.RenderSprite(BgRenderData, renderOptions);
            }
        }
        private void RenderForeground()
        {
            if (atlasForeground[this.foregroundSprite] is UITextureAtlas.SpriteInfo foregroundSprite)
            {
                var foregroundRenderSize = GetForegroundRenderSize(foregroundSprite);
                var foregroundRenderOffset = GetForegroundRenderOffset(foregroundRenderSize);

                var renderOptions = new RenderOptions()
                {
                    atlas = atlasForeground,
                    color = isEnabled ? normalFgColor : disabledFgColor,
                    fillAmount = 1f,
                    flip = m_Flip,
                    offset = foregroundRenderOffset,
                    pixelsToUnits = PixelsToUnits(),
                    size = foregroundRenderSize,
                    spriteInfo = foregroundSprite,
                };

                if (foregroundSprite.isSliced)
                    Render.RenderSlicedSprite(BgRenderData, renderOptions);
                else
                    Render.RenderSprite(BgRenderData, renderOptions);
            }
        }
        protected virtual Vector2 GetForegroundRenderSize(UITextureAtlas.SpriteInfo spriteInfo)
        {
            if (spriteInfo == null)
                return Vector2.zero;

            return new Vector2(width - spritePadding.horizontal, height - spritePadding.vertical);
        }
        protected virtual Vector2 GetForegroundRenderOffset(Vector2 renderSize)
        {
            Vector2 result = pivot.TransformToUpperLeft(size, arbitraryPivotOffset);

            result.x += (width - renderSize.x) * 0.5f;
            result.x += spritePadding.left - spritePadding.right;

            result.y -= (height - renderSize.y) * 0.5f;
            result.y -= spritePadding.top - spritePadding.bottom;

            return result;
        }
    }
}
