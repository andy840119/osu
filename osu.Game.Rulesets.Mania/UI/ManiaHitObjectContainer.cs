using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.UI
{
    public class ManiaHitObjectContainer : ScrollingHitObjectContainer
    {
        private readonly List<DrawableHitObject> objects = new List<DrawableHitObject>();
        public override IEnumerable<DrawableHitObject> Objects => objects;

        public ManiaHitObjectContainer(ScrollingDirection direction)
            : base(direction)
        {
        }

        public override void Add(DrawableHitObject hitObject) => objects.Add(hitObject);

        public override bool Remove(DrawableHitObject hitObject) => objects.Remove(hitObject);
    }
}
