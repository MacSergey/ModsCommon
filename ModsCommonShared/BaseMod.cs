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
    public abstract class BaseMod : IUserMod
    {
        public static BaseMod Instance { get; protected set; }
        public static Logger Logger => Instance.ModLogger;
        public static Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public static string VersionString => Instance.ModVersionString;
        public static List<Version> Versions => Instance.ModVersions;
        public static string Id => !IsBeta ? Instance.ModId : $"{Instance.ModId} BETA";
        public static bool IsBeta => Instance.ModIsBeta;
        public static string ShortName => Instance.ModName;
        public static string BETA => "[BETA]";

        public string Name => !ModIsBeta ? $"{ModName} {Version.GetString()}" : $"{ModName} {Version.GetString()} {BETA}";
        public string Description => ModDescription;

        protected abstract string ModName { get; }
        protected abstract string ModDescription { get; }

        protected virtual bool LoadError { get; set; }

        public Logger ModLogger { get; private set; }
        public abstract string WorkshopUrl { get; }
        protected string ModVersionString => !ModIsBeta ? Version.ToString() : $"{Version} {BETA}";
        protected abstract List<Version> ModVersions { get; }
        protected abstract string ModId { get; }
        protected abstract bool ModIsBeta { get; }
        protected abstract string ModLocale { get; }

        protected CultureInfo Culture
        {
            get
            {
                var locale = string.IsNullOrEmpty(ModLocale) ? SingletonLite<LocaleManager>.instance.language : ModLocale;
                if (locale == "zh")
                    locale = "zh-cn";

                return new CultureInfo(locale);
            }
        }

        public BaseMod()
        {
            Instance = this;
            ModLogger = new Logger(Id);
        }
        public virtual void OnEnabled()
        {
            ModLogger.Debug($"Version {ModVersionString}");
            ModLogger.Debug($"Enabled");
            LoadError = false;
            LoadingManager.instance.m_introLoaded += CheckLoadedError;
        }

        public virtual void OnDisabled()
        {
            ModLogger.Debug($"Disabled");
            LoadingManager.instance.m_introLoaded -= CheckLoadedError;
            LocaleManager.eventLocaleChanged -= LocaleChanged;
        }
        public void OnSettingsUI(UIHelperBase helper)
        {
            LocaleManager.eventLocaleChanged -= LocaleChanged;
            LocaleChanged();
            LocaleManager.eventLocaleChanged += LocaleChanged;

            ModLogger.Debug($"Load SettingsUI");
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
    public abstract class BasePatcherMod : BaseMod
    {
        protected override bool LoadError
        {
            get => base.LoadError || !Patcher.Success;
            set => base.LoadError = value;
        }
        protected BasePatcher Patcher { get; private set; }

        public override void OnEnabled()
        {
            base.OnEnabled();

            try
            {
                Patcher = CreatePatcher();
                Patcher.Patch();
            }
            catch (Exception error)
            {
                LoadError = true;
                ModLogger.Error("Patch failed", error);
            }

            CheckLoadedError();
        }
        public override void OnDisabled()
        {
            base.OnDisabled();

            try { Patcher.Unpatch(); }
            catch (Exception error) { ModLogger.Error("Unpatch failed", error); }
        }
        protected abstract BasePatcher CreatePatcher();
    }
}
