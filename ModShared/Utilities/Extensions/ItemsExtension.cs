using ColossalFramework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace ModsCommon.Utilities
{
    public static class ItemsExtension
    {
        public static bool InGame => !OnStartup && !InMenu;
        public static bool InMenu => SceneManager.GetActiveScene().name is string scene && scene == "IntroScreen";
        public static bool OnStartup => SceneManager.GetActiveScene().name is string scene && scene == "Startup";
    }
}
