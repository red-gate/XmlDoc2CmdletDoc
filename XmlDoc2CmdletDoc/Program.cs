using System;
using System.Collections.Generic;
using System.Linq;
using XmlDoc2CmdletDoc.Core;

namespace XmlDoc2CmdletDoc
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var options = ParseArguments(args);
            Console.WriteLine(options);
            var engine = new Engine();
            var exitCode = engine.GenerateHelp(options);
            Console.WriteLine("GenerateHelp completed with exit code '{0}'", exitCode);
            Environment.Exit((int)exitCode);
        }

        private static Options ParseArguments(IReadOnlyList<string> args)
        {
            const string strictSwitch = "-strict";
            const string excludeParameterSetSwitch = "-excludeParameterSets";

            try
            {
                var treatWarningsAsErrors = false;
                var excludedParameterSets = new List<string>();
                string assemblyPath = null;

                for (var i = 0; i < args.Count; i++)
                {
                    if (args[i] == strictSwitch)
                    {
                        treatWarningsAsErrors = true;
                    }
                    else if (args[i] == excludeParameterSetSwitch)
                    {
                        i++;
                        if (i >= args.Count) throw new ArgumentException();
                        excludedParameterSets.AddRange(args[i].Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()));
                    }
                    else if (assemblyPath == null)
                    {
                        assemblyPath = args[i];
                    }
                    else
                    {
                        throw new ArgumentException();
                    }
                }

                if (assemblyPath == null)
                {
                    throw new ArgumentException();
                }

                return new Options(treatWarningsAsErrors, assemblyPath, excludedParameterSets);
            }
            catch (ArgumentException)
            {
                Console.Error.WriteLine($"Usage: XmlDoc2CmdletDoc.exe [{strictSwitch}] [{excludeParameterSetSwitch} parameterSetToExclude1,parameterSetToExclude2] assemblyPath");
                Environment.Exit(-1);
                throw;
            }
        }
    }
}
