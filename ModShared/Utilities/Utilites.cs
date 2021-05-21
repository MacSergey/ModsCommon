using ColossalFramework.PlatformServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModsCommon.Utilities
{
    public static class Utility
    {
        public static void OpenUrl(this string url)
        {
            if (PlatformService.IsOverlayEnabled())
                PlatformService.ActivateGameOverlayToWebPage(url);
            else
                Process.Start(url);
        }

        public static bool InGame => !OnStartup && !InMenu;
        public static bool InMenu => SceneManager.GetActiveScene().name is string scene && scene == "IntroScreen";
        public static bool OnStartup => SceneManager.GetActiveScene().name is string scene && scene == "Startup";

        public static bool AltIsPressed => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        public static bool ShiftIsPressed => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        public static bool CtrlIsPressed => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        public static bool OnlyAltIsPressed => AltIsPressed && !ShiftIsPressed && !CtrlIsPressed;
        public static bool OnlyShiftIsPressed => ShiftIsPressed && !AltIsPressed && !CtrlIsPressed;
        public static bool OnlyCtrlIsPressed => CtrlIsPressed && !AltIsPressed && !ShiftIsPressed;
    }
}
