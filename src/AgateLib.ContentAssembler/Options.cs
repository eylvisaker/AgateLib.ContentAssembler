using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgateLib.ContentAssembler
{
    public class Options
    {
        [Value(0)]
        public IEnumerable<string> Files { get; set; }
    }
}
