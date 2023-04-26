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
		
		
		static void PrintMembers(object obj, int depth = 0 , int maxDepth = 3)
        {
            if (depth > maxDepth) return;

            Type type = obj.GetType();

            // Print all fields
            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {


                try
                {
                    log.LogInfo($"{0}{1} = {depth}" + new string(' ', depth * 2) + field.Name + field.GetValue(obj));
                    if (field.FieldType.IsClass)
                    {
                        object fieldValue = field.GetValue(obj);
                        if (fieldValue != null)
                        {
                            PrintMembers(fieldValue, depth + 1, maxDepth);
                        }
                    }
                }
                catch (Exception)
                {

                }
            }

            // Print all properties
            foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                try
                {
                    if (prop.CanRead)
                    {
                        log.LogInfo($"{0}{1} = {depth}" + new string(' ', depth * 2) + prop.Name + prop.GetValue(obj));
                        if (prop.PropertyType.IsClass)
                        {
                            object propValue = prop.GetValue(obj);
                            if (propValue != null)
                            {
                                PrintMembers(propValue, depth + 1, maxDepth);
                            }
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
        }
		
		
		
		// check if method names differ
		static KeyboardShortcut testTextureKey = new KeyboardShortcut(KeyCode.H);
		
		class MethodComp : IEqualityComparer<MethodInfo>
        {
            public bool Equals(MethodInfo x, MethodInfo y)
            {
                return x.Name == y.Name;
            }

            public int GetHashCode(MethodInfo obj)
            {
                return obj.Name.GetHashCode();
            }
        }
		
        void Update()
        {
            if (testTextureKey.IsDown())
            {
                //var comp = new EqualityComparer<MethodInfo>((m1, m2) => m1.Name == m2.Name);

                var types = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name.Contains("LBoL.Config")).Single().GetTypes().
                    Where(t => t.Name.EndsWith("Config"));

                types.Do(t => log.LogInfo(t.Name));

                log.LogInfo($"----{types.Count()}");


                types.SelectMany(t => AccessTools.GetDeclaredMethods(t)).Where(m => !m.IsSpecialName && !m.Name.StartsWith("<")).
                    Do(m => log.LogInfo($"{m.DeclaringType}: {m.Name}"));

                var methods = types.Select(t => AccessTools.GetDeclaredMethods(t).Where(m => !m.IsSpecialName && !m.Name.StartsWith("<")).ToHashSet(new MethodComp()));

                var u = methods.Aggregate((s1, s2) => s1.Union(s2).ToHashSet(new MethodComp()));
                var v = methods.Aggregate((s1, s2) => s1.Intersect(s2).ToHashSet(new MethodComp()));

                log.LogInfo("----");

                u.Except(v).Do(m => log.LogInfo($"{m.DeclaringType}: {m.Name}"));

                    //Aggregate((s1, s2) => s1.Union(s2, new MethodComp()).Except(s1.Intersect(s2, new MethodComp())).ToHashSet()).
                    //Do(m => log.LogInfo($"{m.DeclaringType}: {m.Name}"))

            }
        }


// register with reflection
internal void RegisterConfig(EntityDefinition entityDefinition)
	{

		// this is a bit more complicated
		var Id = UniquefyId(entityDefinition.Id);
		log.LogInfo($"{Id},  C:{entityDefinition.GetConfigType()}");

		try
		{
			var configType = entityDefinition.GetConfigType();

			var m_FromId = ConfigHelper.GetFromIdMethod(configType);

			var t_getConfig = GeneralHelper.MakeGenericType(typeof(IConfigProvider<>), new Type[]{ entityDefinition.GetConfigType() });
			var m_getConfig = AccessTools.Method(t_getConfig, nameof(IConfigProvider<object>.GetConfig));
			var newConfig = m_getConfig.Invoke(entityDefinition, null);

			log.LogDebug(newConfig);

			var config = m_FromId.Invoke(null, new object[] { Id});

			if (config == null)
			{
				log.LogInfo($"initial config load for {Id}");



				//var ref_Data = AccessTools.StaticFieldRefAccess<C[](f_Data);
				//ref_Data() = ref_Data().AddItem(newConfig).ToArray();


				// static method
				var f_Data = AccessTools.Field(entityDefinition.GetConfigType(), "_data");
				var m_AddToArray = AccessTools.Method(typeof(HarmonyLib.CollectionExtensions), nameof(HarmonyLib.CollectionExtensions.AddToArray));

				m_AddToArray = m_AddToArray.MakeGenericMethod(new Type[] { configType });
				m_AddToArray.Invoke(null, new object[] { f_Data.GetValue(null), newConfig });

				var t_ConfigDic = GeneralHelper.MakeGenericType(typeof(Dictionary<,>), new Type[] { typeof(string), configType });
				var f_IdTable = AccessTools.Field(configType, "_IdTable");
				var m_dicAdd = AccessTools.Method(t_ConfigDic, nameof(Dictionary<object, object>.Add));
				m_dicAdd.Invoke(f_IdTable.GetValue(null), new object[] { Id, newConfig });

				//((Dictionary<string, C>)f_IdTable.GetValue(null)).Add(Id, newConfig);

			}
			else
			{
				log.LogInfo($"secondary config reload for {Id}");
				config = newConfig;
			}



		}
		catch (Exception ex)
		{
			log.LogError($"Exception registering {Id}: {ex}");

		}
	}
	
//resourceManager = new ResourceManager(assembly.GetName().Name+ ".Properties.Resources", assembly);

/*            var res = resourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

			foreach (var r in res)
			{
				UnityEngine.Debug.Log(((DictionaryEntry)r).Key);
			}*/

//return resourceManager.GetStream(id);

    [HarmonyPatch(typeof(GameEvent<DamageEventArgs>), nameof(GameEvent<DamageEventArgs>.Execute))]
    class Gdeez_Patch
    {

        static void Prefix(GameEvent<DamageEventArgs> __instance)
        {

            foreach (KeyValuePair<GameEventPriority, List <GameEvent<DamageEventArgs>.HandlerEntry>> kv in __instance._invocationListDict)
            {
                foreach (var h in kv.Value)
                {
                    log.LogDebug($"{h.Handler.Target}, {h.Handler.Method}");
                }
            
            }
        }
    }
	
	
	    [HarmonyPatch(typeof(GameEvent<DamageEventArgs>), nameof(GameEvent<DamageEventArgs>.Execute))]
    class Gdeez_Patch
    {

        static void Deezlog(object o)
        {
            log.LogDebug(o);
        }


        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            int i = 0;
            var ciList = instructions.ToList();
            var c = ciList.Count();
            CodeInstruction prevCi = null;
            foreach (var ci in instructions)
            {
                if (ci.opcode == OpCodes.Callvirt && (MethodInfo)ci.operand == AccessTools.PropertyGetter(typeof(GameEvent<DamageEventArgs>.HandlerEntry), "Handler"))
                {
                    log.LogDebug("injected");
                    yield return ci;
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Gdeez_Patch), "Deezlog"));



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
	
	        [HarmonyPatch]
        class Library_Patch
        {


            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return ExtraAccess.InnerMoveNext(typeof(Library), nameof(Library.RegisterAllAsync));
            }


            static void LoadModded()
            {
                EntityManager.Instance.RegisterUsers();


            }

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                foreach (var ci in instructions)
                {
                    if (ci.Is(OpCodes.Stsfld, AccessTools.Field(typeof(Library), nameof(Library._registered))))
                    {
                        log.LogDebug("injected library");

                        yield return ci;
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Library_Patch), nameof(Library_Patch.LoadModded)));
                    }
                    else
                    {
                        yield return ci;
                    }
                }
            }


        }


        [HarmonyPatch]
        class ResourcesHelper_Patch
        {



            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return ExtraAccess.InnerMoveNext(typeof(ResourcesHelper), nameof(ResourcesHelper.InitializeAsync));
            }


            static void LoadModded()
            { 
                EntityManager.Instance.AssetsForResourceHelper();

            }

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                foreach (var ci in instructions)
                {
                    if (ci.Is(OpCodes.Stsfld, AccessTools.Field(typeof(ResourcesHelper), nameof(ResourcesHelper._loaded))))
                    {
                        log.LogDebug("injected resouces");

                        yield return ci;
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ResourcesHelper_Patch), nameof(ResourcesHelper_Patch.LoadModded)));
                    }
                    else
                    {
                        yield return ci;
                    }
                }
            }



        }

