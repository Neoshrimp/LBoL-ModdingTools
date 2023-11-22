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


using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using System.Collections;
using HarmonyLib;
using 
using LBoL.EntityLib.Exhibits.Shining;

namespace LBoLEntitySideloader.TemplateGen
{
    // 2do rename
    public abstract class TemplateGen<ED> where ED : EntityDefinition
    {


        public readonly static Assembly sideLoaderAss = typeof(BepinexPlugin).Assembly;

        public readonly Assembly originAssembly;

        public Assembly newAssembly;

        public string newAssName;

        public CodeNamespace newNameSpace;


        protected CodeCompileUnit compileUnit;



        public Dictionary<IdContainer, CodeTypeDeclaration> generatedTypes = new Dictionary<IdContainer, CodeTypeDeclaration>();

        private bool properInnit = false;


        // 2do add codeDom lib

        public TemplateGen(Assembly originAssembly = null)
        {
            if(originAssembly == null )
                this.originAssembly = new StackTrace().GetFrame(2).GetMethod().ReflectedType.Assembly;
            else
                this.originAssembly = originAssembly;

            // 2do error check
            newAssName = $"{this.originAssembly.GetName().Name}.{typeof(ED).Name}.Dynamic";






/*            if (EntityManager.Instance?.sideloaderUsers?.userInfos?.TryGetValue(originAssembly, out user) == null)
            {
                Log.log.LogError($"{this.GetType().Name} must be instantiated after {nameof(EntityManager.RegisterSelf)} has been called.");
                return;
            }*/



            if (newNameSpace == null)
            {
                compileUnit = new CodeCompileUnit();
                newNameSpace = new CodeNamespace($"{newAssName}");
                compileUnit.Namespaces.Add(newNameSpace);
                newNameSpace.Imports.Add(new CodeNamespaceImport("System"));


                newNameSpace.Imports.Add(new CodeNamespaceImport("LBoL.Base"));
                newNameSpace.Imports.Add(new CodeNamespaceImport("LBoL.Base.Extensions"));
                newNameSpace.Imports.Add(new CodeNamespaceImport("LBoL.ConfigData"));
                newNameSpace.Imports.Add(new CodeNamespaceImport("LBoL.Core"));
                newNameSpace.Imports.Add(new CodeNamespaceImport("LBoL.Core.Cards"));
                newNameSpace.Imports.Add(new CodeNamespaceImport("LBoL.Presentation"));
                newNameSpace.Imports.Add(new CodeNamespaceImport("LBoLEntitySideloader"));
                newNameSpace.Imports.Add(new CodeNamespaceImport("LBoLEntitySideloader.Resource"));
                newNameSpace.Imports.Add(new CodeNamespaceImport("LBoLEntitySideloader.LBoLEntitySideloader.TemplateGen"));
                newNameSpace.Imports.Add(new CodeNamespaceImport("System"));
                newNameSpace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
                newNameSpace.Imports.Add(new CodeNamespaceImport("System.Linq"));
                newNameSpace.Imports.Add(new CodeNamespaceImport("System.Reflection"));

                // 2do more imports
            }


            MethodCache.methodCacheDic.TryAdd(newAssName, new MethodCache());


            properInnit = true;
        }



        internal CodeMemberMethod MakeGetIdMethod(IdContainer Id, CodeTypeDeclaration targetClass)
        {
            CodeMemberMethod newMethod = new CodeMemberMethod();
            newMethod.Name = nameof(EntityDefinition.GetId);
            newMethod.ReturnType = new CodeTypeReference(typeof(IdContainer));
            newMethod.Attributes = MemberAttributes.Public | MemberAttributes.Override;


            newMethod.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(Id.ToString())));
            //new CodeSnippetExpression(@$"return {Id};")

            targetClass.Members.Add(newMethod);
            return newMethod;
        }


        internal CodeMemberMethod MakeMethod<R>(string name, Func<R> func, CodeTypeDeclaration targetClass, bool dontOverwrite = false) where R : class
        {

            MethodCache.methodCacheDic.TryAdd(newAssName, new MethodCache());

            MethodCache.methodCacheDic[newAssName].AddMethod(typeof(ED), targetClass.Name, name, func);

            /*            targetClass.Members.Add(new CodeMemberField() { Name = $"{name}_payload", Type = new CodeTypeReference(typeof(MethodInfo)), Attributes = MemberAttributes.Private });*/

            CodeMemberMethod newMethod = new CodeMemberMethod();
            newMethod.Name = name;
            newMethod.ReturnType = new CodeTypeReference(func.Method.ReturnType);
            newMethod.Attributes = MemberAttributes.Public | MemberAttributes.Override;


            if (dontOverwrite)
                newMethod.CustomAttributes = new CodeAttributeDeclarationCollection
                {
                    new CodeAttributeDeclaration(new CodeTypeReference(typeof(DontOverwriteAttribute)))
                };

            /*            foreach (var p in methodInfo.GetParameters())
                        {
                            newMethod.Parameters.Add(new CodeParameterDeclarationExpression(p.ParameterType, p.Name));
                        }*/




            newMethod.Statements.Add(new CodeSnippetExpression(@$"
                var method = MethodCache.methodCacheDic[this.GetType().Assembly.GetName().Name].GetMethod(this.TemplateType(), this.GetType().Name, ""{name}"");
                return method() as {func.Method.ReturnType.FullName};
            "));


            targetClass.Members.Add(newMethod);
            return newMethod;
        }



        protected CodeTypeDeclaration InnitDefintionType(IdContainer Id, bool overwriteVanilla = false)
        {



            if (!properInnit)
            {
                Log.log.LogError($"{this.GetType().Name} was not properly initialized");
                return null;
            }



            if (!generatedTypes.ContainsKey(Id))
            {
                var targetClass = new CodeTypeDeclaration($"{Id}Definition");
                targetClass.TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed;
                newNameSpace.Types.Add(targetClass);


                targetClass.BaseTypes.Add(typeof(ED));
                if (overwriteVanilla)
                    targetClass.CustomAttributes.Add(new CodeAttributeDeclaration());

                generatedTypes.Add(Id, targetClass);

                MakeGetIdMethod(Id, targetClass);
                return targetClass;
            }
            else
            {
                Log.log.LogError($"{typeof(ED)} template gen: type {Id}Definition was already generated.");
                return null;
            }





        }




        public void FinalizeGen()
        {

            Log.log.LogInfo($"{newAssName}: generating {typeof(ED)} definitions..");

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(OutputCSharpCode().ToString
                ());


            var refPaths = new[] {
                typeof(object).GetTypeInfo().Assembly.Location,
                typeof(EntityManager).GetTypeInfo().Assembly.Location,
                typeof(System.Reflection.MemberInfo).GetTypeInfo().Assembly.Location,
                Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.dll")
            };

            //AppDomain.CurrentDomain.GetAssemblies().ToList().ForEach(a => a.GetTypes().ToList().ForEach(t => Log.log.LogDebug(t.Name)));

            //Assembly.GetAssembly(typeof(MetadataReference)).GetTypes().ToList().ForEach(t => Log.log.LogDebug(t.Name));
            

            var references = new List<MetadataReference> ();

            //MetadataReference[] references = refPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();
            foreach (var p in refPaths)
            {
                Log.log.LogDebug(p);
                references.Add(MetadataReference.CreateFromFile(p, new MetadataReferenceProperties(MetadataImageKind.Assembly), null));



            }

/*            var deez = AccessTools.Method(typeof(MetadataReference), "CreateFromAssembly", new Type[] { typeof(Assembly) });
            references.Add((MetadataReference)deez.Invoke(null, new object[] { typeof(object).Assembly }));*/

            //references.Add(MetadataReference.CreateFromAssembly(typeof(object).Assembly));


            references.Add(MetadataReference.CreateFromFile("C:\\WINDOWS\\Microsoft.NET\\assembly\\GAC_64\\mscorlib\\v4.0_4.0.0.0__b77a5c561934e089\\mscorlib.dll"));


            var compilation = CSharpCompilation.Create(newAssName,
                syntaxTrees: new[] { syntaxTree }, 
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                allowUnsafe: true,
                optimizationLevel: OptimizationLevel.Debug
                )
                );

        



            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);



                if (!result.Success)
                {
                    
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        Log.log.LogError($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                    }
                }
                else
                {

                    ms.Seek(0, SeekOrigin.Begin);
                    Assembly.Load(ms.ToArray());
                    //newAssembly = AssemblyLoadContext.Default.LoadFromStream(ms);
                }
            }
        }


        public StringBuilder OutputCSharpCode(bool outputToFile = false)
        {
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.BracingStyle = "C";


            var stringBuilder = new StringBuilder();
            var stringWriter = new StringWriter(stringBuilder);

            
            provider.GenerateCodeFromCompileUnit(compileUnit, stringWriter, options);

            if(outputToFile) {
                var dir = $"generatedCode";
                Directory.CreateDirectory(dir);

                using FileStream fileStream = File.Open($"{dir}/{typeof(ED).Name}.cs", FileMode.Create, FileAccess.Write, FileShare.None);

                using StreamWriter sourceWriter = new StreamWriter(fileStream, Encoding.UTF8) { AutoFlush = true };

                sourceWriter.Write(stringBuilder.ToString());
            }

            return stringBuilder;

        }
    }
}

// shit doesn't work

        static FieldInfo _selectedEnemyField = AccessTools.Field(typeof(UnitSelector), nameof(UnitSelector._selectedEnemy));

        [HarmonyPatch]
        class UnitSelector_Patch
        {


            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.DeclaredPropertyGetter(typeof(UnitSelector), nameof(UnitSelector.SelectedEnemy));
            }


            static bool Prefix(ref EnemyUnit __result, UnitSelector __instance)
            {
                if (__instance.Type != TargetType.SingleEnemy && __instance.Type != TargetType.RandomEnemy)
                {
                    throw new InvalidOperationException(string.Format("Cannot get enemy with type '{0}'", __instance.Type));
                }




                if (__instance._selectedEnemy == null && __instance.Type == TargetType.RandomEnemy)
                {


                    var randomEnemy = GameMaster.Instance.CurrentGameRun.Battle.AllAliveEnemies.Sample(GameMaster.Instance.CurrentGameRun.BattleRng);

                    log.LogDebug(randomEnemy);

                    _selectedEnemyField.SetValue(__instance, randomEnemy);
                    
                }


                __result = __instance._selectedEnemy;

                return false;


            }
        }


        [HarmonyPatch]
        class UnitSelector_GetEnemy_Patch
        {


            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(UnitSelector), nameof(UnitSelector.GetEnemy));
                yield return AccessTools.Method(typeof(UnitSelector), nameof(UnitSelector.GetUnits));

            }


            static EnemyUnit Set_selectedEnemy(EnemyUnit randomEnemy, UnitSelector unitSelector)
            {
                if (unitSelector._selectedEnemy == null)
                    _selectedEnemyField.SetValue(unitSelector, randomEnemy);
                return unitSelector._selectedEnemy;
            }

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                int i = 0;
                var ciList = instructions.ToList();
                var c = ciList.Count();
                CodeInstruction prevCi = null;
                foreach (var ci in instructions)
                {
                    if (ci.Is(OpCodes.Callvirt, AccessTools.DeclaredPropertyGetter(typeof(BattleController), nameof(BattleController.RandomAliveEnemy))))
                    {
                        yield return ci;
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(UnitSelector_GetEnemy_Patch), nameof(UnitSelector_GetEnemy_Patch.Set_selectedEnemy)));

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
		
		
/*                return new CodeMatcher(instructions)
			.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "Cannot dequeue consuming mana, resetting all."))
			.Advance(1)
			.Set(OpCodes.Pop, null)
			.End()
			.ThrowIfNotMatchBack("deez", new CodeMatch(OpCodes.Leave))
			.Advance(1)
			.Insert(new CodeInstruction(OpCodes.Nop))
			.InstructionEnumeration();*/


/*using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace LBoLEntitySideloader.Resource
{
    public class SfxResource : IResourceProvider<AudioClip>
    {

        public IResourceSource resourceSource;

        public AudioType audioType = AudioType.OGGVORBIS;

        public Func<string, AudioClip> loadingAction;

        public List<string> clipNames = new List<string>(); 

        public SfxResource(IResourceSource resourceSource, string clipName = null, AudioType? audioType = null, Func<string, AudioClip> loadingAction = null)
        {
            this.resourceSource = resourceSource;
            if(audioType != null)
                this.audioType = audioType.Value;
            if (loadingAction != null)
                this.loadingAction = loadingAction;
            else
                loadingAction = (string name) => DefaultLoadingAction(name);


        }

        private async UniTask<AudioClip> DefaultLoadingAction(string name)
        {
            return await ResourceLoader.LoadAudioClip(name, audioType, (DirectorySource)resourceSource);
        }


        public virtual AudioClip Load()
        {
            
        }

        public virtual Dictionary<string, AudioClip> LoadMany()
        {
            var dic = new Dictionary<string, AudioClip>();
            foreach(var n in clipNames)
            {
                dic.Add(n, loadingAction(n));
            }
            return dic;
        }
    }
}
*/


/*        [HarmonyPatch]
		class funny_Patch
		{


			static IEnumerable<MethodBase> TargetMethods()
			{
				yield return ExtraAccess.InnerMoveNext(typeof(SelectCardPanel), nameof(SelectCardPanel.ViewMiniSelect));
			}


			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
			{
				int i = 0;
				var ciList = instructions.ToList();
				var c = ciList.Count();
				CodeInstruction prevCi = null;
				foreach (var ci in instructions)
				{
					if (ci.Is(OpCodes.Ldc_R4, 0.2f) && prevCi.Is(OpCodes.Ldc_R4, 0f))
					{
						log.LogDebug("injected");
						yield return new CodeInstruction(OpCodes.Ldc_R4, 0f);
					}
					else if (ci.opcode == OpCodes.Leave)
					{
						log.LogDebug("deez");

						yield return ci;
						yield return new CodeInstruction(OpCodes.Nop);
					}
					else
					{
						yield return ci;
					}
					prevCi = ci;
					i++;
				}
			}
		}*/.
		
		
		
using UnityEditor;
using System.IO;

public class BuildAB
{
    [MenuItem("Tools/BuildSuikaAB")]
    public static void build()
    {
        string dir = "Suika";
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        BuildPipeline.BuildAssetBundles(dir, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
    }
}


//(Paste() as GameObject).GetComponent<LBoL.Presentation.Environment>().templates.ToList().ForEach(e => Debug.Log(e.name))



/*            sprites.SetStartPanelStand(ResourceLoader.LoadSpriteAsync("Suika.png", dir));
            sprites.SetDeckStand(ResourceLoader.LoadSpriteAsync("Suika.png", dir));

            sprites.SetDefeatedStand(ResourceLoader.LoadSpriteAsync("DefeatedStand.png", dir));
            sprites.SetWinStand(ResourceLoader.LoadSpriteAsync("Suika.png", dir));

            sprites.SetInRunAvatarPic(() => ResourceLoader.LoadSprite("SuikaAvatar.png", dir));
            sprites.SetCollectionIcon(() => ResourceLoader.LoadSprite("SuikaAvatar.png", dir));
            sprites.SetSelectionCircleIcon(() => ResourceLoader.LoadSprite("SuikaAvatar.png", dir));

            sprites.SetPerfectWinIcon(ResourceLoader.LoadSpriteAsync("SuikaAvatar.png", dir));
            sprites.SetWinIcon(ResourceLoader.LoadSpriteAsync("SuikaAvatar.png", dir));
            sprites.SetDefeatedIcon(ResourceLoader.LoadSpriteAsync("DefeatedIcon.png", dir));

            sprites.SetCardBack(() => ResourceLoader.LoadSprite("SuikaCardBack.png", dir));*/




            /*            sprites.SetStartPanelStand(default, () => suikaAB.LoadAsset<Sprite>("Suika"));
                        sprites.SetDeckStand(default, () => suikaAB.LoadAsset<Sprite>("Suika"));

                        sprites.SetDefeatedStand(default, () => ResourceLoader.LoadSprite("DefeatedStand.png", dir));
                        sprites.SetWinStand(default, () => suikaAB.LoadAsset<Sprite>("Suika"));

                        sprites.SetInRunAvatarPic(() => ResourceLoader.LoadSprite("SuikaAvatar.png", dir));
                        sprites.SetCollectionIcon(() => ResourceLoader.LoadSprite("SuikaAvatar.png", dir));
                        sprites.SetSelectionCircleIcon(() => ResourceLoader.LoadSprite("SuikaAvatar.png", dir));

                        sprites.SetPerfectWinIcon(ResourceLoader.LoadSpriteAsync("SuikaAvatar.png", dir));
                        sprites.SetWinIcon(ResourceLoader.LoadSpriteAsync("SuikaAvatar.png", dir));
                        sprites.SetDefeatedIcon(ResourceLoader.LoadSpriteAsync("DefeatedIcon.png", dir));

                        sprites.SetCardBack(() => ResourceLoader.LoadSprite("SuikaCardBack.png", dir));*/
						
						
						
    public class PiecePanel : GDPanelBase, ICellPoolDataSource<PropCell<PieceReadableConfig>>
    {


        public override string Name => "Piece Builder";

        public override int MinWidth => 450;

        public override int MinHeight => 800;

        public override Vector2 DefaultAnchorMin => new Vector2(0.5f, 1f);

        public override Vector2 DefaultAnchorMax => new Vector2(0.5f, 1f);

        public override UIMaster.Panels PanelType => UIMaster.Panels.Piece;


        public Dictionary<int, PieceReadableConfig> configs = new Dictionary<int, PieceReadableConfig>() { {0, new PieceReadableConfig() } };

        //public PropPool<PieceReadableConfig> piecePropPool = new PropPool<PieceReadableConfig>();


        /*        public class ConfigPoolPair
                {
                    public PieceReadableConfig pieceReadableConfig;
                    public PiecePropPool piecePropPool;
                }*/

        List<PropCell<PieceReadableConfig>> cellData = new List<PropCell<PieceReadableConfig>>();

        public int ItemCount => cellData.Count;


        public void OnCellBorrowed(PropCell<PieceReadableConfig> cell) {}

        public void SetCell(PropCell<PieceReadableConfig> cell, int index)
        {
            if (index < 0 || index >= cellData.Count)
            {
                return;
            }


            cell.target = configs.First().Value;
            cell = cellData[index];


        }



        public PiecePanel(UIBase owner) : base(owner)
        {
            UIMaster.panelManager.OnClickedOutsidePanels += Unfocused;

            cellData.Add(new addParentAngle());
            cellData.Add(new addParentAngle());
            cellData.Add(new addParentAngle());


            /*            piecePropPool.target = configs.First().Value;
                        piecePropPool.cells.Add(new PropCell<PieceReadableConfig>());*/


            //piecePropPool.cells.Add(new addParentAngle());

        }


        public void Unfocused()
        {
            if (this.Enabled)
            {
                Log.log.LogInfo("Outside d3eez");
            
            }
        }

        public override void SetDefaultSizeAndPosition()
        {
            base.SetDefaultSizeAndPosition();
            this.Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, MinWidth);
            this.Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, MinHeight);
        }

        protected override void ConstructPanelContent()
        {
            GameObject firstRow = UIFactory.CreateHorizontalGroup(
                parent: ContentRoot,
                name: "FirstRow",   
                forceExpandWidth: false,
                forceExpandHeight: false,
                childControlWidth: true,
                childControlHeight: true,
                spacing: 5,
                padding: new Color(2, 2, 2, 2),
                bgColor: new Color(1, 1, 1, 0));

            UIFactory.SetLayoutElement(firstRow, minHeight: 25, flexibleWidth: 999);

            Text title = UIFactory.CreateLabel(firstRow, "Deez", "Nuts", TextAnchor.MiddleLeft, color: Color.grey);

            UIFactory.SetLayoutElement(title.gameObject, minHeight: 25, minWidth: 100, flexibleWidth: 999);

            UIFactory.CreateScrollView(ContentRoot, "pieceScroll", out var scrollContent, out var autoSliderScrollbar);





            ScrollPool<PropCell<PieceReadableConfig>> scrollPool = UIFactory.CreateScrollPool<PropCell<PieceReadableConfig>>(
                this.ContentRoot,
                "PieceEntries",
                out GameObject scrollObj,
                out GameObject scrollContent);

            scrollPool.Initialize(this);

        }


    }