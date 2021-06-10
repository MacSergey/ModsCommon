using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using ICities;
using static ColossalFramework.Plugins.PluginManager;

namespace ModsCommon.Utilities
{
    public class HarmonyReport
    {
        private Info[] PatchInfos { get; }

        public static HarmonyReport Get() => new HarmonyReport();
        private HarmonyReport()
        {
            var patchedMethods = Harmony.GetAllPatchedMethods().ToArray();
            PatchInfos = patchedMethods.Select(p => new Info(p)).ToArray();
        }

        public bool IsPossibleConflicts(PluginInfo checkPlugin) => PatchInfos.Any(i => !i.Exclusive(checkPlugin));
        public Conflict[] GetConflicts(PluginInfo checkPlugin)
        {
                var conflicts = new Dictionary<PluginInfo, Conflict>();

                foreach (var patchInfo in PatchInfos)
                {
                    if (patchInfo.Exclusive(checkPlugin))
                        continue;

                    foreach (var plugin in patchInfo.Plugins)
                    {
                        if (plugin == checkPlugin)
                            continue;

                        if (!conflicts.TryGetValue(plugin, out var conflict))
                        {
                            conflict = new Conflict(plugin);
                            conflicts[plugin] = conflict;
                        }

                        conflict.Methods.Add(patchInfo.Method);
                    }
                }

                return conflicts.Values.ToArray();
        }

        public string PrintConflicts() => PrintConflicts(Assembly.GetExecutingAssembly());
        public string PrintConflicts(Assembly assembly)
        {
            if (new AssemblySearcher(assembly).GetPlugin() is PluginInfo plugin)
                return PrintConflicts(plugin);
            else
                return "Plugin not found";
        }
        public string PrintConflicts(PluginInfo checkPlugin)
        {
            var conflictText = GetConflicts(checkPlugin).Select(c => PrintConflict(c)).ToArray();

            var text = "\n====================Start Harmony report====================\n";
            text += string.Join("\n--------------------------------------------------\n", conflictText);
            text += "\n====================End Harmony report====================\n";

            return text;
        }
        public string Print()
        {
            var infosText = PatchInfos.Select(i => i.Print()).ToArray();

            var text = "\n====================Start Harmony report====================\n";
            text += string.Join("\n--------------------------------------------------\n", infosText);
            text += "\n====================End Harmony report====================\n";

            return text;
        }
        private string PrintConflict(Conflict conflict)
        {
            var text = $"Possible conflict with {(conflict.Plugin.userModInstance as IUserMod)?.Name ?? "Unknown"} by methods:";
            foreach (var method in conflict.Methods)
                text += $"\n--- {method.DeclaringType.FullName}.{method.Name}";
            return text;
        }

        private class Info
        {
            public MethodBase Method { get; private set; }
            private Dictionary<PluginInfo, List<MethodBase>> PluginsDic { get; } = new Dictionary<PluginInfo, List<MethodBase>>();

            public IEnumerable<PluginInfo> Plugins => PluginsDic.Keys;
            public bool IsSingle => PluginsDic.Count <= 1;

            public Info(MethodBase method)
            {
                Method = method;

                var info = Harmony.GetPatchInfo(Method);

                GetPlugins(info.Prefixes);
                GetPlugins(info.Postfixes);
                GetPlugins(info.Transpilers);
                GetPlugins(info.Finalizers);
            }

            private void GetPlugins(IEnumerable<Patch> patches)
            {
                foreach (var patch in patches)
                {
                    var assembly = patch.PatchMethod.DeclaringType.Assembly;

                    if (new AssemblySearcher(assembly).GetPlugin() is PluginInfo plugin)
                    {
                        if (!PluginsDic.TryGetValue(plugin, out var methods))
                        {
                            methods = new List<MethodBase>();
                            PluginsDic[plugin] = methods;
                        }

                        methods.Add(patch.PatchMethod);
                    }
                }
            }

            public bool Contains(PluginInfo plugin) => PluginsDic.ContainsKey(plugin);
            public bool Exclusive(PluginInfo plugin) => !Contains(plugin) || IsSingle;

            public string Print()
            {
                var text = $"{Method.DeclaringType.FullName}.{Method.Name} patched by mods:";
                foreach(var plugin in Plugins)
                    text += $"\n--- {(plugin.userModInstance as IUserMod)?.Name ?? "Unknown"}";

                return text;
            }
        }
        public class Conflict
        {
            public PluginInfo Plugin { get; }
            public HashSet<MethodBase> Methods { get; } = new HashSet<MethodBase>();

            public Conflict(PluginInfo plugin)
            {
                Plugin = plugin;
            }
        }
    }
}
