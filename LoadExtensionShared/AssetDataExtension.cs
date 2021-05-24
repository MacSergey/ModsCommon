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
    public interface IBaseIntersectionAssetDataExtension 
    {
        void OnPlaceAsset(BuildingInfo buildingInfo, FastList<ushort> segments, FastList<ushort> nodes);
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

    public abstract class BaseIntersectionAssetDataExtension<TypeMod, TypeExtension, TypeObjectMap> : BaseAssetDataExtension<TypeExtension, AssetData>, IBaseIntersectionAssetDataExtension
        where TypeMod : ICustomMod
        where TypeExtension : BaseAssetDataExtension<TypeExtension, AssetData>
        where TypeObjectMap : IObjectsMap
    {
        protected abstract string DataId { get; }
        protected abstract string MapId { get; }

        public override bool Load(BuildingInfo prefab, Dictionary<string, byte[]> userData, out AssetData data)
        {
            if (userData.TryGetValue(DataId, out byte[] rawData) && userData.TryGetValue(MapId, out byte[] map))
            {
                SingletonMod<TypeMod>.Logger.Debug($"Start load prefab data \"{prefab.name}\"");
                try
                {
                    var decompress = Loader.Decompress(rawData);
                    var config = XmlExtension.Parse(decompress);

                    SetMap(map, out var segments, out var nodes);
                    data = new AssetData(config, segments, nodes);
                    SingletonMod<TypeMod>.Logger.Debug($"Prefab data was loaded; Size = {rawData.Length} bytes");
                    return true;
                }
                catch (Exception error)
                {
                    SingletonMod<TypeMod>.Logger.Error("Could not load prefab data", error);
                }
            }

            data = default;
            return false;
        }
        public override void Save(BuildingInfo prefab, Dictionary<string, byte[]> userData)
        {
            SingletonMod<TypeMod>.Logger.Debug($"Start save prefab data \"{prefab.name}\"");
            try
            {
                var config = Loader.GetString(GetConfig());
                var data = Loader.Compress(config);

                userData[DataId] = data;
                userData[MapId] = GetMap();

                SingletonMod<TypeMod>.Logger.Debug($"Prefab data was saved; Size = {data.Length} bytes");
            }
            catch (Exception error)
            {
                SingletonMod<TypeMod>.Logger.Error("Could not save prefab data", error);
            }
        }

        public void OnPlaceAsset(BuildingInfo buildingInfo, FastList<ushort> segments, FastList<ushort> nodes)
        {
            if (AssetDatas.TryGetValue(buildingInfo, out var assetData))
            {
                SingletonMod<TypeMod>.Logger.Debug($"Place asset {buildingInfo.name}");

                var map = CreateMap(true);

                var segmentsCount = Math.Min(assetData.Segments.Length, segments.m_size);
                for (var i = 0; i < segmentsCount; i += 1)
                    map.AddSegment(assetData.Segments[i], segments[i]);

                var nodesCount = Math.Min(assetData.Nodes.Length, nodes.m_size);
                for (var i = 0; i < nodesCount; i += 1)
                    map.AddNode(assetData.Nodes[i], nodes[i]);

                PlaceAsset(assetData.Config, map);
            }
        }

        protected abstract void PlaceAsset(XElement config, TypeObjectMap map);

        protected abstract XElement GetConfig();
        protected abstract TypeObjectMap CreateMap(bool isSimple);

        private byte[] GetMap()
        {
            var instance = Singleton<NetManager>.instance;

            var segmentsId = new List<ushort>();
            for (ushort i = 0; i < NetManager.MAX_SEGMENT_COUNT; i += 1)
            {
                if (instance.m_segments.m_buffer[i].m_flags.CheckFlags(NetSegment.Flags.Created))
                    segmentsId.Add(i);
            }

            var map = new byte[sizeof(ushort) * 3 * segmentsId.Count];

            for (var i = 0; i < segmentsId.Count; i += 1)
            {
                var segmentId = segmentsId[i];
                var segment = instance.m_segments.m_buffer[segmentId];
                GetBytes(segmentId, out map[i * 6], out map[i * 6 + 1]);
                GetBytes(segment.m_startNode, out map[i * 6 + 2], out map[i * 6 + 3]);
                GetBytes(segment.m_endNode, out map[i * 6 + 4], out map[i * 6 + 5]);
            }

            return map;
        }
        private void SetMap(byte[] map, out ushort[] segments, out ushort[] nodes)
        {
            var count = map.Length / 6;
            segments = new ushort[count];
            nodes = new ushort[count * 2];

            for (var i = 0; i < count; i += 1)
            {
                segments[i] = GetUShort(map[i * 6], map[i * 6 + 1]);
                nodes[i * 2] = GetUShort(map[i * 6 + 2], map[i * 6 + 3]);
                nodes[i * 2 + 1] = GetUShort(map[i * 6 + 4], map[i * 6 + 5]);
            }
        }
        private void GetBytes(ushort n, out byte b1, out byte b2)
        {
            b1 = (byte)(n >> 8);
            b2 = (byte)n;
        }
        private ushort GetUShort(byte b1, byte b2) => (ushort)((b1 << 8) + b2);

        
    }
    public struct AssetData
    {
        public XElement Config { get; }
        public ushort[] Segments { get; }
        public ushort[] Nodes { get; }

        public AssetData(XElement config, ushort[] segments, ushort[] nodes)
        {
            Config = config;
            Segments = segments;
            Nodes = nodes;
        }
    }
}
