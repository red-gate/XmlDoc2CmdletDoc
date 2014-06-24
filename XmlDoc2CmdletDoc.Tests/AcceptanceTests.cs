
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
        private XElement testMamlElementsCommandElement;

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
            testMamlElementsCommandElement = rootElement.XPathSelectElement("command:command[command:details/command:name/text() = 'Test-MamlElements']", resolver);
        }

        [Test]
        public void Command_ForTestManualElements()
        {
            Assert.That(testManualElementsCommandElement, Is.Not.Null);
        }

        [Test]
        public void Command_ForTestMamlElements()
        {
            Assert.That(testMamlElementsCommandElement, Is.Not.Null);
        }

        [Test]
        public void Command_Details_Synopsis_ForTestManualElements()
        {
            Assume.That(testManualElementsCommandElement, Is.Not.Null);

            var synopsis = testManualElementsCommandElement.XPathSelectElement("command:details/maml:description", resolver);

            Assert.That(synopsis, Is.Not.Null);

            var expectedXml =
@"<description xmlns=""http://schemas.microsoft.com/maml/2004/10"">
  <para>This is part of the Test-ManualElements synopsis.</para>
  <para>This is also part of the Test-ManualElements synopsis.</para>
</description>";
            Assert.That(synopsis.ToSimpleString(), Is.EqualTo(expectedXml));
        }

        [Test]
        public void Command_Details_Synopsis_ForTestMamlElements()
        {
            Assume.That(testMamlElementsCommandElement, Is.Not.Null);

            var synopsis = testMamlElementsCommandElement.XPathSelectElement("command:details/maml:description", resolver);

            Assert.That(synopsis, Is.Not.Null);

            var expectedXml =
@"<description xmlns=""http://schemas.microsoft.com/maml/2004/10"">
  <para>This is the Test-MamlElements synopsis.</para>
</description>";
            Assert.That(synopsis.ToSimpleString(), Is.EqualTo(expectedXml));
        }

        [Test]
        public void Command_Description_ForTestManualElements()
        {
            Assume.That(testManualElementsCommandElement, Is.Not.Null);

            var description = testManualElementsCommandElement.XPathSelectElement("maml:description", resolver);

            Assert.That(description, Is.Not.Null);

            var expectedXml =
@"<description xmlns=""http://schemas.microsoft.com/maml/2004/10"">
  <para>This is part of the Test-ManualElements description.</para>
  <para>This is also part of the Test-ManualElements description.</para>
</description>";
            Assert.That(description.ToSimpleString(), Is.EqualTo(expectedXml));
        }

        [Test]
        public void Command_Description_ForTestMamlElements()
        {
            Assume.That(testMamlElementsCommandElement, Is.Not.Null);

            var description = testMamlElementsCommandElement.XPathSelectElement("maml:description", resolver);

            Assert.That(description, Is.Not.Null);

            var expectedXml =
@"<description xmlns=""http://schemas.microsoft.com/maml/2004/10"">
  <para>This is the Test-MamlElements description.</para>
</description>";
            Assert.That(description.ToSimpleString(), Is.EqualTo(expectedXml));
        }

        [Test]
        public void Command_Syntax_WhenThereAreNoParameterSets_ThereShouldBeOnlyOneCommandSyntaxItem()
        {
            Assume.That(testManualElementsCommandElement, Is.Not.Null);

            var syntaxItems = testManualElementsCommandElement.XPathSelectElements("command:syntax/command:syntaxItem", resolver).ToList();

            Assert.That(syntaxItems, Is.Not.Empty);
            Assert.That(syntaxItems.Count, Is.EqualTo(1));
        }

        [Test]
        public void Command_Syntax_WhenThereAreMultipleParameterSetNames_ThereShouldBeACommandSyntaxItemForEachOne()
        {
            Assume.That(testMamlElementsCommandElement, Is.Not.Null);

            var syntaxItems = testMamlElementsCommandElement.XPathSelectElements("command:syntax/command:syntaxItem", resolver).ToList();

            Assert.That(syntaxItems, Is.Not.Empty);
            Assert.That(syntaxItems.Count, Is.EqualTo(2));
        }

        [Test]
        public void Command_Syntax_WhenThereAreMultipleParameterSetNames_EachSyntaxItemShouldContainParametersForOnlyASingleParameterSetName()
        {
            Assume.That(testMamlElementsCommandElement, Is.Not.Null);

            var syntaxItems = testMamlElementsCommandElement.XPathSelectElements("command:syntax/command:syntaxItem", resolver).ToList();

            Assume.That(syntaxItems, Is.Not.Empty);
            Assume.That(syntaxItems.Count, Is.EqualTo(2));

            {
                var syntaxItemOne = syntaxItems[0];
                var names = syntaxItemOne.XPathSelectElements("command:parameter/maml:name", resolver).Select(x => x.Value);
                Assume.That(names, Is.EqualTo(new [] {"CommonParameter", "ParameterOne"}));
            }

            {
                var syntaxItemTwo = syntaxItems[1];
                var names = syntaxItemTwo.XPathSelectElements("command:parameter/maml:name", resolver).Select(x => x.Value);
                Assume.That(names, Is.EqualTo(new [] {"CommonParameter", "ParameterTwo"}));
            }
        }

        [Test]
        public void Command_Parameters_Parameter_ForTestManualEvents_MandatoryParameter()
        {
            Assume.That(testManualElementsCommandElement, Is.Not.Null);

            var parameter = testManualElementsCommandElement.XPathSelectElement("command:parameters/command:parameter[maml:name/text() = 'MandatoryParameter']", resolver);
            Assert.That(parameter, Is.Not.Null);
        }

        [Test]
        public void Command_Parameters_Parameter_RequiredAttribute_ForTestManualEvents_MandatoryParameter()
        {
            Assume.That(testManualElementsCommandElement, Is.Not.Null);

            var parameter = testManualElementsCommandElement.XPathSelectElement("command:parameters/command:parameter[maml:name/text() = 'MandatoryParameter']", resolver);
            Assume.That(parameter, Is.Not.Null);

            var attribute = parameter.Attribute("required");
            Assert.That(attribute.Value, Is.EqualTo("true"));
        }

        [Test]
        public void Command_Parmeters_Parameter_Description_ForTestManualEvents_MandatoryParameter()
        {
            Assume.That(testManualElementsCommandElement, Is.Not.Null);

            var parameter = testManualElementsCommandElement.XPathSelectElement("command:parameters/command:parameter[maml:name/text() = 'MandatoryParameter']", resolver);
            Assume.That(parameter, Is.Not.Null);

            var description = parameter.XPathSelectElement("maml:description", resolver);
            Assert.That(description, Is.Not.Null);

            var expectedXml =
@"<description xmlns=""http://schemas.microsoft.com/maml/2004/10"">
  <para>This is part of the MandatoryParameter description.</para>
  <para>This is also part of the MandatoryParameter description.</para>
</description>";
            Assert.That(description.ToSimpleString(), Is.EqualTo(expectedXml));
        }

        [Test]
        public void Command_Parmeters_Parameter_Description_ForTestMamlEvents_CommonParameter()
        {
            Assume.That(testMamlElementsCommandElement, Is.Not.Null);

            var parameter = testMamlElementsCommandElement.XPathSelectElement("command:parameters/command:parameter[maml:name/text() = 'CommonParameter']", resolver);
            Assume.That(parameter, Is.Not.Null);

            var description = parameter.XPathSelectElement("maml:description", resolver);
            Assert.That(description, Is.Not.Null);

            var expectedXml =
@"<description xmlns=""http://schemas.microsoft.com/maml/2004/10"">
  <para>This is the CommonParameter description.</para>
</description>";
            Assert.That(description.ToSimpleString(), Is.EqualTo(expectedXml));
        }
    }
}
