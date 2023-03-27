using ColossalFramework.UI;
using System;
using UnityEngine;

namespace ModsCommon.UI
{
    public class CustomUISlider : UIComponent
    {
        private Vector3 positionBefore;
        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout()
        {
            if ((relativePosition - positionBefore).sqrMagnitude > 0.001)
                relativePosition = positionBefore;
        }

        public event Action<float> OnSliderValueChanged;


        protected UIOrientation orientation;
        public UIOrientation Orientation
        {
            get => orientation;
            set
            {
                if (value != orientation)
                {
                    orientation = value;
                    Invalidate();
                }
            }
        }


        protected float rawValue = 10f;
        public float Value
        {
            get => rawValue;
            set
            {
                value = Mathf.Clamp(value, MinValue, MaxValue).Quantize(StepSize);
                if (!Mathf.Approximately(value, rawValue))
                {
                    rawValue = value;
                    OnValueChanged();
                }
            }
        }


        protected float minValue;
        public float MinValue
        {
            get => minValue;
            set
            {
                if (value != minValue)
                {
                    minValue = value;
                    if (rawValue < value)
                        Value = value;

                    Invalidate();
                }
            }
        }


        protected float maxValue = 100f;
        public float MaxValue
        {
            get => maxValue;
            set
            {
                if (value != maxValue)
                {
                    maxValue = value;
                    if (rawValue > value)
                        Value = value;

                    Invalidate();
                }
            }
        }


        protected float stepSize = 1f;
        public float StepSize
        {
            get => stepSize;
            set
            {
                value = Mathf.Max(0f, value);
                if (value != stepSize)
                {
                    stepSize = value;
                    Value = rawValue.Quantize(value);
                    Invalidate();
                }
            }
        }


        protected float scrollSize = 1f;
        public float ScrollWheelAmount
        {
            get => scrollSize;
            set
            {
                value = Mathf.Max(0f, value);
                if (value != scrollSize)
                {
                    scrollSize = value;
                    Invalidate();
                }
            }
        }


        protected Vector2 thumbOffset;
        public Vector2 ThumbOffset
        {
            get => thumbOffset;
            set
            {
                if (Vector2.Distance(value, thumbOffset) > float.Epsilon)
                {
                    thumbOffset = value;
                    Invalidate();
                }
            }
        }


        protected Vector2 thumbSize;
        public Vector2 ThumbSize
        {
            get => thumbSize;
            set
            {
                if (value != thumbSize)
                {
                    thumbSize = value;
                    Invalidate();
                }
            }
        }


        public Vector2 ThumbPosition
        {
            get
            {
                var position = ThumbOffset;

                switch (Orientation)
                {
                    case UIOrientation.Horizontal:
                        position.x -= thumbSize.x * 0.5f;
                        position.y -= (height - thumbSize.y) * 0.5f;
                        position.x += width / (MaxValue - MinValue) * (Value - MinValue);
                        break;
                    case UIOrientation.Vertical:
                        position.y += thumbSize.y * 0.5f;
                        position.x += (width - thumbSize.x) * 0.5f;
                        position.y -= height - height / (MaxValue - MinValue) * (Value - MinValue);
                        break;
                }

                return position;
            }
        }


        public override bool canFocus => (isEnabled && isVisible) || base.canFocus;


        //protected RectOffset padding;


        #region STYLE

        protected UITextureAtlas atlas;
        public UITextureAtlas Atlas
        {
            get => atlas ??= GetUIView()?.defaultAtlas;
            set
            {
                if (value != atlas)
                {
                    atlas = value;
                    Invalidate();
                }
            }
        }


        protected UITextureAtlas bgAtlas;
        public UITextureAtlas BgAtlas
        {
            get => bgAtlas ?? Atlas;
            set
            {
                if (!Equals(value, bgAtlas))
                {
                    bgAtlas = value;
                    Invalidate();
                }
            }
        }


        protected UITextureAtlas fgAtlas;
        public UITextureAtlas FgAtlas
        {
            get => fgAtlas ?? Atlas;
            set
            {
                if (!Equals(value, fgAtlas))
                {
                    fgAtlas = value;
                    Invalidate();
                }
            }
        }


        protected UITextureAtlas thumbAtlas;
        public UITextureAtlas ThumbAtlas
        {
            get => thumbAtlas ?? Atlas;
            set
            {
                if (!Equals(value, thumbAtlas))
                {
                    thumbAtlas = value;
                    Invalidate();
                }
            }
        }

        protected string bgSprite;
        public string BgSprite
        {
            get => bgSprite;
            set
            {
                if (value != bgSprite)
                {
                    bgSprite = value;
                    Invalidate();
                }
            }
        }


        protected string fgSprite;
        public string FgSprite
        {
            get => fgSprite;
            set
            {
                if (value != fgSprite)
                {
                    fgSprite = value;
                    Invalidate();
                }
            }
        }

        #region THUMB SPRITE

        protected SpriteSet thumbSprites;
        public SpriteSet ThumbSprites
        {
            get => thumbSprites;
            set
            {
                thumbSprites = value;
                Invalidate();
            }
        }

        public string NormalThumbSprite
        {
            get => thumbSprites.normal;
            set
            {
                if (value != thumbSprites.normal)
                {
                    thumbSprites.normal = value;
                    Invalidate();
                }
            }
        }
        public string HoveredThumbSprite
        {
            get => thumbSprites.hovered;
            set
            {
                if (value != thumbSprites.hovered)
                {
                    thumbSprites.hovered = value;
                    Invalidate();
                }
            }
        }
        public string PressedThumbSprite
        {
            get => thumbSprites.pressed;
            set
            {
                if (value != thumbSprites.pressed)
                {
                    thumbSprites.pressed = value;
                    Invalidate();
                }
            }
        }
        public string DisabledThumbSprite
        {
            get => thumbSprites.disabled;
            set
            {
                if (value != thumbSprites.disabled)
                {
                    thumbSprites.disabled = value;
                    Invalidate();
                }
            }
        }

        #endregion


        protected Color bgColor;
        public Color32 BgColor
        {
            get => bgColor;
            set
            {
                if (!bgColor.Equals(value))
                {
                    bgColor = value;
                    OnColorChanged();
                }
            }
        }


        protected Color fgColor;
        public Color32 FgColor
        {
            get => fgColor;
            set
            {
                if (!fgColor.Equals(value))
                {
                    fgColor = value;
                    OnColorChanged();
                }
            }
        }

        #region THUMB COLOR

        protected ColorSet thumbColors = new ColorSet(Color.white);
        public ColorSet ThumbColors
        {
            get => thumbColors;
            set
            {
                thumbColors = value;
                OnColorChanged();
            }
        }

        public Color32 NormalThumbColor
        {
            get => thumbColors.normal;
            set
            {
                if (!thumbColors.normal.Equals(value))
                {
                    thumbColors.normal = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 HoveredThumbColor
        {
            get => thumbColors.hovered;
            set
            {
                if (!thumbColors.hovered.Equals(value))
                {
                    thumbColors.hovered = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 PressedThumbColor
        {
            get => thumbColors.pressed;
            set
            {
                if (!thumbColors.pressed.Equals(value))
                {
                    thumbColors.pressed = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 DisabledThumbColor
        {
            get => thumbColors.disabled;
            set
            {
                if (!thumbColors.disabled.Equals(value))
                {
                    thumbColors.disabled = value;
                    OnColorChanged();
                }
            }
        }

        #endregion


        #endregion


        #region HANDLERS

        protected virtual void OnValueChanged()
        {
            Invalidate();
            OnSliderValueChanged?.Invoke(Value);
        }

        protected override void OnKeyDown(UIKeyEventParameter p)
        {
            if (builtinKeyNavigation)
            {
                if (Orientation == UIOrientation.Horizontal)
                {
                    if (p.keycode == KeyCode.LeftArrow)
                    {
                        Value -= ScrollWheelAmount;
                        p.Use();
                        return;
                    }

                    if (p.keycode == KeyCode.RightArrow)
                    {
                        Value += ScrollWheelAmount;
                        p.Use();
                        return;
                    }
                }
                else
                {
                    if (p.keycode == KeyCode.UpArrow)
                    {
                        Value -= ScrollWheelAmount;
                        p.Use();
                        return;
                    }

                    if (p.keycode == KeyCode.DownArrow)
                    {
                        Value += ScrollWheelAmount;
                        p.Use();
                        return;
                    }
                }
            }

            base.OnKeyDown(p);
        }
        protected override void OnMouseWheel(UIMouseEventParameter p)
        {
            float num = 1f;
            Value += ScrollWheelAmount * p.wheelDelta * num;
            p.Use();
        }
        protected override void OnMouseMove(UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Left))
            {
                Value = GetValueFromMouseEvent(p);
                p.Use();
            }
            else
                base.OnMouseMove(p);
        }
        protected override void OnMouseDown(UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Left))
            {
                Focus();
                Value = GetValueFromMouseEvent(p);
                p.Use();
            }
            else
                base.OnMouseMove(p);
        }
        public override void OnEnable()
        {
            base.OnEnable();
            Invalidate();
        }
        protected override void OnVisibilityChanged()
        {
            Invalidate();
        }

        private float GetValueFromMouseEvent(UIMouseEventParameter p)
        {
            var hitPoint = (p.ray.origin - transform.position) / PixelsToUnits();

            float position;
            switch (orientation)
            {
                case UIOrientation.Horizontal:
                    position = Mathf.Clamp(hitPoint.x, 0f, width) / width;
                    break;
                case UIOrientation.Vertical:
                    position = 1f - Mathf.Clamp(-hitPoint.y, 0f, height) / height;
                    break;
                default:
                    position = 0.5f;
                    break;
            }
            return Mathf.Lerp(minValue, maxValue, position);
        }

        #endregion

        #region RENDER

        protected UIRenderData BgRenderData { get; set; }
        protected UIRenderData FgRenderData { get; set; }
        protected UIRenderData ThumbRenderData { get; set; }

        public override void OnDisable()
        {
            BgRenderData = null;
            FgRenderData = null;
            ThumbRenderData = null;
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

            if (ThumbRenderData == null)
            {
                ThumbRenderData = UIRenderData.Obtain();
                m_RenderData.Add(ThumbRenderData);
            }
            else
                ThumbRenderData.Clear();

            if (BgAtlas is UITextureAtlas bgAtlas && FgAtlas is UITextureAtlas fgAtlas && ThumbAtlas is UITextureAtlas thumbAtlas)
            {
                BgRenderData.material = bgAtlas.material;
                FgRenderData.material = fgAtlas.material;
                ThumbRenderData.material = thumbAtlas.material;

                RenderBackground();
                RenderForeground();
                RenderThumb();
            }
        }

        protected virtual void RenderBackground()
        {
            if (BgAtlas[BgSprite] is UITextureAtlas.SpriteInfo bgSprite)
            {
                var renderOptions = new RenderOptions()
                {
                    atlas = BgAtlas,
                    color = BgColor,
                    fillAmount = 1f,
                    offset = pivot.TransformToUpperLeft(size, arbitraryPivotOffset),
                    pixelsToUnits = PixelsToUnits(),
                    size = size,
                    spriteInfo = bgSprite,
                };

                if (bgSprite.isSliced)
                    Render.RenderSlicedSprite(BgRenderData, renderOptions);
                else
                    Render.RenderSprite(BgRenderData, renderOptions);
            }
        }
        protected virtual void RenderForeground()
        {
            if (FgAtlas[FgSprite] is UITextureAtlas.SpriteInfo fgSprite)
            {
                var renderOptions = new RenderOptions()
                {
                    atlas = FgAtlas,
                    color = FgColor,
                    fillAmount = 1f,
                    offset = pivot.TransformToUpperLeft(size, arbitraryPivotOffset),
                    pixelsToUnits = PixelsToUnits(),
                    size = size,
                    spriteInfo = fgSprite,
                };

                if (fgSprite.isSliced)
                    Render.RenderSlicedSprite(FgRenderData, renderOptions);
                else
                    Render.RenderSprite(FgRenderData, renderOptions);
            }
        }
        protected virtual void RenderThumb()
        {
            if (RenderThumbSprite is UITextureAtlas.SpriteInfo foregroundSprite)
            {
                var renderSize = ThumbSize;
                var renderOffset = pivot.TransformToUpperLeft(size, arbitraryPivotOffset) + (Vector3)ThumbPosition;
                //var renderOffset = pivot.TransformToUpperLeft(size, arbitraryPivotOffset);
                //switch (Orientation)
                //{
                //    case UIOrientation.Horizontal:
                //        renderOffset.x -= thumbSize.x * 0.5f;
                //        renderOffset.y -= (height - thumbSize.y) * 0.5f;
                //        renderOffset.x += width / (MaxValue - MinValue) * (Value - MinValue);
                //        break;
                //    case UIOrientation.Vertical:
                //        renderOffset.y += thumbSize.y * 0.5f;
                //        renderOffset.x += (width - thumbSize.x) * 0.5f;
                //        renderOffset.y -= height - height / (MaxValue - MinValue) * (Value - MinValue);
                //        break;
                //}
                //renderOffset += (Vector3)thumbOffset;

                var renderOptions = new RenderOptions()
                {
                    atlas = ThumbAtlas,
                    color = RenderThumbColor,
                    fillAmount = 1f,
                    offset = renderOffset,
                    pixelsToUnits = PixelsToUnits(),
                    size = renderSize,
                    spriteInfo = foregroundSprite,
                };

                if (foregroundSprite.isSliced)
                    Render.RenderSlicedSprite(ThumbRenderData, renderOptions);
                else
                    Render.RenderSprite(ThumbRenderData, renderOptions);
            }
        }
        protected virtual UITextureAtlas.SpriteInfo RenderThumbSprite
        {
            get
            {
                if (!isEnabled)
                    return ThumbAtlas[DisabledThumbSprite];
                else if (m_IsMouseHovering)
                    return ThumbAtlas[HoveredThumbSprite];
                else
                    return ThumbAtlas[NormalThumbSprite];
            }
        }
        protected virtual Color32 RenderThumbColor
        {
            get
            {
                if (!isEnabled)
                    return DisabledThumbColor;
                else if (m_IsMouseHovering)
                    return HoveredThumbColor;
                else
                    return NormalThumbColor;
            }
        }

        #endregion
    }
}
