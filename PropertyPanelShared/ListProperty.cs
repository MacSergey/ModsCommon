using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class ListPropertyPanel<ValueType, SelectorType, RefType> : EditorPropertyPanel, IReusable
        where SelectorType : UIComponent, ISelector<ValueType, RefType>
        where RefType : ISelectorRef
    {
        public event Action<bool> OnDropDownStateChange;

        protected SelectorType Selector { get; private set; }
        public RefType SelectorRef => Selector.Ref;

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
            Selector = Content.AddUIComponent<SelectorType>();

            Selector.SetDefaultStyle(new Vector2(DropDownWidth, 20));
            if (Selector is UIDropDown dropDown)
            {
                dropDown.eventDropdownOpen += DropDownOpen;
                dropDown.eventDropdownClose += DropDownClose;
            }
        }
        protected virtual void ClearSelector()
        {
            Selector.Clear();
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
            ClearSelector();

            if (AllowNull)
                Selector.AddItem(default, new OptionData(NullText ?? string.Empty));
        }
        public override void DeInit()
        {
            base.DeInit();
            OnDropDownStateChange = null;
            ClearSelector();
        }
        public void Add(ValueType item) => Selector.AddItem(item, new OptionData());
        public void AddRange(IEnumerable<ValueType> items)
        {
            foreach (var item in items)
                Selector.AddItem(item, new OptionData());
        }
        protected abstract bool IsEqual(ValueType first, ValueType second);
    }
    public abstract class ListSinglePropertyPanel<ValueType, SelectorType, RefType> : ListPropertyPanel<ValueType, SelectorType, RefType>, IReusable
        where SelectorType : UIComponent, ISingleSelector<ValueType, RefType>
        where RefType : ISelectorRef
    {
        public event Action<ValueType> OnSelectObjectChanged;

        public ValueType SelectedObject
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
        protected virtual void SelectorValueChanged(ValueType value) => OnSelectObjectChanged?.Invoke(value);

        public override void DeInit()
        {
            OnSelectObjectChanged = null;
            UseWheel = false;
            WheelTip = false;
            base.DeInit();
        }
        public override string ToString() => $"{base.ToString()}: {SelectedObject}";
    }
    public abstract class ListMultiPropertyPanel<ValueType, SelectorType, RefType> : ListPropertyPanel<ValueType, SelectorType, RefType>, IReusable
        where SelectorType : UIComponent, IMultiSelector<ValueType, RefType>
        where RefType : ISelectorRef
    {
        public event Action<List<ValueType>> OnSelectObjectsChanged;

        public List<ValueType> SelectedObjects
        {
            get => Selector.SelectedObjects;
            set => Selector.SelectedObjects = value;
        }

        protected override void AddSelector()
        {
            base.AddSelector();
            Selector.OnSelectedObjectsChanged += SelectorValueChanged;
        }
        protected virtual void SelectorValueChanged(List<ValueType> value) => OnSelectObjectsChanged?.Invoke(value);

        public override void DeInit()
        {
            OnSelectObjectsChanged = null;
            base.DeInit();
        }
        public override string ToString() => $"{base.ToString()}: {string.Join(",", SelectedObjects.Select(i => i.ToString()).ToArray())}";
    }
}
