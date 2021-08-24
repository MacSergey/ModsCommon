using ICities;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ModsCommon
{
    public abstract class SingletonItem<T>
    {
        public static T Instance { get; set; }
    }

    public interface ICustomMod : IUserMod
    {
        public string NameRaw { get; }
        public ILogger Logger { get; }
        public Version Version { get; }
        public List<Version> Versions { get; }
        public CultureInfo Culture { get; }
#if DEBUG
        public bool NeedMonoDevelopDebug { get; }
#else
        public bool NeedMonoDevelop { get; }
#endif

        public Dictionary<Version, string> GetWhatsNewMessages(Version whatNewVersion);
        public string GetVersionString(Version version);
        public void ShowLinuxTip();
        public bool OpenDiscord();
        public bool OpenSupport();
    }
    public abstract class SingletonMod<T> : SingletonItem<T>
        where T : ICustomMod
    {
        public static string Name => Instance.Name;
        public static string NameRaw => Instance.NameRaw;
        public static ILogger Logger => Instance.Logger;
        public static Version Version => Instance.Version;
    }
}
