using System;
using System.Collections.Generic;
using System.Text;

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
    public class NeedDependencyInfo : BaseDependencyInfo
    {
        public string Name { get; }
        public ulong Id { get; }

        public NeedDependencyInfo(DependencyState state, PluginSearcher searcher, string name, ulong id) : base(state, searcher)
        {
            Name = name;
            Id = id;
        }
        public override IEnumerable<BaseDependencyWatcher> GetWatcher(DependenciesWatcher mainWatcher)
        {
            if (State == DependencyState.Subscribe || State == DependencyState.Enable)
                yield return new SubscribeDependencyWatcher(mainWatcher, this);
            if (State == DependencyState.Enable)
                yield return new EnableDependencyWatcher(mainWatcher, this);
        }
    }
    public class ConflictDependencyInfo : BaseDependencyInfo
    {
        public ConflictDependencyInfo(DependencyState state, PluginSearcher searcher) : base(state, searcher) { }
        public override IEnumerable<BaseDependencyWatcher> GetWatcher(DependenciesWatcher mainWatcher)
        {
            if (State == DependencyState.Unsubscribe)
                yield return new UnsubscribeDependencyWatcher(mainWatcher, this);
            if (State == DependencyState.Disable)
                yield return new DisableDependencyWatcher(mainWatcher, this);
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
