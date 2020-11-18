using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModsCommon
{
    public class Logger
    {
        private string Name { get; }
        public Logger(string name) => Name = name;

        public void Debug(string message) => Log(UnityEngine.Debug.Log, message);
        public void Error(string message, Exception error = null) => Log(UnityEngine.Debug.LogError, error == null ? message : $"{message}\n{error.Message}\n{error.StackTrace}");
        private void Log(Action<string> logFunc, string message) => logFunc($"[{Name}] {message}");
    }
}
