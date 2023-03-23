using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModsCommon.UI
{
    public class CustomUIPanel : UIComponent, IAutoLayoutPanel
    {
        private Vector3 positionBefore;
        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout()
        {
            if ((relativePosition - positionBefore).sqrMagnitude > 0.001)
                relativePosition = positionBefore;
        }

        private bool initialized;
        private bool resetNeeded;

        private void Initialize()
        {
            if (!initialized)
            {
                initialized = true;
                if ((resetNeeded || AutoLayout != AutoLayout.Disabled) && !IsLayoutSuspended)
                    Reset();

                Invalidate();
            }
        }
        public override void OnEnable()
        {
            base.OnEnable();

            if (AutoLayout != AutoLayout.Disabled && !IsLayoutSuspended)
                AutoArrange();
        }
        public override void OnDisable()
        {
            BgRenderData = null;
            FgRenderData = null;

            base.OnDisable();
        }
        public override void Update()
        {
            base.Update();

            if (m_IsComponentInvalidated && AutoLayout != AutoLayout.Disabled && !IsLayoutSuspended/* && isVisible*/)
                AutoArrange();
        }
        public override void LateUpdate()
        {
            base.LateUpdate();

            Initialize();
            if (resetNeeded && isVisible)
            {
                resetNeeded = false;
                Reset();
            }
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

        protected UITextureAtlas atlasBackground;
        public UITextureAtlas AtlasBackground
        {
            get => atlasBackground ?? Atlas;
            set
            {
                if (!Equals(value, atlasBackground))
                {
                    atlasBackground = value;
                    Invalidate();
                }
            }
        }

        protected UITextureAtlas atlasForeground;
        public UITextureAtlas AtlasForeground
        {
            get => atlasForeground ?? Atlas;
            set
            {
                if (!Equals(value, atlasForeground))
                {
                    atlasForeground = value;
                    Invalidate();
                }
            }
        }

        Color32? normalBgColor;
        public Color32 NormalBgColor
        {
            get => normalBgColor ?? base.color;
            set
            {
                normalBgColor = value;
                Invalidate();
            }
        }


        Color32? hoveredBgColor;
        public Color32 HoveredBgColor
        {
            get => hoveredBgColor ?? NormalBgColor;
            set
            {
                hoveredBgColor = value;
                Invalidate();
            }
        }


        Color32? disabledBgColor;
        public Color32 DisabledBgColor
        {
            get => disabledBgColor ?? base.disabledColor;
            set
            {
                disabledBgColor = value;
                Invalidate();
            }
        }


        Color32? normalFgColor;
        public Color32 NormalFgColor
        {
            get => normalFgColor ?? base.color;
            set
            {
                normalFgColor = value;
                Invalidate();
            }
        }


        Color32? hoveredFgColor;
        public Color32 HoveredFgColor
        {
            get => hoveredFgColor ?? NormalFgColor;
            set
            {
                hoveredFgColor = value;
                Invalidate();
            }
        }


        Color32? disabledFgColor;
        public Color32 DisabledFgColor
        {
            get => disabledFgColor ?? base.disabledColor;
            set
            {
                disabledFgColor = value;
                Invalidate();
            }
        }


        string backgroundSprite;
        public string BackgroundSprite
        {
            get => backgroundSprite;
            set
            {
                if (value != backgroundSprite)
                {
                    backgroundSprite = value;
                    Invalidate();
                }
            }
        }


        string foregroundSprite;
        public string ForegroundSprite
        {
            get => foregroundSprite;
            set
            {
                if (value != foregroundSprite)
                {
                    foregroundSprite = value;
                    Invalidate();
                }
            }
        }


        private RectOffset spritePadding;
        public RectOffset SpritePadding
        {
            get => spritePadding ??= new RectOffset();
            set
            {
                if (!Equals(value, spritePadding))
                {
                    spritePadding = value;
                    Invalidate();
                }
            }
        }



        protected UISpriteFlip spriteFlip;
        public UISpriteFlip SpriteFlip
        {
            get => spriteFlip;
            set
            {
                if (value != spriteFlip)
                {
                    spriteFlip = value;
                    Invalidate();
                }
            }
        }


        #endregion

        #region LAYOUT

        public event Action<RectOffset> OnPaddingChanged;

        protected RectOffset padding;
        public RectOffset Padding
        {
            get => padding ??= new RectOffset();
            set
            {
                value = value.ConstrainPadding();
                if (!Equals(value, padding))
                {
                    padding = value;
                    Reset();
                    OnPaddingChanged?.Invoke(padding);
                }
            }
        }
        public int PaddingRight
        {
            get => Padding.right;
            set
            {
                var old = Padding;
                Padding = new RectOffset(old.left, value, old.top, old.bottom);
            }
        }
        public int PaddingLeft
        {
            get => Padding.left;
            set
            {
                var old = Padding;
                Padding = new RectOffset(value, old.right, old.top, old.bottom);
            }
        }
        public int PaddingTop
        {
            get => Padding.top;
            set
            {
                var old = Padding;
                Padding = new RectOffset(old.left, old.right, value, old.bottom);
            }
        }
        public int PaddingButtom
        {
            get => Padding.bottom;
            set
            {
                var old = Padding;
                Padding = new RectOffset(old.left, old.right, old.top, value);
            }
        }


        protected AutoLayout autoLayout;
        public AutoLayout AutoLayout
        {
            get => autoLayout;
            set
            {
                if (value != autoLayout)
                {
                    autoLayout = value;
                    Reset();
                }
            }
        }


        protected LayoutStart autoLayoutStart = LayoutStart.TopLeft;
        public LayoutStart AutoLayoutStart
        {
            get => autoLayoutStart;
            set
            {
                value = value.Correct();
                if (value != autoLayoutStart)
                {
                    autoLayoutStart = value;
                    Reset();
                }
            }
        }


        protected int autoLayoutSpace;
        public int AutoLayoutSpace
        {
            get => autoLayoutSpace;
            set
            {
                if (value != autoLayoutSpace)
                {
                    autoLayoutSpace = value;
                    Reset();
                }
            }
        }


        protected bool autoCenterPadding;
        public bool AutoCenterPadding
        {
            get => autoCenterPadding && ((AutoLayout == AutoLayout.Horizontal && AutoChildrenHorizontally == AutoLayoutChildren.None) || (AutoLayout == AutoLayout.Vertical && AutoChildrenVertically == AutoLayoutChildren.None));
            set
            {
                if (value != autoCenterPadding)
                {
                    autoCenterPadding = value;
                    Reset();
                }
            }
        }


        protected AutoLayoutChildren autoChildrenHorizontally;
        public AutoLayoutChildren AutoChildrenHorizontally
        {
            get => autoChildrenHorizontally;
            set
            {
                if (value != autoChildrenHorizontally)
                {
                    autoChildrenHorizontally = value;
                    Reset();
                }
            }
        }


        protected AutoLayoutChildren autoChildrenVertically;
        public AutoLayoutChildren AutoChildrenVertically
        {
            get => autoChildrenVertically;
            set
            {
                if (value != autoChildrenVertically)
                {
                    autoChildrenVertically = value;
                    Reset();
                }
            }
        }


        private Dictionary<UIComponent, RectOffset> itemsMargin = new Dictionary<UIComponent, RectOffset>();
        public void SetItemMargin(UIComponent component, RectOffset margin)
        {
            margin = margin.ConstrainPadding();
            if (itemsMargin.TryGetValue(component, out var oldMargin) && Equals(oldMargin, margin))
                return;

            itemsMargin[component] = margin;
            Reset();
        }
        public RectOffset GetItemMargin(UIComponent component) => itemsMargin.TryGetValue(component, out var margin) ? margin : new RectOffset();


        private int layoutSuspend;
        public bool IsLayoutSuspended => layoutSuspend != 0;

        public Vector2 ItemSize => new Vector2(width - Padding.horizontal, height - Padding.vertical);
        public RectOffset LayoutPadding => Padding;

        public virtual void StopLayout()
        {
            layoutSuspend += 1;
        }
        public virtual void StartLayout(bool layoutNow = true, bool force = false)
        {
            layoutSuspend = force ? 0 : Mathf.Max(layoutSuspend - 1, 0);

            if (layoutSuspend == 0 && layoutNow)
                Reset();
        }
        public virtual void PauseLayout(Action action, bool layoutNow = true, bool force = false)
        {
            if (action != null)
            {
                try
                {
                    StopLayout();
                    action();
                }
                finally
                {
                    StartLayout(layoutNow, force);
                }
            }
        }

        private HashSet<UIComponent> ignoreList = new HashSet<UIComponent>();
        public void Ignore(UIComponent item, bool ignore)
        {
            if (ignore)
                ignoreList.Add(item);
            else
                ignoreList.Remove(item);

            Reset();
        }

        public void Reset()
        {
            if (!IsLayoutSuspended)
            {
                if (AutoLayout != AutoLayout.Disabled)
                    AutoArrange();

                Invalidate();
            }
        }

        protected virtual void AutoArrange()
        {
            try
            {
                StopLayout();

                FitChildren(AutoChildrenHorizontally == AutoLayoutChildren.Fit, AutoChildrenVertically == AutoLayoutChildren.Fit);

                var offset = Vector2.zero;
                var padding = Padding;


                switch (AutoLayoutStart & LayoutStart.Horizontal)
                {
                    case LayoutStart.Left:
                        offset.x = padding.left;
                        break;
                    case LayoutStart.Centre:
                        if (AutoLayout == AutoLayout.Horizontal && AutoChildrenHorizontally != AutoLayoutChildren.Fill)
                            offset.x = (width - GetHorizontalItemsSpace()) * 0.5f;
                        else
                            offset.x = padding.left;
                        break;
                    case LayoutStart.Right:
                        offset.x = padding.right;
                        break;
                }

                switch (AutoLayoutStart & LayoutStart.Vertical)
                {
                    case LayoutStart.Top:
                        offset.y = padding.top;
                        break;
                    case LayoutStart.Middle:
                        if (AutoLayout == AutoLayout.Vertical && AutoChildrenVertically != AutoLayoutChildren.Fill)
                            offset.y = (height - GetVerticalItemsSpace()) * 0.5f;
                        else
                            offset.y = padding.top;
                        break;
                    case LayoutStart.Bottom:
                        offset.y = padding.bottom;
                        break;
                }

                for (int i = 0; i < childCount; i += 1)
                {
                    var child = AutoLayoutStart switch
                    {
                        LayoutStart.TopLeft or LayoutStart.BottomLeft when AutoLayout == AutoLayout.Horizontal => m_ChildComponents[i],
                        LayoutStart.TopRight or LayoutStart.BottomRight when AutoLayout == AutoLayout.Horizontal => m_ChildComponents[childCount - 1 - i],
                        LayoutStart.TopLeft or LayoutStart.TopRight when AutoLayout == AutoLayout.Vertical => m_ChildComponents[i],
                        LayoutStart.BottomLeft or LayoutStart.BottomRight when AutoLayout == AutoLayout.Vertical => m_ChildComponents[childCount - 1 - i],
                        _ => m_ChildComponents[i],
                    };

                    if (ignoreList.Contains(child) || !child.isVisibleSelf || !child.enabled || !child.gameObject.activeSelf)
                        continue;

                    var childPos = Vector2.zero;
                    var childMargin = GetItemMargin(child);
                    var childSize = child.size;

                    switch (AutoLayout)
                    {
                        case AutoLayout.Horizontal:
                            if (AutoChildrenVertically == AutoLayoutChildren.Fill)
                                childSize.y = height - padding.vertical;

                            switch (AutoLayoutStart & LayoutStart.Horizontal)
                            {
                                case LayoutStart.Left:
                                    childPos.x = offset.x + childMargin.left;
                                    break;
                                case LayoutStart.Centre:

                                    break;
                                case LayoutStart.Right:
                                    childPos.x = width - offset.x - childSize.x - childMargin.right;
                                    break;
                            }

                            switch (AutoLayoutStart & LayoutStart.Vertical)
                            {
                                case LayoutStart.Top:
                                    childPos.y = offset.y + childMargin.top;
                                    break;
                                case LayoutStart.Middle:
                                    childPos.y = offset.y + childMargin.top + (height - padding.vertical - childSize.y - childMargin.vertical) * 0.5f;
                                    break;
                                case LayoutStart.Bottom:
                                    childPos.y = height - offset.y - childSize.y - childMargin.bottom;
                                    break;
                            }

                            offset.x += childSize.x + childMargin.horizontal + AutoLayoutSpace;
                            break;

                        case AutoLayout.Vertical:
                            if (AutoChildrenHorizontally == AutoLayoutChildren.Fill)
                                childSize.x = width - padding.horizontal;

                            switch (AutoLayoutStart & LayoutStart.Vertical)
                            {
                                case LayoutStart.Top:
                                    childPos.y = offset.y + childMargin.top;
                                    break;
                                case LayoutStart.Middle:

                                    break;
                                case LayoutStart.Bottom:
                                    childPos.y = height - offset.y - childSize.y - childMargin.bottom;
                                    break;
                            }

                            switch (AutoLayoutStart & LayoutStart.Horizontal)
                            {
                                case LayoutStart.Left:
                                    childPos.x = offset.x + childMargin.left;
                                    break;
                                case LayoutStart.Centre:
                                    childPos.x = offset.x + childMargin.left + (width - padding.horizontal - childSize.x - childMargin.horizontal) * 0.5f;
                                    break;
                                case LayoutStart.Right:
                                    childPos.x = width - offset.x - childSize.x - childMargin.right;
                                    break;
                            }

                            offset.y += childSize.y + childMargin.vertical + AutoLayoutSpace;
                            break;
                    }

                    child.relativePosition = childPos;
                    child.size = childSize;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                StartLayout(false);
            }
        }

        public new void FitChildrenHorizontally() => FitChildren(true, false);
        public new void FitChildrenVertically() => FitChildren(false, true);
        public new void FitChildren() => FitChildren(true, true);
        protected virtual void FitChildren(bool horizontally, bool vertically)
        {
            var newSize = size;

            if (horizontally)
                newSize.x = GetHorizontalItemsSpace();

            if (vertically)
                newSize.y = GetVerticalItemsSpace();

            size = newSize;
        }
        private float GetHorizontalItemsSpace()
        {
            var padding = Padding;

            switch (AutoLayout)
            {
                case AutoLayout.Disabled:
                    var offset = 0f;
                    for (int i = 0; i < childCount; i += 1)
                    {
                        var child = m_ChildComponents[i];
                        if (child.isVisibleSelf && !ignoreList.Contains(child))
                            offset = Mathf.Max(offset, child.relativePosition.x + child.width);
                    }
                    return Mathf.Max(offset, padding.left) + padding.right;
                case AutoLayout.Horizontal:
                    var totalWidth = 0f;
                    var count = 0;
                    for (int i = 0; i < childCount; i += 1)
                    {
                        var child = m_ChildComponents[i];
                        if (child.isVisibleSelf && !ignoreList.Contains(child))
                        {
                            count += 1;
                            totalWidth += child.width + GetItemMargin(child).horizontal;
                        }
                    }
                    return padding.horizontal + totalWidth + Math.Max(0, count - 1) * AutoLayoutSpace;
                case AutoLayout.Vertical:
                    var maxWidth = 0f;
                    for (int i = 0; i < childCount; i += 1)
                    {
                        var child = m_ChildComponents[i];
                        if (child.isVisibleSelf && !ignoreList.Contains(child))
                            maxWidth = Mathf.Max(maxWidth, child.width + GetItemMargin(child).horizontal);
                    }
                    return padding.horizontal + maxWidth;
                default:
                    return 0f;
            }
        }
        private float GetVerticalItemsSpace()
        {
            var padding = Padding;

            switch (AutoLayout)
            {
                case AutoLayout.Disabled:
                    var offset = 0f;
                    for (int i = 0; i < childCount; i += 1)
                    {
                        var child = m_ChildComponents[i];
                        if (child.isVisibleSelf && !ignoreList.Contains(child))
                            offset = Mathf.Max(offset, child.relativePosition.y + child.height);
                    }
                    return Mathf.Max(offset, padding.top) + padding.bottom;
                case AutoLayout.Vertical:
                    var totalHeight = 0f;
                    var count = 0;
                    for (int i = 0; i < childCount; i += 1)
                    {
                        var child = m_ChildComponents[i];
                        if (child.isVisibleSelf && !ignoreList.Contains(child))
                        {
                            count += 1;
                            totalHeight += child.height + GetItemMargin(child).vertical;
                        }
                    }
                    return padding.vertical + totalHeight + Math.Max(0, count - 1) * AutoLayoutSpace;
                case AutoLayout.Horizontal:
                    var maxHeight = 0f;
                    for (int i = 0; i < childCount; i += 1)
                    {
                        var child = m_ChildComponents[i];
                        if (child.isVisibleSelf && !ignoreList.Contains(child))
                            maxHeight = Mathf.Max(maxHeight, child.height + GetItemMargin(child).vertical);
                    }
                    return padding.vertical + maxHeight;
                default:
                    return 0f;
            }
        }

        #endregion

        #region HANDLERS

        protected override void OnComponentAdded(UIComponent child)
        {
            base.OnComponentAdded(child);

            if (child != null)
                AttachEvents(child);

            if (AutoLayout != AutoLayout.Disabled && !IsLayoutSuspended)
                AutoArrange();
        }

        protected override void OnComponentRemoved(UIComponent child)
        {
            base.OnComponentRemoved(child);

            if (child != null)
                DetachEvents(child);

            if (AutoLayout != AutoLayout.Disabled && !IsLayoutSuspended)
                AutoArrange();
        }

        private void AttachEvents(UIComponent child)
        {
            child.eventVisibilityChanged += ChildIsVisibleChanged;
            child.eventPositionChanged += ChildInvalidated;
            child.eventSizeChanged += ChildInvalidated;
            child.eventZOrderChanged += ChildZOrderChanged;
        }

        private void DetachEvents(UIComponent child)
        {
            child.eventVisibilityChanged -= ChildIsVisibleChanged;
            child.eventPositionChanged -= ChildInvalidated;
            child.eventSizeChanged -= ChildInvalidated;
            child.eventZOrderChanged -= ChildZOrderChanged;
        }

        private void ChildZOrderChanged(UIComponent child, int value) => Reset();
        private void ChildIsVisibleChanged(UIComponent child, bool value) => Reset();
        private void ChildInvalidated(UIComponent child, Vector2 value) => Reset();

        protected override void OnResolutionChanged(Vector2 previousResolution, Vector2 currentResolution)
        {
            base.OnResolutionChanged(previousResolution, currentResolution);
            resetNeeded = true;
        }
        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();
            if (isVisible && AutoLayout != AutoLayout.Disabled)
            {
                Reset();
            }
        }
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            if (isVisible && AutoLayout != AutoLayout.Disabled)
            {
                Reset();
            }
        }

        protected override void OnMouseEnter(UIMouseEventParameter p)
        {
            var isHovered = containsMouse;

            base.OnMouseEnter(p);

            if (containsMouse != isHovered)
                Invalidate();
        }
        protected override void OnMouseLeave(UIMouseEventParameter p)
        {
            var isHovered = containsMouse;

            base.OnMouseLeave(p);

            if (containsMouse != isHovered)
                Invalidate();
        }

        #endregion

        #region RENDER

        protected UIRenderData BgRenderData { get; set; }
        protected UIRenderData FgRenderData { get; set; }

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

            if (AtlasBackground is UITextureAtlas bgAtlas && AtlasForeground is UITextureAtlas fgAtlas)
            {
                BgRenderData.material = bgAtlas.material;
                FgRenderData.material = fgAtlas.material;

                RenderBackground();
                RenderForeground();
            }
        }
        private void RenderBackground()
        {
            if (AtlasBackground[BackgroundSprite] is UITextureAtlas.SpriteInfo backgroundSprite)
            {
                var renderOptions = new RenderOptions()
                {
                    atlas = AtlasBackground,
                    color = isEnabled ? (m_IsMouseHovering ? HoveredBgColor : NormalBgColor) : DisabledBgColor,
                    fillAmount = 1f,
                    flip = spriteFlip,
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
            if (AtlasForeground[ForegroundSprite] is UITextureAtlas.SpriteInfo foregroundSprite)
            {
                var foregroundRenderSize = GetForegroundRenderSize(foregroundSprite);
                var foregroundRenderOffset = GetForegroundRenderOffset(foregroundRenderSize);

                var renderOptions = new RenderOptions()
                {
                    atlas = AtlasForeground,
                    color = isEnabled ? (m_IsMouseHovering ? HoveredFgColor : NormalFgColor) : DisabledFgColor,
                    fillAmount = 1f,
                    flip = spriteFlip,
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

            return new Vector2(width - SpritePadding.horizontal, height - SpritePadding.vertical);
        }
        protected virtual Vector2 GetForegroundRenderOffset(Vector2 renderSize)
        {
            Vector2 result = pivot.TransformToUpperLeft(size, arbitraryPivotOffset);

            result.x += SpritePadding.left;
            result.y -= SpritePadding.top;

            return result;
        }
        protected override Plane[] GetClippingPlanes()
        {
            if (clipChildren)
            {
                var corners = GetCorners();
                var right = transform.TransformDirection(Vector3.right);
                var left = transform.TransformDirection(Vector3.left);
                var up = transform.TransformDirection(Vector3.up);
                var down = transform.TransformDirection(Vector3.down);
                var ratio = PixelsToUnits();

                var padding = Padding;
                corners[0] += right * padding.left * ratio + down * padding.top * ratio;
                corners[1] += left * padding.right * ratio + down * padding.top * ratio;
                corners[2] += left * padding.right * ratio + up * padding.bottom * ratio;

                m_CachedClippingPlanes[0] = new Plane(right, corners[0]);
                m_CachedClippingPlanes[1] = new Plane(left, corners[1]);
                m_CachedClippingPlanes[2] = new Plane(up, corners[2]);
                m_CachedClippingPlanes[3] = new Plane(down, corners[0]);

                return m_CachedClippingPlanes;
            }
            else
                return null;
        }

        #endregion
    }
}
