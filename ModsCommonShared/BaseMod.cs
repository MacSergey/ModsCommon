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

        public Logger Logger { get; private set; }
        public abstract string WorkshopUrl { get; }
        public abstract string BetaWorkshopUrl { get; }
        public abstract List<Version> Versions { get; }
        protected abstract string IdRaw { get; }
        public string Id => !IsBeta ? IdRaw : $"{IdRaw} BETA";
        public abstract bool IsBeta { get; }

        public virtual CultureInfo Culture
        {
            get => null;
            protected set { }
        }

        public BaseMod()
        {
            Logger = new Logger(Id);
            SingletonMod<TypeMod>.Instance = (TypeMod)this;
        }
        public void OnEnabled()
        {
            Logger.Debug($"Version {VersionString}");
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
            LocaleManager.eventLocaleChanged -= LocaleChanged;

            Disable();
        }
        private void EnableImpl()
        {
            LoadingManager.instance.m_introLoaded -= EnableImpl;
            Enable();
            CheckLoadedError();
        }
        protected abstract void Enable();
        protected abstract void Disable();

        public void OnSettingsUI(UIHelperBase helper)
        {
            LocaleManager.eventLocaleChanged -= LocaleChanged;
            LocaleChanged();
            LocaleManager.eventLocaleChanged += LocaleChanged;

            Logger.Debug($"Load SettingsUI");
            GetSettings(helper);
        }
        protected virtual void GetSettings(UIHelperBase helper) { }

        public void LocaleChanged()
        {
            var locale = BaseSettings<TypeMod>.Locale.value;
            locale = string.IsNullOrEmpty(locale) ? SingletonLite<LocaleManager>.instance.language : locale;
            if (locale == "zh")
                locale = "zh-cn";

            Culture = new CultureInfo(locale);
            Logger.Debug($"Current cultute - {Culture?.Name ?? "null"}");
        }

        protected void CheckLoadedError()
        {
            if (LoadError && !ErrorShown)
            {
                ErrorShown = true;
                OnLoadedError();
            }
        }
        public virtual string GetLocalizeString(string str, CultureInfo culture = null) => str;
        public virtual void OnLoadedError() { }

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
                var messageBox = MessageBoxBase.ShowModal<WhatsNewMessageBox>();
                messageBox.CaptionText = string.Format(ModLocalize<TypeMod>.WhatsNewCaption, NameRaw);
                messageBox.OnButtonClick = Confirm;
                messageBox.OkText = ModLocalize<TypeMod>.Ok;
                messageBox.Init(messages, GetVersionString);
            }
            else
            {
                var messageBox = MessageBoxBase.ShowModal<BetaWhatsNewMessageBox>();
                messageBox.CaptionText = string.Format(ModLocalize<TypeMod>.WhatsNewCaption, NameRaw);
                messageBox.OnButtonClick = Confirm;
                messageBox.OnGetStableClick = GetStable;
                messageBox.OkText = ModLocalize<TypeMod>.Ok;
                messageBox.GetStableText = ModLocalize<TypeMod>.BetaWarningGetStable;
                messageBox.Init(messages, string.Format(ModLocalize<TypeMod>.BetaWarningMessage, NameRaw), GetVersionString);
            }

            static bool Confirm()
            {
                BaseSettings<TypeMod>.WhatsNewVersion.value = SingletonMod<TypeMod>.Version.ToString();
                return true;
            }
        }

        public Dictionary<Version, string> GetWhatsNewMessages(Version whatNewVersion)
        {
            var messages = new Dictionary<Version, string>(Versions.Count);
#if BETA
            messages[Version] = ModLocalize<TypeMod>.WhatsNewMessageBeta;
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
        public string GetVersionString(Version version) => string.Format(ModLocalize<TypeMod>.WhatsNewVersion, version == Version ? VersionString : version.ToString());

        public void ShowBetaWarning()
        {
            if (!IsBeta)
                BaseSettings<TypeMod>.BetaWarning.value = true;
            else if (BaseSettings<TypeMod>.BetaWarning.value)
            {
                var messageBox = MessageBoxBase.ShowModal<TwoButtonMessageBox>();
                messageBox.CaptionText = ModLocalize<TypeMod>.BetaWarningCaption;
                messageBox.MessageText = string.Format(ModLocalize<TypeMod>.BetaWarningMessage, NameRaw);
                messageBox.Button1Text = ModLocalize<TypeMod>.BetaWarningAgree;
                messageBox.Button2Text = ModLocalize<TypeMod>.BetaWarningGetStable;
                messageBox.OnButton1Click = AgreeClick;
                messageBox.OnButton2Click = GetStable;

                static bool AgreeClick()
                {
                    BaseSettings<TypeMod>.BetaWarning.value = false;
                    return true;
                }
            }
        }

        private bool GetStable()
        {
            BetaWorkshopUrl.OpenUrl();
            return true;
        }
        protected bool OpenWorkshop()
        {
            (!IsBeta ? WorkshopUrl : BetaWorkshopUrl).OpenUrl();
            return true;
        }
    }

    public static class ModLocalize<TypeMod>
        where TypeMod : BaseMod<TypeMod>
    {
        public static string Yes => SingletonMod<TypeMod>.GetLocalizeString("MessageBox_Yes");
        public static string No => SingletonMod<TypeMod>.GetLocalizeString("MessageBox_No");
        public static string Ok => SingletonMod<TypeMod>.GetLocalizeString("MessageBox_OK");
        public static string Cancel => SingletonMod<TypeMod>.GetLocalizeString("MessageBox_Cancel");

        public static string WhatsNewCaption => SingletonMod<TypeMod>.GetLocalizeString("Mod_WhatsNewCaption");
        public static string WhatsNewVersion => SingletonMod<TypeMod>.GetLocalizeString("Mod_WhatsNewVersion");

        public static string WhatsNewMessageBeta => SingletonMod<TypeMod>.GetLocalizeString("Mod_WhatsNewMessageBeta");
        public static string BetaWarningAgree => SingletonMod<TypeMod>.GetLocalizeString("Mod_BetaWarningAgree");
        public static string BetaWarningGetStable => SingletonMod<TypeMod>.GetLocalizeString("Mod_BetaWarningGetStable");

        public static string BetaWarningCaption => SingletonMod<TypeMod>.GetLocalizeString("Mod_BetaWarningCaption");
        public static string BetaWarningMessage => SingletonMod<TypeMod>.GetLocalizeString("Mod_BetaWarningMessage");
    }
}
