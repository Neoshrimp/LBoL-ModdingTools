using HarmonyLib;
using LBoL.Core;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using YamlDotNet.Helpers;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using static ExportModImgs.Exporters.ConfigExporter;
using static ExportModImgs.Exporters.LocExporter;

namespace ExportModImgs.Exporters
{
    public class LocExporter : Exporter<LocalizationOption>
    {

        public LocExporter() : base()
        {
            rootFolder = Path.Join(rootFolder, "Loc");
            var locPEx = new LocProvideAndExport();
            definitionConsumers = new Dictionary<Type, IExportProvider<LocalizationOption>>()
            {
                [typeof(CardTemplate)] = locPEx,
                [typeof(ExhibitTemplate)] = locPEx,
                [typeof(StatusEffectTemplate)] = locPEx,
                [typeof(UltimateSkillTemplate)] = locPEx,
                [typeof(PlayerUnitTemplate)] = locPEx,
                [typeof(EnemyUnitTemplate)] = locPEx,
            };
            postProcess = locPEx;
        }


        public class LocProvideAndExport : IExportProvider<LocalizationOption>, IPostConsume<LocalizationOption>
        {
            string id = "";

            public LocalizationOption Provide(EntityDefinition ed)
            {
                id = ed.UniqueId;
                if (ed is CardTemplate ct) return ct.LoadLocalization();
                if (ed is ExhibitTemplate et) return et.LoadLocalization();
                if (ed is StatusEffectTemplate st) return st.LoadLocalization();
                if (ed is UltimateSkillTemplate ut) return ut.LoadLocalization();
                if (ed is PlayerUnitTemplate pt) return pt.LoadLocalization();
                if (ed is EnemyUnitTemplate eut) return eut.LoadLocalization();

                return null;
            }

            static ISerializer serializer;

            public static ISerializer Serializer
            {
                get
                {
                    if (serializer == null)
                    {
                        serializer = new SerializerBuilder()
                            .DisableAliases()
                            .Build();
                    }
                    return serializer;
                }
            }

            // from sideloader
            Dictionary<string, Dictionary<string, object>> LoadLocTable(IEnumerable<string> Ids, YamlMappingNode yaml, bool addEmptyDic = true)
            {
                Dictionary<string, Dictionary<string, object>> dictionary = new Dictionary<string, Dictionary<string, object>>();


                if (yaml == null)
                {
                    Log.LogWarning($"{nameof(LocalizationFiles)}: No localization found.");
                }

                IOrderedDictionary<YamlNode, YamlNode> children = yaml.Children;
                foreach (var id in Ids)
                {
                    if (children.TryGetValue(id, out var yamlNode))
                    {
                        YamlMappingNode yamlMappingNode = yamlNode as YamlMappingNode;
                        if (yamlMappingNode != null)
                        {
                            dictionary.Add(id, Localization.CreatePropertyLocalizeTable(id, yamlMappingNode));
                            continue;
                        }
                    }
                    if (addEmptyDic)
                        dictionary.Add(id, new Dictionary<string, object>());
                }


                return dictionary;
            }

            public void Process(LocalizationOption locOption, string path)
            {


                var allLocFiles = new Dictionary<Locale, object>();

                if (locOption is GlobalLocalization globalLoc)
                {
                    //Log.LogWarning("GlobalLoc lmao");
                    foreach (var kv in globalLoc.LocalizationFiles.locTable)
                    {
                        var loc = kv.Key;
                        var termDic = LoadLocTable(new string[] { id }, kv.Value());

                        allLocFiles.Add(loc, termDic);
                    }

                }
                else if (locOption is LocalizationFiles locFiles)
                {
                    locFiles.locTable.Do(kv => allLocFiles.Add(kv.Key, kv.Value()));
                }

                else if (locOption is DirectLocalization rawLoc)
                {
                    rawLoc.termDic.Do(kv => allLocFiles.Add(kv.Key, kv.Value));
                }

                else if (locOption is BatchLocalization batchLocalization)
                {

                    foreach (var kv in batchLocalization.localizationFiles.locTable)
                    {
                        var loc = kv.Key;
                        var termDic = LoadLocTable(new string[] { id }, kv.Value());

                        allLocFiles.Add(loc, termDic);
                    }
                }

                foreach (var locale in allLocFiles.Keys)
                {
                    using (FileStream fileStream = File.Open($"{path}{locale}.yaml", FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8) { })
                        {

                            Serializer.Serialize(streamWriter, allLocFiles[locale]);
                            streamWriter.Flush();
                        }
                    }


                }
            }
        }

    }
}

