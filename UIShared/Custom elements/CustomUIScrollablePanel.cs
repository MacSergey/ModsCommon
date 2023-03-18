using ColossalFramework.UI;
using System;
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

        public static readonly Vector2 kMaxVerticalScroll = new Vector2(0f, 2.14748365E+09f);

        public static readonly Vector2 kMaxHorizontalScroll = new Vector2(2.14748365E+09f, 0f);

        protected UITextureAtlas m_Atlas;


        protected string m_BackgroundSprite;


        protected bool m_AutoReset = true;


        protected bool m_AutoLayout;


        protected RectOffset m_ScrollPadding;


        protected RectOffset m_AutoLayoutPadding;


        protected LayoutDirection m_AutoLayoutDirection;


        protected LayoutStart m_AutoLayoutStart;


        protected bool m_WrapLayout;


        protected bool m_Center;


        protected bool m_FreeScroll;


        protected bool m_CustomScrollBounds;


        protected Vector2 m_ScrollPosition = Vector2.zero;


        protected int m_ScrollWheelAmount = 10;


        protected CustomUIScrollbar m_HorizontalScrollbar;


        protected CustomUIScrollbar m_VerticalScrollbar;


        protected UIOrientation m_WheelDirection;


        protected bool m_ScrollWithArrowKeys;


        protected bool m_UseScrollMomentum;


        protected bool m_UseTouchMouseScroll;

        private bool m_Initialized;

        private bool m_ResetNeeded;

        private bool m_Scrolling;

        private bool m_IsMouseDown;

        private Vector2 m_TouchStartPosition = Vector2.zero;

        private Vector2 m_ScrollMomentum = Vector2.zero;

        public bool UseScrollMomentum
        {
            get
            {
                return m_UseScrollMomentum;
            }
            set
            {
                m_UseScrollMomentum = value;
                m_ScrollMomentum = Vector2.zero;
            }
        }

        public bool UseTouchMouseScroll
        {
            get
            {
                return m_UseTouchMouseScroll;
            }
            set
            {
                m_UseTouchMouseScroll = value;
            }
        }

        public bool ScrollWithArrowKeys
        {
            get
            {
                return m_ScrollWithArrowKeys;
            }
            set
            {
                m_ScrollWithArrowKeys = value;
            }
        }

        public bool FreeScroll
        {
            get
            {
                return m_FreeScroll;
            }
            set
            {
                m_FreeScroll = value;
            }
        }

        public bool CustomScrollBounds
        {
            get
            {
                return m_CustomScrollBounds;
            }
            set
            {
                m_CustomScrollBounds = value;
            }
        }

        public UITextureAtlas Atlas
        {
            get
            {
                if (m_Atlas == null)
                {
                    UIView uIView = GetUIView();
                    if (uIView != null)
                    {
                        m_Atlas = uIView.defaultAtlas;
                    }
                }

                return m_Atlas;
            }
            set
            {
                if (!UITextureAtlas.Equals(value, m_Atlas))
                {
                    m_Atlas = value;
                    Invalidate();
                }
            }
        }

        public string BackgroundSprite
        {
            get
            {
                return m_BackgroundSprite;
            }
            set
            {
                if (value != m_BackgroundSprite)
                {
                    m_BackgroundSprite = value;
                    Invalidate();
                }
            }
        }

        public bool AutoReset
        {
            get
            {
                return m_AutoReset;
            }
            set
            {
                if (value != m_AutoReset)
                {
                    m_AutoReset = value;
                    if (value)
                    {
                        Reset();
                    }
                }
            }
        }

        public RectOffset ScrollPadding
        {
            get
            {
                if (m_ScrollPadding == null)
                {
                    m_ScrollPadding = new RectOffset();
                }

                return m_ScrollPadding;
            }
            set
            {
                value = value.ConstrainPadding();
                if (!object.Equals(value, m_ScrollPadding))
                {
                    m_ScrollPadding = value;
                    Reset();
                }
            }
        }

        public bool AutoLayout
        {
            get
            {
                return m_AutoLayout;
            }
            set
            {
                if (value != m_AutoLayout)
                {
                    m_AutoLayout = value;
                    Reset();
                }
            }
        }

        public bool WrapLayout
        {
            get
            {
                return m_WrapLayout;
            }
            set
            {
                if (value != m_WrapLayout)
                {
                    m_WrapLayout = value;
                    Reset();
                }
            }
        }

        public LayoutDirection AutoLayoutDirection
        {
            get
            {
                if (!m_Center)
                {
                    return m_AutoLayoutDirection;
                }

                return LayoutDirection.Horizontal;
            }
            set
            {
                if (value != m_AutoLayoutDirection)
                {
                    m_AutoLayoutDirection = value;
                    Reset();
                }
            }
        }

        public LayoutStart AutoLayoutStart
        {
            get
            {
                return m_AutoLayoutStart;
            }
            set
            {
                if (value == LayoutStart.TopRight || value == LayoutStart.BottomRight)
                {
                    throw new NotSupportedException("Right layout start is unsupported");
                }

                if (value != m_AutoLayoutStart)
                {
                    m_AutoLayoutStart = value;
                    Reset();
                }
            }
        }

        public bool UseCenter
        {
            get
            {
                return m_Center;
            }
            set
            {
                if (value != m_Center)
                {
                    m_Center = value;
                    for (int i = 0; i < base.childCount; i++)
                    {
                        m_ChildComponents[i].PerformLayout();
                    }

                    Reset();
                }
            }
        }

        public RectOffset AutoLayoutPadding
        {
            get
            {
                if (m_AutoLayoutPadding == null)
                {
                    m_AutoLayoutPadding = new RectOffset();
                }

                return m_AutoLayoutPadding;
            }
            set
            {
                value = value.ConstrainPadding();
                if (!object.Equals(value, m_AutoLayoutPadding))
                {
                    m_AutoLayoutPadding = value;
                    Reset();
                }
            }
        }

        public Vector2 ScrollPosition
        {
            get
            {
                return m_ScrollPosition;
            }
            set
            {
                if (!m_FreeScroll)
                {
                    Vector2 vector = CalculateViewSize();
                    Vector2 vector2 = new Vector2(base.size.x - (float)ScrollPadding.horizontal, base.size.y - (float)ScrollPadding.vertical);
                    value = Vector2.Min(vector - vector2, value);
                    value = Vector2.Max(Vector2.zero, value);
                    value = value.RoundToInt();
                }

                if ((value - m_ScrollPosition).sqrMagnitude > float.Epsilon)
                {
                    Vector2 vector3 = value - m_ScrollPosition;
                    m_ScrollPosition = value;
                    ScrollChildControls(vector3, m_FreeScroll);
                    UpdateScrollbars();
                }

                OnScrollPositionChanged();
            }
        }

        public int ScrollWheelAmount
        {
            get
            {
                return m_ScrollWheelAmount;
            }
            set
            {
                m_ScrollWheelAmount = value;
            }
        }

        public CustomUIScrollbar HorizontalScrollbar
        {
            get
            {
                return m_HorizontalScrollbar;
            }
            set
            {
                m_HorizontalScrollbar = value;
                UpdateScrollbars();
            }
        }

        public CustomUIScrollbar VerticalScrollbar
        {
            get
            {
                return m_VerticalScrollbar;
            }
            set
            {
                m_VerticalScrollbar = value;
                UpdateScrollbars();
            }
        }

        public UIOrientation ScrollWheelDirection
        {
            get
            {
                return m_WheelDirection;
            }
            set
            {
                m_WheelDirection = value;
            }
        }

        public override bool canFocus
        {
            get
            {
                if (base.isEnabled && base.isVisible)
                {
                    return true;
                }

                return base.canFocus;
            }
        }

        public event PropertyChangedEventHandler<Vector2> eventScrollPositionChanged;

        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();
            if (base.isVisible && (AutoReset || AutoLayout))
            {
                Reset();
                UpdateScrollbars();
            }
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            if (AutoReset || AutoLayout)
            {
                Reset();
                return;
            }

            Vector2 lhs = CalculateMinChildPosition();
            if (lhs.x > (float)ScrollPadding.left || lhs.y > (float)ScrollPadding.top)
            {
                lhs -= new Vector2(ScrollPadding.left, ScrollPadding.top);
                lhs = Vector2.Max(lhs, Vector2.zero);
                ScrollChildControls(lhs);
            }

            UpdateScrollbars();
        }

        protected override void OnResolutionChanged(Vector2 previousResolution, Vector2 currentResolution)
        {
            base.OnResolutionChanged(previousResolution, currentResolution);
            m_ResetNeeded = true;
        }

        protected override void OnGotFocus(UIFocusEventParameter p)
        {
            base.OnGotFocus(p);
            UIComponent source = p.source;
            while (source != null)
            {
                if (m_ChildComponents.Contains(source))
                {
                    ScrollIntoView(source);
                    break;
                }

                source = source.parent;
            }
        }

        protected override void OnKeyDown(UIKeyEventParameter p)
        {
            if (base.builtinKeyNavigation)
            {
                if (!ScrollWithArrowKeys || p.used)
                {
                    base.OnKeyDown(p);
                    return;
                }

                float num = ((HorizontalScrollbar != null) ? HorizontalScrollbar.Increment : 1f);
                float num2 = ((VerticalScrollbar != null) ? VerticalScrollbar.Increment : 1f);
                if (p.keycode == KeyCode.LeftArrow)
                {
                    ScrollPosition += new Vector2(0f - num, 0f);
                    p.Use();
                }
                else if (p.keycode == KeyCode.RightArrow)
                {
                    ScrollPosition += new Vector2(num, 0f);
                    p.Use();
                }
                else if (p.keycode == KeyCode.UpArrow)
                {
                    ScrollPosition += new Vector2(0f, 0f - num2);
                    p.Use();
                }
                else if (p.keycode == KeyCode.DownArrow)
                {
                    ScrollPosition += new Vector2(0f, num2);
                    p.Use();
                }
            }

            base.OnKeyDown(p);
        }

        protected override void OnMouseEnter(UIMouseEventParameter p)
        {
            base.OnMouseEnter(p);
            m_TouchStartPosition = p.position;
        }

        protected override void OnMouseDown(UIMouseEventParameter p)
        {
            base.OnMouseDown(p);
            m_TouchStartPosition = p.position;
            m_IsMouseDown = true;
        }

        protected override void OnDragEnd(UIDragEventParameter p)
        {
            base.OnDragEnd(p);
            m_IsMouseDown = false;
        }

        protected override void OnMouseUp(UIMouseEventParameter p)
        {
            base.OnMouseUp(p);
            m_IsMouseDown = false;
        }

        protected override void OnMouseMove(UIMouseEventParameter p)
        {
            base.OnMouseMove(p);
            if (UseTouchMouseScroll && m_IsMouseDown && (p.position - m_TouchStartPosition).magnitude > 5f)
            {
                Vector2 vector = p.moveDelta.Scale(-1f, 1f);
                ScrollPosition += vector;
                m_ScrollMomentum = (m_ScrollMomentum + vector) * 0.5f;
            }
        }

        protected override void OnMouseWheel(UIMouseEventParameter p)
        {
            if (base.builtinKeyNavigation)
            {
                if (p.used)
                {
                    return;
                }

                float num = ((ScrollWheelDirection != 0) ? ((VerticalScrollbar != null) ? VerticalScrollbar.Increment : ((float)ScrollWheelAmount)) : ((HorizontalScrollbar != null) ? HorizontalScrollbar.Increment : ((float)ScrollWheelAmount)));
                if (ScrollWheelDirection == UIOrientation.Horizontal)
                {
                    ScrollPosition = new Vector2(ScrollPosition.x - num * p.wheelDelta, ScrollPosition.y);
                    m_ScrollMomentum = new Vector2((0f - num) * p.wheelDelta, 0f);
                }
                else
                {
                    ScrollPosition = new Vector2(ScrollPosition.x, ScrollPosition.y - num * p.wheelDelta);
                    m_ScrollMomentum = new Vector2(0f, (0f - num) * p.wheelDelta);
                }

                p.Use();
                Invoke("OnMouseWheel", p);
            }

            base.OnMouseWheel(p);
        }

        protected override void OnComponentAdded(UIComponent child)
        {
            base.OnComponentAdded(child);
            AttachEvents(child);
            if (AutoLayout)
            {
                AutoArrange();
            }
        }

        protected override void OnComponentRemoved(UIComponent child)
        {
            base.OnComponentRemoved(child);
            if (child != null)
            {
                DetachEvents(child);
            }

            if (AutoLayout)
            {
                AutoArrange();
            }
        }

        protected void OnScrollPositionChanged()
        {
            Invalidate();
            if (this.eventScrollPositionChanged != null)
            {
                this.eventScrollPositionChanged(this, ScrollPosition);
            }

            InvokeUpward("OnScrollPositionChanged", ScrollPosition);
        }

        protected override Plane[] GetClippingPlanes()
        {
            if (!base.clipChildren)
            {
                return null;
            }

            Vector3[] corners = GetCorners();
            Vector3 vector = base.transform.TransformDirection(Vector3.right);
            Vector3 vector2 = base.transform.TransformDirection(Vector3.left);
            Vector3 vector3 = base.transform.TransformDirection(Vector3.up);
            Vector3 vector4 = base.transform.TransformDirection(Vector3.down);
            float num = PixelsToUnits();
            RectOffset rectOffset = ScrollPadding;
            corners[0] += vector * rectOffset.left * num + vector4 * rectOffset.top * num;
            corners[1] += vector2 * rectOffset.right * num + vector4 * rectOffset.top * num;
            corners[2] += vector * rectOffset.left * num + vector3 * rectOffset.bottom * num;
            ref Plane reference = ref m_CachedClippingPlanes[0];
            reference = new Plane(vector, corners[0]);
            ref Plane reference2 = ref m_CachedClippingPlanes[1];
            reference2 = new Plane(vector2, corners[1]);
            ref Plane reference3 = ref m_CachedClippingPlanes[2];
            reference3 = new Plane(vector3, corners[2]);
            ref Plane reference4 = ref m_CachedClippingPlanes[3];
            reference4 = new Plane(vector4, corners[0]);
            return m_CachedClippingPlanes;
        }

        protected override void OnRebuildRenderData()
        {
            if (Atlas == null || string.IsNullOrEmpty(BackgroundSprite))
            {
                return;
            }

            UITextureAtlas.SpriteInfo spriteInfo = Atlas[BackgroundSprite];
            if (!(spriteInfo == null))
            {
                base.renderData.material = Atlas.material;

                var renderOptions = new RenderOptions()
                {
                    atlas = Atlas,
                    color = ApplyOpacity(base.isEnabled ? base.color : base.disabledColor),
                    fillAmount = 1f,
                    flip = UISpriteFlip.None,
                    offset = pivot.TransformToUpperLeft(size, arbitraryPivotOffset),
                    pixelsToUnits = PixelsToUnits(),
                    size = size,
                    spriteInfo = spriteInfo,
                };

                if (spriteInfo.isSliced)
                    Render.RenderSlicedSprite(base.renderData, renderOptions);
                else
                    Render.RenderSprite(base.renderData, renderOptions);
            }
        }

        public void FitToContents()
        {
            if (base.childCount != 0)
            {
                Vector2 vector = Vector2.zero;
                for (int i = 0; i < base.childCount; i++)
                {
                    UIComponent uIComponent = m_ChildComponents[i];
                    Vector2 rhs = (Vector2)uIComponent.relativePosition + uIComponent.size;
                    vector = Vector2.Max(vector, rhs);
                }

                base.size = vector + new Vector2(ScrollPadding.right, ScrollPadding.bottom);
            }
        }

        public void CenterChildControls()
        {
            if (base.childCount != 0)
            {
                Vector2 vector = Vector2.one * float.MaxValue;
                Vector2 vector2 = Vector2.one * float.MinValue;
                for (int i = 0; i < base.childCount; i++)
                {
                    UIComponent uIComponent = m_ChildComponents[i];
                    Vector2 vector3 = uIComponent.relativePosition;
                    Vector2 rhs = vector3 + uIComponent.size;
                    vector = Vector2.Min(vector, vector3);
                    vector2 = Vector2.Max(vector2, rhs);
                }

                Vector2 vector4 = vector2 - vector;
                Vector2 vector5 = (base.size - vector4) * 0.5f;
                for (int j = 0; j < base.childCount; j++)
                {
                    UIComponent uIComponent2 = m_ChildComponents[j];
                    uIComponent2.relativePosition = (Vector2)uIComponent2.relativePosition - vector + vector5;
                }
            }
        }

        public void ScrollToTop()
        {
            ScrollPosition = new Vector2(ScrollPosition.x, 0f);
        }

        public void ScrollToBottom()
        {
            ScrollPosition = new Vector2(ScrollPosition.x, 2.14748365E+09f);
        }

        public void ScrollToLeft()
        {
            ScrollPosition = new Vector2(0f, ScrollPosition.y);
        }

        public void ScrollToRight()
        {
            ScrollPosition = new Vector2(2.14748365E+09f, ScrollPosition.y);
        }

        public void ScrollIntoView(UIComponent component)
        {
            if (!m_ChildComponents.Contains(component))
            {
                return;
            }

            Rect rect = new Rect(ScrollPosition.x + (float)ScrollPadding.left, ScrollPosition.y + (float)ScrollPadding.top, base.size.x - (float)ScrollPadding.horizontal, base.size.y - (float)ScrollPadding.vertical).RoundToInt();
            Vector3 vector = component.relativePosition;
            Vector2 vector2 = component.size;
            Rect other = new Rect(ScrollPosition.x + vector.x, ScrollPosition.y + vector.y, vector2.x, vector2.y).RoundToInt();
            if (!rect.Intersects(other))
            {
                Vector2 vector3 = ScrollPosition;
                if (other.xMin < rect.xMin)
                {
                    vector3.x = other.xMin - (float)ScrollPadding.left;
                }
                else if (other.xMax > rect.xMax)
                {
                    vector3.x = other.xMax - Mathf.Max(base.size.x, vector2.x) + (float)ScrollPadding.horizontal;
                }

                if (other.y < rect.y)
                {
                    vector3.y = other.yMin - (float)ScrollPadding.top;
                }
                else if (other.yMax > rect.yMax)
                {
                    vector3.y = other.yMax - Mathf.Max(base.size.y, vector2.y) + (float)ScrollPadding.vertical;
                }

                ScrollPosition = vector3;
            }
        }

        public void Reset()
        {
            try
            {
                //SuspendLayout();
                if (AutoLayout)
                {
                    if (AutoReset)
                    {
                        ScrollPosition = Vector2.zero;
                    }

                    AutoArrange();
                }
                else
                {
                    ScrollPadding = ScrollPadding.ConstrainPadding();
                    Vector3 vector = CalculateMinChildPosition();
                    vector -= new Vector3(ScrollPadding.left, ScrollPadding.top);
                    for (int i = 0; i < m_ChildComponents.Count; i++)
                    {
                        m_ChildComponents[i].relativePosition -= vector;
                    }
                }

                if (AutoReset)
                {
                    if (AutoLayoutStart == LayoutStart.TopLeft)
                    {
                        ScrollPosition = Vector2.zero;
                    }
                    else if (AutoLayoutStart == LayoutStart.BottomLeft)
                    {
                        ScrollPosition = kMaxVerticalScroll;
                    }
                }

                Invalidate();
                UpdateScrollbars();
            }
            finally
            {
                //ResumeLayout();
            }
        }

        private void AutoArrange()
        {
            //SuspendLayout();
            try
            {
                ScrollPadding = ScrollPadding.ConstrainPadding();
                AutoLayoutPadding = AutoLayoutPadding.ConstrainPadding();
                float num = (float)ScrollPadding.left + (float)AutoLayoutPadding.left - ScrollPosition.x;
                float num2 = 0f;
                if (!UseCenter)
                {
                    if (AutoLayoutStart == LayoutStart.TopLeft)
                    {
                        num2 = (float)ScrollPadding.top + (float)AutoLayoutPadding.top - ScrollPosition.y;
                    }
                    else if (AutoLayoutStart == LayoutStart.BottomLeft)
                    {
                        num2 = base.height - (float)ScrollPadding.bottom - (float)AutoLayoutPadding.bottom - ScrollPosition.y;
                    }
                }

                float num3 = 0f;
                float num4 = 0f;
                for (int i = 0; i < base.childCount; i++)
                {
                    UIComponent uIComponent = m_ChildComponents[i];
                    if (!uIComponent.isVisibleSelf || !uIComponent.enabled || !uIComponent.gameObject.activeSelf || uIComponent == HorizontalScrollbar || uIComponent == VerticalScrollbar)
                    {
                        continue;
                    }

                    if (!UseCenter && WrapLayout)
                    {
                        if (AutoLayoutDirection == LayoutDirection.Horizontal)
                        {
                            if (num + uIComponent.width >= base.size.x - (float)ScrollPadding.right)
                            {
                                num = (float)ScrollPadding.left + (float)AutoLayoutPadding.left;
                                if (AutoLayoutStart == LayoutStart.TopLeft)
                                {
                                    num2 += num4;
                                }
                                else if (AutoLayoutStart == LayoutStart.BottomLeft)
                                {
                                    num2 -= num4;
                                }

                                num4 = 0f;
                            }
                        }
                        else if (num2 + uIComponent.height + (float)AutoLayoutPadding.vertical >= base.size.y - (float)ScrollPadding.bottom)
                        {
                            num2 = (float)ScrollPadding.top + (float)AutoLayoutPadding.top;
                            num += num3;
                            num3 = 0f;
                        }
                    }

                    Vector2 vector = Vector2.zero;
                    if (UseCenter)
                    {
                        vector = new Vector2(num, uIComponent.relativePosition.y);
                    }
                    else if (AutoLayoutStart == LayoutStart.TopLeft)
                    {
                        vector = new Vector2(num, num2);
                    }
                    else if (AutoLayoutStart == LayoutStart.BottomLeft)
                    {
                        vector = new Vector2(num, num2 - uIComponent.height);
                    }

                    uIComponent.relativePosition = vector;
                    float num5 = uIComponent.width + (float)AutoLayoutPadding.horizontal;
                    float num6 = uIComponent.height + (float)AutoLayoutPadding.vertical;
                    num3 = Mathf.Max(num5, num3);
                    num4 = Mathf.Max(num6, num4);
                    if (AutoLayoutDirection == LayoutDirection.Horizontal)
                    {
                        num += num5;
                    }
                    else if (AutoLayoutStart == LayoutStart.TopLeft)
                    {
                        num2 += num6;
                    }
                    else if (AutoLayoutStart == LayoutStart.BottomLeft)
                    {
                        num2 -= num6;
                    }
                }

                UpdateScrollbars();
            }
            finally
            {
                //ResumeLayout();
            }
        }

        private void Initialize()
        {
            if (m_Initialized)
            {
                return;
            }

            m_Initialized = true;
            if (Application.isPlaying)
            {
                if (HorizontalScrollbar != null)
                {
                    HorizontalScrollbar.OnScrollValueChanged += HorizontalScrollbarValueChanged;
                }

                if (VerticalScrollbar != null)
                {
                    VerticalScrollbar.OnScrollValueChanged += VerticalScrollbarValueChanged;
                }
            }

            if (m_ResetNeeded || AutoLayout || AutoReset)
            {
                Reset();
            }

            Invalidate();
            if (AutoReset)
            {
                if (AutoLayoutStart == LayoutStart.TopLeft)
                {
                    ScrollPosition = Vector2.zero;
                }
                else if (AutoLayoutStart == LayoutStart.BottomLeft)
                {
                    ScrollPosition = kMaxVerticalScroll;
                }
            }

            UpdateScrollbars();
        }

        private void ScrollChildControls(Vector3 delta, bool free = false)
        {
            try
            {
                m_Scrolling = true;
                delta = delta.Scale(1f, -1f, 1f);
                for (int i = 0; i < base.childCount; i++)
                {
                    UIComponent uIComponent = m_ChildComponents[i];
                    Vector3 vector = uIComponent.position - delta;
                    if (!free)
                    {
                        vector = vector.RoundToInt();
                    }

                    uIComponent.position = vector;
                }
            }
            finally
            {
                m_Scrolling = false;
            }
        }

        private Vector2 CalculateMinChildPosition()
        {
            float num = float.MaxValue;
            float num2 = float.MaxValue;
            for (int i = 0; i < base.childCount; i++)
            {
                UIComponent uIComponent = m_ChildComponents[i];
                if (uIComponent.enabled && uIComponent.gameObject.activeSelf)
                {
                    Vector3 vector = uIComponent.relativePosition.FloorToInt();
                    num = Mathf.Min(num, vector.x);
                    num2 = Mathf.Min(num2, vector.y);
                }
            }

            return new Vector2(num, num2);
        }

        public Vector2 CalculateViewSize()
        {
            if (m_CustomScrollBounds)
            {
                float x = 0f;
                float y = 0f;
                if (m_HorizontalScrollbar != null)
                {
                    x = m_HorizontalScrollbar.MaxValue - m_HorizontalScrollbar.MinValue;
                }

                if (m_VerticalScrollbar != null)
                {
                    y = m_VerticalScrollbar.MaxValue - m_VerticalScrollbar.MinValue;
                }

                return new Vector2(x, y);
            }

            Vector2 vector = new Vector2(ScrollPadding.horizontal, ScrollPadding.vertical).RoundToInt();
            Vector2 result = base.size.RoundToInt() - vector;
            if (base.childCount == 0)
            {
                return result;
            }

            Vector2 vector2 = Vector2.one * float.MaxValue;
            Vector2 vector3 = Vector2.one * float.MinValue;
            for (int i = 0; i < base.childCount; i++)
            {
                UIComponent uIComponent = m_ChildComponents[i];
                if (uIComponent.isVisibleSelf)
                {
                    Vector2 vector4 = uIComponent.relativePosition.RoundToInt();
                    Vector2 lhs = vector4 + uIComponent.size.RoundToInt();
                    lhs.x += AutoLayoutPadding.horizontal;
                    lhs.y += AutoLayoutPadding.vertical;
                    vector2 = Vector2.Min(vector4, vector2);
                    vector3 = Vector2.Max(lhs, vector3);
                }
            }

            return vector3 - vector2;
        }

        private void UpdateScrollbars()
        {
            Vector2 vector = CalculateViewSize();
            Vector2 vector2 = base.size - new Vector2(ScrollPadding.horizontal, ScrollPadding.vertical);
            if (HorizontalScrollbar != null)
            {
                HorizontalScrollbar.MinValue = 0f;
                HorizontalScrollbar.MaxValue = vector.x;
                HorizontalScrollbar.ScrollSize = vector2.x;
                HorizontalScrollbar.Value = Mathf.Max(0f, ScrollPosition.x);
            }

            if (VerticalScrollbar != null)
            {
                VerticalScrollbar.MinValue = 0f;
                VerticalScrollbar.MaxValue = vector.y;
                VerticalScrollbar.ScrollSize = vector2.y;
                VerticalScrollbar.Value = Mathf.Max(0f, ScrollPosition.y);
            }
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

        private void ChildZOrderChanged(UIComponent child, int value)
        {
            ChildInvalidatedLayout();
        }

        private void ChildIsVisibleChanged(UIComponent child, bool value)
        {
            ChildInvalidatedLayout();
        }

        private void ChildInvalidated(UIComponent child, Vector2 value)
        {
            ChildInvalidatedLayout();
        }

        private void ChildInvalidatedLayout()
        {
            if (!m_Scrolling && !base.isLayoutSuspended)
            {
                if (AutoLayout)
                {
                    AutoArrange();
                }

                UpdateScrollbars();
                Invalidate();
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            if (base.size == Vector2.zero)
            {
                //SuspendLayout();
                Camera camera = GetCamera();
                base.size = new Vector3(camera.pixelWidth / 2, camera.pixelHeight / 2);
                //ResumeLayout();
            }

            if (AutoLayout)
            {
                AutoArrange();
            }

            UpdateScrollbars();
        }

        public override void Update()
        {
            base.Update();
            if (UseScrollMomentum && !m_IsMouseDown && m_ScrollMomentum != Vector2.zero)
            {
                ScrollPosition += m_ScrollMomentum;
            }

            if (m_IsComponentInvalidated && AutoLayout && base.isVisible)
            {
                AutoArrange();
                UpdateScrollbars();
            }

            m_ScrollMomentum *= 0.95f - Time.deltaTime;
            if (m_ScrollMomentum.sqrMagnitude < 0.01f)
            {
                m_ScrollMomentum = Vector2.zero;
            }
        }

        public override void LateUpdate()
        {
            base.LateUpdate();
            Initialize();
            if (m_ResetNeeded)
            {
                m_ResetNeeded = false;
                if (AutoReset || AutoLayout)
                {
                    Reset();
                }
            }
        }

        public override void OnDestroy()
        {
            if (m_HorizontalScrollbar != null)
            {
                m_HorizontalScrollbar.OnScrollValueChanged -= HorizontalScrollbarValueChanged;
            }

            if (m_VerticalScrollbar != null)
            {
                m_VerticalScrollbar.OnScrollValueChanged -= VerticalScrollbarValueChanged;
            }

            m_HorizontalScrollbar = null;
            m_VerticalScrollbar = null;
        }

        private void VerticalScrollbarValueChanged(float value)
        {
            ScrollPosition = new Vector2(ScrollPosition.x, value);
        }

        private void HorizontalScrollbarValueChanged(float value)
        {
            ScrollPosition = new Vector2(value, ScrollPosition.y);
        }


        private int layoutSuspend;
        public bool IsLayoutSuspended => layoutSuspend != 0;

        public Vector2 ItemSize => new Vector2(width - AutoLayoutPadding.horizontal - ScrollPadding.horizontal, height - AutoLayoutPadding.vertical - ScrollPadding.vertical);
        public RectOffset LayoutPadding => AutoLayoutPadding;

        public virtual void StopLayout()
        {
            if (layoutSuspend == 0)
                m_AutoLayout = false;

            layoutSuspend += 1;
        }
        public virtual void StartLayout(bool layoutNow = true, bool force = false)
        {
            layoutSuspend = force ? 0 : Mathf.Max(layoutSuspend - 1, 0);

            if (layoutSuspend == 0)
            {
                m_AutoLayout = true;
                if (layoutNow)
                    Reset();
            }
        }
        public void PauseLayout(Action action, bool layoutNow = true, bool force = false)
        {
            try
            {
                StopLayout();
                action?.Invoke();
            }
            finally
            {
                StartLayout(layoutNow, force);
            }
        }
    }
}
