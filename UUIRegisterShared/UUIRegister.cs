using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnifiedUI.Helpers;

namespace ModsCommon
{
    public abstract partial class BaseTool<TypeMod, TypeTool> : ToolBase, ITool, IUUITool
    where TypeMod : ICustomMod
    where TypeTool : ToolBase, ITool
    {
        private static PluginSearcher UUISearcher { get; } = PluginUtilities.GetSearcher("Unified UI", 2255219025ul) & new VersionSearcher(new Version(1, 3), (m, s) => m >= s);
        public static bool IsUUIEnabled => UUISearcher.GetPlugin()?.isEnabled == true;

        public bool UUIRegistered { get; private set; }
        public UIComponent UUIButton { get; private set; }

        protected abstract UITextureAtlas UUIAtlas { get; }
        protected abstract string UUINormalSprite { get; }
        protected abstract string UUIHoveredSprite { get; }
        protected abstract string UUIPressedSprite { get; }
        protected abstract string UUIDisabledSprite { get; }

        public virtual void RegisterUUI()
        {
            if (IsUUIEnabled)
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
                    UUIButton = UUIHelpers.RegisterToolButton(SingletonMod<TypeMod>.Name, "MacSergeyMods", string.Empty, uuiSprites, tool, tool.Activation.InputKey, tool.Shortcuts.Select(s => s.InputKey));

                    UUIButton.eventTooltipEnter += (UIComponent component, UIMouseEventParameter eventParam) => component.tooltip = ToolTip;
                    UUIRegistered = true;
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
    public abstract class BaseUUIThreadingExtension<TypeTool> : BaseThreadingExtension<TypeTool>
    where TypeTool : IUUITool
    {
        protected override bool Detected(TypeTool instance) => !instance.UUIRegistered && base.Detected(instance);
    }
    public abstract class BaseUUIToolLoadingExtension<TypeTool> : BaseToolLoadingExtension<TypeTool>
        where TypeTool : IUUITool
    {
        protected override void OnLoad()
        {
            base.OnLoad();
            SingletonTool<TypeTool>.Instance.RegisterUUI();
        }
    }
}
