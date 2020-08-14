using AgateLib.ContentAssembler.FileProcessors;
using AgateLib.ContentAssembler.Mocks;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace AgateLib.ContentAssembler.CreateIndexTests
{
    public class IndexFileCreatorUnitTests
    {
        [Theory]
        [InlineData(@"QuestPacks\Core\Quests\find-person.quest", true)]
        [InlineData(@"QuestPacks\Core\fetch.quest", true)]
        [InlineData(@"QuestPacks\core.questpack", false)]
        [InlineData(@"QuestPacks/Core/Quests/find-person.quest", true)]
        [InlineData(@"QuestPacks/Core/fetch.quest", true)]
        [InlineData(@"QuestPacks/core.questpack", false)]
        public void PatternMatchWithRecursion(string filename, bool shouldMatch)
        {
            var fileSystem = new FakeFileSystem();
            var ifc = new IndexFileCreator(fileSystem, null, new CreateIndex
            {
                Filter = "*.quest",
                Output = "QuestPacks/quests.index",
                Recurse = true,
            });

            ifc.Filters.Count.Should().Be(1);

            ifc.Filters[0].IsMatch(filename).Should().Be(shouldMatch);
        }

        [Theory]
        [InlineData(@"QuestPacks\Core\Quests\find-person.quest", false)]
        [InlineData(@"QuestPacks\Core\fetch.quest", false)]
        [InlineData(@"QuestPacks\fetch.quest", true)]
        [InlineData(@"QuestPacks\core.questpack", false)]
        [InlineData(@"QuestPacks/Core/Quests/find-person.quest", false)]
        [InlineData(@"QuestPacks/Core/fetch.quest", false)]
        [InlineData(@"QuestPacks/fetch.quest", true)]
        [InlineData(@"QuestPacks/core.questpack", false)]
        public void PatternMatchNoRecursion(string filename, bool shouldMatch)
        {
            var fileSystem = new FakeFileSystem();
            var ifc = new IndexFileCreator(fileSystem, null, new CreateIndex
            {
                Filter = "*.quest",
                Output = "QuestPacks/quests.index",
            });

            ifc.Filters.Count.Should().Be(1);

            ifc.Filters[0].IsMatch(filename).Should().Be(shouldMatch);
        }
    }
}
