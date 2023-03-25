using System;
using static ColossalFramework.Plugins.PluginManager;

namespace ModsCommon.Utilities
{
    public abstract class RequiredDependencyWatcher : BaseDependencyWatcher
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
                    if (Watcher is PluginStateWatcher watcher)
                        watcher.Enable = IsEnabled;

                    if (IsEnabled)
                        Update();
                    else
                        Message = null;
                }
            }
        }
        protected string PluginName => Watcher?.Name ?? Info.Name;

        protected RequiredDependencyInfo Info { get; }
        protected PluginStateWatcher Watcher { get; set; }
        protected PluginRequest Message { get; set; }

        protected abstract DependencyMessageState State { get; }
        protected abstract string Label { get; }
        protected abstract string RequiredText { get; }
        protected abstract string ResolvedText { get; }
        protected abstract Action Action { get; }
        protected abstract Func<float> Progress { get; }

        public RequiredDependencyWatcher(DependenciesWatcher watcher, RequiredDependencyInfo info) : base(watcher)
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

            Watcher = new PluginStateWatcher(plugin, IsEnabled);
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

        protected virtual void WatcherStateChanged(PluginInfo plugin, bool state) => UpdateMessage();

        private void UpdateMessage()
        {
            if (!MainWatcher.enabled)
                return;

            switch (State)
            {
                case DependencyMessageState.Required:
                    if (Message == null)
                        Message = AddRequest();

                    if (Message != null)
                    {
                        OnRequire();
                        Message.State = DependencyMessageState.Required;
                    }
                    break;
                case DependencyMessageState.InProgress:
                    if (Message != null)
                        Message.State = DependencyMessageState.InProgress;
                    break;
                case DependencyMessageState.Resolved:
                    if (Message != null)
                    {
                        OnResolve();
                        Message.State = DependencyMessageState.Resolved;
                    }
                    break;
            }

            if(Message != null)
            {
                Message.Text = Label;
                Message.RequiredText = RequiredText;
                Message.ResolvedText = ResolvedText;
                Message.OnButtonClick = Action;
                Message.GetProgress = Progress;
            }
        }

        protected virtual void OnRequire() { }
        protected virtual void OnResolve() { }
    }
}
