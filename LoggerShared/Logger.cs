using System;
using UnityEngine;

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
        private static string DebugFormat => "[{0}][{1}] {2}";
        private static string ErrorFormat => "[{0}][{1}] {2}\n{3}\n{4}";
        private static string ExceptionFormat => "[{0}][{1}] {2}\n{3}";

        private string Name { get; }
        private ILogHandler Handle { get; } = UnityEngine.Debug.logger.logHandler;
        public Logger(string name)
        {
            Name = name;
        }

        public void Debug(string message) => Handle.LogFormat(LogType.Log, null, DebugFormat, Name, Time.realtimeSinceStartup, message);

        public void Error(string message, Exception error = null)
        {
            if (error != null)
                Handle.LogFormat(LogType.Log, null, ErrorFormat, Name, Time.realtimeSinceStartup, message, error.Message, error.StackTrace);
            else
                Handle.LogFormat(LogType.Log, null, DebugFormat, Name, Time.realtimeSinceStartup, message);
        }
        public void Error(Exception error) => Handle.LogFormat(LogType.Log, null, ExceptionFormat, Name, Time.realtimeSinceStartup, error.Message, error.StackTrace);
    }
}
