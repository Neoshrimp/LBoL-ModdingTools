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
using LBoL.Presentation.Units;

namespace DebugMode
{
    [BepInPlugin(GUID, "Debug Mode", version)]
    [BepInProcess("LBoL.exe")]
    [BepInDependency("neo.lbol.frameworks.entitySideloader", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "neo.lbol.debugMode";
        public const string version = "1.1.0";

        private static readonly Harmony harmony = new Harmony(GUID);

        internal static BepInEx.Logging.ManualLogSource log;

        public static ConfigEntry<KeyboardShortcut> StartDebugRunKey;
        public static ConfigEntry<KeyboardShortcut> RestartRunKey;
        public static ConfigEntry<KeyboardShortcut> BattleLog;
        // alpha only
        public static ConfigEntry<KeyboardShortcut> DevConsole;
        public static ConfigEntry<KeyboardShortcut> DebugMenu;


        private void Awake()
        {
            log = Logger;

            StartDebugRunKey = Config.Bind("Keys", "StartDebugRunKey", new KeyboardShortcut(KeyCode.F5), new ConfigDescription("Starts a run with special debug map nodes. Only works if not in run."));

            BattleLog = Config.Bind("Keys", "BattleLog", new KeyboardShortcut(KeyCode.F3), new ConfigDescription("BattleLog (only in battle)"));

            DevConsole = Config.Bind("Keys", "DevConsole", new KeyboardShortcut(KeyCode.F2), new ConfigDescription(@"DevConsole. Use ""help"" command to list available commands. Alpha only."));
            DebugMenu = Config.Bind("Keys", "DebugMenu", new KeyboardShortcut(KeyCode.F4), new ConfigDescription("DebugMenu. Opens different menu depending whether player is in mainmenu, battle or gamemap. Alpha only :("));

            RestartRunKey = Config.Bind("Keys", "RestartRun", new KeyboardShortcut(KeyCode.F8), "Restart current run with the same seed and character. Does not respect custom loadouts.");


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

            if (RestartRunKey.Value.IsDown())
            {
                var gr = GameMaster.Instance?.CurrentGameRun;
                if (gr != null)
                {
                    var seed = gr.RootSeed;
                    var difficulty = gr.Difficulty;
                    var puzzles = gr.Puzzles;
                    var oldPlayer = gr.Player;
                    var newPlayer = Library.CreatePlayerUnit(oldPlayer.GetType());
                    newPlayer.SetUs(Library.CreateUs(oldPlayer.Us.GetType()));
                    var playerType = gr.PlayerType; // 2do does not respect custom loadouts
                    var exhibitId = "";
                    var startingDeck = new List<string>();
                    switch (playerType)
                    {
                        case LBoL.Core.Units.PlayerType.TypeA:
                            exhibitId = oldPlayer.Config.ExhibitA;
                            startingDeck.AddRange(oldPlayer.Config.DeckA);
                            break;
                        case LBoL.Core.Units.PlayerType.TypeB:
                            exhibitId = oldPlayer.Config.ExhibitB;
                            startingDeck.AddRange(oldPlayer.Config.DeckB);
                            break;
                        default:
                            break;
                    }
                    var stages = gr.Stages.Select(s => Library.CreateStage(s.GetType()));
                    var jadeboxes = gr.JadeBoxes.Select(jb => Library.CreateJadeBox(jb.GetType()));
                    var revealFate = gr.ShowRandomResult;


                    Utils.InstaAbandonGamerun();


                    GameMaster.StartGame(
                        seed: seed,
                        difficulty: difficulty,
                        puzzles: puzzles,
                        player: newPlayer,
                        playerType: playerType,
                        initExhibit: Library.CreateExhibit(exhibitId),
                        initMoneyOverride: null,
                        deck: startingDeck.Select(s => Library.CreateCard(s)),
                        stages: stages,
                        debutAdventureType: typeof(Debut),
                        jadeBoxes: jadeboxes,
                        gameMode: GameMode.FreeMode,
                        showRandomResult: revealFate);
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
