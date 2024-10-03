using BepInEx;
using Cysharp.Threading.Tasks;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Presentation;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ExportModImgs.Exporters
{
    public class Exporter<T> where T : class
    {
        // add guid here
        public List<string> targetGUIDs = new List<string>() {
            "zosit.lbol.test.utsuho",
            "rmrfmaxx.lbol.PatchouliCharacterMod",
            "rmrfmaxxc.lbol.YuyukoCharacterMod",
            "intoxicatedkid.ayashameimaru",
            "rmrfmaxx.lbol.YoumuCharacterMod",
            "llbol.ea.mima",
            "xeno.lbol.character.Sanae_Kochiya",
            "aqing0601.PKaguya.trial"
        };

        /// <summary>
        /// template type => IDefinitionConsumer
        /// </summary>
        public Dictionary<Type, IDefinitionConsumer<T>> definitionConsumers = null;
        public bool addTimeStamp = true;

        public readonly string rootFolder = "ImgExporter";

        public readonly string rootPath = "";


        public IExPathProvider exPathProvider;

        public IModSubDirProvider modSubDir;

        public IPostConsume<T> postProcess = new EmptyPostConsume<T>();

        public Exporter(string rootPath = "", string rootFolder = "ImgExporter")
        {
            var exPathImpl = new ExPathImpl();
            exPathProvider = exPathImpl;
            modSubDir = exPathImpl;
            this.rootPath = rootPath;
            if (string.IsNullOrEmpty(this.rootPath))
                this.rootPath = Path.Join(Application.dataPath, "..");
            if (rootFolder != null)
                this.rootFolder = rootFolder;

            this.rootPath = Path.Join(rootPath, rootFolder);
        }


        public void HookSelf()
        {
            EntityManager.AddPostLoadAction(() => ExportAll());
        }


        public void ExportAll()
        {

            string suffix = addTimeStamp ? $"_{DateTime.Now:yyyy-MM-dd_HH.mm.ss}" : "";
            foreach (var guid in targetGUIDs)
            {
                Log.LogInfo($"Exporting {guid}...");
                try
                {
                    Export(guid, suffix);
                }
                catch (Exception ex)
                {
                    Log.LogError(ex);
                }
                Log.LogDebug($"Done exporting {guid}!");
            }
        }

        public void Export(string guid, string suffix = "")
        {
            if (!BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue(guid, out var pluginInfo))
            {
                Log.LogWarning($"Mod with {guid} was not loaded.");
                return;
            }

            var modAss = pluginInfo.Instance.GetType().Assembly;

            if (!EntityManager.Instance.sideloaderUsers.userInfos.TryGetValue(modAss, out var userInfo))
            {
                Log.LogWarning($"Mod with {guid} was not registered with Sideloader.");
                return;
            }

            if (userInfo?.definitionInstances == null)
            {
                Log.LogWarning($"Mod with {guid} was probably hot loaded. Aborting.");
                return;
            }


            var exportRoot = rootPath;
            exportRoot = $"{exportRoot}{suffix}";
            exportRoot = Path.Join(exportRoot, Source.LegalizeFileName(modSubDir.ModDir(pluginInfo)));


            foreach (var ed in userInfo.definitionInstances.Values)
            {
                //potential sideloader jank?
                if (ed == null)
                    continue;

                var templateType = ed.TemplateType();

                if (!definitionConsumers.TryGetValue(templateType, out var definitionConsumer))
                    continue;


                var exportPath = Path.Join(exportRoot, exPathProvider.ExSubDirs(ed));

                Directory.CreateDirectory(exportPath);


                T exTarget = definitionConsumer.Consume(ed);


                if (exTarget == null)
                    continue;

                var path = Path.Join(exportPath, Source.LegalizeFileName(exPathProvider.ExportFilePrefix(ed)));

                postProcess.Process(exTarget, path);

            }

        }



    }


    public class ExPathImpl : IExPathProvider, IModSubDirProvider
    {
        public string ExSubDirs(EntityDefinition entityDefinition)
        {
            var rez = entityDefinition.TemplateType().Name;
            // suboptmal sideloader api, not all definition have entity types
            try
            {
                rez = entityDefinition.EntityType().Name;
            }
            catch (InvalidDataException)
            { }
            return rez;

        }

        public string ExportFilePrefix(EntityDefinition entityDefinition)
        {
            return entityDefinition.GetId().ToString();
        }

        public string ModDir(BepInEx.PluginInfo pluginInfo)
        {

            return pluginInfo.Metadata.GUID;
        }
    }

}
