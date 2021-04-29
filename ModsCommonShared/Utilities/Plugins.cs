using ColossalFramework;
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
        [Flags]
        public enum SearchOption
        {
            None = 0,
            Contains = 1 << 0,
            StartsWidth = 1 << 1,
            CaseInsensetive = 1 << 3,
            IgnoreWhiteSpace = 1 << 4,
            UserModName = 1 << 5,
            UserModType = 1 << 6,
            RootNameSpace = 1 << 7,
            PluginName = 1 << 8,
            AssemblyName = 1 << 9,

            AllModes = Contains | StartsWidth,
            AllOptions = CaseInsensetive | IgnoreWhiteSpace,
            AllTargets = UserModName | UserModType | RootNameSpace | PluginName | AssemblyName,

            DefaultSearch = Contains | AllOptions | UserModName,
        }

        public static PluginInfo GetPlugin(string searchName, ulong searchId, SearchOption searchOptions = SearchOption.DefaultSearch) => GetPlugin(searchName, new[] { searchId }, searchOptions);

        public static PluginInfo GetPlugin(string searchName, ulong[] searchIds = null, SearchOption searchOptions = SearchOption.DefaultSearch)
        {
            foreach (var plugin in PluginManager.instance.GetPluginsInfo())
            {
                if (plugin == null)
                    continue;

                if (Matches(plugin, searchIds))
                    return plugin;

                if (plugin.userModInstance is not IUserMod userModInstance)
                    continue;

                if (searchOptions.IsFlagSet(SearchOption.UserModName) && IsEqual(userModInstance.Name, searchName, searchOptions))
                    return plugin;

                var userModType = userModInstance.GetType();
                if (searchOptions.IsFlagSet(SearchOption.UserModType) && IsEqual(userModType.Name, searchName, searchOptions))
                    return plugin;

                var rootNameSpace = userModType.Namespace.Split('.').FirstOrDefault();
                if (searchOptions.IsFlagSet(SearchOption.RootNameSpace) && IsEqual(rootNameSpace, searchName, searchOptions))
                    return plugin;

                if (searchOptions.IsFlagSet(SearchOption.PluginName) && IsEqual(plugin.name, searchName, searchOptions))
                    return plugin;

                var assemblyName = plugin.userModInstance?.GetType().Assembly.GetName().Name;
                if (searchOptions.IsFlagSet(SearchOption.AssemblyName) && IsEqual(assemblyName, searchName, searchOptions))
                    return plugin;
            }

            return null;
        }

        private static bool IsEqual(string name1, string name2, SearchOption searchOptions = SearchOption.DefaultSearch)
        {
            if (string.IsNullOrEmpty(name1))
                return false;

            if (searchOptions.IsFlagSet(SearchOption.CaseInsensetive))
            {
                name1 = name1.ToLower();
                name2 = name2.ToLower();
            }

            if (searchOptions.IsFlagSet(SearchOption.IgnoreWhiteSpace))
            {
                name1 = name1.Replace(" ", "");
                name2 = name2.Replace(" ", "");
            }

            if (name1 == name2)
                return true;

            if (searchOptions.IsFlagSet(SearchOption.Contains) && name1.Contains(name2))
                return true;

            if (searchOptions.IsFlagSet(SearchOption.StartsWidth) && name1.StartsWith(name2))
                return true;

            return false;
        }

        private static bool Matches(PluginInfo plugin, ulong[] searchIds)
        {
            if (searchIds == null)
                return false;

            foreach (var id in searchIds)
            {
                if (id != 0 && id == plugin.publishedFileID.AsUInt64)
                    return true;
            }
            return false;
        }
    }
}
