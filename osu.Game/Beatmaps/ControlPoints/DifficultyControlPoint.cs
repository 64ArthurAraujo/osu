// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.ControlPoints
{
    /// <remarks>
    /// Note that going forward, this control point type should always be assigned directly to HitObjects.
    /// </remarks>
    public class DifficultyControlPoint : ControlPoint, IEquatable<DifficultyControlPoint>
    {
        public static readonly DifficultyControlPoint DEFAULT = new DifficultyControlPoint
        {
            SliderVelocityBindable = { Disabled = true },
            GenerateTicksBindable = { Disabled = true },
        };

        /// <summary>
        /// The slider velocity at this control point.
        /// </summary>
        public readonly BindableDouble SliderVelocityBindable = new BindableDouble(1)
        {
            Precision = 0.01,
            Default = 1,
            MinValue = 0.1,
            MaxValue = 10
        };

        /// <summary>
        /// Whether or not slider ticks should be generated at this control point.
        /// This exists for backwards compatibility with maps that abuse NaN slider velocity behavior on osu!stable (e.g. /b/2628991).
        /// </summary>
        public readonly BindableBool GenerateTicksBindable = new BindableBool(true);

        public override Color4 GetRepresentingColour(OsuColour colours) => colours.Lime1;

        /// <summary>
        /// The slider velocity at this control point.
        /// </summary>
        public double SliderVelocity
        {
            get => SliderVelocityBindable.Value;
            set => SliderVelocityBindable.Value = value;
        }

        /// <summary>
        /// Whether or not slider ticks should be generated at this control point.
        /// This exists for backwards compatibility with maps that abuse NaN slider velocity behavior on osu!stable (e.g. /b/2628991).
        /// </summary>
        public bool GenerateTicks
        {
            get => GenerateTicksBindable.Value;
            set => GenerateTicksBindable.Value = value;
        }

        public override bool IsRedundant(ControlPoint? existing)
            => existing is DifficultyControlPoint existingDifficulty
               && SliderVelocity == existingDifficulty.SliderVelocity
               && GenerateTicks == existingDifficulty.GenerateTicks;

        public override void CopyFrom(ControlPoint other)
        {
            SliderVelocity = ((DifficultyControlPoint)other).SliderVelocity;
            GenerateTicks = ((DifficultyControlPoint)other).GenerateTicks;

            base.CopyFrom(other);
        }

        public override bool Equals(ControlPoint? other)
            => other is DifficultyControlPoint otherDifficultyControlPoint
               && Equals(otherDifficultyControlPoint);

        public bool Equals(DifficultyControlPoint? other)
            => base.Equals(other)
               && SliderVelocity == other.SliderVelocity
               && GenerateTicks == other.GenerateTicks;

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), SliderVelocity, GenerateTicks);
    }
}
