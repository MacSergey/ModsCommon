namespace ModsCommon
{
	public class CommonLocalize
	{
		public static System.Globalization.CultureInfo Culture {get; set;}
		public static ModsCommon.LocaleManager LocaleManager {get;} = new ModsCommon.LocaleManager();

		//Detected сonflicting mods.
		public static string Dependency_Conflict => LocaleManager.GetString("Dependency_Conflict", Culture);

		//Disable
		public static string Dependency_Disable => LocaleManager.GetString("Dependency_Disable", Culture);

		//Disable {0}
		public static string Dependency_DisableMod => LocaleManager.GetString("Dependency_DisableMod", Culture);

		//Enable
		public static string Dependency_Enable => LocaleManager.GetString("Dependency_Enable", Culture);

		//Enable {0}
		public static string Dependency_EnableMod => LocaleManager.GetString("Dependency_EnableMod", Culture);

		//Get
		public static string Dependency_Get => LocaleManager.GetString("Dependency_Get", Culture);

		//Detected missing dependencies.
		public static string Dependency_Missing => LocaleManager.GetString("Dependency_Missing", Culture);

		//Detected missing dependencies and сonflicting mods...
		public static string Dependency_MissingAndConflict => LocaleManager.GetString("Dependency_MissingAndConflict", Culture);

		//To continue using {0} need to fix these issues, ot...
		public static string Dependency_NeedFix => LocaleManager.GetString("Dependency_NeedFix", Culture);

		//All issues are fixed, {0} can be enabled.
		public static string Dependency_NoIssues => LocaleManager.GetString("Dependency_NoIssues", Culture);

		//Remove
		public static string Dependency_Remove => LocaleManager.GetString("Dependency_Remove", Culture);

		//Subscribe
		public static string Dependency_Subscribe => LocaleManager.GetString("Dependency_Subscribe", Culture);

		//Unsubscribe
		public static string Dependency_Unsubscribe => LocaleManager.GetString("Dependency_Unsubscribe", Culture);

		//Scroll the wheel to change
		public static string FieldPanel_ScrollWheel => LocaleManager.GetString("FieldPanel_ScrollWheel", Culture);

		//Alt
		public static string Key_Alt => LocaleManager.GetString("Key_Alt", Culture);

		//Control
		public static string Key_Control => LocaleManager.GetString("Key_Control", Culture);

		//Enter
		public static string Key_Enter => LocaleManager.GetString("Key_Enter", Culture);

		//Shift
		public static string Key_Shift => LocaleManager.GetString("Key_Shift", Culture);

		//Tab
		public static string Key_Tab => LocaleManager.GetString("Key_Tab", Culture);

		//Scroll the wheel to change
		public static string ListPanel_ScrollWheel => LocaleManager.GetString("ListPanel_ScrollWheel", Culture);

		//Cancel
		public static string MessageBox_Cancel => LocaleManager.GetString("MessageBox_Cancel", Culture);

		//Don't show again
		public static string MessageBox_DontShowAgain => LocaleManager.GetString("MessageBox_DontShowAgain", Culture);

		//More info
		public static string MessageBox_MoreInfo => LocaleManager.GetString("MessageBox_MoreInfo", Culture);

		//No
		public static string MessageBox_No => LocaleManager.GetString("MessageBox_No", Culture);

		//OK
		public static string MessageBox_OK => LocaleManager.GetString("MessageBox_OK", Culture);

		//Yes
		public static string MessageBox_Yes => LocaleManager.GetString("MessageBox_Yes", Culture);

		//I agree
		public static string Mod_BetaWarningAgree => LocaleManager.GetString("Mod_BetaWarningAgree", Culture);

		//BETA version warning
		public static string Mod_BetaWarningCaption => LocaleManager.GetString("Mod_BetaWarningCaption", Culture);

		//Get stable version
		public static string Mod_BetaWarningGetStable => LocaleManager.GetString("Mod_BetaWarningGetStable", Culture);

		//You are using the BETA version of {0}. It is for t...
		public static string Mod_BetaWarningMessage => LocaleManager.GetString("Mod_BetaWarningMessage", Culture);

		//This is a BETA version, errors may occur and stabi...
		public static string Mod_DescriptionBeta => LocaleManager.GetString("Mod_DescriptionBeta", Culture);

		//Linux version of Cities Skylines doesn't have all ...
		public static string Mod_LinuxWarning => LocaleManager.GetString("Mod_LinuxWarning", Culture);

		//Mod loaded with errors.
		public static string Mod_LoadedWithErrors => LocaleManager.GetString("Mod_LoadedWithErrors", Culture);

		//Game Language
		public static string Mod_LocaleGame => LocaleManager.GetString("Mod_LocaleGame", Culture);

		//Czech
		public static string Mod_Locale_cs => LocaleManager.GetString("Mod_Locale_cs", Culture);

		//Danish
		public static string Mod_Locale_da => LocaleManager.GetString("Mod_Locale_da", Culture);

		//German
		public static string Mod_Locale_de => LocaleManager.GetString("Mod_Locale_de", Culture);

		//English US
		public static string Mod_Locale_en => LocaleManager.GetString("Mod_Locale_en", Culture);

		//English UK
		public static string Mod_Locale_en_gb => LocaleManager.GetString("Mod_Locale_en-gb", Culture);

		//Spanish
		public static string Mod_Locale_es => LocaleManager.GetString("Mod_Locale_es", Culture);

		//Finnish
		public static string Mod_Locale_fi => LocaleManager.GetString("Mod_Locale_fi", Culture);

		//French
		public static string Mod_Locale_fr => LocaleManager.GetString("Mod_Locale_fr", Culture);

		//Hungarian
		public static string Mod_Locale_hu => LocaleManager.GetString("Mod_Locale_hu", Culture);

		//Indonesian
		public static string Mod_Locale_id => LocaleManager.GetString("Mod_Locale_id", Culture);

		//Italian
		public static string Mod_Locale_it => LocaleManager.GetString("Mod_Locale_it", Culture);

		//Japanese
		public static string Mod_Locale_ja => LocaleManager.GetString("Mod_Locale_ja", Culture);

		//Korean
		public static string Mod_Locale_ko => LocaleManager.GetString("Mod_Locale_ko", Culture);

		//Malay
		public static string Mod_Locale_mr => LocaleManager.GetString("Mod_Locale_mr", Culture);

		//Dutch
		public static string Mod_Locale_nl => LocaleManager.GetString("Mod_Locale_nl", Culture);

		//Polish
		public static string Mod_Locale_pl => LocaleManager.GetString("Mod_Locale_pl", Culture);

		//Portuguese
		public static string Mod_Locale_pt => LocaleManager.GetString("Mod_Locale_pt", Culture);

		//Romanian
		public static string Mod_Locale_ro => LocaleManager.GetString("Mod_Locale_ro", Culture);

		//Russian
		public static string Mod_Locale_ru => LocaleManager.GetString("Mod_Locale_ru", Culture);

		//Turkish
		public static string Mod_Locale_tr => LocaleManager.GetString("Mod_Locale_tr", Culture);

		//Chinese Simplified
		public static string Mod_Locale_zh_cn => LocaleManager.GetString("Mod_Locale_zh-cn", Culture);

		//Chinese Traditional
		public static string Mod_Locale_zh_tw => LocaleManager.GetString("Mod_Locale_zh-tw", Culture);

		//Status: {0}
		public static string Mod_Status => LocaleManager.GetString("Mod_Status", Culture);

		//Game version is out of date
		public static string Mod_Status_GameOutOfDate => LocaleManager.GetString("Mod_Status_GameOutOfDate", Culture);

		//Loading error
		public static string Mod_Status_LoadingError => LocaleManager.GetString("Mod_Status_LoadingError", Culture);

		//Mod version is out of date
		public static string Mod_Status_ModOutOfDate => LocaleManager.GetString("Mod_Status_ModOutOfDate", Culture);

		//Operate normally
		public static string Mod_Status_OperateNormally => LocaleManager.GetString("Mod_Status_OperateNormally", Culture);

		//Unknown
		public static string Mod_Status_Unknown => LocaleManager.GetString("Mod_Status_Unknown", Culture);

		//Support
		public static string Mod_Support => LocaleManager.GetString("Mod_Support", Culture);

		//Version: {0}
		public static string Mod_Version => LocaleManager.GetString("Mod_Version", Culture);

		//The game version is out of date.
		public static string Mod_VersionWarning_GameOutOfDate => LocaleManager.GetString("Mod_VersionWarning_GameOutOfDate", Culture);

		//The mod version is out of date.
		public static string Mod_VersionWarning_ModOutOfDate => LocaleManager.GetString("Mod_VersionWarning_ModOutOfDate", Culture);

		//What's new in {0}
		public static string Mod_WhatsNewCaption => LocaleManager.GetString("Mod_WhatsNewCaption", Culture);

		//[BETA] Look for actual beta change log on Discord
		public static string Mod_WhatsNewMessageBeta => LocaleManager.GetString("Mod_WhatsNewMessageBeta", Culture);

		//Version {0}
		public static string Mod_WhatsNewVersion => LocaleManager.GetString("Mod_WhatsNewVersion", Culture);

		//Additional
		public static string Panel_Additional => LocaleManager.GetString("Panel_Additional", Culture);

		//Cancel
		public static string Settings_Cancel => LocaleManager.GetString("Settings_Cancel", Culture);

		//Change log
		public static string Settings_ChangeLog => LocaleManager.GetString("Settings_ChangeLog", Culture);

		//For Linux users
		public static string Settings_ForLinuxUsers => LocaleManager.GetString("Settings_ForLinuxUsers", Culture);

		//General
		public static string Settings_General => LocaleManager.GetString("Settings_General", Culture);

		//General
		public static string Settings_GeneralTab => LocaleManager.GetString("Settings_GeneralTab", Culture);

		//Language
		public static string Settings_Language => LocaleManager.GetString("Settings_Language", Culture);

		//Notifications
		public static string Settings_Notifications => LocaleManager.GetString("Settings_Notifications", Culture);

		//Press any key
		public static string Settings_PressAnyKey => LocaleManager.GetString("Settings_PressAnyKey", Culture);

		//Activate tool
		public static string Settings_ShortcutActivateTool => LocaleManager.GetString("Settings_ShortcutActivateTool", Culture);

		//Shortcuts
		public static string Settings_Shortcuts => LocaleManager.GetString("Settings_Shortcuts", Culture);

		//Selection step over
		public static string Settings_ShortcutSelectionStepOver => LocaleManager.GetString("Settings_ShortcutSelectionStepOver", Culture);

		//Notification only about major updates
		public static string Settings_ShowOnlyMajor => LocaleManager.GetString("Settings_ShowOnlyMajor", Culture);

		//Show tooltips
		public static string Settings_ShowTooltips => LocaleManager.GetString("Settings_ShowTooltips", Culture);

		//Show what's new
		public static string Settings_ShowWhatsNew => LocaleManager.GetString("Settings_ShowWhatsNew", Culture);

		//Solve crash on Linux
		public static string Settings_SolveCrashOnLinux => LocaleManager.GetString("Settings_SolveCrashOnLinux", Culture);

		//Support
		public static string Settings_SupportTab => LocaleManager.GetString("Settings_SupportTab", Culture);

		//Tool button
		public static string Settings_ToolButton => LocaleManager.GetString("Settings_ToolButton", Culture);

		//Show in both places
		public static string Settings_ToolButtonBoth => LocaleManager.GetString("Settings_ToolButtonBoth", Culture);

		//Show only in toolbar
		public static string Settings_ToolButtonOnlyToolbar => LocaleManager.GetString("Settings_ToolButtonOnlyToolbar", Culture);

		//Show only in UnifiedUI
		public static string Settings_ToolButtonOnlyUUI => LocaleManager.GetString("Settings_ToolButtonOnlyUUI", Culture);

		//You can help improve the mod translation
		public static string Settings_TranslationDescription => LocaleManager.GetString("Settings_TranslationDescription", Culture);

		//Improve translation
		public static string Settings_TranslationImprove => LocaleManager.GetString("Settings_TranslationImprove", Culture);

		//Add new language
		public static string Settings_TranslationNew => LocaleManager.GetString("Settings_TranslationNew", Culture);

		//Troubleshooting
		public static string Settings_Troubleshooting => LocaleManager.GetString("Settings_Troubleshooting", Culture);

		//If you like {0}, you can support author
		public static string Setting_Donate => LocaleManager.GetString("Setting_Donate", Culture);

		//Press {0} to
		public static string Tool_InfoSelectionStepOver => LocaleManager.GetString("Tool_InfoSelectionStepOver", Culture);

		//FIXED
		public static string WhatsNew_FIXED => LocaleManager.GetString("WhatsNew_FIXED", Culture);

		//NEW
		public static string WhatsNew_NEW => LocaleManager.GetString("WhatsNew_NEW", Culture);

		//REMOVED
		public static string WhatsNew_REMOVED => LocaleManager.GetString("WhatsNew_REMOVED", Culture);

		//REVERTED
		public static string WhatsNew_REVERTED => LocaleManager.GetString("WhatsNew_REVERTED", Culture);

		//TRANSLATION
		public static string WhatsNew_TRANSLATION => LocaleManager.GetString("WhatsNew_TRANSLATION", Culture);

		//UPDATED
		public static string WhatsNew_UPDATED => LocaleManager.GetString("WhatsNew_UPDATED", Culture);

		//WARNING
		public static string WhatsNew_WARNING => LocaleManager.GetString("WhatsNew_WARNING", Culture);
	}
}