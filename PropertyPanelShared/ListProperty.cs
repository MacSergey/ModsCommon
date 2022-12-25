using ColossalFramework.UI;
using NodeMarkup.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ModsCommon.UI.StringListPropertyPanel;

namespace ModsCommon.UI
{
    public abstract class ListPropertyPanel<Type, UISelector> : EditorPropertyPanel, IReusable
        where UISelector : UIComponent, IUISelector<Type>
    {
        public event Action<bool> OnDropDownStateChange;

        bool IReusable.InCache { get; set; }
        public UISelector Selector { get; protected set; }

        protected virtual float DropDownWidth => 230;
        protected virtual bool AllowNull => true;
        public string NullText { get; set; } = string.Empty;

        public ListPropertyPanel()
        {
            AddSelector();
            Selector.IsEqualDelegate = IsEqual;
        }
        protected virtual void AddSelector()
        {
            Selector = Content.AddUIComponent<UISelector>();

            Selector.SetDefaultStyle(new Vector2(DropDownWidth, 20));
            Selector.eventSizeChanged += SelectorSizeChanged;
            if (Selector is UIDropDown dropDown)
            {
                dropDown.eventDropdownOpen += DropDownOpen;
                dropDown.eventDropdownClose += DropDownClose;
            }
        }
        private void SelectorSizeChanged(UIComponent component, Vector2 value) => Refresh();

        private void DropDownOpen(UIDropDown dropdown, UIListBox popup, ref bool overridden)
        {
            dropdown.triggerButton.isInteractive = false;
            OnDropDownStateChange?.Invoke(true);
        }
        private void DropDownClose(UIDropDown dropdown, UIListBox popup, ref bool overridden)
        {
            dropdown.triggerButton.isInteractive = true;
            OnDropDownStateChange?.Invoke(false);
        }

        protected override void Init(float? height)
        {
            base.Init(height);
            Selector.Clear();

            if (AllowNull)
                Selector.AddItem(default, NullText ?? string.Empty);
        }
        public override void DeInit()
        {
            base.DeInit();
            OnDropDownStateChange = null;
            Selector.Clear();
        }
        public void Add(Type item) => Selector.AddItem(item);
        public void AddRange(IEnumerable<Type> items)
        {
            foreach (var item in items)
                Selector.AddItem(item);
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
            Selector.OnSelectObjectChanged += SelectorValueChanged;
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
            Selector.OnSelectObjectsChanged += SelectorValueChanged;
        }
        protected virtual void SelectorValueChanged(List<Type> value) => OnSelectObjectsChanged?.Invoke(value);

        public override void DeInit()
        {
            OnSelectObjectsChanged = null;
            base.DeInit();
        }
        public override string ToString() => $"{base.ToString()}: {string.Join(",", SelectedObjects.Select(i => i.ToString()).ToArray())}";
    }

    public class StringListPropertyPanel : ListOncePropertyPanel<string, StringDropDown>
    {
        protected override bool AllowNull => true;
        protected override bool IsEqual(string first, string second) => first == second;

        public override void Init() => Init(new string[0], new string[0]);
        public void Init(string[] values, string[] labels = null)
        {
            base.Init(null);

            Selector.StopLayout();
            if (labels == null)
                labels = values;
            var count = Math.Min(values.Length, labels.Length);
            for (int i = 0; i < count; i += 1)
            {
                Selector.AddItem(values[i], labels[i]);
            }
            Selector.StartLayout();
        }
    }
}
