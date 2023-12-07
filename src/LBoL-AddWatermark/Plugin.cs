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
        public const string version = "1.1.0";

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
                    var infoGoNames = new HashSet<string>() { "EAWarning", "Mode", "Version", "SeedButton" };

                    var Infos = __instance.gameVersion.gameObject.transform.parent;


                    var infoCount = 0;
                    GameObject watermark = null;
                    float? topSiblingPos = null;

                    foreach (Transform c in Infos.transform)
                    {
                        if (c.gameObject.activeSelf && infoGoNames.Contains(c.gameObject.name))
                        {
                            infoCount++;
                            if (topSiblingPos == null)
                                topSiblingPos = c.localPosition.y;
                        }
                        if (c.gameObject.name == "watermark")
                        {
                            watermark = c.gameObject;
                        }
                    }

                    if (watermark == null)
                    {
                        watermark = new GameObject("watermark");
                        watermark.SetActive(activateWatermark);
                        watermark.transform.SetParent(Infos);

                        var moddedText = watermark.AddComponent<TextMeshProUGUI>();

                        // copy most of the component properties
                        foreach (var p in AccessTools.GetDeclaredProperties(typeof(TMP_Text)))
                        {
                            if (p.CanWrite && !p.GetSetMethod().IsVirtual)
                            {
                                p.SetValue(moddedText, p.GetValue(__instance.gameVersion, null), null);
                            }
                        }

                        foreach (var p in AccessTools.GetDeclaredProperties(typeof(TextMeshProUGUI)))
                        {
                            if (p.CanWrite)
                            {
                                p.SetValue(moddedText, p.GetValue(__instance.gameVersion, null), null);
                            }
                        }


                        moddedText.text = "MODDED";
                        watermark.transform.localScale = new Vector3(1, 1, 1);
                    }

                    watermarkRef = watermark;

                    watermark.transform.position = Vector3.zero;
                    watermark.transform.SetAsLastSibling();
                    watermark.transform.localPosition = new Vector3(-50, topSiblingPos.Value - (infoCount * 50) - 25, 0);
                }
                catch (Exception ex)
                {
                    log.LogError(ex);
                }


            }
        }


    }
}
