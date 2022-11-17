using ColossalFramework;
using ColossalFramework.Globalization;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModsCommon
{
    public static class LocalizeExtension
    {
        public static string Separator => " + ";
        public static string Ctrl => CommonLocalize.Key_Control;
        public static string Alt => CommonLocalize.Key_Alt;
        public static string Shift => CommonLocalize.Key_Shift;
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
                    "mr" => "mr-IN",
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
    }
}
