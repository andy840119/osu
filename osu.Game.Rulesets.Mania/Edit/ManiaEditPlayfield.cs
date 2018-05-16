using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class ManiaEditPlayfield : ManiaPlayfield
    {
        public ManiaEditPlayfield(List<StageDefinition> stageDefinitions)
            : base(stageDefinitions)
        {

        }

        /*
        public void AddMask(HitObjectMask mask)
        {
            //TODO :  here to add HitObjectMask into scrollPlayField 
            //Add(mask);
        }
        */

        public override void Add(DrawableHitObject h)
        {
            base.Add(h);

            //TODO : add event if namia need to change column
            if (h is DrawableHitObject<ManiaHitObject> drawableManiaHitObject)
            {
                drawableManiaHitObject.HitObject.ColumnChanged += (a) => { MoveColumn(drawableManiaHitObject); };
            }
        }

        public void MoveColumn(DrawableHitObject h)
        {
            base.Remove(h);
            base.Add(h);
        }
    }
}
