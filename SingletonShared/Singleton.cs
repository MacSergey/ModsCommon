using ICities;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ModsCommon
{
    public abstract class SingletonItem<T>
    {
        public static T Instance { get; set; }
        public static bool Exist => Instance != null;
    }

    public interface ICustomMod : IUserMod
    {
        public event Action OnStatusChanged;

        public string NameRaw { get; }
        public ILogger Logger { get; }
        public Version Version { get; }
        public string VersionString { get; }
        public List<ModVersion> Versions { get; }
        public ModStatus Status { get; }
        public CultureInfo Culture { get; }
#if DEBUG
        public bool NeedMonoDevelopDebug { get; }
        public DependenciesWatcher DependencyWatcher { get; }
#else
        public bool NeedMonoDevelop { get; }
#endif

        public Dictionary<ModVersion, string> GetWhatsNewMessages(Version whatNewVersion);
        public void ShowLinuxTip();
        public bool OpenDiscord();
        public bool OpenSupport();
        public bool OpenTranslationProject();

        public string GetLocalizedString(string key, CultureInfo culture = null);
        public IEnumerable<string> GetSupportLocales();
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
