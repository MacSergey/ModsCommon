﻿using ColossalFramework;
using ColossalFramework.Globalization;
using ICities;
using ModsCommon.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ModsCommon
{
    public abstract class BaseMod<ModType> : IUserMod
        where ModType : BaseMod<ModType>
    {
        public static ModType Instance { get; protected set; }
        public static Logger Logger => Instance.ModLogger;
        public static Version Version => Instance.ModVersion;
        public static List<Version> Versions => Instance.ModVersions;
        public static bool IsBeta => Instance.ModIsBeta;
        public static string ShortName => Instance.ModName;

        public string Name => !ModIsBeta ? $"{ModName} {Version.GetString()}" : $"{ModName} {ModVersion.GetString()} [BETA]";
        public string Description => ModDescription;

        protected abstract string ModName { get; }
        protected abstract string ModDescription { get; }

        protected virtual bool LoadSuccess { get; set; }

        protected Logger ModLogger { get; private set; }
        public abstract string WorkshopUrl { get; }
        protected abstract Version ModVersion { get; }
        protected abstract List<Version> ModVersions { get; }
        public abstract string Id { get; }
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
            Instance = (ModType)this;
            ModLogger = new Logger(Id);
        }
        public virtual void OnEnabled()
        {
            ModLogger.Debug($"Version {ModVersion}");
            ModLogger.Debug($"Enabled");
            LoadSuccess = true;
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
            if (!ItemsExtension.OnStartup && !LoadSuccess)
                OnLoadedError();
        }
        public virtual void OnLoadedError() { }
    }
    public abstract class BasePatcherMod<ModType, PatcherType> : BaseMod<ModType>
        where ModType : BaseMod<ModType>
        where PatcherType : Patcher<ModType>
    {
        protected override bool LoadSuccess
        {
            get => base.LoadSuccess && Patcher.Success;
            set => base.LoadSuccess = value;
        }
        protected PatcherType Patcher { get; private set; }

        public override void OnEnabled()
        {
            base.OnEnabled();
            Logger.Debug(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            
            try
            {
                Patcher = CreatePatcher();
                Patcher.Patch();
            }
            catch (Exception error)
            {
                LoadSuccess = false;
                Logger.Error("Patch failed", error);
            }

            CheckLoadedError();
        }
        public override void OnDisabled()
        {
            base.OnDisabled();

            try { Patcher.Unpatch(); }
            catch (Exception error) { Logger.Error("Unpatch failed", error); }
        }
        protected abstract PatcherType CreatePatcher();
    }
}
