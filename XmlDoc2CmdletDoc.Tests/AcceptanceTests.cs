
using System.IO;
using NUnit.Framework;
using XmlDoc2CmdletDoc.TestModule;

namespace XmlDoc2CmdletDoc.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void Test()
        {
            // ARRANGE
            var assemblyPath = typeof(TestManualElementsCommand).Assembly.Location;
            var cmdletXmlHelpPath = Path.ChangeExtension(assemblyPath, ".dll-Help.xml");
            if (File.Exists(cmdletXmlHelpPath))
            {
                File.Delete(cmdletXmlHelpPath);
            }

            // ACT
            Program.Main(new[] {assemblyPath});

            // ASSERT
            Assert.That(File.Exists(cmdletXmlHelpPath));
            var actualContent = File.ReadAllText(cmdletXmlHelpPath);
            var expectedPath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "expectedHelp.xml");
            var expectedContent = File.ReadAllText(expectedPath);
            Assert.That(actualContent, Is.EqualTo(expectedContent));
        }
    }
}
