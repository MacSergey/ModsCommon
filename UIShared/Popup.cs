﻿using ColossalFramework.UI;
using ModsCommon.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NodeMarkup.UI
{
    public abstract class Popup<ObjectType, EntityPanel> : CustomUIPanel, IReusable
        where EntityPanel : PopupEntity<ObjectType>
        where ObjectType : class
    {
        public event Action<ObjectType> OnSelectedChanged;

        bool IReusable.InCache { get; set; }

        protected virtual float DefaultEntityHeight => 20f;
        
        private CustomUIScrollbar ScrollBar { get; set; }

        private List<ObjectType> RawValues { get; set; } = null;
        private List<ObjectType> Values { get; set; } = null;
        private List<EntityPanel> Entities { get; set; } = new List<EntityPanel>();
        private Func<ObjectType, bool> Selector { get; set; } = null;

        private ObjectType _selectedObject;
        public ObjectType SelectedObject
        {
            get => _selectedObject;
            set
            {
                if(value != _selectedObject)
                {
                    _selectedObject = value;
                    var index = Values.IndexOf(_selectedObject);
                    if (index >= 0)
                    {
                        StartIndex = index;
                        SetValues();
                    }
                }
            }
        }

        private int _startIndex;
        private int StartIndex
        {
            get => _startIndex;
            set
            {
                if(value != _startIndex)
                {
                    _startIndex = Mathf.Clamp(value, 0, Values.Count - VisibleCount);
                    SetValues();
                }
            }
        }
        protected virtual int VisibleCount => Mathf.Min(Values.Count, MaxVisibleItems, Mathf.FloorToInt(maximumSize.y / EntityHeight));
        protected bool ShowScroll => Values.Count > Entities.Count;

        public float EntityHeight { get; set; }
        public int MaxVisibleItems { get; set; }

        public string ItemHover { get; set; }
        public string ItemSelected { get; set; }

        public Popup()
        {
            clipChildren = true;

            var gameObject = GameObject.Instantiate(ModsCommon.UI.UIHelper.ScrollBar.gameObject);
            AttachUIComponent(gameObject);
            ScrollBar = gameObject.GetComponent<CustomUIScrollbar>();
            ScrollBar.eventValueChanged += ScrollBarValueChanged;

            EntityHeight = DefaultEntityHeight;
        }

        public void Init(IEnumerable<ObjectType> values, Func<ObjectType, bool> selector = null)
        {
            Selector = selector;
            RawValues = values.ToList();
            Refresh();
        }

        public virtual void DeInit()
        {
            OnSelectedChanged = null;
            RawValues = null;
            Values = null;
            Selector = null;
            _startIndex = 0;
            EntityHeight = DefaultEntityHeight;
        }

        protected virtual bool Filter(ObjectType value) => Selector == null || Selector(value);
        protected virtual void Refresh()
        {
            Values = RawValues == null ? new List<ObjectType>() : RawValues.Where(Filter).ToList();

            var visibleCount = VisibleCount;
            height = GetHeight();

            if (Entities.Count < visibleCount)
            {
                for(int i = Entities.Count; i < visibleCount; i += 1)
                {
                    var newEntery = AddUIComponent<EntityPanel>();
                    newEntery.atlas = atlas;
                    newEntery.hoveredBgSprite = ItemHover;
                    newEntery.focusedBgSprite = ItemSelected;
                    newEntery.OnSelected += ObjectSelected;
                    Entities.Add(newEntery);
                }
            }
            else if(Entities.Count > visibleCount)
            {
                for (int i = visibleCount; i < Entities.Count; i += 1)
                {
                    Entities[i].OnSelected -= ObjectSelected;
                    ComponentPool.Free(Entities[i]);
                }
                Entities.RemoveRange(visibleCount, Entities.Count - visibleCount);
            }

            ScrollBar.minValue = 0;
            ScrollBar.maxValue = Values.Count - visibleCount;

            RefreshItems();
            SetValues();
        }

        private void ObjectSelected(ObjectType value)
        {
            _selectedObject = value;
            OnSelectedChanged?.Invoke(value);
        }

        protected virtual void SetValues()
        {
            for(int i = 0; i < Entities.Count; i += 1)
            {
                if (StartIndex + i < Values.Count)
                {
                    var value = Values[StartIndex + i];
                    Entities[i].SetObject(value);
                    Entities[i].Selected = (value == SelectedObject);
                }
                else
                {
                    Entities[i].SetObject(null);
                    Entities[i].Selected = false;
                }    
            }

            ScrollBar.value = StartIndex;
        }
        protected virtual void RefreshItems()
        {
            for (int i = 0; i < Entities.Count; i += 1)
            {
                var entity = Entities[i];
                entity.size = GetEntitySize(i);
                entity.relativePosition = GetEntityPosition(i);
            }

            ScrollBar.isVisible = ShowScroll;
            ScrollBar.height = GetScrollHeight();
            ScrollBar.relativePosition = GetScrollPosition();
        }
        protected virtual float GetHeight() => VisibleCount * EntityHeight;
        protected virtual Vector2 GetEntitySize(int index) => new Vector2(width - (ShowScroll ? ScrollBar.width : 0f), EntityHeight);
        protected virtual Vector2 GetEntityPosition(int index) => new Vector2(0, EntityHeight * index);
        protected virtual float GetScrollHeight() => VisibleCount * EntityHeight;
        protected virtual Vector2 GetScrollPosition() => new Vector2(width - ScrollBar.width, 0f);

        protected override void OnMouseWheel(UIMouseEventParameter p)
        {
            p.Use();
            StartIndex = StartIndex + (p.wheelDelta > 0 ? -1 : 1) * (Utility.OnlyShiftIsPressed ? 10 : 1);
        }
        private void ScrollBarValueChanged(UIComponent component, float value)
        {
            StartIndex = (int)value;
        }
    }

    public abstract class PopupEntity<ObjectType> : CustomUIButton, IReusable
    {
        public event Action<ObjectType> OnSelected;
        bool IReusable.InCache { get; set; }

        public virtual ObjectType Object { get; protected set; }
        public bool Selected { get; set; }

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
        }

        public virtual void SetObject(ObjectType value) 
        {
            Object = value;
        }
        protected void Select() => OnSelected?.Invoke(Object);

        protected override void OnClick(UIMouseEventParameter p)
        {
            base.OnClick(p);
            Select();
        }
    }
}
