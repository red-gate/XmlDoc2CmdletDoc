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
            const string strictSwitch = "-strict";

            try
            {
                var treatWarningsAsErrors = false;
                string assemblyPath = null;

                for (var i = 0; i < args.Length; i++)
                {
                    if (args[i] == strictSwitch)
                    {
                        treatWarningsAsErrors = true;
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

                return new Options(treatWarningsAsErrors, assemblyPath);
            }
            catch (ArgumentException)
            {
                Console.Error.WriteLine("Usage: XmlDoc2CmdletDoc.exe [{0}] assemblyPath", strictSwitch);
                Environment.Exit(-1);
                throw;
            }
        }
    }
}
