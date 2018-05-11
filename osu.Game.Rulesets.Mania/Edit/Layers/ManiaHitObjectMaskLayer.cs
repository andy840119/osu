using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Screens.Compose.Layers;

namespace osu.Game.Rulesets.Mania.Edit.Layers
{
    public class ManiaHitObjectMaskLayer : HitObjectMaskLayer
    {
        private readonly ManiaEditPlayfield playfield;
        private readonly HitObjectComposer composer;

        public ManiaHitObjectMaskLayer(ManiaEditPlayfield playfield, HitObjectComposer composer)
            : base(playfield, composer)
        {
            this.playfield = playfield;
            this.composer = composer;
        }

        public override void AddMask(DrawableHitObject hitObject)
        {
            var mask = composer.CreateMaskFor(hitObject);
            if (mask == null)
                return;

            playfield.AddMask(mask);
        }
    }
}
