using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania.Edit.Layers
{
    internal class ManiaMaskStage : ManiaStage
    {
        public ManiaMaskStage(int firstColumnIndex, StageDefinition definition, ref ManiaAction normalColumnStartAction, ref ManiaAction specialColumnStartAction)
            : base(firstColumnIndex, definition, ref normalColumnStartAction, ref specialColumnStartAction)
        {

        }

        public virtual void AddMask()
        {

        }

        public virtual void RemoveMask()
        {

        }
    }
}
