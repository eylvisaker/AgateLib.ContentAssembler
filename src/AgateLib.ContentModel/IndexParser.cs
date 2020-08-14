using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AgateLib.ContentModel
{
    public static class IndexParser
    {
        public static IndexedFile[] Parse(string indexFileContents)
        {
            return JsonConvert.DeserializeObject<IndexedFile[]>(indexFileContents);
        }

        public static IndexedFile[] ReadIndex(Stream fileStream)
        {
            using (var reader = new StreamReader(fileStream))
            {
                string indexFileContents = reader.ReadToEnd();

                return Parse(indexFileContents);
            }
        }
    }
}
