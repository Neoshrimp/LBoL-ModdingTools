using HarmonyLib;
using LBoL.Core;
using LBoL.Presentation;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DebugMode.AlphaDebug
{
    public class DebugEntryPoint
    {

        public const string goName = "GameRunDebugUI";

        public static void Create(Transform gameMasterT)
        {
            if (gameMasterT.Find(goName) == null)
            {
                var go = LBoL.Core.Utils.CreateGameObject(gameMasterT, goName);

                go.AddComponent<DebugMode.AlphaDebug.GameDebugMenuUI>();
                go.AddComponent<DebugMode.AlphaDebug.GameConsoleUI>();
            }
        }
    }



    [HarmonyPatch(typeof(GameMaster), nameof(GameMaster.StartupEnterMainMenu))]
    class InitDebugMenu_Patch
    {
        static void Prefix()
        {
            DebugEntryPoint.Create(GameMaster.Instance.transform);
        }
    }


}
