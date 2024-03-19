// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Moq;
using NUnit.Framework;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.IO;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Skinning;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public class CheckColoursLuminosityTest
    {
        private CheckColoursLuminosity check = null!;

        private IBeatmap beatmap = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckColoursLuminosity();

            beatmap = new Beatmap<HitObject>
            {
                HitObjects =
                {
                    new HitCircle { StartTime = 0 },
                    new HitCircle { StartTime = 1_200 }
                },
            };
        }

        [Test]
        public void TestNoComboColours()
        {
            var skin = new LegacySkin(new SkinInfo(), null!);

            var mock = getMockWorkingBeatmap(skin);

            var context = getContext(mock);
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(0));
        }

        [Test]
        public void TestComboColoursNotDark()
        {
            var skin = new LegacySkin(new SkinInfo(), null!);

            skin.Configuration.CustomComboColours =
            [
                Colour4.FromHex("#E95054"),
                Colour4.FromHex("#FBA2A2")
            ];

            var mock = getMockWorkingBeatmap(skin);

            var context = getContext(mock);
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(0));
        }

        [Test]
        public void TestComboColoursReallyDark()
        {
            var skin = new LegacySkin(new SkinInfo(), null!);

            skin.Configuration.CustomComboColours =
            [
                Colour4.FromHex("#410E0E"),
                Colour4.FromHex("#FBA2A2")
            ];

            var mock = getMockWorkingBeatmap(skin);

            var context = getContext(mock);
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckColoursLuminosity.IssueTemplateReallyDark);
        }

        [Test]
        public void TestComboColoursTooDark()
        {
            var skin = new LegacySkin(new SkinInfo(), null!);

            skin.Configuration.CustomComboColours =
            [
                Colour4.FromHex("#000000"),
                Colour4.FromHex("#FBA2A2")
            ];

            var mock = getMockWorkingBeatmap(skin);

            var context = getContext(mock);
            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckColoursLuminosity.IssueTemplateTooDark);
        }

        private BeatmapVerifierContext getContext(Mock<IWorkingBeatmap> mockWorkingBeatmap)
        {
            return new BeatmapVerifierContext(beatmap, mockWorkingBeatmap.Object);
        }

        private Mock<IWorkingBeatmap> getMockWorkingBeatmap(ISkin skin)
        {
            var mockWorkingBeatmap = new Mock<IWorkingBeatmap>();
            mockWorkingBeatmap.SetupGet(w => w.Beatmap).Returns(beatmap);
            mockWorkingBeatmap.SetupGet(w => w.Skin).Returns(skin);
            return mockWorkingBeatmap;
        }
    }
}
