using System;
using System.Linq;
using static ColossalFramework.Plugins.PluginManager;

namespace ModsCommon.Utilities
{
    public class DisableDependencyWatcher : ConflictDependencyWatcher
    {
        public override bool IsResolved => Watchers.Values.All(watcher => watcher == null || !watcher.IsPluginEnabled);

        protected override string RequiredText => CommonLocalize.Dependency_Disable;
        protected override string ResolvedText => "Disabled";

        public DisableDependencyWatcher(DependenciesWatcher watcher, ConflictDependencyInfo info) : base(watcher, info) { }


        protected override void OnRequire(PluginInfo plugin) => MainWatcher.logger.Debug($"Detected not disabled conflict mod: {plugin.GetName()}");
        protected override void OnResolve(PluginInfo plugin) => MainWatcher.logger.Debug($"Conflict mod disabled: {plugin.GetName()}");

        protected override DependencyMessageState GetState(PluginInfo plugin)
        {
            if (Watchers.TryGetValue(plugin, out var watcher) && watcher != null && watcher.IsPluginEnabled)
                return DependencyMessageState.Required;
            else
                return DependencyMessageState.Resolved;
        }

        protected override string GetLabel(PluginInfo plugin)
        {
            var name = plugin.GetName();
            return !string.IsNullOrEmpty(name) ? name : Info.Name;
        }
        protected override Action GetAction(PluginInfo plugin) => () =>
        {
            if (Messages.TryGetValue(plugin, out var message))
                message.State = DependencyMessageState.InProgress;

            plugin.SetState(false);
        };
        protected override Func<float> GetProgress(PluginInfo plugin) => () => 0.5f;
    }
}
