using Cysharp.Threading.Tasks;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core;
using LBoL.EntityLib.Exhibits.Shining;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using LBoL.Presentation.UI;
using LBoL.Presentation;
using LBoLEntitySideloader.ReflectionHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DebugMode.Plugin;

namespace DebugMode
{
    public class DebugPanel
    {

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

                button.onClick.AddListener(action);

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
                                            if (TypeFactoryReflection.AccessTypeLocalizers(factype)().TryGetValue((string)ConfigReflection.GetIdField(configType)?.GetValue(conf), out Dictionary<string, object> terms) && terms.TryGetValue("Name", out object name))
                                            {
                                                localizedName = name?.ToString();
                                            }

                                        }

                                        string wrappedText = Regex.Replace(conf.ToString(), "(.{1," + maxLineLength + @"})(\s+|$)", "$1" + System.Environment.NewLine);

                                        if (!string.IsNullOrEmpty(localizedName))
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
                    .SelectMany(rpe => new Card[] { Library.CreateCard(rpe.Elem), Library.CreateCard(rpe.Elem) });


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
                            r.transform.localPosition += new Vector3(0f, topShift - (rows * rowHeight));

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
                if (rewardPanel.IsVisible)
                {
                    rewardPanel.Hide();
                }
            }

        }
    }
}
