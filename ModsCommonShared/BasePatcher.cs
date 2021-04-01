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
        private static PrefixAdder PrefixAdder { get; } = new PrefixAdder();
        private static PostfixAdder PostfixAdder { get; } = new PostfixAdder();
        private static TranspilerAdder TranspilerAdder { get; } = new TranspilerAdder();

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

        protected bool AddPrefix(Type patchType, string patchMethod, Type type, string method, Type[] parameters = null) => PrefixAdder.AddPatch(patchType, patchMethod, type, method, parameters);
        protected bool AddPostfix(Type patchType, string patchMethod, Type type, string method, Type[] parameters = null) => PostfixAdder.AddPatch(patchType, patchMethod, type, method, parameters);
        protected bool AddTranspiler(Type patchType, string patchMethod, Type type, string method, Type[] parameters = null) => TranspilerAdder.AddPatch(patchType, patchMethod, type, method, parameters);
    }
    public abstract class PatchAdder
    {
        protected abstract string Name { get; }
        public bool AddPatch(Type patchType, string patchMethod, Type type, string method, Type[] parameters = null)
        {
            var patchName = $"{patchType.FullName}.{patchMethod}()";
            var originalName = $"{type.FullName}.{method}()";
            try
            {
                BaseMod.Logger.Debug($"Start add {Name} {patchName} to {originalName}");

                if (AccessTools.Method(type, method, parameters) is not MethodInfo original)
                    throw new PatchExeption("Can't find original method");
                if (AccessTools.Method(patchType, patchMethod) is not MethodInfo patch)
                    throw new PatchExeption("Can't find patch method");

                AddPatch(original, new HarmonyMethod(patch));

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
        protected abstract void AddPatch(MethodBase original, HarmonyMethod method);

        private class PatchExeption : Exception
        {
            public PatchExeption(string message) : base(message) { }
        }
    }
    public class PrefixAdder : PatchAdder
    {
        protected override string Name => "PREFIX";
        protected override void AddPatch(MethodBase original, HarmonyMethod method) => (BasePatcher.Harmony as Harmony).Patch(original, prefix: method);
    }
    public class PostfixAdder : PatchAdder
    {
        protected override string Name => "POSTFIX";
        protected override void AddPatch(MethodBase original, HarmonyMethod method) => (BasePatcher.Harmony as Harmony).Patch(original, postfix: method);
    }
    public class TranspilerAdder : PatchAdder
    {
        protected override string Name => "TRANSPILER";
        protected override void AddPatch(MethodBase original, HarmonyMethod method) => (BasePatcher.Harmony as Harmony).Patch(original, transpiler: method);
    }
}
