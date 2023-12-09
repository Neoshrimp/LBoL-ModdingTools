using BepInEx;
using HarmonyLib;
using LBoL.Presentation.UI.Panels;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace AddWatermark
{
    [BepInPlugin(API.GUID, "Add MODDED watermark", version)]
    [BepInProcess("LBoL.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public const string version = "1.1.1";

        private static readonly Harmony harmony = new Harmony(API.GUID);

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

        internal static GameObject watermarkRef = null;

        internal static bool activateWatermark = false; // false

       

        [HarmonyPatch(typeof(SystemBoard), nameof(SystemBoard.OnEnterGameRun))]
        class SystemBoard_Patch
        {

            public static void Postfix(SystemBoard __instance)
            {
                try
                {
                    //var infoGoNames = new HashSet<string>() { "EAWarning", "Mode", "Version", "SeedButton" };

                    var Infos = __instance.gameVersion.gameObject.transform.parent;


                    var infoCount = 0;
                    GameObject watermark = null;
                    float? topSiblingPos = null;

                    foreach (Transform c in Infos.transform)
                    {
                        if (c.gameObject.activeSelf && c.gameObject.name != "Hint")
                        {
                            infoCount++;
                            if (topSiblingPos == null)
                            {
                                topSiblingPos = c.localPosition.y;
                            }
                        }
                        if (c.gameObject.name == "watermark")
                        {
                            watermark = c.gameObject;
                        }
                    }

                    if (watermark == null)
                    {
                        watermark = GameObject.Instantiate(__instance.gameVersion.gameObject, Infos.transform);
                        watermark.name = "watermark";
                        var moddedText = watermark.GetComponent<TextMeshProUGUI>();

                        moddedText.text = "MODDED";

                        watermark.transform.SetAsLastSibling();
                        watermark.transform.localPosition = new Vector3(watermark.transform.localPosition.x, topSiblingPos.Value - (infoCount * 50), 0);

                        watermark.SetActive(activateWatermark);

                    }

                    watermarkRef = watermark;

                }
                catch (Exception ex)
                {
                    log.LogError(ex);
                }


            }
        }


    }
}
