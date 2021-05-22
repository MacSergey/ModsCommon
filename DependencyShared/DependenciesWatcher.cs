using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using ModsCommon.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using static ColossalFramework.Plugins.PluginManager;

namespace ModsCommon.Utilities
{
    public class DependenciesWatcher : MonoBehaviour
    {
        private bool Inited { get; set; }

        private PluginSearcher PluginSearcher { get; set; }
        private PluginInfo Plugin => PluginUtilities.GetPlugin(PluginSearcher);
        private string ModName { get; set; }
        private string ModFullName { get; set; }
        private List<BaseDependencyInfo> Infos { get; set; }

        private List<BaseDependencyWatcher> Dependencies { get; } = new List<BaseDependencyWatcher>();
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
                    UpdateMessageBox();

                    if (!IsValid)
                        Plugin.SetState(false);
                }
            }
        }
        private bool IsMissing { get; set; }
        private bool IsConflict { get; set; }

        public DependenciesWatcher()
        {
            enabled = false;
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
            if (!Inited)
            {
                foreach (var info in Infos)
                {
                    var dependency = info.GetWatcher(this);
                    Dependencies.AddRange(dependency);
                }
                Inited = true;
            }

            PluginManager.instance.eventPluginsChanged += UpdateDependencies;
            SetDependenciesState();
            UpdateValid();
        }
        private void OnDisable()
        {
            _isValid = true;
            PluginManager.instance.eventPluginsChanged -= UpdateDependencies;
            SetDependenciesState();
        }
        private void SetDependenciesState()
        {
            foreach (var dependency in Dependencies)
                dependency.Enable = enabled;
        }

        private void UpdateDependencies()
        {
            foreach (var dependency in Dependencies)
                dependency.Update();
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
                MessageBox.CaptionText = ModFullName;
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
                MessageBox.MessageText = string.Format(CommonLocalize.Dependency_NoIssues, ModName);
                MessageBox.ButtonText = string.Format(CommonLocalize.Dependency_EnableMod, ModName);
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

                MessageBox.MessageText = $"{text}\n{string.Format(CommonLocalize.Dependency_NeedFix, ModName)}";
                MessageBox.ButtonText = string.Format(CommonLocalize.Dependency_DisableMod, ModName);
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

        public static DependenciesWatcher Create(PluginSearcher searcher, List<BaseDependencyInfo> infos, string modName, string fullName = null)
        {
            var gameObject = new GameObject();
            DontDestroyOnLoad(gameObject);
            var watcher = gameObject.AddComponent<DependenciesWatcher>();

            watcher.PluginSearcher = searcher;
            watcher.ModName = modName;
            watcher.ModFullName = fullName ?? modName;
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
        protected PluginMessage AddMessage() => MainWatcher.AddMessage();
        protected void RemoveMessage(PluginMessage message) => MainWatcher.RemoveMessage(message);
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
            if (PluginUtilities.GetPlugin(Info.Searcher) is not PluginInfo plugin)
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
                Message = AddMessageImpl();
        }
        protected abstract PluginMessage AddMessageImpl();
    }
    public class SubscribeDependencyWatcher : NeedDependencyWatcher
    {
        protected override bool NeedAddMessage => Watcher == null;
        protected override bool NeedRemoveMessage => Watcher != null;

        public override bool IsValid => Watcher != null;

        public SubscribeDependencyWatcher(DependenciesWatcher watcher, NeedDependencyInfo info) : base(watcher, info) { }

        protected override PluginMessage AddMessageImpl()
        {
            var message = AddMessage();
            message.Text = Info.Name;
            message.ButtonText = WorkshopAvailable ? CommonLocalize.Dependency_Subscribe : CommonLocalize.Dependency_Get;
            message.OnButtonClick = Subscribe;
            message.GetProgress = GetProgress;

            return message;
        }
        private void Subscribe()
        {
            if (Info.Id != 0ul)
            {
                if (WorkshopAvailable)
                {
                    Message.InProgress = true;
                    PlatformService.workshop.Subscribe(new PublishedFileId(Info.Id));
                }
                else
                    Info.Id.OpenWorkshop();
            }
        }
        private float GetProgress()
        {
            var progress = PlatformService.workshop.GetSubscribedItemProgress(new PublishedFileId(Info.Id));
            Debug.Log($"Progress: {progress}");
            return progress;
        }
    }
    public class EnableDependencyWatcher : NeedDependencyWatcher
    {
        protected override bool NeedAddMessage => Watcher != null && !Watcher.IsPluginEnabled;
        protected override bool NeedRemoveMessage => Watcher == null || Watcher.IsPluginEnabled;

        public override bool IsValid => Watcher?.IsPluginEnabled == true;

        public EnableDependencyWatcher(DependenciesWatcher watcher, NeedDependencyInfo info) : base(watcher, info) { }

        protected override PluginMessage AddMessageImpl()
        {
            var message = AddMessage();
            message.Text = (Watcher.Plugin.userModInstance as IUserMod).Name;
            message.ButtonText = CommonLocalize.Dependency_Enable;
            message.OnButtonClick = () => Watcher.Plugin.SetState(true);

            return message;
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
            var plugins = PluginUtilities.GetPlugins(Info.Searcher).ToHashSet();

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
                RemoveMessage(plugin);

            foreach (var plugin in Watchers.Keys)
                AddMessage(plugin);
        }
        protected void UpdateMessage(PluginInfo plugin)
        {
            RemoveMessage(plugin);
            AddMessage(plugin);
        }
        private void RemoveMessage(PluginInfo plugin)
        {
            if (Messages.TryGetValue(plugin, out var message) && NeedRemoveMessage(plugin))
            {
                RemoveMessage(message);
                Messages.Remove(plugin);
            }
        }
        private void AddMessage(PluginInfo plugin)
        {
            if (!Messages.ContainsKey(plugin) && NeedAddMessage(plugin))
                Messages[plugin] = AddMessageImpl(plugin);
        }

        protected abstract PluginMessage AddMessageImpl(PluginInfo plugin);
        protected abstract bool NeedAddMessage(PluginInfo plugin);
        protected abstract bool NeedRemoveMessage(PluginInfo plugin);

        protected virtual void WatcherStateChanged(PluginInfo plugin, bool state) { }
    }
    public class UnsubscribeDependencyWatcher : ConflictDependencyWatcher
    {
        public override bool IsValid => Watchers.Count == 0;

        public UnsubscribeDependencyWatcher(DependenciesWatcher watcher, ConflictDependencyInfo info) : base(watcher, info) { }

        protected override PluginMessage AddMessageImpl(PluginInfo plugin)
        {
            var message = AddMessage();
            message.Text = (plugin.userModInstance as IUserMod).Name;
            message.ButtonText = WorkshopAvailable ? CommonLocalize.Dependency_Unsubscribe : CommonLocalize.Dependency_Remove;
            message.OnButtonClick = () => Unsubscribe(plugin);

            return message;
        }

        protected override bool NeedAddMessage(PluginInfo plugin) => Watchers.ContainsKey(plugin);
        protected override bool NeedRemoveMessage(PluginInfo plugin) => !Watchers.ContainsKey(plugin);

        private void Unsubscribe(PluginInfo plugin)
        {
            if (plugin.publishedFileID == PublishedFileId.invalid)
                Directory.Delete(plugin.modPath, true);
            else if (WorkshopAvailable)
                PlatformService.workshop.Unsubscribe(plugin.publishedFileID);
        }
    }
    public class DisableDependencyWatcher : ConflictDependencyWatcher
    {
        public override bool IsValid => Watchers.Values.All(w => !w.IsPluginEnabled);

        public DisableDependencyWatcher(DependenciesWatcher watcher, ConflictDependencyInfo info) : base(watcher, info) { }

        protected override PluginMessage AddMessageImpl(PluginInfo plugin)
        {
            var message = AddMessage();
            message.Text = (plugin.userModInstance as IUserMod).Name;
            message.ButtonText = CommonLocalize.Dependency_Disable;
            message.OnButtonClick = () => plugin.SetState(false);

            return message;
        }
        protected override bool NeedAddMessage(PluginInfo plugin) => Watchers.TryGetValue(plugin, out var watcher) && watcher.IsPluginEnabled;
        protected override bool NeedRemoveMessage(PluginInfo plugin) => !Watchers.TryGetValue(plugin, out var watcher) || !watcher.IsPluginEnabled;
        protected override void WatcherStateChanged(PluginInfo plugin, bool state) => UpdateMessage(plugin);
    }
}
