using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.SaveData;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Adventures;
using LBoL.EntityLib.PlayerUnits;
using LBoL.EntityLib.Stages;
using LBoL.EntityLib.Stages.NormalStages;
using LBoL.Presentation;
using LBoL.Presentation.I10N;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DebugMode.AlphaDebug
{
    public class GameDebugMenuUI : MonoBehaviour
    {
        public static void Notify(string content, float time = 5f)
        {
            GameDebugMenuUI.Notifications.Add(new GameDebugMenuUI.Notification
            {
                Content = content,
                Time = time
            });
        }

        public static bool NotifyGunTime { get; private set; }

        private void Awake()
        {
            foreach (KeyValuePair<GameDebugMenuUI.WindowID, GameDebugMenuUI.DebugWindow> keyValuePair in this._windowTable)
            {
                GameDebugMenuUI.WindowID windowID;
                GameDebugMenuUI.DebugWindow debugWindow;
                keyValuePair.Deconstruct(out windowID, out debugWindow);
                GameDebugMenuUI.DebugWindow debugWindow2 = debugWindow;
                debugWindow2.Parent = this;
                debugWindow2.Rect = debugWindow2.InitRect;
                debugWindow2.OnAwake();
            }
            UiManager.Instance.DebugMenuAction.performed += delegate (InputAction.CallbackContext _)
            {
                this.OnDebugMenuClick();
            };
            L10nManager.LocaleChanged += delegate
            {
                foreach (KeyValuePair<GameDebugMenuUI.WindowID, GameDebugMenuUI.DebugWindow> keyValuePair2 in this._windowTable)
                {
                    GameDebugMenuUI.WindowID windowID2;
                    GameDebugMenuUI.DebugWindow debugWindow3;
                    keyValuePair2.Deconstruct(out windowID2, out debugWindow3);
                    debugWindow3.OnLocaleChanged();
                }
            };
        }

        private void OnDebugMenuClick()
        {
            if (this._windowTable[GameDebugMenuUI.WindowID.MainMenu].IsActive)
            {
                this._windowTable[GameDebugMenuUI.WindowID.MainMenu].IsActive = false;
                return;
            }
            if (this._windowTable[GameDebugMenuUI.WindowID.GameRunEntry].IsActive)
            {
                this._windowTable[GameDebugMenuUI.WindowID.GameRunEntry].IsActive = false;
                return;
            }
            if (this._windowTable[GameDebugMenuUI.WindowID.BattleEntry].IsActive)
            {
                this._windowTable[GameDebugMenuUI.WindowID.BattleEntry].IsActive = false;
                return;
            }
            GameRunController currentGameRun = Singleton<GameMaster>.Instance.CurrentGameRun;
            if (currentGameRun == null)
            {
                this._windowTable[GameDebugMenuUI.WindowID.MainMenu].IsActive = true;
                return;
            }
            if (currentGameRun.Battle != null)
            {
                this._windowTable[GameDebugMenuUI.WindowID.BattleEntry].IsActive = true;
                return;
            }
            this._windowTable[GameDebugMenuUI.WindowID.GameRunEntry].IsActive = true;
        }

        private void DoWindow(int windowID)
        {
            this._windowTable[(GameDebugMenuUI.WindowID)windowID].DrawWindow();
            string tooltip = GUI.tooltip;
            if (!string.IsNullOrWhiteSpace(tooltip))
            {
                GUIContent guicontent = new GUIContent(tooltip);
                Vector2 mousePosition = Event.current.mousePosition;
                GUIStyle guistyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal =
                    {
                        background = Texture2D.whiteTexture
                    }
                };
                Vector2 vector = guistyle.CalcSize(guicontent);
                Rect rect = new Rect(mousePosition.x + 10f, mousePosition.y + 10f, vector.x + 10f, vector.y + 5f);
                Color backgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0f, 0f, 0f, 0.5f);
                GUI.Label(rect, tooltip, guistyle);
                GUI.backgroundColor = backgroundColor;
            }
            GUI.DragWindow();
        }

        private void OnGUI()
        {
            float num = (float)Screen.height / 600f;
            int num2 = (int)((float)Screen.width / num) - 100;
            int num3 = (int)((float)Screen.height / num) - 100;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(num, num, 1f)) * Matrix4x4.Translate(new Vector3(50f, 50f, 0f));
            foreach (KeyValuePair<GameDebugMenuUI.WindowID, GameDebugMenuUI.DebugWindow> keyValuePair in this._windowTable)
            {
                GameDebugMenuUI.WindowID windowID;
                GameDebugMenuUI.DebugWindow debugWindow;
                keyValuePair.Deconstruct(out windowID, out debugWindow);
                GameDebugMenuUI.WindowID windowID2 = windowID;
                GameDebugMenuUI.DebugWindow debugWindow2 = debugWindow;
                if (debugWindow2.IsActive)
                {
                    debugWindow2.Rect = GUI.Window((int)windowID2, debugWindow2.Rect, new GUI.WindowFunction(this.DoWindow), debugWindow2.Title);
                }
            }
            if (!GameDebugMenuUI.Notifications.Empty<GameDebugMenuUI.Notification>())
            {
                GUIStyle box = GUI.skin.box;
                box.alignment = TextAnchor.MiddleLeft;
                using (new GUILayout.AreaScope(new Rect((float)(num2 - 200), 0f, 200f, (float)num3)))
                {
                    using (new GUILayout.VerticalScope(Array.Empty<GUILayoutOption>()))
                    {
                        foreach (GameDebugMenuUI.Notification notification in GameDebugMenuUI.Notifications)
                        {
                            notification.Time -= Time.deltaTime;
                            GUILayout.Box(notification.Content, box, new GUILayoutOption[]
                            {
                                GUILayout.Width(200f),
                                GUILayout.Height(20f)
                            });
                        }
                    }
                    GameDebugMenuUI.Notifications.RemoveAll((GameDebugMenuUI.Notification n) => n.Time <= 0f);
                }
            }
        }

        public GameDebugMenuUI() : base()
        {
            SortedDictionary<GameDebugMenuUI.WindowID, GameDebugMenuUI.DebugWindow> sortedDictionary = new SortedDictionary<GameDebugMenuUI.WindowID, GameDebugMenuUI.DebugWindow>();
            sortedDictionary[GameDebugMenuUI.WindowID.MainMenu] = new GameDebugMenuUI.MainMenuDebugWindow();
            sortedDictionary[GameDebugMenuUI.WindowID.LoadGameRun] = new GameDebugMenuUI.LoadGameRunWindow();
            sortedDictionary[GameDebugMenuUI.WindowID.GameRunEntry] = new GameDebugMenuUI.GameRunDebugWindow();
            sortedDictionary[GameDebugMenuUI.WindowID.BattleEntry] = new GameDebugMenuUI.BattleDebugWindow();
            sortedDictionary[GameDebugMenuUI.WindowID.AddExhibit] = new GameDebugMenuUI.AddExhibitWindow();
            sortedDictionary[GameDebugMenuUI.WindowID.AddCard] = new GameDebugMenuUI.AddCardWindow();
            sortedDictionary[GameDebugMenuUI.WindowID.AddStatusEffect] = new GameDebugMenuUI.AddStatusEffectWindow();
            sortedDictionary[GameDebugMenuUI.WindowID.GunTest] = new GameDebugMenuUI.GunTestWindow();
            this._windowTable = sortedDictionary;
        }

        private static readonly List<GameDebugMenuUI.Notification> Notifications = new List<GameDebugMenuUI.Notification>();

        private readonly SortedDictionary<GameDebugMenuUI.WindowID, GameDebugMenuUI.DebugWindow> _windowTable;

        private class Notification
        {
            public string Content { get; set; }

            public float Time { get; set; }
        }

        private abstract class DebugWindow
        {
            public GameDebugMenuUI Parent
            {
                get
                {
                    GameDebugMenuUI gameDebugMenuUI;
                    if (!this._parent.TryGetTarget(out gameDebugMenuUI))
                    {
                        return null;
                    }
                    return gameDebugMenuUI;
                }
                set
                {
                    this._parent.SetTarget(value);
                }
            }

            public abstract string Title { get; }

            public virtual void OnAwake()
            {
            }

            public virtual void OnLocaleChanged()
            {
            }

            protected static GUIStyle LeftAlign
            {
                get
                {
                    return new GUIStyle(GUI.skin.button)
                    {
                        alignment = TextAnchor.MiddleLeft
                    };
                }
            }

            public virtual bool IsActive { get; set; }

            public Rect Rect { get; set; }

            public abstract Rect InitRect { get; }

            protected GUILayout.AreaScope ContentScope()
            {
                return new GUILayout.AreaScope(new Rect(10f, 30f, this.Rect.width - 20f, this.Rect.height - 40f));
            }

            public void DrawWindow()
            {
                if (GUI.Button(new Rect(this.Rect.width - 18f, 0f, 18f, 18f), "x"))
                {
                    this.IsActive = false;
                    return;
                }
                this.DrawContent();
            }

            protected abstract void DrawContent();

            protected static bool Filter(GameEntity entity, string filter)
            {
                filter = filter.Trim();
                return entity.Id.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 || entity.Name.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0;
            }

            protected static bool Filter(string source, string filter)
            {
                filter = filter.Trim();
                return source.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0;
            }

            private readonly WeakReference<GameDebugMenuUI> _parent = new WeakReference<GameDebugMenuUI>(null);

            protected const int ButtonHeight = 30;
        }

        private abstract class BattleDebugWindowBase : GameDebugMenuUI.DebugWindow
        {
            protected override void DrawContent()
            {
                GameRunController currentGameRun = Singleton<GameMaster>.Instance.CurrentGameRun;
                BattleController battleController = ((currentGameRun != null) ? currentGameRun.Battle : null);
                if (battleController == null)
                {
                    this.IsActive = false;
                    return;
                }
                using (base.ContentScope())
                {
                    this.DrawBattleContent(battleController);
                }
            }

            protected abstract void DrawBattleContent(BattleController battle);
        }

        private abstract class GameRunDebugWindowBase : GameDebugMenuUI.DebugWindow
        {
            protected override void DrawContent()
            {
                GameRunController currentGameRun = Singleton<GameMaster>.Instance.CurrentGameRun;
                BattleController battleController = ((currentGameRun != null) ? currentGameRun.Battle : null);
                if (currentGameRun == null || battleController != null)
                {
                    this.IsActive = false;
                    return;
                }
                using (base.ContentScope())
                {
                    this.DrawGameRunContent(currentGameRun);
                }
            }

            protected abstract void DrawGameRunContent(GameRunController gameRun);
        }

        private class BattleDebugWindow : GameDebugMenuUI.BattleDebugWindowBase
        {
            public override void OnAwake()
            {
                Dictionary<string, Sprite> dictionary = Resources.LoadAll<Sprite>("Sprite Assets/ManaSprite").ToDictionary((Sprite s) => s.name);
                foreach (char c in GameDebugMenuUI.BattleDebugWindow.ColorShortNames)
                {
                    Sprite sprite;
                    if (dictionary.TryGetValue(c.ToString(), out sprite))
                    {
                        this._colorTextures.Add(c, GameDebugMenuUI.BattleDebugWindow.g__GenerateTextureFromSprite(sprite));
                    }
                    else
                    {
                        Debug.LogError(string.Format("Cannot find mana sprite '{0}' for battle debug UI", c));
                    }
                }
            }

            public override string Title
            {
                get
                {
                    return "Battle Debug";
                }
            }

            public override Rect InitRect
            {
                get
                {
                    return new Rect(20f, 20f, 220f, 500f);
                }
            }

            protected override void DrawBattleContent(BattleController battle)
            {
                base.Parent._windowTable[GameDebugMenuUI.WindowID.AddCard].IsActive = GUILayout.Toggle(base.Parent._windowTable[GameDebugMenuUI.WindowID.AddCard].IsActive, "Add Card", GUI.skin.button, new GUILayoutOption[] { GUILayout.Height(30f) });
                base.Parent._windowTable[GameDebugMenuUI.WindowID.AddStatusEffect].IsActive = GUILayout.Toggle(base.Parent._windowTable[GameDebugMenuUI.WindowID.AddStatusEffect].IsActive, "Add Effect", GUI.skin.button, new GUILayoutOption[] { GUILayout.Height(30f) });
                GUILayout.Space(10f);
                using (new GUILayout.HorizontalScope(Array.Empty<GUILayoutOption>()))
                {
                    this._drawCount = GUILayout.TextField(this._drawCount, Array.Empty<GUILayoutOption>());
                    if (GUILayout.Button("Draw Card", Array.Empty<GUILayoutOption>()))
                    {
                        int num;
                        if (!int.TryParse(this._drawCount, out num))
                        {
                            Debug.LogError("Invalid draw count string: " + this._drawCount);
                        }
                        else if (num <= 0)
                        {
                            Debug.LogError("Invalid draw count string: " + this._drawCount);
                        }
                        else
                        {
                            battle.RequestDebugAction(new DrawManyCardAction(num), string.Format("Debug: Draw {0}", num));
                        }
                    }
                }
                GUILayout.Space(10f);
                GUIStyle guistyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter
                };
                using (new GUILayout.HorizontalScope(Array.Empty<GUILayoutOption>()))
                {
                    foreach (char c in GameDebugMenuUI.BattleDebugWindow.ColorShortNames)
                    {
                        GUILayout.Label(new GUIContent(this._colorTextures[c], c.ToString()), guistyle, new GUILayoutOption[]
                        {
                            GUILayout.Width(25f),
                            GUILayout.Height(20f)
                        });
                    }
                }
                using (new GUILayout.HorizontalScope(Array.Empty<GUILayoutOption>()))
                {
                    foreach (char c2 in GameDebugMenuUI.BattleDebugWindow.ColorShortNames)
                    {
                        if (GUILayout.Button(new GUIContent("1", "Gain 1 " + c2.ToString()), new GUILayoutOption[]
                        {
                            GUILayout.Width(25f),
                            GUILayout.Height(20f)
                        }))
                        {
                            ManaGroup manaGroup = ManaGroup.Single(ManaColors.FromShortName(c2).Value);
                            battle.RequestDebugAction(new GainManaAction(manaGroup), string.Format("Debug: Gain {0}", manaGroup));
                        }
                    }
                }
                using (new GUILayout.HorizontalScope(Array.Empty<GUILayoutOption>()))
                {
                    foreach (char c3 in GameDebugMenuUI.BattleDebugWindow.ColorShortNames)
                    {
                        if (GUILayout.Button(new GUIContent("10", "Gain 10 " + c3.ToString()), new GUILayoutOption[]
                        {
                            GUILayout.Width(25f),
                            GUILayout.Height(20f)
                        }))
                        {
                            ManaGroup manaGroup2 = ManaGroup.Single(ManaColors.FromShortName(c3).Value) * 10;
                            battle.RequestDebugAction(new GainManaAction(manaGroup2), string.Format("Debug: Gain {0}", manaGroup2));
                        }
                    }
                }
                using (new GUILayout.HorizontalScope(Array.Empty<GUILayoutOption>()))
                {
                    foreach (char c4 in GameDebugMenuUI.BattleDebugWindow.ColorShortNames)
                    {
                        if (GUILayout.Button(new GUIContent("-1", "Lose 1 " + c4.ToString()), new GUILayoutOption[]
                        {
                            GUILayout.Width(25f),
                            GUILayout.Height(20f)
                        }))
                        {
                            ManaGroup manaGroup3 = ManaGroup.Single(ManaColors.FromShortName(c4).Value);
                            battle.RequestDebugAction(new LoseManaAction(manaGroup3), string.Format("Debug: Lose {0}", manaGroup3));
                        }
                    }
                }
                GUILayout.Space(10f);
                using (new GUILayout.HorizontalScope(Array.Empty<GUILayoutOption>()))
                {
                    GUILayout.Label("To self: ", Array.Empty<GUILayoutOption>());
                    this._hp = GUILayout.TextField(this._hp, Array.Empty<GUILayoutOption>());
                    if (GUILayout.Button("Damage", Array.Empty<GUILayoutOption>()))
                    {
                        int num2;
                        if (!int.TryParse(this._hp, out num2))
                        {
                            Debug.LogError("Invalid hp string: " + this._hp);
                        }
                        else if (num2 <= 0)
                        {
                            Debug.LogError("Invalid hp string: " + this._hp);
                        }
                        else
                        {
                            battle.RequestDebugAction(new DamageAction(battle.Player, battle.Player, DamageInfo.HpLose((float)num2, false), "Instant", GunType.Single), string.Format("Debug: Damage Self {0}", num2));
                        }
                    }
                    if (GUILayout.Button("Heal", Array.Empty<GUILayoutOption>()))
                    {
                        int num3;
                        if (!int.TryParse(this._hp, out num3))
                        {
                            Debug.LogError("Invalid hp string: " + this._hp);
                        }
                        else if (num3 <= 0)
                        {
                            Debug.LogError("Invalid hp string: " + this._hp);
                        }
                        else
                        {
                            battle.RequestDebugAction(new HealAction(battle.Player, battle.Player, num3, HealType.Normal, 0.2f), string.Format("Debug: Heal Self {0}", num3));
                        }
                    }
                }
                UltimateSkill us = battle.Player.Us;
                if (us != null)
                {
                    GUILayout.Space(10f);
                    if (GUILayout.Button("Gain Power (1 bar)", new GUILayoutOption[] { GUILayout.Height(30f) }))
                    {
                        battle.RequestDebugAction(new GainPowerAction(us.PowerPerLevel), "Debug: Gain Power");
                    }
                }
                GUILayout.Space(5f);
                if (GUILayout.Button("Upgrade Hand", new GUILayoutOption[] { GUILayout.Height(30f) }))
                {
                    List<Card> list = battle.HandZone.Where((Card card) => card.CanUpgrade).ToList<Card>();
                    if (list.Count > 0)
                    {
                        bool flag = true;
                        string text = null;
                        foreach (Card card3 in list)
                        {
                            string text2;
                            if (flag)
                            {
                                text2 = "{" + card3.Name + "}";
                                flag = false;
                            }
                            else
                            {
                                text2 = ", {" + card3.Name + "}";
                            }
                            text += text2;
                        }
                        string text3 = "Debug: Upgrade cards: " + text;
                        battle.RequestDebugAction(new UpgradeCardsAction(list), text3);
                    }
                    else
                    {
                        Debug.Log("No cards in hand can be upgraded.");
                    }
                }
                GUILayout.Space(5f);
                if (GUILayout.Button("Exile Hand", new GUILayoutOption[] { GUILayout.Height(30f) }))
                {
                    List<Card> list2 = battle.HandZone.ToList<Card>();
                    if (list2.Count > 0)
                    {
                        bool flag2 = true;
                        string text4 = null;
                        foreach (Card card2 in list2)
                        {
                            string text5;
                            if (flag2)
                            {
                                text5 = "{" + card2.Name + "}";
                                flag2 = false;
                            }
                            else
                            {
                                text5 = ", {" + card2.Name + "}";
                            }
                            text4 += text5;
                        }
                        string text6 = "Debug: Exile cards: " + text4;
                        battle.RequestDebugAction(new ExileManyCardAction(list2), text6);
                    }
                    else
                    {
                        Debug.Log("No cards in hand can be upgraded.");
                    }
                }
                GUILayout.Space(5f);
                base.Parent._windowTable[GameDebugMenuUI.WindowID.GunTest].IsActive = GUILayout.Toggle(base.Parent._windowTable[GameDebugMenuUI.WindowID.GunTest].IsActive, "Gun Test", GUI.skin.button, new GUILayoutOption[] { GUILayout.Height(30f) });
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("i r winner", new GUILayoutOption[] { GUILayout.Height(30f) }))
                {
                    battle.RequestDebugAction(new InstantWinAction(), "Debug: Instant Win");
                    this.IsActive = false;
                }
            }

            internal static Texture2D g__GenerateTextureFromSprite(Sprite sprite)
            {
                Rect rect = sprite.rect;
                Texture2D texture2D = new Texture2D((int)rect.width, (int)rect.height);
                Color[] pixels = sprite.texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
                texture2D.SetPixels(pixels);
                texture2D.Apply(true);
                return texture2D;
            }

            private static readonly char[] ColorShortNames = ManaColors.Colors.Select((ManaColor c) => c.ToShortName()).ToArray<char>();

            private readonly SortedDictionary<char, Texture> _colorTextures = new SortedDictionary<char, Texture>();

            private string _drawCount = "1";

            private string _hp = "10";
        }

        private class MainMenuDebugWindow : GameDebugMenuUI.DebugWindow
        {
            public override string Title
            {
                get
                {
                    return "Main Menu Debug";
                }
            }

            public override Rect InitRect
            {
                get
                {
                    return new Rect(800f, -20f, 200f, 500f);
                }
            }

            protected override void DrawContent()
            {
                if (Singleton<GameMaster>.Instance.CurrentGameRun != null)
                {
                    this.IsActive = false;
                    return;
                }
                using (base.ContentScope())
                {
                    if (GUILayout.Button("Quick Battle Normal", new GUILayoutOption[] { GUILayout.Height(30f) }))
                    {
                        PlayerUnit playerUnit = Library.GetSelectablePlayers().First<PlayerUnit>();
                        Exhibit exhibit = Library.CreateExhibit(playerUnit.Config.ExhibitA);
                        int num = 1000;
                        UltimateSkill ultimateSkill = Library.CreateUs(playerUnit.Config.UltimateSkillA);
                        playerUnit.SetUs(ultimateSkill);
                        Card[] array = playerUnit.Config.DeckA.Select(delegate (string c)
                        {
                            if (!c.EndsWith("+"))
                            {
                                return Library.CreateCard(c, false, null);
                            }
                            return Library.CreateCard(c.Substring(0, c.Length - 1), true, null);
                        }).ToArray<Card>();
                        Stage[] array2 = new Stage[] { Library.CreateStage<BattleAdvTest>().AsNormalFinal() };
                        GameMaster.StartGame(GameDifficulty.Normal, PuzzleFlag.None, playerUnit, PlayerType.TypeA, exhibit, new int?(num), array, array2, null, Enumerable.Empty<JadeBox>(), GameMode.FreeMode, true);
                    }
                    if (GUILayout.Button("Quick Battle Lunatic", new GUILayoutOption[] { GUILayout.Height(30f) }))
                    {
                        PlayerUnit playerUnit2 = Library.GetSelectablePlayers().First<PlayerUnit>();
                        Exhibit exhibit2 = Library.CreateExhibit(playerUnit2.Config.ExhibitA);
                        int num2 = 1000;
                        UltimateSkill ultimateSkill2 = Library.CreateUs(playerUnit2.Config.UltimateSkillA);
                        playerUnit2.SetUs(ultimateSkill2);
                        Card[] array3 = playerUnit2.Config.DeckA.Select(delegate (string c)
                        {
                            if (!c.EndsWith("+"))
                            {
                                return Library.CreateCard(c, false, null);
                            }
                            return Library.CreateCard(c.Substring(0, c.Length - 1), true, null);
                        }).ToArray<Card>();
                        Stage[] array4 = new Stage[] { Library.CreateStage<BattleAdvTest>().AsNormalFinal() };
                        GameMaster.StartGame(GameDifficulty.Lunatic, PuzzleFlag.None, playerUnit2, PlayerType.TypeA, exhibit2, new int?(num2), array3, array4, null, Enumerable.Empty<JadeBox>(), GameMode.FreeMode, true);
                    }
                    if (GUILayout.Button("Quick AllStations", new GUILayoutOption[] { GUILayout.Height(30f) }))
                    {
                        PlayerUnit playerUnit3 = Library.GetSelectablePlayers().First<PlayerUnit>();
                        Exhibit exhibit4 = Library.CreateExhibit(playerUnit3.Config.ExhibitA);
                        int num4 = 1000;
                        UltimateSkill ultimateSkill4 = Library.CreateUs(playerUnit3.Config.UltimateSkillA);
                        playerUnit3.SetUs(ultimateSkill4);
                        Card[] array7 = playerUnit3.Config.DeckA.Select(delegate (string c)
                        {
                            if (!c.EndsWith("+"))
                            {
                                return Library.CreateCard(c, false, null);
                            }
                            return Library.CreateCard(c.Substring(0, c.Length - 1), true, null);
                        }).ToArray<Card>();
                        Stage[] array8 = new Stage[] { Library.CreateStage<AllStations>().AsNormalFinal() };
                        GameMaster.StartGame(GameDifficulty.Normal, PuzzleFlag.None, playerUnit3, PlayerType.TypeA, exhibit4, new int?(num4), array7, array8, null, Enumerable.Empty<JadeBox>(), GameMode.FreeMode, true);
                    }
                    if (GUILayout.Button("Quick SingleRoutes", new GUILayoutOption[] { GUILayout.Height(30f) }))
                    {
                        PlayerUnit playerUnit4 = Library.GetSelectablePlayers().First<PlayerUnit>();
                        Exhibit exhibit5 = Library.CreateExhibit(playerUnit4.Config.ExhibitA);
                        int num5 = 1000;
                        UltimateSkill ultimateSkill5 = Library.CreateUs(playerUnit4.Config.UltimateSkillA);
                        playerUnit4.SetUs(ultimateSkill5);
                        Card[] array9 = playerUnit4.Config.DeckA.Select(delegate (string c)
                        {
                            if (!c.EndsWith("+"))
                            {
                                return Library.CreateCard(c, false, null);
                            }
                            return Library.CreateCard(c.Substring(0, c.Length - 1), true, null);
                        }).ToArray<Card>();
                        Stage[] array10 = new Stage[]
                        {
                            Library.CreateStage<SingleRoute>(),
                            Library.CreateStage<SingleRouteStage2>(),
                            Library.CreateStage<SingleRouteStage3>().AsNormalFinal(),
                            Library.CreateStage<FinalStage>().AsTrueEndFinal()
                        };
                        GameMaster.StartGame(GameDifficulty.Normal, PuzzleFlag.None, playerUnit4, PlayerType.TypeA, exhibit5, new int?(num5), array9, array10, null, Enumerable.Empty<JadeBox>(), GameMode.FreeMode, true);
                    }
                    GUILayout.Space(30f);
                    GUILayout.Label("Debug Routes", Array.Empty<GUILayoutOption>());
                    foreach (ValueTuple<string, StartGameData> valueTuple in GameDebugMenuUI.MainMenuDebugWindow.DebugRoutes)
                    {
                        string item = valueTuple.Item1;
                        StartGameData item2 = valueTuple.Item2;
                        if (GUILayout.Button(item, new GUILayoutOption[] { GUILayout.Height(30f) }))
                        {
                            UiManager.GetPanel<StartGamePanel>().Show(item2);
                            this.IsActive = false;
                        }
                    }
                    GUILayout.Space(30f);
                    if (GUILayout.Button("Load GameRun", new GUILayoutOption[] { GUILayout.Height(30f) }))
                    {
                        base.Parent._windowTable[GameDebugMenuUI.WindowID.LoadGameRun].IsActive = true;
                    }
                    if (GUILayout.Button("Quick Abandon GameRun", new GUILayoutOption[] { GUILayout.Height(30f) }) && Singleton<GameMaster>.Instance.GameRunSaveData != null)
                    {
                        GameMaster.RequestAbandonGameRun(true);
                    }
                }
            }

            //[TupleElementNames(new string[] { "name", "data" })]
            private static readonly List<ValueTuple<string, StartGameData>> DebugRoutes = new List<ValueTuple<string, StartGameData>>
            {
                new ValueTuple<string, StartGameData>("SingleRoutes", new StartGameData
                {
                    StagesCreateFunc = () => new Stage[]
                    {
                        Library.CreateStage<SingleRoute>(),
                        Library.CreateStage<SingleRouteStage2>(),
                        Library.CreateStage<SingleRouteStage3>().AsNormalFinal(),
                        Library.CreateStage<FinalStage>().AsTrueEndFinal()
                    }
                }),
                new ValueTuple<string, StartGameData>("BattleAdvTest", new StartGameData
                {
                    StagesCreateFunc = () => new Stage[] { Library.CreateStage<BattleAdvTest>().AsNormalFinal() }
                }),
                new ValueTuple<string, StartGameData>("AllStations", new StartGameData
                {
                    StagesCreateFunc = () => new Stage[] { Library.CreateStage<AllStations>().AsNormalFinal() }
                }),
                new ValueTuple<string, StartGameData>("BossOnly", new StartGameData
                {
                    StagesCreateFunc = () => new Stage[]
                    {
                        Library.CreateStage<BossOnlyStage>().WithLevel(1),
                        Library.CreateStage<BossOnlyStage>().WithLevel(2),
                        Library.CreateStage<BossOnlyStage>().WithLevel(3),
                        Library.CreateStage<BossOnlyStage>().WithLevel(4).AsNormalFinal(),
                        Library.CreateStage<FinalStage>().AsTrueEndFinal()
                    },
                    DebutAdventure = typeof(Debut)
                })
            };
        }

        private class LoadGameRunWindow : GameDebugMenuUI.DebugWindow
        {
            public override string Title
            {
                get
                {
                    return "Load GameRun";
                }
            }

            public override Rect InitRect
            {
                get
                {
                    return new Rect(300f, 20f, 400f, 400f);
                }
            }

            public override bool IsActive
            {
                get
                {
                    return this._isActive;
                }
                set
                {
                    this._isActive = value;
                    if (value)
                    {
                        if (!Directory.Exists("SaveData/Debug"))
                        {
                            return;
                        }
                        using (IEnumerator<string> enumerator = Directory.EnumerateFiles("SaveData/Debug").GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                string text = enumerator.Current;
                                this._list.Add(Path.GetFileName(text));
                            }
                            return;
                        }
                    }
                    this._list.Clear();
                }
            }

            protected override void DrawContent()
            {
                if (Singleton<GameMaster>.Instance.CurrentGameRun != null)
                {
                    this.IsActive = false;
                    return;
                }
                using (base.ContentScope())
                {
                    using (GUILayout.ScrollViewScope scrollViewScope = new GUILayout.ScrollViewScope(this._scrollPosition, Array.Empty<GUILayoutOption>()))
                    {
                        this._scrollPosition = scrollViewScope.scrollPosition;
                        foreach (string text in this._list)
                        {
                            if (GUILayout.Button(new GUIContent(text), GameDebugMenuUI.DebugWindow.LeftAlign, new GUILayoutOption[] { GUILayout.Height(30f) }))
                            {
                                GameMaster.RestoreGameRun(SaveDataHelper.DeserializeGameRun(File.ReadAllBytes(Path.Combine("SaveData/Debug", text))));
                            }
                        }
                    }
                }
            }

            private Vector2 _scrollPosition;

            private bool _isActive;

            private readonly List<string> _list = new List<string>();

            private const string Dir = "SaveData/Debug";
        }

        private class GameRunDebugWindow : GameDebugMenuUI.GameRunDebugWindowBase
        {
            public override string Title
            {
                get
                {
                    return "Game-run Debug";
                }
            }

            public override Rect InitRect
            {
                get
                {
                    return new Rect(20f, 20f, 200f, 200f);
                }
            }

            protected override void DrawGameRunContent(GameRunController gameRun)
            {
                if (GUILayout.Button("Add Exhibit", new GUILayoutOption[] { GUILayout.Height(30f) }))
                {
                    GameDebugMenuUI.DebugWindow debugWindow = base.Parent._windowTable[GameDebugMenuUI.WindowID.AddExhibit];
                    debugWindow.IsActive = !debugWindow.IsActive;
                }
                if (GUILayout.Button("Add Deck Card", new GUILayoutOption[] { GUILayout.Height(30f) }))
                {
                    GameDebugMenuUI.DebugWindow debugWindow2 = base.Parent._windowTable[GameDebugMenuUI.WindowID.AddCard];
                    debugWindow2.IsActive = !debugWindow2.IsActive;
                }
                if (GUILayout.Button("Clear Deck Cards", new GUILayoutOption[] { GUILayout.Height(30f) }))
                {
                    gameRun.RemoveDeckCards(gameRun.BaseDeck, false);
                }
                if (GUILayout.Button("SE Icon Check", new GUILayoutOption[] { GUILayout.Height(30f) }))
                {
                    List<StatusEffectConfig> list = new List<StatusEffectConfig>();
                    foreach (StatusEffectConfig statusEffectConfig in StatusEffectConfig.AllConfig())
                    {
                        if (ResourcesHelper.TryGetSprite<StatusEffect>(statusEffectConfig.Id) == null)
                        {
                            list.Add(statusEffectConfig);
                        }
                    }
                    list = list.OrderBy((StatusEffectConfig config) => config.Index).ToList<StatusEffectConfig>();
                    foreach (StatusEffectConfig statusEffectConfig2 in list)
                    {
                        Debug.Log(string.Format("{0}: {1}", statusEffectConfig2.Index, statusEffectConfig2.Id));
                    }
                }
            }
        }

        private class AddExhibitWindow : GameDebugMenuUI.GameRunDebugWindowBase
        {
            public override string Title
            {
                get
                {
                    return "Add Exhibit";
                }
            }

            public override Rect InitRect
            {
                get
                {
                    return new Rect(300f, 20f, 400f, 400f);
                }
            }

            private List<GameDebugMenuUI.AddExhibitWindow.ExhibitInfo> GetExhibitList()
            {
                if (this._exhibitInfos != null)
                {
                    return this._exhibitInfos;
                }
                try
                {
                    this._exhibitInfos = (from kv in Library.EnumerateExhibitTypes()
                                          let exhibit = Library.CreateExhibit(kv.Item1)
                                          orderby exhibit.Config.Order, exhibit.Name
                                          select new GameDebugMenuUI.AddExhibitWindow.ExhibitInfo(exhibit)).ToList<GameDebugMenuUI.AddExhibitWindow.ExhibitInfo>();
                }
                catch (Exception ex)
                {
                    this._exhibitInfos = new List<GameDebugMenuUI.AddExhibitWindow.ExhibitInfo>();
                    Debug.LogError(ex);
                }
                return this._exhibitInfos;
            }

            public override void OnLocaleChanged()
            {
                this._exhibitInfos = null;
            }

            protected override void DrawGameRunContent(GameRunController gameRun)
            {
                using (new GUILayout.HorizontalScope(Array.Empty<GUILayoutOption>()))
                {
                    GUILayout.Label("Filter: ", new GUILayoutOption[] { GUILayout.Width(50f) });
                    this._filter = GUILayout.TextField(this._filter, Array.Empty<GUILayoutOption>());
                    if (GUILayout.Button("x", new GUILayoutOption[] { GUILayout.Width(25f) }))
                    {
                        this._filter = null;
                    }
                }
                using (GUILayout.ScrollViewScope scrollViewScope = new GUILayout.ScrollViewScope(this._scrollPosition, Array.Empty<GUILayoutOption>()))
                {
                    this._scrollPosition = scrollViewScope.scrollPosition;
                    foreach (GameDebugMenuUI.AddExhibitWindow.ExhibitInfo exhibitInfo in this.GetExhibitList())
                    {
                        if (!gameRun.Player.HasExhibit(exhibitInfo.Exhibit.GetType()) && (string.IsNullOrWhiteSpace(this._filter) || GameDebugMenuUI.DebugWindow.Filter(exhibitInfo.Exhibit, this._filter)) && GUILayout.Button(new GUIContent(exhibitInfo.Id + " (" + exhibitInfo.Name + ")", exhibitInfo.Description), GameDebugMenuUI.DebugWindow.LeftAlign, new GUILayoutOption[] { GUILayout.Height(30f) }))
                        {
                            GameMaster.DebugGainExhibit(Library.CreateExhibit(exhibitInfo.Exhibit.GetType()));
                        }
                    }
                }
            }

            private List<GameDebugMenuUI.AddExhibitWindow.ExhibitInfo> _exhibitInfos;

            private string _filter;

            private Vector2 _scrollPosition;

            private class ExhibitInfo
            {
                public ExhibitInfo(Exhibit exhibit)
                {
                    this.Exhibit = exhibit;
                    this.Id = exhibit.Id;
                    this.Name = exhibit.Name;
                    this.Description = exhibit.Description;
                }

                public readonly Exhibit Exhibit;

                public readonly string Id;

                public readonly string Name;

                public readonly string Description;
            }
        }

        private class AddCardWindow : GameDebugMenuUI.DebugWindow
        {
            public override string Title
            {
                get
                {
                    return "Add Card";
                }
            }

            public override Rect InitRect
            {
                get
                {
                    return new Rect(300f, 20f, 400f, 400f);
                }
            }

            private List<GameDebugMenuUI.AddCardWindow.CardInfo> GetCardList()
            {
                if (this._cardInfos != null)
                {
                    return this._cardInfos;
                }
                try
                {
                    this._cardInfos = (from kv in Library.EnumerateCardTypes()
                                       let card = Library.CreateCard(kv.Item1)
                                       orderby card.Config.Order, card.Id
                                       select new GameDebugMenuUI.AddCardWindow.CardInfo(card)).ToList<GameDebugMenuUI.AddCardWindow.CardInfo>();
                }
                catch (Exception ex)
                {
                    this._cardInfos = new List<GameDebugMenuUI.AddCardWindow.CardInfo>();
                    Debug.LogError(ex);
                }
                return this._cardInfos;
            }

            public override void OnLocaleChanged()
            {
                this._cardInfos = null;
            }

            protected override void DrawContent()
            {
                GameRunController currentGameRun = Singleton<GameMaster>.Instance.CurrentGameRun;
                if (currentGameRun == null)
                {
                    this.IsActive = false;
                    return;
                }
                BattleController battle = currentGameRun.Battle;
                using (base.ContentScope())
                {
                    using (new GUILayout.HorizontalScope(Array.Empty<GUILayoutOption>()))
                    {
                        this._upgraded = GUILayout.Toggle(this._upgraded, "Upgraded", Array.Empty<GUILayoutOption>());
                        this._targetId = GUILayout.SelectionGrid(this._targetId, this._targets, this._targets.Length, new GUILayoutOption[] { GUILayout.Width(320f) });
                    }
                    using (new GUILayout.HorizontalScope(Array.Empty<GUILayoutOption>()))
                    {
                        GUILayout.Label("Filter: ", new GUILayoutOption[] { GUILayout.Width(50f) });
                        this._filter = GUILayout.TextField(this._filter, Array.Empty<GUILayoutOption>());
                        if (GUILayout.Button("x", new GUILayoutOption[] { GUILayout.Width(25f) }))
                        {
                            this._filter = null;
                        }
                    }
                    using (GUILayout.ScrollViewScope scrollViewScope = new GUILayout.ScrollViewScope(this._scrollPosition, Array.Empty<GUILayoutOption>()))
                    {
                        this._scrollPosition = scrollViewScope.scrollPosition;
                        foreach (GameDebugMenuUI.AddCardWindow.CardInfo cardInfo in this.GetCardList())
                        {
                            if ((string.IsNullOrWhiteSpace(this._filter) || GameDebugMenuUI.DebugWindow.Filter(cardInfo.Card, this._filter)) && GUILayout.Button(new GUIContent(cardInfo.Id + " (" + cardInfo.Name + ")", cardInfo.Description), GameDebugMenuUI.DebugWindow.LeftAlign, new GUILayoutOption[] { GUILayout.Height(30f) }))
                            {
                                Card card = cardInfo.Card.Clone();
                                if (this._upgraded)
                                {
                                    card.Upgrade();
                                }
                                if (battle == null)
                                {
                                    currentGameRun.AddDeckCards(new Card[] { card }, false, null);
                                }
                                else if (this._targetId == 0)
                                {
                                    battle.RequestDebugAction(new AddCardsToHandAction(new Card[] { card }), "Debug: Add Card to Hand" + card.Name);
                                }
                                else if (this._targetId == 1)
                                {
                                    battle.RequestDebugAction(new AddCardsToDiscardAction(new Card[] { card }), "Debug: Add Card to Discard " + card.Name);
                                }
                                else if (this._targetId == 2)
                                {
                                    battle.RequestDebugAction(new AddCardsToExileAction(new Card[] { card }), "Debug: Add Card to Discard " + card.Name);
                                }
                                else if (this._targetId == 3)
                                {
                                    battle.RequestDebugAction(new AddCardsToDrawZoneAction(new Card[] { card }, DrawZoneTarget.Top, AddCardsType.Normal), "Debug: Add Card to Discard " + card.Name);
                                }
                            }
                        }
                    }
                }
            }

            private readonly string[] _targets = new string[] { "Hand", "Discard", "Exile", "DrawTop" };

            private List<GameDebugMenuUI.AddCardWindow.CardInfo> _cardInfos;

            private bool _upgraded;

            private int _targetId;

            private string _filter;

            private Vector2 _scrollPosition;

            private class CardInfo
            {
                public CardInfo(Card card)
                {
                    this.Card = card;
                    this.Id = card.Id;
                    this.Name = card.Name;
                    string text = "[" + (card.IsXCost ? "X" : card.Cost.ToString()) + "]" + card.Description;
                    if (card.Keywords != Keyword.None)
                    {
                        this.Description = text + "\n<" + string.Join(" ", from k in card.EnumerateCardKeywords()
                                                                           select Keywords.GetDisplayWord(k).Name) + ">";
                        return;
                    }
                    this.Description = text;
                }

                public readonly Card Card;

                public readonly string Id;

                public readonly string Name;

                public readonly string Description;
            }
        }

        private class AddStatusEffectWindow : GameDebugMenuUI.BattleDebugWindowBase
        {
            public override string Title
            {
                get
                {
                    return "Add StatusEffect";
                }
            }

            public override Rect InitRect
            {
                get
                {
                    return new Rect(300f, 20f, 400f, 400f);
                }
            }

            private List<StatusEffect> GetStatusEffectList()
            {
                if (this._statusEffects != null)
                {
                    return this._statusEffects;
                }
                try
                {
                    this._statusEffects = (from kv in Library.EnumerateStatusEffectTypes()
                                           select Library.CreateStatusEffect(kv.Item1) into s
                                           orderby s.Config.Order, s.Id
                                           select s).ToList<StatusEffect>();
                }
                catch (Exception ex)
                {
                    this._statusEffects = new List<StatusEffect>();
                    Debug.LogError(ex);
                }
                return this._statusEffects;
            }

            public override void OnLocaleChanged()
            {
                this._statusEffects = null;
            }

            protected override void DrawBattleContent(BattleController battle)
            {
                using (new GUILayout.HorizontalScope(Array.Empty<GUILayoutOption>()))
                {
                    GUILayout.Label("Target: ", Array.Empty<GUILayoutOption>());
                    this._targetId = GUILayout.SelectionGrid(this._targetId, this._targets, this._targets.Length, new GUILayoutOption[] { GUILayout.Width(320f) });
                }
                using (new GUILayout.HorizontalScope(Array.Empty<GUILayoutOption>()))
                {
                    GUILayout.Label("Level: ", new GUILayoutOption[] { GUILayout.Width(50f) });
                    this._level = GUILayout.TextField(this._level, Array.Empty<GUILayoutOption>());
                    GUILayout.Space(20f);
                    GUILayout.Label("Duration: ", new GUILayoutOption[] { GUILayout.Width(60f) });
                    this._duration = GUILayout.TextField(this._duration, Array.Empty<GUILayoutOption>());
                }
                using (new GUILayout.HorizontalScope(Array.Empty<GUILayoutOption>()))
                {
                    GUILayout.Label("Filter: ", new GUILayoutOption[] { GUILayout.Width(50f) });
                    this._filter = GUILayout.TextField(this._filter, Array.Empty<GUILayoutOption>());
                    if (GUILayout.Button("x", new GUILayoutOption[] { GUILayout.Width(25f) }))
                    {
                        this._filter = null;
                    }
                }
                using (GUILayout.ScrollViewScope scrollViewScope = new GUILayout.ScrollViewScope(this._scrollPosition, Array.Empty<GUILayoutOption>()))
                {
                    this._scrollPosition = scrollViewScope.scrollPosition;
                    foreach (StatusEffect statusEffect in this.GetStatusEffectList())
                    {
                        if ((string.IsNullOrWhiteSpace(this._filter) || GameDebugMenuUI.DebugWindow.Filter(statusEffect, this._filter)) && GUILayout.Button(new GUIContent(statusEffect.Id + " (" + statusEffect.Name + ")", statusEffect.Brief ?? "No Brief"), GameDebugMenuUI.DebugWindow.LeftAlign, new GUILayoutOption[] { GUILayout.Height(30f) }))
                        {
                            int num;
                            int num2;
                            if (!int.TryParse(this._level, out num))
                            {
                                Debug.LogError("Invalid level string");
                            }
                            else if (!int.TryParse(this._duration, out num2))
                            {
                                Debug.LogError("Invalid duration string");
                            }
                            else if (this._targetId == 0)
                            {
                                battle.RequestDebugAction(new ApplyStatusEffectAction(statusEffect.GetType(), battle.Player, new int?(num), new int?(num2), null, null, 0f, true), "Debug: Apply " + statusEffect.Name);
                            }
                            else
                            {
                                if (this._targetId == this._targets.Length - 1)
                                {
                                    using (IEnumerator<EnemyUnit> enumerator2 = battle.EnemyGroup.Alives.GetEnumerator())
                                    {
                                        while (enumerator2.MoveNext())
                                        {
                                            EnemyUnit enemyUnit = enumerator2.Current;
                                            battle.RequestDebugAction(new ApplyStatusEffectAction(statusEffect.GetType(), enemyUnit, new int?(num), new int?(num2), null, null, 0f, true), "Debug: Apply " + statusEffect.Name);
                                        }
                                        continue;
                                    }
                                }
                                EnemyUnit enemyUnit2 = battle.EnemyGroup.Alives.OrderBy((EnemyUnit e) => new ValueTuple<int, int>(e.MovePriority, e.RootIndex)).ElementAt(this._targetId - 1);
                                battle.RequestDebugAction(new ApplyStatusEffectAction(statusEffect.GetType(), enemyUnit2, new int?(num), new int?(num2), null, null, 0f, true), "Debug: Apply " + statusEffect.Name);
                            }
                        }
                    }
                }
            }

            private List<StatusEffect> _statusEffects;

            private readonly string[] _targets = new string[] { "Player", "Enemy0", "Enemy1", "Enemy2", "AllEnemies" };

            private int _targetId;

            private string _level = "1";

            private string _duration = "1";

            private string _filter;

            private Vector2 _scrollPosition;
        }

        private class GunTestWindow : GameDebugMenuUI.BattleDebugWindowBase
        {
            public override string Title
            {
                get
                {
                    return "Gun Test";
                }
            }

            public override Rect InitRect
            {
                get
                {
                    return new Rect(300f, 20f, 400f, 400f);
                }
            }

            public override bool IsActive
            {
                get
                {
                    return this._isActive;
                }
                set
                {
                    if (this._isActive != value)
                    {
                        this._isActive = value;
                        List<string> list;
                        if (!value)
                        {
                            list = null;
                        }
                        else
                        {
                            list = (from config in GunConfig.AllConfig()
                                    select config.Name).ToList<string>();
                        }
                        this._allGunName = list;
                    }
                }
            }

            private static void RequestDebugAction(BattleController battle, BattleAction action, string recordName, float delay = 0f)
            {
                if (delay > 0f)
                {
                    battle.RequestDebugAction(new WaitForYieldInstructionAction(new WaitForSeconds(delay)), recordName + " (Delay)");
                }
                battle.RequestDebugAction(action, recordName);
            }

            protected override void DrawBattleContent(BattleController battle)
            {
                using (new GUILayout.HorizontalScope(Array.Empty<GUILayoutOption>()))
                {
                    GUILayout.Label("Source: ", new GUILayoutOption[] { GUILayout.Width(50f) });
                    this._sourceId = GUILayout.SelectionGrid(this._sourceId, this._sources, this._sources.Length, new GUILayoutOption[] { GUILayout.Width(320f) });
                }
                using (new GUILayout.HorizontalScope(Array.Empty<GUILayoutOption>()))
                {
                    GUILayout.Label("Targets: ", new GUILayoutOption[] { GUILayout.Width(50f) });
                    this._targetId = GUILayout.SelectionGrid(this._targetId, this._targets, this._targets.Length, new GUILayoutOption[] { GUILayout.Width(320f) });
                }
                using (new GUILayout.HorizontalScope(Array.Empty<GUILayoutOption>()))
                {
                    GUILayout.Label("Damage: ", new GUILayoutOption[] { GUILayout.Width(60f) });
                    this._damage = GUILayout.TextField(this._damage, new GUILayoutOption[] { GUILayout.Width(50f) });
                    GameDebugMenuUI.NotifyGunTime = GUILayout.Toggle(GameDebugMenuUI.NotifyGunTime, "Nofity Time", Array.Empty<GUILayoutOption>());
                    GUILayout.Label(string.Format("Delay: {0: 0.##}", this._delay), new GUILayoutOption[] { GUILayout.Width(80f) });
                    this._delay = GUILayout.HorizontalSlider(this._delay, 0f, 10f, Array.Empty<GUILayoutOption>());
                }
                using (new GUILayout.HorizontalScope(Array.Empty<GUILayoutOption>()))
                {
                    GUILayout.Label("Filter: ", new GUILayoutOption[] { GUILayout.Width(60f) });
                    this._filter = GUILayout.TextField(this._filter, Array.Empty<GUILayoutOption>());
                    if (GUILayout.Button("x", new GUILayoutOption[] { GUILayout.Width(25f) }))
                    {
                        this._filter = null;
                    }
                }
                using (GUILayout.ScrollViewScope scrollViewScope = new GUILayout.ScrollViewScope(this._scrollPosition, Array.Empty<GUILayoutOption>()))
                {
                    this._scrollPosition = scrollViewScope.scrollPosition;
                    foreach (string text in this._allGunName)
                    {
                        if ((string.IsNullOrWhiteSpace(this._filter) || GameDebugMenuUI.DebugWindow.Filter(text, this._filter)) && GUILayout.Button(GunConfig.FromName(text).Id.ToString() + " " + text, GameDebugMenuUI.DebugWindow.LeftAlign, new GUILayoutOption[] { GUILayout.Height(30f) }))
                        {
                            int num;
                            if (!int.TryParse(this._damage, out num))
                            {
                                Debug.LogError("Invalid damage string");
                            }
                            else if (this._sourceId == 0 && this._targetId == 0)
                            {
                                Debug.LogError("Invalid source/target: player to player");
                            }
                            else if (this._sourceId != 0 && this._targetId != 0)
                            {
                                Debug.LogError("Invalid source/target: enemy to enemy");
                            }
                            else if (this._sourceId == 0)
                            {
                                if (this._targetId == this._targets.Length - 1)
                                {
                                    GameDebugMenuUI.GunTestWindow.RequestDebugAction(battle, new DamageAction(battle.Player, battle.EnemyGroup.Alives, DamageInfo.Attack((float)num, false), text, GunType.Single).SetCause(ActionCause.Card), "Debug: Gun Test " + text, this._delay);
                                }
                                else
                                {
                                    EnemyUnit enemyUnit = battle.EnemyGroup.ElementAt(this._targetId - 1);
                                    GameDebugMenuUI.GunTestWindow.RequestDebugAction(battle, new DamageAction(battle.Player, enemyUnit, DamageInfo.Attack((float)num, false), text, GunType.Single).SetCause(ActionCause.Card), "Debug: Gun Test " + text, this._delay);
                                }
                            }
                            else
                            {
                                EnemyUnit enemyUnit2 = battle.EnemyGroup.ElementAt(this._sourceId - 1);
                                GameDebugMenuUI.GunTestWindow.RequestDebugAction(battle, new DamageAction(enemyUnit2, battle.Player, DamageInfo.Attack((float)num, false), text, GunType.Single).SetSource(enemyUnit2).SetCause(ActionCause.EnemyAction), "Debug: Gun Test " + text, this._delay);
                            }
                        }
                    }
                }
            }

            private List<string> _allGunName;

            private readonly string[] _sources = new string[] { "Player", "Enemy0", "Enemy1", "Enemy2" };

            private int _sourceId;

            private readonly string[] _targets = new string[] { "Player", "Enemy0", "Enemy1", "Enemy2", "All" };

            private int _targetId = 1;

            private string _damage = "0";

            private float _delay;

            private string _filter;

            private Vector2 _scrollPosition;

            private bool _isActive;
        }

        private enum WindowID
        {
            MainMenu,
            LoadGameRun,
            GameRunEntry,
            BattleEntry,
            AddExhibit,
            AddCard,
            AddStatusEffect,
            GunTest
        }
    }
}
