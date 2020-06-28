using NUnit.Framework;
using XmlDoc2CmdletDoc.Core;
using XmlDoc2CmdletDoc.TestModule.Manual;

namespace XmlDoc2CmdletDoc.Tests
{
    [TestFixture]
    public class InProcessAcceptanceTests : AcceptanceTestBase
    {
        protected override string TestAssemblyPath => typeof(TestManualElementsCommand).Assembly.Location;

        protected override void GenerateHelpForTestAssembly(string assemblyPath)
        {
            var options = new Options(false, assemblyPath);
            var engine = new Engine();
            engine.GenerateHelp(options);
        }
    }
}
