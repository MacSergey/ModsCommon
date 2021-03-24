using System;

namespace ModsCommon
{
    public class Logger
    {
        private string Name { get; }
        public Logger(string name) => Name = name;

        public void Debug(string message) => Log(UnityEngine.Debug.Log, message);
        public void Warning(string message, Exception error = null) => Log(UnityEngine.Debug.LogWarning, GetMessage(message, error));
        public void Error(string message, Exception error = null) => Log(UnityEngine.Debug.LogError, GetMessage(message, error));
        private void Log(Action<string> logFunc, string message) => logFunc($"[{Name}] {message}");
        private string GetMessage(string message, Exception error) => error == null ? message : $"{message}\n{error.Message}\n{error.StackTrace}";
    }
}
