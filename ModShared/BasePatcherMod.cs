using HarmonyLib;
using ModsCommon.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ModsCommon
{
    public abstract class BasePatcherMod<TypeMod> : BaseMod<TypeMod>
        where TypeMod : BaseMod<TypeMod>
    {
        #region PROPERTIES

        public override ModStatus Status
        {
            get => base.Status | (PatchResult == PatchResult.Failed ? ModStatus.LoadingError : ModStatus.Unknown);
            protected set => base.Status = value;
        }

        private PatchResult _patchResult;
        protected PatchResult PatchResult
        {
            get => _patchResult;
            set
            {
                if (value != _patchResult)
                {
                    _patchResult = value;
                    StatusChanged();
                }
            }
        }
        public object Harmony => new Harmony(Id);

        protected override List<BaseDependencyInfo> DependencyInfos
        {
            get
            {
                var infos = base.DependencyInfos;

                var nameSearcher = IdSearcher.Invalid &
                    new UserModNameSearcher("Harmony 2", BaseMatchSearcher.Option.AllOptions | BaseMatchSearcher.Option.StartsWidth) &
                    new UserModDescriptionSearcher("Mod Dependency", BaseMatchSearcher.Option.AllOptions);
                var idSearcher = new IdSearcher(2040656402ul) | new IdSearcher(2399204842ul);
                infos.Add(new RequiredDependencyInfo(DependencyState.Enable, nameSearcher | idSearcher, "Harmony", 2040656402ul));

                var conflictSearcher = new IdSearcher(2399343344ul);
                infos.Add(new ConflictDependencyInfo(DependencyState.Unsubscribe, conflictSearcher, "Harmony (redesigned)"));

                return infos;
            }
        }

        #endregion

        #region BASIC

        protected override void Enable()
        {
            PatchResult = PatchResult.None;
            Patch();
        }
        protected override void Disable()
        {
            if (PatchResult == PatchResult.Success)
                Unpatch();

            PatchResult = PatchResult.None;
        }
        private void Patch()
        {
            Logger.Debug("Patch");

            try
            {
                PatchResult = PatchProcess() ? PatchResult.Success : PatchResult.Failed;
                Logger.Debug($"Patching result: {PatchResult}");
            }
            catch (Exception error)
            {
                PatchResult = PatchResult.Failed;
                Logger.Error($"Patching result: {PatchResult}", error);
            }
        }
        private void Unpatch()
        {
            Logger.Debug($"Unpatch all");
            var harmony = Harmony as Harmony;
            harmony.UnpatchAll(harmony.Id);
            Logger.Debug($"Unpatched");
        }
        protected override void OnLoadError(out bool shown)
        {
            base.OnLoadError(out shown);

            if (shown)
                return;
            else if (PatchResult == PatchResult.Failed)
            {
                var message = MessageBox.Show<ErrorLoadMessageBox>();
                message.Init<TypeMod>();

                shown = true;
            }
        }

        protected abstract bool PatchProcess();

        protected bool AddPrefix(Type patchType, string patchName, Type originalType, string originalName, Type[] parameters = null) => AddMethodPatch(PatcherType.Prefix, patchType, patchName, originalType, originalName, parameters);
        protected bool AddPostfix(Type patchType, string patchName, Type originalType, string originalName, Type[] parameters = null) => AddMethodPatch(PatcherType.Postfix, patchType, patchName, originalType, originalName, parameters);
        protected bool AddTranspiler(Type patchType, string patchName, Type originalType, string originalName, Type[] parameters = null) => AddMethodPatch(PatcherType.Transpiler, patchType, patchName, originalType, originalName, parameters);

        private bool AddMethodPatch(PatcherType patcher, Type patchType, string patchName, Type originalType, string originalName, Type[] parameters = null)
        {
            void action()
            {
                Logger.Debug($"Start adding [{patcher.ToString().ToUpper()}] [{patchType?.FullName}.{patchName}] to [{originalType?.FullName}.{originalName}]");

                if (AccessTools.Method(originalType, originalName, parameters) is not MethodInfo original)
                    throw new PatchExeption("Can't find original method");
                if (AccessTools.Method(patchType, patchName) is not MethodInfo patch)
                    throw new PatchExeption("Can't find patch method");

                AddPatch(patcher, patch, original);

                Logger.Debug("Successfully patched!");
            }

            return AddPatchProcess(action);
        }

        protected bool AddPrefix(Type patchType, string patchName, MethodInfo original) => AddMethodPatch(PatcherType.Prefix, patchType, patchName, original);
        protected bool AddPostfix(Type patchType, string patchName, MethodInfo original) => AddMethodPatch(PatcherType.Postfix, patchType, patchName, original);
        protected bool AddTranspiler(Type patchType, string patchName, MethodInfo original) => AddMethodPatch(PatcherType.Transpiler, patchType, patchName, original);

        private bool AddMethodPatch(PatcherType patcher, Type patchType, string patchName, MethodInfo original)
        {
            void action()
            {
                Logger.Debug($"Start adding [{patcher.ToString().ToUpper()}] [{patchType?.FullName}.{patchName}] to [{original?.DeclaringType.FullName}.{original?.Name}]");

                if (original == null)
                    throw new PatchExeption("Can't find original method");
                if (AccessTools.Method(patchType, patchName) is not MethodInfo patch)
                    throw new PatchExeption("Can't find patch method");

                AddPatch(patcher, patch, original);

                Logger.Debug("Successfully patched!");
            }

            return AddPatchProcess(action);
        }


        protected bool AddPrefix(MethodInfo patch, Type originalType, string originalName, Type[] parameters = null) => AddMethodPatch(PatcherType.Prefix, patch, originalType, originalName, parameters);
        protected bool AddPostfix(MethodInfo patch, Type originalType, string originalName, Type[] parameters = null) => AddMethodPatch(PatcherType.Postfix, patch, originalType, originalName, parameters);
        protected bool AddTranspiler(MethodInfo patch, Type originalType, string originalName, Type[] parameters = null) => AddMethodPatch(PatcherType.Transpiler, patch, originalType, originalName, parameters);

        private bool AddMethodPatch(PatcherType patcher, MethodInfo patch, Type originalType, string originalName, Type[] parameters = null)
        {
            void action()
            {
                Logger.Debug($"Start adding [{patcher.ToString().ToUpper()}] [{patch?.DeclaringType.FullName}.{patch?.Name}] to [{originalType?.FullName}.{originalName}]");

                if (AccessTools.Method(originalType, originalName, parameters) is not MethodInfo original)
                    throw new PatchExeption("Can't find original method");
                if (patch == null)
                    throw new PatchExeption("Can't find patch method");

                AddPatch(patcher, patch, original);

                Logger.Debug("Successfully patched!");
            }

            return AddPatchProcess(action);
        }

        protected bool AddPrefix(Type patchType, string patchName, Type originalType, Type[] parameters = null) => AddConstructorPatch(PatcherType.Prefix, patchType, patchName, originalType, parameters);
        protected bool AddPostfix(Type patchType, string patchName, Type originalType, Type[] parameters = null) => AddConstructorPatch(PatcherType.Postfix, patchType, patchName, originalType, parameters);
        protected bool AddTranspiler(Type patchType, string patchName, Type originalType, Type[] parameters = null) => AddConstructorPatch(PatcherType.Transpiler, patchType, patchName, originalType, parameters);

        private bool AddConstructorPatch(PatcherType patcher, Type patchType, string patchName, Type originalType, Type[] parameters = null)
        {
            void action()
            {
                Logger.Debug($"Start adding [{patcher.ToString().ToUpper()}] [{patchType?.FullName}.{patchName}] to [{originalType?.FullName}..ctor]");

                if (AccessTools.Constructor(originalType, parameters) is not ConstructorInfo original)
                    throw new PatchExeption("Can't find original constructor");
                if (AccessTools.Method(patchType, patchName) is not MethodInfo patch)
                    throw new PatchExeption("Can't find patch method");

                AddPatch(patcher, patch, original);

                Logger.Debug("Successfully patched!");
            }

            return AddPatchProcess(action);
        }


        protected bool AddPrefix(Type patchType, string patchName, MethodType methodType, Type originalType, string propertyName) => AddPropertyPatch(PatcherType.Prefix, patchType, patchName, methodType, originalType, propertyName);
        protected bool AddPostfix(Type patchType, string patchName, MethodType methodType, Type originalType, string propertyName) => AddPropertyPatch(PatcherType.Postfix, patchType, patchName, methodType, originalType, propertyName);
        protected bool AddTranspiler(Type patchType, string patchName, MethodType methodType, Type originalType, string propertyName) => AddPropertyPatch(PatcherType.Transpiler, patchType, patchName, methodType, originalType, propertyName);

        private bool AddPropertyPatch(PatcherType patcher, Type patchType, string patchName, MethodType methodType, Type originalType, string propertyName)
        {
            void action()
            {
                Logger.Debug($"Start adding [{patcher.ToString().ToUpper()}] [{patchType?.FullName}.{patchName}] to [{originalType?.FullName}.{propertyName}.{methodType}]");

                MethodInfo original = null;
                if (AccessTools.Property(originalType, propertyName) is not PropertyInfo propertyInfo)
                    throw new PatchExeption("Can't find original property");
                else if (methodType == MethodType.Getter)
                {
                    if (!propertyInfo.CanRead)
                        throw new PatchExeption("Property does not have getter");
                    else
                        original = propertyInfo.GetGetMethod();
                }
                else if (methodType == MethodType.Setter)
                {
                    if (!propertyInfo.CanWrite)
                        throw new PatchExeption("Property does not have setter");
                    else
                        original = propertyInfo.GetSetMethod();
                }
                else
                    throw new PatchExeption("Unexpected state");

                if (AccessTools.Method(patchType, patchName) is not MethodInfo patch)
                    throw new PatchExeption("Can't find patch method");

                AddPatch(patcher, patch, original);

                Logger.Debug("Successfully patched!");
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
                Logger.Error($"Patch failed: {error.Message}");
                return false;
            }
            catch (Exception error)
            {
                if (error.InnerException is PatchExeption patchExeption)
                    Logger.Error($"Patch failed: {patchExeption.Message}");
                else
                    Logger.Error($"Patch failed:", error);
                return false;
            }
        }
        private void AddPatch(PatcherType patcher, MethodInfo patch, MethodBase original)
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

        protected enum MethodType
        {
            Getter,
            Setter,
        }

        private class PatchExeption : Exception
        {
            public PatchExeption(string message) : base(message) { }
        }
        #endregion
    }
    public enum PatchResult
    {
        None = 0,
        Success = 1,
        Failed = 2,
    }
}
