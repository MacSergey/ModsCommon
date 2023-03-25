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
        protected List<CustomUIScrollablePanel> TabPanels { get; set; }
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
            MainPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 5);

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
            TabPanels = new List<CustomUIScrollablePanel>();

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
        private void SetTabSize(CustomUIScrollablePanel panel)
        {
            var size = new Vector2(MainPanel.width, MainPanel.height - (TabStrip.isVisible ? MainPanel.autoLayoutPadding.vertical + TabStrip.height : 0f));
            panel.minimumSize = size;
            panel.maximumSize = size;
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

            var tabPanel = MainPanel.AddUIComponent<CustomUIScrollablePanel>();
            tabPanel.ScrollOrientation = UIOrientation.Vertical;
            tabPanel.AutoLayout = AutoLayout.Vertical;
            tabPanel.AutoLayoutSpace = 25;
            tabPanel.AutoChildrenVertically = AutoLayoutChildren.Fit;
            tabPanel.AutoChildrenHorizontally = AutoLayoutChildren.Fill;
            tabPanel.Padding = new RectOffset(15, 15, 15, 15);
            SetTabSize(tabPanel);
            tabPanel.isVisible = false;
            TabPanels.Add(tabPanel);

            tabPanel.Scrollbar.SettingsStyle();
            tabPanel.ScrollbarSize = 12f;

            TabContent[name] = tabPanel;
            return tabPanel;
        }
        protected UIComponent GetTabContent(string label) => TabContent[label];

        #region INFO

        private void AddInfo(UIComponent tabContent)
        {
            SettingsItemSection optionsSection;
            CustomUILabel title;
            if (GetType().Assembly.LoadTextureFromAssembly("PreviewImage") is Texture2D preview)
            {
                var section = tabContent.AddSection();
                section.NormalBgColor = ComponentStyle.SettingsColor60;
                section.ForegroundSprite = string.Empty;
                section.AutoLayout = AutoLayout.Horizontal;
                section.maximumSize = new Vector2(0f, 200f);

                var optionsContent = section.AddUIComponent<CustomUIPanel>();
                optionsContent.PauseLayout(() =>
                {
                    optionsContent.AutoLayout = AutoLayout.Vertical;
                    optionsContent.AutoChildrenVertically = AutoLayoutChildren.Fit;
                    optionsContent.AutoChildrenHorizontally = AutoLayoutChildren.Fill;
                    optionsContent.AutoLayoutSpace = 15;
                });

                var imagePanel = section.AddUIComponent<CustomUIPanel>();
                imagePanel.size = new Vector2(200f, 200f);

                var image = imagePanel.AddUIComponent<CustomUITextureSprite>();
                image.texture = preview;
                image.color = Color.white;
                image.size = new Vector2(200f, 200f);
                image.relativePosition = new Vector3((imagePanel.width - image.width) * 0.5f, (imagePanel.height - image.height) * 0.5f);

                section.eventSizeChanged += (_, size) => SetSize();
                SetSize();
                void SetSize() => optionsContent.width = section.width - imagePanel.width - section.AutoLayoutSpace;

                optionsSection = optionsContent.FillOptionsSection(out title, SingletonMod<TypeMod>.Instance.NameRaw);
            }
            else
            {
                optionsSection = tabContent.AddOptionsSection(out title, SingletonMod<TypeMod>.Instance.NameRaw);

                optionsSection.NormalBgColor = ComponentStyle.SettingsColor60;
                optionsSection.ForegroundSprite = string.Empty;
            }
            optionsSection.CustomSection = true;
            title.textScale = 2f;

            var versionItem = optionsSection.AddLabel(string.Format(CommonLocalize.Mod_Version, SingletonMod<TypeMod>.Instance.VersionString));
            versionItem.Padding = new RectOffset();
            versionItem.Borders = SettingsItemBorder.None;
            versionItem.CanHover = false;

            if (InfoCallback != null)
            {
                SingletonMod<TypeMod>.Instance.OnStatusChanged -= InfoCallback;
                InfoCallback = null;
            }

            var infoLabel = optionsSection.AddLabel(GetStatusText());
            infoLabel.Padding = new RectOffset();
            infoLabel.Borders = SettingsItemBorder.None;
            infoLabel.CanHover = false;
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

            optionsSection.AddSpace(25f);
            var changeLogPanel = optionsSection.AddButtonPanel(new RectOffset(0, 0, 5, 5), 0);
            changeLogPanel.CanHover = false;
            var changeLog = changeLogPanel.AddButton(CommonLocalize.Settings_ChangeLog, ShowChangeLog, 250f, 1f);
            changeLog.SetBgColor(new ColorSet(ComponentStyle.SettingsColor25, ComponentStyle.SettingsColor20, ComponentStyle.SettingsColor20, ComponentStyle.SettingsColor25, ComponentStyle.SettingsColor15));
            changeLog.height = 50f;
        }

        private string GetStatusText()
        {
            var statusText = string.Empty;
            var status = SingletonMod<TypeMod>.Instance.Status;
            if (status == ModStatus.Unknown)
                statusText = ModStatus.Unknown.Description<ModStatus, TypeMod>().AddColor(new Color32(255, 215, 81, 255));
            else if (status == ModStatus.Normal)
                statusText = ModStatus.Normal.Description<ModStatus, TypeMod>().AddColor(new Color32(96, 209, 21, 255));
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
            var section = tabContent.AddOptionsSection(CommonLocalize.Settings_Language);
            section.CustomSection = true;
            AddLanguageList(section);

            section.AddSpace(25f);

            var labelItem = section.AddLabel(CommonLocalize.Settings_TranslationDescription, 0.8f);
            labelItem.CanHover = false;
            labelItem.Borders = SettingsItemBorder.None;
            labelItem.Padding = new RectOffset(0, 0, 5, 5);

            var buttonPanel = section.AddButtonPanel(itemSpacing: 10);
            buttonPanel.CanHover = false;
            buttonPanel.AddButton(CommonLocalize.Settings_TranslationImprove, () => SingletonMod<TypeMod>.Instance.OpenTranslationProject(), 250f, 1f);
            buttonPanel.AddButton(CommonLocalize.Settings_TranslationNew, () => "https://crowdin.com/messages/create/14337258/".OpenUrl(), 250f, 1f);
        }

        private void AddLanguageList(UIComponent tabContent)
        {
            var item = tabContent.AddUIComponent<LanguageSettingsItem>();
            item.CanHover = false;
            item.Borders = SettingsItemBorder.None;

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
            var section = tabContent.AddOptionsSection(CommonLocalize.Settings_Notifications);

            var showToggle = section.AddToggle(CommonLocalize.Settings_ShowWhatsNew, ShowWhatsNew);
            var onlyMajorToggle = section.AddToggle(CommonLocalize.Settings_ShowOnlyMajor, ShowOnlyMajor);

            showToggle.Control.OnStateChanged += OnChange;
            OnChange(ShowWhatsNew);

            void OnChange(bool show) => onlyMajorToggle.isVisible = show;
        }

        #endregion

        #region SUPPORT

        private void AddSupport(UIComponent tabContent)
        {
            var section = tabContent.AddOptionsSection();

            section.AddButtonPanel(new RectOffset(0, 0, 5, 5), 0).AddButton(CommonLocalize.Settings_Troubleshooting, () => SingletonMod<TypeMod>.Instance.OpenSupport());
            section.AddButtonPanel(new RectOffset(0, 0, 5, 5), 0).AddButton("Discord", () => SingletonMod<TypeMod>.Instance.OpenDiscord());
#if DEBUG
            if (SingletonMod<TypeMod>.Instance.NeedMonoDevelopDebug)
#else
            if (SingletonMod<TypeMod>.Instance.NeedMonoDevelop)
#endif
            {
                var linuxSection = tabContent.AddOptionsSection(CommonLocalize.Settings_ForLinuxUsers);
                linuxSection.AddButtonPanel(new RectOffset(0, 0, 5, 5), 0).AddButton(CommonLocalize.Settings_SolveCrashOnLinux, () => SingletonMod<TypeMod>.Instance.ShowLinuxTip());
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

#if DEBUG
        private void AddDebug(UIComponent tabContent)
        {
            var section = tabContent.AddOptionsSection("Base");

            section.AddStringField("Whats new version", WhatsNewVersionValue);
            section.AddStringField("Compatible check game version", CompatibleCheckGameVersionValue);
            section.AddStringField("Compatible check mod version", CompatibleCheckModVersionValue);
            section.AddToggle("Show Beta warning", BetaWarning);
            section.AddToggle("Show Linux warning", LinuxWarning);
            section.AddToggle("Any versions", AnyVersions);

            var buttonPanel = section.AddButtonPanel();
            buttonPanel.AddButton("Dependency message", OpenDependency, 200);

            static void OpenDependency()
            {
                var message = SingletonMod<TypeMod>.Instance.DependencyWatcher.AddMessage();
                message.Text = "Harmony";
                message.RequiredText = "Get";

                message = SingletonMod<TypeMod>.Instance.DependencyWatcher.AddMessage();
                message.Text = "Harmony";
                message.RequiredText = "Get";
            }
        }
#endif
        #endregion
    }
}
