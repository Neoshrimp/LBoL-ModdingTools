using BepInEx;
using ExportModImgs.Exporters;
using HarmonyLib;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Reflection;
using UnityEngine;


namespace ExportModImgs
{
    [BepInPlugin(ExportModImgs.PInfo.GUID, ExportModImgs.PInfo.Name, ExportModImgs.PInfo.version)]
    [BepInDependency(LBoLEntitySideloader.PluginInfo.GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInProcess("LBoL.exe")]
    public class BepinexPlugin : BaseUnityPlugin
    {

        private static readonly Harmony harmony = ExportModImgs.PInfo.harmony;



        internal static BepInEx.Logging.ManualLogSource log;


        private void Awake()
        {
            log = Logger;

            // very important. Without this the entry point MonoBehaviour gets destroyed
            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = HideFlags.HideAndDontSave;


            harmony.PatchAll();

            var texExporter = new TexExporter() {
                addTimeStamp = true
            };
            texExporter.HookSelf();

        }

        private void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchSelf();
        }


    }
}
