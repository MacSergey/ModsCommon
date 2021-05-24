using System;

namespace ModsCommon
{
    public interface ILogger
    {
        public void Debug(string message);

        public void Warning(string message, Exception error = null);
        public void Warning(Exception error);

        public void Error(string message, Exception error = null);
        public void Error(Exception error);
    }
    public class Logger : ILogger
    {
        private string Name { get; }
        public Logger(string name) => Name = name;

        public void Debug(string message) => Log(UnityEngine.Debug.Log, message);

        public void Warning(string message, Exception error = null) => Log(UnityEngine.Debug.LogWarning, GetMessage(message, error));
        public void Warning(Exception error) => Log(UnityEngine.Debug.LogWarning, GetMessage(error));

        public void Error(string message, Exception error = null) => Log(UnityEngine.Debug.LogError, GetMessage(message, error));
        public void Error(Exception error) => Log(UnityEngine.Debug.LogError, GetMessage(error));

        private void Log(Action<string> logFunc, string message) => logFunc($"[{Name}] {message}");

        private string GetMessage(string message, Exception error) => error == null ? message : $"{message}\n{GetMessage(error)}";
        private string GetMessage(Exception error) => $"{error.Message}\n{error.StackTrace}";
    }
}
