using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class ObjectPopup<ObjectType, EntityType> : CustomUIPanel, IPopup<ObjectType, EntityType>, IReusable
        where EntityType : CustomUIButton, IPopupEntity<ObjectType>, IReusable
    {
        bool IReusable.InCache { get; set; }

        public event Action<ObjectType> OnSelect;
        public event EntityStyleDelegate<ObjectType, EntityType> OnSetEntityStyle;

        protected virtual float DefaultEntityHeight => 20f;

        private CustomUIScrollbar ScrollBar { get; set; }
        private List<EntityType> Entities { get; set; } = new List<EntityType>();

        public Func<ObjectType, ObjectType, bool> IsEqualDelegate { protected get; set; }
        private Func<ObjectType, bool> Selector { get; set; } = null;
        private Func<ObjectType, ObjectType, int> Sorter { get; set; } = null;

        private List<ObjectType> RawValues { get; set; } = new List<ObjectType>();
        private List<ObjectType> Values { get; set; } = new List<ObjectType>();


        private ObjectType selectedObject;
        public ObjectType SelectedObject
        {
            get => selectedObject;
            set
            {
                if (!Equal(value, selectedObject))
                {
                    selectedObject = value;
                    StartIndex = Values.IndexOf(selectedObject);
                }
            }
        }

        private int startIndex;
        private int StartIndex
        {
            get => startIndex;
            set
            {
                startIndex = Mathf.Clamp(value, 0, Values.Count - VisibleCount);
                RefreshEntities();
            }
        }

        private float entityHeight = 20f;
        public float EntityHeight
        {
            get => entityHeight;
            set
            {
                if (entityHeight != value)
                {
                    entityHeight = value;
                    StartIndex = StartIndex;
                }
            }
        }
        public float EntityWidth => width - (ShowScroll ? ScrollBar.width : 0f) - itemsPadding.horizontal;


        private int maxVisibleItems = 0;
        public int MaxVisibleItems
        {
            get => maxVisibleItems;
            set
            {
                if (value != maxVisibleItems)
                {
                    maxVisibleItems = value;
                    StartIndex = StartIndex;
                }
            }
        }
        private new Vector2 maximumSize;
        public Vector2 MaximumSize
        {
            get => maximumSize;
            set
            {
                if (value != maximumSize)
                {
                    maximumSize = value;
                    StartIndex = StartIndex;
                }
            }
        }
        private new Vector2 minimumSize;
        public Vector2 MinimumSize
        {
            get => minimumSize;
            set
            {
                if (value != minimumSize)
                {
                    minimumSize = value;
                    StartIndex = StartIndex;
                }
            }
        }

        private bool autoWidth;
        public bool AutoWidth
        {
            get => autoWidth && VisibleCount == Values.Count;
            set
            {
                if (value != autoWidth)
                {
                    autoWidth = value;
                    StartIndex = StartIndex;
                }
            }
        }
        private bool autoHeight;
        public bool AutoHeight
        {
            get => autoHeight && VisibleCount == Values.Count;
            set
            {
                if (value != autoHeight)
                {
                    autoHeight = value;
                    StartIndex = StartIndex;
                }
            }
        }

        private RectOffset itemsPadding = new RectOffset(0, 0, 0, 0);
        public RectOffset ItemsPadding
        {
            get => itemsPadding;
            set
            {
                itemsPadding = value;
                PlaceEntities();
            }
        }

        protected virtual int VisibleCount
        {
            get
            {
                var visibleCount = MaxVisibleItems > 0 ? MaxVisibleItems : int.MaxValue;
                var fitCount = Mathf.FloorToInt(Mathf.Max(MaximumSize.y - ItemsPadding.vertical, 0f) / EntityHeight);
                return Mathf.Min(Values.Count, visibleCount, fitCount);
            }
        }
        protected virtual float PopupHeight => VisibleCount * EntityHeight + ItemsPadding.vertical;
        protected bool ShowScroll => Values.Count > VisibleCount;
        protected virtual Vector2 ScrollPosition => new Vector2(width - ScrollBar.width, 0f);
        protected virtual float ScrollHeight => height;

        public ObjectPopup()
        {
            clipChildren = true;

            ScrollBar = AddUIComponent<CustomUIScrollbar>();
            ScrollBar.name = nameof(ScrollBar);
            ScrollBar.Orientation = UIOrientation.Vertical;
            ScrollBar.DefaultStyle();
            ScrollBar.OnScrollValueChanged += ScrollBarValueChanged;

            MaximumSize = new Vector2(200f, 700f);
            entityHeight = DefaultEntityHeight;
        }

        public virtual void Init(IEnumerable<ObjectType> values, Func<ObjectType, bool> selector, Func<ObjectType, ObjectType, int> sorter)
        {
            Selector = selector;
            Sorter = sorter;
            RawValues.Clear();
            RawValues.AddRange(values);
            RefreshValues();
        }
        public virtual void DeInit()
        {
            OnSelect = null;

            RawValues.Clear();
            Values.Clear();
            Selector = null;
            Sorter = null;

            startIndex = 0;
            entityHeight = DefaultEntityHeight;
            maxVisibleItems = 0;
            autoWidth = false;
            refreshEnable = true;
        }
        protected bool Equal(ObjectType value1, ObjectType value2)
        {
            if (IsEqualDelegate != null)
                return IsEqualDelegate(value1, value2);
            else if (value1 != null)
                return value1.Equals(value2);
            else if (value2 != null)
                return value2.Equals(value1);
            else
                return ReferenceEquals(value1, value2);
        }

        private void ObjectSelected(int index, ObjectType value)
        {
            selectedObject = value;
            OnSelect?.Invoke(value);
        }


        private bool refreshEnable = true;
        public void StopRefresh()
        {
            refreshEnable = false;
        }
        public void StartRefresh()
        {
            refreshEnable = true;
            RefreshEntities();
        }
        protected virtual bool FilterObjects(ObjectType value) => Selector == null || Selector(value);
        protected virtual int SortObjects(ObjectType objA, ObjectType objB) => Sorter != null ? Sorter(objA, objB) : -1;
        protected virtual void RefreshValues()
        {
            Values.Clear();
            Values.AddRange(RawValues.Where(FilterObjects));
            Values.Sort(SortObjects);
            StartIndex = 0;

            ScrollBar.MinValue = 0;
            ScrollBar.MaxValue = Values.Count - VisibleCount + 1;
        }
        protected virtual void RefreshEntities()
        {
            if (!refreshEnable)
                return;

            var visibleCount = VisibleCount;

            var count = Math.Max(visibleCount, Entities.Count);

            for (var i = 0; i < count; i += 1)
            {
                var index = StartIndex + i;

                if (i < visibleCount)
                {
                    EntityType entity;
                    if (i < Entities.Count)
                        entity = Entities[i];
                    else
                    {
                        entity = ComponentPool.Get<EntityType>(this);
                        entity.OnSelected += ObjectSelected;

                        var overridden = false;
                        OnSetEntityStyle?.Invoke(entity, ref overridden);
                        if (!overridden)
                            SetEntityStyle(entity);

                        Entities.Add(entity);
                    }

                    var value = Values[index];
                    SetEntityValue(Entities[i], index, value, Equal(value, SelectedObject));
                }
                else
                {
                    Entities[i].OnSelected -= ObjectSelected;
                    ComponentPool.Free(Entities[i]);
                }
            }
            Entities.RemoveRange(visibleCount, Entities.Count - visibleCount);

            PlaceEntities();
        }
        protected virtual void SetEntityStyle(EntityType entity) { }
        protected virtual void SetEntityValue(EntityType entity, int index, ObjectType value, bool selected)
        {
            entity.SetObject(index, value, selected);
        }
        protected virtual void PlaceEntities()
        {
            var visibleCount = VisibleCount;

            if (AutoWidth)
            {
                var entitySize = new Vector2(0f, EntityHeight);
                for (var i = 0; i < visibleCount; i += 1)
                {
                    Entities[i].PerformWidth();
                    entitySize.x = Mathf.Max(entitySize.x, Entities[i].width);
                }

                for (var i = 0; i < visibleCount; i += 1)
                {
                    Entities[i].size = entitySize;
                    Entities[i].relativePosition = GetEntityPosition(i);
                }

                width = entitySize.x + ItemsPadding.horizontal;
            }
            else
            {
                var entitySize = new Vector2(EntityWidth, EntityHeight);
                for (var i = 0; i < visibleCount; i += 1)
                {
                    Entities[i].size = entitySize;
                    Entities[i].relativePosition = GetEntityPosition(i);
                }
            }

            height = PopupHeight;
            ScrollBar.height = ScrollHeight;
            ScrollBar.relativePosition = ScrollPosition;
            ScrollBar.Value = StartIndex;
            ScrollBar.isVisible = ShowScroll;
        }

        protected virtual Vector2 GetEntityPosition(int index) => new Vector2(ItemsPadding.left, EntityHeight * index + ItemsPadding.top);

        protected override void OnMouseWheel(UIMouseEventParameter p)
        {
            p.Use();
            StartIndex += (p.wheelDelta > 0 ? -1 : 1) * (Utility.OnlyShiftIsPressed ? 10 : 1);
        }
        private void ScrollBarValueChanged(float value)
        {
            StartIndex = Mathf.RoundToInt(value);
        }
    }

    public abstract class SearchPopup<ObjectType, EntityType> : ObjectPopup<ObjectType, EntityType>
        where EntityType : CustomUIButton, IPopupEntity<ObjectType>, IReusable
        where ObjectType : class
    {
        private bool CanSubmit { get; set; } = true;
        protected CustomUITextField Search { get; private set; }
        private CustomUILabel NothingFound { get; set; }
        private CustomUIButton ResetButton { get; set; }
        protected abstract string NotFoundText { get; }

        public SearchPopup()
        {
            Search = AddUIComponent<CustomUITextField>();
            Search.name = nameof(Search);
            Search.atlasBackground = CommonTextures.Atlas;
            Search.normalBgSprite = CommonTextures.PanelSmall;
            Search.atlas = TextureHelper.InGameAtlas;
            Search.selectionSprite = "EmptySprite";
            Search.color = new Color32(10, 10, 10, 255);
            Search.relativePosition = new Vector2(5f, 5f);
            Search.height = 20f;
            Search.builtinKeyNavigation = true;
            Search.cursorWidth = 1;
            Search.cursorBlinkTime = 0.45f;
            Search.selectOnFocus = true;
            Search.textScale = 0.7f;
            Search.padding = new RectOffset(20, 30, 6, 0);
            Search.horizontalAlignment = UIHorizontalAlignment.Left;
            Search.eventTextChanged += SearchTextChanged;
            Search.eventGotFocus += SearchGotFocus;
            Search.eventLostFocus += SearchLostFocus;

            var loop = Search.AddUIComponent<UISprite>();
            loop.atlas = TextureHelper.InGameAtlas;
            loop.spriteName = "ContentManagerSearch";
            loop.size = new Vector2(10f, 10f);
            loop.relativePosition = new Vector2(5f, 5f);

            ResetButton = Search.AddUIComponent<CustomUIButton>();
            ResetButton.name = nameof(ResetButton);
            ResetButton.atlas = TextureHelper.InGameAtlas;
            ResetButton.normalFgSprite = "ContentManagerSearchReset";
            ResetButton.size = new Vector2(10f, 10f);
            ResetButton.hoveredBgColor = new Color32(127, 127, 127, 255);
            ResetButton.isVisible = false;
            ResetButton.eventClick += ResetClick;

            NothingFound = AddUIComponent<CustomUILabel>();
            NothingFound.name = nameof(NothingFound);
            NothingFound.text = NotFoundText;
            NothingFound.autoSize = false;
            NothingFound.autoHeight = false;
            NothingFound.height = EntityHeight;
            NothingFound.relativePosition = new Vector2(0, 30f);
            NothingFound.verticalAlignment = UIVerticalAlignment.Middle;
            NothingFound.textAlignment = UIHorizontalAlignment.Center;
            NothingFound.isVisible = false;
        }


        public override void DeInit()
        {
            base.DeInit();
            CanSubmit = false;
            Search.text = string.Empty;
            Search.Unfocus();
        }

        protected string SearchText => Search.text.ToUpper();
        protected override bool FilterObjects(ObjectType value) => base.FilterObjects(value) && FilterSearch(value);
        protected virtual bool FilterSearch(ObjectType value)
        {
            name = GetName(value);
            return name.ToUpper().Contains(SearchText);
        }
        protected abstract string GetName(ObjectType value);

        private void SearchTextChanged(UIComponent component, string value)
        {
            if (CanSubmit)
                RefreshValues();

            ResetButton.isVisible = !string.IsNullOrEmpty(value);
        }

        private void ResetClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            Search.text = string.Empty;
            Focus();
        }
        protected override void RefreshValues()
        {
            base.RefreshValues();
            Search.width = width - 10f;
            NothingFound.size = new Vector2(width, EntityHeight);
            NothingFound.isVisible = VisibleCount == 0;
            ResetButton.relativePosition = new Vector2(Search.width - 15f, 5f);
        }
        private void SearchGotFocus(UIComponent component, UIFocusEventParameter p)
        {
            isInteractive = false;
        }
        private void SearchLostFocus(UIComponent component, UIFocusEventParameter p)
        {
            isInteractive = true;
            if (p.gotFocus == null)
                Focus();
        }
        protected override void OnEnterFocus(UIFocusEventParameter p)
        {
            if (isInteractive && p.gotFocus == this)
                Search.Focus();
            else
                base.OnEnterFocus(p);
        }
        protected override void OnLeaveFocus(UIFocusEventParameter p)
        {
            if (isInteractive && p.gotFocus == null && p.lostFocus != null && p.lostFocus != this && p.lostFocus.transform.IsChildOf(transform))
                Focus();
            else
                base.OnLeaveFocus(p);
        }

        protected override float PopupHeight => Math.Max(base.PopupHeight, EntityHeight) + 30f;
        protected override Vector2 ScrollPosition => base.ScrollPosition + new Vector2(0f, 30f);
        protected override float ScrollHeight => height - 30f;
        protected override Vector2 GetEntityPosition(int index) => base.GetEntityPosition(index) + new Vector2(0f, 30f);
    }

    public abstract class PopupEntity<ObjectType> : CustomUIButton, IPopupEntity<ObjectType>, IReusable
    {
        public event Action<int, ObjectType> OnSelected;
        bool IReusable.InCache { get; set; }

        public virtual ObjectType EditObject { get; protected set; }
        public bool Selected { get; set; }
        public int Index { get; set; }
        public RectOffset Padding { get; set; } = new RectOffset();

        public override void Update()
        {
            base.Update();

            if (Selected)
                state = ButtonState.Focused;
            else if (m_IsMouseHovering)
                state = ButtonState.Hovered;
            else
                state = ButtonState.Normal;
        }

        public virtual void DeInit()
        {
            OnSelected = null;
            Selected = false;
            Index = -1;
            Padding = new RectOffset();
        }

        public virtual void SetObject(int index, ObjectType value, bool selected)
        {
            Index = index;
            EditObject = value;
            Selected = selected;
        }
        protected void Select() => OnSelected?.Invoke(Index, EditObject);
        public virtual void PerformWidth()
        {
            AutoWidth();
        }

        protected override void OnClick(UIMouseEventParameter p)
        {
            base.OnClick(p);
            if (!p.used)
            {
                Select();
                p.Use();
            }
        }
    }
}
