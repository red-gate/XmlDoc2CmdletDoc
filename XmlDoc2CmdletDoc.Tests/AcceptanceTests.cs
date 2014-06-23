
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;
using XmlDoc2CmdletDoc.TestModule.Manual;

namespace XmlDoc2CmdletDoc.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        private static readonly XNamespace mshNs = XNamespace.Get("http://msh");
        private static readonly XNamespace mamlNs = XNamespace.Get("http://schemas.microsoft.com/maml/2004/10");
        private static readonly XNamespace commandNs = XNamespace.Get("http://schemas.microsoft.com/maml/dev/command/2004/10");
        private static readonly XNamespace devNs = XNamespace.Get("http://schemas.microsoft.com/maml/dev/2004/10");

        private static readonly IXmlNamespaceResolver resolver;

        static AcceptanceTests()
        {
            var manager = new XmlNamespaceManager(new NameTable());
            manager.AddNamespace("", mshNs.NamespaceName);
            manager.AddNamespace("maml", mamlNs.NamespaceName);
            manager.AddNamespace("command", commandNs.NamespaceName);
            manager.AddNamespace("dev", devNs.NamespaceName);
            resolver = manager;
        }

        private XElement rootElement;
        private XElement testManualElementsCommandElement;

        [TestFixtureSetUp]
        public void SetUp()
        {
            // ARRANGE
            var assemblyPath = typeof(TestManualElementsCommand).Assembly.Location;
            var cmdletXmlHelpPath = Path.ChangeExtension(assemblyPath, ".dll-Help.xml");
            if (File.Exists(cmdletXmlHelpPath))
            {
                File.Delete(cmdletXmlHelpPath);
            }

            // ACT
            Program.Main(new[] { assemblyPath });

            // ASSERT
            Assert.That(File.Exists(cmdletXmlHelpPath));

            using (var stream = File.OpenRead(cmdletXmlHelpPath))
            {
                var document = XDocument.Load(stream);
                rootElement = document.Root;
            }
            testManualElementsCommandElement = rootElement.XPathSelectElement("command:command[command:details/command:name/text() = 'Test-ManualElements']", resolver);
        }

        [Test]
        public void ThereShouldBeACommandEntry_ForTheTestManualElementsCmdlet()
        {
            Assert.That(testManualElementsCommandElement, Is.Not.Null);
        }

        [Test]
        public void ThereShouldBeACmdletSynopsis_ForTheTestManualElementsCmdlet()
        {
            Assume.That(testManualElementsCommandElement, Is.Not.Null);

            var synopsisDescription = testManualElementsCommandElement.XPathSelectElement("command:details/maml:description", resolver);

            Assert.That(synopsisDescription, Is.Not.Null);

            var expectedXml =
@"<description xmlns=""http://schemas.microsoft.com/maml/2004/10"">
  <para>This is part of the Test-ManualElement synopsis.</para>
  <para>This is also part of the Test-ManualElement synopsis.</para>
</description>";
            Assert.That(synopsisDescription.ToSimpleString(), Is.EqualTo(expectedXml));
        }

        [Test]
        public void ThereShouldBeACmdletDescription_ForTheTestManualElementsCmdlet()
        {
            Assume.That(testManualElementsCommandElement, Is.Not.Null);

            var synopsisDescription = testManualElementsCommandElement.XPathSelectElement("maml:description", resolver);

            Assert.That(synopsisDescription, Is.Not.Null);

            var expectedXml =
@"<description xmlns=""http://schemas.microsoft.com/maml/2004/10"">
  <para>This is part of the Test-ManualElement description.</para>
  <para>This is also part of the Test-ManualElement description.</para>
</description>";
            Assert.That(synopsisDescription.ToSimpleString(), Is.EqualTo(expectedXml));
        }

        [Test]
        public void WhenThereAreNoParameterSets_ThereShouldBeOnlyOneCommandSyntaxItem_ForTheTestManualElementsCmdlet()
        {
            Assume.That(testManualElementsCommandElement, Is.Not.Null);

            var syntaxItems = testManualElementsCommandElement.XPathSelectElements("command:syntax/command:syntaxItem", resolver).ToList();

            Assert.That(syntaxItems, Is.Not.Empty);
            Assert.That(syntaxItems.Count, Is.EqualTo(1));
        }
    }
}
