using HarmonyLib;

namespace ExportModImgs
{
    public static class PInfo
    {
        // each loaded plugin needs to have a unique GUID. usually author+generalCategory+Name is good enough
        public const string GUID = "neo.lbol.tools.exportModImgs";
        public const string Name = "Export mod images";
        public const string version = "0.1.0";
        public static readonly Harmony harmony = new Harmony(GUID);

    }
}
