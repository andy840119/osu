using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Edit.Layers.Overlays
{
    public class HoldNoteMask : HitObjectMask
    {
        public HoldNoteMask(DrawableHitObject hitObject)
            : base(hitObject)
        {
        }
    }
}
