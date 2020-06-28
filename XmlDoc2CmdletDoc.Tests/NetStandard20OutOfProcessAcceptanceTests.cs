using NUnit.Framework;

namespace XmlDoc2CmdletDoc.Tests
{
    [TestFixture]
    public class NetStandard20OutOfProcessAcceptanceTests : OutOfProcessAcceptanceTestBase
    {
        protected override string TestAssemblyFrameworkName => "netstandard2.0";
    }
}
