using System;
using System.Collections.Generic;
using System.Text;

namespace AddWatermark
{
    public class API
    {
        public const string GUID = "neo.lbol.tools.watermark";

        static public void ActivateWatermark()
        {
            Plugin.activateWatermark = true;

            if (Plugin.watermarkRef != null)
            {
                Plugin.watermarkRef.SetActive(true);
            }

        }
    }


}
