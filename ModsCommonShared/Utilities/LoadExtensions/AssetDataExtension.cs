using ColossalFramework.UI;
using HarmonyLib;
using ICities;
using System.Reflection;

namespace ModsCommon.Utilities
{
    public abstract class BaseAssetDataExtension<TypeExtension> : AssetDataExtensionBase
        where TypeExtension : BaseAssetDataExtension<TypeExtension>
    {
        public override void OnCreated(IAssetData assetData)
        {
            base.OnCreated(assetData);
            SingletonItem<TypeExtension>.Instance = (TypeExtension)this;
        }
        public override void OnReleased() => SingletonItem<TypeExtension>.Instance = null;

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
}
