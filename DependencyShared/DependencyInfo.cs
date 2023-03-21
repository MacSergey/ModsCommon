using System.Collections.Generic;

namespace ModsCommon.Utilities
{
    public abstract class BaseDependencyInfo
    {
        public DependencyState State { get; }
        public PluginSearcher Searcher { get; }

        public BaseDependencyInfo(DependencyState state, PluginSearcher searcher, string name = null, ulong id = 0ul)
        {
            State = state;
            Searcher = searcher;
        }
        public abstract IEnumerable<BaseDependencyWatcher> GetWatcher(DependenciesWatcher mainWatcher);
    }
    public class RequiredDependencyInfo : BaseDependencyInfo
    {
        public string Name { get; }
        public ulong Id { get; }

        public RequiredDependencyInfo(DependencyState state, PluginSearcher searcher, string name, ulong id) : base(state, searcher)
        {
            Name = name;
            Id = id;
        }
        public override IEnumerable<BaseDependencyWatcher> GetWatcher(DependenciesWatcher mainWatcher)
        {
            switch (State)
            {
                case DependencyState.Subscribe:
                    yield return new SubscribeDependencyWatcher(mainWatcher, this);
                    break;
                case DependencyState.Enable:
                    yield return new EnableDependencyWatcher(mainWatcher, this);
                    break;
                default:
                    yield break;
            }    
        }
    }
    public class ConflictDependencyInfo : BaseDependencyInfo
    {
        public string Name { get; }

        public ConflictDependencyInfo(DependencyState state, PluginSearcher searcher, string name) : base(state, searcher) 
        {
            Name = name;
        }
        public override IEnumerable<BaseDependencyWatcher> GetWatcher(DependenciesWatcher mainWatcher)
        {
            switch (State)
            {
                case DependencyState.Unsubscribe:
                    yield return new UnsubscribeDependencyWatcher(mainWatcher, this);
                    break;
                case DependencyState.Disable:
                    yield return new DisableDependencyWatcher(mainWatcher, this);
                    break;
                default:
                    yield break;
            }
        }
    }

    public enum DependencyState
    {
        Subscribe,
        Enable,
        Unsubscribe,
        Disable,
    }
}
