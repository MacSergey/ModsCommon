using CitiesHarmony.API;
using HarmonyLib;
using System;
using System.Reflection;

namespace ModsCommon
{
    public abstract class BasePatcher
    {
        private BaseMod Mod { get; }
        protected object Harmony => new Harmony(BaseMod.Id);
        public bool Success { get; private set; }

        public BasePatcher(BaseMod mod)
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
            var harmony = Harmony as Harmony;
            harmony.UnpatchAll(harmony.Id);
            Mod.ModLogger.Debug($"Unpatched");
        }

        private void Begin()
        {
            Mod.ModLogger.Debug("Start patching");

            try { Success = PatchProcess(); }
            catch { Success = false; }

            BaseMod.Instance.CheckLoadedError();
            Mod.ModLogger.Debug(Success ? "Patch success" : "Patch Filed");
        }
        protected abstract bool PatchProcess();

        protected bool AddPrefix(MethodInfo prefix, Type type, string method, Type[] parameters = null)
            => AddPatch((original) => ((Harmony)Harmony).Patch(original, prefix: new HarmonyMethod(prefix)), type, method, parameters);

        protected bool AddPostfix(MethodInfo postfix, Type type, string method, Type[] parameters = null)
            => AddPatch((original) => ((Harmony)Harmony).Patch(original, postfix: new HarmonyMethod(postfix)), type, method, parameters);

        protected bool AddTranspiler(MethodInfo transpiler, Type type, string method, Type[] parameters = null)
            => AddPatch((original) => ((Harmony)Harmony).Patch(original, transpiler: new HarmonyMethod(transpiler)), type, method, parameters);

        protected bool AddPatch(Action<MethodInfo> patch, Type type, string method, Type[] parameters = null)
        {
            var methodName = $"{type.Name}.{method}()";
            try
            {
                Mod.ModLogger.Debug($"Patch {methodName}");

                var original = AccessTools.Method(type, method, parameters);
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
