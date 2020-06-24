using System;
using System.IO;
using CommandLine;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AgateLib.ContentAssembler
{
    public class ContentPipelineBuilder : FileAccessor
    {
        private string buildFile;
        private Options options;
        private readonly ILogger log;
        private ProjectBuild build;

        public ContentPipelineBuilder(string buildFile, Options options, IFileSystem fileSystem, ILogger log) : base(fileSystem)
        {
            this.buildFile = buildFile;
            this.options = options;
            this.log = log;
        }

        public void Run()
        {
            FileSystem.PathRoot = Path.GetDirectoryName(buildFile);

            ReadBuildFile();

            ContentIndex index = new IndexBuilder(options, build, FileSystem, log).BuildContentIndex();
            new FileProcessor(build, index, FileSystem).ProcessFiles();
            new MgcbFileCreator(build, index, FileSystem).CreateMgcbFile();
        }

        private void ReadBuildFile()
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new HyphenatedNamingConvention())
                .Build();

            try
            {
                build = deserializer.Deserialize<ProjectBuild>(File.ReadAllText(buildFile));

                if (build == null)
                {
                    throw new InvalidOperationException($"{buildFile} is empty.");
                }
            }
            catch (YamlException e)
            {
                log.LogError("YamlDeserialization",
                             "13",
                             "YAML",
                             Path.GetFullPath(buildFile),
                             e.Start.Line,
                             e.Start.Column,
                             e.End.Line,
                             e.End.Column,
                             e.Message);

                throw new ContentException($"Failed to deserialize {buildFile}.", e);
            }
        }
    }
}
