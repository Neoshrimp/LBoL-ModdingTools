using LBoL.Core.Battle.Interactions;
using LBoL.Core;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using LBoL.Presentation.UI;
using LBoL.Presentation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using static DebugMode.Plugin;
using System.Linq;
using UnityEngine.UI;

namespace DebugMode
{
    // not working
    public class CardScreenCap
    {

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

                GameObject.Destroy(tex);
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

            GameObject.Destroy(srcCapButton);

        }
    }
}
