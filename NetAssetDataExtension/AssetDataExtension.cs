using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace ModsCommon.Utilities
{
    public interface IBaseBuildingAssetDataExtension
    {
        void OnPlaceAsset(BuildingInfo buildingInfo, FastList<ushort> segments, FastList<ushort> nodes);
    }

    public abstract class BaseBuildingAssetDataExtension<TypeMod, TypeExtension, TypeObjectMap> : BaseBuildingDataExtension<TypeExtension, BuildingAssetData>, IBaseBuildingAssetDataExtension
        where TypeMod : ICustomMod
        where TypeExtension : BaseBuildingDataExtension<TypeExtension, BuildingAssetData>
        where TypeObjectMap : INetObjectsMap
    {
        protected abstract string DataId { get; }
        protected abstract string MapId { get; }

        public override bool Load(BuildingInfo prefab, Dictionary<string, byte[]> userData, out BuildingAssetData data, string dataId = null, string mapId = null)
        {
            dataId ??= DataId;
            mapId ??= MapId;
            if (userData.TryGetValue(dataId, out byte[] rawData) && userData.TryGetValue(mapId, out byte[] map))
            {
                SingletonMod<TypeMod>.Logger.Debug($"Start load prefab data \"{prefab.name}\"");
                try
                {
                    var decompress = Loader.Decompress(rawData);
                    var config = XmlExtension.Parse(decompress);

                    SetMap(map, out var segments, out var nodes);
                    data = new BuildingAssetData(config, segments, nodes);
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
        public override void Save(BuildingInfo prefab, Dictionary<string, byte[]> userData, string dataId = null, string mapId = null)
        {
            SingletonMod<TypeMod>.Logger.Debug($"Start save prefab data \"{prefab.name}\"");
            try
            {
                var config = Loader.GetString(GetConfig());
                var data = Loader.Compress(config);

                dataId ??= DataId;
                mapId ??= MapId;
                userData[dataId] = data;
                userData[mapId] = GetMap();

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
    }

    public struct BuildingAssetData
    {
        public XElement Config { get; }
        public ushort[] Segments { get; }
        public ushort[] Nodes { get; }

        public BuildingAssetData(XElement config, ushort[] segments, ushort[] nodes)
        {
            Config = config;
            Segments = segments;
            Nodes = nodes;
        }
    }

    public abstract class BaseNetworkAssetDataExtension<TypeMod, TypeExtension, TypeObjectMap> : BaseNetworkDataExtension<TypeExtension, NetworkAssetData>
        where TypeMod : ICustomMod
        where TypeExtension : BaseNetworkDataExtension<TypeExtension, NetworkAssetData>
        where TypeObjectMap : INetObjectsMap
    {
        protected abstract string DataId { get; }
        protected abstract string MapId { get; }

        protected abstract XElement GetConfig(NetInfo prefab, out ushort segmentId, out ushort startNodeId, out ushort endNodeId);
        protected abstract TypeObjectMap CreateMap(bool isSimple);
        protected abstract bool PlaceAsset(XElement config, TypeObjectMap map);

        public override bool Load(NetInfo prefab, Dictionary<string, byte[]> userData, out NetworkAssetData data, string dataId = null, string mapId = null)
        {
            dataId ??= DataId;
            mapId ??= MapId;
            if (userData.TryGetValue(dataId, out byte[] rawData) && userData.TryGetValue(mapId, out byte[] map))
            {
                SingletonMod<TypeMod>.Logger.Debug($"Start load prefab data \"{prefab.name}\"");
                try
                {
                    var decompress = Loader.Decompress(rawData);
                    var config = XmlExtension.Parse(decompress);
                    data = new NetworkAssetData(config, GetUShort(map[0], map[1]), GetUShort(map[2], map[3]), GetUShort(map[4], map[5]));

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
        public override void Save(NetInfo prefab, Dictionary<string, byte[]> userData, string dataId = null, string mapId = null)
        {
            SingletonMod<TypeMod>.Logger.Debug($"Start save prefab data \"{prefab.name}\"");
            try
            {
                var config = GetConfig(prefab, out ushort segmentId, out ushort startNodeId, out ushort endNodeId);
                if (config != null)
                {
                    var data = Loader.Compress(Loader.GetString(config));

                    dataId ??= DataId;
                    userData[dataId] = data;

                    var map = new byte[6];
                    GetBytes(segmentId, out map[0], out map[1]);
                    GetBytes(startNodeId, out map[2], out map[3]);
                    GetBytes(endNodeId, out map[4], out map[5]);
                    mapId ??= MapId;
                    userData[mapId] = map;


                    SingletonMod<TypeMod>.Logger.Debug($"Prefab data was saved; Size = {data.Length} bytes");
                }
                else
                    SingletonMod<TypeMod>.Logger.Debug($"Nothing to save");
            }
            catch (Exception error)
            {
                SingletonMod<TypeMod>.Logger.Error("Could not save prefab data", error);
            }
        }

        public virtual bool OnPlaceAsset(NetInfo networkInfo, ushort segmentId, ushort startNodeId, ushort endNodeId)
        {
            if (AssetDatas.TryGetValue(networkInfo, out var assetData))
            {
                SingletonMod<TypeMod>.Logger.Debug($"Place asset {networkInfo.name}");

                var map = CreateMap(true);
                map.AddSegment(assetData.SegmentId, segmentId);
                map.AddNode(assetData.StartNodeId, startNodeId);
                map.AddNode(assetData.EndNodeId, endNodeId);

                return PlaceAsset(assetData.Config, map);
            }
            else
                return false;
        }
    }

    public struct NetworkAssetData
    {
        public XElement Config { get; }
        public ushort SegmentId { get; }
        public ushort StartNodeId { get; }
        public ushort EndNodeId { get; }


        public NetworkAssetData(XElement config, ushort segmentId, ushort startNodeId, ushort endNodeId)
        {
            Config = config;
            SegmentId = segmentId;
            StartNodeId = startNodeId;
            EndNodeId = endNodeId;
        }
    }
}
