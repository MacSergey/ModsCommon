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
using static ModsCommon.Settings.Helper;

namespace ModsCommon.Settings
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
        private Dictionary<string, UIComponent> TabContent { get; } = new Dictionary<string, UIComponent>();
        protected UIComponent GeneralTab => TabContent[nameof(GeneralTab)];
        protected UIComponent SupportTab => TabContent[nameof(SupportTab)];
#if DEBUG
        protected UIComponent DebugTab => TabContent[nameof(DebugTab)];
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
            TabStrip.SettingsStyle();
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
        private UIComponent CreateTab(string name, string label)
        {
            TabStrip.AddTab(label, 1.25f);

            var tabPanel = MainPanel.AddUIComponent<AdvancedScrollablePanel>();
            tabPanel.Content.autoLayoutPadding = new RectOffset(8, 8, 5, 5);
            SetTabSize(tabPanel);
            tabPanel.isVisible = false;
            TabPanels.Add(tabPanel);

            TabContent[name] = tabPanel.Content;
            return tabPanel.Content;
        }
        protected UIComponent GetTabContent(string label) => TabContent[label];

        #region INFO

        private void AddInfo(UIComponent tabContent)
        {
            CustomUIPanel optionsGroup;
            CustomUILabel title;
            if (GetType().Assembly.LoadTextureFromAssembly("PreviewImage") is Texture2D preview)
            {
                var group = tabContent.AddGroup();
                group.autoLayoutDirection = LayoutDirection.Horizontal;

                var optionsContent = group.AddUIComponent<CustomUIPanel>();
                optionsContent.autoLayout = true;
                optionsContent.autoFitChildrenVertically = true;
                optionsContent.autoLayoutDirection = LayoutDirection.Vertical;
                optionsContent.autoLayoutPadding = new RectOffset(0, 0, 0, 15);

                var imagePanel = group.AddUIComponent<CustomUIPanel>();
                imagePanel.size = new Vector2(200f, 200f);

                var image = imagePanel.AddUIComponent<CustomUITextureSprite>();
                image.texture = preview;
                image.color = Color.white;
                image.size = new Vector2(170f, 170f);
                image.relativePosition = new Vector3((imagePanel.width - image.width) * 0.5f, (imagePanel.height - image.height) * 0.5f);

                group.eventSizeChanged += (_, size) => SetSize();
                SetSize();
                void SetSize() => optionsContent.width = group.width - imagePanel.width;

                optionsGroup = optionsContent.FillOptionsGroup(out title, SingletonMod<TypeMod>.Instance.NameRaw);
            }
            else
                optionsGroup = tabContent.AddOptionsGroup(out title, SingletonMod<TypeMod>.Instance.NameRaw);

            title.textScale = 2f;

            var versionItem = optionsGroup.AddLabel(string.Format(CommonLocalize.Mod_Version, SingletonMod<TypeMod>.Instance.VersionString));
            versionItem.padding = new RectOffset();
            versionItem.Borders = SettingsContentItem.Border.None;

            if (InfoCallback != null)
            {
                SingletonMod<TypeMod>.Instance.OnStatusChanged -= InfoCallback;
                InfoCallback = null;
            }

            var infoLabel = optionsGroup.AddLabel(GetStatusText());
            infoLabel.padding = new RectOffset();
            infoLabel.Borders = SettingsContentItem.Border.None;
            infoLabel.LabelItem.processMarkup = true;

            InfoCallback = () =>
            {
                try
                {
                    infoLabel.Label = GetStatusText();
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

            optionsGroup.AddSpace(35f);
            optionsGroup.AddButtonPanel(new RectOffset(0, 0, 5, 5), 0).AddButton(CommonLocalize.Settings_ChangeLog, ShowChangeLog, 250f, 1f);
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

        protected void AddLanguage(UIComponent tabContent)
        {
            var group = tabContent.AddOptionsGroup(CommonLocalize.Settings_Language);
            AddLanguageList(group);

            group.AddSpace(25f);

            var labelItem = group.AddLabel(CommonLocalize.Settings_TranslationDescription, 0.8f);
            labelItem.Borders = SettingsContentItem.Border.None;
            labelItem.padding = new RectOffset(0, 0, 5, 5);

            var buttonPanel = group.AddButtonPanel(new RectOffset(0, 0, 0, 10), 10);
            buttonPanel.AddButton(CommonLocalize.Settings_TranslationImprove, () => SingletonMod<TypeMod>.Instance.OpenTranslationProject(), 250f, 1f);
            buttonPanel.AddButton(CommonLocalize.Settings_TranslationNew, () => "https://crowdin.com/messages/create/14337258/".OpenUrl(), 250f, 1f);
        }

        private void AddLanguageList(UIComponent tabContent)
        {
            var item = tabContent.AddUIComponent<LanguageSettingsItem>();
            item.Borders = SettingsContentItem.Border.None;

            item.DropDown.AddItem(GetLocaleItem(string.Empty));

            foreach (var locale in GetSupportLanguages())
                item.DropDown.AddItem(GetLocaleItem(locale));

            item.DropDown.SelectedObject = Locale.value;
            item.DropDown.OnSelectObject += LanguageChanged;

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

        protected void AddNotifications(UIComponent tabContent)
        {
            var group = tabContent.AddOptionsGroup(CommonLocalize.Settings_Notifications);

            var showToggle = group.AddToggle(CommonLocalize.Settings_ShowWhatsNew, ShowWhatsNew);
            var onlyMajorToggle = group.AddToggle(CommonLocalize.Settings_ShowOnlyMajor, ShowOnlyMajor);

            showToggle.Control.OnStateChanged += OnChange;
            OnChange(ShowWhatsNew);

            void OnChange(bool show) => onlyMajorToggle.isVisible = show;
        }

        #endregion

        #region SUPPORT

        private void AddSupport(UIComponent tabContent)
        {
            var group = tabContent.AddOptionsGroup();

            group.AddButtonPanel(new RectOffset(0, 0, 5, 5), 0).AddButton(CommonLocalize.Settings_Troubleshooting, () => SingletonMod<TypeMod>.Instance.OpenSupport());
            group.AddButtonPanel(new RectOffset(0, 0, 5, 5), 0).AddButton("Discord", () => SingletonMod<TypeMod>.Instance.OpenDiscord());
#if DEBUG
            if (SingletonMod<TypeMod>.Instance.NeedMonoDevelopDebug)
#else
            if (SingletonMod<TypeMod>.Instance.NeedMonoDevelop)
#endif
            {
                var linuxGroup = tabContent.AddOptionsGroup(CommonLocalize.Settings_ForLinuxUsers);
                linuxGroup.AddButtonPanel(new RectOffset(0, 0, 5, 5), 0).AddButton(CommonLocalize.Settings_SolveCrashOnLinux, () => SingletonMod<TypeMod>.Instance.ShowLinuxTip());
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

        private void AddDebug(UIComponent tabContent)
        {
            var group = tabContent.AddOptionsGroup("Base");

            group.AddStringField("Whats new version", WhatsNewVersionValue);
            group.AddStringField("Compatible check game version", CompatibleCheckGameVersionValue);
            group.AddStringField("Compatible check mod version", CompatibleCheckModVersionValue);
            group.AddToggle("Show Beta warning", BetaWarning);
            group.AddToggle("Show Linux warning", LinuxWarning);
            group.AddToggle("Any versions", AnyVersions);
        }

        #endregion
    }
}
