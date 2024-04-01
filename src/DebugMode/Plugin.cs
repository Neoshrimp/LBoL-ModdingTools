using BepInEx;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using LBoL.EntityLib.Adventures;
using LBoL.EntityLib.Exhibits.Shining;
using LBoL.EntityLib.Stages;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LBoLEntitySideloader.ReflectionHelpers;
using System.Text.RegularExpressions;
using System.Threading;
using static LBoL.Core.CrossPlatformHelper;
using System.Net.NetworkInformation;
using static DebugMode.Plugin;

namespace DebugMode
{
    [BepInPlugin(GUID, "Debug Mode", version)]
    [BepInProcess("LBoL.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "neo.lbol.debugMode";
        public const string version = "0.8.0";

        private static readonly Harmony harmony = new Harmony(GUID);

        internal static BepInEx.Logging.ManualLogSource log;

        public static ConfigEntry<KeyboardShortcut> StartDebugRunKey;
        public static ConfigEntry<KeyboardShortcut> BattleLog;
        // alpha only
        public static ConfigEntry<KeyboardShortcut> DevConsole;
        public static ConfigEntry<KeyboardShortcut> DebugMenu;


        private void Awake()
        {
            log = Logger;

            StartDebugRunKey = Config.Bind("Keys", "StartDebugRunKey", new KeyboardShortcut(KeyCode.F5), new ConfigDescription("Starts a run with special debug map nodes. Only works if not in run."));

            BattleLog = Config.Bind("Keys", "BattleLog", new KeyboardShortcut(KeyCode.F4), new ConfigDescription("BattleLog (only in battle)"));

            DevConsole = Config.Bind("Keys", "DevConsole", new KeyboardShortcut(KeyCode.F2), new ConfigDescription(@"DevConsole. Use ""help"" command to list available commands. Alpha only."));
            DebugMenu = Config.Bind("Keys", "DebugMenu", new KeyboardShortcut(KeyCode.F3), new ConfigDescription("DebugMenu. Opens different menu depending whether player is in mainmenu, battle or gamemap. Alpha only :("));



            // very important. Without it the entry point MonoBehaviour gets destroyed
            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = HideFlags.HideAndDontSave;

            harmony.PatchAll();


        }

        private void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchSelf();
        }




        private void Update()
        {

            if (StartDebugRunKey.Value.IsDown())
            {
                if (GameMaster.Instance?.CurrentGameRun == null)
                {

                    DebugMap.ShuffleStations();

                    StartGameData startGameData = new StartGameData();
                    startGameData.StagesCreateFunc = () => new Stage[]
                    {
                        Library.CreateStage<BattleAdvTest>(),
                    };
                    startGameData.DebutAdventure = typeof(Debut);
                    UiManager.GetPanel<StartGamePanel>().Show(startGameData);
                }
                else
                {
                    log.LogInfo("Run has already been started.");
                }

            }
        }




        public static UnityEngine.Events.UnityAction CoroutineWrapper(IEnumerator routine)
        {
            return delegate {
                var gm = GameMaster.Instance;
                if (gm == null)
                    throw new InvalidOperationException("GameMaster is null");
                gm.StartCoroutine(routine);
            };
        }
            





    }
}
