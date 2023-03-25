using System;

namespace ModsCommon.Utilities
{
    public class EnableDependencyWatcher : SubscribeDependencyWatcher
    {
        public override bool IsResolved => Watcher != null && Watcher.IsPluginEnabled;

        protected override DependencyMessageState State
        {
            get
            {
                if (Watcher == null)
                    return base.State;
                else if (!Watcher.IsPluginEnabled)
                    return DependencyMessageState.Required;
                else
                    return DependencyMessageState.Resolved;
            }
        }

        protected override string Label => Watcher == null ? base.Label : Watcher.Name;
        protected override string RequiredText => Watcher == null ? base.RequiredText : CommonLocalize.Dependency_Enable;
        protected override string ResolvedText => Watcher == null ? base.ResolvedText : CommonLocalize.Dependency_Enabled;
        protected override Action Action => Watcher == null ? base.Action : Enable;
        protected override Func<float> Progress => Watcher == null ? base.Progress : null;

        public EnableDependencyWatcher(DependenciesWatcher watcher, RequiredDependencyInfo info) : base(watcher, info) { }

        private void Enable()
        {
            Message.State = DependencyMessageState.InProgress;
            Watcher.Plugin.SetState(true);
        }

        protected override void OnRequire() => MainWatcher.logger.Debug($"Detected not enable dependency: {PluginName}");
        protected override void OnResolve() => MainWatcher.logger.Debug($"Dependency enabled: {PluginName}");
    }
}
