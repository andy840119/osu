
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Tests.Visual;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public class TestCaseKaraokeNote : OsuTestCase
    {
        public TestCaseKaraokeNote()
        {
            HoldNote holdNote = new HoldNote { StartTime = 1000,Duration = 100000 };
            holdNote.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            Add(new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new Container
                    {
                        Name = "Hold note column",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.X,
                        Height = 50,
                        Children = new[]
                        {
                            new DrawableKaraokeNote(holdNote, ManiaAction.Key1)
                            {
                                X = 100,
                                Width = 100,
                                LifetimeStart = double.MinValue,
                                LifetimeEnd = double.MaxValue,
                                AccentColour = Color4.Red,
                            }
                        }
                    }
                }
            });
        }
    }

    public abstract class DrawableBaseNote<TObject> : DrawableHitObject<ManiaHitObject>
        where TObject : ManiaHitObject
    {
        /// <summary>
        /// The key that will trigger input for this hit object.
        /// </summary>
        protected ManiaAction Action { get; }

        public new TObject HitObject;

        protected DrawableBaseNote(TObject hitObject, ManiaAction? action = null)
            : base(hitObject)
        {
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;

            HitObject = hitObject;

            if (action != null)
                Action = action.Value;
        }
    }

    /// <summary>
    /// Visualises a <see cref="HoldNote"/> hit object.
    /// </summary>
    public class DrawableKaraokeNote : DrawableBaseNote<HoldNote>, IKeyBindingHandler<ManiaAction>
    {
        private readonly DrawableNote head;
        private readonly DrawableNote tail;

        private readonly GlowPiece glowPiece;
        private readonly BodyPiece bodyPiece;
        private readonly Container fullHeightContainer;

        /// <summary>
        /// Time at which the user started holding this hold note. Null if the user is not holding this hold note.
        /// </summary>
        private double? holdStartTime;

        /// <summary>
        /// Whether the hold note has been released too early and shouldn't give full score for the release.
        /// </summary>
        private bool hasBroken;

        public DrawableKaraokeNote(HoldNote hitObject, ManiaAction action)
            : base(hitObject, action)
        {
            Container<DrawableHoldNoteTick> tickContainer;
            RelativeSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                // The hit object itself cannot be used for various elements because the tail overshoots it
                // So a specialized container that is updated to contain the tail height is used
                fullHeightContainer = new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    Child = glowPiece = new GlowPiece()
                },
                bodyPiece = new BodyPiece
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Y,
                },
                tickContainer = new Container<DrawableHoldNoteTick>
                {
                    RelativeSizeAxes = Axes.Both,
                    ChildrenEnumerable = HitObject.NestedHitObjects.OfType<HoldNoteTick>().Select(tick => new DrawableHoldNoteTick(tick)
                    {
                        HoldStartTime = () => holdStartTime
                    })
                },
                head = new DrawableHeadNote(this, action)
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft
                },
                tail = new DrawableTailNote(this, action)
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft
                },
                new TextFlowContainer()
                {
                    Text = "Hello",
                }
            };

            foreach (var tick in tickContainer)
                AddNested(tick);

            AddNested(head);
            AddNested(tail);
        }

        public override Color4 AccentColour
        {
            get { return base.AccentColour; }
            set
            {
                base.AccentColour = value;

                glowPiece.AccentColour = value;
                bodyPiece.AccentColour = value;
                head.AccentColour = value;
                tail.AccentColour = value;
            }
        }

        protected override void UpdateState(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Hit:
                    // Good enough for now, we just want them to have a lifetime end
                    this.Delay(2000).Expire();
                    break;
            }
        }

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (tail.AllJudged)
                AddJudgement(new HoldNoteJudgement { Result = HitResult.Perfect });
        }

        protected override void Update()
        {
            base.Update();

            // Make the body piece not lie under the head note
            bodyPiece.X = head.Width;
            bodyPiece.Width = DrawWidth - head.Width;

            // Make the fullHeightContainer "contain" the height of the tail note, keeping in mind
            // that the tail note overshoots the height of this hit object
            fullHeightContainer.Width = DrawWidth + tail.Width;
        }

        public bool OnPressed(ManiaAction action)
        {
            // Make sure the action happened within the body of the hold note
            if (Time.Current < HitObject.StartTime || Time.Current > HitObject.EndTime)
                return false;

            if (action != Action)
                return false;

            // The user has pressed during the body of the hold note, after the head note and its hit windows have passed
            // and within the limited range of the above if-statement. This state will be managed by the head note if the
            // user has pressed during the hit windows of the head note.
            holdStartTime = Time.Current;

            return true;
        }

        public bool OnReleased(ManiaAction action)
        {
            // Make sure that the user started holding the key during the hold note
            if (!holdStartTime.HasValue)
                return false;

            if (action != Action)
                return false;

            holdStartTime = null;

            // If the key has been released too early, the user should not receive full score for the release
            if (!tail.IsHit)
                hasBroken = true;

            return true;
        }

        /// <summary>
        /// The head note of a hold.
        /// </summary>
        private class DrawableHeadNote : DrawableNote
        {
            private readonly DrawableKaraokeNote holdNote;

            public DrawableHeadNote(DrawableKaraokeNote holdNote, ManiaAction action)
                : base(holdNote.HitObject.Head, action)
            {
                this.holdNote = holdNote;

                GlowPiece.Alpha = 0;
            }

            protected override void UpdateState(ArmedState state)
            {
                // The holdnote keeps scrolling through for now, so having the head disappear looks weird
            }
        }

        /// <summary>
        /// The tail note of a hold.
        /// </summary>
        private class DrawableTailNote : DrawableNote
        {
            /// <summary>
            /// Lenience of release hit windows. This is to make cases where the hold note release
            /// is timed alongside presses of other hit objects less awkward.
            /// Todo: This shouldn't exist for non-LegacyBeatmapDecoder beatmaps
            /// </summary>
            private const double release_window_lenience = 1.5;

            private readonly DrawableKaraokeNote holdNote;

            public DrawableTailNote(DrawableKaraokeNote holdNote, ManiaAction action)
                : base(holdNote.HitObject.Tail, action)
            {
                this.holdNote = holdNote;

                GlowPiece.Alpha = 0;
            }

            protected override void CheckForJudgements(bool userTriggered, double timeOffset)
            {
                // Factor in the release lenience
                timeOffset /= release_window_lenience;

                if (!userTriggered)
                {
                    if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    {
                        AddJudgement(new HoldNoteTailJudgement
                        {
                            Result = HitResult.Miss,
                            HasBroken = holdNote.hasBroken
                        });
                    }

                    return;
                }

                var result = HitObject.HitWindows.ResultFor(timeOffset);
                if (result == HitResult.None)
                    return;

                AddJudgement(new HoldNoteTailJudgement
                {
                    Result = result,
                    HasBroken = holdNote.hasBroken
                });
            }

            protected override void UpdateState(ArmedState state)
            {
                // The holdnote keeps scrolling through, so having the tail disappear looks weird
            }
        }
    }

        /// <summary>
    /// Visualises a <see cref="Note"/> hit object.
    /// </summary>
    public class DrawableNote : DrawableBaseNote<Note>
    {
        protected readonly GlowPiece GlowPiece;

        private readonly LaneGlowPiece laneGlowPiece;
        private readonly NotePiece headPiece;

        public DrawableNote(Note hitObject, ManiaAction action)
            : base(hitObject, action)
        {
            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;

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
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft
                }
            };
        }

        public override Color4 AccentColour
        {
            get { return base.AccentColour; }
            set
            {
                base.AccentColour = value;
                laneGlowPiece.AccentColour = AccentColour;
                GlowPiece.AccentColour = AccentColour;
                headPiece.AccentColour = AccentColour;
            }
        }

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    AddJudgement(new ManiaJudgement { Result = HitResult.Miss });
                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None)
                return;

            AddJudgement(new ManiaJudgement { Result = result });
        }

        protected override void UpdateState(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Hit:
                case ArmedState.Miss:
                    this.FadeOut(100).Expire();
                    break;
            }
        }
    }
}
