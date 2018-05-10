// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Edit.Layers.Selection.Overlays
{
    public class HoldNoteMask : HitObjectMask
    {
        public HoldNoteMask(DrawableHoldNote holdNote)
            : base(holdNote)
        {
        }
    }
}
