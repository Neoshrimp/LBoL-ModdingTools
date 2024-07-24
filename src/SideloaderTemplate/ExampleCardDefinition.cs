using LBoL.ConfigData;
using LBoL.Core.Cards;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System;
using System.Collections.Generic;
using System.Text;
using static SideloaderTemplate.BepinexPlugin;

namespace SideloaderTemplate
{
    public sealed class ExampleCardDefinition : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(ExampleCard);
        }

        public override CardImages LoadCardImages()
        {
            return null;
        }

        public override LocalizationOption LoadLocalization()
        {
            return cardBatchLoc.AddEntity(this);
        }

        public override CardConfig MakeConfig()
        {
            return this.DefaultConfig();
        }

        [EntityLogic(typeof(ExampleCardDefinition))]
        public sealed class ExampleCard : Card
        {


        }

    }
}
