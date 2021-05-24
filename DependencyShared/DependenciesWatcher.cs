using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ColossalFramework.Plugins.PluginManager;

namespace ModsCommon.Utilities
{
    public class DependenciesWatcher : MonoBehaviour
    {
        private PluginInfo _plugin;
        private List<BaseDependencyWatcher> _dependencies;

        public ICustomMod ModInstance { get; private set; }
        public ILogger logger => ModInstance.Logger;

        private PluginSearcher PluginSearcher => new UserModInstanceSearcher(ModInstance);
        private PluginInfo Plugin => _plugin ??= PluginSearcher.GetPlugin();
        private List<BaseDependencyInfo> Infos { get; set; }

        private List<BaseDependencyWatcher> Dependencies => _dependencies ??= Infos.SelectMany(i => i.GetWatcher(this)).ToList();
        private DependenciesMessageBox MessageBox { get; set; }

        private bool _isValid = true;
        public bool IsValid
        {
            get => _isValid;
            private set
            {
                if (value != _isValid)
                {
                    _isValid = value;
                    ModInstance.Logger.Debug(IsValid ? "Dependencies valid" : "Dependencies not valid");

                    UpdateMessageBox();
                }
            }
        }
        private bool IsMissing { get; set; }
        private bool IsConflict { get; set; }

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
            UpdateValid();
        }
        private void OnDisable()
        {
            ModInstance.Logger.Debug("Dependencies watcher disabled");

            _isValid = true;
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
                SetState(Utility.InMenu);
            }
        }

        private void SetWatchersState()
        {
            foreach (var watcher in Dependencies)
                watcher.Enable = enabled;
        }

        private void UpdateWatchers()
        {
            foreach (var watcher in Dependencies)
                watcher.Update();
        }
        public void UpdateValid()
        {
            IsMissing = false;
            IsConflict = false;
            foreach (var dependency in Dependencies)
            {
                if (dependency.IsValid)
                    continue;
                else if (dependency is NeedDependencyWatcher)
                    IsMissing = true;
                else if (dependency is ConflictDependencyWatcher)
                    IsConflict = true;
            }

            IsValid = !IsMissing && !IsConflict;
        }

        private void Show()
        {
            if (MessageBox == null)
            {
                MessageBox = UI.MessageBox.Show<DependenciesMessageBox>();
                MessageBox.CaptionText = ModInstance.Name;
                MessageBox.OnButtonClick = () => IsValid ? EnablePlugin() : DisablePlugin();
                MessageBox.OnCloseClick += () => enabled = false;

                Plugin.SetState(false);
                UpdateValid();
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
        public PluginMessage AddMessage()
        {
            Show();
            return MessageBox.Content.AddUIComponent<PluginMessage>();
        }
        public void RemoveMessage(PluginMessage message)
        {
            MessageBox.Content.RemoveUIComponent(message);
            Destroy(message.gameObject);
            Destroy(message);
        }
        private void UpdateMessageBox()
        {
            if (IsValid)
            {
                MessageBox.MessageText = string.Format(CommonLocalize.Dependency_NoIssues, ModInstance.NameRaw);
                MessageBox.ButtonText = string.Format(CommonLocalize.Dependency_EnableMod, ModInstance.NameRaw);
            }
            else
            {
                var text = string.Empty;
                if (IsMissing && IsConflict)
                    text = CommonLocalize.Dependency_MissingAndConflict;
                else if (IsMissing)
                    text = CommonLocalize.Dependency_Missing;
                else if (IsConflict)
                    text = CommonLocalize.Dependency_Conflict;

                MessageBox.MessageText = $"{text}\n{string.Format(CommonLocalize.Dependency_NeedFix, ModInstance.NameRaw)}";
                MessageBox.ButtonText = string.Format(CommonLocalize.Dependency_DisableMod, ModInstance.NameRaw);
            }
        }

        public void Update()
        {
            if (MessageBox != null)
                UpdateValid();
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
    }


    public abstract class BaseDependencyWatcher
    {
        public bool WorkshopAvailable => PlatformService.platformType == PlatformType.Steam && !noWorkshop;

        public abstract bool Enable { get; set; }
        public DependenciesWatcher MainWatcher { get; }
        public abstract bool IsValid { get; }

        public BaseDependencyWatcher(DependenciesWatcher watcher)
        {
            MainWatcher = watcher;
        }

        public abstract void Update();
        protected virtual PluginMessage AddMessage() => MainWatcher.AddMessage();
        protected virtual void RemoveMessage(PluginMessage message) => MainWatcher.RemoveMessage(message);
    }

    public abstract class NeedDependencyWatcher : BaseDependencyWatcher
    {
        private bool _enable;
        public override bool Enable
        {
            get => _enable;
            set
            {
                if (value != _enable)
                {
                    _enable = value;
                    if (Watcher is PluginStateWatcher watcher)
                        watcher.Enable = Enable;

                    if (Enable)
                        Update();
                    else
                        Message = null;
                }
            }
        }

        protected NeedDependencyInfo Info { get; }
        protected PluginStateWatcher Watcher { get; set; }
        protected PluginMessage Message { get; set; }

        protected abstract bool NeedAddMessage { get; }
        protected abstract bool NeedRemoveMessage { get; }

        public NeedDependencyWatcher(DependenciesWatcher watcher, NeedDependencyInfo info) : base(watcher)
        {
            Info = info;
        }

        public override void Update()
        {
            if (Info.Searcher.GetPlugin() is not PluginInfo plugin)
                RemoveWatcher();
            else if (Watcher == null || Watcher.Plugin != plugin)
                AddWatcher(plugin);

            UpdateMessage();
        }

        private void AddWatcher(PluginInfo plugin)
        {
            RemoveWatcher();

            Watcher = new PluginStateWatcher(plugin, Enable);
            Watcher.StateChanged += WatcherStateChanged;
        }
        private void RemoveWatcher()
        {
            if (Watcher != null)
            {
                Watcher.StateChanged -= WatcherStateChanged;
                Watcher = null;
            }
        }

        private void WatcherStateChanged(PluginInfo plugin, bool state) => UpdateMessage();

        private void UpdateMessage()
        {
            if (!MainWatcher.enabled)
                return;

            if (Message != null && NeedRemoveMessage)
            {
                RemoveMessage(Message);
                Message = null;
            }

            if (Message == null && NeedAddMessage)
                Message = AddMessage();
        }
    }
    public class SubscribeDependencyWatcher : NeedDependencyWatcher
    {
        protected override bool NeedAddMessage => Watcher == null;
        protected override bool NeedRemoveMessage => Watcher != null;

        public override bool IsValid => Watcher != null;

        public SubscribeDependencyWatcher(DependenciesWatcher watcher, NeedDependencyInfo info) : base(watcher, info) { }

        protected override PluginMessage AddMessage()
        {
            MainWatcher.logger.Debug($"Detected missing dependency: {Info.Name}");

            var message = base.AddMessage();
            message.Text = Info.Name;
            message.ButtonText = WorkshopAvailable ? CommonLocalize.Dependency_Subscribe : CommonLocalize.Dependency_Get;
            message.OnButtonClick = Subscribe;
            message.GetProgress = GetProgress;

            return message;
        }
        protected override void RemoveMessage(PluginMessage message)
        {
            MainWatcher.logger.Debug($"Dependency found: {Watcher.Name}");
            base.RemoveMessage(message);
        }

        private void Subscribe()
        {
            if (Info.Id != 0ul)
            {
                if (WorkshopAvailable)
                {
                    MainWatcher.logger.Debug($"Subscribe missing dependency: {Watcher.Name}");
                    Message.InProgress = true;
                    PlatformService.workshop.Subscribe(new PublishedFileId(Info.Id));
                }
                else
                    Info.Id.OpenWorkshop();
            }
        }
        private float GetProgress() => PlatformService.workshop.GetSubscribedItemProgress(new PublishedFileId(Info.Id));
    }
    public class EnableDependencyWatcher : NeedDependencyWatcher
    {
        protected override bool NeedAddMessage => Watcher != null && !Watcher.IsPluginEnabled;
        protected override bool NeedRemoveMessage => Watcher == null || Watcher.IsPluginEnabled;

        public override bool IsValid => Watcher?.IsPluginEnabled == true;

        public EnableDependencyWatcher(DependenciesWatcher watcher, NeedDependencyInfo info) : base(watcher, info) { }

        protected override PluginMessage AddMessage()
        {
            MainWatcher.logger.Debug($"Detected not enable dependency: {Info.Name}");

            var message = base.AddMessage();
            message.Text = Watcher.Name;
            message.ButtonText = CommonLocalize.Dependency_Enable;
            message.OnButtonClick = () => Watcher.Plugin.SetState(true);

            return message;
        }
        protected override void RemoveMessage(PluginMessage message)
        {
            MainWatcher.logger.Debug($"Dependency enabled: {Watcher.Name}");
            base.RemoveMessage(message);
        }
    }

    public abstract class ConflictDependencyWatcher : BaseDependencyWatcher
    {
        private bool _enable;
        public override bool Enable
        {
            get => _enable;
            set
            {
                if (value != _enable)
                {
                    _enable = value;

                    foreach (var watcher in Watchers.Values)
                        watcher.Enable = Enable;

                    if (Enable)
                        Update();
                    else
                        Messages.Clear();
                }
            }
        }

        protected ConflictDependencyInfo Info { get; }
        protected Dictionary<PluginInfo, PluginStateWatcher> Watchers { get; } = new Dictionary<PluginInfo, PluginStateWatcher>();
        protected Dictionary<PluginInfo, PluginMessage> Messages { get; } = new Dictionary<PluginInfo, PluginMessage>();

        public ConflictDependencyWatcher(DependenciesWatcher watcher, ConflictDependencyInfo info) : base(watcher)
        {
            Info = info;
        }

        public override void Update()
        {
            var plugins = Info.Searcher.GetPlugins().ToHashSet();

            foreach (var plugin in plugins)
            {
                if (!Watchers.ContainsKey(plugin))
                {
                    var watcher = new PluginStateWatcher(plugin, Enable);
                    watcher.StateChanged += WatcherStateChanged;
                    Watchers[plugin] = watcher;
                }
            }

            foreach (var plugin in Watchers.Keys.ToArray())
            {
                if (!plugins.Contains(plugin))
                {
                    var watcher = Watchers[plugin];
                    watcher.StateChanged -= WatcherStateChanged;
                    Watchers.Remove(plugin);
                }
            }

            UpdateMessages();
        }

        private void UpdateMessages()
        {
            if (!MainWatcher.enabled)
                return;

            foreach (var plugin in Messages.Keys.ToArray())
                RemoveMessageImpl(plugin);

            foreach (var plugin in Watchers.Keys)
                AddMessageImpl(plugin);
        }
        protected void UpdateMessage(PluginInfo plugin)
        {
            RemoveMessageImpl(plugin);
            AddMessageImpl(plugin);
        }
        private void RemoveMessageImpl(PluginInfo plugin)
        {
            if (Messages.TryGetValue(plugin, out var message) && NeedRemoveMessage(plugin))
            {
                RemoveMessage(plugin, message);
                Messages.Remove(plugin);
            }
        }
        private void AddMessageImpl(PluginInfo plugin)
        {
            if (!Messages.ContainsKey(plugin) && NeedAddMessage(plugin))
                Messages[plugin] = AddMessage(plugin);
        }

        protected abstract PluginMessage AddMessage(PluginInfo plugin);
        protected abstract void RemoveMessage(PluginInfo plugin, PluginMessage message);

        protected abstract bool NeedAddMessage(PluginInfo plugin);
        protected abstract bool NeedRemoveMessage(PluginInfo plugin);

        protected virtual void WatcherStateChanged(PluginInfo plugin, bool state) { }
    }
    public class UnsubscribeDependencyWatcher : ConflictDependencyWatcher
    {
        public override bool IsValid => Watchers.Count == 0;

        public UnsubscribeDependencyWatcher(DependenciesWatcher watcher, ConflictDependencyInfo info) : base(watcher, info) { }

        protected override PluginMessage AddMessage(PluginInfo plugin)
        {
            MainWatcher.logger.Debug($"Detected conflict mod: {plugin.GetName()}");

            var message = base.AddMessage();
            message.Text = plugin.GetName();
            message.ButtonText = WorkshopAvailable ? CommonLocalize.Dependency_Unsubscribe : CommonLocalize.Dependency_Remove;
            message.OnButtonClick = () => Unsubscribe(plugin);

            return message;
        }
        protected override void RemoveMessage(PluginInfo plugin, PluginMessage message)
        {
            MainWatcher.logger.Debug($"Conflict mod no more exist: {plugin.GetName()}");
            base.RemoveMessage(message);
        }

        protected override bool NeedAddMessage(PluginInfo plugin) => Watchers.ContainsKey(plugin);
        protected override bool NeedRemoveMessage(PluginInfo plugin) => !Watchers.ContainsKey(plugin);

        private void Unsubscribe(PluginInfo plugin)
        {
            if (plugin.publishedFileID == PublishedFileId.invalid)
            {
                MainWatcher.logger.Debug($"Unsubscribe conflict mod: {plugin.GetName()}");
                Directory.Delete(plugin.modPath, true);
            }
            else if (WorkshopAvailable)
            {
                MainWatcher.logger.Debug($"Remove conflict mod: {plugin.GetName()}");
                PlatformService.workshop.Unsubscribe(plugin.publishedFileID);
            }
        }
    }
    public class DisableDependencyWatcher : ConflictDependencyWatcher
    {
        public override bool IsValid => Watchers.Values.All(w => !w.IsPluginEnabled);

        public DisableDependencyWatcher(DependenciesWatcher watcher, ConflictDependencyInfo info) : base(watcher, info) { }

        protected override PluginMessage AddMessage(PluginInfo plugin)
        {
            MainWatcher.logger.Debug($"Detected not disabled conflict mod: {plugin.GetName()}");

            var message = base.AddMessage();
            message.Text = plugin.GetName();
            message.ButtonText = CommonLocalize.Dependency_Disable;
            message.OnButtonClick = () => plugin.SetState(false);

            return message;
        }
        protected override void RemoveMessage(PluginInfo plugin, PluginMessage message)
        {
            MainWatcher.logger.Debug($"Conflict mod disabled: {plugin.GetName()}");
            base.RemoveMessage(message);
        }

        protected override bool NeedAddMessage(PluginInfo plugin) => Watchers.TryGetValue(plugin, out var watcher) && watcher.IsPluginEnabled;
        protected override bool NeedRemoveMessage(PluginInfo plugin) => !Watchers.TryGetValue(plugin, out var watcher) || !watcher.IsPluginEnabled;
        protected override void WatcherStateChanged(PluginInfo plugin, bool state) => UpdateMessage(plugin);
    }
}
