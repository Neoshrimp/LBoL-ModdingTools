using JetBrains.Annotations;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Dialogs;
using LBoL.Core.Helpers;
using LBoL.Core.Stations;
using LBoL.Core.Units;
using LBoL.Core;
using LBoL.Presentation.I10N;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using LBoL.Presentation.UI;
using LBoL.Presentation.Units;
using LBoL.Presentation;
using System.Text;
using System;
using UnityEngine;
using static DebugMode.Plugin;

namespace DebugMode.AlphaDebug
{
    public static class DebugCommands
    {
        private static GameRunController GameRun
        {
            get
            {
                GameRunController currentGameRun = Singleton<GameMaster>.Instance.CurrentGameRun;
                if (currentGameRun == null)
                {
                    throw new InvalidOperationException("Not in game-run");
                }
                return currentGameRun;
            }
        }

        private static BattleController Battle
        {
            get
            {
                BattleController battle = DebugCommands.GameRun.Battle;
                if (battle == null)
                {
                    throw new InvalidOperationException("Not in battle");
                }
                return battle;
            }
        }

        private static UnitView PlayerView
        {
            get
            {
                return GameDirector.GetUnit(DebugCommands.Battle.Player);
            }
        }

        private static UnitView Enemy0View
        {
            get
            {
                return GameDirector.GetEnemy(0);
            }
        }

        [RuntimeCommand("help", "List all commands available.")]
        [UsedImplicitly]
        public static void Help()
        {
            GameConsoleUI.ListAll();
        }

        [RuntimeCommand("clear", "Clear console logs.")]
        [UsedImplicitly]
        public static void Clear()
        {
            LogCache.Clear();
        }

        [RuntimeCommand("test", "Custom test.")]
        [UsedImplicitly]
        public static void Test(float? time = null)
        {
        }

        [RuntimeCommand("locale", "Set locale.")]
        [UsedImplicitly]
        public static void SetLocale(string localeName)
        {
            L10nManager.SetLocaleAsync(EnumHelper<Locale>.Parse(localeName));
        }

        [RuntimeCommand("relocale", "Reload localization.")]
        [UsedImplicitly]
        public static void ReloadLocalization()
        {
            L10nManager.ReloadLocalization();
        }

        [RuntimeCommand("nodes", "Show map-nodes.")]
        [UsedImplicitly]
        public static void ShowMapNodes()
        {
            MapNode[,] nodes = DebugCommands.GameRun.CurrentMap.Nodes;
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < nodes.GetLength(0); i++)
            {
                for (int j = 0; j < nodes.GetLength(1); j++)
                {
                    stringBuilder.AppendLine(string.Format("[{0}, {1}]: {2}", i, j, nodes[i, j].StationType));
                }
            }
            Debug.Log(stringBuilder.ToString());
        }

        [RuntimeCommand("appearall", "Let all units appear.")]
        [UsedImplicitly]
        public static void AppearAll()
        {
            GameDirector.RevealAll(true);
        }

        [RuntimeCommand("opening", "Test opening comic.")]
        [UsedImplicitly]
        public static void Opening()
        {
            VnPanel panel = UiManager.GetPanel<VnPanel>();
            panel.ResetComic();
            panel.RunDialog("Opening", new DialogStorage(), new Yarn.Library(), null, null, null, null);
        }

        [RuntimeCommand("dialog", "Test dialog.")]
        [UsedImplicitly]
        public static void TestDialog()
        {
            UiManager.GetPanel<VnPanel>().RunDialog("DialogTest", new DialogStorage(), new Yarn.Library(), null, null, null, null);
        }

        [RuntimeCommand("bgm", "Test Bgm.")]
        [UsedImplicitly]
        public static void Bgm(string id, float fadeOut = 1f)
        {
            AudioManager.FadeOutAndPlayBgm(id, fadeOut, 0f, 0f, false);
        }

        [RuntimeCommand("damage", "Damage self.")]
        [UsedImplicitly]
        public static void Damage(int amount)
        {
            DebugCommands.GameRun.Damage(amount, DamageType.HpLose, true, false, null);
        }

        [RuntimeCommand("heal", "Heal self.")]
        [UsedImplicitly]
        public static void Heal(int amount)
        {
            DebugCommands.GameRun.Heal(amount, true, null);
        }

        [RuntimeCommand("deck", "Add deck card.")]
        [UsedImplicitly]
        public static void AddDeckCard(string name)
        {
            Card card = LBoL.Core.Library.CreateCard(name);
            DebugCommands.GameRun.AddDeckCard(card, false, null);
            Debug.Log("Added card: " + card.DebugName);
        }

        [RuntimeCommand("money", "Gain Money.")]
        [UsedImplicitly]
        public static void GainMoney(int money = 100)
        {
            DebugCommands.GameRun.GainMoney(money, false, null);
            if (DebugCommands.GameRun.CurrentStation is ShopStation)
            {
                ShopPanel panel = UiManager.GetPanel<ShopPanel>();
                if (panel.IsVisible)
                {
                    panel.SetShopAfterBuying();
                }
            }
        }

        [RuntimeCommand("losemoney", "Lose Money.")]
        [UsedImplicitly]
        public static void LoseMoney(int money)
        {
            DebugCommands.GameRun.LoseMoney(money);
        }

        [RuntimeCommand("basemana", "Add base mana.")]
        [UsedImplicitly]
        public static void AddBaseMana(string s)
        {
            DebugCommands.GameRun.GainBaseMana(ManaGroup.Parse(s), false);
        }

        [RuntimeCommand("removehand", "Remove hand.")]
        [UsedImplicitly]
        public static void RemoveHand()
        {
            foreach (Card card in DebugCommands.Battle.HandZone)
            {
                DebugCommands.Battle.RequestDebugAction(new RemoveCardAction(card), "Debug: RemoveHand");
            }
        }

        [RuntimeCommand("removeallcards", "Remove all cards in battle.")]
        [UsedImplicitly]
        public static void RemoveAllCards()
        {
            foreach (Card card in DebugCommands.Battle.EnumerateAllCards())
            {
                DebugCommands.Battle.RequestDebugAction(new RemoveCardAction(card), "Debug: RemoveHand");
            }
        }

        [RuntimeCommand("shake", "Shake screen with levels from 0 to 5.")]
        [UsedImplicitly]
        public static void Shake(int level = 0)
        {
            GameDirector.Shake(level, true);
        }

        [RuntimeCommand("seconds", "Show game-run played time in seconds.")]
        [UsedImplicitly]
        public static void QueryPlayedSeconds()
        {
            if (Singleton<GameMaster>.Instance.CurrentGameRun == null)
            {
                Debug.Log("Not in game-run");
                return;
            }
            Debug.Log(string.Format("Played seconds: {0}", Singleton<GameMaster>.Instance.CurrentGameRunPlayedSeconds));
        }

        [RuntimeCommand("addexp", "Add exp.")]
        [UsedImplicitly]
        public static void AddExp(int exp)
        {
            if (Singleton<GameMaster>.Instance.CurrentGameRun != null)
            {
                Debug.LogError("Cannot add exp in game-run");
                return;
            }
            GameMaster.DebugAddExp(exp);
        }

        [RuntimeCommand("expmax", "Add exp to max level (10).")]
        [UsedImplicitly]
        public static void ExpMax()
        {
            if (Singleton<GameMaster>.Instance.CurrentGameRun != null)
            {
                Debug.LogError("Cannot add exp in game-run");
                return;
            }
            GameMaster.DebugAddExp(ExpHelper.MaxExp);
        }

        [RuntimeCommand("power", "Gain power.")]
        [UsedImplicitly]
        public static void Power(int gainValue = 10)
        {
            DebugCommands.GameRun.GainPower(gainValue, false);
        }

        [RuntimeCommand("explevel", "Add exp to specific level. (Maybe obsolete)")]
        [UsedImplicitly]
        public static void CheckExpLevel(int exp)
        {
            int levelForTotalExp = ExpHelper.GetLevelForTotalExp(exp);
            int maxExpForLevel = ExpHelper.GetMaxExpForLevel(levelForTotalExp);
            int num = exp - maxExpForLevel;
            int expForLevel = ExpHelper.GetExpForLevel(levelForTotalExp);
            if (levelForTotalExp == ExpHelper.MaxLevel)
            {
                Debug.Log(string.Format("Lv. {0} (max) ({1}/{2})", levelForTotalExp, num, expForLevel));
                return;
            }
            Debug.Log(string.Format("Lv. {0} ({1}/{2})", levelForTotalExp, num, expForLevel));
        }

        [RuntimeCommand("debugexp", "Add TempDebugExp. (Maybe obsolete)")]
        [UsedImplicitly]
        public static void GainDebugExp(int exp)
        {
            GameMaster.TempDebugExp = new int?(exp);
        }

        [RuntimeCommand("chat", "Let Player and the first enemy chat something.")]
        [UsedImplicitly]
        public static void Chat(string content = null)
        {
            if (content == null)
            {
                content = "测试对话 Test chat";
            }
            DebugCommands.PlayerView.Chat(content, 2f, ChatWidget.CloudType.LeftTalk, 0f);
            DebugCommands.Enemy0View.Chat(content, 2f, ChatWidget.CloudType.RightTalk, 0f);
        }

        [RuntimeCommand("message", "Send a message on Top Message Panel.")]
        [UsedImplicitly]
        public static void Message(string message)
        {
            UiManager.GetPanel<TopMessagePanel>().ShowMessage(message);
        }

        [RuntimeCommand("cardid", "Show/hide card Ids in all card widgets.")]
        [UsedImplicitly]
        public static void CardId()
        {
            GameMaster.ShowCardId = !GameMaster.ShowCardId;
            CardWidget[] array = UnityEngine.Object.FindObjectsOfType<CardWidget>();
            for (int i = 0; i < array.Length; i++)
            {
                array[i].SetIdVisible(GameMaster.ShowCardId);
            }
        }

        [RuntimeCommand("revealall", "Reveal all cards and exhibits in collection/museum.")]
        [UsedImplicitly]
        public static void RevealAll()
        {
            foreach (ValueTuple<Type, CardConfig> valueTuple in LBoL.Core.Library.EnumerateCardTypes())
            {
                GameMaster.RevealCard(valueTuple.Item2.Id);
            }
            foreach (ValueTuple<Type, ExhibitConfig> valueTuple2 in LBoL.Core.Library.EnumerateExhibitTypes())
            {
                GameMaster.RevealExhibit(valueTuple2.Item2.Id);
            }
        }

        [RuntimeCommand("revealcard", "Reveal a card in collection/museum with Id, case sensitive.")]
        [UsedImplicitly]
        public static void RevealCard(string cardId)
        {
            GameMaster.RevealCard(cardId);
        }

        [RuntimeCommand("puzzles", "List all the puzzles.")]
        [UsedImplicitly]
        public static void ListPuzzles()
        {
            foreach (PuzzleFlag puzzleFlag in PuzzleFlags.AllPuzzleFlags)
            {
                PuzzleFlagDisplayWord displayWord = PuzzleFlags.GetDisplayWord(puzzleFlag);
                Debug.Log(displayWord.Name + " = " + displayWord.Description);
            }
        }

        [RuntimeCommand("dollslot", "Create or remove slot (use negative)")]
        [UsedImplicitly]
        public static void CreateDollSlots(int n)
        {
            if (n > 0)
            {
                DebugCommands.Battle.RequestDebugAction(new AddDollSlotAction(n), "Debug: DollSlot");
                return;
            }
            if (n < 0)
            {
                DebugCommands.Battle.RequestDebugAction(new RemoveDollSlotAction(-n), "Debug: DollSlot");
            }
        }

        [RuntimeCommand("doll", "")]
        [UsedImplicitly]
        public static void CreateDoll(string id)
        {
            DebugCommands.Battle.RequestDebugAction(new AddDollAction(LBoL.Core.Library.CreateDoll(id)), "Debug: Doll");
        }

        [RuntimeCommand("dollactive", "")]
        [UsedImplicitly]
        public static void TriggerDollActive(int index = 0, bool remove = true)
        {
            Doll doll = DebugCommands.Battle.Player.Dolls[index];
            DebugCommands.Battle.RequestDebugAction(new TriggerDollActiveAction(doll, true), "Debug: DollActive");
        }

        [RuntimeCommand("dollpassive", "")]
        [UsedImplicitly]
        public static void TriggerDollPassive(int index, bool remove = false)
        {
            Doll doll = DebugCommands.Battle.Player.Dolls[index];
            DebugCommands.Battle.RequestDebugAction(new TriggerDollPassiveAction(doll, false), "Debug: DollPassive");
        }

/*        [RuntimeCommand("clearachievement", "Clear all achievements in current profile.")]
        [UsedImplicitly]
        public static void ClearAchievement()
        {
            GameMaster.ClearAllAchievements();
        }

        [RuntimeCommand("achievement", "Gain an achievement with the key, case sensitive.")]
        [UsedImplicitly]
        public static void Achievement(string key)
        {
            GameMaster.UnlockAchievement(key);
        }*/

        [RuntimeCommand("bosses", "List all bosses in current game.")]
        [UsedImplicitly]
        public static void ListBosses()
        {
            foreach (Stage stage in DebugCommands.GameRun.Stages)
            {
                if (stage.IsSelectingBoss)
                {
                    string[] array = new string[5];
                    array[0] = stage.Name;
                    array[1] = " selected: ";
                    array[2] = stage.SelectedBoss;
                    array[3] = ", boss: ";
                    int num = 4;
                    EnemyGroupEntry boss = stage.Boss;
                    array[num] = ((boss != null) ? boss.Id : null);
                    Debug.Log(string.Concat(array));
                }
                else
                {
                    string name = stage.Name;
                    string text = ": boss: ";
                    EnemyGroupEntry boss2 = stage.Boss;
                    Debug.Log(name + text + ((boss2 != null) ? boss2.Id : null));
                }
            }
        }

        [RuntimeCommand("achievements", "List all achievements in current profile.")]
        [UsedImplicitly]
        public static void ListAchievements()
        {
            foreach (string text in Singleton<GameMaster>.Instance.CurrentProfile.Achievements)
            {
                IDisplayWord achievementDisplayWord = Achievements.GetAchievementDisplayWord(text);
                Debug.Log(string.Concat(new string[] { "[", text, "] ", achievementDisplayWord.Name, ": ", achievementDisplayWord.Description }));
            }
        }

        [RuntimeCommand("unlockdiff", "Unlock all difficulty.")]
        [UsedImplicitly]
        public static void UnlockDifficulty()
        {
            GameMaster.UnlockDifficulty();
        }

        [RuntimeCommand("showallcards", "Show all cards in mesuem.")]
        [UsedImplicitly]
        public static void ShowAllCards()
        {
            GameMaster.ShowAllCardsInMuseum = true;
            foreach (ValueTuple<Type, CardConfig> valueTuple in LBoL.Core.Library.EnumerateCardTypes())
            {
                GameMaster.RevealCard(valueTuple.Item2.Id);
            }
            UiManager.GetPanel<MuseumPanel>().RefreshCards();
        }

/*        [RuntimeCommand("manyhints", "Show 10 hints.")]
        [UsedImplicitly]
        public static void ManyHints(int count)
        {
            for (int i = 0; i < count; i++)
            {
                UiManager.GetPanel<TopMessagePanel>().UnlockAchievement("Reimu");
            }
        }*/
    }
}
