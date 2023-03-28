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
                Logger.Debug($"Patch {PatchResult}");
            }
            catch (Exception error)
            {
                PatchResult = PatchResult.Failed;
                Logger.Error($"Patch {PatchResult}", error);
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

        protected bool AddPrefix(Type patchType, string patchMethod, Type type, string method, Type[] parameters = null) => AddMethodPatch(PatcherType.Prefix, patchType, patchMethod, type, method, parameters);
        protected bool AddPostfix(Type patchType, string patchMethod, Type type, string method, Type[] parameters = null) => AddMethodPatch(PatcherType.Postfix, patchType, patchMethod, type, method, parameters);
        protected bool AddTranspiler(Type patchType, string patchMethod, Type type, string method, Type[] parameters = null) => AddMethodPatch(PatcherType.Transpiler, patchType, patchMethod, type, method, parameters);

        private bool AddMethodPatch(PatcherType patcher, Type patchType, string patchMethod, Type type, string method, Type[] parameters = null)
        {
            void action()
            {
                Logger.Debug($"Start add [{patcher.ToString().ToUpper()}] [{patchType?.FullName}.{patchMethod}] to [{type?.FullName}.{method}]");

                if (AccessTools.Method(type, method, parameters) is not MethodInfo original)
                    throw new PatchExeption("Can't find original method");
                if (AccessTools.Method(patchType, patchMethod) is not MethodInfo patch)
                    throw new PatchExeption("Can't find patch method");

                AddPatch(patcher, patch, original);

                Logger.Debug("Success patched!");
            }

            return AddPatchProcess(action);
        }

        protected bool AddPrefix(MethodInfo patch, Type type, string method, Type[] parameters = null) => AddMethodPatch(PatcherType.Prefix, patch, type, method, parameters);
        protected bool AddPostfix(MethodInfo patch, Type type, string method, Type[] parameters = null) => AddMethodPatch(PatcherType.Postfix, patch, type, method, parameters);
        protected bool AddTranspiler(MethodInfo patch, Type type, string method, Type[] parameters = null) => AddMethodPatch(PatcherType.Transpiler, patch, type, method, parameters);

        private bool AddMethodPatch(PatcherType patcher, MethodInfo patch, Type type, string method, Type[] parameters = null)
        {
            void action()
            {
                Logger.Debug($"Start add [{patcher.ToString().ToUpper()}] [{patch?.DeclaringType.FullName}.{patch?.Name}] to [{type?.FullName}.{method}]");

                if (AccessTools.Method(type, method, parameters) is not MethodInfo original)
                    throw new PatchExeption("Can't find original method");
                if (patch == null)
                    throw new PatchExeption("Can't find patch method");

                AddPatch(patcher, patch, original);

                Logger.Debug("Success patched!");
            }

            return AddPatchProcess(action);
        }

        protected bool AddPrefix(Type patchType, string patchMethod, Type type, Type[] parameters = null) => AddConstructorPatch(PatcherType.Prefix, patchType, patchMethod, type, parameters);
        protected bool AddPostfix(Type patchType, string patchMethod, Type type, Type[] parameters = null) => AddConstructorPatch(PatcherType.Postfix, patchType, patchMethod, type, parameters);
        protected bool AddTranspiler(Type patchType, string patchMethod, Type type, Type[] parameters = null) => AddConstructorPatch(PatcherType.Transpiler, patchType, patchMethod, type, parameters);

        private bool AddConstructorPatch(PatcherType patcher, Type patchType, string patchMethod, Type type, Type[] parameters = null)
        {
            void action()
            {
                Logger.Debug($"Start add [{patcher.ToString().ToUpper()}] [{patchType?.FullName}.{patchMethod}] to [{type?.FullName}..ctor]");

                if (AccessTools.Constructor(type, parameters) is not ConstructorInfo original)
                    throw new PatchExeption("Can't find original constructor");
                if (AccessTools.Method(patchType, patchMethod) is not MethodInfo patch)
                    throw new PatchExeption("Can't find patch method");

                AddPatch(patcher, patch, original);

                Logger.Debug("Success patched!");
            }

            return AddPatchProcess(action);
        }


        protected bool AddPrefix(Type patchType, string patchMethod, PropertyType propertyType, Type type, string property) => AddPropertyPatch(PatcherType.Prefix, patchType, patchMethod, propertyType, type, property);
        protected bool AddPostfix(Type patchType, string patchMethod, PropertyType propertyType, Type type, string property) => AddPropertyPatch(PatcherType.Postfix, patchType, patchMethod, propertyType, type, property);
        protected bool AddTranspiler(Type patchType, string patchMethod, PropertyType propertyType, Type type, string property) => AddPropertyPatch(PatcherType.Transpiler, patchType, patchMethod, propertyType, type, property);

        private bool AddPropertyPatch(PatcherType patcher, Type patchType, string patchMethod, PropertyType propertyType, Type type, string property)
        {
            void action()
            {
                Logger.Debug($"Start add [{patcher.ToString().ToUpper()}] [{patchType?.FullName}.{patchMethod}] to [{type?.FullName}.{property}.{propertyType}]");

                MethodInfo original = null;
                if (AccessTools.Property(type, property) is not PropertyInfo propertyInfo)
                    throw new PatchExeption("Can't find original property");
                else if (propertyType == PropertyType.Getter)
                {
                    if (!propertyInfo.CanRead)
                        throw new PatchExeption("Property does not have getter");
                    else
                        original = propertyInfo.GetGetMethod();
                }
                else if (propertyType == PropertyType.Setter)
                {
                    if (!propertyInfo.CanWrite)
                        throw new PatchExeption("Property does not have setter");
                    else
                        original = propertyInfo.GetSetMethod();
                }
                else
                    throw new PatchExeption("Unexpected state");

                if (AccessTools.Method(patchType, patchMethod) is not MethodInfo patch)
                    throw new PatchExeption("Can't find patch method");

                AddPatch(patcher, patch, original);

                Logger.Debug("Success patched!");
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

        protected enum PropertyType
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
