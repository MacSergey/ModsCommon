using CitiesHarmony.API;
using ColossalFramework.UI;
using HarmonyLib;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ModsCommon
{
    public abstract partial class BasePatcherMod<TypeMod> : BaseMod<TypeMod>
        where TypeMod : BaseMod<TypeMod>
    {
        #region PROPERTIES

        protected override bool LoadError
        {
            get => base.LoadError || PatchResult == Result.Failed;
            set => base.LoadError = value;
        }
        private Result PatchResult { get; set; }
        public object Harmony => new Harmony(Id);

        #endregion

        #region BASIC

        protected override void Enable()
        {
            PatchResult = Result.None;
            Patch();
        }
        protected override void Disable()
        {
            if (PatchResult == Result.Success)
                Unpatch();
        }
        private void Patch()
        {
            Logger.Debug("Patch");
            HarmonyHelper.DoOnHarmonyReady(() => StartPatch());
        }
        private void Unpatch()
        {
            Logger.Debug($"Unpatch all");
            var harmony = Harmony as Harmony;
            harmony.UnpatchAll(harmony.Id);
            Logger.Debug($"Unpatched");
        }

        private void StartPatch()
        {
            Logger.Debug("Start patching");

            try
            {
                PatchResult = PatchProcess() ? Result.Success : Result.Failed;
                Logger.Debug($"Patch {PatchResult}");
            }
            catch (Exception error)
            {
                PatchResult = Result.Failed;
                Logger.Error($"Patch {PatchResult}", error);
            }

            CheckLoadedError(PatchResult == Result.Failed);
        }
        protected abstract bool PatchProcess();

        protected bool AddPrefix(Type patchType, string patchMethod, Type type, string method, Type[] parameters = null) => AddPatch(PatcherType.Prefix, patchType, patchMethod, type, method, parameters);
        protected bool AddPostfix(Type patchType, string patchMethod, Type type, string method, Type[] parameters = null) => AddPatch(PatcherType.Postfix, patchType, patchMethod, type, method, parameters);
        protected bool AddTranspiler(Type patchType, string patchMethod, Type type, string method, Type[] parameters = null) => AddPatch(PatcherType.Transpiler, patchType, patchMethod, type, method, parameters);

        private bool AddPatch(PatcherType patcher, Type patchType, string patchMethod, Type type, string method, Type[] parameters = null)
        {
            void action()
            {
                Logger.Debug($"Start add [{patcher.ToString().ToUpper()}] [{patchType?.FullName}.{patchMethod}] to [{type?.FullName}.{method}]");

                if (AccessTools.Method(type, method, parameters) is not MethodInfo original)
                    throw new PatchExeption("Can't find original method");
                if (AccessTools.Method(patchType, patchMethod) is not MethodInfo patch)
                    throw new PatchExeption("Can't find patch method");

                AddPatch(patcher, patch, original);

                Logger.Debug($"[{type?.FullName}.{method}] success patched!");
            }

            return AddPatchProcess(action);
        }

        protected bool AddPrefix(MethodInfo patch, Type type, string method, Type[] parameters = null) => AddPatch(PatcherType.Prefix, patch, type, method, parameters);
        protected bool AddPostfix(MethodInfo patch, Type type, string method, Type[] parameters = null) => AddPatch(PatcherType.Postfix, patch, type, method, parameters);
        protected bool AddTranspiler(MethodInfo patch, Type type, string method, Type[] parameters = null) => AddPatch(PatcherType.Transpiler, patch, type, method, parameters);

        private bool AddPatch(PatcherType patcher, MethodInfo patch, Type type, string method, Type[] parameters = null)
        {
            void action()
            {
                Logger.Debug($"Start add [{patcher.ToString().ToUpper()}] [{patch?.DeclaringType.FullName}.{patch?.Name}] to [{type?.FullName}.{method}]");

                if (AccessTools.Method(type, method, parameters) is not MethodInfo original)
                    throw new PatchExeption("Can't find original method");
                if (patch == null)
                    throw new PatchExeption("Can't find patch method");

                AddPatch(patcher, patch, original);

                Logger.Debug($"[{type?.FullName}.{method}] success patched!");
            }

            return AddPatchProcess(action);
        }
        private bool AddPatchProcess(Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (PatchExeption error)
            {
                Logger.Error($"Failed patch: {error.Message}");
                return false;
            }
            catch (Exception error)
            {
                Logger.Error($"Failed patch:", error);
                return false;
            }
        }
        private void AddPatch(PatcherType patcher, MethodInfo patch, MethodInfo original)
        {
            var harmony = Harmony as Harmony;
            var harmonyMethod = new HarmonyMethod(patch);

            switch (patcher)
            {
                case PatcherType.Prefix: harmony.Patch(original, prefix: harmonyMethod); break;
                case PatcherType.Postfix: harmony.Patch(original, postfix: harmonyMethod); break;
                case PatcherType.Transpiler: harmony.Patch(original, transpiler: harmonyMethod); break;
            }
        }

        #endregion

        #region ADDITIONAL

        private enum PatcherType
        {
            Prefix,
            Postfix,
            Transpiler
        }
        private enum Result
        {
            None,
            Success,
            Failed
        }
        private class PatchExeption : Exception
        {
            public PatchExeption(string message) : base(message) { }
        }

        #endregion
    }
}
