using CitiesHarmony.API;
using HarmonyLib;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ModsCommon
{
    public abstract class Patcher<ModType>
        where ModType : BaseMod<ModType>
    {
        protected Harmony Harmony { get; } = new Harmony(BaseMod<ModType>.Instance.Id);
        public bool Success { get; private set; }

        public void Patch()
        {
            BaseMod<ModType>.Logger.Debug("Patch");
            HarmonyHelper.DoOnHarmonyReady(() => Begin());
        }
        public void Unpatch()
        {
            BaseMod<ModType>.Logger.Debug($"Unpatch all");
            Harmony.UnpatchAll();
            BaseMod<ModType>.Logger.Debug($"Unpatched");
        }

        private void Begin()
        {
            BaseMod<ModType>.Logger.Debug("Start patching");

            try { Success = PatchProcess(); }
            catch { Success = false; }

            BaseMod<ModType>.Instance.LoadedError();
            BaseMod<ModType>.Logger.Debug(Success ? "Patch success" : "Patch Filed");
        }
        protected abstract bool PatchProcess();

        protected bool AddPrefix(MethodInfo prefix, Type type, string method, Func<Type, string, MethodInfo> originalGetter = null)
            => AddPatch((original) => Harmony.Patch(original, prefix: new HarmonyMethod(prefix)), type, method, originalGetter);

        protected bool AddPostfix(MethodInfo postfix, Type type, string method, Func<Type, string, MethodInfo> originalGetter = null)
            => AddPatch((original) => Harmony.Patch(original, postfix: new HarmonyMethod(postfix)), type, method, originalGetter);

        protected bool AddTranspiler(MethodInfo transpiler, Type type, string method, Func<Type, string, MethodInfo> originalGetter = null)
            => AddPatch((original) => Harmony.Patch(original, transpiler: new HarmonyMethod(transpiler)), type, method, originalGetter);

        protected bool AddPatch(Action<MethodInfo> patch, Type type, string method, Func<Type, string, MethodInfo> originalGetter)
        {
            var methodName = $"{type.Name}.{method}()";
            try
            {
                BaseMod<ModType>.Logger.Debug($"Patch {methodName}");

                var original = originalGetter?.Invoke(type, method) ?? AccessTools.Method(type, method);
                patch(original);

                BaseMod<ModType>.Logger.Debug($"Patched {methodName}");
                return true;
            }
            catch (Exception error)
            {
                BaseMod<ModType>.Logger.Error($"Failed Patch {methodName}", error);
                return false;
            }
        }
    }
}
