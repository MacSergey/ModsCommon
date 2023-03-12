using ColossalFramework;
using ColossalFramework.Globalization;
using ICities;
using ModsCommon.UI;
using ModsCommon.Utilities;
using ModsCommon.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static ColossalFramework.Plugins.PluginManager;

namespace ModsCommon
{
    public abstract class BaseMod<TypeMod> : ICustomMod
        where TypeMod : BaseMod<TypeMod>
    {
        public event Action OnStatusChanged;

        public static string DiscordURL { get; } = "https://discord.gg/NnwhuBKMqj";
        public static string BETA => "[BETA]";

        public bool IsEnable { get; private set; }

        private ModStatus _status;
        public virtual ModStatus Status
        {
            get => _status;
            protected set
            {
                if (value != _status)
                {
                    _status = value;
                    StatusChanged();
                }
            }
        }

        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public string VersionString => !IsBeta ? Version.GetString() : $"{Version.GetString()} {BETA}";
        protected abstract Version RequiredGameVersion { get; }
        protected Version CurrentGameVersion => new Version(
            (int)(uint)typeof(BuildConfig).GetField(nameof(BuildConfig.APPLICATION_VERSION_A)).GetValue(null),
            (int)(uint)typeof(BuildConfig).GetField(nameof(BuildConfig.APPLICATION_VERSION_B)).GetValue(null),
            (int)(uint)typeof(BuildConfig).GetField(nameof(BuildConfig.APPLICATION_VERSION_C)).GetValue(null),
            (int)(uint)typeof(BuildConfig).GetField(nameof(BuildConfig.APPLICATION_BUILD_NUMBER)).GetValue(null));

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
        public virtual string CrowdinUrl => string.Empty;

        public abstract List<ModVersion> Versions { get; }
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
        protected virtual LocalizeManager LocalizeManager => null;

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
            Status = ModStatus.Unknown;

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
                var gameVersion = CurrentGameVersion;
                var requiredVersion = RequiredGameVersion;

                if (gameVersion != requiredVersion)
                {
                    if (gameVersion < requiredVersion)
                    {
                        Status |= ModStatus.GameOutOfDate;
                        Logger.Debug($"Mod is out of date. Required game version: {RequiredGameVersion.GetStringGameFormat()}\tCurrent game version: {CurrentGameVersion.GetStringGameFormat()}");
                    }
                    else
                    {
                        Status |= ModStatus.ModOutOfDate;
                        Logger.Debug($"Game is out of date. Required game version: {RequiredGameVersion.GetStringGameFormat()}\tCurrent game version: {CurrentGameVersion.GetStringGameFormat()}");
                    }
                }


                DependencyWatcher.SetState(true);
                if (DependencyWatcher.IsValid)
                    Enable();

                if (Status == ModStatus.Unknown)
                    Status = ModStatus.Normal;
            }
            catch (Exception error)
            {
                Status |= ModStatus.LoadingError;
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
        protected void StatusChanged() => OnStatusChanged?.Invoke();

        public IEnumerable<string> GetSupportLocales()
        {
            if (LocalizeManager is LocalizeManager manager)
                return manager.GetSupportLocales();
            else
                return new string[0];
        }
        public void ChangeLocale()
        {
            var locale = BaseSettings<TypeMod>.Locale.value;

            if (string.IsNullOrEmpty(locale))
            {
                if (LocaleManager.exists)
                    locale = LocaleManager.instance.language;
                else
                    locale = new SavedString(global::Settings.localeID, global::Settings.gameSettingsFile, DefaultSettings.localeID).value;
            }

            if (!LocalizeExtension.TryGetCulture(locale, out var culture))
            {
                Logger.Debug($"locale {locale} is not supported");
                culture = new CultureInfo(LocalizeExtension.GetRegionLocale(DefaultSettings.localeID));
            }

            Culture = culture;
            Logger.Debug($"Current cultute - {Culture?.Name ?? "null"}");
        }

        protected void CheckLoadError()
        {
            if ((Status & ModStatus.ErrorShown) == 0)
            {
                if ((Status & ModStatus.WithErrors) != 0)
                {
                    OnLoadError(out var shown);
                    Status |= shown ? ModStatus.ErrorShown : ModStatus.Unknown;
                }
                else if (NeedMonoDevelop && BaseSettings<TypeMod>.LinuxWarning)
                {
                    ShowLinuxTip();
                    Status |= ModStatus.ErrorShown;
                }

                if ((Status & ModStatus.GameOutOfDate) != 0)
                    ShowGameOutOfDate();
                else if ((Status & ModStatus.ModOutOfDate) != 0)
                    ShowModOutOfDate();
            }
        }
        protected virtual void SetCulture(CultureInfo culture) { }
        public virtual string GetLocalizedString(string key, CultureInfo culture = null)
        {
            culture ??= Culture;

            if (LocalizeManager != null && LocalizeManager.TryGetString(key, culture, out var str))
                return str;
            else if (CommonLocalize.LocaleManager.TryGetString(key, culture, out str))
                return str;
            else
                return key;
        }
        public bool TryGetLocalizedString(string key, out string str, CultureInfo culture = null)
        {
            culture ??= Culture;

            if (LocalizeManager != null && LocalizeManager.TryGetString(key, culture, out str))
                return true;
            else if (CommonLocalize.LocaleManager.TryGetString(key, culture, out str))
                return true;
            else
                return false;
        }

        protected virtual void OnLoadError(out bool shown) => shown = (Status & ModStatus.ErrorShown) != 0;

        public void ShowWhatsNew()
        {
            var fromVersion = BaseSettings<TypeMod>.WhatsNewVersion;

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
                messageBox.Init(messages, NameRaw, culture: _culture);
            }
            else
            {
                var messageBox = MessageBox.Show<BetaWhatsNewMessageBox>();
                messageBox.CaptionText = string.Format(CommonLocalize.Mod_WhatsNewCaption, NameRaw);
                messageBox.OnButtonClick = Confirm;
                messageBox.OnGetStableClick = GetStable;
                messageBox.OkText = CommonLocalize.MessageBox_OK;
                messageBox.GetStableText = CommonLocalize.Mod_BetaWarningGetStable;
                messageBox.Init(messages, string.Format(CommonLocalize.Mod_BetaWarningMessage, NameRaw), NameRaw, culture: _culture);
            }

            static bool Confirm()
            {
                BaseSettings<TypeMod>.WhatsNewVersion = SingletonMod<TypeMod>.Version;
                return true;
            }
            static bool GetStable()
            {
                SingletonMod<TypeMod>.Instance.GetStable();
                return Confirm();
            }
        }

        public Dictionary<ModVersion, string> GetWhatsNewMessages(Version whatNewVersion)
        {
            var messages = new Dictionary<ModVersion, string>(Versions.Count);
#if DEBUG
            messages[new ModVersion(new Version(1, 2, 3, 4), new DateTime(1994, 12, 27))] =
                "[NEW] New\n" +
                "[FIXED] Fixed\n" +
                "[UPDATED] Updated\n" +
                "[REMOVED] Removed\n" +
                "[REVERTED] Reverted\n" +
                "[TRANSLATION] Translation\n" +
                "[WARNING] Warning\n" +
                "Without tag";
#endif
#if BETA
            messages[new ModVersion(Version, isBeta: true)] = CommonLocalize.Mod_WhatsNewMessageBeta;
#endif
            foreach (var version in Versions)
            {
#if !BETA
                if (Version < version.Number)
                    continue;
#endif

                if (version.Number <= whatNewVersion)
                    break;

                if (BaseSettings<TypeMod>.ShowOnlyMajor && !version.Number.IsMinor())
                    continue;

                var key = $"Mod_WhatsNewMessage{version.Number.ToString().Replace('.', '_')}";
                if (TryGetLocalizedString(key, out var message) && !string.IsNullOrEmpty(message))
                    messages[version] = message;
            }

            return messages;
        }

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

        public void ShowGameOutOfDate()
        {
            if (BaseSettings<TypeMod>.CompatibleCheckGameVersion != CurrentGameVersion || BaseSettings<TypeMod>.CompatibleCheckModVersion != Version)
            {
                var message = MessageBox.Show<ThreeButtonMessageBox>();
                message.CaptionText = NameRaw;
                message.MessageText = string.Format(CommonLocalize.Mod_VersionWarning_GameOutOfDate, RequiredGameVersion.GetStringGameFormat(), CurrentGameVersion.GetStringGameFormat());
                message.Button1Text = CommonLocalize.MessageBox_OK;
                message.Button2Text = CommonLocalize.Mod_VersionWarning_DontShow;
                message.Button3Text = CommonLocalize.Dependency_Disable;
                message.OnButton2Click = OnDontShowAgain;
                message.OnButton3Click = OnDisable;

                message.SetAutoButtonRatio();
            }
        }
        public void ShowModOutOfDate()
        {
            if (BaseSettings<TypeMod>.CompatibleCheckGameVersion != CurrentGameVersion || BaseSettings<TypeMod>.CompatibleCheckModVersion != Version)
            {
                var message = MessageBox.Show<ThreeButtonMessageBox>();
                message.CaptionText = NameRaw;
                var requiredString = BuildConfig.VersionToString(BuildConfig.MakeVersionNumber((uint)RequiredGameVersion.Major, (uint)RequiredGameVersion.Minor, (uint)RequiredGameVersion.Build, BuildConfig.ReleaseType.Final, (uint)RequiredGameVersion.Revision, BuildConfig.BuildType.Unknown), false);
                message.MessageText = string.Format(CommonLocalize.Mod_VersionWarning_ModOutOfDate, requiredString, BuildConfig.applicationVersion);
                message.Button1Text = CommonLocalize.MessageBox_OK;
                message.Button2Text = CommonLocalize.Dependency_Disable;
                message.Button3Text = CommonLocalize.Mod_VersionWarning_DontShow;
                message.OnButton2Click = OnDisable;
                message.OnButton3Click = OnDontShowAgain;

                message.SetAutoButtonRatio();
            }
        }
        private bool OnDisable()
        {
            if (new UserModInstanceSearcher(this).GetPlugin() is PluginInfo plugin)
                plugin.SetState(false);

            return true;
        }
        private bool OnDontShowAgain()
        {
            BaseSettings<TypeMod>.CompatibleCheckGameVersion = CurrentGameVersion;
            BaseSettings<TypeMod>.CompatibleCheckModVersion = Version;

            return true;
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
        public bool OpenTranslationProject()
        {
            CrowdinUrl.OpenUrl();
            return true;
        }
    }
    public struct ModVersion
    {
        public Version Number { get; set; }
        public DateTime Date { get; set; }
        public bool IsBeta { get; set; }

        public ModVersion(Version number, DateTime date = default, bool isBeta = false)
        {
            Number = number;
            Date = date;
            IsBeta = isBeta;
        }

        public override int GetHashCode() => Number.GetHashCode();

        public override string ToString() => $"{Number} {Date}";
    }

    [Flags]
    public enum ModStatus
    {
        [Description(nameof(CommonLocalize.Mod_Status_Unknown))]
        Unknown = 0,

        [Description(nameof(CommonLocalize.Mod_Status_OperateNormally))]
        Normal = 1,

        [Description(nameof(CommonLocalize.Mod_Status_LoadingError))]
        LoadingError = 2,
        [Description(nameof(CommonLocalize.Mod_Status_ModOutOfDate))]
        ModOutOfDate = 4,
        [Description(nameof(CommonLocalize.Mod_Status_GameOutOfDate))]
        GameOutOfDate = 8,

        [NotVisible]
        ErrorShown = 16,

        [NotVisible]
        WithErrors = LoadingError | ModOutOfDate | GameOutOfDate,
    }
}
