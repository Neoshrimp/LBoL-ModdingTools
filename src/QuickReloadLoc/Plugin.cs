using BepInEx;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using LBoL.Core;
using LBoL.Presentation;
using LBoL.Presentation.I10N;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace QuickReloadLoc
{
    [BepInPlugin(GUID, "Quick locale reload", version)]
    [BepInProcess("LBoL.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "neo.lbol.tools.QuickReloadLoc";
        public const string version = "1.0.0";

        private static readonly Harmony harmony = new Harmony(GUID);

        internal static BepInEx.Logging.ManualLogSource log;

        internal static BaseUnityPlugin instance;

        internal static BepInEx.Configuration.ConfigEntry<KeyboardShortcut> reloadKeyConfig;

        private void Awake()
        {
            log = Logger;

            // very important. Without this the entry point MonoBehaviour gets destroyed
            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = HideFlags.HideAndDontSave;
            instance = this;

            reloadKeyConfig = Config.Bind("Keybinds", "ReloadLocKey", new KeyboardShortcut(KeyCode.L), "Hotkey for reloading current loc");

            harmony.PatchAll();

        }

        private void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchSelf();
            if(SettingPanel_Patch.reloadGo != null)
                Object.Destroy(SettingPanel_Patch.reloadGo);
        }


        private void Update()
        {
            if (reloadKeyConfig.Value.IsDown()
                && UiManager.Instance != null
                && UiManager.Instance._panelTable.ContainsKey(typeof(SettingPanel)))
            { 
                instance.StartCoroutine(SettingPanel_Patch.CoReloadLoc());
            }
        }

        [HarmonyPatch(typeof(SettingPanel), "OnShown")]
        class SettingPanel_Patch
        {

            public static GameObject reloadGo;

            public const string goName = "ReloadLoc";

            public static void ReloadLoc()
            {
                if (UiManager.Instance._panelTable.ContainsKey(typeof(SettingPanel)))
                {
                    var panel = UiManager.GetPanel<SettingPanel>();
                    panel.StartCoroutine(CoReloadLoc());
                }
            }

            public static IEnumerator CoReloadLoc()
            {
                yield return UiManager.ShowLoading(0.1f).ToCoroutine(null);
                yield return UniTask.ToCoroutine(() => L10nManager.ReloadLocalization());
                yield return UiManager.HideLoading(0.1f).ToCoroutine(null);
                yield break;
            }

            static void Postfix(SettingPanel __instance)
            {
                var localeT = __instance.localeDropdown.gameObject.transform.parent;
                if (localeT.Find(goName) == null)
                {

                    var btnTemplate = localeT.parent.parent.Find("RightPanel")?.Find("RecommendVolume")?.Find("CommonButton")?.gameObject;
                    if (btnTemplate == null)
                        return;

                    reloadGo = GameObject.Instantiate(btnTemplate, localeT);
                    //Object.Destroy(reloadGo.GetComponentInChildren<HorizontalLayoutGroup>());
                    reloadGo.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 300);

                    reloadGo.transform.localPosition = new Vector3(-700, 0, 0);

                    reloadGo.name = goName;

                    Object.Destroy(reloadGo.GetComponent<Button>());

                    var textMesh = reloadGo.GetComponentInChildren<TextMeshProUGUI>();
                    Object.Destroy(reloadGo.GetComponentInChildren<LocalizedText>());
                    textMesh.text = "Reload";

                    instance.StartCoroutine(AddButton());

                }


            }

            private static IEnumerator AddButton()
            {
                // fuck unity
                yield return null;
                var button = reloadGo.AddComponent<Button>();
                button.onClick.AddListener(() => ReloadLoc());

                var widget = reloadGo.GetComponent<CommonButtonWidget>();
                widget.button = button;

                // widget awake method
                button.onClick.AddListener(() => AudioManager.Button(widget.audioIndex));

            }
        }



    }
}
