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

namespace ModsCommon
{
    public static class SettingsHelper
    {
        public static FloatSettingsItem AddFloatField(UIHelper group, string label, SavedFloat saved, float? min = null, float? max = null)
        {
            var item = (group.self as UIPanel).AddUIComponent<FloatSettingsItem>();
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
        public static IntSettingsItem AddIntField(UIHelper group, string label, SavedInt saved, int? min = null, int? max = null)
        {
            var item = (group.self as UIPanel).AddUIComponent<IntSettingsItem>();
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
        public static StringSettingsItem AddStringField(UIHelper group, string label, SavedString saved)
        {
            var item = (group.self as UIPanel).AddUIComponent<StringSettingsItem>();
            item.Label = label;

            item.Control.Value = saved;
            item.Control.OnValueChanged += OnValueChanged;

            void OnValueChanged(string value)
            {
                saved.value = value;
            };

            return item;
        }

        public static ToggleSettingsItem AddToggle(UIHelper group, string label, SavedBool saved)
        {
            var item = (group.self as UIPanel).AddUIComponent<ToggleSettingsItem>();
            item.Label = label;

            item.Control.State = saved;
            item.Control.OnStateChanged += OnStateChanged;

            void OnStateChanged(bool value)
            {
                saved.value = value;
            };

            return item;
        }
        public static OptionPanelWithLabelData AddTogglePanel(UIHelper group, string mainLabel, SavedInt optionsSaved, string[] labels, Action onChanged = null)
        {
            var item = (group.self as UIPanel).AddUIComponent<LabelSettingsItem>();
            item.Label = mainLabel;

            var result = AddCheckboxPanel(group, optionsSaved, labels, 0, onChanged);

            return new OptionPanelWithLabelData()
            {
                label = item,
                panel = result.panel,
                checkBoxes = result.checkBoxes,
            };
        }
        public static OptionPanelWithMainData AddTogglePanel(UIHelper group, string mainLabel, SavedBool mainSaved, SavedInt optionsSaved, string[] labels, Action onChanged = null)
        {
            var item = (group.self as UIPanel).AddUIComponent<ToggleSettingsItem>();
            item.Label = mainLabel;

            item.Control.State = mainSaved;

            var result = AddCheckboxPanel(group, optionsSaved, labels, 0, onChanged);
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

        public static KeymappingSettingsItem AddKeyMappingButton(UIHelper group, Shortcut shortcut)
        {
            var item = (group.self as UIPanel).AddUIComponent<KeymappingSettingsItem>();
            item.Shortcut = shortcut;

            return item;
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
        private static CustomUIPanel AddPanel(UIHelper group, int padding = 25)
        {
            var optionsPanel = (group.self as UIComponent).AddUIComponent<CustomUIPanel>();
            optionsPanel.autoLayout = true;
            optionsPanel.autoLayoutDirection = LayoutDirection.Vertical;
            optionsPanel.autoFitChildrenHorizontally = true;
            optionsPanel.autoFitChildrenVertically = true;
            optionsPanel.autoLayoutPadding = new RectOffset(padding, 0, 0, 0);
            return optionsPanel;
        }
        public static CustomUIButton AddButton(UIHelper group, string text, OnButtonClicked click, float? width = 400, float? textScale = null)
        {
            var button = (group.self as UIComponent).AddUIComponent<CustomUIButton>();
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
        public static UILabel AddLabel(UIHelper helper, string text, float size = 1.125f, Color? color = null, int padding = 0)
        {
            var component = helper.self as UIComponent;

            var temp = UITemplateManager.GetAsGameObject("OptionsCheckBoxTemplate").GetComponent<UIComponent>();
            var label = temp.Find<UILabel>("Label");
            component.AttachUIComponent(label.gameObject);
            GameObject.Destroy(temp.gameObject);
            label.textScale = size;
            label.textColor = color ?? Color.white;
            label.padding = new RectOffset(padding, 0, 0, 5);
            label.wordWrap = true;
            label.autoSize = false;
            label.autoHeight = true;
            if (component is UIPanel panel)
                label.width = panel.width - panel.padding.left;

            //text should be set after everything, otherwise it causes game crash on chenise localization
            label.text = text;

            return label;
        }

        public static SettingsItem AddHorizontalPanel(UIHelper helper, RectOffset padding)
        {
            var optionsPanel = (helper.self as UIComponent).AddUIComponent<SettingsItem>();
            optionsPanel.Content.autoLayoutPadding = padding;
            return optionsPanel;
            //var component = helper.self as UIComponent;

            //var panel = component.AddUIComponent<UIPanel>();
            //panel.autoLayout = true;
            //panel.autoLayoutDirection = LayoutDirection.Horizontal;
            //panel.autoLayoutPadding = padding;
            //panel.autoFitChildrenHorizontally = true;
            //panel.autoFitChildrenVertically = true;
            //return new UIHelper(panel);
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
    public class UIAdvancedHelper : UIHelper, IEnumerable<UIHelper>
    {
        private static UIDynamicFont SemiBoldFont { get; }
        private List<UIHelper> Groups { get; } = new List<UIHelper>();
        public UIAutoLayoutScrollablePanel Content => self as UIAutoLayoutScrollablePanel;
        public UIAdvancedHelper(UIAutoLayoutScrollablePanel panel) : base(panel) { }

        static UIAdvancedHelper()
        {
            var panel = UITemplateManager.Get<UIPanel>("OptionsGroupTemplate");
            SemiBoldFont = panel.Find<UILabel>("Label").font as UIDynamicFont;
            GameObject.Destroy(panel);
        }

        public new UIHelper AddGroup(string name = null) => AddGroup(out _, name);
        public UIHelper AddGroup(out UILabel label, string name = null)
        {
            var group = AddGroupBase(out label, name);
            var content = group.self as CustomUIPanel;

            content.autoLayoutPadding = new RectOffset(0, 0, 0, 5);
            content.padding = new RectOffset(14, 14, 0, 0);
            content.verticalSpacing = 10;

            return group;
        }

        public UIHelper AddOptionsGroup(string name = null) => AddOptionsGroup(out _, name);
        public UIHelper AddOptionsGroup(out UILabel label, string name = null)
        {
            var group = AddGroupBase(out label, name);
            var content = group.self as CustomUIPanel;

            content.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            content.padding = new RectOffset(40, 0, 0, 0);
            content.eventComponentAdded += ComponentAdded;
            content.eventSizeChanged += SizeChanged;

            return group;

            static void ComponentAdded(UIComponent container, UIComponent child)
            {
                if (child is SettingsItem item)
                    item.width = container.width - (container as UIPanel).padding.horizontal;
            }
            static void SizeChanged(UIComponent component, Vector2 value)
            {
                foreach (var child in component.components)
                {
                    if (child is SettingsItem item)
                        item.width = component.width - (component as UIPanel).padding.horizontal;
                }
            }
        }

        private UIHelper AddGroupBase(out UILabel label, string name = null)
        {
            var panel = Content.AddUIComponent<CustomUIPanel>();
            panel.width = 738f;
            panel.autoLayout = true;
            panel.autoFitChildrenVertically = true;
            panel.autoLayoutDirection = LayoutDirection.Vertical;
            panel.autoLayoutPadding = new RectOffset(0, 0, 0, 15);

            panel.atlas = CommonTextures.Atlas;
            panel.backgroundSprite = CommonTextures.PanelBig;
            panel.color = new Color32(20, 25, 38, 255);

            label = panel.AddUIComponent<CustomUILabel>();
            label.name = "Title";
            label.font = SemiBoldFont;
            label.textScale = 1.3f;
            label.padding = new RectOffset(12, 0, 12, 0);
            if (!string.IsNullOrEmpty(name))
                label.text = name;
            else
                label.isVisible = false;

            var content = panel.AddUIComponent<CustomUIPanel>();
            content.name = "Content";
            content.autoLayout = true;
            content.autoFitChildrenVertically = true;
            content.autoLayoutDirection = LayoutDirection.Vertical;
            content.width = 738f;


            var group = new UIHelper(content);
            Groups.Add(group);

            return group;
        }

        public IEnumerator<UIHelper> GetEnumerator() => Groups.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
