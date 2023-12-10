using System.Collections;
using System.Runtime.CompilerServices;
using BepInEx;
using HarmonyLib;
using Mono.Cecil;
using UnityEngine;


namespace CWT_test
{
    [BepInPlugin(GUID, "CWT test", version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "neo.tests.cwtTest";
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

            StartCoroutine(ConditionalWeakTableTest.Test());

        }



        private void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchSelf();
        }


        



    }




 
public class ConditionalWeakTableTest
    {
        private static void Collect()
        {
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
        }

        static object[] refArray = new object[100];

        static ConditionalWeakTable<object, object> cwt = new ConditionalWeakTable<object, object>();


        public static IEnumerator Test()
        {
            Plugin.log.LogInfo($"Starting CWT test..");


            object a = new object();
            object b = new object();

/*            for (var i = 0; i < refArray.Length; i++)
            {
                refArray[i] = new object();
                cwt.Add(refArray[i], new object());

            }*/

            cwt.Add(a, b);

            var wa = new System.WeakReference(a);
            var wb = new System.WeakReference(b);


            yield return null;
            Collect();


            b = null;

            for (int i = 0; i < 10; ++i)
            {
                yield return null;
                Collect();

            }

            a = null;

            for (int i = 0; i < 1000; ++i)
            {
                yield return null;
                Collect();
            }

            Plugin.log.LogInfo($"If CWS worked, this would be false: {wa.IsAlive}");
            Plugin.log.LogInfo($"If CWS worked, this would be false: {wb.IsAlive}");
            Plugin.log.LogInfo($"cwt size: {AccessTools.Field(typeof(ConditionalWeakTable<object,object>), "size").GetValue(cwt)}");

        }
    }

}
