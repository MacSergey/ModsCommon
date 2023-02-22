using ICities;
using System;
using System.Diagnostics;
using System.Xml.Linq;

namespace ModsCommon.Utilities
{
    public abstract class BaseSerializableDataExtension<TypeExtension, TypeMod> : SerializableDataExtensionBase
        where TypeExtension : BaseSerializableDataExtension<TypeExtension, TypeMod>
        where TypeMod : BaseMod<TypeMod>
    {
        protected abstract string Id { get; }
        protected abstract XElement GetSaveData();
        protected abstract void SetLoadData(XElement config);

        protected virtual void OnLoadSucces() { }
        protected virtual void OnSaveSucces() { }

        protected virtual void OnLoadFailed() { }
        protected virtual void OnSaveFailed(string config) { }

        protected virtual void OnCreate() { }
        protected virtual void OnRelease() { }

        public sealed override void OnCreated(ISerializableData serializableData)
        {
            SingletonMod<TypeMod>.Instance.Logger.Debug($"On create serialize data extension");
            base.OnCreated(serializableData);
            SingletonItem<TypeExtension>.Instance = (TypeExtension)this;
            OnCreate();
        }
        public sealed override void OnReleased()
        {
            SingletonMod<TypeMod>.Instance.Logger.Debug($"On release serialize data extension");
            base.OnReleased();
            OnRelease();
        }

        public override void OnLoadData()
        {
            SingletonMod<TypeMod>.Logger.Debug($"Start load map data");

            if (serializableDataManager.LoadData(Id) is byte[] data)
            {
                try
                {
                    var sw = Stopwatch.StartNew();

                    var decompress = Loader.Decompress(data);
#if DEBUG
                    SingletonMod<TypeMod>.Logger.Debug(decompress);
#endif
                    var config = XmlExtension.Parse(decompress);
                    SetLoadData(config);

                    sw.Stop();
                    SingletonMod<TypeMod>.Logger.Debug($"Map data was loaded in {sw.ElapsedMilliseconds}ms; Size = {data.Length} bytes");

                    OnLoadSucces();
                }
                catch (Exception error)
                {
                    SingletonMod<TypeMod>.Logger.Error("Could not load map data", error);
                    OnLoadFailed();
                }
            }
            else
                SingletonMod<TypeMod>.Logger.Debug($"Saved map data is not found");
        }
        public override void OnSaveData()
        {
            SingletonMod<TypeMod>.Logger.Debug($"Start save map data");

            var config = string.Empty;
            try
            {
                var sw = Stopwatch.StartNew();
                config = Loader.GetString(GetSaveData());
#if DEBUG
                SingletonMod<TypeMod>.Logger.Debug(config);
#endif
                var compress = Loader.Compress(config);
                serializableDataManager.SaveData(Id, compress);

                sw.Stop();
                SingletonMod<TypeMod>.Logger.Debug($"Map data saved in {sw.ElapsedMilliseconds}ms; Size = {compress.Length} bytes");

                OnSaveSucces();
            }
            catch (Exception error)
            {
                SingletonMod<TypeMod>.Logger.Error("Save map data failed", error);
                OnSaveFailed(config);
                throw;
            }
        }
    }
}
