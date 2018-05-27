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

                var drawableNote = new DrawableKaraokeNote(note, ManiaAction.Key1)
                {
                    AccentColour = playfield.Columns.ElementAt(col).AccentColour
                };

                playfield.OnJudgement(drawableNote, new ManiaJudgement { Result = HitResult.Perfect });
                playfield.Columns[col].OnJudgement(drawableNote, new ManiaJudgement { Result = HitResult.Perfect });
            });

            //add note
            AddStep("Add Note", () =>
            {
                int col = rng.Next(0, 4);
                var note = new HoldNote { Column = col , Duration = 1000, StartTime  = 1000};

                note.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                var drawableNote = new DrawableKaraokeNote(note, ManiaAction.Key1)
                {
                    AccentColour = playfield.Columns.ElementAt(col).AccentColour
                };

                playfield.Add(drawableNote);
            });
        }

        protected DrawableKaraokeNote CreateDrawableHitObject(int column = -1)
        {
            if(column == -1)
            {
                var rng = new Random(1337);
                column = rng.Next(0, 4);
            }

            var note = new HoldNote { Column = column, Duration = 1000, StartTime  = 1000};
            note.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
            var drawableNote = new DrawableKaraokeNote(note, ManiaAction.Key1)
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

        public List<Column> Columns => stages.SelectMany(x => x.Columns).ToList();
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
    /// A collection of <see cref="Column"/>s.
    /// </summary>
    internal class KaraokeStage : ScrollingPlayfield
    {
        public const float HIT_TARGET_POSITION = 200;

        /// <summary>
        /// Whether this playfield should be inverted. This flips everything inside the playfield.
        /// </summary>
        public readonly Bindable<bool> Inverted = new Bindable<bool>(true);

        public IReadOnlyList<Column> Columns => columnFlow.Children;
        private readonly FillFlowContainer<Column> columnFlow;

        protected override Container<Drawable> Content => content;
        private readonly Container<Drawable> content;

        public Container<DrawableManiaJudgement> Judgements => judgements;
        private readonly JudgementContainer<DrawableManiaJudgement> judgements;

        private readonly Container topLevelContainer;

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
                                columnFlow = new FillFlowContainer<Column>
                                {
                                    Name = "Columns",
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Padding = new MarginPadding { Top = 1, Bottom = 1 },
                                    Spacing = new Vector2(0, 1)
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
                        topLevelContainer = new Container { RelativeSizeAxes = Axes.Both }
                    }
                }
            };

            for (int i = 0; i < definition.Columns; i++)
            {
                var isSpecial = definition.IsSpecialColumn(i);
                var column = new Column
                {
                    IsSpecial = isSpecial,
                    //Action = isSpecial ? specialColumnStartAction++ : normalColumnStartAction++
                };

                AddColumn(column);
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

        public void AddColumn(Column c)
        {
            c.VisibleTimeRange.BindTo(VisibleTimeRange);

            topLevelContainer.Add(c.TopLevelContainer.CreateProxy());
            columnFlow.Add(c);
            AddNested(c);
        }

        public override void Add(DrawableHitObject h)
        {
            var maniaObject = (ManiaHitObject)h.HitObject;
            int columnIndex = maniaObject.Column - firstColumnIndex;
            Columns.ElementAt(columnIndex).Add(h);
            h.OnJudgement += OnJudgement;
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

            // Set the special column + colour + key
            foreach (var column in Columns)
            {
                if (!column.IsSpecial)
                    continue;

                column.AccentColour = specialColumnColour;
            }

            var nonSpecialColumns = Columns.Where(c => !c.IsSpecial).ToList();

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

    public class Column : ScrollingPlayfield, IHasAccentColour
    {
        private const float key_icon_size = 10;
        private const float key_icon_corner_radius = 3;
        private const float key_icon_border_radius = 2;

        private const float hit_target_width = 0;
        private const float hit_target_bar_width = 2;

        private const float column_height = 25;
        private const float special_column_height = 30;

        public ManiaAction Action;

        private readonly Box background;
        private readonly Container hitTargetBar;
        private readonly Container keyIcon;

        internal readonly Container TopLevelContainer;
        private readonly Container explosionContainer;

        protected override Container<Drawable> Content => content;
        private readonly Container<Drawable> content;

        private const float opacity_released = 0.1f;
        private const float opacity_pressed = 0.25f;

        public Column()
            : base(ScrollingDirection.Left)
        {
            RelativeSizeAxes = Axes.X;
            Height = column_height;

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    Name = "Background",
                    RelativeSizeAxes = Axes.Both,
                    Alpha = opacity_released
                },
                new Container
                {
                    Name = "Hit target + hit objects",
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Left = KaraokeStage.HIT_TARGET_POSITION },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Name = "Hit target",
                            RelativeSizeAxes = Axes.Y,
                            Width = hit_target_width,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Name = "Background",
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Black
                                },
                                hitTargetBar = new Container
                                {
                                    Name = "Bar",
                                    RelativeSizeAxes = Axes.Y,
                                    Width = hit_target_bar_width,
                                    Masking = true,
                                    Children = new[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both
                                        }
                                    }
                                }
                            }
                        },
                        content = new Container
                        {
                            Name = "Hit objects",
                            RelativeSizeAxes = Axes.Both,
                        },
                        explosionContainer = new Container
                        {
                            Name = "Hit explosions",
                            RelativeSizeAxes = Axes.Both,
                            
                        }
                    }
                },
                //TODO : this container is unnecessary
                new Container
                {
                    Name = "Key",
                    RelativeSizeAxes = Axes.Y,
                    Width = KaraokeStage.HIT_TARGET_POSITION,
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Name = "Key gradient",
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientVertical(Color4.Black, Color4.Black.Opacity(0)),
                            Alpha = 0.3f
                        },
                        keyIcon = new Container
                        {
                            Name = "Key icon",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(key_icon_size),
                            Masking = true,
                            CornerRadius = key_icon_corner_radius,
                            BorderThickness = 2,
                            BorderColour = Color4.White, // Not true
                            Children = new[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0,
                                    AlwaysPresent = true
                                }
                            }
                        }
                    }
                },
                TopLevelContainer = new Container { RelativeSizeAxes = Axes.Both }
            };

            TopLevelContainer.Add(explosionContainer.CreateProxy());
        }

        public override Axes RelativeSizeAxes => Axes.X;

        private bool isSpecial;
        public bool IsSpecial
        {
            get { return isSpecial; }
            set
            {
                if (isSpecial == value)
                    return;
                isSpecial = value;

                Height = isSpecial ? special_column_height : column_height;
            }
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

                background.Colour = accentColour;

                hitTargetBar.EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Radius = 5,
                    Colour = accentColour.Opacity(0.5f),
                };

                keyIcon.EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Radius = 5,
                    Colour = accentColour.Opacity(0.5f),
                };
            }
        }

        /// <summary>
        /// Adds a DrawableHitObject to this Playfield.
        /// </summary>
        /// <param name="hitObject">The DrawableHitObject to add.</param>
        public override void Add(DrawableHitObject hitObject)
        {
            hitObject.AccentColour = AccentColour;
            hitObject.OnJudgement += OnJudgement;

            HitObjects.Add(hitObject);
        }

        internal void OnJudgement(DrawableHitObject judgedObject, Judgement judgement)
        {
            if (!judgement.IsHit)
                return;

            explosionContainer.Add(new HitExplosion(judgedObject)
            {
                Anchor = Anchor.CentreLeft
            });
        }
    }
}
