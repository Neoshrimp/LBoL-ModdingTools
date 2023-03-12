using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Attributes;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActionRecord;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Dialogs;
using LBoL.Core.GapOptions;
using LBoL.Core.Helpers;
using LBoL.Core.Intentions;
using LBoL.Core.JadeBoxes;
using LBoL.Core.PlatformHandlers;
using LBoL.Core.Randoms;
using LBoL.Core.SaveData;
using LBoL.Core.Stations;
using LBoL.Core.Stats;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Adventures;
using LBoL.EntityLib.Adventures.Common;
using LBoL.EntityLib.Adventures.FirstPlace;
using LBoL.EntityLib.Adventures.Shared12;
using LBoL.EntityLib.Adventures.Shared23;
using LBoL.EntityLib.Adventures.Stage1;
using LBoL.EntityLib.Adventures.Stage2;
using LBoL.EntityLib.Adventures.Stage3;
using LBoL.EntityLib.Cards.Character.Cirno;
using LBoL.EntityLib.Cards.Character.Cirno.FairySupport;
using LBoL.EntityLib.Cards.Character.Koishi;
using LBoL.EntityLib.Cards.Character.Marisa;
using LBoL.EntityLib.Cards.Character.Reimu;
using LBoL.EntityLib.Cards.Character.Sakuya;
using LBoL.EntityLib.Cards.Devel;
using LBoL.EntityLib.Cards.Neutral;
using LBoL.EntityLib.Cards.Neutral.Black;
using LBoL.EntityLib.Cards.Neutral.Blue;
using LBoL.EntityLib.Cards.Neutral.Green;
using LBoL.EntityLib.Cards.Neutral.MultiColor;
using LBoL.EntityLib.Cards.Neutral.NoColor;
using LBoL.EntityLib.Cards.Neutral.Red;
using LBoL.EntityLib.Cards.Neutral.TwoColor;
using LBoL.EntityLib.Cards.Neutral.White;
using LBoL.EntityLib.Cards.Other.Adventure;
using LBoL.EntityLib.Cards.Other.Enemy;
using LBoL.EntityLib.Cards.Other.Misfortune;
using LBoL.EntityLib.Cards.Other.Tool;
using LBoL.EntityLib.Devel;
using LBoL.EntityLib.Dolls;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.EnemyUnits.Character.DreamServants;
using LBoL.EntityLib.EnemyUnits.Lore;
using LBoL.EntityLib.EnemyUnits.Normal;
using LBoL.EntityLib.EnemyUnits.Normal.Bats;
using LBoL.EntityLib.EnemyUnits.Normal.Drones;
using LBoL.EntityLib.EnemyUnits.Normal.Guihuos;
using LBoL.EntityLib.EnemyUnits.Normal.Maoyus;
using LBoL.EntityLib.EnemyUnits.Normal.Ravens;
using LBoL.EntityLib.EnemyUnits.Opponent;
using LBoL.EntityLib.Exhibits;
using LBoL.EntityLib.Exhibits.Adventure;
using LBoL.EntityLib.Exhibits.Common;
using LBoL.EntityLib.Exhibits.Mythic;
using LBoL.EntityLib.Exhibits.Seija;
using LBoL.EntityLib.Exhibits.Shining;
using LBoL.EntityLib.JadeBoxes;
using LBoL.EntityLib.Mixins;
using LBoL.EntityLib.PlayerUnits;
using LBoL.EntityLib.Stages;
using LBoL.EntityLib.Stages.NormalStages;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoL.EntityLib.StatusEffects.Enemy.SeijaItems;
using LBoL.EntityLib.StatusEffects.Marisa;
using LBoL.EntityLib.StatusEffects.Neutral;
using LBoL.EntityLib.StatusEffects.Neutral.Black;
using LBoL.EntityLib.StatusEffects.Neutral.Blue;
using LBoL.EntityLib.StatusEffects.Neutral.Green;
using LBoL.EntityLib.StatusEffects.Neutral.Red;
using LBoL.EntityLib.StatusEffects.Neutral.TwoColor;
using LBoL.EntityLib.StatusEffects.Neutral.White;
using LBoL.EntityLib.StatusEffects.Others;
using LBoL.EntityLib.StatusEffects.Reimu;
using LBoL.EntityLib.StatusEffects.Sakuya;
using LBoL.EntityLib.UltimateSkills;
using LBoL.Presentation;
using LBoL.Presentation.Animations;
using LBoL.Presentation.Bullet;
using LBoL.Presentation.Effect;
using LBoL.Presentation.I10N;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Transitions;
using LBoL.Presentation.UI.Widgets;
using LBoL.Presentation.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Untitled;
using Untitled.ConfigDataBuilder;
using Untitled.ConfigDataBuilder.Base;
using Debug = UnityEngine.Debug;


namespace DebugMode
{
    [BepInPlugin(GUID, "Debug Mode", version)]
    [BepInProcess("LBoL.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "neo.lbol.debugMode";
        public const string version = "0.7.0";

        private static readonly Harmony harmony = new Harmony(GUID);

        internal static BepInEx.Logging.ManualLogSource log;

        public static ConfigEntry<KeyboardShortcut> StartDebugRunKey;

        private void Awake()
        {
            log = Logger;

            StartDebugRunKey = Config.Bind("Keys", "StartDebugRunKey", new KeyboardShortcut(KeyCode.F5), new ConfigDescription("Starts a run with special debug map nodes. Only works if not in run."));


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
                    log.LogInfo("Run is already started.");
                }

            }
        }


        [HarmonyPatch(typeof(UiManager), nameof(UiManager.Awake))]
        class UiManager_Patch
        {
            static void Postfix(UiManager __instance)
            {
                var iam = __instance.inputActions.FindActionMap("Debug", false);
                iam.Enable();
                log.LogInfo(iam["DebugBattleLog"]);
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


        public class Rewards_ExtraProps
        {
            public bool lotsOfExhibits = false;
        }

        static public ConditionalWeakTable<RewardInteraction, Rewards_ExtraProps> wt_RewardInteraction = new ConditionalWeakTable<RewardInteraction, Rewards_ExtraProps>();

        static public ConditionalWeakTable<ShowRewardContent, Rewards_ExtraProps> wt_ShowRewardContent = new ConditionalWeakTable<ShowRewardContent, Rewards_ExtraProps>();




        [HarmonyPatch(typeof(SelectDebugPanel))]
        class SelectDebugPanel_Patch
        {



            static HashSet<Type> cEx = new HashSet<Type>();
            static HashSet<Type> uEx = new HashSet<Type>();
            static HashSet<Type> rEx = new HashSet<Type>();
            static HashSet<Type> mEx = new HashSet<Type>();
            static HashSet<Type> sEx = new HashSet<Type>();


            [HarmonyPatch(nameof(SelectDebugPanel.OnShowing))]
            [HarmonyPostfix]
            static void Awake_Postfix()
            {
                foreach (var tc in Library.EnumerateExhibitTypes().OrderByDescending(tc => tc.config.Index))
                {
                    switch (tc.config.Rarity)
                    {
                        case Rarity.Common:
                            cEx.Add(tc.exhibitType);
                            break;
                        case Rarity.Uncommon:
                            uEx.Add(tc.exhibitType);
                            break;
                        case Rarity.Rare:
                            rEx.Add(tc.exhibitType);
                            break;
                        case Rarity.Mythic:
                            mEx.Add(tc.exhibitType);
                            break;
                        case Rarity.Shining:
                            sEx.Add(tc.exhibitType);
                            break;
                        default:
                            log.LogWarning($"Exhibit {tc.exhibitType.Name} doesn't have a rarity");
                            break;
                    }
                }
            }



            [HarmonyPatch(nameof(SelectDebugPanel.CreateEnemyButtons))]
            [HarmonyPostfix]
            static void CreateEnemyButtons_Postfix(SelectDebugPanel __instance)
            {
                // panel with the most space
                if (__instance._selectionType == SelectDebugPanel.SelectionType.RealName)
                {
                    CreateButton(__instance, "Gimme Commons", CoroutineWrapper(PoolCards(new CardWeightTable(RarityWeightTable.OnlyCommon))));
                    CreateButton(__instance, "Gimme Uncommons", CoroutineWrapper(PoolCards(new CardWeightTable(RarityWeightTable.OnlyUncommon))));
                    CreateButton(__instance, "Gimme Rares", CoroutineWrapper(PoolCards(new CardWeightTable(RarityWeightTable.OnlyRare))));

                    CreateButton(__instance, "Gimme other cards", CoroutineWrapper(
                        PoolCards(new CardWeightTable(RarityWeightTable.AllOnes, OwnerWeightTable.Valid, 
                        CardTypeWeightTable.OnlyMisfortune
                        //new CardTypeWeightTable(0f, 0f, 0f, 0f, 1f, 1f, 0f)
                        ), 
                        false)));


                    CreateButton(__instance, "Gimme Common Exhibits", CoroutineWrapper(PoolExhibits(cEx)));
                    CreateButton(__instance, "Gimme Uncommon Exhibits", CoroutineWrapper(PoolExhibits(uEx)));
                    CreateButton(__instance, "Gimme Rare Exhibits", CoroutineWrapper(PoolExhibits(rEx)));
                    CreateButton(__instance, "Gimme Mythics", CoroutineWrapper(PoolExhibits(mEx)));
                    CreateButton(__instance, "Gimme Shinnies", CoroutineWrapper(PoolExhibits(sEx)));

                }

            }




            public static void CreateButton(SelectDebugPanel debugPanel, string desc, UnityEngine.Events.UnityAction action)
            {
                var button = UnityEngine.Object.Instantiate<Button>(debugPanel.buttonTemplate, debugPanel.layout);

                button.transform.GetComponentInChildren<TextMeshProUGUI>().text = desc;

                button.onClick.AddListener( action );

                // reset to make button clickable again
                button.onClick.AddListener(delegate
                {
                    debugPanel.SelectAdventure();
                    debugPanel.SelectRealName();
                });
                
                button.gameObject.SetActive(true);

            }


            public static IEnumerator PoolExhibits(HashSet<Type> exhibits)
            {
                var gr = GameMaster.Instance.CurrentGameRun;

                foreach (var ex in gr.Player.Exhibits)
                {
                    exhibits.Remove(ex.GetType());
                }

                var list = exhibits.Select(t => Library.CreateExhibit(t));


                var rewardInteraction = new RewardInteraction(list)
                {
                    CanCancel = true,
                };

                wt_RewardInteraction.Add(rewardInteraction, new Rewards_ExtraProps() { lotsOfExhibits = true });

                yield return gr.InteractionViewer.View(rewardInteraction);
                
                
            }



            public static IEnumerator PoolCards(CardWeightTable cardWeightTable, bool colorLimit = true)
            {

                var gr = GameMaster.Instance.CurrentGameRun;

                var cards = GameMaster.Instance.CurrentGameRun.CreateValidCardsPool(cardWeightTable, new ManaGroup?(gr.BaseMana), colorLimit, true)
                    .SelectMany( rpe => new Card[] { Library.CreateCard(rpe.Elem), Library.CreateCard(rpe.Elem) });


                // modded cards will probably have highest index and tools appear last
                cards = cards.OrderBy(c => c.CardType).ThenByDescending(c => c.Config.Index).ToList();

                cards.Do(c => c.GameRun = gr);

                cards.Where((c, i) => i % 2 == 1 && c.CanUpgradeAndPositive).Do(c => c.Upgrade());


                SelectCardInteraction interaction = new SelectCardInteraction(0, cards.Count(), cards, SelectedCardHandling.DoNothing)
                {
                    CanCancel = true,
                    Description = "Pick any number of cards"
                };

                yield return gr.InteractionViewer.View(interaction);

                gr.AddDeckCards(interaction.SelectedCards);


            }



        }


        [HarmonyPatch]
        class RewardPanel_ViewReward_Patch
        {

            static Type innerEnumType;

            static IEnumerable<MethodBase> TargetMethods()
            {
                innerEnumType = typeof(RewardPanel).GetNestedTypes(AccessTools.allDeclared).Where(t => t.Name.Contains("ViewReward")).Single();
                yield return AccessTools.Method(innerEnumType, "MoveNext");
            }


            static ShowRewardContent AttachProperty(ShowRewardContent target, RewardInteraction source)
            {
                if (wt_RewardInteraction.TryGetValue(source, out Rewards_ExtraProps ep))
                {
                    wt_ShowRewardContent.Add(target, ep);
                }
                return target;
            }


            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                foreach (var ci in instructions)
                {
                    if (ci.Is(OpCodes.Newobj, AccessTools.Constructor(typeof(ShowRewardContent))))
                    {
                        yield return ci;
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(innerEnumType, "interaction"));
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RewardPanel_ViewReward_Patch), 
                            nameof(RewardPanel_ViewReward_Patch.AttachProperty)));
                    }
                    else
                    {
                        yield return ci;
                    }
                }
            }

                
        }


        [HarmonyPatch(typeof(RewardPanel), nameof(RewardPanel.OnShowing))]
        class RewardPanel_OnShowing_Patch
        {

            static public float newWidgetScale = 0.4f;

            static public float topShift = 500f;

            static public float widgetWidth = 270f;

            static public float rowHeight = 420f;

            static public int rowCount = 14;

            static void Postfix(RewardPanel __instance, ShowRewardContent showRewardContent)
            {


                if (wt_ShowRewardContent.TryGetValue(showRewardContent, out Rewards_ExtraProps ep))
                {
                    if (ep.lotsOfExhibits)
                    {

                        var rewards = __instance.rewardLayout.GetComponentsInChildren<RewardWidget>();

                        
                        var i = 0; 
                        var rows = 0;

                        var anchorXcoord = -1750;
                        foreach (var r in rewards)
                        {
                            r.transform.transform.localScale = new Vector3(newWidgetScale, newWidgetScale, newWidgetScale);
                            r.transform.localPosition += new Vector3(0f, topShift-(rows*rowHeight));

                            r.transform.localPosition = new Vector3(anchorXcoord + (widgetWidth * i), r.transform.localPosition.y, r.transform.localPosition.z);

                            i++;
                            if (i % rowCount == 0)
                            {
                                rows++;
                                i = 0;
                            }
                        }


                    }
                }
            }
        }


        [HarmonyPatch(typeof(RewardPanel), nameof(RewardPanel.ResetAllPositions))]
        class ResetAllPositions_Patch
        {

            static bool Prefix(RewardPanel __instance, bool approaching)
            {
                if (__instance._rewardWidgets.Count > 0)
                {
                    // lazy way to skip widget position reset on picking exhibit
                    if (__instance._rewardWidgets[0].transform.localScale == Vector3.one * RewardPanel_OnShowing_Patch.newWidgetScale)
                    {
                        return false;
                    }
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(HuiyeBaoxiang), nameof(HuiyeBaoxiang.SpecialGain))]
        class HuiyeBaoxiang_Patch
        {
            static void Prefix()
            {
                var rewardPanel = UiManager.GetPanel<RewardPanel>();
                if(rewardPanel.IsVisible)
                {
                    rewardPanel.Hide();
                }
            }

        }



    }
}
