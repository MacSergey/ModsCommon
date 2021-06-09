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
        public PluginInfo Plugin { get; }
        private Info[] PatchInfos { get; }

        public bool IsPossibleConflicts => PatchInfos.Any(i => !i.Exclusive(Plugin));

        private HarmonyReport(PluginInfo plugin, Info[] patchInfos)
        {
            Plugin = plugin;
            PatchInfos = patchInfos;
        }
        public Conflict[] Conflicts
        {
            get
            {
                var conflicts = new Dictionary<PluginInfo, Conflict>();

                foreach (var patchInfo in PatchInfos)
                {
                    if (patchInfo.Exclusive(Plugin))
                        continue;

                    foreach (var plugin in patchInfo.Plugins)
                    {
                        if (plugin == Plugin)
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
        }

        public string Print()
        {
            var text = "\n====================Start Harmony report====================\n";
            foreach (var conflict in Conflicts)
            {
                text += $"Possible conflict with {(conflict.Plugin.userModInstance as IUserMod)?.Name ?? "Unknown"} by methods:";
                foreach (var method in conflict.Methods)
                    text += $"\n{method.DeclaringType.FullName}.{method.Name}";
                text += "--------------------------------------------------";
            }
            text += "====================End Harmony report====================\n";

            return text;
        }

        public static HarmonyReport Get() => Get(Assembly.GetExecutingAssembly());
        public static HarmonyReport Get(Assembly assembly)
        {
            if (new AssemblySearcher(assembly).GetPlugin() is not PluginInfo plugin)
                return null;

            var patchedMethods = Harmony.GetAllPatchedMethods().ToArray();
            var patchInfos = patchedMethods.Select(p => new Info(p)).ToArray();

            return new HarmonyReport(plugin, patchInfos);
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
