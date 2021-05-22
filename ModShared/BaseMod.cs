using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using ModsCommon.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static ColossalFramework.Plugins.PluginManager;

namespace ModsCommon
{
    public abstract class BaseMod<TypeMod> : IUserMod
        where TypeMod : BaseMod<TypeMod>
    {
        public static string BETA => "[BETA]";

        protected virtual bool LoadError { get; set; }
        private bool ErrorShown { get; set; }

        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public string VersionString => !IsBeta ? Version.ToString() : $"{Version} {BETA}";

        public string Name => !IsBeta ? $"{NameRaw} {Version.GetString()}" : $"{NameRaw} {Version.GetString()} {BETA}";
        public abstract string NameRaw { get; }
        public abstract string Description { get; }

        public PluginSearcher ThisSearcher { get; }
        public PluginInfo Plugin => PluginUtilities.GetPlugin(ThisSearcher);
        private DependenciesWatcher DependencyWatcher { get; set; }
        public Logger Logger { get; private set; }
        protected abstract ulong StableWorkshopId { get; }
        protected abstract ulong BetaWorkshopId { get; }
        public ulong WorkshopId => !IsBeta ? StableWorkshopId : BetaWorkshopId;

        protected virtual string ModSupportUrl => string.Empty;
        public string SupportUrl => !string.IsNullOrEmpty(ModSupportUrl) ? ModSupportUrl : WorkshopId.GetWorkshopUrl();
        public abstract List<Version> Versions { get; }
        protected abstract string IdRaw { get; }
        public string Id => !IsBeta ? IdRaw : $"{IdRaw} BETA";
        public abstract bool IsBeta { get; }

        protected virtual List<BaseDependencyInfo> DependencyInfos
        {
            get
            {
                var infos = new List<BaseDependencyInfo>();

                var otherSearcher = PluginUtilities.GetSearcher(NameRaw, !IsBeta ? BetaWorkshopId : StableWorkshopId);
                var searcher = otherSearcher & !ThisSearcher;
#if DEBUG
                var info = new ConflictDependencyInfo(DependencyState.Disable, searcher);
#else
                var info = new ConflictDependencyInfo(DependencyState.Unsubscribe, searcher);
#endif

                infos.Add(info);

                return infos;
            }
        }

        public virtual CultureInfo Culture
        {
            get => null;
            protected set { }
        }

        public BaseMod()
        {
            Logger = new Logger(Id);
            SingletonMod<TypeMod>.Instance = (TypeMod)this;
            Logger.Debug($"Create mod instance Version {VersionString}");

            ThisSearcher = new UserModInstanceSearcher(this);
            DependencyWatcher = DependenciesWatcher.Create(ThisSearcher, DependencyInfos, NameRaw, Name);

            ChangeLocale();
            LocaleManager.eventUIComponentLocaleChanged += ChangeLocale;
        }
        public void OnEnabled()
        {
            Logger.Debug($"Enabled");
            LoadError = false;
            ErrorShown = false;

            if (UIView.GetAView() != null)
                EnableImpl();
            else
                LoadingManager.instance.m_introLoaded += EnableImpl;
        }
        public void OnDisabled()
        {
            Logger.Debug($"Disabled");
            LoadingManager.instance.m_introLoaded -= EnableImpl;
            DependencyWatcher.SetState(false);

            try
            {
                Disable();
            }
            catch (Exception error)
            {
                Logger.Error("Disable failed", error);
            }
        }
        private void EnableImpl()
        {
            LoadingManager.instance.m_introLoaded -= EnableImpl;
            DependencyWatcher.SetState(true);

            try
            {
                if (DependencyWatcher.IsValid)
                    Enable();
            }
            catch (Exception error)
            {
                LoadError = true;
                Logger.Error("Enable failed", error);
            }
            finally
            {
                CheckLoadError(LoadError && !ErrorShown);
            }
        }
        protected abstract void Enable();
        protected abstract void Disable();

        public void OnSettingsUI(UIHelperBase helper)
        {
            Logger.Debug($"Load SettingsUI");
            GetSettings(helper);
        }
        protected virtual void GetSettings(UIHelperBase helper) { }

        public void ChangeLocale()
        {
            var locale = BaseSettings<TypeMod>.Locale.value;
            locale = string.IsNullOrEmpty(locale) ? SingletonLite<LocaleManager>.instance.language : locale;
            if (locale == "zh")
                locale = "zh-cn";

            var culture = new CultureInfo(locale);
            CommonLocalize.Culture = culture;
            Culture = culture;
            Logger.Debug($"Current cultute - {culture?.Name ?? "null"}");
        }

        protected void CheckLoadError(bool condition)
        {
            if (condition)
            {
                OnLoadError(out var shown);
                ErrorShown = shown;
            }
        }
        public virtual string GetLocalizeString(string str, CultureInfo culture = null) => str;
        protected virtual void OnLoadError(out bool shown) => shown = ErrorShown;

        public void ShowWhatsNew()
        {
            var fromVersion = new Version(BaseSettings<TypeMod>.WhatsNewVersion);

            if (!BaseSettings<TypeMod>.ShowWhatsNew || Version <= fromVersion)
                return;

            var messages = GetWhatsNewMessages(fromVersion);
            if (!messages.Any())
                return;

            if (!IsBeta)
            {
                var messageBox = MessageBox.Show<WhatsNewMessageBox>();
                messageBox.CaptionText = string.Format(CommonLocalize.Mod_WhatsNewCaption, NameRaw);
                messageBox.OnButtonClick = Confirm;
                messageBox.OkText = CommonLocalize.MessageBox_OK;
                messageBox.Init(messages, GetVersionString);
            }
            else
            {
                var messageBox = MessageBox.Show<BetaWhatsNewMessageBox>();
                messageBox.CaptionText = string.Format(CommonLocalize.Mod_WhatsNewCaption, NameRaw);
                messageBox.OnButtonClick = Confirm;
                messageBox.OnGetStableClick = GetStable;
                messageBox.OkText = CommonLocalize.MessageBox_OK;
                messageBox.GetStableText = CommonLocalize.Mod_BetaWarningGetStable;
                messageBox.Init(messages, string.Format(CommonLocalize.Mod_BetaWarningMessage, NameRaw), GetVersionString);
            }

            static bool Confirm()
            {
                BaseSettings<TypeMod>.WhatsNewVersion.value = SingletonMod<TypeMod>.Version.ToString();
                return true;
            }
            static bool GetStable()
            {
                SingletonMod<TypeMod>.Instance.GetStable();
                return Confirm();
            }
        }

        public Dictionary<Version, string> GetWhatsNewMessages(Version whatNewVersion)
        {
            var messages = new Dictionary<Version, string>(Versions.Count);
#if BETA
            messages[Version] = CommonLocalize.Mod_WhatsNewMessageBeta;
#endif
            foreach (var version in Versions)
            {
                if (Version < version)
                    continue;

                if (version <= whatNewVersion)
                    break;

                if (BaseSettings<TypeMod>.ShowOnlyMajor && !version.IsMinor())
                    continue;

                if (GetLocalizeString($"Mod_WhatsNewMessage{version.ToString().Replace('.', '_')}") is string message && !string.IsNullOrEmpty(message))
                    messages[version] = message;
            }

            return messages;
        }
        public string GetVersionString(Version version) => string.Format(CommonLocalize.Mod_WhatsNewVersion, version == Version ? VersionString : version.ToString());

        public void ShowBetaWarning()
        {
            if (!IsBeta)
                BaseSettings<TypeMod>.BetaWarning.value = true;
            else if (BaseSettings<TypeMod>.BetaWarning.value)
            {
                var messageBox = MessageBox.Show<TwoButtonMessageBox>();
                messageBox.CaptionText = CommonLocalize.Mod_BetaWarningCaption;
                messageBox.MessageText = string.Format(CommonLocalize.Mod_BetaWarningMessage, NameRaw);
                messageBox.Button1Text = CommonLocalize.Mod_BetaWarningAgree;
                messageBox.Button2Text = CommonLocalize.Mod_BetaWarningGetStable;
                messageBox.OnButton1Click = AgreeClick;
                messageBox.OnButton2Click = GetStable;

                static bool AgreeClick()
                {
                    BaseSettings<TypeMod>.BetaWarning.value = false;
                    return true;
                }
                static bool GetStable()
                {
                    SingletonMod<TypeMod>.Instance.GetStable();
                    return true;
                }
            }
        }

        public bool GetStable()
        {
            StableWorkshopId.OpenWorkshop();
            return true;
        }
        public bool OpenWorkshop()
        {
            WorkshopId.OpenWorkshop();
            return true;
        }
        public bool OpenSupport()
        {
            SupportUrl.OpenUrl();
            return true;
        }
    }
}
