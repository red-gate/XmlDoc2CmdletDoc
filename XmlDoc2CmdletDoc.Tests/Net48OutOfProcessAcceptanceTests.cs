#if NET48
using NUnit.Framework;

namespace XmlDoc2CmdletDoc.Tests
{
    [TestFixture]
    public class Net48OutOfProcessAcceptanceTests : OutOfProcessAcceptanceTestBase
    {
        protected override string TestAssemblyFrameworkName => "net48";
    }
}
#endif
