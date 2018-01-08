using System;
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

        private static Options ParseArguments(string[] args)
        {
            const string StrictSwitch = "-strict";

            var treatWarningsAsErrors = false;
            var arguments = args.ToList();
            if (arguments.Contains(StrictSwitch))
            {
                treatWarningsAsErrors = true;
                arguments.Remove(StrictSwitch);
            }

            if (arguments.Count != 1)
            {
                Console.Error.WriteLine("Usage: XmlDoc2CmdletDoc.exe [{0}] assemblyPath", StrictSwitch);
                Environment.Exit(-1);
            }

            var options = new Options(treatWarningsAsErrors, arguments.First());
            return options;
        }
    }
}
