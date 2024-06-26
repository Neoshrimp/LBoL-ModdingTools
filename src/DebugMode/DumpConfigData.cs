using LBoL.ConfigData;
using LBoL.Core;
using LBoLEntitySideloader.ReflectionHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet;
using YamlDotNet.Serialization;
using HarmonyLib;
using DebugMode.SerializationConverters;
using Newtonsoft.Json;

namespace DebugMode
{

    public class DumpConfigData
    {
        public string dir = "";

        private Dictionary<string, Dictionary<string, object>> GetDump(Type configType)
        {
            var toDump = new Dictionary<string, Dictionary<string, object>>();
            var allConfig = (Array)ConfigReflection.GetArrayField(configType).GetValue(null);

            foreach (var conf in allConfig)
            {
                var id = (string)ConfigReflection.GetIdField(conf.GetType()).GetValue(conf);
                var localizedName = "";
                // finds localized name associated with config if it has one
                if (ConfigReflection.GetConfig2FactoryType().TryGetValue(configType, out Type factype))
                {
                    if (TypeFactoryReflection.AccessTypeLocalizers(factype)().TryGetValue((string)ConfigReflection.GetIdField(configType)?.GetValue(conf), out Dictionary<string, object> terms) && terms.TryGetValue("Name", out object name))
                    {
                        localizedName = name?.ToString();
                    }
                }

                toDump.TryAdd(id, new Dictionary<string, object>());
                toDump[id].Add("localizedName", localizedName);
                toDump[id].Add("config", conf);

            }
            return toDump;
        }

        public void DumpYaml()
        {
            Plugin.log.LogInfo("Dumping config yaml");

            Action<Type> writeConfig = (configType) =>
            {

                var sb = new SerializerBuilder()
                    .DisableAliases()
                    .WithTypeConverter(new YamlToStringConverter())
                    .Build();


                var toDump = GetDump(configType);


                dir = $"configYaml_{VersionInfo.Current.Version}";
                Directory.CreateDirectory(dir);

                using (FileStream fileStream = File.Open($"{dir}/{configType.Name}.yaml", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8) { })
                    {

                        sb.Serialize(streamWriter, toDump);
                        streamWriter.Flush();
                    }
                }
            };

            DoDump(writeConfig);
        }

        public void DumpJson()
        {
            Plugin.log.LogInfo("Dumping config json");

            Action<Type> writeConfig = (configType) =>
            {

                var settings = new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter> { new JsonToStringConverter() },
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore

                };

                var serializer = JsonSerializer.Create(settings);

                var toDump = GetDump(configType);

                dir = $"configJson_{VersionInfo.Current.Version}";
                Directory.CreateDirectory(dir);

                using (FileStream fileStream = File.Open($"{dir}/{configType.Name}.json", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8) { })
                    {
                        serializer.Serialize(streamWriter, toDump);
                        streamWriter.Flush();
                    }
                }
            };

            DoDump(writeConfig);
        }


        private void DoDump(Action<Type> writeConfig)
        {


            foreach (var ct in ConfigReflection.AllConfigTypes())
            {
                try
                {
                    writeConfig(ct);
                }
                catch (Exception ex)
                {
                    Plugin.log.LogWarning($"Problems with config: {ct.Name}");

                    using (FileStream fileStream = File.Open($"{dir}/{ct.Name}Error.txt", FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8) { })
                        {
                            streamWriter.Write(ex.Message);
                            streamWriter.Flush();
                        }
                    }
                }
            }
        }

    }
}
