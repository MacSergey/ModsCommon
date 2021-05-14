using CitiesHarmony.API;
using ColossalFramework.UI;
using HarmonyLib;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ModsCommon
{
    public abstract partial class BasePatcherMod<TypeMod> : BaseMod<TypeMod>
        where TypeMod : BaseMod<TypeMod>
    {
        protected bool AssetDataExtensionFix<TypeExtension>()
            where TypeExtension : BaseAssetDataExtension<TypeExtension>
        {
            return AddPostfix(typeof(TypeExtension), nameof(BaseAssetDataExtension<TypeExtension>.LoadAssetPanelOnLoadPostfix), typeof(LoadAssetPanel), nameof(LoadAssetPanel.OnLoad));
        }
    }
}
