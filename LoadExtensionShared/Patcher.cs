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
    }
}
