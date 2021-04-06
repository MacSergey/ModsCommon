﻿using CitiesHarmony.API;
using ColossalFramework.UI;
using HarmonyLib;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ModsCommon
{
    public abstract class BasePatcherMod<TypeMod> : BaseMod<TypeMod>
        where TypeMod : BaseMod<TypeMod>
    {
        protected override bool LoadError
        {
            get => base.LoadError || PatchError;
            set => base.LoadError = value;
        }
        public bool PatchError { get; private set; }
        public object Harmony => new Harmony(Id);

        public override void OnEnabled()
        {
            base.OnEnabled();
            PatchError = false;

            try
            {
                Patch();
            }
            catch (Exception error)
            {
                LoadError = true;
                Logger.Error("Patch failed", error);
            }

            CheckLoadedError();
        }
        public override void OnDisabled()
        {
            base.OnDisabled();

            try { Unpatch(); }
            catch (Exception error) { Logger.Error("Unpatch failed", error); }
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

            try { PatchError = !PatchProcess(); }
            catch { PatchError = true; }

            CheckLoadedError();
            Logger.Debug(PatchError ? "Patch Filed" : "Patch success");
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

        protected delegate IEnumerable<CodeInstruction> Transpiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions);
        protected bool Patch_ToolController_Awake(Transpiler transpiler)
        {
            return AddTranspiler(transpiler.Method, typeof(ToolController), "Awake");
        }
        protected bool Patch_ToolController_Awake<TypeTool>()
            where TypeTool : BaseTool<TypeMod, TypeTool>
        {
            var patch = AccessTools.Method(typeof(BasePatcherMod<TypeMod>), nameof(ToolControllerAwakeTranspiler), generics: new Type[] { typeof(TypeTool) });
            return AddTranspiler(patch, typeof(ToolController), "Awake");
        }

        protected static IEnumerable<CodeInstruction> ToolControllerAwakeTranspiler<TypeTool>(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
            where TypeTool : BaseTool<TypeMod, TypeTool>
        {
            var createMethod = AccessTools.Method(typeof(TypeTool), nameof(BaseTool<TypeMod, TypeTool>.Create));
            yield return new CodeInstruction(OpCodes.Call, createMethod);

            foreach (var instruction in instructions)
                yield return instruction;
        }

        protected bool Patch_GameKeyShortcuts_Escape(Transpiler transpiler)
        {
            return AddTranspiler(transpiler.Method, typeof(GameKeyShortcuts), "Escape");
        }
        protected static IEnumerable<CodeInstruction> GameKeyShortcutsEscapeTranspiler<TypeTool>(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
            where TypeTool : BaseTool<TypeMod, TypeTool>
        {
            var instructionList = instructions.ToList();

            var elseIndex = instructionList.FindLastIndex(i => i.opcode == OpCodes.Brfalse);
            var elseLabel = (Label)instructionList[elseIndex].operand;

            for (var i = elseIndex + 1; i < instructionList.Count; i += 1)
            {
                if (instructionList[i].labels.Contains(elseLabel))
                {
                    var elseInstruction = instructionList[i];
                    var oldElseLabels = elseInstruction.labels;
                    var newElseLabel = generator.DefineLabel();
                    elseInstruction.labels = new List<Label>() { newElseLabel };
                    var returnLabel = generator.DefineLabel();

                    var newInstructions = new List<CodeInstruction>()
                    {
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SingletonTool<TypeTool>), $"get_{nameof(SingletonTool<TypeTool>.Instance)}")),
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TypeTool), $"get_{nameof(BaseTool<TypeTool>.enabled)}")),
                        new CodeInstruction(OpCodes.Brfalse, newElseLabel),

                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SingletonTool<TypeTool>), $"get_{nameof(SingletonTool<TypeTool>.Instance)}")),
                        new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(TypeTool), nameof(BaseTool<TypeMod, TypeTool>.Escape))),
                        new CodeInstruction(OpCodes.Br, returnLabel),
                    };

                    newInstructions[0].labels = oldElseLabels;
                    instructionList.InsertRange(i, newInstructions);
                    instructionList.Last().labels.Add(returnLabel);

                    break;
                }
            }

            return instructionList;
        }
        protected bool Patch_LoadAssetPanel_OnLoad(Action<LoadAssetPanel, UIListBox> postfix)
        {
            return AddPostfix(postfix.Method, typeof(LoadAssetPanel), nameof(LoadAssetPanel.OnLoad));
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
    public abstract class Mod
    {
        private static void Prefix<Type>(Type value)
        {
            ...

        }
    }
    public class Mod1 : BaseMode<Tool1> { }
    public class Mod2 : BaseMode<Tool2> { }
}
