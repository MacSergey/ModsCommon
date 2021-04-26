using ColossalFramework.UI;
using HarmonyLib;
using ICities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ModsCommon.Utilities
{
    public abstract class BaseAssetDataExtension<TypeExtension> : AssetDataExtensionBase
        where TypeExtension : BaseAssetDataExtension<TypeExtension>
    {
        public static void LoadAssetPanelOnLoadPostfix(LoadAssetPanel __instance, UIListBox ___m_SaveList)
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
    }

    public abstract class BaseAssetDataExtension<TypeExtension, TypeAssetData> : BaseAssetDataExtension<TypeExtension>
        where TypeExtension : BaseAssetDataExtension<TypeExtension, TypeAssetData>
    {
        private Dictionary<BuildingInfo, TypeAssetData> AssetDatas { get; } = new Dictionary<BuildingInfo, TypeAssetData>();

        public override void OnCreated(IAssetData assetData)
        {
            base.OnCreated(assetData);
            SingletonItem<TypeExtension>.Instance = (TypeExtension)this;
        }
        public override void OnReleased() => SingletonItem<TypeExtension>.Instance = null;
        public override void OnAssetLoaded(string name, object asset, Dictionary<string, byte[]> userData)
        {
            if(asset is BuildingInfo prefab && userData != null && Load(prefab, userData, out var data))
                AssetDatas[prefab] = data;
        }
        public override void OnAssetSaved(string name, object asset, out Dictionary<string, byte[]> userData)
        {
            userData = new Dictionary<string, byte[]>();

            if (asset is BuildingInfo prefab && prefab.m_paths.Any())
                Save(prefab, userData);
        }
        public void OnPlaceAsset(BuildingInfo buildingInfo, FastList<ushort> segments, FastList<ushort> nodes)
        {
            if (AssetDatas.TryGetValue(buildingInfo, out var assetData))
                PlaceAsset(assetData, segments, nodes);
        }

        public abstract bool Load(BuildingInfo prefab, Dictionary<string, byte[]> userData, out TypeAssetData data);
        public abstract void Save(BuildingInfo prefab, Dictionary<string, byte[]> userData);
        protected abstract void PlaceAsset(TypeAssetData data, FastList<ushort> segments, FastList<ushort> nodes);


        public static IEnumerable<CodeInstruction> BuildingDecorationLoadPathsTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var segmentBufferField = AccessTools.DeclaredField(typeof(NetManager), nameof(NetManager.m_tempSegmentBuffer));
            var nodeBufferField = AccessTools.DeclaredField(typeof(NetManager), nameof(NetManager.m_tempNodeBuffer));
            var clearMethod = AccessTools.DeclaredMethod(nodeBufferField.FieldType, nameof(FastList<ushort>.Clear));

            var matchCount = 0;
            var inserted = false;
            var enumerator = instructions.GetEnumerator();
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
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TypeExtension), nameof(OnPlaceAsset)));
                    inserted = true;
                }

                if (prevInstruction != null)
                    yield return prevInstruction;

                prevInstruction = instruction;
            }

            if (prevInstruction != null)
                yield return prevInstruction;
        }
    }
}
