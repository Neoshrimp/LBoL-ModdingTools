using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExportModImgs.Exporters
{

    public interface IDefinitionConsumer<out T> where T : class
    {
        public T Consume(EntityDefinition entityDefinition);
    }


    public interface IPostConsume<in T> where T : class
    {
        public void Process(T input, string path);
    }

    public class EmptyPostConsume<T> : IPostConsume<T> where T : class
    {
        public void Process(T input, string path)
        {
            return;
        }
    }

    public interface IExPathProvider
    {
        public string ExSubDirs(EntityDefinition entityDefinition);

        public string ExportFilePrefix(EntityDefinition entityDefinition);

    }

    public interface IModSubDirProvider
    {
        public string ModDir(BepInEx.PluginInfo pluginInfo);
    }

}
