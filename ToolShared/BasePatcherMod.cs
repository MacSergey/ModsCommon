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
        #region ADD TOOL

        protected bool AddTool<TypeTool>()
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

        #endregion

        #region TOOL ONESCAPE

        protected bool ToolOnEscape<TypeTool>()
            where TypeTool : BaseTool<TypeMod, TypeTool>
        {
            var patch = AccessTools.Method(typeof(BasePatcherMod<TypeMod>), nameof(GameKeyShortcutsEscapeTranspiler), generics: new Type[] { typeof(TypeTool) });
            return AddTranspiler(patch, typeof(GameKeyShortcuts), "Escape");
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
                        new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(SingletonTool<TypeTool>), nameof(SingletonTool<TypeTool>.Instance))),
                        new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(TypeTool), nameof(BaseTool<TypeTool>.enabled))),
                        new CodeInstruction(OpCodes.Brfalse, newElseLabel),

                        new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(SingletonTool<TypeTool>), nameof(SingletonTool<TypeTool>.Instance))),
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

        #endregion

        #region ADD NETTOOL BUTTON

        private static string RoadsOptionPanel => nameof(RoadsOptionPanel);
        protected bool AddNetToolButton<TypeButton>()
        {
            var patch = AccessTools.Method(typeof(BasePatcherMod<TypeMod>), nameof(GeneratedScrollPanelCreateOptionPanelPostfix), generics: new Type[] { typeof(TypeButton) });
            return AddPostfix(patch, typeof(GeneratedScrollPanel), "CreateOptionPanel");
        }
        public static void GeneratedScrollPanelCreateOptionPanelPostfix<TypeButton>(string templateName, ref OptionPanelBase __result)
            where TypeButton : UIButton
        {
            if (__result == null || templateName != RoadsOptionPanel || __result.component.Find<TypeButton>(nameof(TypeButton)) != null)
                return;

            SingletonMod<TypeMod>.Logger.Debug($"Create button");
            __result.component.AddUIComponent<TypeButton>();
            SingletonMod<TypeMod>.Logger.Debug($"Button created");
        }

        #endregion
    }
}
