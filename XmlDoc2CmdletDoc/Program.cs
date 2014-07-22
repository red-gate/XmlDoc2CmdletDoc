using System;
using System.Linq;
using XmlDoc2CmdletDoc.Core;

namespace XmlDoc2CmdletDoc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            const string StrictSwitch = "-strict";

            bool treatWarningsAsErrors = false;
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
            else
            {
                var options = new Options(treatWarningsAsErrors, arguments.First());
                Console.WriteLine(options);
                var engine = new Engine();
                var errorCode = engine.GenerateHelp(options);
                Console.WriteLine("GenerateHelp completed with error code '{0}'", errorCode);
                Environment.Exit((int)errorCode);
            }
        }
    }
}
