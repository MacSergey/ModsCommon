using ICities;
using System;

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
