using LBoL.EntityLib.Cards.Character.Sakuya;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ExportModImgs.Exporters
{

    public class TexContainer
    {
        public Dictionary<string, Texture2D> dic = new Dictionary<string, Texture2D>();
    }

    public class TexExporter : Exporter<TexContainer>
    {



        public TexExporter() : base()
        {

            rootFolder = Path.Join(rootFolder, "Imgs");
            definitionConsumers = new Dictionary<Type, IExportProvider<TexContainer>>() {
                [typeof(CardTemplate)] = new CardTex(),
                [typeof(ExhibitTemplate)] = new ExTex(),
                [typeof(StatusEffectTemplate)] = new SeTex(),
                [typeof(UltimateSkillTemplate)] = new DefinitionConsumer<TexContainer>(ed => {
                    var tex = (ed as UltimateSkillTemplate)?.LoadSprite()?.texture;
                    var texContainer = new TexContainer();
                    texContainer.dic.Add("", tex);
                    return texContainer;
                }),
                [typeof(PlayerUnitTemplate)] = new PUTex()
            };
            postProcess = new TexExport();
        }

        public class TexExport : IPostConsume<TexContainer>
        {
            public void Process(TexContainer input, string path)
            {

                foreach (var kv in input.dic)
                {
                    if (kv.Value == null)
                        continue;
                    var texBytes = ImageConversion.EncodeToPNG(kv.Value);
                    if(texBytes != null && texBytes.Length > 0)
                        File.WriteAllBytes(path + kv.Key + ".png", texBytes);
                
                }
            }
        }

        static void TypeWarning(EntityDefinition ed, string correctType) => Log.LogWarning($"{ed.GetType()} is not {correctType}");

        public class CardTex : IExportProvider<TexContainer>
        {
            public TexContainer Provide(EntityDefinition entityDefinition)
            {
                if (entityDefinition is CardTemplate ct)
                {
                    var texContainer = new TexContainer();
                    var imgs = ct.LoadCardImages();
                    texContainer.dic.Add("", imgs?.main);

                    var upId = ct.MakeConfig().UpgradeImageId;
                    texContainer.dic.Add("_" + upId, imgs.upgrade);

                    return texContainer;
                }
                TypeWarning(entityDefinition, nameof(CardTemplate));
                return null;
            }
        }

        public class ExTex : IExportProvider<TexContainer>
        {
            public TexContainer Provide(EntityDefinition entityDefinition)
            {
                if (entityDefinition is ExhibitTemplate ext)
                {
                    var texContainer = new TexContainer();
                    texContainer.dic.Add("", ext.LoadSprite()?.main?.texture);
                    return texContainer;
                }
                TypeWarning(entityDefinition, nameof(ExhibitTemplate));
                return null;
            }
        }


        public class SeTex : IExportProvider<TexContainer>
        {
            public TexContainer Provide(EntityDefinition entityDefinition)
            {
                if (entityDefinition is StatusEffectTemplate set)
                {
                    var texContainer = new TexContainer();
                    texContainer.dic.Add("", set.LoadSprite()?.texture);
                    return texContainer;
                }
                TypeWarning(entityDefinition, nameof(StatusEffectTemplate));
                return null;
            }
        }

        private class PUTex : IExportProvider<TexContainer>
        {
            public TexContainer Provide(EntityDefinition ed)
            {
                if (ed is PlayerUnitTemplate pu)
                {
                    var texContainer = new TexContainer();
                    var imgs = pu.LoadPlayerImages();
                    
                    texContainer.dic.Add("_CardBack", imgs.LoadCardBack()?.texture);
                    texContainer.dic.Add("_SelectionCircleIcon", imgs.LoadSelectionCircleIcon()?.texture);
                    texContainer.dic.Add("_DefeatedIcon", imgs.LoadDefeatedIcon()?.texture);
                    texContainer.dic.Add("_WinIcon", imgs.LoadWinIcon()?.texture);
                    texContainer.dic.Add("_PerfectWinIcon", imgs.LoadPerfectWinIcon()?.texture);


                    return texContainer;
                }
                TypeWarning(ed, nameof(PlayerUnitTemplate));
                return null;
            }
        }
    }



    public class UpgradedImgExporter : Exporter<Texture2D>
    {

        public UpgradedImgExporter() : base()
        {

            rootFolder = Path.Join(rootFolder, "UpgradeImgs");
            definitionConsumers = new Dictionary<Type, IExportProvider<Texture2D>>()
            {
                [typeof(CardTemplate)] = new DefinitionConsumer<Texture2D>(ed => {
                    if (ed is CardTemplate ct)
                    {
                        return ct.LoadCardImages()?.upgrade;
                    }
                    return null;
                })
            };

            exPathProvider = new ExportPathForUpgrade();

            postProcess = new TexExport();
        }


        public class TexExport : IPostConsume<Texture2D>
        {
            public void Process(Texture2D input, string path)
            {
                var texBytes = ImageConversion.EncodeToPNG(input);
                File.WriteAllBytes(path + ".png", texBytes);
            }
        }

        public class ExportPathForUpgrade : ExportPath
        {
            public override string ExportFilePrefix(EntityDefinition ed)
            {
                if (ed is CardTemplate ct)
                {
                    var upId = ct.MakeConfig().UpgradeImageId;
                    if (!string.IsNullOrEmpty(upId))
                        return upId;
                }

                return base.ExportFilePrefix(ed);
            }
        }

    }


}
