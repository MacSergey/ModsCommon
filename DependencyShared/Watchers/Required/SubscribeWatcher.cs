using ColossalFramework.PlatformServices;
using System;

namespace ModsCommon.Utilities
{
    public class SubscribeDependencyWatcher : RequiredDependencyWatcher
    {
        public override bool IsResolved => Watcher != null;
        protected override DependencyMessageState State
        {
            get
            {
                if (Watcher != null)
                    return DependencyMessageState.Resolved;
                else if (IsSubscribing)
                    return DependencyMessageState.InProgress;
                else
                    return DependencyMessageState.Required;
            }
        }
        private bool IsSubscribing { get; set; }

        protected override string Label => Info.Name;
        protected override string RequiredText => IsWorkshopAvailable ? CommonLocalize.Dependency_Subscribe : CommonLocalize.Dependency_Get;
        protected override string ResolvedText => IsWorkshopAvailable ? "Subscribed" : "Installed";
        protected override Action Action => Subscribe;
        protected override Func<float> Progress => GetProgress;

        public SubscribeDependencyWatcher(DependenciesWatcher watcher, RequiredDependencyInfo info) : base(watcher, info) { }

        protected override void OnRequire()
        {
            MainWatcher.logger.Debug($"Detected missing dependency: {PluginName}");
            IsSubscribing = false;
        }
        protected override void OnResolve()
        {
            MainWatcher.logger.Debug($"Dependency found: {PluginName}");
            IsSubscribing = false;
        }

        private void Subscribe()
        {
            if (Info.Id != 0ul)
            {
                if (IsWorkshopAvailable)
                {
                    IsSubscribing = true;
                    MainWatcher.logger.Debug($"Subscribe missing dependency: {PluginName}");
                    Message.State = DependencyMessageState.InProgress;
                    PlatformService.workshop.Subscribe(new PublishedFileId(Info.Id));
                }
                else
                    Info.Id.OpenWorkshop();
            }
        }
        private float GetProgress() => PlatformService.workshop.GetSubscribedItemProgress(new PublishedFileId(Info.Id));
    }
}
