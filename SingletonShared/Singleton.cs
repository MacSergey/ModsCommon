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
        public ILogger Logger { get; }
        public Version Version { get; }
    }
    public abstract class SingletonMod<T> : SingletonItem<T>
        where T : ICustomMod
    {
        public static string Name => Instance.Name;
        public static ILogger Logger => Instance.Logger;
        public static Version Version => Instance.Version;
    }
    public static class SingletonManager<T>
        where T : class, IManager, new()
    {
        private static T _instance;
        public static T Instance
        {
            get => _instance ??= new T();
            set => _instance = value;
        }
        public static void Destroy() => Instance = null;
    }
    public interface IManager { }
}
