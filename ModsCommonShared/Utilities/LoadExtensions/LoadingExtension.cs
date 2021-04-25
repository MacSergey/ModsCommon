using ICities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModsCommon.Utilities
{
    public abstract class BaseLoadingExtension<TypeMod> : LoadingExtensionBase
        where TypeMod : BaseMod<TypeMod>
    {
        public override void OnLevelLoaded(LoadMode mode)
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

                    SingletonMod<TypeMod>.Instance.ShowWhatsNew();
                    SingletonMod<TypeMod>.Instance.ShowBetaWarning();
                    break;
            }
        }
        protected virtual void OnLoad() { }
    }
}
