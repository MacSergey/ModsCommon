using ColossalFramework.PlatformServices;
using ColossalFramework.UI;
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
            if (string.IsNullOrEmpty(url))
                return;
            else if (PlatformService.IsOverlayEnabled())
                PlatformService.ActivateGameOverlayToWebPage(url);
            else
                Process.Start(url);
        }
        public static void OpenWorkshop(this ulong id) => id.GetWorkshopUrl().OpenUrl();
        public static string GetWorkshopUrl(this ulong id) => $"https://steamcommunity.com/sharedfiles/filedetails/?id={id}";
        public static void OpenPatreon() => OpenUrl("https://www.patreon.com/MacSergey");
        public static void OpenPayPal() => OpenUrl("https://www.paypal.me/macsergey");

        public static bool InGame => !OnStartup && !OnMenu;
        public static bool OnGame => SceneManager.GetActiveScene().name is string scene && scene == "Game";
        public static bool OnMenu => SceneManager.GetActiveScene().name is string scene && scene == "MainMenu";
        public static bool OnStartup => SceneManager.GetActiveScene().name is string scene && scene == "Startup";

        public static bool AltIsPressed => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        public static bool ShiftIsPressed => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        public static bool CtrlIsPressed => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        public static bool OnlyAltIsPressed => AltIsPressed && !ShiftIsPressed && !CtrlIsPressed;
        public static bool OnlyShiftIsPressed => ShiftIsPressed && !AltIsPressed && !CtrlIsPressed;
        public static bool OnlyCtrlIsPressed => CtrlIsPressed && !AltIsPressed && !ShiftIsPressed;
        public static bool NotPressed => !AltIsPressed && !ShiftIsPressed && !CtrlIsPressed;
    }
    public static class IntroUtility
    {
        private static List<Action> IntroActions { get; } = new List<Action>();

        public static void OnLoaded(Action action)
        {
            if (UIView.GetAView() != null)
                action();
            else
            {
                LoadingManager.instance.m_introLoaded -= IntroLoaded;
                LoadingManager.instance.m_introLoaded += IntroLoaded;

                IntroActions.Add(action);
            }
        }

        private static void IntroLoaded()
        {
            LoadingManager.instance.m_introLoaded -= IntroLoaded;

            foreach (var action in IntroActions)
                action();
        }
    }
}
