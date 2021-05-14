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
    }
}
