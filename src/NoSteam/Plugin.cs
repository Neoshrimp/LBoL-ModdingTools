using BepInEx;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.PlatformHandlers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace NoSteam
{
    [BepInPlugin(GUID, "No Steam", version)]
    [BepInProcess("LBoL.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "neo.lbol.debug.NoSteam";
        public const string version = "1.0.0";

        private static readonly Harmony harmony = new Harmony(GUID);

        internal static BepInEx.Logging.ManualLogSource log;

        private void Awake()
        {
            log = Logger;

            // very important. Without this the entry point MonoBehaviour gets destroyed
            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = HideFlags.HideAndDontSave;

            harmony.PatchAll();

        }

        private void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchSelf();
        }


        public static bool foundSteam = false;

        [HarmonyPatch(typeof(SteamPlatformHandler), nameof(SteamPlatformHandler.Init))]
        class SteamPlatformHandlerInit_Patch
        {

            static void Postfix(ref bool __result, SteamPlatformHandler __instance)
            {

                if (__result == true)
                {

                    foundSteam = true;
                    log.LogInfo("Steam is ON. This plugins' effect is disabled..");
                    return;
                }

                __result = true;
                var steamId = Directory.GetDirectories(Application.persistentDataPath).Select(
                    d => Regex.Match(d, @"\d+$", RegexOptions.RightToLeft)).First().Value;

                if (!string.IsNullOrEmpty(steamId))
                    __instance._steamId = ulong.Parse(steamId);
                
            }
        }


        [HarmonyPatch]
        class SteamPlatformHandler_Patch
        {


            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(SteamPlatformHandler), nameof(SteamPlatformHandler.Update));
                yield return AccessTools.Method(typeof(SteamPlatformHandler), nameof(SteamPlatformHandler.Shutdown));
                yield return AccessTools.Method(typeof(SteamPlatformHandler), nameof(SteamPlatformHandler.SetMainMenuInfo));
                yield return AccessTools.Method(typeof(SteamPlatformHandler), nameof(SteamPlatformHandler.SetGameRunInfo));


            }

            static bool Prefix()
            {
                return foundSteam;
            }

        }


        [HarmonyPatch(typeof(SteamPlatformHandler), nameof(SteamPlatformHandler.GetDefaultLocale))]
        class SteamPlatformHandlerGetDefaultLocale_Patch
        {
            static bool Prefix(ref Locale __result)
            {
                __result = Locale.En;
                return foundSteam;
            }

        }






    }
}
