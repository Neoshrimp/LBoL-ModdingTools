using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExportModImgs
{
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
