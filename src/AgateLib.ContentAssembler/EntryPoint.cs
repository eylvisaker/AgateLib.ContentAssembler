﻿using CommandLine;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AgateLib.ContentAssembler.Loggers;

namespace AgateLib.ContentAssembler
{
    public class EntryPoint
    {
        public static int Main(string[] args)
        {
            int exitCode = 0;
            
            Console.WriteLine("AgateLib Content Assembler " + typeof(EntryPoint).Assembly.GetName().Version.ToString());
            Console.WriteLine("===================================");

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options =>
                {
                    foreach(string file in options.Files) 
                    {
                        Console.WriteLine("Procesing file " + file);

                        try
                        {
                            var builder = new ContentPipelineBuilder(file, options, new SystemIOFileSystem(), new ConsoleLogger());
                            builder.Run();
                        }
                        catch (ContentException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            exitCode = 1;
                            Console.Error.WriteLine(e.ToString());
                        }
                    }
                })
                .WithNotParsed(errors =>
                {
                    exitCode = -1;
                    errors.Output();
                });

            return exitCode;
        }
    }

    public class AgateLibContentAssembler : Task
    {
        public string Include { get; set; }

        public override bool Execute()
        {
            Log.LogMessage("AgateLib.ContentAssembler starting up...");

            var options = new Options();

            if (!File.Exists(Include))
            {
                Log.LogError($"Cannot find AgateLibContentAssembler file {Include} because it does not exist.");
                return false;
            }

            var builder = new ContentPipelineBuilder(
                Include,
                options,
                new SystemIOFileSystem(),
                new TaskLogger(Log));

            try
            {
                builder.Run();
                return true;
            }
            catch (ContentException)
            {
                return false;
            }
            catch (Exception e)
            {
                Log.LogError("Unknown error." + e.ToString());
                return false;
            }
        }
    }
}
