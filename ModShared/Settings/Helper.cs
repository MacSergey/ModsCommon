using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using ModsCommon.UI;
using ModsCommon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModsCommon.Settings
{
    public static partial class Helper
    {
        public static CustomUIPanel AddSection(this UIComponent parent)
        {
            var panel = parent.AddUIComponent<CustomUIPanel>();

            panel.width = 738f;
            panel.AutoChildrenVertically = AutoLayoutChildren.Fit;
            panel.AutoChildrenHorizontally = AutoLayoutChildren.Fill;
            panel.AutoLayoutSpace = 15;
            panel.AutoLayout = AutoLayout.Vertical;

            panel.Atlas = CommonTextures.Atlas;
            panel.BackgroundSprite = CommonTextures.PanelLarge;
            panel.NormalBgColor = ComponentStyle.SettingsColor15;
            panel.ForegroundSprite = CommonTextures.BorderLarge;
            panel.NormalFgColor = ComponentStyle.SettingsColor30;

            return panel;
        }
        public static SettingsItemSection AddOptionsSection(this UIComponent parent, string name = null) => AddOptionsSection(parent, out _, name);
        public static SettingsItemSection AddOptionsSection(this UIComponent parent, out CustomUILabel label, string name = null)
        {
            return parent.AddSection().FillOptionsSection(out label, name);
        }
        public static SettingsItemSection FillOptionsSection(this CustomUIPanel parent, out CustomUILabel label, string name = null)
        {
            parent.StopLayout();

            if (!string.IsNullOrEmpty(name))
            {
                label = parent.AddUIComponent<CustomUILabel>();
                label.name = "Title";
                label.autoHeight = true;
                label.font = ComponentStyle.SemiBoldFont;
                label.textScale = 1.3f;
                label.padding = new RectOffset(12, 0, 12, 0);
                label.text = name;
            }
            else
                label = null;

            var сontent = parent.AddUIComponent<SettingsItemSection>();
            сontent.PaddingTop = string.IsNullOrEmpty(name) ? 15 : 0;
            сontent.PaddingBottom = 15;

            parent.StartLayout();

            return сontent;
        }

        public static FloatSettingsItem AddFloatField(this UIComponent parent, string label, SavedFloat saved, float? min = null, float? max = null, Action<float> onValueChanged = null)
        {
            var item = parent.AddUIComponent<FloatSettingsItem>();
            item.Label = label;

            if (min.HasValue)
            {
                item.Control.CheckMin = true;
                item.Control.MinValue = min.Value;
            }
            if (max.HasValue)
            {
                item.Control.CheckMax = true;
                item.Control.MaxValue = max.Value;
            }
            item.Control.Value = saved;
            item.Control.OnValueChanged += OnValueChanged;
            if (onValueChanged != null)
                item.Control.OnValueChanged += onValueChanged;

            void OnValueChanged(float value)
            {
                saved.value = value;
            };

            return item;
        }
        public static IntSettingsItem AddIntField(this UIComponent parent, string label, SavedInt saved, int? min = null, int? max = null, Action<int> onValueChanged = null)
        {
            var item = parent.AddUIComponent<IntSettingsItem>();
            item.Label = label;

            if (min.HasValue)
            {
                item.Control.CheckMin = true;
                item.Control.MinValue = min.Value;
            }
            if (max.HasValue)
            {
                item.Control.CheckMax = true;
                item.Control.MaxValue = max.Value;
            }
            item.Control.Value = saved;
            item.Control.OnValueChanged += OnValueChanged;
            if (onValueChanged != null)
                item.Control.OnValueChanged += onValueChanged;

            void OnValueChanged(int value)
            {
                saved.value = value;
            };

            return item;
        }
        public static StringSettingsItem AddStringField(this UIComponent parent, string label, SavedString saved, Action<string> onValueChanged = null)
        {
            var item = parent.AddUIComponent<StringSettingsItem>();
            item.Label = label;

            item.Control.Value = saved;
            item.Control.OnValueChanged += OnValueChanged;
            if (onValueChanged != null)
                item.Control.OnValueChanged += onValueChanged;

            void OnValueChanged(string value)
            {
                saved.value = value;
            };

            return item;
        }

        public static LabelSettingsItem AddLabel(this UIComponent parent, string label, float textScale = 1.125f, Color? color = null)
        {
            var item = parent.AddUIComponent<LabelSettingsItem>();
            item.Label = label;
            item.LabelItem.textScale = textScale;
            item.LabelItem.textColor = color ?? Color.white;

            return item;
        }
        public static LabelSettingsItem AddInfoLabel(this UIComponent parent, string label, float textScale = 1.125f, Color? color = null)
        {
            var item = parent.AddLabel(label, textScale, color);
            item.Borders = SettingsItemBorder.None;
            item.PaddingTop = 0;

            return item;
        }

        public static ToggleSettingsItem AddToggle(this UIComponent parent, string label, SavedBool saved, Action<bool> onStateChanged = null)
        {
            var item = parent.AddUIComponent<ToggleSettingsItem>();
            item.Label = label;

            item.Control.State = saved;
            item.Control.OnStateChanged += OnStateChanged;
            if (onStateChanged != null)
                item.Control.OnStateChanged += onStateChanged;

            void OnStateChanged(bool value)
            {
                saved.value = value;
            };

            return item;
        }
        public static OptionPanelData AddTogglePanel(this UIComponent parent, string mainLabel, SavedInt optionsSaved, string[] labels, Action<int> onValueChanged = null)
        {
            var groupItem = parent.AddUIComponent<SettingsItemGroup>();

            var labelItem = groupItem.AddUIComponent<LabelSettingsItem>();
            labelItem.Label = mainLabel;

            var checkBoxItem = groupItem.AddCheckboxPanel(optionsSaved, labels, onValueChanged);
            checkBoxItem.PaddingTop = 0;

            return new OptionPanelData()
            {
                group = groupItem,
                label = labelItem,
                checkBoxes = checkBoxItem,
            };
        }
        public static OptionPanelData AddTogglePanel(this UIComponent parent, string mainLabel, SavedBool mainSaved, SavedInt optionsSaved, string[] labels, Action<bool> onStateChanged = null, Action<int> onValueChanged = null)
        {
            var groupItem = parent.AddUIComponent<SettingsItemGroup>();

            var toggleItem = groupItem.AddUIComponent<ToggleSettingsItem>();
            toggleItem.Label = mainLabel;
            toggleItem.Control.State = mainSaved;

            var checkBoxItem = groupItem.AddCheckboxPanel(optionsSaved, labels, onValueChanged);
            checkBoxItem.PaddingTop = 0;

            toggleItem.Control.OnStateChanged += OnStateChanged;
            if (onStateChanged != null)
                toggleItem.Control.OnStateChanged += onStateChanged;

            SetVisible(mainSaved);

            return new OptionPanelData()
            {
                group = groupItem,
                toggle = toggleItem,
                checkBoxes = checkBoxItem,
            };

            void OnStateChanged(bool value)
            {
                mainSaved.value = value;
                SetVisible(value);
            };
            void SetVisible(bool visible) => checkBoxItem.isVisible = visible;
        }

        public static KeymappingSettingsItem AddKeyMappingButton(this UIComponent parent, Shortcut shortcut, Action<Shortcut> onBindingChanged = null)
        {
            var item = parent.AddUIComponent<KeymappingSettingsItem>();
            item.Shortcut = shortcut;

            if (onBindingChanged != null)
                item.BindingChanged += onBindingChanged;

            return item;
        }

        private static CheckPanelSettingsItem AddCheckboxPanel(this UIComponent parent, SavedInt optionsSaved, string[] labels, Action<int> onValueChanged)
        {
            var item = parent.AddUIComponent<CheckPanelSettingsItem>();
            item.Init(labels);
            item.Value = optionsSaved;

            item.OnValueChanged += OnValueChanged;
            if (onValueChanged != null)
                item.OnValueChanged += onValueChanged;

            return item;

            void OnValueChanged(int value) => optionsSaved.value = value;
        }
        private static CustomUIPanel AddPanel(this UIComponent parent, RectOffset padding = null)
        {
            var optionsPanel = parent.AddUIComponent<CustomUIPanel>();
            optionsPanel.PauseLayout(() =>
            {
                optionsPanel.AutoLayout = AutoLayout.Vertical;
                optionsPanel.AutoChildrenHorizontally = AutoLayoutChildren.Fit;
                optionsPanel.AutoChildrenVertically = AutoLayoutChildren.Fit;
                optionsPanel.Padding = padding ?? new RectOffset();
            });
            return optionsPanel;
        }

        public static ContentSettingsItem AddButtonPanel(this UIComponent parent, RectOffset padding = null, int itemSpacing = 10)
        {
            var item = parent.AddHorizontalPanel(padding ?? new RectOffset(0, 0, 5, 5), itemSpacing);
            item.Content.AutoLayoutStart = UI.LayoutStart.TopCentre;
            item.BorderEnabled = false;
            item.CanHover = false;
            return item;
        }
        public static CustomUIButton AddButton(this ContentSettingsItem item, string text, OnButtonClicked click, float? width = 600, float? textScale = null)
        {
            var button = item.Content.AddUIComponent<CustomUIButton>();
            button.text = text;
            if (click != null)
                button.eventClick += (_, _) => click();
            if (textScale != null)
                button.textScale = textScale.Value;
            button.autoSize = false;
            button.height = 34f;
            if (width != null)
                button.width = width.Value;

            button.ButtonSettingsStyle();

            return button;
        }

        public static ContentSettingsItem AddHorizontalPanel(this UIComponent parent, RectOffset padding, int itemSpacing)
        {
            var optionsPanel = parent.AddUIComponent<ContentSettingsItem>();
            optionsPanel.Padding = padding;
            optionsPanel.Content.AutoLayoutSpace = itemSpacing;
            return optionsPanel;
        }
        public static EmptySpaceSettingsItem AddSpace(this UIComponent parent, float height)
        {
            var item = parent.AddUIComponent<EmptySpaceSettingsItem>();
            item.height = height;

            return item;
        }
        public static SettingsItemGroup AddItemsGroup(this UIComponent parent) => parent.AddUIComponent<SettingsItemGroup>();

        public struct OptionPanelData
        {
            public SettingsItemGroup group;
            public LabelSettingsItem label;
            public ToggleSettingsItem toggle;
            public CheckPanelSettingsItem checkBoxes;
        }
    }
}
