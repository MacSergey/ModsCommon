using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModsCommon.UI
{
    public class CustomUIScrollablePanel : UIComponent, IAutoLayoutPanel
    {
        private Vector3 positionBefore;
        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout()
        {
            if ((relativePosition - positionBefore).sqrMagnitude > 0.001)
                relativePosition = positionBefore;
        }

        public const float kMaxScroll = int.MaxValue;


        public event Action<float> OnScrollPositionChanged;


        private bool initialized;

        private bool resetNeeded;

        private bool scrolling;

        private bool isMouseDown;

        public override bool canFocus => (isEnabled && isVisible) || base.canFocus;

        public CustomUIScrollablePanel()
        {
            clipChildren = true;
            builtinKeyNavigation = true;

            if (maximumSize == Vector2.zero)
            {
                Camera camera = GetCamera();
                maximumSize = new Vector3(camera.pixelWidth / 2, camera.pixelHeight / 2);
            }
        }

        private void Initialize()
        {
            if (!initialized)
            {
                initialized = true;

                if (Scrollbar != null)
                    Scrollbar.OnScrollValueChanged += ScrollbarValueChanged;

                if (resetNeeded || AutoLayout != AutoLayout.Disabled || AutoReset)
                    Reset();

                Invalidate();
                if (AutoReset)
                {
                    if (AutoLayoutStart == LayoutStart.TopLeft)
                        ScrollPosition = 0f;
                    else if (AutoLayoutStart == LayoutStart.BottomLeft)
                        ScrollPosition = kMaxScroll;
                }

                UpdateScrollbarValues();
                UpdateScrollbarSizeAndPosition();
            }
        }
        public override void OnEnable()
        {
            base.OnEnable();

            if (AutoLayout != AutoLayout.Disabled)
                AutoArrange();

            UpdateScrollbarValues();
            UpdateScrollbarSizeAndPosition();
        }
        public override void Update()
        {
            base.Update();

            if (UseScrollMomentum && !isMouseDown && scrollMomentum != 0f)
                ScrollPosition += scrollMomentum;

            if (m_IsComponentInvalidated && AutoLayout != AutoLayout.Disabled && !IsLayoutSuspended && isVisible)
            {
                AutoArrange();
                UpdateScrollbarValues();
                UpdateScrollbarSizeAndPosition();
            }

            scrollMomentum *= 0.95f - Time.deltaTime;
            if (scrollMomentum < 0.1f)
                scrollMomentum = 0f;
        }
        public override void LateUpdate()
        {
            base.LateUpdate();

            Initialize();
            if (resetNeeded && isVisible)
            {
                resetNeeded = false;
                if (AutoReset || AutoLayout != AutoLayout)
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

        #endregion

        #region LAYOUT

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


        protected LayoutStart autoLayoutStart;
        public LayoutStart AutoLayoutStart
        {
            get => autoLayoutStart;
            set
            {
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


        protected bool autoLayoutCenter;
        public bool AutoLayoutCenter
        {
            get => autoLayoutCenter;
            set
            {
                if (value != autoLayoutCenter)
                {
                    autoLayoutCenter = value;
                    Reset();
                }
            }
        }


        protected bool autoFitChildren;
        public bool AutoFitChildren
        {
            get => autoFitChildren;
            set
            {
                if (value != autoFitChildren)
                {
                    autoFitChildren = value;
                    Reset();
                }
            }
        }


        protected bool autoFillChildren;
        public bool AutoFillChildren
        {
            get => autoFillChildren;
            set
            {
                if(value != autoFillChildren)
                {
                    autoFillChildren = value;
                    Reset();
                }
            }
        }


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
                {
                    if (AutoReset)
                        ScrollPosition = 0f;

                    AutoArrange();
                }
                else
                {
                    var minPos = (Vector3)CalculateMinChildPosition();
                    for (int i = 0; i < m_ChildComponents.Count; i += 1)
                    {
                        if (!ignoreList.Contains(m_ChildComponents[i]))
                            m_ChildComponents[i].relativePosition -= minPos;
                    }
                }

                if (AutoReset)
                {
                    if (AutoLayoutStart == LayoutStart.TopLeft)
                        ScrollPosition = 0f;
                    else if (AutoLayoutStart == LayoutStart.BottomLeft)
                        ScrollPosition = kMaxScroll;
                }

                Invalidate();
                UpdateScrollbarValues();
                UpdateScrollbarSizeAndPosition();
            }
        }
        private void AutoArrange()
        {
            try
            {
                StopLayout();

                FitChildren(AutoFitChildren && ScrollOrientation == UIOrientation.Horizontal, AutoFitChildren && ScrollOrientation == UIOrientation.Vertical);

                var offset = Vector2.zero;
                var padding = Padding;
                var scrollbarSize = VisibleScrollbarSize;

                if (AutoLayoutStart.StartLeft())
                    offset.x = padding.left;
                else if (AutoLayoutStart.StartRight())
                    offset.x = padding.right;

                if (AutoLayoutStart.StartTop())
                    offset.y = padding.top;
                else if (AutoLayoutStart.StartBottom())
                    offset.y = padding.bottom;

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

                    if (ignoreList.Contains(child) || !child.isVisible || !child.enabled || !child.gameObject.activeSelf)
                        continue;

                    var childPos = ScrollOrientation switch
                    {
                        UIOrientation.Horizontal => new Vector2(ScrollPosition, 0f),
                        UIOrientation.Vertical => new Vector2(0f, -ScrollPosition),
                        _ => Vector2.zero
                    };
                    var childSize = child.size;

                    switch (AutoLayout)
                    {
                        case AutoLayout.Horizontal:
                            if (AutoFillChildren && scrollOrientation == UIOrientation.Horizontal)
                                childSize.y = height - padding.vertical - scrollbarSize;

                            if (AutoLayoutStart.StartRight())
                                childPos.x += width - offset.x - childSize.x;
                            else
                                childPos.x += offset.x;

                            if (AutoLayoutCenter)
                                childPos.y += offset.y + (height - scrollbarSize - padding.vertical - childSize.y) * 0.5f;
                            else if (AutoLayoutStart.StartBottom())
                                childPos.y += height - scrollbarSize - offset.y - childSize.y;
                            else
                                childPos.y += offset.y;

                            offset.x += childSize.x + AutoLayoutSpace;
                            break;

                        case AutoLayout.Vertical:
                            if (AutoFillChildren && scrollOrientation == UIOrientation.Vertical)
                                childSize.x = width - padding.horizontal - scrollbarSize;

                            if (AutoLayoutStart.StartBottom())
                                childPos.y += height - offset.y - childSize.y;
                            else
                                childPos.y += offset.y;

                            if (AutoLayoutCenter)
                                childPos.x += offset.x + (width - scrollbarSize - padding.horizontal - childSize.x) * 0.5f;
                            else if (AutoLayoutStart.StartRight())
                                childPos.x += width - scrollbarSize - offset.x - childSize.x;
                            else
                                childPos.x += offset.x;

                            offset.y += childSize.y + AutoLayoutSpace;
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
                    var min = float.MaxValue;
                    var max = float.MinValue;

                    for (int i = 0; i < childCount; i++)
                    {
                        var child = m_ChildComponents[i];

                        if (child.isVisibleSelf && !ignoreList.Contains(child))
                        {
                            var minPos = Mathf.RoundToInt(child.relativePosition.x);
                            var maxPos = minPos + Mathf.RoundToInt(child.width);
                            min = Mathf.Min(minPos, min);
                            max = Mathf.Max(maxPos, max);
                        }
                    }

                    return max - min;

                case AutoLayout.Horizontal:
                    var totalWidth = 0f;
                    var count = 0;
                    for (int i = 0; i < childCount; i += 1)
                    {
                        var child = m_ChildComponents[i];
                        if (child.isVisibleSelf && !ignoreList.Contains(child))
                        {
                            count += 1;
                            totalWidth += child.width;
                        }
                    }
                    return padding.horizontal + totalWidth + Math.Max(0, count - 1) * AutoLayoutSpace;

                case AutoLayout.Vertical:
                    var maxWidth = 0f;
                    for (int i = 0; i < childCount; i += 1)
                    {
                        var child = m_ChildComponents[i];
                        if (child.isVisibleSelf && !ignoreList.Contains(child))
                            maxWidth = Mathf.Max(maxWidth, child.width);
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
                    var min = float.MaxValue;
                    var max = float.MinValue;

                    for (int i = 0; i < childCount; i++)
                    {
                        var child = m_ChildComponents[i];

                        if (child.isVisibleSelf && !ignoreList.Contains(child))
                        {
                            var minPos = Mathf.RoundToInt(child.relativePosition.y);
                            var maxPos = minPos + Mathf.RoundToInt(child.height);
                            min = Mathf.Min(minPos, min);
                            max = Mathf.Max(maxPos, max);
                        }
                    }

                    return max - min;

                case AutoLayout.Vertical:
                    var totalHeight = 0f;
                    var count = 0;
                    for (int i = 0; i < childCount; i += 1)
                    {
                        var child = m_ChildComponents[i];
                        if (child.isVisibleSelf && !ignoreList.Contains(child))
                        {
                            count += 1;
                            totalHeight += child.height;
                        }
                    }
                    return padding.vertical + totalHeight + Math.Max(0, count - 1) * AutoLayoutSpace;

                case AutoLayout.Horizontal:
                    var maxHeight = 0f;
                    for (int i = 0; i < childCount; i += 1)
                    {
                        var child = m_ChildComponents[i];
                        if (child.isVisibleSelf && !ignoreList.Contains(child))
                            maxHeight = Mathf.Max(maxHeight, child.height);
                    }
                    return padding.vertical + maxHeight;

                default:
                    return 0f;
            }
        }

        private Vector2 CalculateMinChildPosition()
        {
            var min = new Vector2(float.MaxValue, float.MaxValue);

            for (int i = 0; i < childCount; i++)
            {
                var item = m_ChildComponents[i];
                if (item.enabled && item.gameObject.activeSelf && !ignoreList.Contains(item))
                {
                    min = Vector2.Min(min, item.relativePosition.XY().FloorToInt());
                }
            }

            return min;
        }

        #endregion

        #region SCROLLING

        protected CustomUIScrollbar scrollbar;
        public CustomUIScrollbar Scrollbar
        {
            get
            {
                if(scrollbar == null)
                {
                    scrollbar = AddUIComponent<CustomUIScrollbar>();
                    ignoreList.Add(scrollbar);
                    UpdateScrollbarSizeAndPosition();
                }

                return scrollbar;
            }
        }


        private bool showScroll = true;
        public bool ShowScroll
        {
            get => showScroll;
            set
            {
                if (value != showScroll)
                {
                    showScroll = value;
                    if (value)
                    {
                        Scrollbar.AutoHide = true;
                    }
                    else
                    {
                        Scrollbar.AutoHide = false;
                        Scrollbar.Hide();
                    }

                    Invalidate();
                }
            }
        }


        protected UIOrientation scrollOrientation = UIOrientation.Vertical;
        public UIOrientation ScrollOrientation
        {
            get => scrollOrientation;
            set
            {
                if (value != scrollOrientation)
                {
                    scrollOrientation = value;
                    Invalidate();
                }
            }
        }

        public float scrollbarSize = 10;
        public float ScrollbarSize
        {
            get => scrollbarSize;
            set
            {
                if (value != scrollbarSize)
                {
                    scrollbarSize = value;
                    Invalidate();
                }
            }
        }
        public float VisibleScrollbarSize => Scrollbar.isVisible ? ScrollbarSize : 0f;


        protected bool autoReset = false;
        public bool AutoReset
        {
            get => autoReset;
            set
            {
                if (value != autoReset)
                {
                    autoReset = value;
                    if (value)
                        Reset();
                }
            }
        }


        protected float scrollPosition = 0f;
        public float ScrollPosition
        {
            get => scrollPosition;
            set
            {
                switch (ScrollOrientation)
                {
                    case UIOrientation.Horizontal:
                        var horizontalSpace = GetHorizontalItemsSpace();
                        value = Mathf.Clamp(value, 0f, horizontalSpace - width);
                        break;
                    case UIOrientation.Vertical:
                        var verticalSpace = GetVerticalItemsSpace();
                        value = Mathf.Clamp(value, 0f, verticalSpace - height);
                        break;
                }

                value = Mathf.RoundToInt(value);

                if (Mathf.Abs(value - scrollPosition) > float.Epsilon)
                {
                    var delta = value - scrollPosition;
                    scrollPosition = value;
                    ScrollChildControls(delta);
                    UpdateScrollbarValues();
                }

                ScrollPositionChanged();
            }
        }


        protected int scrollWheelAmount = 10;
        public int ScrollWheelAmount
        {
            get => scrollWheelAmount;
            set => scrollWheelAmount = value;
        }


        protected bool scrollWithArrowKeys;
        public bool ScrollWithArrowKeys
        {
            get => scrollWithArrowKeys;
            set => scrollWithArrowKeys = value;
        }


        private float scrollMomentum = 0f;
        protected bool useScrollMomentum;
        public bool UseScrollMomentum
        {
            get => useScrollMomentum;
            set
            {
                useScrollMomentum = value;
                scrollMomentum = 0f;
            }
        }


        protected bool useTouchMouseScroll;
        public bool UseTouchMouseScroll
        {
            get => useTouchMouseScroll;
            set => useTouchMouseScroll = value;
        }

        private Vector2 touchStartPosition = Vector2.zero;

        private void ScrollbarValueChanged(float value)
        {
            ScrollPosition = value;
        }

        public void ScrollToBegin() => ScrollPosition = 0f;
        public void ScrollToEnd() => ScrollPosition = kMaxScroll;

        public void ScrollIntoView(UIComponent item)
        {
            var itemPosition = Vector3.zero;
            for (var current = item; current != this; current = current.parent)
            {
                if (current == null)
                    return;
                else
                    itemPosition += current.relativePosition;
            }

            var viewRect = (ScrollOrientation switch
            {
                UIOrientation.Horizontal => new Rect(ScrollPosition, 0f, width, height - Scrollbar.height),
                UIOrientation.Vertical => new Rect(0f, ScrollPosition, width - Scrollbar.width, height),
                _ => new Rect(0f, 0f, width, height),
            }).RoundToInt();

            var itemRect = (ScrollOrientation switch
            {
                UIOrientation.Horizontal => new Rect(itemPosition.x + ScrollPosition, itemPosition.y, item.width, item.height),
                UIOrientation.Vertical => new Rect(itemPosition.x, itemPosition.y + ScrollPosition, item.width, item.height),
                _ => new Rect(itemPosition.x, itemPosition.y, item.width, item.height),
            }).RoundToInt();

            if (!viewRect.Intersects(itemRect))
            {
                switch (ScrollOrientation)
                {
                    case UIOrientation.Horizontal:
                        if (itemRect.xMin < viewRect.xMin)
                            ScrollPosition = itemRect.xMin;
                        else if (itemRect.xMax > viewRect.xMax)
                            ScrollPosition = itemRect.xMax - Mathf.Max(item.width, width);
                        break;
                    case UIOrientation.Vertical:
                        if (itemRect.y < viewRect.y)
                            ScrollPosition = itemRect.yMin;
                        else if (itemRect.yMax > viewRect.yMax)
                            ScrollPosition = itemRect.yMax - Mathf.Max(item.height, height);
                        break;
                }
            }
        }

        private void ScrollChildControls(float delta)
        {
            try
            {
                scrolling = true;
                for (int i = 0; i < childCount; i++)
                {
                    var child = m_ChildComponents[i];

                    if (ignoreList.Contains(child))
                        continue;

                    var position = child.position;
                    switch (ScrollOrientation)
                    {
                        case UIOrientation.Horizontal:
                            position.x -= delta;
                            break;
                        case UIOrientation.Vertical:
                            position.y -= delta;
                            break;
                    }

                    child.position = position.RoundToInt();
                }
            }
            finally
            {
                scrolling = false;
            }
        }
        private void UpdateScrollbarValues()
        {
            if (Scrollbar is CustomUIScrollbar scrollbar)
            {
                switch (ScrollOrientation)
                {
                    case UIOrientation.Horizontal:
                        scrollbar.MinValue = 0f;
                        scrollbar.MaxValue = GetHorizontalItemsSpace();
                        scrollbar.ScrollSize = width;
                        scrollbar.Value = Mathf.Max(0f, ScrollPosition);
                        break;
                    case UIOrientation.Vertical:
                        scrollbar.MinValue = 0f;
                        scrollbar.MaxValue = GetVerticalItemsSpace();
                        scrollbar.ScrollSize = height;
                        scrollbar.Value = Mathf.Max(0f, ScrollPosition);
                        break;
                }
            }
        }
        private void UpdateScrollbarSizeAndPosition()
        {
            if (this.scrollbar is CustomUIScrollbar scrollbar)
            {
                switch (ScrollOrientation)
                {
                    case UIOrientation.Horizontal:
                        scrollbar.relativePosition = new Vector3(0f, height - ScrollbarSize);
                        scrollbar.size = new Vector2(width, ScrollbarSize);
                        scrollbar.Orientation = UIOrientation.Horizontal;
                        break;
                    case UIOrientation.Vertical:
                        scrollbar.relativePosition = new Vector3(width - ScrollbarSize, 0f);
                        scrollbar.size = new Vector2(ScrollbarSize, height);
                        scrollbar.Orientation = UIOrientation.Vertical;
                        break;
                }
            }
        }

        #endregion

        #region HANDLERS

        protected override void OnComponentAdded(UIComponent child)
        {
            base.OnComponentAdded(child);

            if (child is CustomUIScrollbar)
                return;

            if (child != null)
                AttachEvents(child);

            if (AutoLayout != AutoLayout.Disabled && !IsLayoutSuspended)
                AutoArrange();
        }
        protected override void OnComponentRemoved(UIComponent child)
        {
            base.OnComponentRemoved(child);

            if (child == scrollbar)
            {
                scrollbar = null;
                return;
            }

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

        private void ChildZOrderChanged(UIComponent child, int value) => ChildInvalidatedLayout();
        private void ChildIsVisibleChanged(UIComponent child, bool value) => ChildInvalidatedLayout();
        private void ChildInvalidated(UIComponent child, Vector2 value) => ChildInvalidatedLayout();
        private void ChildInvalidatedLayout()
        {
            if (!scrolling && !IsLayoutSuspended)
            {
                if (AutoLayout != AutoLayout.Disabled)
                    AutoArrange();

                UpdateScrollbarValues();
                Invalidate();
            }
        }

        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();
            if (isVisible && (AutoReset || AutoLayout != AutoLayout.Disabled))
            {
                Reset();
                UpdateScrollbarValues();
                UpdateScrollbarSizeAndPosition();
            }
        }
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            if (AutoReset || AutoLayout != AutoLayout.Disabled)
            {
                Reset();
            }
            else
            {
                var minPos = CalculateMinChildPosition();
                switch (ScrollOrientation)
                {
                    case UIOrientation.Horizontal:
                        if (minPos.x > 0)
                        {
                            ScrollChildControls(minPos.x);
                        }
                        break;
                    case UIOrientation.Vertical:
                        if (minPos.y > 0)
                        {
                            ScrollChildControls(minPos.y);
                        }
                        break;
                }

                UpdateScrollbarValues();
                UpdateScrollbarSizeAndPosition();
            }
        }

        protected override void OnResolutionChanged(Vector2 previousResolution, Vector2 currentResolution)
        {
            base.OnResolutionChanged(previousResolution, currentResolution);
            resetNeeded = true;
        }
        protected override void OnGotFocus(UIFocusEventParameter p)
        {
            base.OnGotFocus(p);
            ScrollIntoView(p.source);
        }

        protected override void OnKeyDown(UIKeyEventParameter p)
        {
            if (builtinKeyNavigation && ScrollWithArrowKeys && !p.used)
            {
                var increment = Scrollbar.Increment;

                switch (ScrollOrientation)
                {
                    case UIOrientation.Horizontal when p.keycode == KeyCode.LeftArrow:
                        ScrollPosition -= increment;
                        p.Use();
                        break;
                    case UIOrientation.Horizontal when p.keycode == KeyCode.RightArrow:
                        ScrollPosition += increment;
                        p.Use();
                        break;
                    case UIOrientation.Vertical when p.keycode == KeyCode.UpArrow:
                        ScrollPosition -= increment;
                        p.Use();
                        break;
                    case UIOrientation.Vertical when p.keycode == KeyCode.DownArrow:
                        ScrollPosition += increment;
                        p.Use();
                        break;
                    default:
                        base.OnKeyDown(p);
                        break;
                }
            }
            else
                base.OnKeyDown(p);
        }
        protected override void OnMouseEnter(UIMouseEventParameter p)
        {
            base.OnMouseEnter(p);
            touchStartPosition = p.position;
        }
        protected override void OnMouseDown(UIMouseEventParameter p)
        {
            base.OnMouseDown(p);
            touchStartPosition = p.position;
            isMouseDown = true;
        }
        protected override void OnDragEnd(UIDragEventParameter p)
        {
            base.OnDragEnd(p);
            isMouseDown = false;
        }
        protected override void OnMouseUp(UIMouseEventParameter p)
        {
            base.OnMouseUp(p);
            isMouseDown = false;
        }
        protected override void OnMouseMove(UIMouseEventParameter p)
        {
            base.OnMouseMove(p);
            if (UseTouchMouseScroll && isMouseDown && (p.position - touchStartPosition).magnitude > 5f)
            {
                var delta = p.moveDelta.Scale(-1f, 1f);
                switch (ScrollOrientation)
                {
                    case UIOrientation.Horizontal:
                        ScrollPosition += delta.x;
                        scrollMomentum = (scrollMomentum + delta.x) * 0.5f;
                        break;
                    case UIOrientation.Vertical:
                        ScrollPosition += delta.y;
                        scrollMomentum = (scrollMomentum + delta.y) * 0.5f;
                        break;

                }
            }
        }
        protected override void OnMouseWheel(UIMouseEventParameter p)
        {
            if (builtinKeyNavigation)
            {
                if (p.used)
                    return;

                var increment = Scrollbar.Increment;
                switch (ScrollOrientation)
                {
                    case UIOrientation.Horizontal:
                        ScrollPosition = ScrollPosition - increment * p.wheelDelta;
                        scrollMomentum = -increment * p.wheelDelta;
                        break;
                    case UIOrientation.Vertical:
                        ScrollPosition = ScrollPosition - increment * p.wheelDelta;
                        scrollMomentum = -increment * p.wheelDelta;
                        break;
                }

                p.Use();
            }

            base.OnMouseWheel(p);
        }
        protected void ScrollPositionChanged()
        {
            Invalidate();
            OnScrollPositionChanged?.Invoke(ScrollPosition);
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
                    color = isEnabled ? NormalBgColor : DisabledBgColor,
                    fillAmount = 1f,
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
                    color = isEnabled ? NormalFgColor : DisabledFgColor,
                    fillAmount = 1f,
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

                //var padding = Padding;
                //corners[0] += right * padding.left * ratio + down * padding.top * ratio;
                //corners[1] += left * padding.right * ratio + down * padding.top * ratio;
                //corners[2] += right * padding.left * ratio + up * padding.bottom * ratio;

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
