using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using ModsCommon.UI;
using ModsCommon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ModsCommon.SettingsHelper;

namespace ModsCommon
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SettingFileAttribute : Attribute
    {
        public string Name { get; }
        public SettingFileAttribute(string name)
        {
            Name = name;
        }
    }
    public abstract partial class BaseSettings<TypeMod>
        where TypeMod : ICustomMod
    {
        public static string SettingsFile { get; } = $"{typeof(TypeMod).GetCustomAttributes(false).OfType<SettingFileAttribute>().FirstOrDefault()?.Name ?? typeof(TypeMod).Namespace}{nameof(SettingsFile)}";
        public static SavedString Locale { get; } = new SavedString(nameof(Locale), SettingsFile, string.Empty, true);
        public static SavedBool ShowWhatsNew { get; } = new SavedBool(nameof(ShowWhatsNew), SettingsFile, true, true);
        public static SavedBool ShowOnlyMajor { get; } = new SavedBool(nameof(ShowOnlyMajor), SettingsFile, false, true);
        public static SavedBool BetaWarning { get; } = new SavedBool(nameof(BetaWarning), SettingsFile, true, true);
        public static SavedBool LinuxWarning { get; } = new SavedBool(nameof(LinuxWarning), SettingsFile, true, true);
        public static SavedBool AnyVersions { get; } = new SavedBool(nameof(AnyVersions), SettingsFile, false, true);


        private static SavedString WhatsNewVersionValue { get; } = new SavedString(nameof(WhatsNewVersion), SettingsFile, SingletonMod<TypeMod>.Instance.Version.PrevMinor(SingletonMod<TypeMod>.Instance.Versions.Select(v => v.Number).ToList()).ToString(), true);
        private static SavedString CompatibleCheckGameVersionValue { get; } = new SavedString(nameof(CompatibleCheckGameVersion), SettingsFile, new Version(0, 0).ToString(), true);
        private static SavedString CompatibleCheckModVersionValue { get; } = new SavedString(nameof(CompatibleCheckModVersion), SettingsFile, new Version(0, 0).ToString(), true);

        public static Version WhatsNewVersion
        {
            get => new Version(WhatsNewVersionValue);
            set => WhatsNewVersionValue.value = value.ToString();
        }
        public static Version CompatibleCheckGameVersion
        {
            get => new Version(CompatibleCheckGameVersionValue);
            set => CompatibleCheckGameVersionValue.value = value.ToString();
        }
        public static Version CompatibleCheckModVersion
        {
            get => new Version(CompatibleCheckModVersionValue);
            set => CompatibleCheckModVersionValue.value = value.ToString();
        }

        static BaseSettings()
        {
            if (GameSettings.FindSettingsFileByName(SettingsFile) == null)
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = SettingsFile } });
        }

        private UIPanel MainPanel { get; set; }
        protected TabStrip TabStrip { get; set; }
        protected List<CustomUIPanel> TabPanels { get; set; }
        private Dictionary<string, UIAdvancedHelper> Tabs { get; } = new Dictionary<string, UIAdvancedHelper>();
        protected UIAdvancedHelper GeneralTab => Tabs[nameof(GeneralTab)];
        protected UIAdvancedHelper SupportTab => Tabs[nameof(SupportTab)];
#if DEBUG
        protected UIAdvancedHelper DebugTab => Tabs[nameof(DebugTab)];
#endif
        private static Action InfoCallback { get; set; }

        public void OnSettingsUI(UIHelperBase helper)
        {
            var scrollable = (helper as UIHelper).self as UIScrollablePanel;
            MainPanel = scrollable.parent as UIPanel;

            foreach (var components in MainPanel.components)
                components.isVisible = false;

            MainPanel.autoLayout = true;
            MainPanel.autoLayoutDirection = LayoutDirection.Vertical;
            MainPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 15);

            CreateTabStrip();
            CreateTabs();
            FillSettings();

            TabStrip.SelectedTab = 0;
            TabStrip.isVisible = TabPanels.Count > 1;
        }
        protected virtual IEnumerable<KeyValuePair<string, string>> AdditionalTabs => new KeyValuePair<string, string>[0];
        protected virtual void FillSettings()
        {
            AddInfo(GeneralTab);
            AddSupport(SupportTab);
#if DEBUG
            AddDebug(DebugTab);
#endif
        }

        protected void CreateTabStrip()
        {
            TabPanels = new List<CustomUIPanel>();

            TabStrip = MainPanel.AddUIComponent<TabStrip>();
            TabStrip.SelectedTabChanged += OnSelectedTabChanged;
            TabStrip.SelectedTab = -1;
            TabStrip.width = MainPanel.width - MainPanel.autoLayoutPadding.horizontal;
            TabStrip.eventSizeChanged += (_, _) => TabStripSizeChanged();
        }
        private void OnSelectedTabChanged(int index)
        {
            if (index >= 0 && TabPanels.Count > index)
            {
                foreach (var tab in TabPanels)
                    tab.isVisible = false;

                TabPanels[index].isVisible = true;
            }
        }
        private void TabStripSizeChanged()
        {
            foreach (var tab in TabPanels)
                SetTabSize(tab);
        }
        private void SetTabSize(UIPanel panel)
        {
            panel.size = new Vector2(MainPanel.width, MainPanel.height - (TabStrip.isVisible ? MainPanel.autoLayoutPadding.vertical + TabStrip.height : 0f));
        }

        private void CreateTabs()
        {
            CreateTab(nameof(GeneralTab), CommonLocalize.Settings_GeneralTab);

            foreach (var tab in AdditionalTabs)
                CreateTab(tab.Key, tab.Value);

            CreateTab(nameof(SupportTab), CommonLocalize.Settings_SupportTab);
#if DEBUG
            CreateTab(nameof(DebugTab), "Debug");
#endif
        }
        private UIAdvancedHelper CreateTab(string name, string label)
        {
            TabStrip.AddTab(label, 1.25f);

            var tabPanel = MainPanel.AddUIComponent<AdvancedScrollablePanel>();
            tabPanel.Content.autoLayoutPadding = new RectOffset(8, 8, 0, 0);
            SetTabSize(tabPanel);
            tabPanel.isVisible = false;
            TabPanels.Add(tabPanel);

            var helper = new UIAdvancedHelper(tabPanel.Content);
            Tabs[name] = helper;
            return helper;
        }
        protected UIAdvancedHelper GetTab(string label) => Tabs[label];

        #region INFO

        private void AddInfo(UIAdvancedHelper helper)
        {
            var group = helper.AddGroup(out var title, SingletonMod<TypeMod>.Instance.NameRaw);
            title.textScale = 2f;

            AddLabel(group, string.Format(CommonLocalize.Mod_Version, SingletonMod<TypeMod>.Instance.VersionString));

            if (InfoCallback != null)
            {
                SingletonMod<TypeMod>.Instance.OnStatusChanged -= InfoCallback;
                InfoCallback = null;
            }

            var infoLabel = AddLabel(group, GetStatusText());
            infoLabel.processMarkup = true;

            InfoCallback = () =>
            {
                try
                {
                    infoLabel.text = GetStatusText();
                }
                catch
                {
                    if (InfoCallback != null)
                    {
                        SingletonMod<TypeMod>.Instance.OnStatusChanged -= InfoCallback;
                        InfoCallback = null;
                    }
                }
            };
            SingletonMod<TypeMod>.Instance.OnStatusChanged += InfoCallback;
            group.AddSpace(10);
            AddButton(group, CommonLocalize.Settings_ChangeLog, ShowChangeLog, 250f, 1f);
        }

        private string GetStatusText()
        {
            var statusText = string.Empty;
            var status = SingletonMod<TypeMod>.Instance.Status;
            if (status == ModStatus.Unknown)
                statusText = ModStatus.Unknown.Description<ModStatus, TypeMod>().AddColor(new Color32(255, 215, 81, 255));
            else if (status == ModStatus.Normal)
                statusText = ModStatus.Normal.Description<ModStatus, TypeMod>().AddColor(new Color32(65, 229, 107, 255));
            else
            {
                status &= ModStatus.WithErrors;
                var errors = status.GetEnumValues().Select(s => s.Description<ModStatus, TypeMod>()).ToArray();
                statusText = string.Join(" | ", errors).AddColor(new Color32(255, 68, 68, 255));
            }

            return string.Format(CommonLocalize.Mod_Status, statusText);
        }

        #endregion

        #region LANGUAGE

        protected void AddLanguage(UIAdvancedHelper helper)
        {
            var group = helper.AddGroup(CommonLocalize.Settings_Language);
            AddLanguageList(group);
            group.AddSpace(10);
            AddLabel(group, CommonLocalize.Settings_TranslationDescription, 0.8f);
            AddButton(group, CommonLocalize.Settings_TranslationImprove, () => SingletonMod<TypeMod>.Instance.OpenTranslationProject(), 250f, 1f);
            AddButton(group, CommonLocalize.Settings_TranslationNew, () => "https://crowdin.com/messages/create/14337258/".OpenUrl(), 250f, 1f);
        }

        private void AddLanguageList(UIHelper group)
        {
            var dropDown = (group.self as UIComponent).AddUIComponent<LanguageDropDown>();
            dropDown.AddItem(string.Empty, CommonLocalize.LocaleManager.GetString("Mod_LocaleGame", CommonLocalize.Culture));

            foreach (var locale in GetSupportLanguages())
            {
                var localizeString = $"Mod_Locale_{locale}";
                var localeText = LocalizeExtension.TryGetCulture(locale, out var culture) ? CommonLocalize.LocaleManager.GetString(localizeString, culture) : localizeString;
                if (SingletonMod<TypeMod>.Instance.Culture.Name.ToLower() != locale.ToLower())
                    localeText += $" ({CommonLocalize.LocaleManager.GetString(localizeString, CommonLocalize.Culture)})";

                dropDown.AddItem(locale, localeText);
            }

            dropDown.SelectedObject = Locale.value;
            dropDown.eventSelectedIndexChanged += IndexChanged;

            void IndexChanged(UIComponent component, int value)
            {
                var locale = dropDown.SelectedObject;
                Locale.value = locale;
                LocaleManager.ForceReload();
            }
        }
        private string[] GetSupportLanguages()
        {
            var languages = new HashSet<string> { "en-US" };
            languages.AddRange(SingletonMod<TypeMod>.Instance.GetSupportLocales());

            return languages.OrderBy(l => l).ToArray();
        }

        #endregion

        #region NOTIFICATIONS

        protected void AddNotifications(UIAdvancedHelper helper)
        {
            var group = helper.AddGroup(CommonLocalize.Settings_Notifications);
            var panel = default(CustomUIPanel);
            AddCheckBox(group, CommonLocalize.Settings_ShowWhatsNew, ShowWhatsNew, OnChange);
            panel = AddPanel(group);
            AddCheckBox(new UIHelper(panel), CommonLocalize.Settings_ShowOnlyMajor, ShowOnlyMajor);

            OnChange();

            void OnChange() => panel.isVisible = ShowWhatsNew;
        }

        #endregion

        #region SUPPORT

        private void AddSupport(UIAdvancedHelper helper)
        {
            var group = helper.AddGroup();

            AddButton(group, CommonLocalize.Settings_Troubleshooting, () => SingletonMod<TypeMod>.Instance.OpenSupport());
            AddButton(group, "Discord", () => SingletonMod<TypeMod>.Instance.OpenDiscord());
#if DEBUG
            if (SingletonMod<TypeMod>.Instance.NeedMonoDevelopDebug)
#else
            if (SingletonMod<TypeMod>.Instance.NeedMonoDevelop)
#endif
            {
                var linuxGroup = helper.AddGroup(CommonLocalize.Settings_ForLinuxUsers);
                AddButton(linuxGroup, CommonLocalize.Settings_SolveCrashOnLinux, () => SingletonMod<TypeMod>.Instance.ShowLinuxTip());
            }
        }
        private void ShowChangeLog()
        {
            var messages = SingletonMod<TypeMod>.Instance.GetWhatsNewMessages(new Version(1, 0));
            var messageBox = MessageBox.Show<WhatsNewMessageBox>();
            messageBox.CaptionText = CommonLocalize.Settings_ChangeLog;
            messageBox.OkText = CommonLocalize.MessageBox_OK;
#if DEBUG
            messageBox.Init(messages, SingletonMod<TypeMod>.Instance.NameRaw, false, culture: SingletonMod<TypeMod>.Instance.Culture);
#else
            messageBox.Init(messages, expandFirst: false, culture: SingletonMod<TypeMod>.Instance.Culture);
#endif
        }

        #endregion

        #region DEBUG

        private void AddDebug(UIAdvancedHelper helper)
        {
            var group = helper.AddGroup("Base");

            AddStringField(group, "Whats new version", WhatsNewVersionValue);
            AddStringField(group, "Compatible check game version", CompatibleCheckGameVersionValue);
            AddStringField(group, "Compatible check mod version", CompatibleCheckModVersionValue);
            AddCheckBox(group, "Show Beta warning", BetaWarning);
            AddCheckBox(group, "Show Linux warning", LinuxWarning);
            AddCheckBox(group, "Any versions", AnyVersions);
        }

        #endregion
    }
    public class UIAdvancedHelper : UIHelper, IEnumerable<UIHelper>
    {
        private List<UIHelper> Groups { get; } = new List<UIHelper>();
        public UIAutoLayoutScrollablePanel Content => self as UIAutoLayoutScrollablePanel;
        public UIAdvancedHelper(UIAutoLayoutScrollablePanel panel) : base(panel) { }

        public new UIHelper AddGroup(string name = null) => AddGroup(out _, name);
        public UIHelper AddGroup(out UILabel label, string name = null)
        {
            var panel = Content.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsGroupTemplate")) as UIPanel;
            label = panel.Find<UILabel>("Label");

            if (!string.IsNullOrEmpty(name))
                label.text = name;
            else
                label.isVisible = false;

            var group = new UIHelper(panel.Find("Content"));
            Groups.Add(group);

            return group;
        }

        public IEnumerator<UIHelper> GetEnumerator() => Groups.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    public static class SettingsHelper
    {
        public static UIPanel AddFloatField(UIHelper group, string label, SavedFloat saved, float? defaultValue = null, float? min = null, float? max = null, Action onSubmit = null, int padding = 0)
        {
            UITextField field = null;
            field = group.AddTextfield(label, saved.ToString(), OnChanged, OnSubmitted) as UITextField;
            field.CustomSettingsStyle();
            field.width = 100f;
            var panel = field.parent as UIPanel;
            panel.padding.left = padding;
            return panel;

            static void OnChanged(string distance) { }
            void OnSubmitted(string text)
            {
                if (float.TryParse(text, out var value))
                {
                    if (min.HasValue && value < min.Value)
                        value = min.Value;
                    if (max.HasValue && value > max.Value)
                        value = max.Value;

                    saved.value = value;
                    field.text = value.ToString();
                }
                else
                    field.text = defaultValue.HasValue ? defaultValue.ToString() : saved.ToString();

                onSubmit?.Invoke();
            }
        }
        public static UIPanel AddIntField(UIHelper group, string label, SavedInt saved, int? defaultValue = null, int? min = null, int? max = null, Action onSubmit = null, int padding = 0)
        {
            UITextField field = null;
            field = group.AddTextfield(label, saved.ToString(), OnChanged, OnSubmitted) as UITextField;
            field.CustomSettingsStyle();
            field.width = 100f;
            var panel = field.parent as UIPanel;
            panel.padding.left = padding;
            return panel;

            static void OnChanged(string distance) { }
            void OnSubmitted(string text)
            {
                if (int.TryParse(text, out var value))
                {
                    if (min.HasValue && value < min.Value)
                        value = min.Value;
                    if (max.HasValue && value > max.Value)
                        value = max.Value;

                    saved.value = value;
                    field.text = value.ToString();
                }
                else
                    field.text = defaultValue.HasValue ? defaultValue.ToString() : saved.ToString();

                onSubmit?.Invoke();
            }
        }
        public static UIPanel AddStringField(UIHelper group, string label, SavedString saved, Action onSubmit = null, int padding = 0)
        {
            UITextField field = null;
            field = group.AddTextfield(label, saved.ToString(), OnChanged, OnSubmitted) as UITextField;
            field.CustomSettingsStyle();
            var panel = field.parent as UIPanel;
            panel.padding.left = padding;
            return panel;

            static void OnChanged(string distance) { }
            void OnSubmitted(string text)
            {
                saved.value = text;
                onSubmit?.Invoke();
            }
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
        public static KeymappingsPanel AddKeyMappingPanel(UIHelper helper) => (helper.self as UIPanel).gameObject.AddComponent<KeymappingsPanel>();

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
    }

    public class LanguageDropDown : UIDropDown<string>
    {
        public LanguageDropDown()
        {
            ComponentStyle.CustomSettingsStyle(this, new Vector2(250, 31));
        }
    }
}
