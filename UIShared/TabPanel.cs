using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModsCommon.UI
{
    public abstract class TabStrip<TabType> : CustomUIPanel, IAutoLayoutPanel
        where TabType : Tab
    {
        public Action<int> SelectedTabChanged;

        protected List<TabType> Tabs { get; } = new List<TabType>();

        public int TabSpacingVertical
        {
            get => padding.vertical / 2;
            set
            {
                value = Math.Max(value, 0);
                padding.top = value;
                padding.bottom = value;
                ArrangeTabs();
            }
        }
        public int TabSpacingHorizontal
        {
            get => padding.horizontal / 2;
            set
            {
                value = Math.Max(value, 0);
                padding.left = value;
                padding.right = value;
                ArrangeTabs();
            }
        }

        private int selectedTab;
        public int SelectedTab
        {
            get => selectedTab;
            set
            {
                if (value != selectedTab)
                {
                    selectedTab = value;
                    SelectedTabChanged?.Invoke(selectedTab);
                }
            }
        }

        private UITextureAtlas tabAtlas;
        public UITextureAtlas TabAtlas
        {
            get => tabAtlas;
            set
            {
                if (value != tabAtlas)
                {
                    tabAtlas = value;
                    UpdateTabStyle();
                }
            }
        }

        private string tabNormalSprite;
        public string TabNormalSprite
        {
            get => tabNormalSprite;
            set
            {
                if (value != tabNormalSprite)
                {
                    tabNormalSprite = value;
                    UpdateTabStyle();
                }
            }
        }
        private string tabHoveredSprite;
        public string TabHoveredSprite
        {
            get => tabHoveredSprite;
            set
            {
                if (value != tabHoveredSprite)
                {
                    tabHoveredSprite = value;
                    UpdateTabStyle();
                }
            }
        }
        private string tabPressedSprite;
        public string TabPressedSprite
        {
            get => tabPressedSprite;
            set
            {
                if (value != tabPressedSprite)
                {
                    tabPressedSprite = value;
                    UpdateTabStyle();
                }
            }
        }
        private string tabFocusedSprite;
        public string TabFocusedSprite
        {
            get => tabFocusedSprite;
            set
            {
                if (value != tabFocusedSprite)
                {
                    tabFocusedSprite = value;
                    UpdateTabStyle();
                }
            }
        }
        private string tabDisabledSprite;
        public string TabDisabledSprite
        {
            get => tabDisabledSprite;
            set
            {
                if (value != tabDisabledSprite)
                {
                    tabDisabledSprite = value;
                    UpdateTabStyle();
                }
            }
        }

        private Color32 tabColor = Color.white;
        public Color32 TabColor
        {
            get => tabColor;
            set
            {
                if (!tabColor.Equals(value))
                {
                    tabColor = value;
                    UpdateTabStyle();
                }
            }
        }
        private Color32 tabHoveredColor = Color.white;
        public Color32 TabHoveredColor
        {
            get => tabHoveredColor;
            set
            {
                if (!tabHoveredColor.Equals(value))
                {
                    tabHoveredColor = value;
                    UpdateTabStyle();
                }
            }
        }
        private Color32 tabPressedColor = Color.white;
        public Color32 TabPressedColor
        {
            get => tabPressedColor;
            set
            {
                if (!tabPressedColor.Equals(value))
                {
                    tabPressedColor = value;
                    UpdateTabStyle();
                }
            }
        }
        private Color32 tabFocusedColor = Color.white;
        public Color32 TabFocusedColor
        {
            get => tabFocusedColor;
            set
            {
                if (!tabFocusedColor.Equals(value))
                {
                    tabFocusedColor = value;
                    UpdateTabStyle();
                }
            }
        }
        private Color32 tabDisabledColor = Color.white;
        public Color32 TabDisabledColor
        {
            get => tabDisabledColor;
            set
            {
                if (!tabDisabledColor.Equals(value))
                {
                    tabDisabledColor = value;
                    UpdateTabStyle();
                }
            }
        }

        private Color32 tabFocusedDisabledColor = Color.white;
        public Color32 TabFocusedDisabledColor
        {
            get => tabFocusedDisabledColor;
            set
            {
                if (!tabFocusedDisabledColor.Equals(value))
                {
                    tabFocusedDisabledColor = value;
                    UpdateTabStyle();
                }
            }
        }

        public TabStrip()
        {
            clipChildren = true;
        }
        public override void Update()
        {
            base.Update();

            for (var i = 0; i < Tabs.Count; i += 1)
            {
                if (Tabs[i].state == UIButton.ButtonState.Focused)
                    Tabs[i].state = UIButton.ButtonState.Normal;

                Tabs[i].isSelected = i == SelectedTab;
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
            if (!tabLayout)
                return;

            var tabs = Tabs.Where(t => t.isVisible).ToArray();
            if (!tabs.Any() || ArrangeInProgress)
                return;

            ArrangeInProgress = true;

            foreach (var tab in tabs)
            {
                tab.autoSize = true;
                tab.autoSize = false;
                tab.MakePixelPerfect(false);
                tab.textHorizontalAlignment = UIHorizontalAlignment.Center;
            }

            var tabRows = FillTabRows(tabs);
            ArrangeTabRows(tabRows);
            PlaceTabRows(tabRows);

            MakePixelPerfect();

            ArrangeInProgress = false;
        }
        private List<List<TabType>> FillTabRows(TabType[] tabs)
        {
            var totalWidth = tabs.Sum(t => t.width) + tabs.Length * padding.left + padding.right;
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
                var totalRowWidth = (float)padding.right;
                for (var j = 0; j < tabRow.Count; j += 1)
                {
                    if (totalRowWidth + tabRow[j].width + padding.left > width)
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
                        totalRowWidth += tabRow[j].width + padding.left;
                }
            }
        }
        private void PlaceTabRows(List<List<TabType>> tabRows)
        {
            var totalHeight = 0f;
            for (var i = 0; i < tabRows.Count; i += 1)
            {
                totalHeight += padding.top;
                var tabRow = tabRows[i];

                var rowWidth = tabRow.Sum(t => t.width) + tabRow.Count * padding.left + padding.right;
                var rowHeight = Mathf.Ceil(tabRow.Max(t => t.height));

                var additional = Mathf.Floor((width - rowWidth) / tabRow.Count);
                var totalRowWidth = (float)padding.right;

                for (var j = 0; j < tabRow.Count; j += 1)
                {
                    var tab = tabRow[j];

                    tab.width = j < tabRow.Count - 1 ? Mathf.Floor(tab.width + additional) : width - totalRowWidth - padding.right;
                    tab.height = rowHeight;
                    tab.relativePosition = new Vector2(totalRowWidth, totalHeight);
                    totalRowWidth += tab.width + padding.left;
                }

                totalHeight += rowHeight;
            }

            height = totalHeight + padding.bottom;
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
                button.disabledColor = button.state == UIButton.ButtonState.Focused ? button.focusedBgColor : button.normalBgColor;
            }
        }
        private void TabButtonVisibilityChanged(UIComponent component, bool value) => ArrangeTabs();

        private void UpdateTabStyle()
        {
            foreach (var tab in Tabs)
                SetStyle(tab);
        }
        protected virtual void SetStyle(TabType tabButton)
        {
            tabButton.atlas = tabAtlas;
            tabButton.SetBgSprite(new SpriteSet(TabNormalSprite, TabHoveredSprite, TabPressedSprite, TabFocusedSprite, TabDisabledSprite));
            tabButton.SetBgColor(new ColorSet(TabColor, TabHoveredColor, TabPressedColor, TabFocusedColor, TabDisabledColor));
            tabButton.SetSelectedBgColor(new ColorSet(TabFocusedColor, TabFocusedColor, TabFocusedColor, TabFocusedColor, TabFocusedDisabledColor));
        }

        private bool tabLayout = true;
        public void StopLayout()
        {
            tabLayout = false;
        }

        public void StartLayout(bool layoutNow = true)
        {
            tabLayout = true;
            ArrangeTabs();
        }
    }
    public class Tab : CustomUIButton
    {
        public bool Hovered => m_IsMouseHovering;
    }
    public class TabStrip : TabStrip<Tab> { }
}
