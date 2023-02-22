using ICities;
using ModsCommon.UI;

namespace ModsCommon.Utilities
{
    public abstract class BaseLoadingExtension<TypeMod> : LoadingExtensionBase
        where TypeMod : BaseMod<TypeMod>
    {
        public sealed override void OnCreated(ILoading loading)
        {
            SingletonMod<TypeMod>.Instance.Logger.Debug($"On create loading extension");
            base.OnCreated(loading);
            OnPreLoaded();
        }
        protected virtual void OnPreLoaded() { }

        public sealed override void OnReleased()
        {
            SingletonMod<TypeMod>.Instance.Logger.Debug($"On release loading extension");
            base.OnReleased();
            OnPreUnloaded();
        }
        protected virtual void OnPreUnloaded() { }

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
        protected virtual void OnLoad()
        {
            SingletonMod<TypeMod>.Instance.ShowWhatsNew();
            SingletonMod<TypeMod>.Instance.ShowBetaWarning();
        }

        public sealed override void OnLevelUnloading()
        {
            SingletonMod<TypeMod>.Instance.Logger.Debug($"On level unloading");
            OnUnload();
        }
        protected virtual void OnUnload() { }

    }
}
