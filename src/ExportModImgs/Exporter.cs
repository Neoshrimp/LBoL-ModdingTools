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

namespace ExportModImgs
{
    public class Exporter : IExPathProvider, IModSubDirProvider
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


        public HashSet<Type> targetEntityTypes = new HashSet<Type>() {
            typeof(Card),
            typeof(StatusEffect),
            typeof(Exhibit),
        };

        public bool addTimeStamp = true;

        public readonly string rootFolder = "ImgExporter";

        public readonly string rootPath = "";


        public IExPathProvider exPathProvider;

        public IModSubDirProvider modSubDir;

        public Exporter(string rootPath = "", string rootFolder = "ImgExporter")
        {
            exPathProvider = this;
            modSubDir = this;
            this.rootPath = rootPath;
            if (string.IsNullOrEmpty(this.rootPath))
                this.rootPath = Path.Join(Application.dataPath, "..");
            if(rootFolder != null)
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
                Log.LogInfo($"Done exporting {guid}!");
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


            var exportRoot = this.rootPath;
            exportRoot = $"{exportRoot}{suffix}";
            exportRoot = Path.Join(exportRoot, Source.LegalizeFileName(modSubDir.ModDir(pluginInfo)));


            // not inited for mods from scripts folder?
            foreach (var ed in userInfo.definitionInstances.Values)
            {
                if (ed == null)
                    continue;
                // sideloader jank
                try
                {
                    if (!targetEntityTypes.Contains(ed.EntityType()))
                        continue;
                }
                catch (InvalidDataException)
                {
                    continue;
                }

                var exportPath = Path.Join(exportRoot, exPathProvider.ExSubDirs(ed));

                Directory.CreateDirectory(exportPath);


                Texture2D tex = null;

                if (ed is CardTemplate ct)
                {
                    tex = ct.LoadCardImages()?.main;
                }
                else if (ed is ExhibitTemplate ext)
                {
                    tex = ext.LoadSprite()?.main?.texture;
                }
                else if (ed is StatusEffectTemplate set)
                {
                    tex = set.LoadSprite()?.texture;
                }

                if (tex == null)
                    continue;
                var texBytes = ImageConversion.EncodeToPNG(tex);
                var path = Path.Join(exportPath, Source.LegalizeFileName(exPathProvider.ExportFilePrefix(ed)) + ".png");
                File.WriteAllBytes(path, texBytes);
            }

            




        }


        public string ExSubDirs(EntityDefinition entityDefinition)
        {
            return entityDefinition.EntityType().Name;
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
