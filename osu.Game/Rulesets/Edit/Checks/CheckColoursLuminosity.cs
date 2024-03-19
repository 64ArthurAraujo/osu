// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckColoursLuminosity : ICheck
    {
        private const float luminosity_min_rankable = 30;

        private const float luminosity_min_warning = 43;

        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Settings, "Too dark or bright combo colours or slider borders.");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateTooDark(this),
            new IssueTemplateReallyDark(this)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            var beatmapSkin = (Skin)context.WorkingBeatmap.Skin!;

            var comboColours = beatmapSkin.Configuration.ComboColours;

            if (comboColours.IsNull()) yield break;

            foreach (var colour in comboColours)
            {
                float luminosity = getLuminosity(colour);

                if (luminosity < luminosity_min_rankable)
                {
                    yield return new IssueTemplateTooDark(this).Create(colour.ToHex());
                }
                else if (luminosity < luminosity_min_warning)
                {
                    yield return new IssueTemplateReallyDark(this).Create(colour.ToHex());
                }
            }
        }

        public class IssueTemplateTooDark : IssueTemplate
        {
            public IssueTemplateTooDark(ICheck check)
                : base(check, IssueType.Problem, "Combo colour {0} is way too dark. ")
            {
            }

            public Issue Create(string colour) => new Issue(this, colour);
        }

        public class IssueTemplateReallyDark : IssueTemplate
        {
            public IssueTemplateReallyDark(ICheck check)
                : base(check, IssueType.Warning, "Combo colour {0} is really dark, and might be hard to see.")
            {
            }

            public Issue Create(string colour) => new Issue(this, colour);
        }

        // Method from https://github.com/Naxesss/MapsetChecks/blob/ed77d6d0c4fd9aea41538e6aca395decce9c6637/Checks/AllModes/Settings/CheckLuminosity.cs#L168
        private static float getLuminosity(Colour4 colour)
        {
            int red = (int)(colour.R * 255);
            int green = (int)(colour.G * 255);
            int blue = (int)(colour.B * 255);

            return (float)Math.Sqrt(
                red * red * .299f +
                green * green * .587f +
                blue * blue * .114f);
        }

    }
}
