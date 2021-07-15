using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnifiedUI.Helpers;

namespace ModsCommon
{
    public abstract partial class BaseTool<TypeMod, TypeTool> : ToolBase, ITool
    where TypeMod : ICustomMod
    where TypeTool : ToolBase, ITool
    {
        private static PluginSearcher UUISearcher { get; } = PluginUtilities.GetSearcher("Unified UI", 2255219025ul) & new VersionSearcher(new Version(1, 3), (m, s) => m >= s);
        public static bool IsUUIInstaled => UUISearcher.GetPlugin() != null;

        protected abstract UITextureAtlas UUIAtlas { get; }
        protected abstract string UUINormalSprite { get; }
        protected abstract string UUIHoveredSprite { get; }
        protected abstract string UUIPressedSprite { get; }
        protected abstract string UUIDisabledSprite { get; }

        public void RegisterUUI()
        {
            if (IsUUIInstaled)
            {
                SingletonMod<TypeMod>.Logger.Debug("Register button in UUI");

                try
                {
                    var uuiSprites = new UUIHelpers.UUISprites()
                    {
                        Atlas = UUIAtlas,
                        NormalSprite = UUINormalSprite,
                        HoveredSprite = UUIHoveredSprite,
                        PressedSprite = UUIPressedSprite,
                        DisabledSprite = UUIDisabledSprite,
                    };

                    var tool = SingletonTool<TypeTool>.Instance;
                    var button = UUIHelpers.RegisterToolButton(SingletonMod<TypeMod>.Name, "MacSergeyMods", string.Empty, uuiSprites, tool, tool.Activation.InputKey, tool.Shortcuts.Select(s => s.InputKey));

                    button.eventTooltipEnter += OnTooltipEnter;

                    void OnTooltipEnter(UIComponent component, UIMouseEventParameter eventParam)
                    {
                        component.tooltip = Activation.NotSet ? ModInstance.NameRaw : $"{ModInstance.NameRaw} ({Activation})";
                    }
                }
                catch (Exception error)
                {
                    SingletonMod<TypeMod>.Logger.Error("Failed to register button in UUI", error);
                }
            }
            else
                SingletonMod<TypeMod>.Logger.Debug("UUI not found");
        }
    }
}
