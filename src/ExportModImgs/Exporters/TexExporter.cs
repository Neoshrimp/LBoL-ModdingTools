using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using static ExportModImgs.Exporters.TexExporter;

namespace ExportModImgs.Exporters
{
    public class TexExporter : Exporter<Texture2D>
    {
        public TexExporter() : base()
        {

            rootFolder = Path.Join(rootFolder, "Imgs");
            definitionConsumers = new Dictionary<Type, IExportProvider<Texture2D>>() {
                [typeof(CardTemplate)] = new CardTex(),
                [typeof(ExhibitTemplate)] = new ExTex(),
                [typeof(StatusEffectTemplate)] = new SeTex(),
                [typeof(UltimateSkillTemplate)] = new DefinitionConsumer<Texture2D>(ed => (ed as UltimateSkillTemplate)?.LoadSprite()?.texture)
            };
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

        static void TypeWarning(EntityDefinition ed, string correctType) => Log.LogWarning($"{ed.GetType()} is not {correctType}");

        public class CardTex : IExportProvider<Texture2D>
        {
            public Texture2D Provide(EntityDefinition entityDefinition)
            {
                if (entityDefinition is CardTemplate ct)
                {
                    return ct.LoadCardImages()?.main;
                }
                TypeWarning(entityDefinition, nameof(CardTemplate));
                return null;
            }
        }

        public class ExTex : IExportProvider<Texture2D>
        {
            public Texture2D Provide(EntityDefinition entityDefinition)
            {
                if (entityDefinition is ExhibitTemplate ext)
                {
                    return ext.LoadSprite()?.main?.texture;
                }
                TypeWarning(entityDefinition, nameof(ExhibitTemplate));
                return null;
            }
        }


        public class SeTex : IExportProvider<Texture2D>
        {
            public Texture2D Provide(EntityDefinition entityDefinition)
            {
                if (entityDefinition is StatusEffectTemplate set)
                {
                    return set.LoadSprite()?.texture;
                }
                TypeWarning(entityDefinition, nameof(StatusEffectTemplate));
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


        public class ExportPathForUpgrade : ExportPath
        {
            public override string ExportFilePrefix(EntityDefinition ed)
            {
                if (ed is CardTemplate ct)
                {
                    var upId = ct.MakeConfig().UpgradeImageId;
                    if (string.IsNullOrEmpty(upId))
                        return upId;
                }

                return base.ExportFilePrefix(ed);
            }
        }

    }


}
