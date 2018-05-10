using System;
using System.Collections.Generic;
using System.Text;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI;
using OpenTK;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class ManiaEditRulesetContainer : ManiaRulesetContainer
    {
        public ManiaEditRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap, bool isForCurrentRuleset)
            : base(ruleset, beatmap, isForCurrentRuleset)
        {
        }

        protected  override Playfield CreatePlayfield() => new ManiaPlayfield(Beatmap.Stages)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
        };

        protected override Vector2 PlayfieldArea => Vector2.One;

        protected override CursorContainer CreateCursor() => null;
    }
}
