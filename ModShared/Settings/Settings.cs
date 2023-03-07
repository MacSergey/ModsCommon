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
using UnifiedUI.Helpers;
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
            TabStrip.CustomSettingsStyle();
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
            TabStrip.StopLayout();
            {
                CreateTab(nameof(GeneralTab), CommonLocalize.Settings_GeneralTab);

                foreach (var tab in AdditionalTabs)
                    CreateTab(tab.Key, tab.Value);

                CreateTab(nameof(SupportTab), CommonLocalize.Settings_SupportTab);
#if DEBUG
                CreateTab(nameof(DebugTab), "Debug");
#endif
            }
            TabStrip.StartLayout();
        }
        private UIAdvancedHelper CreateTab(string name, string label)
        {
            TabStrip.AddTab(label, 1.25f);

            var tabPanel = MainPanel.AddUIComponent<AdvancedScrollablePanel>();
            tabPanel.Content.autoLayoutPadding = new RectOffset(8, 8, 5, 5);
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
            dropDown.AddItem(GetLocaleItem(string.Empty));

            foreach (var locale in GetSupportLanguages())
                dropDown.AddItem(GetLocaleItem(locale));

            dropDown.SelectedObject = Locale.value;
            dropDown.OnValueChanged += LanguageChanged;

            static void LanguageChanged(LanguageDropDown.Language language)
            {
                Locale.value = language.locale;
                LocaleManager.ForceReload();
            }
        }
        private string[] GetSupportLanguages()
        {
            var languages = new HashSet<string> { "en-US" };
            languages.AddRange(SingletonMod<TypeMod>.Instance.GetSupportLocales());

            return languages.OrderBy(l => l).ToArray();
        }
        private LanguageDropDown.Language GetLocaleItem(string locale)
        {
            if (string.IsNullOrEmpty(locale))
            {
                var label = CommonLocalize.LocaleManager.GetString("Mod_LocaleGame", CommonLocalize.Culture);
                return new LanguageDropDown.Language(locale, label, LocalizeExtension.GetRegionLocale(LocaleManager.instance.language));
            }
            else
            {
                var key = $"Mod_Locale_{locale}";
                var label = LocalizeExtension.TryGetCulture(locale, out var culture) ? CommonLocalize.LocaleManager.GetString(key, culture) : key;
                if (SingletonMod<TypeMod>.Instance.Culture.Name.ToLower() != locale.ToLower())
                    label += $" ({CommonLocalize.LocaleManager.GetString(key, CommonLocalize.Culture)})";

                return new LanguageDropDown.Language(locale, label, locale);
            }
        }

        #endregion

        #region NOTIFICATIONS

        protected void AddNotifications(UIAdvancedHelper helper)
        {
            var group = helper.AddOptionsGroup(CommonLocalize.Settings_Notifications);
 
            var showToggle = AddToggle(group, CommonLocalize.Settings_ShowWhatsNew, ShowWhatsNew);
            var onlyMajorToggle = AddToggle(group, CommonLocalize.Settings_ShowOnlyMajor, ShowOnlyMajor);

            showToggle.Toggle.OnStateChanged += OnChange;
            OnChange(ShowWhatsNew);

            void OnChange(bool show) => onlyMajorToggle.isVisible = show;
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
            var group = helper.AddOptionsGroup("Base");

            AddStringField(group, "Whats new version", WhatsNewVersionValue);
            AddStringField(group, "Compatible check game version", CompatibleCheckGameVersionValue);
            AddStringField(group, "Compatible check mod version", CompatibleCheckModVersionValue);
            AddToggle(group, "Show Beta warning", BetaWarning);
            AddToggle(group, "Show Linux warning", LinuxWarning);
            AddToggle(group, "Any versions", AnyVersions);
        }

        #endregion
    }
}
