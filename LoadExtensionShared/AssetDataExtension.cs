using ColossalFramework;
using ColossalFramework.UI;
using HarmonyLib;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml.Linq;

namespace ModsCommon.Utilities
{
    public interface IBaseAssetDataExtension 
    {
        void OnAssetLoaded(string name, object asset, Dictionary<string, byte[]> userData);
    }

    public abstract class BaseAssetDataExtension<TypeExtension, TypeAssetData> : AssetDataExtensionBase, IBaseAssetDataExtension
        where TypeExtension : BaseAssetDataExtension<TypeExtension, TypeAssetData>
    {
        protected Dictionary<BuildingInfo, TypeAssetData> AssetDatas { get; } = new Dictionary<BuildingInfo, TypeAssetData>();

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

        public abstract bool Load(BuildingInfo prefab, Dictionary<string, byte[]> userData, out TypeAssetData data);
        public abstract void Save(BuildingInfo prefab, Dictionary<string, byte[]> userData);
    }
}
