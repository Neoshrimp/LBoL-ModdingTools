using BepInEx.Configuration;
using HarmonyLib;
using LBoL.Presentation.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using static DebugMode.Plugin;

namespace DebugMode
{
    public class DebugKeys
    {
        
        [HarmonyPatch(typeof(UiManager), nameof(UiManager.Awake))]
        class UiManager_Patch
        {
            // runs earlier than scriptengine inits lol
            static void Postfix(UiManager __instance)
            {
                var debugInputActionMap = __instance.inputActions.FindActionMap("Debug", false);
                debugInputActionMap?.Enable();

                if (__instance.DebugBattleLogAction != null)
                { 
                    __instance.DebugBattleLogAction.ApplyBindingOverride("<Keyboard>/" + BattleLog.Value);
                    log.LogInfo(__instance.DebugBattleLogAction);
                }

                if (__instance.DebugMenuAction != null)
                {
                    __instance.DebugMenuAction.ApplyBindingOverride("<Keyboard>/" + DebugMenu.Value);
                    __instance.DebugMenuAction.AddBinding("<Mouse>/middleButton");
                    log.LogInfo(__instance.DebugMenuAction);
                }

                if (__instance.DebugConsoleAction != null)
                {
                    __instance.DebugConsoleAction.ApplyBindingOverride("<Keyboard>/" + DevConsole.Value);
                    log.LogInfo(__instance.DebugConsoleAction);
                }

            }
        }

    }
}
