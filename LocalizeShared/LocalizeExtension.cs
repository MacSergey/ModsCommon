using ColossalFramework;
using ColossalFramework.Globalization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ModsCommon
{
    public static class LocalizeExtension
    {
        public static string Separator => " + ";
        public static string Ctrl => CommonLocalize.Key_Control;
        public static string Alt => CommonLocalize.Key_Alt;
        public static string Shift => CommonLocalize.Key_Shift;
        public static string Esc => CommonLocalize.Key_Esc;
        public static string Enter => CommonLocalize.Key_Enter;
        public static string Space => CommonLocalize.Key_Space;
        public static string Backspace => CommonLocalize.Key_Backspace;

        public static string CtrlAlt => $"{Ctrl}{Separator}{Alt}";
        public static string CtrlShift => $"{Ctrl}{Separator}{Shift}";
        public static string AltShift => $"{Alt}{Separator}{Shift}";
        public static string CtrlAltShift => $"{Ctrl}{Separator}{Alt}{Separator}{Shift}";

        public static string GetLocale(this SavedInputKey savedKey)
        {
            var text = string.Empty;

            if (savedKey.Control)
                text += Ctrl + Separator;
            if (savedKey.Alt)
                text += Alt + Separator;
            if (savedKey.Shift)
                text += Shift + Separator;
            text += savedKey.GetKeyLocale();

            return text;
        }
        public static string GetModifiers(bool ctrl = false, bool alt = false, bool shift = false)
        {
            var modifiers = new List<string>(3);

            if (ctrl)
                modifiers.Add(Ctrl);
            if (alt)
                modifiers.Add(Alt);
            if (shift)
                modifiers.Add(Shift);

            return string.Join(Separator, modifiers.ToArray());
        }
        private static string GetKeyLocale(this SavedInputKey savedKey) => savedKey.Key switch
        {
            KeyCode.LeftBracket => "[",
            KeyCode.RightBracket => "]",
            KeyCode.Plus or KeyCode.KeypadPlus => "+",
            KeyCode.Minus or KeyCode.KeypadMinus => "-",
            KeyCode.LeftControl or KeyCode.RightControl => CommonLocalize.Key_Control,
            KeyCode.LeftAlt or KeyCode.RightAlt => CommonLocalize.Key_Alt,
            KeyCode.LeftShift or KeyCode.RightShift => CommonLocalize.Key_Shift,
            KeyCode.KeypadEnter or KeyCode.Return => CommonLocalize.Key_Enter,
            KeyCode.Tab => CommonLocalize.Key_Tab,
            _ => savedKey.Key.GetLocale(),
        };
        public static string GetLocale(this KeyCode key) => Locale.Exists("KEYNAME", key.ToString()) ? Locale.Get("KEYNAME", key.ToString()) : key.ToString();

        public static string GetRegionLocale(string locale)
        {
            if (locale.Length == 2)
            {
                return locale switch
                {
                    "cs" => "cs-CZ",
                    "da" => "da-DK",
                    "de" => "de-DE",
                    "en" => "en-US",
                    "es" => "es-ES",
                    "fi" => "fi-FI",
                    "fr" => "fr-FR",
                    "hu" => "hu-HU",
                    "id" => "id-ID",
                    "it" => "it-IT",
                    "ja" => "ja-JP",
                    "ko" => "ko-KR",
                    "ms" => "ms-MY",
                    "nl" => "nl-NL",
                    "pl" => "pl-PL",
                    "pt" => "pt-PT",
                    "ro" => "ro-RO",
                    "ru" => "ru-RU",
                    "tr" => "tr-TR",
                    "zh" => "zh-CN",
                    _ => locale,
                };
            }
            else
                return locale;
        }

        private static Dictionary<string, CultureInfo> Cultures { get; } = new Dictionary<string, CultureInfo>();
        public static bool TryGetCulture(string locale, out CultureInfo culture)
        {
            locale = GetRegionLocale(locale);

            if (!Cultures.TryGetValue(locale, out culture))
            {
                if (CultureInfo.GetCultures(CultureTypes.AllCultures).Any(c => c.Name == locale))
                    culture = new CultureInfo(locale);
                else
                    culture = CreateCulture(locale);

                Cultures[locale] = culture;
            }

            return culture != null;
        }
        private static CultureInfo CreateCulture(string locale)
        {
            var culture = new CultureInfo(string.Empty).Clone() as CultureInfo;
            SetValue(culture, "m_name", locale);

            switch (locale)
            {
                case "ms-MY":
                    SetValue(culture, "nativename", "Melayu (Malaysia)");
                    SetValue(culture, "englishname", "Malay (Malaysia)");
                    SetValue(culture, "displayname", "Malay (Malaysia)");
                    SetValue(culture, "iso2lang", "ms");
                    SetValue(culture, "iso3lang", "msa");
                    SetValue(culture, "win3lang", "MSL");
                    culture.NumberFormat = new NumberFormatInfo()
                    {
                        CurrencyDecimalDigits = 2,
                        CurrencyDecimalSeparator = ".",
                        CurrencyGroupSeparator = ",",
                        CurrencyGroupSizes = new int[] { 3 },
                        CurrencyNegativePattern = 1,
                        CurrencyPositivePattern = 0,
                        CurrencySymbol = "RM",
                        DigitSubstitution = DigitShapes.None,
                        NaNSymbol = "NaN",
                        NativeDigits = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" },
                        NegativeInfinitySymbol = "-∞",
                        NegativeSign = "-",
                        NumberDecimalDigits = 2,
                        NumberDecimalSeparator = ".",
                        NumberGroupSeparator = ",",
                        NumberGroupSizes = new int[] { 3 },
                        NumberNegativePattern = 1,
                        PerMilleSymbol = "‰",
                        PercentDecimalDigits = 2,
                        PercentDecimalSeparator = ".",
                        PercentGroupSeparator = ",",
                        PercentGroupSizes = new int[] { 3 },
                        PercentNegativePattern = 1,
                        PercentPositivePattern = 1,
                        PercentSymbol = "%",
                        PositiveInfinitySymbol = "∞",
                        PositiveSign = "+",
                    };
                    culture.DateTimeFormat = new DateTimeFormatInfo()
                    {
                        AMDesignator = "PG",
                        AbbreviatedDayNames = new string[] { "Ahd", "Isn", "Sel", "Rab", "Kha", "Jum", "Sab" },
                        AbbreviatedMonthGenitiveNames = new string[] { "Jan", "Feb", "Mac", "Apr", "Mei", "Jun", "Jul", "Ogo", "Sep", "Okt", "Nov", "Dis", "" },
                        AbbreviatedMonthNames = new string[] { "Jan", "Feb", "Mac", "Apr", "Mei", "Jun", "Jul", "Ogo", "Sep", "Okt", "Nov", "Dis", "" },
                        Calendar = new GregorianCalendar(GregorianCalendarTypes.Localized),
                        CalendarWeekRule = CalendarWeekRule.FirstDay,
                        DateSeparator = "/",
                        DayNames = new string[] { "Ahad", "Isnin", "Selasa", "Rabu", "Khamis", "Jumaat", "Sabtu" },
                        FirstDayOfWeek = DayOfWeek.Monday,
                        FullDateTimePattern = "dddd, d MMMM yyyy h:mm:ss tt",
                        LongDatePattern = "dddd, d MMMM yyyy",
                        LongTimePattern = "h:mm:ss tt",
                        MonthDayPattern = "d MMMM",
                        MonthGenitiveNames = new string[] { "Januari", "Februari", "Mac", "April", "Mei", "Jun", "Julai", "Ogos", "September", "Oktober", "November", "Disember", "" },
                        MonthNames = new string[] { "Januari", "Februari", "Mac", "April", "Mei", "Jun", "Julai", "Ogos", "September", "Oktober", "November", "Disember", "" },
                        PMDesignator = "PTG",
                        ShortDatePattern = "d/MM/yyyy",
                        ShortTimePattern = "h:mm tt",
                        ShortestDayNames = new string[] { "Ah", "Is", "Se", "Ra", "Kh", "Ju", "Sa" },
                        TimeSeparator = ":",
                        YearMonthPattern = "MMMM yyyy",
                    };
                    return culture;
                default:
                    return null;
            }
        }
        private static void SetValue<T>(CultureInfo culture, string field, T value)
        {
            var nameField = typeof(CultureInfo).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            nameField.SetValue(culture, value);
        }
    }
}
