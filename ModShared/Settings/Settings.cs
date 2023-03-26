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
            image.texture = GetType().Assembly.LoadTextureFromAssembly("PreviewImage");
            image.color = Color.white;
            image.size = new Vector2(200f, 200f);
            image.relativePosition = new Vector3((imagePanel.width - image.width) * 0.5f, (imagePanel.height - image.height) * 0.5f);

            section.eventSizeChanged += (_, size) => SetSize();
            SetSize();
            void SetSize() => optionsContent.width = section.width - imagePanel.width - section.AutoLayoutSpace;

            var optionsSection = optionsContent.FillOptionsSection(out var title, SingletonMod<TypeMod>.Instance.NameRaw);
            optionsSection.CustomSection = true;
            title.textScale = 2f;

            var infoPanel = optionsSection.AddUIComponent<CustomUIPanel>();
            infoPanel.name = "Info";
            infoPanel.PauseLayout(() =>
            {
                infoPanel.AutoLayout = AutoLayout.Vertical;
                infoPanel.AutoLayoutSpace = 5;
                infoPanel.AutoChildrenHorizontally = AutoLayoutChildren.Fit;
                infoPanel.AutoChildrenVertically = AutoLayoutChildren.Fit;

                var versionData = infoPanel.AddUIComponent<CustomUIPanel>();
                versionData.PauseLayout(() =>
                {
                    versionData.AutoLayout = AutoLayout.Horizontal;
                    versionData.AutoChildrenHorizontally = AutoLayoutChildren.Fit;
                    versionData.AutoChildrenVertically = AutoLayoutChildren.Fit;

                    var version = versionData.AddUIComponent<CustomUILabel>();
                    version.Bold = true;
                    version.text = SingletonMod<TypeMod>.Instance.Version.ToString();
                    version.Atlas = CommonTextures.Atlas;
                    version.BackgroundSprite = CommonTextures.FieldLeft;
                    version.color = ComponentStyle.SettingsColor25;
                    version.Padding = new RectOffset(5, 5, 3, 0);

                    var beta = versionData.AddUIComponent<CustomUILabel>();
                    beta.Bold = true;
                    beta.Atlas = CommonTextures.Atlas;
                    beta.BackgroundSprite = CommonTextures.FieldRight;
                    beta.Padding = new RectOffset(5, 5, 3, 0);
                    if (!SingletonMod<TypeMod>.Instance.IsBeta)
                    {
                        beta.text = "STABLE";
                        beta.color = ComponentStyle.NormalGreen;
                    }
                    else
                    {
                        beta.text = "BETA";
                        beta.color = ComponentStyle.WarningColor;
                    }
                });

                var statusData = infoPanel.AddUIComponent<CustomUILabel>();
                statusData.Bold = true;
                statusData.text = GetStatusText();
                statusData.processMarkup = true;
                statusData.Atlas = CommonTextures.Atlas;
                statusData.BackgroundSprite = CommonTextures.FieldSingle;
                statusData.color = ComponentStyle.SettingsColor25;
                statusData.Padding = new RectOffset(5, 5, 5, 3);

                if (InfoCallback != null)
                {
                    SingletonMod<TypeMod>.Instance.OnStatusChanged -= InfoCallback;
                    InfoCallback = null;
                }
                InfoCallback = () =>
                {
                    try
                    {
                        statusData.text = GetStatusText();
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
            });

            optionsSection.AddSpace(20f);
            var changeLogPanel = optionsSection.AddButtonPanel(new RectOffset(0, 0, 5, 5), 0);
            changeLogPanel.Content.AutoLayoutStart = UI.LayoutStart.TopLeft;
            changeLogPanel.CanHover = false;
            var changeLog = changeLogPanel.AddButton(CommonLocalize.Settings_ChangeLog, ShowChangeLog, 250f, 1f);
            changeLog.BgColors = new ColorSet(ComponentStyle.SettingsColor25, ComponentStyle.SettingsColor20, ComponentStyle.SettingsColor20, ComponentStyle.SettingsColor25, ComponentStyle.SettingsColor15);
            changeLog.Bold = true;
            changeLog.height = 50f;
        }

        private string GetStatusText()
        {
            var statusText = string.Empty;
            var status = SingletonMod<TypeMod>.Instance.Status;
            if (status == ModStatus.Unknown)
                statusText = ModStatus.Unknown.Description<ModStatus, TypeMod>().AddColor(ComponentStyle.WarningColor);
            else if (status == ModStatus.Normal)
                statusText = ModStatus.Normal.Description<ModStatus, TypeMod>().AddColor(ComponentStyle.HoveredGreen);
            else
            {
                status &= ModStatus.WithErrors;
                var errors = status.GetEnumValues().Select(s => s.Description<ModStatus, TypeMod>()).ToArray();
                statusText = string.Join(" | ", errors).AddColor(ComponentStyle.ErrorFocusedColor);
            }

            return statusText;
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
            buttonPanel.Content.AutoLayoutStart = UI.LayoutStart.TopLeft;
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

            showToggle.Control.OnValueChanged += OnChange;
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
            buttonPanel.AddButton("Game out of data", SingletonMod<TypeMod>.Instance.ShowGameOutOfDate, 200);
            buttonPanel.AddButton("Mod out of date", SingletonMod<TypeMod>.Instance.ShowModOutOfDate, 200);

            static void OpenDependency()
            {
                var message = SingletonMod<TypeMod>.Instance.DependencyWatcher.AddRequest(false);
                message.State = DependencyMessageState.Required;
                message.Text = "Harmony";
                message.RequiredText = "Get";

                message = SingletonMod<TypeMod>.Instance.DependencyWatcher.AddRequest(false);
                message.State = DependencyMessageState.InProgress;
                message.Text = "Harmony";
                message.GetProgress = () => Mathf.Clamp01(DateTime.Now.Millisecond * 0.00125f);

                message = SingletonMod<TypeMod>.Instance.DependencyWatcher.AddRequest(false);
                message.State = DependencyMessageState.Resolved;
                message.Text = "Harmony";
                message.ResolvedText = "Installed";
            }
        }
#endif
        #endregion
    }
}
