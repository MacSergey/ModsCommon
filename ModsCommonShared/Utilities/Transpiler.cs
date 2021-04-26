using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace ModsCommon.Utilities
{
    public static class TranspilerUtilities
    {
        public static CodeInstruction GetLDArg(this MethodBase method, string argName)
        {
            var idx = Array.FindIndex(method.GetParameters(), p => p.Name == argName);

            if (idx == -1)
                return null;
            else if (!method.IsStatic)
                idx += 1;

            return idx switch
            {
                0 => new CodeInstruction(OpCodes.Ldarg_0),
                1 => new CodeInstruction(OpCodes.Ldarg_1),
                2 => new CodeInstruction(OpCodes.Ldarg_2),
                3 => new CodeInstruction(OpCodes.Ldarg_3),
                _ => new CodeInstruction(OpCodes.Ldarg_S, idx)
            };
        }
        public static CodeInstruction BuildLdLocFromStLoc(this CodeInstruction instruction)
        {
            if (instruction.opcode == OpCodes.Stloc_0)
                return new CodeInstruction(OpCodes.Ldloc_0);
            else if (instruction.opcode == OpCodes.Stloc_1)
                return new CodeInstruction(OpCodes.Ldloc_1);
            else if (instruction.opcode == OpCodes.Stloc_2)
                return new CodeInstruction(OpCodes.Ldloc_2);
            else if (instruction.opcode == OpCodes.Stloc_3)
                return new CodeInstruction(OpCodes.Ldloc_3);
            else if (instruction.opcode == OpCodes.Stloc_S)
                return new CodeInstruction(OpCodes.Ldloc_S, instruction.operand);
            else if (instruction.opcode == OpCodes.Stloc)
                return new CodeInstruction(OpCodes.Ldloc, instruction.operand);
            else
                throw new Exception("instruction is not stloc! : " + instruction);
        }

        public static IEnumerable<CodeInstruction> SimulationStepImplTranspiler(IEnumerable<CodeInstruction> instructions, Action action)
        {
            var enumerator = instructions.GetEnumerator();

            var ldLoc1Found = false;
            var brFalseLabel = default(Label);

            while (enumerator.MoveNext())
            {
                var instruction = enumerator.Current;
                if (instruction.opcode == OpCodes.Ldloc_1)
                {
                    ldLoc1Found = true;
                    yield return instruction;
                }
                else if (ldLoc1Found && instruction.opcode == OpCodes.Brfalse)
                {
                    brFalseLabel = (Label)instruction.operand;
                    yield return instruction;
                    break;
                }
                else
                {
                    ldLoc1Found = false;
                    yield return instruction;
                }
            }

            while (enumerator.MoveNext())
            {
                var instruction = enumerator.Current;
                if (instruction.labels.Contains(brFalseLabel))
                    yield return new CodeInstruction(OpCodes.Call, action.Method);

                yield return instruction;
            }
        }
    }
}
