using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ColossalFramework.Plugins.PluginManager;

namespace ModsCommon.Utilities
{
    public class DependenciesWatcher : MonoBehaviour
    {
        private PluginInfo plugin;
        private List<BaseDependencyWatcher> dependencies;

        public ICustomMod ModInstance { get; private set; }
        public ILogger logger => ModInstance.Logger;

        private PluginSearcher PluginSearcher => new UserModInstanceSearcher(ModInstance);
        private PluginInfo Plugin => plugin ??= PluginSearcher.GetPlugin();
        private List<BaseDependencyInfo> Infos { get; set; }

        private List<BaseDependencyWatcher> Dependencies => dependencies ??= Infos.SelectMany(i => i.GetWatcher(this)).ToList();
        private DependenciesMessageBox MessageBox { get; set; }

        public bool IsValid => State == WatcherState.Valid;
        private WatcherState state = WatcherState.Valid;
        private WatcherState State
        {
            get => state;
            set
            {
                if (value != state)
                {
                    state = value;
                    ModInstance.Logger.Debug(IsValid ? "Dependencies valid" : "Dependencies not valid");
                    UpdateMessageBox();
                }
            }
        }

        public DependenciesWatcher()
        {
            enabled = false;
            LoadingManager.instance.m_levelPreLoaded += LevelPreLoaded;
            LoadingManager.instance.m_levelUnloaded += LevelUnloaded;
        }

        public void SetState(bool state)
        {
            if (state)
                enabled = true;
            else if (MessageBox == null)
                enabled = false;
        }
        private void OnEnable()
        {
            ModInstance.Logger.Debug("Dependencies watcher enabled");

            PluginManager.instance.eventPluginsChanged += UpdateWatchers;
            SetWatchersState();
            UpdateState();
        }
        private void OnDisable()
        {
            ModInstance.Logger.Debug("Dependencies watcher disabled");

            state = WatcherState.Valid;
            PluginManager.instance.eventPluginsChanged -= UpdateWatchers;
            SetWatchersState();
        }
        private void LevelPreLoaded()
        {
            if (Plugin.isEnabled)
            {
                ModInstance.Logger.Debug("Start load level");
                SetState(false);
            }
        }
        private void LevelUnloaded()
        {
            if (Plugin.isEnabled)
            {
                ModInstance.Logger.Debug("Level unloaded");
                SetState(Utility.OnMenu);
            }
        }

        private void SetWatchersState()
        {
            foreach (var watcher in Dependencies)
                watcher.IsEnabled = enabled;
        }

        private void UpdateWatchers()
        {
            foreach (var watcher in Dependencies)
                watcher.Update();
        }
        public void UpdateState()
        {
            var state = WatcherState.Valid;

            foreach (var dependency in Dependencies)
            {
                if (dependency.IsResolved)
                    continue;
                else if (dependency is RequiredDependencyWatcher)
                    state |= WatcherState.Missing;
                else if (dependency is ConflictDependencyWatcher)
                    state |= WatcherState.Conflict;
            }

            State = state;
        }

        private void ShowMessageBox()
        {
            if (MessageBox == null)
            {
                MessageBox = UI.MessageBox.Show<DependenciesMessageBox>();
                MessageBox.CaptionText = ModInstance.Name;
                MessageBox.OnButtonClick = () => IsValid ? EnablePlugin() : DisablePlugin();
                MessageBox.OnCloseClick += () => enabled = false;

                UpdateMessageBox();
            }
        }
        private void Hide()
        {
            if (MessageBox != null)
            {
                UI.MessageBox.Hide(MessageBox);
                MessageBox = null;
            }
        }
        public PluginRequest AddRequest(bool setState = true)
        {
            ShowMessageBox();

            if(setState)
                Plugin.SetState(false);

            UpdateState();
            return MessageBox.AddRequest();
        }
        private void UpdateMessageBox()
        {
            if (MessageBox == null)
                return;
            else if (IsValid)
            {
                MessageBox.MessageText = string.Format(CommonLocalize.Dependency_NoIssues, ModInstance.NameRaw);
                MessageBox.ButtonText = string.Format(CommonLocalize.Dependency_EnableMod, ModInstance.NameRaw);
            }
            else
            {
                var text = State switch
                {
                    WatcherState.Missing => CommonLocalize.Dependency_Missing,
                    WatcherState.Conflict => CommonLocalize.Dependency_Conflict,
                    WatcherState.All => CommonLocalize.Dependency_MissingAndConflict,
                };

                MessageBox.MessageText = $"{text}\n{string.Format(CommonLocalize.Dependency_NeedFix, ModInstance.NameRaw)}";
                MessageBox.ButtonText = string.Format(CommonLocalize.Dependency_DisableMod, ModInstance.NameRaw);
            }
        }

        public void Update()
        {
            if (MessageBox != null)
                UpdateState();
        }
        private bool EnablePlugin()
        {
            Plugin.SetState(true);
            Hide();
            return true;
        }
        private bool DisablePlugin()
        {
            Plugin.SetState(false);
            Hide();
            SetState(false);
            return true;
        }

        public static DependenciesWatcher Create(ICustomMod instance, List<BaseDependencyInfo> infos)
        {
            var gameObject = new GameObject();
            DontDestroyOnLoad(gameObject);
            var watcher = gameObject.AddComponent<DependenciesWatcher>();

            watcher.ModInstance = instance;
            watcher.Infos = infos;

            return watcher;
        }

        private enum WatcherState
        {
            Valid = 0,
            Missing = 1,
            Conflict = 2,
            All = Missing | Conflict,
        }
    }

    public abstract class BaseDependencyWatcher
    {
        public static bool IsWorkshopAvailable => PlatformService.platformType == PlatformType.Steam && !noWorkshop;

        public abstract bool IsEnabled { get; set; }
        public DependenciesWatcher MainWatcher { get; }
        public abstract bool IsResolved { get; }

        public BaseDependencyWatcher(DependenciesWatcher watcher)
        {
            MainWatcher = watcher;
        }

        public abstract void Update();
        protected PluginRequest AddRequest() => MainWatcher.AddRequest();
        protected float FakeProgress() => Mathf.Clamp01(DateTime.Now.Millisecond * 0.00125f);
    }
}
