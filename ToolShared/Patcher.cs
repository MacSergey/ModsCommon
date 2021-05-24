using CitiesHarmony.API;
using ColossalFramework.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ModsCommon
{
    public static partial class Patcher
    {
        public static IEnumerable<CodeInstruction> ToolControllerAwakeTranspiler<TypeMod, TypeTool>(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
            where TypeMod : BaseMod<TypeMod>
            where TypeTool : BaseTool<TypeMod, TypeTool>
        {
            var createMethod = AccessTools.Method(typeof(TypeTool), nameof(BaseTool<TypeMod, TypeTool>.Create));
            yield return new CodeInstruction(OpCodes.Call, createMethod);

            foreach (var instruction in instructions)
                yield return instruction;
        }

        public static IEnumerable<CodeInstruction> GameKeyShortcutsEscapeTranspiler<TypeMod, TypeTool>(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
            where TypeMod : BaseMod<TypeMod>
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
                        new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(TypeTool), nameof(BaseTool<TypeMod, TypeTool>.enabled))),
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

        public static void GeneratedScrollPanelCreateOptionPanelPostfix<TypeMod, TypeButton>(string templateName, ref OptionPanelBase __result)
            where TypeMod : BaseMod<TypeMod>
            where TypeButton : UIButton
        {
            if (__result == null || templateName != "RoadsOptionPanel" || __result.component.Find<TypeButton>(typeof(TypeButton).Name) != null)
                return;

            SingletonMod<TypeMod>.Logger.Debug($"Create button");
            __result.component.AddUIComponent<TypeButton>();
            SingletonMod<TypeMod>.Logger.Debug($"Button created");
        }
    }
}
