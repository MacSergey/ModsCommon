using ColossalFramework.UI;
using System;
using UnityEngine;

namespace ModsCommon.UI
{
    public class CustomUITextField : UITextField
    {
        private Vector3 positionBefore;
        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout()
        {
            if ((relativePosition - positionBefore).sqrMagnitude > 0.001)
                relativePosition = positionBefore;
        }

        protected override void OnKeyDown(UIKeyEventParameter p)
        {
            if (builtinKeyNavigation && !readOnly && !p.used)
            {
                if (p.keycode == KeyCode.Return && multiline)
                {
                    if (!p.shift)
                    {
                        multiline = false;
                        base.OnKeyDown(p);
                        multiline = true;
                        return;
                    }
                }
            }

            base.OnKeyDown(p);
        }


        UITextureAtlas bgAtlas;
        public UITextureAtlas BgAtlas
        {
            get => bgAtlas ?? atlas;
            set
            {
                if (!Equals(value, bgAtlas))
                {
                    bgAtlas = value;
                    Invalidate();
                }
            }
        }


        UITextureAtlas fgAtlas;
        public UITextureAtlas FgAtlas
        {
            get => fgAtlas ?? atlas;
            set
            {
                if (!Equals(value, fgAtlas))
                {
                    fgAtlas = value;
                    Invalidate();
                }
            }
        }


        [Obsolete]
        public new Color32 color
        {
            get => base.color;
            set
            {
                bgColors = value;
                fgColors = value;
                base.color = value;
            }
        }
        [Obsolete]
        public new string normalFgSprite
        {
            get => base.normalFgSprite;
            set => base.normalFgSprite = value;
        }
        [Obsolete]
        public new string hoveredFgSprite
        {
            get => base.hoveredFgSprite;
            set => base.hoveredFgSprite = value;
        }
        [Obsolete]
        public new string disabledFgSprite
        {
            get => base.disabledFgSprite;
            set => base.disabledFgSprite = value;
        }
        [Obsolete]
        public new string focusedFgSprite
        {
            get => base.focusedFgSprite;
            set => base.focusedFgSprite = value;
        }
        [Obsolete]
        public new string normalBgSprite
        {
            get => base.normalBgSprite;
            set => base.normalBgSprite = value;
        }
        [Obsolete]
        public new string hoveredBgSprite
        {
            get => base.hoveredBgSprite;
            set => base.hoveredBgSprite = value;
        }
        [Obsolete]
        public new string disabledBgSprite
        {
            get => base.disabledBgSprite;
            set => base.disabledBgSprite = value;
        }
        [Obsolete]
        public new string focusedBgSprite
        {
            get => base.focusedBgSprite;
            set => base.focusedBgSprite = value;
        }


        #region BACKGROUND SPRITE

        protected UI.SpriteSet bgSprites;
        public UI.SpriteSet BgSprites
        {
            get => bgSprites;
            set
            {
                bgSprites = value;
                Invalidate();
            }
        }
        public string NormalBgSprite
        {
            get => bgSprites.normal;
            set
            {
                if (value != bgSprites.normal)
                {
                    bgSprites.normal = value;
                    Invalidate();
                }
            }
        }
        public string HoveredBgSprite
        {
            get => bgSprites.hovered;
            set
            {
                if (value != bgSprites.hovered)
                {
                    bgSprites.hovered = value;
                    Invalidate();
                }
            }
        }
        public string FocusedBgSprite
        {
            get => bgSprites.focused;
            set
            {
                if (value != bgSprites.focused)
                {
                    bgSprites.focused = value;
                    Invalidate();
                }
            }
        }
        public string DisabledBgSprite
        {
            get => bgSprites.disabled;
            set
            {
                if (value != bgSprites.disabled)
                {
                    bgSprites.disabled = value;
                    Invalidate();
                }
            }
        }

        #endregion

        #region FOREGROUND SPRITE

        protected UI.SpriteSet fgSprites;
        public UI.SpriteSet FgSprites
        {
            get => fgSprites;
            set
            {
                fgSprites = value;
                Invalidate();
            }
        }

        public string NormalFgSprite
        {
            get => fgSprites.normal;
            set
            {
                if (value != fgSprites.normal)
                {
                    fgSprites.normal = value;
                    Invalidate();
                }
            }
        }
        public string HoveredFgSprite
        {
            get => fgSprites.hovered;
            set
            {
                if (value != fgSprites.hovered)
                {
                    fgSprites.hovered = value;
                    Invalidate();
                }
            }
        }
        public string FocusedFgSprite
        {
            get => fgSprites.focused;
            set
            {
                if (value != fgSprites.focused)
                {
                    fgSprites.focused = value;
                    Invalidate();
                }
            }
        }
        public string DisabledFgSprite
        {
            get => fgSprites.disabled;
            set
            {
                if (value != fgSprites.disabled)
                {
                    fgSprites.disabled = value;
                    Invalidate();
                }
            }
        }

        #endregion

        #region BACKGROUND COLOR

        protected ColorSet bgColors = new ColorSet(Color.white);
        public ColorSet BgColors
        {
            get => bgColors;
            set
            {
                bgColors = value;
                OnColorChanged();
            }
        }

        public Color32 NormalBgColor
        {
            get => bgColors.normal;
            set
            {
                if (!bgColors.normal.Equals(value))
                {
                    bgColors.normal = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 FocusedBgColor
        {
            get => bgColors.focused;
            set
            {
                if (!bgColors.focused.Equals(value))
                {
                    bgColors.focused = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 HoveredBgColor
        {
            get => bgColors.hovered;
            set
            {
                if (!bgColors.hovered.Equals(value))
                {
                    bgColors.hovered = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 DisabledBgColor
        {
            get => bgColors.disabled;
            set
            {
                if (!bgColors.disabled.Equals(value))
                {
                    bgColors.disabled = value;
                    OnColorChanged();
                }
            }
        }

        #endregion

        #region FOREGROUND COLOR

        protected ColorSet fgColors = new ColorSet(Color.white);
        public ColorSet FgColors
        {
            get => fgColors;
            set
            {
                fgColors = value;
                OnColorChanged();
            }
        }

        public Color32 NormalFgColor
        {
            get => fgColors.normal;
            set
            {
                if (!fgColors.normal.Equals(value))
                {
                    fgColors.normal = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 FocusedFgColor
        {
            get => fgColors.focused;
            set
            {
                if (!fgColors.focused.Equals(value))
                {
                    fgColors.focused = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 HoveredFgColor
        {
            get => fgColors.hovered;
            set
            {
                if (!fgColors.hovered.Equals(value))
                {
                    fgColors.hovered = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 DisabledFgColor
        {
            get => fgColors.disabled;
            set
            {
                if (!fgColors.disabled.Equals(value))
                {
                    fgColors.disabled = value;
                    OnColorChanged();
                }
            }
        }

        #endregion


        public TextFieldStyle TextFieldStyle
        {
            set
            {
                bgAtlas = value.BgAtlas;
                fgAtlas = value.FgAtlas;

                bgSprites = value.BgSprites;
                fgSprites = value.FgSprites;

                bgColors = value.BgColors;
                fgColors = value.FgColors;

                m_TextColor = value.TextColor;

                m_Atlas = value.BgAtlas;
                m_SelectionSprite = value.SelectionSprite;
                m_SelectionBackground = value.SelectionColor;

                Invalidate();
            }
        }

        protected UIRenderData FgRenderData { get; set; }

        public override void OnDisable()
        {
            FgRenderData = null;
            base.OnDisable();
        }

        protected override void OnRebuildRenderData()
        {
            base.OnRebuildRenderData();

            if (FgRenderData == null)
            {
                FgRenderData = UIRenderData.Obtain();
                m_RenderData.Add(FgRenderData);
            }
            else
                FgRenderData.Clear();

            if (renderData != null && BgAtlas is UITextureAtlas atlas)
                renderData.material = atlas.material;

            if (FgAtlas is UITextureAtlas fgAtlas)
            {
                FgRenderData.material = fgAtlas.material;
                RenderForeground();
            }
        }

        protected override void RenderBackground()
        {
            if (BgAtlas is UITextureAtlas atlas)
            {
                var backgroundSprite = GetBackgroundSprite();
                if (backgroundSprite != null)
                {
                    var renderOptions = new RenderOptions()
                    {
                        atlas = atlas,
                        color = ApplyOpacity(GetActiveColor()),
                        fillAmount = 1f,
                        flip = UISpriteFlip.None,
                        offset = pivot.TransformToUpperLeft(size, arbitraryPivotOffset),
                        pixelsToUnits = PixelsToUnits(),
                        size = size,
                        spriteInfo = backgroundSprite,
                    };

                    if (backgroundSprite.isSliced)
                        Render.RenderSlicedSprite(renderData, renderOptions);
                    else
                        Render.RenderSprite(renderData, renderOptions);
                }
            }
        }
        protected override UITextureAtlas.SpriteInfo GetBackgroundSprite()
        {
            if (BgAtlas is UITextureAtlas atlas)
            {
                if (!isEnabled)
                    return atlas[DisabledBgSprite];
                else if (hasFocus)
                    return atlas[FocusedBgSprite] ?? atlas[NormalBgSprite];
                else if (m_IsMouseHovering)
                    return atlas[HoveredBgSprite] ?? atlas[NormalBgSprite];
                else
                    return atlas[NormalBgSprite];
            }

            return null;
        }
        protected override Color32 GetActiveColor()
        {
            if (!isEnabled)
            {
                if (!string.IsNullOrEmpty(DisabledBgSprite) && atlas != null && atlas[DisabledBgSprite] != null)
                    return DisabledBgColor;
                else
                    return NormalBgColor;
            }
            else if (hasFocus)
            {
                if (!string.IsNullOrEmpty(FocusedBgSprite) && atlas != null && atlas[FocusedBgSprite] != null)
                    return FocusedBgColor;
                else
                    return NormalBgColor;
            }
            else if (m_IsMouseHovering)
            {
                if (!string.IsNullOrEmpty(HoveredBgSprite) && atlas != null && atlas[HoveredBgSprite] != null)
                    return HoveredBgColor;
                else
                    return NormalBgColor;
            }
            else
                return NormalBgColor;
        }

        protected override void RenderForeground()
        {
            if (RenderForegroundSprite is UITextureAtlas.SpriteInfo foregroundSprite)
            {
                var foregroundRenderSize = GetForegroundRenderSize(foregroundSprite);
                var foregroundRenderOffset = GetForegroundRenderOffset(foregroundRenderSize);

                var renderOptions = new RenderOptions()
                {
                    atlas = FgAtlas,
                    color = RenderForegroundColor,
                    fillAmount = 1f,
                    flip = UISpriteFlip.None,
                    offset = foregroundRenderOffset,
                    pixelsToUnits = PixelsToUnits(),
                    size = foregroundRenderSize,
                    spriteInfo = foregroundSprite,
                };

                if (foregroundSprite.isSliced)
                    Render.RenderSlicedSprite(FgRenderData, renderOptions);
                else
                    Render.RenderSprite(FgRenderData, renderOptions);
            }
        }

        protected virtual UITextureAtlas.SpriteInfo RenderForegroundSprite
        {
            get
            {
                if (FgAtlas is not UITextureAtlas atlas)
                    return null;

                if (!isEnabled)
                    return atlas[DisabledFgSprite];
                else if (hasFocus)
                    return atlas[FocusedFgSprite];
                else if (m_IsMouseHovering)
                    return atlas[HoveredFgSprite];
                else
                    return atlas[NormalFgSprite];
            }
        }
        private Color32 RenderForegroundColor
        {
            get
            {
                if (!isEnabled)
                    return DisabledFgColor;
                else if (hasFocus)
                    return FocusedFgColor;
                else if (m_IsMouseHovering)
                    return HoveredFgColor;
                else
                    return NormalFgColor;
            }
        }
    }
}
