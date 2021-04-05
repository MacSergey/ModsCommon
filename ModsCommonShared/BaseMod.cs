using ColossalFramework;
using ColossalFramework.Globalization;
using ICities;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ModsCommon
{
    public abstract class BaseMod<TypeMod> : IUserMod
        where TypeMod : BaseMod<TypeMod>
    {
        public static string BETA => "[BETA]";

        protected virtual bool LoadSuccess { get; set; }

        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public string VersionString => !IsBeta ? Version.ToString() : $"{Version} {BETA}";

        public string Name => !IsBeta ? $"{NameRow} {Version.GetString()}" : $"{NameRow} {Version.GetString()} {BETA}";
        protected abstract string NameRow { get; }
        public abstract string Description { get; }

        public Logger Logger { get; private set; }
        public abstract string WorkshopUrl { get; }
        public abstract List<Version> Versions { get; }
        protected abstract string IdRow { get; }
        public string Id => !IsBeta ? IdRow : $"{IdRow} BETA";
        public abstract bool IsBeta { get; }
        protected abstract string Locale { get; }

        protected CultureInfo Culture
        {
            get
            {
                var locale = string.IsNullOrEmpty(Locale) ? SingletonLite<LocaleManager>.instance.language : Locale;
                if (locale == "zh")
                    locale = "zh-cn";

                return new CultureInfo(locale);
            }
        }

        public BaseMod()
        {
            Logger = new Logger(Id);
            SingletonMod<TypeMod>.Instance = (TypeMod)this;
        }
        public virtual void OnEnabled()
        {
            Logger.Debug($"Version {VersionString}");
            Logger.Debug($"Enabled");
            LoadSuccess = true;
            LoadingManager.instance.m_introLoaded += CheckLoadedError;
        }
        public virtual void OnDisabled()
        {
            Logger.Debug($"Disabled");
            LoadingManager.instance.m_introLoaded -= CheckLoadedError;
            LocaleManager.eventLocaleChanged -= LocaleChanged;
        }
        public void OnSettingsUI(UIHelperBase helper)
        {
            LocaleManager.eventLocaleChanged -= LocaleChanged;
            LocaleChanged();
            LocaleManager.eventLocaleChanged += LocaleChanged;

            Logger.Debug($"Load SettingsUI");
            GetSettings(helper);
        }
        protected virtual void GetSettings(UIHelperBase helper) { }

        public virtual void LocaleChanged() { }

        public void CheckLoadedError()
        {
            if (!ItemsExtension.OnStartup && !LoadSuccess)
                OnLoadedError();
        }
        public virtual void OnLoadedError() { }
    }
}
