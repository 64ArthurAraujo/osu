// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mania.Configuration;
using osuTK;

namespace osu.Game.Rulesets.Mania.UI
{
    /// <summary>
    /// An overlay that captures and displays osu!mania mouse and touch input.
    /// </summary>
    public partial class ManiaTouchInputArea : VisibilityContainer
    {
        private readonly DrawableManiaRuleset drawableRuleset;

        // visibility state affects our child. we always want to handle input.
        public override bool PropagatePositionalInputSubTree => true;
        public override bool PropagateNonPositionalInputSubTree => true;

        [SettingSource("Spacing", "The spacing between receptors.")]
        public BindableFloat Spacing { get; } = new BindableFloat(10)
        {
            Precision = 1,
            MinValue = 0,
            MaxValue = 100,
        };

        [SettingSource("Opacity", "The receptor opacity.")]
        public BindableFloat Opacity { get; } = new BindableFloat(1)
        {
            Precision = 0.1f,
            MinValue = 0,
            MaxValue = 1
        };

        [Resolved]
        private ManiaRulesetConfigManager rulesetConfig { get; set; } = null!;

        private GridContainer gridContainer = null!;

        private readonly BindableBool touchControls = new BindableBool();

        public ManiaTouchInputArea(DrawableManiaRuleset drawableRuleset)
        {
            this.drawableRuleset = drawableRuleset;

            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;

            RelativeSizeAxes = Axes.Both;
            Height = 0.5f;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            List<Drawable> receptorGridContent = new List<Drawable>();
            List<Dimension> receptorGridDimensions = new List<Dimension>();

            bool first = true;

            foreach (var stage in drawableRuleset.Playfield.Stages)
            {
                foreach (var column in stage.Columns)
                {
                    if (!first)
                    {
                        receptorGridContent.Add(new Gutter { Spacing = { BindTarget = Spacing } });
                        receptorGridDimensions.Add(new Dimension(GridSizeMode.AutoSize));
                    }

                    receptorGridContent.Add(new ColumnInputReceptor
                    {
                        Action = { BindTarget = column.Action },
                        Enabled = { BindTarget = touchControls },
                    });
                    receptorGridDimensions.Add(new Dimension());

                    first = false;
                }
            }

            InternalChild = gridContainer = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                AlwaysPresent = true,
                Content = new[] { receptorGridContent.ToArray() },
                ColumnDimensions = receptorGridDimensions.ToArray()
            };
        }

        private IBindable<ManiaMobileLayout> mobilePlayStyle = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            mobilePlayStyle = rulesetConfig.GetBindable<ManiaMobileLayout>(ManiaRulesetSetting.MobileLayout);
            mobilePlayStyle.BindValueChanged(p => touchControls.Value = p.NewValue == ManiaMobileLayout.LandscapeWithOverlay, true);

            Opacity.BindValueChanged(o => Alpha = o.NewValue, true);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            // Hide whenever the keyboard is used.
            Hide();
            return false;
        }

        protected override bool OnTouchDown(TouchDownEvent e)
        {
            if (touchControls.Value)
            {
                Show();
                return true;
            }

            return false;
        }

        protected override void PopIn()
        {
            gridContainer.FadeIn(500, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            gridContainer.FadeOut(300);
        }

        public partial class ColumnInputReceptor : CompositeDrawable
        {
            public readonly IBindable<ManiaAction> Action = new Bindable<ManiaAction>();
            public readonly IBindable<bool> Enabled = new BindableBool();

            private readonly Box highlightOverlay;

            [Resolved]
            private ManiaInputManager? inputManager { get; set; }

            private bool isPressed;

            public ColumnInputReceptor()
            {
                RelativeSizeAxes = Axes.Both;

                InternalChildren = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 10,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0.15f,
                            },
                            highlightOverlay = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                                Blending = BlendingParameters.Additive,
                            }
                        }
                    }
                };
            }

            protected override bool OnTouchDown(TouchDownEvent e)
            {
                if (Enabled.Value)
                {
                    updateButton(true);
                    return false; // handled by parent container to show overlay.
                }

                return false;
            }

            protected override void OnTouchUp(TouchUpEvent e)
            {
                updateButton(false);
            }

            private void updateButton(bool press)
            {
                if (press == isPressed)
                    return;

                isPressed = press;

                if (press)
                {
                    inputManager?.KeyBindingContainer.TriggerPressed(Action.Value);
                    highlightOverlay.FadeTo(0.1f, 80, Easing.OutQuint);
                }
                else
                {
                    inputManager?.KeyBindingContainer.TriggerReleased(Action.Value);
                    highlightOverlay.FadeTo(0, 400, Easing.OutQuint);
                }
            }
        }

        private partial class Gutter : Drawable
        {
            public readonly IBindable<float> Spacing = new Bindable<float>();

            public Gutter()
            {
                Spacing.BindValueChanged(s => Size = new Vector2(s.NewValue));
            }
        }
    }
}
