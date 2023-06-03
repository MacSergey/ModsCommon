namespace ModsCommon
{
    public interface IManager { }
    public static class SingletonManager<T>
        where T : class, IManager, new()
    {
        private static T _instance;
        public static T Instance
        {
            get => _instance ??= new T();
            set => _instance = value;
        }
        public static bool Exist => _instance != null;
        public static void Destroy() => Instance = null;
    }
}
