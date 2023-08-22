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
            static bool Prefix(BattleAdvTest __instance, ref GameMap __result)
            {
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


                __result = GameMap.CreateMultiRoute(__instance.Boss.Id, debugStations.ToArray());

                return false;
            }
        }


        static List<StationType> debugStations = new List<StationType>();

        static void ShuffleStations(ulong? seed = null)
        {
            debugStations = new List<StationType>();

            debugStations.AddRange(Enumerable.Repeat(StationType.BattleAdvTest, 85));
            debugStations.AddRange(Enumerable.Repeat(StationType.Shop, 30));
            debugStations.AddRange(Enumerable.Repeat(StationType.Gap, 30));


            seed ??= RandomGen.GetRandomSeed();

            debugStations.Shuffle(new RandomGen(seed.Value));
        }






        private void Update()
        {

            if (StartDebugRunKey.Value.IsDown())
            {
                if (GameMaster.Instance?.CurrentGameRun == null)
                {

                    ShuffleStations();

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




        internal class TypeCache
        {


            public HashSet<Type> cEx = new HashSet<Type>();
            public HashSet<Type> uEx = new HashSet<Type>();
            public HashSet<Type> rEx = new HashSet<Type>();
            public HashSet<Type> mEx = new HashSet<Type>();
            public HashSet<Type> sEx = new HashSet<Type>();

            public HashSet<Type> theRest = new HashSet<Type>();


            public void BuildCache()
            {
                foreach (var tc in Library.EnumerateExhibitTypes().OrderByDescending(tc => tc.config.Index))
                {
                    switch (tc.config.Rarity)
                    {
                        case Rarity.Common:
                            this.cEx.Add(tc.exhibitType);
                            break;
                        case Rarity.Uncommon:
                            this.uEx.Add(tc.exhibitType);
                            break;
                        case Rarity.Rare:
                            this.rEx.Add(tc.exhibitType);
                            break;
                        case Rarity.Mythic:
                            this.mEx.Add(tc.exhibitType);
                            break;
                        case Rarity.Shining:
                            this.sEx.Add(tc.exhibitType);
                            break;
                        default:
                            log.LogWarning($"Exhibit {tc.exhibitType.Name} doesn't have a rarity");
                            break;
                    }
                }

                Library.EnumerateCardTypes().Where(tc => tc.config.Type ==
                CardType.Misfortune || tc.config.Type == CardType.Status || tc.config.Type == CardType.Unknown
                //|| !tc.config.IsPooled
                ).
                Select(tc => tc.cardType).Do(t => this.theRest.Add(t));

            }
        }


        [HarmonyPatch(typeof(SelectDebugPanel))]
        public class SelectDebugPanel_Patch
        {

            static TypeCache typeCache = null;



            [HarmonyPatch(nameof(SelectDebugPanel.OnShowing))]
            [HarmonyPostfix]
            static void OnShowing_Postfix()
            {
                // clear cache
                typeCache = new TypeCache();
                typeCache.BuildCache();

            }


            [HarmonyPatch(nameof(SelectDebugPanel.CreateEnemyButtons))]
            [HarmonyPostfix]
            static void CreateEnemyButtons_Postfix(SelectDebugPanel __instance)
            {
                // panel with the most space
                if (__instance._selectionType == SelectDebugPanel.SelectionType.RealName)
                {

                    if (typeCache == null)
                    {
                        typeCache = new TypeCache();
                        typeCache.BuildCache();
                    }

                    CreateButton(__instance, "Gimme Commons", CoroutineWrapper(PoolCards(new CardWeightTable(RarityWeightTable.OnlyCommon))));
                    CreateButton(__instance, "Gimme Uncommons", CoroutineWrapper(PoolCards(new CardWeightTable(RarityWeightTable.OnlyUncommon))));
                    CreateButton(__instance, "Gimme Rares", CoroutineWrapper(PoolCards(new CardWeightTable(RarityWeightTable.OnlyRare))));

                    CreateButton(__instance, "Gimme the rest", CoroutineWrapper(
                        PoolCards(new CardWeightTable(RarityWeightTable.AllOnes, OwnerWeightTable.Valid,
                        new CardTypeWeightTable(0f, 0f, 0f, 0f, 0f, 1f, 0f, 0f)
                        ),
                        false,
                        typeCache.theRest)));


                    CreateButton(__instance, "Gimme Common Exhibits", CoroutineWrapper(PoolExhibits(typeCache.cEx)));
                    CreateButton(__instance, "Gimme Uncommon Exhibits", CoroutineWrapper(PoolExhibits(typeCache.uEx)));
                    CreateButton(__instance, "Gimme Rare Exhibits", CoroutineWrapper(PoolExhibits(typeCache.rEx)));
                    CreateButton(__instance, "Gimme Mythics", CoroutineWrapper(PoolExhibits(typeCache.mEx)));
                    CreateButton(__instance, "Gimme Shinnies", CoroutineWrapper(PoolExhibits(typeCache.sEx)));

                    CreateButton(__instance, "Remove Cards", CoroutineWrapper(RemoveCards()));

                    CreateButton(__instance, "Gimme Cash", () => {
                        GameMaster.Instance?.CurrentGameRun.GainMoney(10000, false);
                    });



                    CreateButton(__instance, "Output All Config", async () => await OutputConfig());


                    // not working..
                    //CreateButton(__instance, "Screenshot Cards", CoroutineWrapper(ScreencapCards()));
                }


            }




            public static Button CreateButton(SelectDebugPanel debugPanel, string desc, UnityEngine.Events.UnityAction action)
            {
                var button = UnityEngine.Object.Instantiate<Button>(debugPanel.buttonTemplate, debugPanel.layout);

                button.transform.GetComponentInChildren<TextMeshProUGUI>().text = desc;

                button.onClick.AddListener( action );

                // reset to make buttons clickable again
                button.onClick.AddListener(delegate
                {
                    debugPanel.SelectAdventure();
                    debugPanel.SelectRealName();
                });
                
                button.gameObject.SetActive(true);

                return button;

            }

            static SemaphoreSlim semaphoreOutputConfig = new SemaphoreSlim(1);

            public static async UniTask OutputConfig()
            {
                if (await semaphoreOutputConfig.WaitAsync(0))
                {
                    try
                    {
                        var dir = $"readableConfig_{VersionInfo.Current.Version}";
                        Directory.CreateDirectory(dir);

                        var maxLineLength = 100;

                        Action<Type> writeConfig = (configType) =>
                        {

                            using (FileStream fileStream = File.Open($"{dir}/{configType.Name}.txt", FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8) { AutoFlush = true })
                                {
                                    var allConfig = (Array)ConfigReflection.GetArrayField(configType).GetValue(null);
                                    foreach (var conf in allConfig)
                                    {

                                        var localizedName = "";
                                        // finds localized name associated with config if it has one
                                        if (ConfigReflection.GetConfig2FactoryType().TryGetValue(configType, out Type factype))
                                        { 
                                            if(TypeFactoryReflection.AccessTypeLocalizers(factype)().TryGetValue((string)ConfigReflection.GetIdField(configType)?.GetValue(conf), out Dictionary<string, object> terms) && terms.TryGetValue("Name", out object name))
                                            {
                                                localizedName = name?.ToString();
                                            }
                                        
                                        }

                                        string wrappedText = Regex.Replace( conf.ToString(), "(.{1," + maxLineLength + @"})(\s+|$)", "$1" + System.Environment.NewLine);

                                        if(!string.IsNullOrEmpty(localizedName))
                                            streamWriter.WriteLine($"{localizedName}: ");
                                        streamWriter.Write(wrappedText);
                                        streamWriter.WriteLine("------------------------");
                                    }
                                }
                            }
                        };


                        foreach (var ct in ConfigReflection.AllConfigTypes())
                        {
                            await UniTask.RunOnThreadPool(() => writeConfig(ct));
                        }


                        log.LogInfo($"Done writing config at LBoL/{dir}/!");
                    }
                    finally
                    {
                        semaphoreOutputConfig.Release();
                    }

                }
                else
                {
                    log.LogInfo("Already writing config, please wait.");
                }



            }


            // not working..
            public static IEnumerator DoScreencaps()
            {

                var cardPanel = UiManager.GetPanel<SelectCardPanel>();
                log.LogDebug($"CardPanel: {cardPanel}");


                var cardLayout = cardPanel.normalSelectCardRoot.Find("ScrollRect")?.Find("CardLayout");

                if (cardLayout == null)
                    throw new NullReferenceException("Can't find CardLayout in transform hierarchy");

                var dir = $"card_screencaps_{VersionInfo.Current.Version}";
                Directory.CreateDirectory(dir);

                var uiCam = GameObject.Find("UICamera").GetComponent<Camera>();

                log.LogDebug($"uicam: {uiCam}");

                foreach (Transform trans in cardLayout)
                {

                    yield return new WaitForEndOfFrame();


                    var rectT = trans.gameObject.GetComponent<RectTransform>();

                    var card = trans.gameObject.GetComponent<CardWidget>().Card;

                    var cardId = card.Id;





/*                    RenderTexture rt = RenderTexture.GetTemporary(width, height, 24);
                    rt.antiAliasing = 8;
                    rt.filterMode = FilterMode.Trilinear;
                    rt.anisoLevel = 16;

                    uiCam.targetTexture = rt;
                    uiCam.clearFlags = CameraClearFlags.Color;
                    uiCam.backgroundColor = Color.black;
                    uiCam.gameObject.SetActive(true);

                    uiCam.Render();*/


				    //UiManager.GetPanel<CardDetailPanel>().Show(new CardDetailPayload(rectT, card));


                    Vector3[] corners = new Vector3[4];
                    rectT.GetWorldCorners(corners);


                    for (var i = 0; i < 4; i++)
                    {
                        Debug.Log("World Corner " + i + " : " + corners[i]);
                    }


                    int width = (int)Vector3.Distance(corners[0], corners[3]);
                    //((int)corners[1].x - (int)corners[2].x);
                    int height = (int)Vector3.Distance(corners[0], corners[1]);
                    // (int)corners[1].y - (int)corners[0].y;
                    var startX = corners[1].x;
                    var startY = corners[1].y;


/*                    var width = Convert.ToInt32(rectT.rect.width);
                    var height = Convert.ToInt32(rectT.rect.height);
                    Vector2 vector2 = rectT.position;
                    Debug.Log(vector2);
                    var startX = vector2.x - width / 2;
                    var startY = vector2.y - height / 2;*/

                    Debug.Log($"{startX} : {startY}");
                    Debug.Log($"{width} : {height}");




                    var tex = new Texture2D(width, height);
                    tex.ReadPixels(new Rect(startX, startY, width, height), 0, 0);
                    tex.Apply();

                    // Encode texture into PNG
                    var bytes = tex.EncodeToPNG();

                    File.WriteAllBytes(Path.Combine(dir, $"{cardId}.png"), bytes);

                    Destroy(tex);
                    //UiManager.GetPanel<CardDetailPanel>().Hide(false);


                }
            }

            // not working..
            public static IEnumerator ScreencapCards()
            {

                var gr = GameMaster.Instance.CurrentGameRun;

                if (gr == null)
                    yield break;

                var cards = Library.EnumerateCardTypes().Select(tc => Library.CreateCard(tc.cardType)).OrderBy(c => c.CardType).Where((v, i) => i < 10);
                //debug


                // UiManager.GetPanel<SelectDebugPanel>().buttonTemplate

                var panel = UiManager.GetPanel<SelectCardPanel>();

                var srcCapButton = UnityEngine.Object.Instantiate<Button>(panel.confirmButton, panel.normalSelectCardRoot.Find("ButtonLayout"));

                //var srcCapButton = UnityEngine.Object.Instantiate<Button>(panel.confirmButton, __instance.layout);


                srcCapButton.transform.GetComponentInChildren<TextMeshProUGUI>().text = "Take screencaps..";

                srcCapButton.onClick.AddListener(CoroutineWrapper(DoScreencaps()));

                srcCapButton.gameObject.SetActive(true);


                SelectCardInteraction interaction = new SelectCardInteraction(0, cards.Count(), cards, SelectedCardHandling.DoNothing)
                {
                    CanCancel = true,
                    Description = "Screencapping cards..", 
                };

                yield return gr.InteractionViewer.View(interaction);

                log.LogInfo("deez");

                Destroy(srcCapButton);

            }


            public static IEnumerator RemoveCards()
            {

                var gr = GameMaster.Instance.CurrentGameRun;

                if (gr == null)
                    yield break;

                List<Card> list = gr.BaseDeck.ToList<Card>();
                if (list.Count > 0)
                {
                    SelectCardInteraction interaction = new SelectCardInteraction(0, list.Count(), list, SelectedCardHandling.DoNothing)
                    {
                        CanCancel = true,
                        Description = "Remove any number of cards"
                    };
                    yield return gr.InteractionViewer.View(interaction);
                    if (!interaction.IsCanceled)
                    {
                        gr.RemoveDeckCards(interaction.SelectedCards, true);
                    }
                }
            }


            public static IEnumerator PoolExhibits(HashSet<Type> exhibits)
            {
                var gr = GameMaster.Instance.CurrentGameRun;

                if (gr == null)
                    yield break;

                foreach (var ex in gr.Player.Exhibits)
                {
                    exhibits.Remove(ex.GetType());
                }

                var list = exhibits.Select(t => Library.CreateExhibit(t));


                var rewardInteraction = new RewardInteraction(list)
                {
                    CanCancel = false,
                };

                wt_RewardInteraction.Add(rewardInteraction, new Rewards_ExtraProps() { lotsOfExhibits = true });

                yield return gr.InteractionViewer.View(rewardInteraction);
                
                
            }



            public static IEnumerator PoolCards(CardWeightTable cardWeightTable, bool colorLimit = true, IEnumerable<Type> additionalCards = null)
            {

                var gr = GameMaster.Instance.CurrentGameRun;

                if (gr == null)
                    yield break;

                var cards = GameMaster.Instance.CurrentGameRun.CreateValidCardsPool(cardWeightTable, new ManaGroup?(gr.BaseMana), colorLimit, true)
                    .SelectMany( rpe => new Card[] { Library.CreateCard(rpe.Elem), Library.CreateCard(rpe.Elem) });


                if (additionalCards != null)
                {
                    cards = cards.Concat(additionalCards.Select(t => Library.CreateCard(t)));
                }


                // modded cards will probably have highest index and tools appear last
                cards = cards.OrderBy(c => c.CardType).ThenByDescending(c => c.Config.Index).ToList();

                cards.Where((c, i) => i % 2 == 1 && c.CanUpgradeAndPositive).Do(c => c.Upgrade());

                cards.Do(c => c.GameRun = gr);

                


                SelectCardInteraction interaction = new SelectCardInteraction(0, cards.Count(), cards, SelectedCardHandling.DoNothing)
                {
                    CanCancel = false,
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

            static public float anchorXcoord = -1750f;

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
