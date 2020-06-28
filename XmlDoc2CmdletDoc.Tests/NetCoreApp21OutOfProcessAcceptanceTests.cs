#if NETCOREAPP2_1 || NETCOREAPP3_1
using NUnit.Framework;

namespace XmlDoc2CmdletDoc.Tests
{
    [TestFixture]
    public class NetCoreApp21OutOfProcessAcceptanceTests : OutOfProcessAcceptanceTestBase
    {
        protected override string TestAssemblyFrameworkName => "netcoreapp2.1";
    }
}
#endif
