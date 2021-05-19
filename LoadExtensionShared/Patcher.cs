using ColossalFramework.UI;
using HarmonyLib;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace ModsCommon
{
    public static partial class Patcher
    {
        public static void LoadAssetPanelOnLoadPostfix<TypeExtension>(LoadAssetPanel __instance, UIListBox ___m_SaveList)
            where TypeExtension : IBaseAssetDataExtension
        {
            if (AccessTools.Method(typeof(LoadSavePanelBase<CustomAssetMetaData>), "GetListingMetaData") is not MethodInfo method)
                return;

            var listingMetaData = (CustomAssetMetaData)method.Invoke(__instance, new object[] { ___m_SaveList.selectedIndex });
            if (listingMetaData.userDataRef != null)
            {
                var userAssetData = (listingMetaData.userDataRef.Instantiate() as AssetDataWrapper.UserAssetData) ?? new AssetDataWrapper.UserAssetData();
                SingletonItem<TypeExtension>.Instance.OnAssetLoaded(listingMetaData.name, ToolsModifierControl.toolController.m_editPrefabInfo, userAssetData.Data);
            }
        }

        public static IEnumerable<CodeInstruction> BuildingDecorationLoadPathsTranspiler<TypeExtension>(IEnumerable<CodeInstruction> instructions)
            where TypeExtension : IBaseIntersectionAssetDataExtension
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
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TypeExtension), nameof(IBaseIntersectionAssetDataExtension.OnPlaceAsset)));
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
