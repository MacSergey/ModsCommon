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
        private static string Separator => " + ";
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
    }
}
