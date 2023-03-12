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
        private static UIDynamicFont SemiBoldFont { get; }

        static Helper()
        {
            var panel = UITemplateManager.Get<UIPanel>("OptionsGroupTemplate");
            SemiBoldFont = panel.Find<UILabel>("Label").font as UIDynamicFont;
            GameObject.Destroy(panel);
        }


        public static CustomUIPanel AddGroup(this UIComponent parent)
        {
            var panel = parent.AddUIComponent<CustomUIPanel>();

            panel.width = 738f;
            panel.autoLayout = true;
            panel.autoFitChildrenVertically = true;
            panel.autoLayoutDirection = LayoutDirection.Vertical;
            panel.autoLayoutPadding = new RectOffset(0, 0, 0, 15);

            panel.atlas = CommonTextures.Atlas;
            panel.backgroundSprite = CommonTextures.PanelBig;
            panel.color = new Color32(20, 25, 38, 255);

            return panel;
        }
        public static CustomUIPanel AddOptionsGroup(this UIComponent parent, string name = null) => AddOptionsGroup(parent, out _, name);
        public static CustomUIPanel AddOptionsGroup(this UIComponent parent, out CustomUILabel label, string name = null)
        {
            return parent.AddGroup().FillOptionsGroup(out label, name);
        }
        public static CustomUIPanel FillOptionsGroup(this CustomUIPanel parent, out CustomUILabel label, string name = null)
        {
            label = parent.AddUIComponent<CustomUILabel>();
            label.name = "Title";
            label.font = SemiBoldFont;
            label.textScale = 1.3f;
            label.padding = new RectOffset(12, 0, 12, 0);
            if (!string.IsNullOrEmpty(name))
                label.text = name;
            else
                label.isVisible = false;

            var сontent = parent.AddUIComponent<CustomUIPanel>();
            сontent.name = "Content";
            сontent.autoLayout = true;
            сontent.autoFitChildrenVertically = true;
            сontent.autoLayoutDirection = LayoutDirection.Vertical;
            сontent.verticalSpacing = 5;
            сontent.width = parent.width;

            сontent.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            сontent.padding = new RectOffset(40, 0, 0, 0);
            сontent.eventComponentAdded += ComponentAdded;
            сontent.eventSizeChanged += SizeChanged;

            parent.eventSizeChanged += ParentSizeChanged;

            return сontent;

            static void ComponentAdded(UIComponent container, UIComponent child)
            {
                if (child is BaseSettingItem item)
                    item.width = container.width - (container as UIPanel).padding.horizontal;
            }

            static void SizeChanged(UIComponent component, Vector2 value)
            {
                foreach (var child in component.components)
                {
                    if (child is BaseSettingItem item)
                        item.width = component.width - (component as UIPanel).padding.horizontal;
                }
            }

            void ParentSizeChanged(UIComponent component, Vector2 value)
            {
                сontent.width = component.width;
            }
        }

        public static FloatSettingsItem AddFloatField(this UIComponent parent, string label, SavedFloat saved, float? min = null, float? max = null)
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

            void OnValueChanged(float value)
            {
                saved.value = value;
            };

            return item;
        }
        public static IntSettingsItem AddIntField(this UIComponent parent, string label, SavedInt saved, int? min = null, int? max = null)
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

            void OnValueChanged(int value)
            {
                saved.value = value;
            };

            return item;
        }
        public static StringSettingsItem AddStringField(this UIComponent parent, string label, SavedString saved)
        {
            var item = parent.AddUIComponent<StringSettingsItem>();
            item.Label = label;

            item.Control.Value = saved;
            item.Control.OnValueChanged += OnValueChanged;

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

        public static ToggleSettingsItem AddToggle(this UIComponent parent, string label, SavedBool saved)
        {
            var item = parent.AddUIComponent<ToggleSettingsItem>();
            item.Label = label;

            item.Control.State = saved;
            item.Control.OnStateChanged += OnStateChanged;

            void OnStateChanged(bool value)
            {
                saved.value = value;
            };

            return item;
        }
        public static OptionPanelWithLabelData AddTogglePanel(this UIComponent parent, string mainLabel, SavedInt optionsSaved, string[] labels, Action onChanged = null)
        {
            var item = parent.AddUIComponent<LabelSettingsItem>();
            item.Label = mainLabel;

            var result = parent.AddCheckboxPanel(optionsSaved, labels, 0, onChanged);

            return new OptionPanelWithLabelData()
            {
                label = item,
                panel = result.panel,
                checkBoxes = result.checkBoxes,
            };
        }
        public static OptionPanelWithMainData AddTogglePanel(this UIComponent parent, string mainLabel, SavedBool mainSaved, SavedInt optionsSaved, string[] labels, Action onChanged = null)
        {
            var item = parent.AddUIComponent<ToggleSettingsItem>();
            item.Label = mainLabel;

            item.Control.State = mainSaved;

            var result = parent.AddCheckboxPanel(optionsSaved, labels, 0, onChanged);
            var optionsPanel = result.panel;

            item.Control.OnStateChanged += OnStateChanged;
            SetVisible(mainSaved);

            return new OptionPanelWithMainData()
            {
                toggle = item,
                panel = result.panel,
                checkBoxes = result.checkBoxes,
            };

            void OnStateChanged(bool value)
            {
                mainSaved.value = value;
                SetVisible(value);
            };
            void SetVisible(bool visible) => optionsPanel.isVisible = visible;
        }

        public static KeymappingSettingsItem AddKeyMappingButton(this UIComponent parent, Shortcut shortcut)
        {
            var item = parent.AddUIComponent<KeymappingSettingsItem>();
            item.Shortcut = shortcut;

            return item;
        }

        private static OptionPanelData AddCheckboxPanel(this UIComponent parent, SavedInt optionsSaved, string[] labels, int padding, Action onChanged)
        {
            var inProcess = false;
            var checkBoxes = new UICheckBox[labels.Length];

            var optionsPanel = parent.AddPanel(25 + padding);
            var panelHelper = new UIHelper(optionsPanel);

            for (var i = 0; i < checkBoxes.Length; i += 1)
            {
                var index = i;
                checkBoxes[i] = panelHelper.AddCheckbox(labels[i], optionsSaved == i, (value) => Set(index, value)) as UICheckBox;
                checkBoxes[i].Find<UILabel>("Label").textScale = 0.9f;
            }

            return new OptionPanelData()
            {
                panel = optionsPanel,
                checkBoxes = checkBoxes,
            };

            void Set(int index, bool value)
            {
                if (!inProcess)
                {
                    inProcess = true;
                    optionsSaved.value = index;
                    onChanged?.Invoke();
                    for (var i = 0; i < checkBoxes.Length; i += 1)
                        checkBoxes[i].isChecked = optionsSaved == i;
                    inProcess = false;
                }
            }
        }
        private static CustomUIPanel AddPanel(this UIComponent parent, int padding = 25)
        {
            var optionsPanel = parent.AddUIComponent<CustomUIPanel>();
            optionsPanel.autoLayout = true;
            optionsPanel.autoLayoutDirection = LayoutDirection.Vertical;
            optionsPanel.autoFitChildrenHorizontally = true;
            optionsPanel.autoFitChildrenVertically = true;
            optionsPanel.autoLayoutPadding = new RectOffset(padding, 0, 0, 0);
            return optionsPanel;
        }

        public static SettingsContentItem AddButtonPanel(this UIComponent parent, RectOffset padding = null, int itemSpacing = 10, SettingsContentItem.Border border = SettingsContentItem.Border.None)
        {
            var item = parent.AddHorizontalPanel(padding ?? new RectOffset(0, 0, 5, 5), itemSpacing);
            item.Borders = border;
            return item;
        }
        public static CustomUIButton AddButton(this SettingsContentItem item, string text, OnButtonClicked click, float? width = 400, float? textScale = null)
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

            button.CustomSettingsStyle();

            return button;
        }

        public static SettingsContentItem AddHorizontalPanel(this UIComponent parent, RectOffset padding, int itemSpacing)
        {
            var optionsPanel = parent.AddUIComponent<SettingsContentItem>();
            optionsPanel.padding = padding;
            optionsPanel.Content.autoLayoutPadding = new RectOffset(0, itemSpacing, 0, 0);
            return optionsPanel;
        }
        public static EmptySpaceSettingsItem AddSpace(this UIComponent parent, float height)
        {
            var item = parent.AddUIComponent<EmptySpaceSettingsItem>();
            item.height = height;

            return item;
        }

        public struct OptionPanelData
        {
            public CustomUIPanel panel;
            public UICheckBox[] checkBoxes;
        }
        public struct OptionPanelWithMainData
        {
            public ToggleSettingsItem toggle;
            public CustomUIPanel panel;
            public UICheckBox[] checkBoxes;
        }
        public struct OptionPanelWithLabelData
        {
            public LabelSettingsItem label;
            public CustomUIPanel panel;
            public UICheckBox[] checkBoxes;
        }

        public class LanguageDropDown : AdvancedDropDown<LanguageDropDown.Language, LanguageDropDown.LanguagePopup, LanguageDropDown.LanguageEntity>
        {
            public new string SelectedObject
            {
                get => base.SelectedObject.locale;
                set => base.SelectedObject = new Language(value, string.Empty, string.Empty);
            }
            public LanguageDropDown()
            {
                ComponentStyle.CustomSettingsStyle(this, new Vector2(250f, 34f));
            }
            protected override void SetPopupStyle() => Popup.CustomSettingsStyle(34f);
            protected override void InitPopup()
            {
                Popup.AutoWidth = true;
                base.InitPopup();
            }

            public readonly struct Language
            {
                public readonly string locale;
                public readonly string label;
                public readonly string sprite;

                public Language(string locale, string label, string sprite)
                {
                    this.locale = locale;
                    this.label = label;
                    this.sprite = sprite;
                }

                public override bool Equals(object obj)
                {
                    if (obj is Language language)
                        return language.locale == locale;
                    else
                        return false;
                }
                public override int GetHashCode() => locale.GetHashCode();
                public override string ToString() => $"{locale}: {label}";
            }

            public class LanguageEntity : PopupEntity<Language>
            {
                public LanguageEntity()
                {
                    atlas = CommonTextures.Atlas;
                    foregroundSpriteMode = UIForegroundSpriteMode.Scale;

                    horizontalAlignment = UIHorizontalAlignment.Left;
                    verticalAlignment = UIVerticalAlignment.Middle;

                    textVerticalAlignment = UIVerticalAlignment.Middle;
                    textHorizontalAlignment = UIHorizontalAlignment.Left;
                    textScale = 0.9f;
                }

                public override void SetObject(int index, Language language, bool selected)
                {
                    base.SetObject(index, language, selected);

                    text = language.label;
                    normalFgSprite = language.sprite;
                }
                public override void DeInit()
                {
                    base.DeInit();

                    text = string.Empty;
                    normalFgSprite = string.Empty;
                }

                protected override void OnSizeChanged()
                {
                    textPadding = new RectOffset(Mathf.CeilToInt(height) + 4, 8, 3, 0);
                    spritePadding = new RectOffset(5, 0, 0, 0);
                    scaleFactor = 24f / height;
                }
            }
            public class LanguagePopup : Popup<Language, LanguageEntity>
            {
                protected override float DefaultEntityHeight => 34f;
            }
        }
    }
}
