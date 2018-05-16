// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.UI
{
    public class ManiaPlayfield : ScrollingPlayfield
    {
        /// <summary>
        /// Whether this playfield should be inverted. This flips everything inside the playfield.
        /// </summary>
        public readonly Bindable<bool> Inverted = new Bindable<bool>(true);

        public List<Column> Columns => stages.SelectMany(x => x.Columns).ToList();
        private readonly List<ManiaStage> stages = new List<ManiaStage>();

        public ManiaPlayfield(List<StageDefinition> stageDefinitions)
            : base(ScrollingDirection.Up)
        {
            if (stageDefinitions == null)
                throw new ArgumentNullException(nameof(stageDefinitions));

            if (stageDefinitions.Count <= 0)
                throw new ArgumentException("Can't have zero or fewer stages.");

            Inverted.Value = true;

            GridContainer playfieldGrid;
            InternalChild = playfieldGrid = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = new[] { new Drawable[stageDefinitions.Count] }
            };

            var normalColumnAction = ManiaAction.Key1;
            var specialColumnAction = ManiaAction.Special1;
            int firstColumnIndex = 0;
            for (int i = 0; i < stageDefinitions.Count; i++)
            {
                var newStage = new ManiaStage(firstColumnIndex, stageDefinitions[i], ref normalColumnAction, ref specialColumnAction);
                newStage.VisibleTimeRange.BindTo(VisibleTimeRange);
                newStage.Inverted.BindTo(Inverted);

                playfieldGrid.Content[0][i] = newStage;

                stages.Add(newStage);
                AddNested(newStage);

                firstColumnIndex += newStage.Columns.Count;
            }
        }

        public void AddMask(HitObjectMask mask)
        {
            getStageByColumn(((ManiaHitObject)mask.HitObject.HitObject).Column).Add(mask);
        }

        public void RemoveMask(HitObjectMask mask)
        {
            getStageByColumn(((ManiaHitObject)mask.HitObject.HitObject).Column).Remove(mask);
        }

        public override void Add(DrawableHitObject h)
        {
            getStageByColumn(((ManiaHitObject)h.HitObject).Column).Add(h);
            base.Add(h);
        }

        public override void Remove(DrawableHitObject h)
        {
            getStageByColumn(((ManiaHitObject)h.HitObject).Column).Remove(h);
            base.Remove(h);
        }

        public void Add(BarLine barline) => stages.ForEach(s => s.Add(barline));

        private ManiaStage getStageByColumn(int column)
        {
            int sum = 0;
            foreach (var stage in stages)
            {
                sum = sum + stage.Columns.Count;
                if (sum > column)
                    return stage;
            }

            return null;
        }

        [BackgroundDependencyLoader]
        private void load(ManiaConfigManager maniaConfig)
        {
            maniaConfig.BindWith(ManiaSetting.ScrollTime, VisibleTimeRange);
        }

        internal void OnJudgement(DrawableHitObject judgedObject, Judgement judgement)
        {
            getStageByColumn(((ManiaHitObject)judgedObject.HitObject).Column).OnJudgement(judgedObject, judgement);
        }

        protected override HitObjectContainer CreateHitObjectContainer() => new ManiaHitObjectContainer(ScrollingDirection.Up);
    }
}
