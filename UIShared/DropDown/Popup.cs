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

        public bool AutoClose { get; protected set; } = true;
        private CustomUIScrollbar ScrollBar { get; set; }
        private CustomUILabel EmptyLabel { get; set; }
        private CustomUIPanel Content { get; set; }
        protected virtual string EmptyText => CommonLocalize.Popup_Empty;
        private List<EntityType> Entities { get; set; } = new List<EntityType>();

        public Func<ObjectType, ObjectType, bool> IsEqualDelegate { protected get; set; }
        private Func<ObjectType, bool> Selector { get; set; } = null;
        private Func<ObjectType, ObjectType, int> Sorter { get; set; } = null;

        private List<ObjectType> RawValues { get; set; } = new List<ObjectType>();
        private List<ObjectType> Values { get; set; } = new List<ObjectType>();


        private DropDownStyle style;
        public DropDownStyle PopupStyle
        {
            get => style;
            set
            {
                style = value;

                atlasBackground = value.PopupAtlas;
                backgroundSprite = value.PopupSprite;
                bgColors = value.PopupColor;
                if (value.PopupItemsPadding != null)
                    ItemsPadding = value.PopupItemsPadding;

                Invalidate();
            }
        }


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
                Refresh();
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

        public RectOffset ItemsPadding
        {
            get => Content.Padding;
            set => Content.Padding = value;
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
        protected bool ShowScroll => Values.Count > VisibleCount;
        protected virtual Vector2 ScrollPosition => new Vector2(width - ScrollBar.width, 0f);
        protected virtual float ScrollHeight => height;

        public ObjectPopup()
        {
            clipChildren = true;

            PauseLayout(() =>
            {
                AutoLayout = AutoLayout.Vertical;
                AutoChildrenVertically = AutoLayoutChildren.Fit;
                AutoChildrenHorizontally = AutoLayoutChildren.Fill;

                FillPopup();

                MaximumSize = new Vector2(200f, 700f);
                entityHeight = DefaultEntityHeight;
            });
        }
        protected virtual void FillPopup()
        {
            ScrollBar = AddUIComponent<CustomUIScrollbar>();
            ScrollBar.name = nameof(ScrollBar);
            ScrollBar.Orientation = UIOrientation.Vertical;
            ScrollBar.DefaultStyle();
            ScrollBar.OnScrollValueChanged += ScrollBarValueChanged;
            Ignore(ScrollBar, true);

            EmptyLabel = AddUIComponent<CustomUILabel>();
            EmptyLabel.name = nameof(EmptyLabel);
            EmptyLabel.text = EmptyText;
            EmptyLabel.AutoSize = AutoSize.None;
            EmptyLabel.height = 30f;
            EmptyLabel.relativePosition = new Vector2(0, 0);
            EmptyLabel.VerticalAlignment = UIVerticalAlignment.Middle;
            EmptyLabel.HorizontalAlignment = UIHorizontalAlignment.Center;
            EmptyLabel.isVisible = false;

            Content = AddUIComponent<CustomUIPanel>();
            Content.name = nameof(Content);
            Content.AutoLayout = AutoLayout.Vertical;
            Content.AutoChildrenVertically = AutoLayoutChildren.Fit;
            Content.AutoChildrenHorizontally = AutoLayoutChildren.Fill;
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
            selectedObject = default;

            startIndex = 0;
            entityHeight = DefaultEntityHeight;
            maxVisibleItems = 0;
            autoWidth = false;
            AutoClose = true;
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

        public void PauseRefreshing(Action action)
        {
            PauseLayout(action, false);
            Refresh();
        }

        protected virtual bool FilterObjects(ObjectType value) => Selector == null || Selector(value);
        protected virtual int SortObjects(ObjectType objA, ObjectType objB) => Sorter != null ? Sorter(objA, objB) : -1;
        protected virtual void RefreshValues()
        {
            Values.Clear();
            Values.AddRange(RawValues.Where(FilterObjects));
            Values.Sort(SortObjects);
            StartIndex = 0;
        }

        private void Refresh()
        {
            if (IsLayoutSuspended)
                return;

            StopLayout();
            {
                Content.PauseLayout(RefreshEntities);

                ScrollBar.MinValue = 0;
                ScrollBar.MaxValue = Values.Count - VisibleCount + 1;
                ScrollBar.Value = StartIndex;
                ScrollBar.isVisible = ShowScroll;
            }
            StartLayout();
        }
        protected virtual void RefreshEntities()
        {
            ArrangeEntitiesList();

            var visibleCount = VisibleCount;

            if (AutoWidth)
            {
                var entityWidth = 0f;
                for (var i = 0; i < visibleCount; i += 1)
                {
                    Entities[i].height = EntityHeight;
                    entityWidth = Mathf.Max(entityWidth, Entities[i].MeasuredSize.x);
                }

                width = entityWidth + ItemsPadding.horizontal + (ShowScroll ? ScrollBar.width : 0f);
            }
            else
            {
                for (var i = 0; i < visibleCount; i += 1)
                    Entities[i].height = EntityHeight;
            }

            if (ShowScroll)
                SetItemMargin(Content, new RectOffset(0, Mathf.CeilToInt(ScrollBar.width), 0, 0));
            else
                SetItemMargin(Content, new RectOffset());

            EmptyLabel.isVisible = VisibleCount == 0;
            EmptyLabel.text = EmptyText;
        }
        private void ArrangeEntitiesList()
        {
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
                        entity = ComponentPool.Get<EntityType>(Content);
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
        }
        protected virtual void SetEntityStyle(EntityType entity) { }
        protected virtual void SetEntityValue(EntityType entity, int index, ObjectType value, bool selected)
        {
            entity.SetObject(index, value, selected);
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            ScrollBar.height = ScrollHeight;
            ScrollBar.relativePosition = ScrollPosition;
        }
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
        private bool FocusSearch { get; set; } = true;
        protected CustomUITextField Search { get; private set; }
        private CustomUIButton ResetButton { get; set; }

        protected override void FillPopup()
        {
            Search = AddUIComponent<CustomUITextField>();
            Search.name = nameof(Search);
            Search.BgAtlas = CommonTextures.Atlas;
            Search.BgSprites = CommonTextures.PanelSmall;
            Search.atlas = TextureHelper.InGameAtlas;
            Search.selectionSprite = "EmptySprite";
            Search.BgColors = new Color32(10, 10, 10, 255);
            Search.height = 20f;
            Search.builtinKeyNavigation = true;
            Search.cursorWidth = 1;
            Search.cursorBlinkTime = 0.45f;
            Search.selectOnFocus = true;
            Search.textScale = 0.7f;
            Search.padding = new RectOffset(20, 30, 6, 0);
            Search.horizontalAlignment = UIHorizontalAlignment.Left;
            Search.eventTextChanged += SearchTextChanged;
            Search.eventGotFocus += ItemGotFocus;
            Search.eventLostFocus += ItemLostFocus;
            SetItemMargin(Search, new RectOffset(5, 5, 5, 5));

            var loop = Search.AddUIComponent<UISprite>();
            loop.atlas = TextureHelper.InGameAtlas;
            loop.spriteName = "ContentManagerSearch";
            loop.size = new Vector2(10f, 10f);
            loop.relativePosition = new Vector2(5f, 5f);

            ResetButton = Search.AddUIComponent<CustomUIButton>();
            ResetButton.name = nameof(ResetButton);
            ResetButton.Atlas = TextureHelper.InGameAtlas;
            ResetButton.FgSprites = "ContentManagerSearchReset";
            ResetButton.size = new Vector2(10f, 10f);
            ResetButton.HoveredBgColor = new Color32(127, 127, 127, 255);
            ResetButton.isVisible = false;
            ResetButton.eventClick += ResetClick;
            ResetButton.eventGotFocus += ItemGotFocus;
            ResetButton.eventLostFocus += ItemLostFocus;

            Search.eventSizeChanged += SearchSizeChanged;

            base.FillPopup();
        }

        public override void DeInit()
        {
            base.DeInit();
            CanSubmit = false;
            Search.text = string.Empty;
            CanSubmit = true;
            Search.Unfocus();
            FocusSearch = true;
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
        }
        private void SearchSizeChanged(UIComponent component, Vector2 value)
        {
            ResetButton.relativePosition = new Vector2(Search.width - 15f, 5f);
        }

        private void ItemGotFocus(UIComponent component, UIFocusEventParameter p)
        {
            AutoClose = false;
        }
        private void ItemLostFocus(UIComponent component, UIFocusEventParameter p)
        {
            if (p.gotFocus == null || !p.gotFocus.transform.IsChildOf(transform))
                Focus();
        }
        protected override void OnGotFocus(UIFocusEventParameter p)
        {
            if (FocusSearch)
            {
                Search.Focus();
                FocusSearch = false;
            }
            else
            {
                base.OnGotFocus(p);
                if(p.gotFocus == this)
                    AutoClose = true;
            }
        }

        protected override Vector2 ScrollPosition => base.ScrollPosition + new Vector2(0f, 30f);
        protected override float ScrollHeight => height - 30f;
    }

    public abstract class PopupEntity<ObjectType> : CustomUIButton, IPopupEntity<ObjectType>, IReusable
    {
        public event Action<int, ObjectType> OnSelected;
        bool IReusable.InCache { get; set; }

        public virtual ObjectType EditObject { get; protected set; }
        public int Index { get; set; }
        public RectOffset Padding { get; set; } = new RectOffset();

        public virtual void DeInit()
        {
            OnSelected = null;
            IsSelected = false;
            Index = -1;
            Padding = new RectOffset();
        }

        public virtual void SetObject(int index, ObjectType value, bool selected)
        {
            Index = index;
            EditObject = value;
            IsSelected = selected;
        }
        protected void Select() => OnSelected?.Invoke(Index, EditObject);

        protected override void OnClick(UIMouseEventParameter p)
        {
            base.OnClick(p);
            if (!p.used)
            {
                Select();
                p.Use();
            }
        }

        public DropDownStyle EntityStyle
        {
            set
            {
                bgAtlas = value.EntityAtlas;

                bgSprites = value.EntitySprites;
                selBgSprites = value.EntitySelSprites;

                bgColors = value.EntityColors;
                selBgColors = value.EntitySelColors;

                Invalidate();
            }
        }
    }
}
