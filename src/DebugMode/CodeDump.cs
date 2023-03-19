        static BattleAdvTestStation testStation = null;

        static MapNode fakeNode = null;


        //DebugBattleLogPanel

        static StationType? realType = null;

        public void ToggleDebugMenu()
        {

            try
            {
                if (GameMaster.Instance?.CurrentGameRun != null
                    //&& GameMaster.Instance?.CurrentGameRun?.CurrentStation?.Status == StationStatus.Finished
                    )
                {


                    /*                    if (true)
                                        {
                                            fakeNode = new MapNode(GameMaster.Instance.CurrentGameRun.CurrentMap, 0, 1, GameMaster.Instance.CurrentGameRun.CurrentMap.Nodes[0, 0].Act);
                                            fakeNode.Status = MapNodeStatus.CrossActive;
                                            fakeNode.StationType = StationType.BattleAdvTest;
                                            fakeNode.AdjacencyList.AddRange(GameMaster.Instance.CurrentGameRun.CurrentMap.VisitingNode.AdjacencyList);

                                            GameMaster.Instance.CurrentGameRun.CurrentMap.Nodes[0, 1] = fakeNode;
                                        }*/

                    var debugPanel = UiManager.GetPanel<DebugBattleLogPanel>();
                    var visitingNode = GameMaster.Instance.CurrentGameRun.CurrentMap.VisitingNode;


                    if (debugPanel.IsVisible)
                    {
                        debugPanel.Hide();
                        UiManager.Hide<RewardPanel>(false);
                        if(realType != null)
                            visitingNode.StationType = realType.Value;

                    }

                    else 
                    {
                        var nodes = GameMaster.Instance.CurrentGameRun.CurrentMap.Nodes;
                        for(int x = 0;  x < nodes.GetLength(0); x++)
                        {
                            for (int y = 0; y < nodes.GetLength(1); y++)
                            {
                                var n = nodes[x, y];
                                log.LogInfo($"{n?.X}, {n?.Y}: {n?.StationType}, {n?.Status}");
                            
                            }
                        }


                        //GameMaster.Instance.CurrentGameRun.CurrentMap.Nodes[1, 1].StationType = StationType.BattleAdvTest;
                        UiManager.Hide<RewardPanel>(false);
                        realType = visitingNode.StationType;

                        GameMaster.Instance.StopAllCoroutines();


                        if (GameMaster.Instance.CurrentGameRun.Battle != null)
                        {
                            UiManager.LeaveBattle();
                        }

                        //GameDirector.ClearAll();



                        GameMaster.Instance.CurrentGameRun.CurrentStation.Status = StationStatus.Finished;



                        //visitingNode.Status = MapNodeStatus.Active;
                        visitingNode.StationType = StationType.BattleAdvTest;

                        GameMaster.Instance.CurrentGameRun.EnterMapNode(visitingNode, true);

                        GameMaster.Instance.StartCoroutine(GameMaster.Instance.RunInnerStation(GameMaster.Instance.CurrentGameRun.CurrentStation));



                    }

                    /*                    {
                                            log.LogInfo("click");
                                            var testStation = (BattleAdvTestStation)GameMaster.Instance.CurrentGameRun.CurrentStage.CreateStationFromType(GameMaster.Instance.CurrentGameRun.CurrentMap.VisitingNode, StationType.BattleAdvTest);
                                            //UiManager.GetPanel<SelectDebugPanel>().Show(testStation);
                                            GameMaster.Instance.CurrentGameRun.CurrentStation = testStation;
                                            var visitingNode = GameMaster.Instance.CurrentGameRun.CurrentMap.VisitingNode;


                                            visitingNode.StationType = StationType.BattleAdvTest;
                                            GameMaster.Instance.CurrentGameRun.CurrentStation.Status = StationStatus.Finished;
                                            visitingNode.Status = MapNodeStatus.Visiting;


                                            //GameMaster.Instance.CurrentGameRun.LeaveStation();
                                            //GameMaster.Instance.RunInnerStation(testStation);
                                            //GameMaster.Instance.CoTeleportToNode(visitingNode);
                                            GameMaster.Instance.CurrentGameRun.EnterMapNode(visitingNode, true);
                                            //GameMaster.RequestEnterMapNode(visitingNode.X, visitingNode.Y);

                                            GameMaster.Instance.CurrentGameRun.CurrentStation.Status = StationStatus.Dialog;
                                            visitingNode.Status = MapNodeStatus.Active;

                                        }*/


                    /*                    {
                                            log.LogInfo("click2");
                                            testStation = (BattleAdvTestStation)GameMaster.Instance.CurrentGameRun.CurrentStage.CreateStationFromType(GameMaster.Instance.CurrentGameRun.CurrentMap.VisitingNode, StationType.BattleAdvTest);
                                            UiManager.GetPanel<SelectDebugPanel>().Show(testStation);

                                        }*/


                }
                else
                {
                    log.LogInfo("Run is not started. Please start a run to use debug menu");
                }
            }
            catch (Exception ex)
            {

                log.LogError(ex);
            }


        }




            // out card config
			using (FileStream fileStream = File.Open("DEEZNUTS.txt", FileMode.Open, FileAccess.Write, FileShare.None))
			{
				using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
				{
					foreach (var cc in CardConfig.AllConfig())
					{
						streamWriter.WriteLine(cc);

						streamWriter.WriteLine("------------------------");
					}
				}
			}



            // out card config
			using (FileStream fileStream = File.Open("readableConfig.txt", FileMode.Create, FileAccess.Write, FileShare.None))
			{
				using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
				{
					foreach (var cc in CardConfig.AllConfig())
					{
						streamWriter.WriteLine(cc);

						streamWriter.WriteLine("------------------------");
					}
				}
			}






        [HarmonyPatch(typeof(GameMap), nameof(GameMap.EnterNode))]
        class EnterNode_Patch
        {

            

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                int i = 0;
                var ciList = instructions.ToList();
                var c = ciList.Count();
                CodeInstruction prevCi = null;
                foreach (var ci in instructions)
                {
                    if ((ci.opcode == OpCodes.Beq || ci.opcode == OpCodes.Beq_S)
                        && prevCi.opcode == OpCodes.Ldc_I4_3 
                        && ciList[i-2].Is(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(MapNode), nameof(MapNode.Status)))
                        )
                    {
                        log.LogInfo("deeznuts");


                        yield return new CodeInstruction(OpCodes.Pop);
                        yield return new CodeInstruction(OpCodes.Pop);

                        yield return new CodeInstruction(OpCodes.Br, ci.operand);

                    }
                    else
                    {
                        yield return ci;
                    }
                    prevCi = ci;
                    i++;
                }
            }

        }


        public static void DumpFields(object o, int currentDepth = 0, int maxDepth = 3)
        {

            // incorrect reflector
            var fields = AccessTools.GetDeclaredFields(o.GetType());

            log.LogInfo($"{new string(' ', currentDepth * 2)}{o}");
            foreach (var f in fields)
            {
                try
                {
                    var val = f.IsStatic ? f.GetValue(null) : f.GetValue(o);
                    log.LogInfo($"{new string(' ', currentDepth * 2)}{f.Name}: {val}");
                    if (f.FieldType is object && currentDepth < maxDepth)
                    {
                        DumpFields(val, currentDepth, maxDepth);
                    }
                }
                catch (Exception)
                {
                    log.LogWarning($"{new string(' ', currentDepth * 2)}Exception on {f.Name}");
                    continue;
                }
            }
        }
		
		
		CardConfig.AllConfig().Where(c => c.Owner == "Reimu" && c.Rarity == Rarity.Rare).ToList().ForEach(c => Plugin.SaveTexture2D(c.Id, ResourcesHelper.CardImages[c.Id]))
		
		
        static KeyboardShortcut testTextureKey = new KeyboardShortcut(KeyCode.H);

        void Update()
        {
            if (testTextureKey.IsDown())
            {
                //SuikaBigball
                (ResourcesHelper.CardImages["TemporalGuardian"] as Texture2D).EncodeToPNG();

                var tg = "TemporalGuardian";

                tg = "SuikaBigball";

                SaveTexture2D(tg, ResourcesHelper.CardImages[tg]);
            }
        }



        public static Texture2D duplicateTexture(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }

        public static void SaveTexture2D(string name, Texture tex)
        {

            log.LogInfo("deeznuts");

            var tex2d = (Texture2D)tex;

            if (!tex2d.isReadable)
            {
                tex2d = duplicateTexture(tex2d);
            }


            var texBytes = tex2d.EncodeToPNG();

            var loc = Path.Combine(Application.dataPath, "exportedTextures");
            name = name + ".png";

            Directory.CreateDirectory(loc);

            File.WriteAllBytes(Path.Combine(loc, name), texBytes);

        }