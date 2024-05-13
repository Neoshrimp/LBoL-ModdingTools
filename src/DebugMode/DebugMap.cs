using HarmonyLib;
using LBoL.Base.Extensions;
using LBoL.Base;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using LBoL.Core;
using LBoL.EntityLib.Stages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using LBoLEntitySideloader;

namespace DebugMode
{
    public class DebugMap
    {

        [HarmonyPatch]
        class BattleAdvTestBoss_Patch
        {


            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.DeclaredConstructor(typeof(BattleAdvTest));
            }


            static void Postfix(BattleAdvTest __instance)
            {
                __instance.BossPool = new RepeatableRandomPool<string> { { "Seija", 1f } };

            }
        }




        [HarmonyPatch(typeof(BattleAdvTest), nameof(BattleAdvTest.CreateMap))]
        class BattleAdvTest_Patch
        {

            static MethodInfo createRouteMethod;


            static bool Prefix(BattleAdvTest __instance, ref GameMap __result)
            {
                if (createRouteMethod == null)
                {
                    try { createRouteMethod = AccessTools.Method(typeof(GameMap), "CreateMultiRoute"); }
                    catch (Exception) {}
                    try { createRouteMethod = AccessTools.Method(typeof(GameMap), "CreateFourRoute"); }
                    catch (Exception) {}
                }

                var error = createRouteMethod switch
                {
                    null => "Method not found (methodInfo is null).",
                    _ when !createRouteMethod.IsStatic => "Found method is not static",
                    _ when createRouteMethod.GetParameters().Length != 2 => "Found method must have exactly two parameters.",
                    _ => null
                };

                if (error != null)
                {
                    Plugin.log.LogError(error);
                    return true;
                }


                // for game loading
                if (debugStations.Empty())
                {
                    var gr = __instance.GameRun;
                    ulong? seed = null;

                    if (gr != null)
                    {
                        seed = gr.RootSeed;
                    }
                    ShuffleStations(seed);
                }


                __result = (GameMap) createRouteMethod.Invoke(null, new object[] { __instance.Boss.Id, debugStations.ToArray()});

                return false;
            }
        }


        public static List<StationType> debugStations = new List<StationType>();

        public static void ShuffleStations(ulong? seed = null)
        {
            debugStations = new List<StationType>();

            debugStations.AddRange(Enumerable.Repeat(StationType.BattleAdvTest, 85));
            debugStations.AddRange(Enumerable.Repeat(StationType.Shop, 30));
            debugStations.AddRange(Enumerable.Repeat(StationType.Gap, 30));


            seed ??= RandomGen.GetRandomSeed();

            debugStations.Shuffle(new RandomGen(seed.Value));
        }

    }
}
