using System;
using System.Collections.Generic;
using System.Text;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Edit.Layers.Overlays
{
    public class NoteMask : HitObjectMask
    {
        protected readonly GlowPiece GlowPiece;

        private readonly LaneGlowPiece laneGlowPiece;
        private readonly NotePiece headPiece;

        public NoteMask(DrawableNote note)
            : base(note)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                laneGlowPiece = new LaneGlowPiece
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                },
                GlowPiece = new GlowPiece(),
                headPiece = new NotePiece
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre
                }
            };
        }
    }
}
