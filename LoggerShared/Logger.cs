using System;

namespace ModsCommon
{
    public interface ILogger
    {
        public void Debug(string message);

        public void Error(string message, Exception error = null);
        public void Error(Exception error);
    }
    public class Logger : ILogger
    {
        private static string DebugFormat => "[{0}] {1}";
        private static string ErrorFormat => "[{0}] {1}\n{2}\n{3}";
        private static string ExceptionFormat => "[{0}] {1}\n{2}";

        private string Name { get; }
        private UnityEngine.ILogHandler Handle { get; } = UnityEngine.Debug.logger.logHandler;
        public Logger(string name)
        {
            Name = name;
        }

        public void Debug(string message) => Handle.LogFormat(UnityEngine.LogType.Log, null, DebugFormat, Name, message);

        public void Error(string message, Exception error = null)
        {
            if (error != null)
                Handle.LogFormat(UnityEngine.LogType.Log, null, ErrorFormat, Name, message, error.Message, error.StackTrace);
            else
                Handle.LogFormat(UnityEngine.LogType.Log, null, DebugFormat, Name, message);
        }
        public void Error(Exception error) => Handle.LogFormat(UnityEngine.LogType.Log, null, ExceptionFormat, Name, error.Message, error.StackTrace);
    }
}
