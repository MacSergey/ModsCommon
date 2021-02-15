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
        private BaseMod<ModType> Mod { get; }
        protected object Harmony => new Harmony(BaseMod<ModType>.Instance.Id);
        public bool Success { get; private set; }

        public Patcher(BaseMod<ModType> mod)
        {
            Mod = mod;
        }

        public void Patch()
        {
            Mod.ModLogger.Debug("Patch");
            HarmonyHelper.DoOnHarmonyReady(() => Begin());
        }
        public void Unpatch()
        {
            Mod.ModLogger.Debug($"Unpatch all");
            ((Harmony)Harmony).UnpatchAll();
            Mod.ModLogger.Debug($"Unpatched");
        }

        private void Begin()
        {
            Mod.ModLogger.Debug("Start patching");

            try { Success = PatchProcess(); }
            catch { Success = false; }

            BaseMod<ModType>.Instance.CheckLoadedError();
            Mod.ModLogger.Debug(Success ? "Patch success" : "Patch Filed");
        }
        protected abstract bool PatchProcess();

        protected bool AddPrefix(MethodInfo prefix, Type type, string method, Func<Type, string, MethodInfo> originalGetter = null)
            => AddPatch((original) => ((Harmony)Harmony).Patch(original, prefix: new HarmonyMethod(prefix)), type, method, originalGetter);

        protected bool AddPostfix(MethodInfo postfix, Type type, string method, Func<Type, string, MethodInfo> originalGetter = null)
            => AddPatch((original) => ((Harmony)Harmony).Patch(original, postfix: new HarmonyMethod(postfix)), type, method, originalGetter);

        protected bool AddTranspiler(MethodInfo transpiler, Type type, string method, Func<Type, string, MethodInfo> originalGetter = null)
            => AddPatch((original) => ((Harmony)Harmony).Patch(original, transpiler: new HarmonyMethod(transpiler)), type, method, originalGetter);

        protected bool AddPatch(Action<MethodInfo> patch, Type type, string method, Func<Type, string, MethodInfo> originalGetter)
        {
            var methodName = $"{type.Name}.{method}()";
            try
            {
                Mod.ModLogger.Debug($"Patch {methodName}");

                var original = originalGetter?.Invoke(type, method) ?? AccessTools.Method(type, method);
                patch(original);

                Mod.ModLogger.Debug($"Patched {methodName}");
                return true;
            }
            catch (Exception error)
            {
                Mod.ModLogger.Error($"Failed Patch {methodName}", error);
                return false;
            }
        }
    }
}
