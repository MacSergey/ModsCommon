using ColossalFramework.PlatformServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ModsCommon.Utilities
{
    public static class Utilites
    {
        public static void OpenUrl(this string url)
        {
            if (PlatformService.IsOverlayEnabled())
                PlatformService.ActivateGameOverlayToWebPage(url);
            else
                Process.Start(url);
        }
    }
}
