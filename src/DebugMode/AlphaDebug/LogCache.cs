using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DebugMode.AlphaDebug
{
    public static class LogCache
    {
        public static void AddHandlers()
        {
            Application.logMessageReceived += delegate (string condition, string trace, LogType type)
            {
                LogCache.Logs.Add(new LogCache.LogEntry
                {
                    Message = "[" + DateTime.Now.ToLongTimeString() + "] " + condition,
                    Trace = trace,
                    Type = type
                });
            };
        }

        public static void Clear()
        {
            LogCache.Logs.Clear();
        }

        public static readonly List<LogCache.LogEntry> Logs = new List<LogCache.LogEntry>();

        public class LogEntry
        {
            public string Message { get; set; }

            public string Trace { get; set; }

            public LogType Type { get; set; }
        }
    }
}
