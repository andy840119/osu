using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Screens.Compose.Layers;

namespace osu.Game.Rulesets.Mania.Edit.Layers
{
    public class ManiaLayerContainer : LayerContainer
    {
        public ManiaLayerContainer(HitObjectComposer composer,float? customWidth = null, float? customHeight = null)
            : base(composer,customWidth, customHeight)
        {

        }

        public override void CreateLayer()
        {
            Child = new ManiaHitObjectMaskLayer(Composer.RulesetContainer.Playfield as ManiaEditPlayfield, Composer);
        }
    }
}
