using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace ExportModImgs.Exporters
{
    public class TexExporter : Exporter<Texture2D>
    {
        public TexExporter() : base()
        {
            definitionConsumers = new Dictionary<Type, IDefinitionConsumer<Texture2D>>() {
                [typeof(CardTemplate)] = new CardTex(),
                [typeof(ExhibitTemplate)] = new ExTex(),
                [typeof(StatusEffectTemplate)] = new SeTex(),
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

        public class CardTex : IDefinitionConsumer<Texture2D>
        {
            public Texture2D Consume(EntityDefinition entityDefinition)
            {
                if (entityDefinition is CardTemplate ct)
                {
                    return ct.LoadCardImages()?.main;
                }
                TypeWarning(entityDefinition, nameof(CardTemplate));
                return null;
            }
        }

        public class ExTex : IDefinitionConsumer<Texture2D>
        {
            public Texture2D Consume(EntityDefinition entityDefinition)
            {
                if (entityDefinition is ExhibitTemplate ext)
                {
                    return ext.LoadSprite()?.main?.texture;
                }
                TypeWarning(entityDefinition, nameof(ExhibitTemplate));
                return null;
            }
        }


        public class SeTex : IDefinitionConsumer<Texture2D>
        {
            public Texture2D Consume(EntityDefinition entityDefinition)
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






}
