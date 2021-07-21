using ColossalFramework;
using ColossalFramework.Globalization;
using ICities;
using ModsCommon.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ModsCommon
{
    public abstract class BaseMod<TypeMod> : ICustomMod
        where TypeMod : BaseMod<TypeMod>
    {
        public static string DiscordURL { get; } = "https://discord.gg/NnwhuBKMqj";
        public static string BETA => "[BETA]";

        public bool IsEnable { get; private set; }
        protected virtual bool LoadError { get; set; }
        private bool ErrorShown { get; set; }

        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public string VersionString => !IsBeta ? Version.ToString() : $"{Version} {BETA}";

        public string Name => !IsBeta ? $"{NameRaw} {Version.GetString()}" : $"{NameRaw} {Version.GetString()} {BETA}";
        public abstract string NameRaw { get; }
        public abstract string Description { get; }

        public PluginSearcher ThisSearcher { get; }
        private DependenciesWatcher DependencyWatcher { get; set; }

        public ILogger Logger { get; private set; }
        protected abstract ulong StableWorkshopId { get; }
        protected abstract ulong BetaWorkshopId { get; }
        public ulong WorkshopId => !IsBeta ? StableWorkshopId : BetaWorkshopId;

        protected virtual string ModSupportUrl => string.Empty;
        public string SupportUrl => !string.IsNullOrEmpty(ModSupportUrl) ? ModSupportUrl : WorkshopId.GetWorkshopUrl();
        public abstract List<Version> Versions { get; }
        protected abstract string IdRaw { get; }
        public string Id => !IsBeta ? IdRaw : $"{IdRaw} BETA";
        public abstract bool IsBeta { get; }

        public bool NeedMonoDevelop => Application.platform == RuntimePlatform.LinuxPlayer && NeedMonoDevelopImpl;
        protected virtual bool NeedMonoDevelopImpl => false;
#if DEBUG
        public bool NeedMonoDevelopDebug => NeedMonoDevelopImpl;
#endif

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
                var info = new ConflictDependencyInfo(BaseSettings<TypeMod>.AnyVersions ? DependencyState.Disable : DependencyState.Unsubscribe, searcher);
#endif
                infos.Add(info);

                return infos;
            }
        }

        private CultureInfo _culture;
        public CultureInfo Culture
        {
            get => _culture;
            protected set
            {
                _culture = value;
                CommonLocalize.Culture = value;
                SetCulture(value);
            }
        }
        protected virtual System.Resources.ResourceManager LocalizeManager => null;

        public BaseMod()
        {
            Logger = new Logger(Id);
            SingletonMod<TypeMod>.Instance = (TypeMod)this;
            Logger.Debug($"Create mod instance Version {VersionString}");

            ThisSearcher = new UserModInstanceSearcher(this);
            DependencyWatcher = DependenciesWatcher.Create(this, DependencyInfos);

            ChangeLocale();
            LocaleManager.eventUIComponentLocaleChanged += ChangeLocale;
        }

        public void OnEnabled()
        {
            Logger.Debug($"Enabled");
            IsEnable = true;
            LoadError = false;
            ErrorShown = false;

            IntroUtility.OnLoaded(EnableImpl);
        }
        public void OnDisabled()
        {
            Logger.Debug($"Disabled");
            IsEnable = false;

            try
            {
                DependencyWatcher.SetState(false);
                Disable();
            }
            catch (Exception error)
            {
                Logger.Error("Disable failed", error);
            }
        }
        private void EnableImpl()
        {
            try
            {
                DependencyWatcher.SetState(true);
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
                CheckLoadError();
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

            if (string.IsNullOrEmpty(locale))
            {
                if (LocaleManager.exists)
                    locale = LocaleManager.instance.language;
                else
                    locale = new SavedString(Settings.localeID, Settings.gameSettingsFile, DefaultSettings.localeID).value;
            }

            if (locale == "zh")
                locale = "zh-cn";

            Culture = new CultureInfo(locale);
            Logger.Debug($"Current cultute - {Culture?.Name ?? "null"}");
        }

        protected void CheckLoadError()
        {
            if (!ErrorShown)
            {
                if (LoadError)
                {
                    OnLoadError(out var shown);
                    ErrorShown = shown;
                }
                else if (NeedMonoDevelop && BaseSettings<TypeMod>.LinuxWarning)
                {
                    ShowLinuxTip();
                    ErrorShown = true;
                }
            }
        }
        protected virtual void SetCulture(CultureInfo culture) { }
        public virtual string GetLocalizeString(string key, CultureInfo culture = null)
        {
            culture ??= Culture;
            return LocalizeManager?.GetString(key, culture) ?? CommonLocalize.ResourceManager.GetString(key, culture);
        }

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
                messageBox.Init(messages, GetVersionString, modName: NameRaw);
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
        public void ShowLinuxTip()
        {
            var message = MessageBox.Show<ThreeButtonMessageBox>();
            message.CaptionText = NameRaw;
            message.MessageText = CommonLocalize.Mod_LinuxWarning +
                "\n" +
                "\nUbuntu: sudo apt install mono-devel" +
                "\nArch Linux: sudo pacman -S mono" +
                "\nLinux Mint: apt install mono-devel" +
                "\nFedora: sudo dnf install mono-core mono-devel";
            message.Button1Text = CommonLocalize.MessageBox_OK;
            message.Button2Text = CommonLocalize.MessageBox_DontShowAgain;
            message.Button3Text = CommonLocalize.MessageBox_MoreInfo;
            message.OnButton2Click = OnDontShowMore;
            message.OnButton3Click = OnMoreInfo;

            message.SetAutoButtonRatio();

            static bool OnDontShowMore()
            {
                BaseSettings<TypeMod>.LinuxWarning.value = false;
                return true;
            }
            static bool OnMoreInfo()
            {
                Utility.OpenUrl("https://github.com/MacSergey/NodeMarkup/issues/96");
                return false;
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
        public bool OpenDiscord()
        {
            DiscordURL.OpenUrl();
            return true;
        }
    }
}
