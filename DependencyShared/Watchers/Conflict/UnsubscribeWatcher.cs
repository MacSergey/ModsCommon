using ColossalFramework.PlatformServices;
using System;
using System.IO;
using System.Linq;
using static ColossalFramework.Plugins.PluginManager;

namespace ModsCommon.Utilities
{
    public class UnsubscribeDependencyWatcher : ConflictDependencyWatcher
    {
        public override bool IsResolved => Watchers.Values.Count(watcher => watcher != null) == 0;

        public UnsubscribeDependencyWatcher(DependenciesWatcher watcher, ConflictDependencyInfo info) : base(watcher, info) { }


        protected override void OnRequire(PluginInfo plugin) => MainWatcher.logger.Debug($"Detected conflict mod: {plugin.GetName()}");
        protected override void OnResolve(PluginInfo plugin) => MainWatcher.logger.Debug($"Conflict mod no more exist: {plugin.GetName()}");

        private void Unsubscribe(PluginInfo plugin)
        {
            if (plugin.publishedFileID == PublishedFileId.invalid)
            {
                MainWatcher.logger.Debug($"Unsubscribe conflict mod: {plugin.GetName()}");

                if (Messages.TryGetValue(plugin, out var message))
                    message.State = DependencyMessageState.InProgress;

                if (Directory.Exists(plugin.modPath))
                    Directory.Delete(plugin.modPath, true);
            }
            else if (IsWorkshopAvailable)
            {
                MainWatcher.logger.Debug($"Remove conflict mod: {plugin.GetName()}");

                if (Messages.TryGetValue(plugin, out var message))
                    message.State = DependencyMessageState.InProgress;

                PlatformService.workshop.Unsubscribe(plugin.publishedFileID);
            }
        }

        protected override DependencyMessageState GetState(PluginInfo plugin)
        {
            if (Watchers.TryGetValue(plugin, out var watcher) && watcher != null)
                return DependencyMessageState.Required;
            else
                return DependencyMessageState.Resolved;
        }

        protected override string GetLabel(PluginInfo plugin)
        {
            var name = plugin.GetName();
            return !string.IsNullOrEmpty(name) ? name : Info.Name;
        }
        protected override Action GetAction(PluginInfo plugin) => () => Unsubscribe(plugin);
        protected override Func<float> GetProgress(PluginInfo plugin) => () => DateTime.Now.Millisecond * 0.001f /*0.5f*/;

        protected override string GetRequiredText(PluginInfo plugin)
        {
            if (!IsWorkshopAvailable)
                return CommonLocalize.Dependency_Remove;
            else if (plugin == null || plugin.publishedFileID == PublishedFileId.invalid)
                return CommonLocalize.Dependency_Remove;
            else
                return CommonLocalize.Dependency_Unsubscribe;
        }

        protected override string GetResolvedText(PluginInfo plugin)
        {
            if (!IsWorkshopAvailable)
                return CommonLocalize.Dependency_Removed;
            else if (plugin == null || plugin.publishedFileID == PublishedFileId.invalid)
                return CommonLocalize.Dependency_Removed;
            else
                return CommonLocalize.Dependency_Unsubscribed;
        }
    }
}
