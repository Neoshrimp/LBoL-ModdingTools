using BepInEx;
using HarmonyLib;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Reflection;
using UnityEngine;


namespace SideloaderTemplate
{
    [BepInPlugin(SideloaderTemplate.PInfo.GUID, SideloaderTemplate.PInfo.Name, SideloaderTemplate.PInfo.version)]
    [BepInDependency(LBoLEntitySideloader.PluginInfo.GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(AddWatermark.API.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("LBoL.exe")]
    public class BepinexPlugin : BaseUnityPlugin
    {
        
        private static readonly Harmony harmony = SideloaderTemplate.PInfo.harmony;

        internal static BepInEx.Logging.ManualLogSource log;

        internal static IResourceSource embeddedSource = new EmbeddedSource(Assembly.GetExecutingAssembly());
        internal static DirectorySource directorySource = new DirectorySource(SideloaderTemplate.PInfo.GUID, "resources");

        internal static BatchLocalization cardBatchLoc = new BatchLocalization(directorySource, typeof(CardTemplate), "cards");


        private void Awake()
        {
            log = Logger;

            // very important. Without this the entry point MonoBehaviour gets destroyed
            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = HideFlags.HideAndDontSave;

            EntityManager.RegisterSelf();

            harmony.PatchAll();

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(AddWatermark.API.GUID))
                WatermarkWrapper.ActivateWatermark();
        }

        private void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchSelf();
        }


    }
}
