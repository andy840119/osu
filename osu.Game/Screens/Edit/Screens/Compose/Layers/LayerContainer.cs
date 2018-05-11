using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.UI;

namespace osu.Game.Screens.Edit.Screens.Compose.Layers
{
    public class LayerContainer : ScalableContainer
    {
        protected readonly HitObjectComposer Composer;
        public LayerContainer(HitObjectComposer composer, float? customWidth = null, float? customHeight = null)
            : base(customWidth, customHeight)
        {
            Composer = composer;

            CreateLayer();
        }

        public virtual void CreateLayer()
        {
            Child = new HitObjectMaskLayer(Composer.RulesetContainer.Playfield, Composer);
        }
    }
}
