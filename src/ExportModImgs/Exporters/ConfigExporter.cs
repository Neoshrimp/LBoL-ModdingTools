using LBoLEntitySideloader.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static ExportModImgs.Exporters.TexExporter;
using UnityEngine;
using LBoLEntitySideloader.ReflectionHelpers;
using System.IO.Pipes;
using System.Text.RegularExpressions;
using HarmonyLib;
using System.Reflection;
using System.Linq;

namespace ExportModImgs.Exporters
{
    public class ConfigExporter : Exporter<object>
    {
        public ConfigExporter() : base()
        {
            rootFolder = Path.Join(rootFolder, "Configs");
            var configProvider = new ConfigProvider();
            definitionConsumers = new Dictionary<Type, IExportProvider<object>>()
            {
                [typeof(CardTemplate)] = configProvider,
                [typeof(ExhibitTemplate)] = configProvider,
                [typeof(StatusEffectTemplate)] = configProvider,
                [typeof(UltimateSkillTemplate)] = configProvider,
                [typeof(PlayerUnitTemplate)] = configProvider,
                [typeof(EnemyUnitTemplate)] = configProvider,
            };
            postProcess = new ConfigExport();
        }

        public class ConfigProvider : IExportProvider<object>
        {



            static Dictionary<Type, MethodInfo> miCache = new Dictionary<Type, MethodInfo>();

            public object Provide(EntityDefinition ed)
            {
                var it = ed.GetType().GetInterfaces().FirstOrDefault(i => i.Name.StartsWith("IConfigProvider"));
                if (it != null)
                {
                    if (!miCache.TryGetValue(it, out var mi))
                    { 
                        mi = it.GetMethod("MakeConfig", AccessTools.allDeclared);
                        miCache[it] = mi;
                    }
                    return mi.Invoke(ed, new object[] { });
                }
                return null;
            }
        }



        public class ConfigExport : IPostConsume<object>
        {
            public void Process(object conf, string path)
            {
                using (FileStream fileStream = File.Open($"{path}.txt", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8) { AutoFlush = true })
                    {
                        var configType = conf.GetType();
                        var localizedName = "";
                        // finds localized name associated with config if it has one
                        // doesn't work here. timing ig
                        if (ConfigReflection.GetConfig2FactoryType().TryGetValue(configType, out Type factype))
                        {
                            if (TypeFactoryReflection.AccessTypeLocalizers(factype)()
                            .TryGetValue((string)ConfigReflection.GetIdField(configType)?.GetValue(conf), out Dictionary<string, object> terms)
                            && terms.TryGetValue("Name", out object name))
                            {
                                localizedName = name?.ToString();
                            }

                        }

                        string wrappedText = "";
                        try
                        {
                            wrappedText = Regex.Replace(conf.ToString(), "(.{1," + 100 + @"})(\s+|$)", "$1" + System.Environment.NewLine);
                        }
                        catch (Exception ex)
                        {
                            Log.LogError($"{ConfigReflection.GetIdField(configType).GetValue(conf)}: \n{ex}");
                        }


                        if (!string.IsNullOrEmpty(localizedName))
                            streamWriter.WriteLine($"{localizedName}: ");
                        streamWriter.Write(wrappedText);
                        //streamWriter.WriteLine("------------------------");
                    }
                }
            }
        }
    }

}
