using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static ColossalFramework.Plugins.PluginManager;

namespace ModsCommon.Utilities
{
    public abstract class ConflictDependencyWatcher : BaseDependencyWatcher
    {
        private bool enable;
        public override bool IsEnabled
        {
            get => enable;
            set
            {
                if (value != enable)
                {
                    enable = value;

                    foreach (var watcher in Watchers.Values)
                    {
                        if(watcher != null)
                            watcher.Enable = IsEnabled;
                    }

                    if (IsEnabled)
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

        protected abstract DependencyMessageState GetState(PluginInfo plugin);
        protected abstract string GetLabel(PluginInfo plugin);
        protected abstract string GetRequiredText(PluginInfo plugin);
        protected abstract string GetResolvedText(PluginInfo plugin);
        protected abstract Action GetAction(PluginInfo plugin);
        protected abstract Func<float> GetProgress(PluginInfo plugin);

        public override void Update()
        {
            var plugins = Info.Searcher.GetPlugins().ToHashSet();

            foreach (var plugin in plugins)
            {
                if (!Watchers.TryGetValue(plugin, out var watcher) || watcher == null)
                {
                    watcher = new PluginStateWatcher(plugin, IsEnabled);
                    watcher.StateChanged += WatcherStateChanged;
                    Watchers[plugin] = watcher;
                }
            }

            foreach (var plugin in Watchers.Keys.ToArray())
            {
                if (!plugins.Contains(plugin) && Watchers[plugin] is PluginStateWatcher watcher)
                {
                    watcher.StateChanged -= WatcherStateChanged;
                    Watchers[plugin] = null;
                }
            }

            foreach (var plugin in Watchers.Keys)
                UpdateMessage(plugin);
        }
        protected void UpdateMessage(PluginInfo plugin)
        {
            if (!MainWatcher.enabled)
                return;

            Messages.TryGetValue(plugin, out var message);

            var state = GetState(plugin);
            switch (state)
            {
                case DependencyMessageState.Required:
                    if (message == null)
                    {
                        message = AddMessage();
                        Messages[plugin] = message;
                    }

                    if (message != null)
                    {
                        OnRequire(plugin);
                        message.State = DependencyMessageState.Required;
                    }
                    break;
                case DependencyMessageState.InProgress:
                    if (message != null)
                        message.State = DependencyMessageState.InProgress;
                    break;
                case DependencyMessageState.Resolved:
                    if (message != null)
                    {
                        OnResolve(plugin);
                        message.State = DependencyMessageState.Resolved;
                    }
                    break;
            }

            if (message != null)
            {
                message.Text = GetLabel(plugin);
                message.RequiredText = GetRequiredText(plugin);
                message.ResolvedText = GetResolvedText(plugin);
                message.OnButtonClick = GetAction(plugin);
                message.GetProgress = GetProgress(plugin);
            }
        }

        protected virtual void OnRequire(PluginInfo plugin) { }
        protected virtual void OnResolve(PluginInfo plugin) { }

        protected virtual void WatcherStateChanged(PluginInfo plugin, bool state) => UpdateMessage(plugin);
    }
}
