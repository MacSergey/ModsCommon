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
        public static string GetLocale(this SavedInputKey savedKey)
        {
            var text = string.Empty;

            if (savedKey.Control)
                text += Locale.Get("KEYNAME", KeyCode.LeftControl.ToString()) + " + ";
            if (savedKey.Alt)
                text += Locale.Get("KEYNAME", KeyCode.LeftAlt.ToString()) + " + ";
            if (savedKey.Shift)
                text += Locale.Get("KEYNAME", KeyCode.LeftShift.ToString()) + " + ";
            text += savedKey.GetKeyLocale();

            return text;
        }
        private static string GetKeyLocale(this SavedInputKey savedKey) => savedKey.Key switch
        {
            KeyCode.LeftBracket => "[",
            KeyCode.RightBracket => "]",
            KeyCode.Plus or KeyCode.KeypadPlus => "+",
            KeyCode.Minus or KeyCode.KeypadMinus => "-",
            _ => savedKey.Key.GetLocale(),
        };
        private static string GetLocale(this KeyCode key) => Locale.Exists("KEYNAME", key.ToString()) ? Locale.Get("KEYNAME", key.ToString()) : key.ToString();
    }
}
