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
    public class CreateIndexPathSeparatorUnitTests
    {
        private FakeFileSystem fileSystem;
        private ProjectBuild build;
        private ContentIndexer builder;
        private Mock<ILogger> log = new Mock<ILogger>();

        private void Init(string pathSep)
        {
            fileSystem = new FakeFileSystem(pathSep);

            Options options = new Options();
            build = new ProjectBuild
            {
                Output = "Content",
                Include = { "cc0", "cc-by-sa" },
                CreateMgcb = "Content.mgcb",
                CreateCredits = "credits.txt",
                Index = new List<CreateIndex>
                {
                    new CreateIndex 
                    {
                        Output = $"QuestPacks{pathSep}quests.index",
                        Filter = "*.quest",
                        Recurse = true
                    },
                    new CreateIndex
                    {
                        Output = $"Sprites{pathSep}LPC{pathSep}sprite.index",
                        Filter = "*.png",
                        Recurse = true
                    }
                }
            };

            AddFile($"cc0{pathSep}content.index",
                @"credits: none");

            AddFile($"cc0{pathSep}QuestPacks{pathSep}Core{pathSep}Quests{pathSep}find-person.quest");
            AddFile($"cc0{pathSep}QuestPacks{pathSep}Core.questpack");

            AddFile($"cc-by-sa{pathSep}content.index",
                @"credits: none");

            AddFile($@"cc-by-sa{pathSep}Sprites{pathSep}LPC{pathSep}body{pathSep}female{pathSep}dark.png");
            AddFile($@"cc-by-sa{pathSep}Sprites{pathSep}LPC{pathSep}body{pathSep}female{pathSep}dark2.png");
            AddFile($@"cc-by-sa{pathSep}Sprites{pathSep}LPC{pathSep}body{pathSep}female{pathSep}darkelf.png");
            AddFile($@"cc-by-sa{pathSep}Sprites{pathSep}LPC{pathSep}body{pathSep}female{pathSep}light.png");
            AddFile($@"cc-by-sa{pathSep}Sprites{pathSep}LPC{pathSep}body{pathSep}female{pathSep}orc.png");

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

        [Theory]
        [InlineData(@"/")]
        [InlineData(@"\")]
        public void QuestFilesMatch(string pathSep)
        {
            Init(pathSep);

            ContentIndex index = builder.BuildContentIndex();
            new FileProcessor(build, index, fileSystem).ProcessFiles();

            var outputFile = index.OutputFiles[$"QuestPacks{pathSep}quests.index"];

            var result = IndexParser.Parse(fileSystem.FileContents[$"Content{pathSep}QuestPacks{pathSep}quests.index"]);

            result.Select(x => x.Path)
                .Should().BeEquivalentTo(new[] { "QuestPacks/Core/Quests/find-person.quest" });

        }

        [Theory]
        [InlineData(@"/")]
        [InlineData(@"\")]
        public void ImagePatternMatch(string pathSep)
        {
            Init(pathSep);

            ContentIndex index = builder.BuildContentIndex();
            new FileProcessor(build, index, fileSystem).ProcessFiles();

            IndexedFile[] result = IndexParser.Parse(fileSystem.FileContents[$"Content{pathSep}Sprites{pathSep}LPC{pathSep}sprite.index"]);
            List<string> paths = result.Select(x => x.Path).ToList();

            paths.Count.Should().Be(5);
            paths.Should().BeEquivalentTo(new[] {
                $@"Sprites/LPC/body/female/dark",
                $@"Sprites/LPC/body/female/dark2",
                $@"Sprites/LPC/body/female/darkelf",
                $@"Sprites/LPC/body/female/light",
                $@"Sprites/LPC/body/female/orc"});

        }
    }
}
