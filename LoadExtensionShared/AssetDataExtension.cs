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

    public abstract class BaseAssetDataExtension<PrefabType, TypeExtension, TypeAssetData> : AssetDataExtensionBase, IBaseAssetDataExtension
        where TypeExtension : BaseAssetDataExtension<PrefabType, TypeExtension, TypeAssetData>
        where PrefabType : PrefabInfo
    {
        protected Dictionary<PrefabType, TypeAssetData> AssetDatas { get; } = new Dictionary<PrefabType, TypeAssetData>();

        public override void OnCreated(IAssetData assetData)
        {
            base.OnCreated(assetData);
            SingletonItem<TypeExtension>.Instance = (TypeExtension)this;
        }
        public override void OnReleased() => SingletonItem<TypeExtension>.Instance = null;

        public abstract bool Load(PrefabType prefab, Dictionary<string, byte[]> userData, out TypeAssetData data);
        public abstract void Save(PrefabType prefab, Dictionary<string, byte[]> userData);

        protected void GetBytes(ushort n, out byte b1, out byte b2)
        {
            b1 = (byte)(n >> 8);
            b2 = (byte)n;
        }
        protected ushort GetUShort(byte b1, byte b2) => (ushort)((b1 << 8) + b2);
    }

    public abstract class BaseBuildingDataExtension<TypeExtension, TypeAssetData> : BaseAssetDataExtension<BuildingInfo, TypeExtension, TypeAssetData>, IBaseAssetDataExtension
        where TypeExtension : BaseAssetDataExtension<BuildingInfo, TypeExtension, TypeAssetData>
    {
        public override void OnAssetLoaded(string name, object asset, Dictionary<string, byte[]> userData)
        {
            if (asset is BuildingInfo prefab && userData != null && Load(prefab, userData, out var data))
                AssetDatas[prefab] = data;
        }
        public override void OnAssetSaved(string name, object asset, out Dictionary<string, byte[]> userData)
        {
            userData = new Dictionary<string, byte[]>();

            if (asset is BuildingInfo prefab && prefab.m_paths.Any())
                Save(prefab, userData);
        }
    }

    public abstract class BaseNetworkDataExtension<TypeExtension, TypeAssetData> : BaseAssetDataExtension<NetInfo, TypeExtension, TypeAssetData>, IBaseAssetDataExtension
    where TypeExtension : BaseAssetDataExtension<NetInfo, TypeExtension, TypeAssetData>
    {
        public override void OnAssetLoaded(string name, object asset, Dictionary<string, byte[]> userData)
        {
            if (asset is NetInfo prefab && userData != null && Load(prefab, userData, out var data))
                AssetDatas[prefab] = data;
        }
        public override void OnAssetSaved(string name, object asset, out Dictionary<string, byte[]> userData)
        {
            userData = new Dictionary<string, byte[]>();

            if (asset is NetInfo prefab)
                Save(prefab, userData);
        }
    }
}
