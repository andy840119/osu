using osu.Game.Rulesets.Mania.Beatmaps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public class TestCaseKaraokeNotePlayfield : OsuTestCase
    {
        private RulesetInfo maniaRuleset;
        private DependencyContainer dependencies;
        private KaraokePlayfield playfield;

        public TestCaseKaraokeNotePlayfield()
        {
            var rng = new Random(1337);

            /* 
            AddStep("test columns", () =>
            {
                Clear();
                
                var drawableNote = CreateDrawableHitObject();

                var column = new Column();
                column.VisibleTimeRange.Value = 1000;
                column.VisibleTimeRange.TriggerChange();
                column.AccentColour = Color4.Blue;
                Add(column);

                column.Add(drawableNote);
            });
            */

            /* 
            AddStep("test stage", () =>
            {
                Clear();

                var drawableNote = CreateDrawableHitObject();

                //add stage
                var stage = new KaraokeStage(0,new StageDefinition(){Columns = 10});
                Add(stage);

                //add hit object 
                stage.Add(drawableNote);
            });
            */

            AddStep("test playField", () =>
            {
                var drawableNote = CreateDrawableHitObject();

                //add playfield
                var stages = new List<StageDefinition>
                {
                    new StageDefinition { Columns = 10 },
                    new StageDefinition { Columns = 10 },
                };
                playfield = createPlayfield(stages);

                playfield.Add(drawableNote);
            });
            

            //add hitExplosion
            AddStep("Hit explosion", () =>
            {

                int col = rng.Next(0, 4);

                var note = new HoldNote { Column = col , Duration = 1000, StartTime  = 1000};
                note.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                var drawableNote = new DrawableKaraokeNoteGroup(note, ManiaAction.Key1)
                {
                    //AccentColour = playfield.Columns.ElementAt(col).AccentColour
                };

                playfield.OnJudgement(drawableNote, new ManiaJudgement { Result = HitResult.Perfect });
                //playfield.Columns[col].OnJudgement(drawableNote, new ManiaJudgement { Result = HitResult.Perfect });
            });

            //add note
            AddStep("Add Note", () =>
            {
                int col = rng.Next(0, 4);
                var note = new HoldNote { Column = col , Duration = 1000, StartTime  = 1000};

                note.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                var drawableNote = new DrawableKaraokeNoteGroup(note, ManiaAction.Key1)
                {
                    //AccentColour = playfield.Columns.ElementAt(col).AccentColour
                };

                playfield.Add(drawableNote);
            });
        }

        protected DrawableKaraokeNoteGroup CreateDrawableHitObject(int column = -1)
        {
            if(column == -1)
            {
                var rng = new Random(1337);
                column = rng.Next(0, 4);
            }

            var note = new HoldNote { Column = column, Duration = 1000, StartTime  = 1000};
            note.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
            var drawableNote = new DrawableKaraokeNoteGroup(note, ManiaAction.Key1)
            {
                X = 100,
                Width = 100,
                LifetimeStart = double.MinValue,
                LifetimeEnd = double.MaxValue,
                AccentColour = Color4.Red,
            };

            return drawableNote;
        }

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DependencyContainer(base.CreateLocalDependencies(parent));

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets, SettingsStore settings)
        {
            maniaRuleset = rulesets.GetRuleset(3);

            dependencies.Cache(new ManiaConfigManager(settings, maniaRuleset, 4));
        }

        private KaraokePlayfield createPlayfield(List<StageDefinition> stages, bool inverted = false)
        {
            Clear();

            var inputManager = new ManiaInputManager(maniaRuleset, stages.Sum(g => g.Columns)) { RelativeSizeAxes = Axes.Both };
            Add(inputManager);

            KaraokePlayfield playfield;

            inputManager.Add(playfield = new KaraokePlayfield(stages)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });

            playfield.Inverted.Value = inverted;

            return playfield;
        }
    }

    public class KaraokePlayfield : ScrollingPlayfield
    {
        /// <summary>
        /// Whether this playfield should be inverted. This flips everything inside the playfield.
        /// </summary>
        public readonly Bindable<bool> Inverted = new Bindable<bool>(false);

        private readonly List<KaraokeStage> stages = new List<KaraokeStage>();

        public KaraokePlayfield(List<StageDefinition> stageDefinitions)
            : base(ScrollingDirection.Left)
        {
            if (stageDefinitions == null)
                throw new ArgumentNullException(nameof(stageDefinitions));

            if (stageDefinitions.Count <= 0)
                throw new ArgumentException("Can't have zero or fewer stages.");

            Inverted.Value = true;

            GridContainer playfieldGrid;
            
            int firstColumnIndex = 0;

            var content = new Drawable[stageDefinitions.Count][];
            for (int i = 0; i < stageDefinitions.Count; i++)
            {
                var newStage = new KaraokeStage(firstColumnIndex, stageDefinitions[i]);
                newStage.VisibleTimeRange.BindTo(VisibleTimeRange);
                newStage.Inverted.BindTo(Inverted);

                content[i] = new[]{ newStage } ;

                stages.Add(newStage);
                AddNested(newStage);

                firstColumnIndex += newStage.Columns.Count;
            }

            InternalChild = playfieldGrid = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = content,
            };
        }

        public override void Add(DrawableHitObject h) => getStageByColumn(((ManiaHitObject)h.HitObject).Column).Add(h);

        public void Add(BarLine barline) => stages.ForEach(s => s.Add(barline));

        private KaraokeStage getStageByColumn(int column)
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
    }

    /// <summary>
    /// the stage contains list note group
    /// </summary>
    internal class KaraokeStage : ScrollingPlayfield
    {
        public const float HIT_TARGET_POSITION = 200;

        public const float COLUMN_HEIGHT = 25;

        public const float COLUMN_SPACING = 1;

        /// <summary>
        /// Whether this playfield should be inverted. This flips everything inside the playfield.
        /// </summary>
        public readonly Bindable<bool> Inverted = new Bindable<bool>(true);

        public IReadOnlyList<Background> Columns => columnFlow.Children;
        private readonly FillFlowContainer<Background> columnFlow;

        protected override Container<Drawable> Content => content;
        private readonly Container<Drawable> content;

        public Container<DrawableManiaJudgement> Judgements => judgements;
        private readonly JudgementContainer<DrawableManiaJudgement> judgements;

        private List<Color4> normalColumnColours = new List<Color4>();
        private Color4 specialColumnColour;

        private readonly int firstColumnIndex;

        public KaraokeStage(int firstColumnIndex, StageDefinition definition)
            : base(ScrollingDirection.Left)
        {
            this.firstColumnIndex = firstColumnIndex;

            Name = "Stage";

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Name = "Columns mask",
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Name = "Background",
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Black,
                                    Alpha = 0.5f,
                                },
                                columnFlow = new FillFlowContainer<Background>
                                {
                                    Name = "Columns",
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Padding = new MarginPadding { Top = COLUMN_SPACING, Bottom = COLUMN_SPACING },
                                    Spacing = new Vector2(0, COLUMN_SPACING)
                                },
                            }
                        },
                        new Container
                        {
                            Name = "Barlines mask",
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            Height = 1366, // Bar lines should only be masked on the vertical axis
                            BypassAutoSizeAxes = Axes.Both,
                            Masking = true,
                            Child = content = new Container
                            {
                                Name = "Bar lines",
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                Padding = new MarginPadding { Left = HIT_TARGET_POSITION }
                            }
                        },
                        judgements = new JudgementContainer<DrawableManiaJudgement>
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            X = HIT_TARGET_POSITION + 150,
                            BypassAutoSizeAxes = Axes.Both
                        },
                    }
                }
            };

            for (int i = 0; i < definition.Columns; i++)
            {
                var isSpecial = definition.IsSpecialColumn(i);
                var column = new Background
                {
                    Height = COLUMN_HEIGHT,
                };
                AddBackground(column);
            }

            Inverted.ValueChanged += invertedChanged;
            Inverted.TriggerChange();
        }

        private void invertedChanged(bool newValue)
        {
            //TODO : change the position but not change scale
            //Scale = new Vector2(newValue ? -1 : 1,1);
            Judgements.Scale = Scale;
        }

        public void AddBackground(Background c)
        {
            columnFlow.Add(c);
        }

        public override void Add(DrawableHitObject h)
        {
            h.Y = 0;
            h.OnJudgement += OnJudgement;
            //add
            base.Add(h);
        }

        public void Add(BarLine barline) => base.Add(new DrawableBarLine(barline));

        internal void OnJudgement(DrawableHitObject judgedObject, Judgement judgement)
        {
            judgements.Clear();
            judgements.Add(new DrawableManiaJudgement(judgement, judgedObject)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            normalColumnColours = new List<Color4>
            {
                colours.Gray0,
                colours.Gray9
            };

            specialColumnColour = colours.BlueLight;

            var nonSpecialColumns = Columns.ToList();

            // We'll set the colours of the non-special columns in a separate loop, because the non-special
            // column colours are mirrored across their centre and special styles mess with this
            for (int i = 0; i < nonSpecialColumns.Count; i++)
            {
                Color4 colour = normalColumnColours[i % normalColumnColours.Count];
                nonSpecialColumns[i].AccentColour = colour;
                //nonSpecialColumns[nonSpecialColumns.Count - 1 - i].AccentColour = colour;
            }
        }

        protected override void Update()
        {
            // Due to masking differences, it is not possible to get the width of the columns container automatically
            // While masking on effectively only the Y-axis, so we need to set the width of the bar line container manually
            content.Height = columnFlow.Height;
        }
    }

    public class Background : Box, IHasAccentColour
    {
        public Background()
        {
            RelativeSizeAxes = Axes.X;
        }


        private Color4 accentColour;
        public Color4 AccentColour
        {
            get { return accentColour; }
            set
            {
                if (accentColour == value)
                    return;
                accentColour = value;

                this.Colour = accentColour;

            }
        }
    }

    
    public class DrawableLyricNote : DrawableBaseNote<HoldNote>
    {
        private readonly DrawableNote head;
        private readonly DrawableNote tail;

        private readonly GlowPiece glowPiece;
        private readonly BodyPiece bodyPiece;
        private readonly Container fullHeightContainer;

        private readonly Container<DrawableHoldNoteTick> tickContainer;

        /// <summary>
        /// Time at which the user started holding this hold note. Null if the user is not holding this hold note.
        /// </summary>
        private double? holdStartTime;

        /// <summary>
        /// Whether the hold note has been released too early and shouldn't give full score for the release.
        /// </summary>
        private bool hasBroken;

        private Container noteContainer;

        public DrawableLyricNote(float height,HoldNote hitObject, ManiaAction action) : base(hitObject,action)
        {
            RelativeSizeAxes = Axes.Y;
            InternalChildren = new Drawable[]
            {
                noteContainer = new Container()
                {
                    Y = height,
                    Children = new Drawable[]
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
                    }
                }
            };
            

            //foreach (var tick in tickContainer)
            //    noteContainer.Add(tick);

            //noteContainer.Add(head);
            //noteContainer.Add(tail);
        }

        private Color4 accentColour;
        public Color4 AccentColour
        {
            get { return accentColour; }
            set
            {
                accentColour = value;

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

        /// <summary>
        /// The head note of a hold.
        /// </summary>
        private class DrawableHeadNote : DrawableNote
        {
            private readonly DrawableLyricNote holdNote;

            public DrawableHeadNote(DrawableLyricNote holdNote, ManiaAction action)
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

            private readonly DrawableLyricNote holdNote;

            public DrawableTailNote(DrawableLyricNote holdNote, ManiaAction action)
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
    /// list of DrawableLyricNote
    /// </summary>
    public class DrawableKaraokeNoteGroup : DrawableBaseNote<HoldNote>
    {

        private FillFlowContainer<DrawableLyricNote> listNote;

        /// <summary>
        /// Whether the hold note has been released too early and shouldn't give full score for the release.
        /// </summary>
        private bool hasBroken;

        public DrawableKaraokeNoteGroup(HoldNote hitObject, ManiaAction action)
            : base(hitObject, action)
        {
            RelativeSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                listNote = new FillFlowContainer<DrawableLyricNote>
                {
                    Name = "Background",
                    Direction = FillDirection.Horizontal,
                    RelativeSizeAxes = Axes.Both,
                },
            };

            for(int i=0;i<10;i++)
            {
                var note = new DrawableLyricNote(KaraokeStage.COLUMN_HEIGHT * i,hitObject,action);
                note.Width = 100;
                //note.Height = KaraokeStage.COLUMN_HEIGHT;
                //note.Y = KaraokeStage.COLUMN_HEIGHT * i;
                listNote.Add(note);
            }
        }

        protected override void UpdateState(ArmedState state)
        {
            switch (state)
            {
                //case ArmedState.Hit:
                    // Good enough for now, we just want them to have a lifetime end
                //    this.Delay(2000).Expire();
                //    break;
            }
        }

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            AddJudgement(new HoldNoteJudgement { Result = HitResult.Perfect });
        }
    }
}
