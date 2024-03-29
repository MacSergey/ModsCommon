﻿using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class ListPropertyPanel<Type, UISelector> : EditorPropertyPanel, IReusable
        where UISelector : UIComponent, IUISelector<Type>
    {
        public event Action<bool> OnDropDownStateChange;

        public UISelector Selector { get; protected set; }

        protected virtual float DropDownWidth => 230;
        protected virtual bool AllowNull => true;
        public string NullText { get; set; } = string.Empty;

        protected override void FillContent()
        {
            AddSelector();
            Selector.IsEqualDelegate = IsEqual;
        }
        protected virtual void AddSelector()
        {
            Selector = Content.AddUIComponent<UISelector>();

            Selector.SetDefaultStyle(new Vector2(DropDownWidth, 20));
            if (Selector is UIDropDown dropDown)
            {
                dropDown.eventDropdownOpen += DropDownOpen;
                dropDown.eventDropdownClose += DropDownClose;
            }
        }

        private void DropDownOpen(UIDropDown dropdown, UIListBox popup, ref bool overridden)
        {
            OnDropDownStateChange?.Invoke(true);
        }
        private void DropDownClose(UIDropDown dropdown, UIListBox popup, ref bool overridden)
        {
            OnDropDownStateChange?.Invoke(false);
        }

        protected override void Init(float? height)
        {
            base.Init(height);
            Selector.Clear();

            if (AllowNull)
                Selector.AddItem(default, new OptionData(NullText ?? string.Empty));
        }
        public override void DeInit()
        {
            base.DeInit();
            OnDropDownStateChange = null;
            Selector.Clear();
        }
        public void Add(Type item) => Selector.AddItem(item, new OptionData());
        public void AddRange(IEnumerable<Type> items)
        {
            foreach (var item in items)
                Selector.AddItem(item, new OptionData());
        }
        protected abstract bool IsEqual(Type first, Type second);
    }
    public abstract class ListOncePropertyPanel<Type, UISelector> : ListPropertyPanel<Type, UISelector>, IReusable
        where UISelector : UIComponent, IUIOnceSelector<Type>
    {
        public event Action<Type> OnSelectObjectChanged;

        public Type SelectedObject
        {
            get => Selector.SelectedObject;
            set => Selector.SelectedObject = value;
        }
        public bool UseWheel
        {
            get => Selector.UseWheel;
            set => Selector.UseWheel = value;
        }
        public bool WheelTip
        {
            set => Selector.WheelTip = value;
        }

        protected override void AddSelector()
        {
            base.AddSelector();
            Selector.OnSelectObject += SelectorValueChanged;
        }
        protected virtual void SelectorValueChanged(Type value) => OnSelectObjectChanged?.Invoke(value);

        public override void DeInit()
        {
            OnSelectObjectChanged = null;
            UseWheel = false;
            WheelTip = false;
            base.DeInit();
        }
        public override string ToString() => $"{base.ToString()}: {SelectedObject}";
    }
    public abstract class ListMultyPropertyPanel<Type, UISelector> : ListPropertyPanel<Type, UISelector>, IReusable
        where UISelector : UIComponent, IUIMultySelector<Type>
    {
        public event Action<List<Type>> OnSelectObjectsChanged;

        public List<Type> SelectedObjects
        {
            get => Selector.SelectedObjects;
            set => Selector.SelectedObjects = value;
        }

        protected override void AddSelector()
        {
            base.AddSelector();
            Selector.OnSelectedObjectsChanged += SelectorValueChanged;
        }
        protected virtual void SelectorValueChanged(List<Type> value) => OnSelectObjectsChanged?.Invoke(value);

        public override void DeInit()
        {
            OnSelectObjectsChanged = null;
            base.DeInit();
        }
        public override string ToString() => $"{base.ToString()}: {string.Join(",", SelectedObjects.Select(i => i.ToString()).ToArray())}";
    }
}
