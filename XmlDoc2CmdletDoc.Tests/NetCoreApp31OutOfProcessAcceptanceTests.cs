#if NETCOREAPP3_1
using NUnit.Framework;

namespace XmlDoc2CmdletDoc.Tests
{
    [TestFixture]
    public class NetCoreApp31OutOfProcessAcceptanceTests : OutOfProcessAcceptanceTestBase
    {
        protected override string TestAssemblyFrameworkName => "netcoreapp3.1";
    }
}
#endif
