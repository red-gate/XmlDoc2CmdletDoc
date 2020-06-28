#if NET461 || NET472 || NET48
using NUnit.Framework;

namespace XmlDoc2CmdletDoc.Tests
{
    [TestFixture]
    public class Net461OutOfProcessAcceptanceTests : OutOfProcessAcceptanceTestBase
    {
        protected override string TestAssemblyFrameworkName => "net461";
    }
}
#endif
