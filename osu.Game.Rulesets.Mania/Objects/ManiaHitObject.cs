// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mania.Objects.Types;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mania.Objects
{
    public abstract class ManiaHitObject : HitObject, IHasColumn
    {
        private int column;

        public virtual int Column
        {
            get => column;
            set
            {
                if(column == value)
                    return;
                
                column = value;
                ColumnChanged?.Invoke(column);
            }
        }

        public event Action<int> ColumnChanged;

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            HitWindows.AllowsPerfect = true;
            HitWindows.AllowsOk = true;
        }
    }
}
