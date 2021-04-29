using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;
using ICities;
using System;
using System.Linq;
using System.Reflection;
using static ColossalFramework.Plugins.PluginManager;

namespace ModsCommon.Utilities
{
    public static class PluginUtilities
    {
        public static PluginInfo GetPlugin(IPluginSearcher searcher)
        {
            foreach (var plugin in PluginManager.instance.GetPluginsInfo())
            {
                if (searcher.IsMatch(plugin))
                    return plugin;
            }

            return null;
        }
        public static IPluginSearcher GetSearcher(string name, params ulong[] ids)
        {
            var idSearcher = ids.Length <= 1 ? (IPluginSearcher)new IdSearcher(ids[0]) : new AnySearcher(ids.Select(id => new IdSearcher(id)).ToArray());
            var workshopSearcher = new AllSearcher(idSearcher, PathSearcher.Workshop);
            var localSearcher = new AllSearcher(new UserModNameSearcher(name), PathSearcher.Local);

            return new AnySearcher(workshopSearcher, localSearcher);
        }
    }

    public interface IPluginSearcher
    {
        public bool IsMatch(PluginInfo plugin);
    }
    public class IdSearcher : IPluginSearcher
    {
        public ulong Id { get; }
        public IdSearcher(ulong id)
        {
            Id = id;
        }

        public bool IsMatch(PluginInfo plugin) => plugin.publishedFileID.AsUInt64 == Id;
    }
    public class PathSearcher : IPluginSearcher
    {
        public static PathSearcher Local { get; } = new PathSearcher(Option.Local);
        public static PathSearcher Workshop { get; } = new PathSearcher(Option.Workshop);

        public Option Options {get;}
        private PathSearcher(Option options)
        {
            Options = options;
        }

        public bool IsMatch(PluginInfo plugin) => Options == Option.Workshop ^ plugin.modPath.StartsWith(DataLocation.modsPath);
        

        public enum Option
        {
            Local = 1,
            Workshop = 2,
        }
    }
    public abstract class BaseSearcher : IPluginSearcher
    {
        public string ToSearch { get; }
        public Option Options { get; }

        public BaseSearcher(string toSearch, Option options)
        {
            Options = options;
            ToSearch = ApplyOptions(toSearch);
        }
        private string ApplyOptions(string name)
        {
            if (Options.IsSet(Option.CaseInsensetive))
                name = name.ToLower();

            if (Options.IsSet(Option.IgnoreWhiteSpace))
                name.Replace(" ", "");

            return name;
        }
        public virtual bool IsMatch(PluginInfo plugin)
        {
            var toMatch = ApplyOptions(GetMatch(plugin));

            if (toMatch == ToSearch)
                return true;

            if (Options.IsSet(Option.Contains) && toMatch.Contains(ToSearch))
                return true;

            if (Options.IsSet(Option.StartsWidth) && toMatch.StartsWith(ToSearch))
                return true;

            return false;
        }
        protected abstract string GetMatch(PluginInfo plugin);


        [Flags]
        public enum Option
        {
            None = 0,
            Contains = 1 << 0,
            StartsWidth = 1 << 1,
            CaseInsensetive = 1 << 3,
            IgnoreWhiteSpace = 1 << 4,

            AllModes = Contains | StartsWidth,
            AllOptions = CaseInsensetive | IgnoreWhiteSpace,

            DefaultSearch = Contains | AllOptions,
        }
    }
    public abstract class BaseUserModSearcher : BaseSearcher
    {
        public BaseUserModSearcher(string toSearch, Option options) : base(toSearch, options) { }
        public override bool IsMatch(PluginInfo plugin)
        {
            if (plugin.userModInstance is not IUserMod)
                return false;

            return base.IsMatch(plugin);
        }
        protected sealed override string GetMatch(PluginInfo plugin) => GetMatch(plugin.userModInstance as IUserMod);
        protected abstract string GetMatch(IUserMod mod);

    }
    public class UserModNameSearcher : BaseUserModSearcher
    {
        public UserModNameSearcher(string toSearch, Option options = Option.DefaultSearch) : base(toSearch, options) { }

        protected override string GetMatch(IUserMod mod) => mod.Name;
    }
    public class UserModTypeSearcher : BaseUserModSearcher
    {
        public UserModTypeSearcher(string toSearch, Option options = Option.DefaultSearch) : base(toSearch, options) { }

        protected override string GetMatch(IUserMod mod) => mod.GetType().Name;
    }
    public class UserModNamespaceSearcher : BaseUserModSearcher
    {
        public UserModNamespaceSearcher(string toSearch, Option options = Option.DefaultSearch) : base(toSearch, options) { }

        protected override string GetMatch(IUserMod mod) => mod.GetType().Namespace.Split('.').FirstOrDefault();
    }
    public class UserModAssemblySearcher : BaseUserModSearcher
    {
        public UserModAssemblySearcher(string toSearch, Option options = Option.DefaultSearch) : base(toSearch, options) { }

        protected override string GetMatch(IUserMod mod) => mod.GetType().Assembly.GetName().Name;
    }
    public class VersionSearcher : IPluginSearcher
    {
        public delegate bool VersionPredicat(Version toMatch, Version toSearch);
        public Version Version { get; }
        public VersionPredicat Predicat { get; }

        public VersionSearcher(Version version, VersionPredicat predicat)
        {
            Version = version;
            Predicat = predicat;
        }

        public bool IsMatch(PluginInfo plugin)
        {
            if (plugin.userModInstance is not IUserMod userModInstance)
                return false;

            var userModVersion = userModInstance.GetType().Assembly.GetName().Version;
            return Predicat(userModVersion, Version);
        }
    }
    public class PluginNameSearcher : BaseSearcher
    {
        public PluginNameSearcher(string toSearch, Option options) : base(toSearch, options) { }
        protected override string GetMatch(PluginInfo plugin) => plugin.name;
    }

    public abstract class BaseCombineSearcher : IPluginSearcher
    {
        protected IPluginSearcher[] Searchers { get; }

        public BaseCombineSearcher(params IPluginSearcher[] searchers)
        {
            Searchers = searchers;
        }
        public abstract bool IsMatch(PluginInfo plugin);
    }
    public class AnySearcher : BaseCombineSearcher
    {
        public AnySearcher(params IPluginSearcher[] searchers) : base(searchers) { }

        public override bool IsMatch(PluginInfo plugin) => Searchers.Any(s => s.IsMatch(plugin));
    }
    public class AllSearcher : BaseCombineSearcher
    {
        public AllSearcher(params IPluginSearcher[] searchers) : base(searchers) { }

        public override bool IsMatch(PluginInfo plugin) => Searchers.All(s => s.IsMatch(plugin));
    }

    public class PlaginStateWatcher
    {
        public event Action<PluginInfo, bool> StateChanged;

        public PluginInfo Plugin { get; }
        public bool IsEnabled { get; private set; }

        public PlaginStateWatcher(PluginInfo plugin)
        {
            Plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
            IsEnabled = Plugin.isEnabled;

            PluginManager.instance.eventPluginsStateChanged += PluginsStateChanged;
        }

        private void PluginsStateChanged()
        {
            if (Plugin.isEnabled != IsEnabled)
            {
                IsEnabled = Plugin.isEnabled;
                StateChanged?.Invoke(Plugin, IsEnabled);
            }
        }
    }
}
