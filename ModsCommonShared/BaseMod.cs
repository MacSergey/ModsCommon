using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine.SceneManagement;

namespace ModsCommon
{
    public abstract class BaseMod<TypeMod> : IUserMod
        where TypeMod : BaseMod<TypeMod>
    {
        public static string BETA => "[BETA]";

        protected virtual bool LoadError { get; set; }

        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public string VersionString => !IsBeta ? Version.ToString() : $"{Version} {BETA}";

        public string Name => !IsBeta ? $"{NameRaw} {Version.GetString()}" : $"{NameRaw} {Version.GetString()} {BETA}";
        public abstract string NameRaw { get; }
        public abstract string Description { get; }

        public Logger Logger { get; private set; }
        public abstract string WorkshopUrl { get; }
        public abstract List<Version> Versions { get; }
        protected abstract string IdRaw { get; }
        public string Id => !IsBeta ? IdRaw : $"{IdRaw} BETA";
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
            LoadError = false;
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
            if (UIView.GetAView() != null && LoadError)
                OnLoadedError();
        }
        public virtual void OnLoadedError() { }
    }
}
