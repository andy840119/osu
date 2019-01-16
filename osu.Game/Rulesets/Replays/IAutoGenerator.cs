﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Replays;

namespace osu.Game.Rulesets.Replays
{
    public interface IAutoGenerator
    {
        Replay Generate();
    }
}
