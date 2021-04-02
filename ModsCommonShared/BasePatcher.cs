using CitiesHarmony.API;
using HarmonyLib;
using System;
using System.Reflection;

namespace ModsCommon
{
    public abstract class BasePatcher
    {
        private BaseMod Mod { get; }
        public static object Harmony => new Harmony(BaseMod.Id);
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

        protected bool AddPrefix(Type patchType, string patchMethod, Type type, string method, Type[] parameters = null) => AddPatch(PatcherType.Prefix, patchType, patchMethod, type, method, parameters);
        protected bool AddPostfix(Type patchType, string patchMethod, Type type, string method, Type[] parameters = null) => AddPatch(PatcherType.Postfix, patchType, patchMethod, type, method, parameters);
        protected bool AddTranspiler(Type patchType, string patchMethod, Type type, string method, Type[] parameters = null) => AddPatch(PatcherType.Transpiler, patchType, patchMethod, type, method, parameters);

        private bool AddPatch(PatcherType patcher, Type patchType, string patchMethod, Type type, string method, Type[] parameters = null)
        {
            try
            {
                BaseMod.Logger.Debug($"Start add [{patcher.ToString().ToUpper()}] [{patchType.FullName}.{patchMethod}] to [{type.FullName}.{method}]");

                if (AccessTools.Method(type, method, parameters) is not MethodInfo original)
                    throw new PatchExeption("Can't find original method");
                if (AccessTools.Method(patchType, patchMethod) is not MethodInfo patch)
                    throw new PatchExeption("Can't find patch method");

                var harmony = Harmony as Harmony;
                var harmonyMethod = new HarmonyMethod(patch);
                switch (patcher)
                {
                    case PatcherType.Prefix: harmony.Patch(original, prefix: harmonyMethod); break;
                    case PatcherType.Postfix: harmony.Patch(original, postfix: harmonyMethod); break;
                    case PatcherType.Transpiler: harmony.Patch(original, transpiler: harmonyMethod); break;
                }

                BaseMod.Logger.Debug("Success patched!");
                return true;
            }
            catch (PatchExeption error)
            {
                BaseMod.Logger.Error($"Failed patch: {error.Message}");
                return false;
            }
            catch (Exception error)
            {
                BaseMod.Logger.Error($"Failed patch:", error);
                return false;
            }
        }
        private enum PatcherType
        {
            Prefix,
            Postfix,
            Transpiler
        }
        private class PatchExeption : Exception
        {
            public PatchExeption(string message) : base(message) { }
        }
    }
}
