using ICities;
using ModsCommon.UI;

namespace ModsCommon.Utilities
{
    public abstract class BaseLoadingExtension<TypeMod> : LoadingExtensionBase
        where TypeMod : BaseMod<TypeMod>
    {
        public sealed override void OnLevelLoaded(LoadMode mode)
        {
            SingletonMod<TypeMod>.Instance.Logger.Debug($"On level loaded");
            switch (mode)
            {
                case LoadMode.NewGame:
                case LoadMode.LoadGame:
                case LoadMode.NewGameFromScenario:
                case LoadMode.NewAsset:
                case LoadMode.LoadAsset:
                case LoadMode.NewMap:
                case LoadMode.LoadMap:
                    OnLoad();
                    break;
            }
        }
        public sealed override void OnLevelUnloading()
        {
            SingletonMod<TypeMod>.Instance.Logger.Debug($"On level unloading");
            ComponentPool.Clear();
            OnUnload();
        }
        protected virtual void OnLoad()
        {
            SingletonMod<TypeMod>.Instance.ShowWhatsNew();
            SingletonMod<TypeMod>.Instance.ShowBetaWarning();
        }
        protected virtual void OnUnload() { }
    }
}
