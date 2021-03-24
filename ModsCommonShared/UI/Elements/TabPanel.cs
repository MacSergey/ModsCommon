using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class TabStrip<TabType> : CustomUIPanel
        where TabType : Tab
    {
        public Action<int> SelectedTabChanged;
        private int _selectedTab;
        public int SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (value != _selectedTab)
                {
                    _selectedTab = value;
                    SelectedTabChanged?.Invoke(_selectedTab);
                }
            }
        }
        protected List<TabType> Tabs { get; } = new List<TabType>();

        public TabStrip()
        {
            clipChildren = true;
        }
        public override void Update()
        {
            base.Update();

            for (var i = 0; i < Tabs.Count; i += 1)
            {
                var tab = Tabs[i];

                if (i == SelectedTab)
                    tab.state = UIButton.ButtonState.Focused;
                else if (!tab.Hovered)
                    tab.state = UIButton.ButtonState.Normal;
            }
        }
        public void AddTab(string name, float textScale = 0.85f) => AddTabImpl(name, textScale);
        protected TabType AddTabImpl(string name, float textScale = 0.85f)
        {
            var tabButton = AddUIComponent<TabType>();
            tabButton.text = name;
            tabButton.textPadding = new RectOffset(5, 5, 2, 2);
            tabButton.textScale = textScale;
            tabButton.verticalAlignment = UIVerticalAlignment.Middle;

            SetStyle(tabButton);

            ArrangeTabs();

            return tabButton;
        }

        private bool ArrangeInProgress { get; set; }
        public void ArrangeTabs()
        {
            var tabs = Tabs.Where(t => t.isVisible).ToArray();
            if (!tabs.Any() || ArrangeInProgress)
                return;

            ArrangeInProgress = true;

            foreach (var tab in tabs)
            {
                tab.autoSize = true;
                tab.autoSize = false;
                tab.textHorizontalAlignment = UIHorizontalAlignment.Center;
            }

            var tabRows = FillTabRows(tabs);
            ArrangeTabRows(tabRows);
            PlaceTabRows(tabRows);

            FitChildrenVertically();

            ArrangeInProgress = false;
        }
        private List<List<TabType>> FillTabRows(TabType[] tabs)
        {
            var totalWidth = tabs.Sum(t => t.width);
            var rows = (int)(totalWidth / width) + 1;
            var tabInRow = tabs.Length / rows;
            var extraRows = tabs.Length - (tabInRow * rows);

            var tabRows = new List<List<TabType>>();
            for (var i = 0; i < rows; i += 1)
            {
                var tabRow = new List<TabType>();
                tabRows.Add(tabRow);

                var from = i * tabInRow + Math.Min(i, extraRows);
                var to = from + tabInRow + (i < extraRows ? 1 : 0);
                for (var j = from; j < to; j += 1)
                    tabRow.Add(tabs[j]);
            }
            return tabRows;
        }
        private void ArrangeTabRows(List<List<TabType>> tabRows)
        {
            for (var i = 0; i < tabRows.Count; i += 1)
            {
                var tabRow = tabRows[i];
                var totalRowWidth = 0f;
                for (var j = 0; j < tabRow.Count; j += 1)
                {
                    if (totalRowWidth + tabRow[j].width > width)
                    {
                        var toMove = tabRow.Skip(j == 0 ? j + 1 : j).ToArray();

                        if (toMove.Any())
                        {
                            if (i == tabRows.Count - 1)
                                tabRows.Add(new List<TabType>());

                            tabRows[i + 1].InsertRange(0, toMove);
                            foreach (var tab in toMove)
                                tabRow.Remove(tab);
                        }

                        break;
                    }
                    else
                        totalRowWidth += tabRow[j].width;
                }
            }
        }
        private void PlaceTabRows(List<List<TabType>> tabRows)
        {
            var totalHeight = 0f;
            for (var i = 0; i < tabRows.Count; i += 1)
            {
                var tabRow = tabRows[i];

                var rowWidth = tabRow.Sum(t => t.width);
                var rowHeight = tabRow.Max(t => t.height);
                if (i < tabRows.Count - 1)
                    rowHeight += 4;

                var space = (width - rowWidth) / tabRow.Count;
                var totalRowWidth = 0f;

                for (var j = 0; j < tabRow.Count; j += 1)
                {
                    var tab = tabRow[j];

                    tab.width = j < tabRow.Count - 1 ? Mathf.Floor(tab.width + space) : width - totalRowWidth;
                    tab.height = rowHeight;
                    tab.relativePosition = new Vector2(totalRowWidth, totalHeight);
                    totalRowWidth += tab.width;
                }

                totalHeight += rowHeight - 4;
            }
        }

        protected override void OnComponentAdded(UIComponent child)
        {
            base.OnComponentAdded(child);

            if (child is TabType tabButton)
            {
                tabButton.eventClick += TabClick;
                tabButton.eventIsEnabledChanged += TabButtonIsEnabledChanged;
                tabButton.eventVisibilityChanged += TabButtonVisibilityChanged;
                Tabs.Add(tabButton);
            }
        }
        protected override void OnComponentRemoved(UIComponent child)
        {
            base.OnComponentRemoved(child);

            if (child is TabType tabButton)
            {
                tabButton.eventClick -= TabClick;
                tabButton.eventIsEnabledChanged -= TabButtonIsEnabledChanged;
                tabButton.eventVisibilityChanged -= TabButtonVisibilityChanged;
                Tabs.Remove(tabButton);
            }
        }
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            ArrangeTabs();
        }

        private void TabClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (component is TabType tabButton)
                SelectedTab = Tabs.IndexOf(tabButton);
        }

        private void TabButtonIsEnabledChanged(UIComponent component, bool value)
        {
            if (!component.isEnabled)
            {
                var button = component as TabType;
                button.disabledColor = button.state == UIButton.ButtonState.Focused ? button.focusedColor : button.color;
            }
        }
        private void TabButtonVisibilityChanged(UIComponent component, bool value) => ArrangeTabs();

        protected virtual void SetStyle(TabType tabButton)
        {
            tabButton.atlas = TextureHelper.CommonAtlas;

            tabButton.normalBgSprite = TextureHelper.TabNormal;
            tabButton.focusedBgSprite = TextureHelper.TabFocused;
            tabButton.hoveredBgSprite = TextureHelper.TabHover;
        }
    }
    public class Tab : CustomUIButton
    {
        public bool Hovered => m_IsMouseHovering;
    }
    public class TabStrip : TabStrip<Tab> { }
}
