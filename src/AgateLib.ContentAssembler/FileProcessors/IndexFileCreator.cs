using AgateLib.ContentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AgateLib.ContentAssembler.FileProcessors
{
    public class IndexFileCreator : FileAccessor, IFileSource
    {
        private readonly ContentIndex index;
        private readonly CreateIndex file;

        private readonly Regex[] filters;
        private readonly string indexRoot;

        public IndexFileCreator(IFileSystem fileSystem, ContentIndex index, CreateIndex file)
            : base(fileSystem)
        {
            this.index = index;
            this.file = file;

            indexRoot = Path.GetDirectoryName(file.Output).Replace(@"\", "/");

            filters = file.Filter.Split('|').Select(FileGlobToRegex).ToArray();
        }

        public bool IsFile => false;

        public string SourcePath => throw new NotSupportedException();

        public IReadOnlyList<Regex> Filters => filters;

        public void Process(string outputPath)
        {
            List<IndexedFile> files = new List<IndexedFile>();

            foreach (var file in MatchFiles())
            {
                FileContent fileBuild = index.OutputFiles[file];
                ContentType contentType = ContentTypeOf(fileBuild);

                string indexedPath = file;

                if (contentType != ContentType.Raw)
                {
                    indexedPath = Path.Combine(
                        Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file));
                }

                files.Add(new IndexedFile
                {
                    Path = indexedPath.Replace("\\", "/"),
                    Type = ContentTypeOf(fileBuild),
                });
            }

            switch (file.Format)
            {
                case OutputFormat.JSON:
                    string result = JsonConvert.SerializeObject(files, new StringEnumConverter());

                    File.WriteAllText(outputPath, result);
                    break;

                case OutputFormat.YAML:
                    throw new NotImplementedException();
            }
        }

        private ContentType ContentTypeOf(FileContent fileBuild)
        {
            if (fileBuild.MonoGame.AdditionType == AdditionType.Import)
            {
                switch (fileBuild.MonoGame.Processor)
                {
                    case "TextureProcessor":
                        return ContentType.Texture;
                }
            }

            return ContentType.Raw;
        }

        private IEnumerable<string> MatchFiles()
            => index.OutputFiles.Keys.Where(x => x.Replace(@"\", "/").StartsWith(indexRoot)).Where(FilterMatches);

        private bool FilterMatches(string outputFile) => filters.Any(x => x.IsMatch(outputFile));

        private Regex FileGlobToRegex(string arg)
        {
            var regex = arg.Replace("\\", "/")
                           .Replace("/", @"[\\/]")
                           .Replace(".", @"\.")
                           .Replace("*", @"[^/\\]*");

            if (!file.Recurse)
            {
                regex = "^" + indexRoot.Replace("/", @"[\\/]") + @"[\\/]" + regex;
            }
            else
            {
                regex = "^" + indexRoot.Replace("/", @"[\\/]") + @"[\\/]([^\\/]+[\\/])*" + regex;
            }

            regex += "$";

            var result = new Regex(regex);

            return result;
        }

    }
}
