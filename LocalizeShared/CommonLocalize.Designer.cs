namespace ModsCommon
{
	public class CommonLocalize
	{
		public static System.Globalization.CultureInfo Culture {get; set;}
		public static ModsCommon.LocalizeManager LocaleManager {get;} = new ModsCommon.LocalizeManager("CommonLocalize", typeof(CommonLocalize).Assembly);

		/// <summary>
		/// Detected сonflicting mods.
		/// </summary>
		public static string Dependency_Conflict => LocaleManager.GetString("Dependency_Conflict", Culture);

		/// <summary>
		/// Disable
		/// </summary>
		public static string Dependency_Disable => LocaleManager.GetString("Dependency_Disable", Culture);

		/// <summary>
		/// Disable {0}
		/// </summary>
		public static string Dependency_DisableMod => LocaleManager.GetString("Dependency_DisableMod", Culture);

		/// <summary>
		/// Enable
		/// </summary>
		public static string Dependency_Enable => LocaleManager.GetString("Dependency_Enable", Culture);

		/// <summary>
		/// Enable {0}
		/// </summary>
		public static string Dependency_EnableMod => LocaleManager.GetString("Dependency_EnableMod", Culture);

		/// <summary>
		/// Get
		/// </summary>
		public static string Dependency_Get => LocaleManager.GetString("Dependency_Get", Culture);

		/// <summary>
		/// Detected missing dependencies.
		/// </summary>
		public static string Dependency_Missing => LocaleManager.GetString("Dependency_Missing", Culture);

		/// <summary>
		/// Detected missing dependencies and сonflicting mods.
		/// </summary>
		public static string Dependency_MissingAndConflict => LocaleManager.GetString("Dependency_MissingAndConflict", Culture);

		/// <summary>
		/// To continue using {0} need to fix these issues, otherwise the mod will be disabled.
		/// </summary>
		public static string Dependency_NeedFix => LocaleManager.GetString("Dependency_NeedFix", Culture);

		/// <summary>
		/// All issues are fixed, {0} can be enabled.
		/// </summary>
		public static string Dependency_NoIssues => LocaleManager.GetString("Dependency_NoIssues", Culture);

		/// <summary>
		/// Remove
		/// </summary>
		public static string Dependency_Remove => LocaleManager.GetString("Dependency_Remove", Culture);

		/// <summary>
		/// Subscribe
		/// </summary>
		public static string Dependency_Subscribe => LocaleManager.GetString("Dependency_Subscribe", Culture);

		/// <summary>
		/// Unsubscribe
		/// </summary>
		public static string Dependency_Unsubscribe => LocaleManager.GetString("Dependency_Unsubscribe", Culture);

		/// <summary>
		/// Scroll the wheel to change
		/// </summary>
		public static string FieldPanel_ScrollWheel => LocaleManager.GetString("FieldPanel_ScrollWheel", Culture);

		/// <summary>
		/// Alt
		/// </summary>
		public static string Key_Alt => LocaleManager.GetString("Key_Alt", Culture);

		/// <summary>
		/// Control
		/// </summary>
		public static string Key_Control => LocaleManager.GetString("Key_Control", Culture);

		/// <summary>
		/// Enter
		/// </summary>
		public static string Key_Enter => LocaleManager.GetString("Key_Enter", Culture);

		/// <summary>
		/// Shift
		/// </summary>
		public static string Key_Shift => LocaleManager.GetString("Key_Shift", Culture);

		/// <summary>
		/// Tab
		/// </summary>
		public static string Key_Tab => LocaleManager.GetString("Key_Tab", Culture);

		/// <summary>
		/// Scroll the wheel to change
		/// </summary>
		public static string ListPanel_ScrollWheel => LocaleManager.GetString("ListPanel_ScrollWheel", Culture);

		/// <summary>
		/// Cancel
		/// </summary>
		public static string MessageBox_Cancel => LocaleManager.GetString("MessageBox_Cancel", Culture);

		/// <summary>
		/// Don't show again
		/// </summary>
		public static string MessageBox_DontShowAgain => LocaleManager.GetString("MessageBox_DontShowAgain", Culture);

		/// <summary>
		/// More info
		/// </summary>
		public static string MessageBox_MoreInfo => LocaleManager.GetString("MessageBox_MoreInfo", Culture);

		/// <summary>
		/// No
		/// </summary>
		public static string MessageBox_No => LocaleManager.GetString("MessageBox_No", Culture);

		/// <summary>
		/// OK
		/// </summary>
		public static string MessageBox_OK => LocaleManager.GetString("MessageBox_OK", Culture);

		/// <summary>
		/// Yes
		/// </summary>
		public static string MessageBox_Yes => LocaleManager.GetString("MessageBox_Yes", Culture);

		/// <summary>
		/// I agree
		/// </summary>
		public static string Mod_BetaWarningAgree => LocaleManager.GetString("Mod_BetaWarningAgree", Culture);

		/// <summary>
		/// BETA version warning
		/// </summary>
		public static string Mod_BetaWarningCaption => LocaleManager.GetString("Mod_BetaWarningCaption", Culture);

		/// <summary>
		/// Get stable version
		/// </summary>
		public static string Mod_BetaWarningGetStable => LocaleManager.GetString("Mod_BetaWarningGetStable", Culture);

		/// <summary>
		/// You are using the BETA version of {0}. It is for testing new features and may contain errors. If you
		/// </summary>
		public static string Mod_BetaWarningMessage => LocaleManager.GetString("Mod_BetaWarningMessage", Culture);

		/// <summary>
		/// This is a BETA version, errors may occur and stability is not guaranteed!
		/// </summary>
		public static string Mod_DescriptionBeta => LocaleManager.GetString("Mod_DescriptionBeta", Culture);

		/// <summary>
		/// Linux version of Cities Skylines doesn't have all necessary libraries for the mod to work correctly.
		/// </summary>
		public static string Mod_LinuxWarning => LocaleManager.GetString("Mod_LinuxWarning", Culture);

		/// <summary>
		/// Mod loaded with errors.
		/// </summary>
		public static string Mod_LoadedWithErrors => LocaleManager.GetString("Mod_LoadedWithErrors", Culture);

		/// <summary>
		/// Game Language
		/// </summary>
		public static string Mod_LocaleGame => LocaleManager.GetString("Mod_LocaleGame", Culture);

		/// <summary>
		/// Czech
		/// </summary>
		public static string Mod_Locale_cs_cz => LocaleManager.GetString("Mod_Locale_cs-cz", Culture);

		/// <summary>
		/// Danish
		/// </summary>
		public static string Mod_Locale_da_dk => LocaleManager.GetString("Mod_Locale_da-dk", Culture);

		/// <summary>
		/// German
		/// </summary>
		public static string Mod_Locale_de_de => LocaleManager.GetString("Mod_Locale_de-de", Culture);

		/// <summary>
		/// English US
		/// </summary>
		public static string Mod_Locale_en_us => LocaleManager.GetString("Mod_Locale_en-us", Culture);

		/// <summary>
		/// English UK
		/// </summary>
		public static string Mod_Locale_en_gb => LocaleManager.GetString("Mod_Locale_en-gb", Culture);

		/// <summary>
		/// Spanish
		/// </summary>
		public static string Mod_Locale_es_es => LocaleManager.GetString("Mod_Locale_es-es", Culture);

		/// <summary>
		/// Finnish
		/// </summary>
		public static string Mod_Locale_fi_fi => LocaleManager.GetString("Mod_Locale_fi-fi", Culture);

		/// <summary>
		/// French
		/// </summary>
		public static string Mod_Locale_fr_fr => LocaleManager.GetString("Mod_Locale_fr-fr", Culture);

		/// <summary>
		/// Hungarian
		/// </summary>
		public static string Mod_Locale_hu_hu => LocaleManager.GetString("Mod_Locale_hu-hu", Culture);

		/// <summary>
		/// Indonesian
		/// </summary>
		public static string Mod_Locale_id_id => LocaleManager.GetString("Mod_Locale_id-id", Culture);

		/// <summary>
		/// Italian
		/// </summary>
		public static string Mod_Locale_it_it => LocaleManager.GetString("Mod_Locale_it-it", Culture);

		/// <summary>
		/// Japanese
		/// </summary>
		public static string Mod_Locale_ja_jp => LocaleManager.GetString("Mod_Locale_ja-jp", Culture);

		/// <summary>
		/// Korean
		/// </summary>
		public static string Mod_Locale_ko_kr => LocaleManager.GetString("Mod_Locale_ko-kr", Culture);

		/// <summary>
		/// Malay
		/// </summary>
		public static string Mod_Locale_mr_mr => LocaleManager.GetString("Mod_Locale_mr-mr", Culture);

		/// <summary>
		/// Dutch
		/// </summary>
		public static string Mod_Locale_nl_nl => LocaleManager.GetString("Mod_Locale_nl-nl", Culture);

		/// <summary>
		/// Polish
		/// </summary>
		public static string Mod_Locale_pl_pl => LocaleManager.GetString("Mod_Locale_pl-pl", Culture);

		/// <summary>
		/// Portuguese
		/// </summary>
		public static string Mod_Locale_pt_pt => LocaleManager.GetString("Mod_Locale_pt-pt", Culture);

		/// <summary>
		/// Romanian
		/// </summary>
		public static string Mod_Locale_ro_ro => LocaleManager.GetString("Mod_Locale_ro-ro", Culture);

		/// <summary>
		/// Russian
		/// </summary>
		public static string Mod_Locale_ru_ru => LocaleManager.GetString("Mod_Locale_ru-ru", Culture);

		/// <summary>
		/// Turkish
		/// </summary>
		public static string Mod_Locale_tr_tr => LocaleManager.GetString("Mod_Locale_tr-tr", Culture);

		/// <summary>
		/// Chinese Simplified
		/// </summary>
		public static string Mod_Locale_zh_cn => LocaleManager.GetString("Mod_Locale_zh-cn", Culture);

		/// <summary>
		/// Chinese Traditional
		/// </summary>
		public static string Mod_Locale_zh_tw => LocaleManager.GetString("Mod_Locale_zh-tw", Culture);

		/// <summary>
		/// Status: {0}
		/// </summary>
		public static string Mod_Status => LocaleManager.GetString("Mod_Status", Culture);

		/// <summary>
		/// Game version is out of date
		/// </summary>
		public static string Mod_Status_GameOutOfDate => LocaleManager.GetString("Mod_Status_GameOutOfDate", Culture);

		/// <summary>
		/// Loading error
		/// </summary>
		public static string Mod_Status_LoadingError => LocaleManager.GetString("Mod_Status_LoadingError", Culture);

		/// <summary>
		/// Mod version is out of date
		/// </summary>
		public static string Mod_Status_ModOutOfDate => LocaleManager.GetString("Mod_Status_ModOutOfDate", Culture);

		/// <summary>
		/// Operate normally
		/// </summary>
		public static string Mod_Status_OperateNormally => LocaleManager.GetString("Mod_Status_OperateNormally", Culture);

		/// <summary>
		/// Unknown
		/// </summary>
		public static string Mod_Status_Unknown => LocaleManager.GetString("Mod_Status_Unknown", Culture);

		/// <summary>
		/// Support
		/// </summary>
		public static string Mod_Support => LocaleManager.GetString("Mod_Support", Culture);

		/// <summary>
		/// Version: {0}
		/// </summary>
		public static string Mod_Version => LocaleManager.GetString("Mod_Version", Culture);

		/// <summary>
		/// The game version is out of date.
		/// </summary>
		public static string Mod_VersionWarning_GameOutOfDate => LocaleManager.GetString("Mod_VersionWarning_GameOutOfDate", Culture);

		/// <summary>
		/// The mod version is out of date.
		/// </summary>
		public static string Mod_VersionWarning_ModOutOfDate => LocaleManager.GetString("Mod_VersionWarning_ModOutOfDate", Culture);

		/// <summary>
		/// What's new in {0}
		/// </summary>
		public static string Mod_WhatsNewCaption => LocaleManager.GetString("Mod_WhatsNewCaption", Culture);

		/// <summary>
		/// [BETA] Look for actual beta change log on Discord
		/// </summary>
		public static string Mod_WhatsNewMessageBeta => LocaleManager.GetString("Mod_WhatsNewMessageBeta", Culture);

		/// <summary>
		/// Version {0}
		/// </summary>
		public static string Mod_WhatsNewVersion => LocaleManager.GetString("Mod_WhatsNewVersion", Culture);

		/// <summary>
		/// Additional
		/// </summary>
		public static string Panel_Additional => LocaleManager.GetString("Panel_Additional", Culture);

		/// <summary>
		/// Cancel
		/// </summary>
		public static string Settings_Cancel => LocaleManager.GetString("Settings_Cancel", Culture);

		/// <summary>
		/// Change log
		/// </summary>
		public static string Settings_ChangeLog => LocaleManager.GetString("Settings_ChangeLog", Culture);

		/// <summary>
		/// For Linux users
		/// </summary>
		public static string Settings_ForLinuxUsers => LocaleManager.GetString("Settings_ForLinuxUsers", Culture);

		/// <summary>
		/// General
		/// </summary>
		public static string Settings_General => LocaleManager.GetString("Settings_General", Culture);

		/// <summary>
		/// General
		/// </summary>
		public static string Settings_GeneralTab => LocaleManager.GetString("Settings_GeneralTab", Culture);

		/// <summary>
		/// Language
		/// </summary>
		public static string Settings_Language => LocaleManager.GetString("Settings_Language", Culture);

		/// <summary>
		/// Notifications
		/// </summary>
		public static string Settings_Notifications => LocaleManager.GetString("Settings_Notifications", Culture);

		/// <summary>
		/// Press any key
		/// </summary>
		public static string Settings_PressAnyKey => LocaleManager.GetString("Settings_PressAnyKey", Culture);

		/// <summary>
		/// Activate tool
		/// </summary>
		public static string Settings_ShortcutActivateTool => LocaleManager.GetString("Settings_ShortcutActivateTool", Culture);

		/// <summary>
		/// Shortcuts
		/// </summary>
		public static string Settings_Shortcuts => LocaleManager.GetString("Settings_Shortcuts", Culture);

		/// <summary>
		/// Selection step over
		/// </summary>
		public static string Settings_ShortcutSelectionStepOver => LocaleManager.GetString("Settings_ShortcutSelectionStepOver", Culture);

		/// <summary>
		/// Notification only about major updates
		/// </summary>
		public static string Settings_ShowOnlyMajor => LocaleManager.GetString("Settings_ShowOnlyMajor", Culture);

		/// <summary>
		/// Show tooltips
		/// </summary>
		public static string Settings_ShowTooltips => LocaleManager.GetString("Settings_ShowTooltips", Culture);

		/// <summary>
		/// Show what's new
		/// </summary>
		public static string Settings_ShowWhatsNew => LocaleManager.GetString("Settings_ShowWhatsNew", Culture);

		/// <summary>
		/// Solve crash on Linux
		/// </summary>
		public static string Settings_SolveCrashOnLinux => LocaleManager.GetString("Settings_SolveCrashOnLinux", Culture);

		/// <summary>
		/// Support
		/// </summary>
		public static string Settings_SupportTab => LocaleManager.GetString("Settings_SupportTab", Culture);

		/// <summary>
		/// Tool button
		/// </summary>
		public static string Settings_ToolButton => LocaleManager.GetString("Settings_ToolButton", Culture);

		/// <summary>
		/// Show in both places
		/// </summary>
		public static string Settings_ToolButtonBoth => LocaleManager.GetString("Settings_ToolButtonBoth", Culture);

		/// <summary>
		/// Show only in toolbar
		/// </summary>
		public static string Settings_ToolButtonOnlyToolbar => LocaleManager.GetString("Settings_ToolButtonOnlyToolbar", Culture);

		/// <summary>
		/// Show only in UnifiedUI
		/// </summary>
		public static string Settings_ToolButtonOnlyUUI => LocaleManager.GetString("Settings_ToolButtonOnlyUUI", Culture);

		/// <summary>
		/// You can help improve the mod translation
		/// </summary>
		public static string Settings_TranslationDescription => LocaleManager.GetString("Settings_TranslationDescription", Culture);

		/// <summary>
		/// Improve translation
		/// </summary>
		public static string Settings_TranslationImprove => LocaleManager.GetString("Settings_TranslationImprove", Culture);

		/// <summary>
		/// Add new language
		/// </summary>
		public static string Settings_TranslationNew => LocaleManager.GetString("Settings_TranslationNew", Culture);

		/// <summary>
		/// Troubleshooting
		/// </summary>
		public static string Settings_Troubleshooting => LocaleManager.GetString("Settings_Troubleshooting", Culture);

		/// <summary>
		/// If you like {0}, you can support author
		/// </summary>
		public static string Setting_Donate => LocaleManager.GetString("Setting_Donate", Culture);

		/// <summary>
		/// Press {0} to
		/// </summary>
		public static string Tool_InfoSelectionStepOver => LocaleManager.GetString("Tool_InfoSelectionStepOver", Culture);

		/// <summary>
		/// FIXED
		/// </summary>
		public static string WhatsNew_FIXED => LocaleManager.GetString("WhatsNew_FIXED", Culture);

		/// <summary>
		/// NEW
		/// </summary>
		public static string WhatsNew_NEW => LocaleManager.GetString("WhatsNew_NEW", Culture);

		/// <summary>
		/// REMOVED
		/// </summary>
		public static string WhatsNew_REMOVED => LocaleManager.GetString("WhatsNew_REMOVED", Culture);

		/// <summary>
		/// REVERTED
		/// </summary>
		public static string WhatsNew_REVERTED => LocaleManager.GetString("WhatsNew_REVERTED", Culture);

		/// <summary>
		/// TRANSLATION
		/// </summary>
		public static string WhatsNew_TRANSLATION => LocaleManager.GetString("WhatsNew_TRANSLATION", Culture);

		/// <summary>
		/// UPDATED
		/// </summary>
		public static string WhatsNew_UPDATED => LocaleManager.GetString("WhatsNew_UPDATED", Culture);

		/// <summary>
		/// WARNING
		/// </summary>
		public static string WhatsNew_WARNING => LocaleManager.GetString("WhatsNew_WARNING", Culture);

		/// <summary>
		/// Brazilian
		/// </summary>
		public static string Mod_Locale_pt_br => LocaleManager.GetString("Mod_Locale_pt-br", Culture);
	}
}