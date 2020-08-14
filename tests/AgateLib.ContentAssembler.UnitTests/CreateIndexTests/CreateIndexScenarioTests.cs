using AgateLib.ContentAssembler.Mocks;
using AgateLib.ContentModel;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace AgateLib.ContentAssembler.CreateIndexTests
{
    public class CreateIndexScenarioTests
    {
        private FakeFileSystem fileSystem = new FakeFileSystem();
        private ProjectBuild build;
        private ContentIndexer builder;
        private Mock<ILogger> log = new Mock<ILogger>();

        public CreateIndexScenarioTests()
        {
            Options options = new Options();
            build = new ProjectBuild
            {
                Output = "Content",
                Include = { "cc0" },
                CreateMgcb = "Content.mgcb",
                CreateCredits = "credits.txt",
                Index = new List<CreateIndex>
                {
                    new CreateIndex 
                    {
                        Output = "QuestPacks/quests.index",
                        Filter = "*.quest",
                        Recurse = true
                    }
                }
            };

            AddFile("cc0/content.index",
                @"credits: none");

            AddFile("cc0/QuestPacks/Core/Quests/find-person.quest");
            AddFile("cc0/QuestPacks/Core.questpack");

            builder = new ContentIndexer(options, build, fileSystem, log.Object);
        }

        protected void AddFile(string fileName, string content = null)
        {
            fileSystem.AddFile(fileName, content ?? fileName);
        }

        protected void RemoveFile(string fileName)
        {
            fileSystem.RemoveFile(fileName);
        }

        [Fact]
        public void PathPatternMatch()
        {
            ContentIndex index = builder.BuildContentIndex();
            new FileProcessor(build, index, fileSystem).ProcessFiles();

            var outputFile = index.OutputFiles["QuestPacks/quests.index"];

            var result = IndexParser.Parse(fileSystem.FileContents["Content/QuestPacks/quests.index"]);

            result.Select(x => x.Path)
                .Should().BeEquivalentTo(new[] { "QuestPacks/Core/Quests/find-person.quest" });

        }
    }
}
