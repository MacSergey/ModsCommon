using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModsCommon.UI
{
    [Obsolete("Use SimpleDropDown")]
    public abstract class UIDropDown<ValueType> : CustomUIDropDown, IReusable
    {
        public event Action<ValueType> OnSelectedObjectChanged;

        bool IReusable.InCache { get; set; }
        public Func<ValueType, ValueType, bool> IsEqualDelegate { get; set; }
        private List<ValueType> Objects { get; } = new List<ValueType>();
        public ValueType SelectedObject
        {
            get => selectedIndex >= 0 ? Objects[selectedIndex] : default;
            set => selectedIndex = Objects.FindIndex(o => IsEqualDelegate?.Invoke(o, value) ?? ReferenceEquals(o, value) || (o != null && o.Equals(value)));
        }
        public bool CanWheel { get; set; }
        public bool UseWheel { get; set; }
        public bool WheelTip
        {
            set => tooltip = value ? CommonLocalize.ListPanel_ScrollWheel : string.Empty;
        }
        public bool UseScrollBar { get; set; }

        public UIDropDown()
        {
            eventSelectedIndexChanged += IndexChanged;
            eventDropdownOpen += DropdownOpen;
            eventDropdownClose += DropDownClose;
        }

        protected virtual void IndexChanged(UIComponent component, int value) => OnSelectedObjectChanged?.Invoke(SelectedObject);
        private void DropdownOpen(UIDropDown dropdown, UIListBox popup, ref bool overridden)
        {
            if (triggerButton != null)
                triggerButton.isInteractive = false;

            var position = popup.selectedIndex * popup.itemHeight;
            popup.scrollPosition = position;
        }
        private void DropDownClose(UIDropDown dropdown, UIListBox popup, ref bool overridden)
        {
            if (triggerButton != null)
                triggerButton.isInteractive = true;
        }

        public void AddItem(ValueType item, OptionData data)
        {
            Objects.Add(item);
            AddItem(data.label ?? item.ToString());
        }
        public void Clear()
        {
            selectedIndex = -1;
            Objects.Clear();
            items = new string[0];
        }
        protected override void OnMouseMove(UIMouseEventParameter p)
        {
            base.OnMouseMove(p);
            CanWheel = true;
        }
        protected override void OnMouseLeave(UIMouseEventParameter p)
        {
            base.OnMouseLeave(p);
            CanWheel = false;
        }
        protected sealed override void OnMouseWheel(UIMouseEventParameter p)
        {
            m_TooltipShowing = true;
            tooltipBox.Hide();

            if (UseWheel && (CanWheel || Time.realtimeSinceStartup - m_HoveringStartTime >= UIHelper.PropertyScrollTimeout))
            {
                if (p.wheelDelta > 0 && selectedIndex > 0)
                    selectedIndex -= 1;
                else if (p.wheelDelta < 0 && selectedIndex < Objects.Count - 1)
                    selectedIndex += 1;

                p.Use();
            }
        }

        public void StopLayout() { }
        public void StartLayout(bool layoutNow = true) { }

        void IReusable.DeInit()
        {
            Clear();
            UseWheel = false;
            WheelTip = false;
            UseScrollBar = false;
        }
    }
}
