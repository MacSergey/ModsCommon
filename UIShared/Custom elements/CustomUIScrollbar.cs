using ColossalFramework;
using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModsCommon.UI
{
    public class CustomUIScrollbar : UIComponent
    {
        private Vector3 positionBefore;
        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout()
        {
            if ((relativePosition - positionBefore).sqrMagnitude > 0.001)
                relativePosition = positionBefore;
        }

        public event Action<float> OnScrollValueChanged;

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

        protected float rawValue = 1f;
        public float Value
        {
            get => rawValue;
            set
            {
                value = AdjustValue(value);
                if (!Mathf.Approximately(value, rawValue))
                {
                    rawValue = value;
                    OnValueChanged();
                }

                SetAutoHide();
                UpdateThumb(rawValue);
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
                    Value = Value;
                    SetAutoHide();
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
                    Value = Value;
                    Invalidate();
                    SetAutoHide();
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
                    Value = Value;
                    Invalidate();
                }
            }
        }

        protected float scrollSize = 1f;
        public float ScrollSize
        {
            get => scrollSize;
            set
            {
                value = Mathf.Max(0f, value);
                if (value != scrollSize)
                {
                    scrollSize = value;
                    Value = Value;
                    Invalidate();
                    SetAutoHide();
                }
            }
        }

        protected float increment = 1f;
        public float Increment
        {
            get => increment;
            set
            {
                value = Mathf.Max(0f, value);
                if (!Mathf.Approximately(value, increment))
                    increment = value;
            }
        }

        private AnimatedFloat easing;
        protected EasingType easingType;
        public EasingType scrollEasingType
        {
            get => easingType;
            set => easingType = value;
        }

        protected float easingTime = 1f;
        public float scrollEasingTime
        {
            get => easingTime;
            set => easingTime = value;
        }

        protected RectOffset trackPadding;
        public RectOffset TrackPadding
        {
            get => trackPadding ??= new RectOffset();
            set
            {
                if (!Equals(value, trackPadding))
                {
                    trackPadding = value;
                    UpdateThumb(rawValue);
                }
            }
        }

        protected RectOffset thumbPadding;
        public RectOffset ThumbPadding
        {
            get => thumbPadding ??= new RectOffset();
            set
            {
                //switch (orientation)
                //{
                //    case UIOrientation.Horizontal:
                //        value.left = 0;
                //        value.right = 0;
                //        break;
                //    case UIOrientation.Vertical:
                //        value.top = 0;
                //        value.bottom = 0;
                //        break;
                //}

                if (!Equals(value, thumbPadding))
                {
                    thumbPadding = value;
                    UpdateThumb(rawValue);
                }
            }
        }
        protected Vector2 minThumbSize;
        public Vector2 MinThumbSize
        {
            get => minThumbSize;
            set
            {
                if (value != minThumbSize)
                {
                    minThumbSize = value;
                    UpdateThumb(rawValue);
                }
            }
        }

        protected bool autoHide;
        public bool AutoHide
        {
            get => autoHide;
            set
            {
                if (value != autoHide)
                {
                    autoHide = value;
                    Invalidate();
                    SetAutoHide();
                }
            }
        }

        public override bool canFocus => (isEnabled && isVisible) || base.canFocus;

        private bool thumbVisible;
        private Vector2 thumbSize;
        private Vector2 thumbPosition;
        private Vector2 thumbMouseOffset;

        protected virtual void OnValueChanged()
        {
            SetAutoHide();
            Invalidate();

            OnScrollValueChanged?.Invoke(Value);
        }
        public override void Update()
        {
            base.Update();
            if (easing != null)
            {
                if (!easing.isDone)
                {
                    Value = easing;
                }
                else
                {
                    Value = easing.endValue;
                    easing = null;
                }
            }
        }
        private void SetAutoHide()
        {
            if (autoHide)
            {
                if (Mathf.CeilToInt(scrollSize) >= Mathf.CeilToInt(maxValue - minValue))
                    Hide();
                else
                    Show();
            }
        }
        private void ScrollEase(float delta)
        {
            if (easing == null)
            {
                easing = new AnimatedFloat(Value, Value + delta, easingTime, easingType);
            }
            else
            {
                float num = Mathf.Max(maxValue - minValue, 0f);
                float max = Mathf.Max(num - scrollSize, 0f) + minValue;
                float num2 = Mathf.Clamp(easing.endValue + delta, minValue, max);
                if (!Mathf.Approximately(num2, easing.endValue))
                {
                    easing.startValue = easing.value;
                    easing.endValue = num2;
                }
            }

            Value = easing;
        }
        private float AdjustValue(float value)
        {
            float num = Mathf.Max(maxValue - minValue, 0f);
            float a = Mathf.Max(num - scrollSize, 0f) + minValue;
            float num2 = Mathf.Max(Mathf.Min(a, value), minValue);
            return num2.Quantize(stepSize);
        }
        protected override void OnKeyDown(UIKeyEventParameter p)
        {
            if (builtinKeyNavigation)
            {
                if (orientation == UIOrientation.Horizontal)
                {
                    if (p.keycode == KeyCode.LeftArrow)
                    {
                        ScrollEase(0f - increment);
                        p.Use();
                        return;
                    }

                    if (p.keycode == KeyCode.RightArrow)
                    {
                        ScrollEase(increment);
                        p.Use();
                        return;
                    }
                }
                else
                {
                    if (p.keycode == KeyCode.UpArrow)
                    {
                        ScrollEase(0f - increment);
                        p.Use();
                        return;
                    }

                    if (p.keycode == KeyCode.DownArrow)
                    {
                        ScrollEase(increment);
                        p.Use();
                        return;
                    }
                }
            }

            base.OnKeyDown(p);
        }
        protected override void OnMouseWheel(UIMouseEventParameter p)
        {
            Value += increment * (0f - p.wheelDelta);
            p.Use();
        }
        protected override void OnMouseHover(UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Left))
            {
                UpdateFromTrackClick(p);
                p.Use();
            }
            else
                base.OnMouseHover(p);
        }
        protected override void OnMouseMove(UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Left))
            {
                Value = Mathf.Max(minValue, GetValueFromMouseEvent(p) - scrollSize * 0.5f);
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
                Raycast(p.ray, out var hitPoint);
                hitPoint = (hitPoint - transform.position) / PixelsToUnits();

                var delta = new Vector2(hitPoint.x - thumbPosition.x, -hitPoint.y - thumbPosition.y);
                if (0 <= delta.x && delta.x <= thumbSize.x && 0 <= delta.y && delta.y <= thumbSize.y)
                    thumbMouseOffset = delta - thumbSize * 0.5f;
                else
                    UpdateFromTrackClick(p);

                p.Use();
            }
            else
                base.OnMouseDown(p);
        }
        private float GetValueFromMouseEvent(UIMouseEventParameter p)
        {
            var hitPoint = (p.ray.origin - transform.position) / PixelsToUnits();

            float position;
            float length;
            switch (orientation)
            {
                case UIOrientation.Horizontal:
                    position = Mathf.Clamp(hitPoint.x - thumbMouseOffset.x, 0f, width) - ThumbPadding.horizontal;
                    length = width - ThumbPadding.horizontal;
                    break;
                case UIOrientation.Vertical:
                    position = Mathf.Clamp(-hitPoint.y - thumbMouseOffset.y, 0f, height) - ThumbPadding.vertical;
                    length = height - ThumbPadding.vertical;
                    break;
                default:
                    position = 0.5f;
                    length = 1f;
                    break;
            }
            return Mathf.Lerp(minValue, maxValue, position / length);
        }
        private void UpdateFromTrackClick(UIMouseEventParameter p)
        {
            float valueFromMouseEvent = GetValueFromMouseEvent(p);

            if (valueFromMouseEvent > rawValue + scrollSize)
                Value += scrollSize;
            else if (valueFromMouseEvent < rawValue)
                Value -= scrollSize;
        }

        private void UpdateThumb(float rawValue)
        {
            var delta = maxValue - minValue;
            if (delta <= 0f || delta <= scrollSize)
            {
                thumbVisible = false;
            }
            else
            {
                thumbVisible = true;

                var trackLength = 0f;
                var thumbLength = 0f;
                switch (orientation)
                {
                    case UIOrientation.Horizontal:
                        trackLength = width - ThumbPadding.horizontal;
                        thumbLength = Mathf.Max(scrollSize / delta * trackLength, minThumbSize.x);
                        thumbSize = new Vector2(thumbLength, height - ThumbPadding.vertical);
                        break;
                    case UIOrientation.Vertical:
                        trackLength = height - ThumbPadding.vertical;
                        thumbLength = Mathf.Max(scrollSize / delta * trackLength, minThumbSize.y);
                        thumbSize = new Vector2(width - ThumbPadding.horizontal, thumbLength);
                        break;

                }

                var t = (rawValue - minValue) / (delta - scrollSize);
                float num5 = t * (trackLength - thumbLength);

                var direction = orientation switch
                {
                    UIOrientation.Horizontal => Vector2.right,
                    UIOrientation.Vertical => Vector2.up,
                    _ => Vector2.zero,
                };
                var offset = new Vector2(ThumbPadding.left, ThumbPadding.top);
                thumbPosition = direction * num5 + offset;
            }

            Invalidate();
        }

        #region STYLE

        protected UITextureAtlas atlas;
        public UITextureAtlas Atlas
        {
            get => atlas ??= GetUIView()?.defaultAtlas;
            set
            {
                if (!Equals(value, atlas))
                {
                    atlas = value;
                    Invalidate();
                }
            }
        }

        protected UITextureAtlas atlasTrack;
        public UITextureAtlas AtlasTrack
        {
            get => atlasTrack ?? Atlas;
            set
            {
                if (!Equals(value, atlasTrack))
                {
                    atlasTrack = value;
                    Invalidate();
                }
            }
        }

        protected UITextureAtlas atlasThumb;
        public UITextureAtlas AtlasThumb
        {
            get => atlasThumb ?? Atlas;
            set
            {
                if (!Equals(value, atlasThumb))
                {
                    atlasThumb = value;
                    Invalidate();
                }
            }
        }

        string trackSprite;
        public string TrackSprite
        {
            get => trackSprite;
            set
            {
                if (value != trackSprite)
                {
                    trackSprite = value;
                    Invalidate();
                }
            }
        }

        string thumbSprite;
        public string ThumbSprite
        {
            get => thumbSprite;
            set
            {
                if (value != thumbSprite)
                {
                    thumbSprite = value;
                    Invalidate();
                }
            }
        }

        Color32? trackColor;
        public Color32 TrackColor
        {
            get => trackColor ?? base.color;
            set
            {
                trackColor = value;
                Invalidate();
            }
        }

        Color32? thumbColor;
        public Color32 ThumbColor
        {
            get => thumbColor ?? base.color;
            set
            {
                thumbColor = value;
                Invalidate();
            }
        }

        #endregion

        #region RENDER

        protected UIRenderData TrackRenderData { get; set; }
        protected UIRenderData ThumbRenderData { get; set; }

        public override void OnDisable()
        {
            TrackRenderData = null;
            ThumbRenderData = null;

            base.OnDisable();
        }

        protected override void OnRebuildRenderData()
        {
            if (TrackRenderData == null)
            {
                TrackRenderData = UIRenderData.Obtain();
                m_RenderData.Add(TrackRenderData);
            }
            else
                TrackRenderData.Clear();

            if (ThumbRenderData == null)
            {
                ThumbRenderData = UIRenderData.Obtain();
                m_RenderData.Add(ThumbRenderData);
            }
            else
                ThumbRenderData.Clear();

            if (AtlasTrack is UITextureAtlas trackAtlas && AtlasThumb is UITextureAtlas thumbAtlas)
            {
                TrackRenderData.material = trackAtlas.material;
                ThumbRenderData.material = thumbAtlas.material;

                RenderTrack();
                RenderThumb();
            }
        }

        private void RenderTrack()
        {
            if (AtlasTrack[TrackSprite] is UITextureAtlas.SpriteInfo trackSprite)
            {
                var trackRenderSize = GetTrackRenderSize(trackSprite);
                var trackRenderOffset = GetTrackRenderOffset(trackRenderSize);

                var renderOptions = new RenderOptions()
                {
                    atlas = AtlasTrack,
                    color = TrackColor,
                    fillAmount = 1f,
                    offset = trackRenderOffset,
                    pixelsToUnits = PixelsToUnits(),
                    size = trackRenderSize,
                    spriteInfo = trackSprite,
                };

                if (trackSprite.isSliced)
                    Render.RenderSlicedSprite(TrackRenderData, renderOptions);
                else
                    Render.RenderSprite(TrackRenderData, renderOptions);
            }
        }
        protected virtual Vector2 GetTrackRenderSize(UITextureAtlas.SpriteInfo spriteInfo)
        {
            if (spriteInfo == null)
                return Vector2.zero;

            return new Vector2(width - TrackPadding.horizontal, height - TrackPadding.vertical);
        }
        protected virtual Vector2 GetTrackRenderOffset(Vector2 renderSize)
        {
            Vector2 result = pivot.TransformToUpperLeft(size, arbitraryPivotOffset);

            result.x += TrackPadding.left;
            result.y -= TrackPadding.top;

            return result;
        }

        private void RenderThumb()
        {
            if (thumbVisible && AtlasThumb[ThumbSprite] is UITextureAtlas.SpriteInfo thumbSprite)
            {
                var thumbRenderSize = GetThumbRenderSize(thumbSprite);
                var thumbRenderOffset = GetThumbRenderOffset(thumbRenderSize);

                var renderOptions = new RenderOptions()
                {
                    atlas = AtlasThumb,
                    color = ThumbColor,
                    fillAmount = 1f,
                    offset = thumbRenderOffset,
                    pixelsToUnits = PixelsToUnits(),
                    size = thumbRenderSize,
                    spriteInfo = thumbSprite,
                };

                if (thumbSprite.isSliced)
                    Render.RenderSlicedSprite(ThumbRenderData, renderOptions);
                else
                    Render.RenderSprite(ThumbRenderData, renderOptions);
            }
        }
        protected virtual Vector2 GetThumbRenderSize(UITextureAtlas.SpriteInfo spriteInfo)
        {
            if (spriteInfo == null)
                return Vector2.zero;

            return thumbSize;
        }
        protected virtual Vector2 GetThumbRenderOffset(Vector2 renderSize)
        {
            var offset = pivot.TransformToUpperLeft(size, arbitraryPivotOffset);

            offset.x += thumbPosition.x;
            offset.y -= thumbPosition.y;

            return offset;
        }

        #endregion
    }
}
