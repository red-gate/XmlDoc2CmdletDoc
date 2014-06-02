using System;
using XmlDoc2CmdletDoc.Core;

namespace XmlDoc2CmdletDoc
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("Usage: XmlDoc2CmdletDoc.exe assemblyPath");
                Environment.ExitCode = -1;
            }
            else
            {
                var options = new Options(args[0]);
                Console.WriteLine(options);
                var engine = new Engine();
                var errorCode = engine.GenerateHelp(options);
                Console.WriteLine("Error code: " + errorCode);
                Environment.ExitCode = (int)errorCode;
            }

            Console.ReadLine();
        }
    }
}
