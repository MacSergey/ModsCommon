using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static ColossalFramework.Plugins.PluginManager;

namespace ModsCommon.Utilities
{
    public static class PluginUtilities
    {
        public static PluginInfo GetPlugin(PluginSearcher searcher) => GetPlugins(searcher).FirstOrDefault();
        public static IEnumerable<PluginInfo> GetPlugins(PluginSearcher searcher)
        {
            var plugins = PluginManager.instance.GetPluginsInfo().ToArray();
            foreach (var plugin in plugins)
            {
                if (searcher.IsMatch(plugin))
                    yield return plugin;
            }
        }

        public static PluginSearcher GetSearcher(string name, params ulong[] ids)
        {
            var localSearcher = new UserModNameSearcher(name) & PathSearcher.Local;

            if (ids.Length == 0)
                return localSearcher;
            else
            {
                var idSearcher = ids.Length <= 1 ? (PluginSearcher)new IdSearcher(ids[0]) : new AnySearcher(ids.Select(id => new IdSearcher(id)).ToArray());
                var workshopSearcher = idSearcher & PathSearcher.Workshop;
                return workshopSearcher | localSearcher;
            }
        }

        public static void SetState(this PluginInfo plugin, bool enable)
        {
            if (plugin.isEnabled == enable)
                return;

            plugin.isEnabled = enable;

            if (UIView.library.Get<ContentManagerPanel>("ContentManagerPanel") is ContentManagerPanel managerPanel)
            {
                var categoriesContainerField = typeof(ContentManagerPanel).GetField("m_CategoriesContainer", BindingFlags.Instance | BindingFlags.NonPublic);
                var categoriesContainer = categoriesContainerField.GetValue(managerPanel) as UITabContainer;
                var modCategory = categoriesContainer.Find("Mods");
                if (categoriesContainer.components[categoriesContainer.selectedIndex] == modCategory)
                {
                    var categoryContentPanel = modCategory.Find("Content").GetComponent<CategoryContentPanel>();
                    var refreshEntriesMethod = typeof(CategoryContentPanel).GetMethod("RefreshEntries", BindingFlags.Instance | BindingFlags.NonPublic);
                    refreshEntriesMethod.Invoke(categoryContentPanel, new object[0]);
                }
            }
        }
    }

    public abstract class PluginSearcher
    {
        public abstract bool IsMatch(PluginInfo plugin);

        public static NotSearcher operator !(PluginSearcher searcher) => new NotSearcher(searcher);
        public static AllSearcher operator &(PluginSearcher first, PluginSearcher second) => new AllSearcher(first, second);
        public static AnySearcher operator |(PluginSearcher first, PluginSearcher second) => new AnySearcher(first, second);
    }

    public class IdSearcher : PluginSearcher
    {
        public ulong Id { get; }
        public IdSearcher(ulong id)
        {
            Id = id;
        }

        public override bool IsMatch(PluginInfo plugin) => plugin.publishedFileID.AsUInt64 == Id;
    }
    public class PathSearcher : PluginSearcher
    {
        public static PathSearcher Local { get; } = new PathSearcher(Option.Local);
        public static PathSearcher Workshop { get; } = new PathSearcher(Option.Workshop);

        public Option Options { get; }
        private PathSearcher(Option options)
        {
            Options = options;
        }

        public override bool IsMatch(PluginInfo plugin) => Options == Option.Workshop ^ plugin.modPath.StartsWith(DataLocation.modsPath);

        public static PathSearcher operator !(PathSearcher searcher) => searcher.Options == Option.Local ? Workshop : Local;

        public enum Option
        {
            Local = 1,
            Workshop = 2,
        }
    }
    public abstract class BaseMatchSearcher : PluginSearcher
    {
        public string ToSearch { get; }
        public Option Options { get; }

        public BaseMatchSearcher(string toSearch, Option options)
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
        public override bool IsMatch(PluginInfo plugin)
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
    public abstract class BaseUserModSearcher : BaseMatchSearcher
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
    public class UserModInstanceSearcher : PluginSearcher
    {
        public IUserMod Instance { get; }
        public UserModInstanceSearcher(IUserMod instance)
        {
            Instance = instance;
        }

        public override bool IsMatch(PluginInfo plugin) => plugin.userModInstance == Instance;
    }
    public class VersionSearcher : PluginSearcher
    {
        public delegate bool VersionPredicat(Version toMatch, Version toSearch);
        public Version Version { get; }
        public VersionPredicat Predicat { get; }

        public VersionSearcher(Version version, VersionPredicat predicat)
        {
            Version = version;
            Predicat = predicat;
        }

        public override bool IsMatch(PluginInfo plugin)
        {
            if (plugin.userModInstance is not IUserMod userModInstance)
                return false;

            var userModVersion = userModInstance.GetType().Assembly.GetName().Version;
            return Predicat(userModVersion, Version);
        }
    }
    public class PluginNameSearcher : BaseMatchSearcher
    {
        public PluginNameSearcher(string toSearch, Option options) : base(toSearch, options) { }
        protected override string GetMatch(PluginInfo plugin) => plugin.name;
    }

    public abstract class BaseCombineSearcher : PluginSearcher
    {
        protected PluginSearcher[] Searchers { get; }

        public BaseCombineSearcher(params PluginSearcher[] searchers)
        {
            Searchers = searchers;
        }

        protected static PluginSearcher[] Concat(PluginSearcher[] first, PluginSearcher[] second)
        {
            var searchers = new PluginSearcher[first.Length + second.Length];
            first.CopyTo(searchers, 0);
            second.CopyTo(searchers, first.Length);
            return searchers;
        }
    }
    public class AnySearcher : BaseCombineSearcher
    {
        public AnySearcher(params PluginSearcher[] searchers) : base(searchers) { }

        public override bool IsMatch(PluginInfo plugin) => Searchers.Any(s => s.IsMatch(plugin));

        public static AnySearcher operator &(AnySearcher first, PluginSearcher second) => new AnySearcher(Concat(first.Searchers, new PluginSearcher[] { second }));
        public static AnySearcher operator &(PluginSearcher first, AnySearcher second) => new AnySearcher(Concat(new PluginSearcher[] { first }, second.Searchers));
        public static AnySearcher operator &(AnySearcher first, AnySearcher second) => new AnySearcher(Concat(first.Searchers, second.Searchers));
    }
    public class AllSearcher : BaseCombineSearcher
    {
        public AllSearcher(params PluginSearcher[] searchers) : base(searchers) { }

        public override bool IsMatch(PluginInfo plugin) => Searchers.All(s => s.IsMatch(plugin));

        public static AllSearcher operator &(AllSearcher first, PluginSearcher second) => new AllSearcher(Concat(first.Searchers, new PluginSearcher[] { second }));
        public static AllSearcher operator &(PluginSearcher first, AllSearcher second) => new AllSearcher(Concat(new PluginSearcher[] { first }, second.Searchers));
        public static AllSearcher operator &(AllSearcher first, AllSearcher second) => new AllSearcher(Concat(first.Searchers, second.Searchers));
    }
    public class NotSearcher : PluginSearcher
    {
        protected PluginSearcher Searcher { get; }

        public NotSearcher(PluginSearcher searcher)
        {
            Searcher = searcher;
        }

        public override bool IsMatch(PluginInfo plugin) => !Searcher.IsMatch(plugin);

        public static PluginSearcher operator !(NotSearcher notSearcher) => notSearcher.Searcher;
    }

    public class PluginStateWatcher
    {
        public event Action<PluginInfo, bool> StateChanged;

        private bool _enable = false;
        public bool Enable
        {
            get => _enable;
            set
            {
                if(value != _enable)
                {
                    _enable = value;

                    if (Enable)
                    {
                        IsPluginEnabled = Plugin.isEnabled;
                        PluginManager.instance.eventPluginsStateChanged += PluginsStateChanged;
                    }
                    else
                        PluginManager.instance.eventPluginsStateChanged -= PluginsStateChanged;
                }
            }
        }

        public PluginInfo Plugin { get; }
        public bool IsPluginEnabled { get; private set; }

        public PluginStateWatcher(PluginInfo plugin, bool enable = true)
        {
            Plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
            Enable = enable;
        }

        private void PluginsStateChanged()
        {
            if (Plugin.isEnabled != IsPluginEnabled)
            {
                IsPluginEnabled = Plugin.isEnabled;
                StateChanged?.Invoke(Plugin, IsPluginEnabled);
            }
        }
    }
}
