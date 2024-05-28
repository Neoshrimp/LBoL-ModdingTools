using LBoL.Presentation.UI;
using LBoL.Presentation.Units;
using LBoL.Presentation;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DebugMode
{
    public static class Utils
    {
        public static void InstaAbandonGamerun()
        {
            if (GameMaster.Instance.CurrentGameRun.Battle != null)
            {
                UiManager.LeaveBattle();
            }
            UiManager.LeaveGameRun();
            GameMaster.Instance.CurrentGameRun = null;
            LBoL.Presentation.Environment.Instance.ClearEnvironment();
            GameDirector.ClearAll();
            GameMaster.UnloadGameRunUi();
            GC.Collect();
            Resources.UnloadUnusedAssets();
        }
    }
}
