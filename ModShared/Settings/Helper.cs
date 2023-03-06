using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using ModsCommon.UI;
using ModsCommon.Utilities;
using System;
using UnityEngine;

namespace ModsCommon
{
    public static class SettingsHelper
    {
        public static FloatSettingsItem AddFloatField(UIHelper group, string label, SavedFloat saved, float? min = null, float? max = null)
        {
            var item = (group.self as UIPanel).AddUIComponent<FloatSettingsItem>();
            item.Text = label;

            if(min.HasValue)
            {
                item.Field.CheckMin = true;
                item.Field.MinValue = min.Value;
            }    
            if(max.HasValue)
            {
                item.Field.CheckMax = true;
                item.Field.MaxValue = max.Value;    
            }
            item.Field.Value = saved;
            item.Field.OnValueChanged += OnValueChanged;

            void OnValueChanged(float value)
            {
                saved.value = value;
            };

            return item;
        }
        public static IntSettingsItem AddIntField(UIHelper group, string label, SavedInt saved, int? min = null, int? max = null)
        {
            var item = (group.self as UIPanel).AddUIComponent<IntSettingsItem>();
            item.Text = label;

            if (min.HasValue)
            {
                item.Field.CheckMin = true;
                item.Field.MinValue = min.Value;
            }
            if (max.HasValue)
            {
                item.Field.CheckMax = true;
                item.Field.MaxValue = max.Value;
            }
            item.Field.Value = saved;
            item.Field.OnValueChanged += OnValueChanged;

            void OnValueChanged(int value)
            {
                saved.value = value;
            };

            return item;
        }
        public static StringSettingsItem AddStringField(UIHelper group, string label, SavedString saved)
        {
            var item = (group.self as UIPanel).AddUIComponent<StringSettingsItem>();
            item.Text = label;

            item.Field.Value = saved;
            item.Field.OnValueChanged += OnValueChanged;

            void OnValueChanged(string value)
            {
                saved.value = value;
            };

            return item;
        }
        public static KeymappingSettingItem AddKeyMappingButton(UIHelper group, Shortcut shortcut)
        {
            var item = (group.self as UIPanel).AddUIComponent<KeymappingSettingItem>();
            item.Shortcut = shortcut;

            return item;
        }

        public static UICheckBox AddCheckBox(UIHelper group, string label, SavedBool saved, Action onChanged = null)
        {
            var checkbox = group.AddCheckbox(label, saved, OnValueChanged) as UICheckBox;
            return checkbox;

            void OnValueChanged(bool value)
            {
                saved.value = value;
                onChanged?.Invoke();
            }
        }

        public static OptionPanelWithLabelData AddCheckboxPanel(UIHelper group, string mainLabel, SavedInt optionsSaved, string[] labels, Action onChanged = null)
        {
            var label = AddLabel(group, mainLabel, padding: 0);
            var result = AddCheckboxPanel(group, optionsSaved, labels, 0, onChanged);

            return new OptionPanelWithLabelData()
            {
                label = label,
                panel = result.panel,
                checkBoxes = result.checkBoxes,
            };
        }
        public static OptionPanelWithMainData AddCheckboxPanel(UIHelper group, string mainLabel, SavedBool mainSaved, SavedInt optionsSaved, string[] labels, Action onChanged = null)
        {
            var optionsPanel = default(CustomUIPanel);
            var mainCheckBox = group.AddCheckbox(mainLabel, mainSaved, OnMainChanged) as UICheckBox;
            var result = AddCheckboxPanel(group, optionsSaved, labels, 0, onChanged);
            optionsPanel = result.panel;

            SetVisible();

            return new OptionPanelWithMainData()
            {
                mainCheckBox = mainCheckBox,
                panel = result.panel,
                checkBoxes = result.checkBoxes,
            };

            void OnMainChanged(bool value)
            {
                mainSaved.value = value;
                onChanged?.Invoke();
                SetVisible();
            }
            void SetVisible() => optionsPanel.isVisible = mainSaved;
        }
        private static OptionPanelData AddCheckboxPanel(UIHelper group, SavedInt optionsSaved, string[] labels, int padding, Action onChanged)
        {
            var inProcess = false;
            var checkBoxes = new UICheckBox[labels.Length];

            var optionsPanel = AddPanel(group, 25 + padding);
            var panelHelper = new UIHelper(optionsPanel);

            for (var i = 0; i < checkBoxes.Length; i += 1)
            {
                var index = i;
                checkBoxes[i] = panelHelper.AddCheckbox(labels[i], optionsSaved == i, (value) => Set(index, value)) as UICheckBox;
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
        public static CustomUIPanel AddPanel(UIHelper group, int padding = 25)
        {
            var optionsPanel = (group.self as UIComponent).AddUIComponent<CustomUIPanel>();
            optionsPanel.autoLayout = true;
            optionsPanel.autoLayoutDirection = LayoutDirection.Vertical;
            optionsPanel.autoFitChildrenHorizontally = true;
            optionsPanel.autoFitChildrenVertically = true;
            optionsPanel.autoLayoutPadding = new RectOffset(padding, 0, 0, 5);
            return optionsPanel;
        }
        public static UIButton AddButton(UIHelper group, string text, OnButtonClicked click, float? width = 400, float? textScale = null)
        {
            var button = group.AddButton(text, click) as UIButton;
            if (textScale != null)
                button.textScale = textScale.Value;
            button.autoSize = false;
            if (width != null)
                button.width = width.Value;

            button.CustomStyle();

            return button;
        }
        public static UILabel AddLabel(UIHelper helper, string text, float size = 1.125f, Color? color = null, int padding = 0)
        {
            var component = helper.self as UIComponent;

            var temp = UITemplateManager.GetAsGameObject("OptionsCheckBoxTemplate").GetComponent<UIComponent>();
            var label = temp.Find<UILabel>("Label");
            component.AttachUIComponent(label.gameObject);
            GameObject.Destroy(temp.gameObject);
            label.textScale = size;
            label.textColor = color ?? Color.white;
            label.padding = new RectOffset(padding, 0, 0, 0);
            label.wordWrap = true;
            label.autoSize = false;
            label.autoHeight = true;
            if (component is UIPanel panel)
                label.width = panel.width - panel.padding.left;

            //text should be set after everything, otherwise it cause game crash on chenise localization
            label.text = text;

            return label;
        }
        public static UIHelper AddHorizontalPanel(UIHelper helper, RectOffset padding)
        {
            var component = helper.self as UIComponent;

            var panel = component.AddUIComponent<UIPanel>();
            panel.autoLayout = true;
            panel.autoLayoutDirection = LayoutDirection.Horizontal;
            panel.autoLayoutPadding = padding;
            panel.autoFitChildrenHorizontally = true;
            panel.autoFitChildrenVertically = true;
            return new UIHelper(panel);
        }

        public struct OptionPanelData
        {
            public CustomUIPanel panel;
            public UICheckBox[] checkBoxes;
        }
        public struct OptionPanelWithMainData
        {
            public UICheckBox mainCheckBox;
            public CustomUIPanel panel;
            public UICheckBox[] checkBoxes;
        }
        public struct OptionPanelWithLabelData
        {
            public UILabel label;
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
            protected override void InitPopup()
            {
                Popup.CustomSettingsStyle(34f);
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
