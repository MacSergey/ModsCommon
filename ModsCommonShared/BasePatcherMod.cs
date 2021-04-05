using CitiesHarmony.API;
using HarmonyLib;
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
            get => base.LoadError || !PatchSuccess;
            set => base.LoadError = value;
        }
        public bool PatchSuccess { get; private set; }
        public object Harmony => new Harmony(Id);

        public override void OnEnabled()
        {
            base.OnEnabled();

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
            HarmonyHelper.DoOnHarmonyReady(() => Begin());
        }
        private void Unpatch()
        {
            Logger.Debug($"Unpatch all");
            var harmony = Harmony as Harmony;
            harmony.UnpatchAll(harmony.Id);
            Logger.Debug($"Unpatched");
        }

        private void Begin()
        {
            Logger.Debug("Start patching");

            try { PatchSuccess = PatchProcess(); }
            catch { PatchSuccess = false; }

            CheckLoadedError();
            Logger.Debug(PatchSuccess ? "Patch success" : "Patch Filed");
        }
        protected abstract bool PatchProcess();

        protected bool AddPrefix(Type patchType, string patchMethod, Type type, string method, Type[] parameters = null) => AddPatch(PatcherType.Prefix, patchType, patchMethod, type, method, parameters);
        protected bool AddPostfix(Type patchType, string patchMethod, Type type, string method, Type[] parameters = null) => AddPatch(PatcherType.Postfix, patchType, patchMethod, type, method, parameters);
        protected bool AddTranspiler(Type patchType, string patchMethod, Type type, string method, Type[] parameters = null, Type[] transpilerGenerics = null) => AddPatch(PatcherType.Transpiler, patchType, patchMethod, type, method, parameters, transpilerGenerics);

        private bool AddPatch(PatcherType patcher, Type patchType, string patchMethod, Type type, string method, Type[] parameters = null, Type[] patchGenerics = null)
        {
            try
            {
                Logger.Debug($"Start add [{patcher.ToString().ToUpper()}] [{patchType?.FullName}.{patchMethod}] to [{type?.FullName}.{method}]");

                if (AccessTools.Method(type, method, parameters) is not MethodInfo original)
                    throw new PatchExeption("Can't find original method");
                if (AccessTools.Method(patchType, patchMethod, generics: patchGenerics) is not MethodInfo patch)
                    throw new PatchExeption("Can't find patch method");

                var harmony = Harmony as Harmony;
                var harmonyMethod = new HarmonyMethod(patch);
                switch (patcher)
                {
                    case PatcherType.Prefix: harmony.Patch(original, prefix: harmonyMethod); break;
                    case PatcherType.Postfix: harmony.Patch(original, postfix: harmonyMethod); break;
                    case PatcherType.Transpiler: harmony.Patch(original, transpiler: harmonyMethod); break;
                }

                Logger.Debug($"[{patchType?.FullName}.{patchMethod}] success patched!");
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

        protected bool Patch_ToolController_Awake<TypeTool>()
            where TypeTool : BaseTool<TypeMod, TypeTool>
        {
            return AddTranspiler(typeof(TypeMod), nameof(BasePatcherMod<TypeMod>.ToolControllerAwakeTranspiler), typeof(ToolController), "Awake", transpilerGenerics: new Type[] { typeof(TypeTool) });
        }

        protected static IEnumerable<CodeInstruction> ToolControllerAwakeTranspiler<TypeTool>(IEnumerable<CodeInstruction> instructions)
            where TypeTool : BaseTool<TypeMod, TypeTool>
        {
            var createMethod = AccessTools.Method(typeof(TypeTool), nameof(BaseTool<TypeMod, TypeTool>.Create));
            yield return new CodeInstruction(OpCodes.Call, createMethod);

            foreach (var instruction in instructions)
                yield return instruction;
        }

        protected bool Patch_GameKeyShortcuts_Escape<TypeTool>()
            where TypeTool : BaseTool<TypeMod, TypeTool>
        {
            return AddTranspiler(typeof(TypeMod), nameof(BasePatcherMod<TypeMod>.GameKeyShortcutsEscapeTranspiler), typeof(GameKeyShortcuts), "Escape", transpilerGenerics: new Type[] { typeof(TypeTool) });
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
