using ColossalFramework;
using ColossalFramework.UI;
using ModsCommon.UI;
using ModsCommon.Utilities;
using System;
using System.Linq;
using UnifiedUI.Helpers;
using ModsCommon.Settings;

namespace ModsCommon
{
    public partial interface ITool
    {
        public UIComponent UUIButton { get; }
        public bool CanBeShownUUIButton { get; }

        public void RegisterUUI();
        public void SetButtonsVisibility(bool toolButton, bool uuiButton);
    }
    public abstract partial class BaseTool<TypeMod, TypeTool> : ToolBase, ITool
    where TypeMod : ICustomMod
    where TypeTool : ToolBase, ITool
    {
        private static PluginSearcher UUISearcher { get; } = PluginUtilities.GetSearcher("Unified UI", 2255219025ul, 2966990700ul) & new VersionSearcher(new Version(1, 3), (m, s) => m >= s);
        public static bool IsUUIEnabled => UUISearcher.GetPlugin()?.isEnabled == true;

        public UIComponent UUIButton { get; private set; }

        protected abstract UITextureAtlas UUIAtlas { get; }
        protected abstract string UUINormalSprite { get; }
        protected abstract string UUIHoveredSprite { get; }
        protected abstract string UUIPressedSprite { get; }
        protected abstract string UUIDisabledSprite { get; }


        private bool showUUIButton;
        private bool canBeShownUUIButton;

        private bool ShowUUIButton
        {
            get => showUUIButton;
            set
            {
                if (value != showUUIButton)
                {
                    showUUIButton = value;
                    SetUUIButtonVisibility();
                }
            }
        }
        public bool CanBeShownUUIButton
        {
            get => canBeShownUUIButton;
            private set
            {
                if (value != canBeShownUUIButton)
                {
                    canBeShownUUIButton = value;
                    SetUUIButtonVisibility();
                }
            }
        }
        private void SetUUIButtonVisibility()
        {
            if (UUIButton != null)
                UUIButton.isVisible = ShowUUIButton && CanBeShownUUIButton;
        }
        public void SetButtonsVisibility(bool toolButton, bool uuiButton)
        {
            if(toolButton && uuiButton)
            {
                ShowToolButton = true;
                ShowUUIButton = true;
            }
            else if(toolButton)
            {
                ShowToolButton = true;
                ShowUUIButton = false;
            }
            else if(uuiButton)
            {
                ShowToolButton = !CanBeShownUUIButton;
                ShowUUIButton = true;
            }
        }

        public virtual void RegisterUUI()
        {
            if (IsUUIEnabled)
            {
                SingletonMod<TypeMod>.Logger.Debug("Register button in UUI");

                try
                {
                    var tool = SingletonTool<TypeTool>.Instance;
                    var uuiSprites = new UUISprites()
                    {
                        Atlas = UUIAtlas,
                        NormalSprite = UUINormalSprite,
                        HoveredSprite = UUIHoveredSprite,
                        PressedSprite = UUIPressedSprite,
                        DisabledSprite = UUIDisabledSprite,
                    };
                    var hotkeys = new UUIHotKeys() { ActivationKey = tool.Activation.InputKey, };
                    foreach (var inToolKey in tool.Shortcuts.Select(s => s.InputKey))
                        hotkeys.AddInToolKey(inToolKey);

                    UUIButton = UUIHelpers.RegisterToolButton(SingletonMod<TypeMod>.Name, "MacSergeyMods", string.Empty, tool, uuiSprites, hotkeys);

                    UUIButton.eventTooltipEnter += (UIComponent component, UIMouseEventParameter eventParam) => component.tooltip = ToolTip;
                    CanBeShownUUIButton = true;
                    SetButtonsVisibility(BaseSettings<TypeMod>.IsToolbarButtonVisible, BaseSettings<TypeMod>.IsUUIButtonVisible);
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
    where TypeTool : ITool
    {
        protected override bool Detected(TypeTool instance) => !instance.CanBeShownUUIButton && base.Detected(instance);
    }
    public abstract class BaseUUIToolLoadingExtension<TypeTool> : BaseToolLoadingExtension<TypeTool>
        where TypeTool : ITool
    {
        protected override void OnLoad()
        {
            base.OnLoad();
            SingletonTool<TypeTool>.Instance.RegisterUUI();
        }
    }
}

namespace ModsCommon.Settings
{  
    public abstract partial class BaseSettings<TypeMod>
        where TypeMod : ICustomMod
    {
        public static SavedInt ToolButtonVisible { get; } = new SavedInt(nameof(ToolButtonVisible), SettingsFile, (int)ButtonVisible.Both, true);
        public static bool IsUUIButtonVisible => ToolButtonVisible != (int)ButtonVisible.OnlyToolbar;
        public static bool IsToolbarButtonVisible => ToolButtonVisible != (int)ButtonVisible.OnlyUUI;

        public static void AddToolButton<TypeTool, TypeButton>(UIComponent group)
            where TypeTool : ToolBase, ITool
            where TypeButton : ToolButton<TypeTool>
        {
            if (BaseTool<TypeMod, TypeTool>.IsUUIEnabled)
                group.AddTogglePanel(CommonLocalize.Settings_ToolButton, ToolButtonVisible, new string[] { CommonLocalize.Settings_ToolButtonOnlyToolbar, CommonLocalize.Settings_ToolButtonOnlyUUI, CommonLocalize.Settings_ToolButtonBoth }, OnButtonVisibleChanged);

            static void OnButtonVisibleChanged(int value) => SingletonTool<TypeTool>.Instance.SetButtonsVisibility(IsToolbarButtonVisible, IsUUIButtonVisible);
        }

        private enum ButtonVisible
        {
            OnlyToolbar,
            OnlyUUI,
            Both,
        }
    }
}
