using HarmonyLib;
using ModsCommon.Utilities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ModsCommon
{
    public static partial class Patcher
    {
        public static IEnumerable<CodeInstruction> BuildingDecorationLoadPathsTranspiler<TypeExtension>(IEnumerable<CodeInstruction> instructions)
            where TypeExtension : IBaseBuildingAssetDataExtension
        {
            var segmentBufferField = AccessTools.DeclaredField(typeof(NetManager), nameof(NetManager.m_tempSegmentBuffer));
            var nodeBufferField = AccessTools.DeclaredField(typeof(NetManager), nameof(NetManager.m_tempNodeBuffer));
            var clearMethod = AccessTools.DeclaredMethod(nodeBufferField.FieldType, nameof(FastList<ushort>.Clear));

            var matchCount = 0;
            var inserted = false;
            var enumerator = instructions.GetEnumerator();
            var prevPrevInstruction = (CodeInstruction)null;
            var prevInstruction = (CodeInstruction)null;
            while (enumerator.MoveNext())
            {
                var instruction = enumerator.Current;

                if (prevInstruction != null && prevInstruction.opcode == OpCodes.Ldfld && prevInstruction.operand == nodeBufferField && instruction.opcode == OpCodes.Callvirt && instruction.operand == clearMethod)
                    matchCount += 1;

                if (!inserted && matchCount == 2)
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(SingletonItem<TypeExtension>), nameof(SingletonItem<TypeExtension>.Instance)));
                    yield return new CodeInstruction(OpCodes.Box, typeof(TypeExtension));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, segmentBufferField);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, nodeBufferField);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TypeExtension), nameof(IBaseBuildingAssetDataExtension.OnPlaceAsset)));
                    inserted = true;
                }

                if (prevPrevInstruction != null)
                    yield return prevPrevInstruction;

                prevPrevInstruction = prevInstruction;
                prevInstruction = instruction;
            }

            if (prevPrevInstruction != null)
                yield return prevPrevInstruction;

            if (prevInstruction != null)
                yield return prevInstruction;
        }
    }
}
