using System;
using System.Collections.Generic;
using System.Text;

namespace ExportModImgs
{
    internal static class Log
    {
        static BepInEx.Logging.ManualLogSource log = BepinexPlugin.log;

        internal static void LogMessage(object data) => log.LogMessage(data);
        internal static void LogInfo(object data) => log.LogInfo(data);
        internal static void LogDebug(object data) => log.LogDebug(data);
        internal static void LogWarning(object data) => log.LogWarning(data);
        internal static void LogError(object data) => log.LogError(data);
        internal static void LogFatal(object data) => log.LogFatal(data);

    }
}
