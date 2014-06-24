
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;
using XmlDoc2CmdletDoc.TestModule.Maml;
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
        public void Command_Syntax_NoParameterSetNames()
        {
            Assume.That(testManualElementsCommandElement, Is.Not.Null);

            var syntaxItems = testManualElementsCommandElement.XPathSelectElements("command:syntax/command:syntaxItem", resolver).ToList();

            Assert.That(syntaxItems, Is.Not.Empty);
            Assert.That(syntaxItems.Count, Is.EqualTo(1));
        }

        [Test]
        public void Command_Syntax_MultipleParameterSetNames_ThereShouldBeACommandSyntaxItemForEachOne()
        {
            Assume.That(testMamlElementsCommandElement, Is.Not.Null);

            var syntaxItems = testMamlElementsCommandElement.XPathSelectElements("command:syntax/command:syntaxItem", resolver).ToList();

            Assert.That(syntaxItems, Is.Not.Empty);
            Assert.That(syntaxItems.Count, Is.EqualTo(2));
        }

        [Test]
        public void Command_Syntax_MultipleParameterSetNames_EachSyntaxItemShouldContainParametersForOnlyASingleParameterSetName()
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
        [TestCase("MandatoryParameter")]
        [TestCase("OptionalParameter")]
        [TestCase("PositionedParameter")]
        [TestCase("ValueFromPipelineParameter")]
        [TestCase("ValueFromPipelineByPropertyNameParameter")]
        public void Command_Parameters_Parameter(string parameterName)
        {
            Assume.That(testManualElementsCommandElement, Is.Not.Null);

            var parameter = testManualElementsCommandElement.XPathSelectElement(
                string.Format("command:parameters/command:parameter[maml:name/text() = '{0}']", parameterName), resolver);
            Assert.That(parameter, Is.Not.Null);
        }

        [Test]
        [TestCase("MandatoryParameter", "true")]
        [TestCase("OptionalParameter", "false")]
        public void Command_Parameters_Parameter_RequiredAttribute(string parameterName, string expectedValue)
        {
            Assume.That(testManualElementsCommandElement, Is.Not.Null);

            var parameter = testManualElementsCommandElement.XPathSelectElement(
                string.Format("command:parameters/command:parameter[maml:name/text() = '{0}']", parameterName), resolver);
            Assume.That(parameter, Is.Not.Null);

            var attribute = parameter.Attribute("required");
            Assert.That(attribute.Value, Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCase("MandatoryParameter", "named")]
        [TestCase("PositionedParameter", "1")]
        public void Command_Parameters_Parameter_PositionAttribute(string parameterName, string expectedValue)
        {
            Assume.That(testManualElementsCommandElement, Is.Not.Null);

            var parameter = testManualElementsCommandElement.XPathSelectElement(
                string.Format("command:parameters/command:parameter[maml:name/text() = '{0}']", parameterName), resolver);
            Assume.That(parameter, Is.Not.Null);

            var attribute = parameter.Attribute("position");
            Assert.That(attribute.Value, Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCase("MandatoryParameter", "false")]
        [TestCase("ValueFromPipelineParameter", "true")]
        [TestCase("ValueFromPipelineByPropertyNameParameter", "true")]
        public void Command_Parameters_Parameter_PipelineInputAttribute(string parameterName, string expectedValue)
        {
            Assume.That(testManualElementsCommandElement, Is.Not.Null);

            var parameter = testManualElementsCommandElement.XPathSelectElement(
                string.Format("command:parameters/command:parameter[maml:name/text() = '{0}']", parameterName), resolver);
            Assume.That(parameter, Is.Not.Null);

            var attribute = parameter.Attribute("pipelineInput");
            Assert.That(attribute.Value, Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCase("MandatoryParameter", "false")] // TODO: Globbing is always false. Once we add support for it, update this test.
        public void Command_Parameters_Parameter_GlobbingInputAttribute(string parameterName, string expectedValue)
        {
            Assume.That(testManualElementsCommandElement, Is.Not.Null);

            var parameter = testManualElementsCommandElement.XPathSelectElement(
                string.Format("command:parameters/command:parameter[maml:name/text() = '{0}']", parameterName), resolver);
            Assume.That(parameter, Is.Not.Null);

            var attribute = parameter.Attribute("globbing");
            Assert.That(attribute.Value, Is.EqualTo(expectedValue));
        }

        [Test]
        public void Command_Parmeters_Parameter_Description_ForTestManualElements()
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
        public void Command_Parmeters_Parameter_Description_ForTestMamlElements()
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

        [Test]
        public void Command_Parmeters_Parameter_Type_ForTestManualElements()
        {
            Assume.That(testManualElementsCommandElement, Is.Not.Null);

            var parameter = testManualElementsCommandElement.XPathSelectElement("command:parameters/command:parameter[maml:name/text() = 'MandatoryParameter']", resolver);
            Assume.That(parameter, Is.Not.Null);

            var type = parameter.XPathSelectElement("dev:type", resolver);
            CheckManualClassType(type);
        }

        [Test]
        public void Command_Parmeters_Parameter_Type_ForTestMamlElements()
        {
            Assume.That(testMamlElementsCommandElement, Is.Not.Null);

            var parameter = testMamlElementsCommandElement.XPathSelectElement("command:parameters/command:parameter[maml:name/text() = 'CommonParameter']", resolver);
            Assume.That(parameter, Is.Not.Null);

            var type = parameter.XPathSelectElement("dev:type", resolver);
            CheckMamlClassType(type);
        }

        [Test]
        public void Command_InputTypes_ForTestManualElements()
        {
            Assume.That(testManualElementsCommandElement, Is.Not.Null);

            var inputTypes = testManualElementsCommandElement
                .XPathSelectElements("command:inputTypes/command:inputType", resolver)
                .ToList();
            Assert.That(inputTypes, Is.Not.Empty);
            Assert.That(inputTypes.Count, Is.EqualTo(2));

            {
                var inputType = inputTypes.First();
                var name = inputType.XPathSelectElement("dev:type/maml:name", resolver);
                Assert.That(name.Value, Is.EqualTo(typeof(string).FullName));
            }

            {
                var returnValue = inputTypes.Last();
                var type = returnValue.XPathSelectElement("dev:type", resolver);
                CheckManualClassType(type);

                // Currently the returnValue description is the same as the type description. If we provide another
                // means to specify the description, the following assertion should be changed.
                var description = returnValue.XPathSelectElement("maml:description", resolver);
                Assert.That(description.ToSimpleString(), Is.EqualTo(ManualClassDescription));
            }
        }

        [Test]
        public void Command_InputTypes_ForTestMamlElements()
        {
            Assume.That(testMamlElementsCommandElement, Is.Not.Null);

            var inputTypes = testMamlElementsCommandElement
                .XPathSelectElements("command:inputTypes/command:inputType", resolver)
                .ToList();
            Assert.That(inputTypes, Is.Not.Empty);
            Assert.That(inputTypes.Count, Is.EqualTo(2));

            {
                var inputType = inputTypes.First();
                var name = inputType.XPathSelectElement("dev:type/maml:name", resolver);
                Assert.That(name.Value, Is.EqualTo(typeof(string).FullName));
            }

            {
                var returnValue = inputTypes.Last();
                var type = returnValue.XPathSelectElement("dev:type", resolver);
                CheckMamlClassType(type);

                // Currently the returnValue description is the same as the type description. If we provide another
                // means to specify the description, the following assertion should be changed.
                var description = returnValue.XPathSelectElement("maml:description", resolver);
                Assert.That(description.ToSimpleString(), Is.EqualTo(MamlClassDescription));
            }
        }

        [Test]
        public void Command_ReturnValues_ForTestManualElements()
        {
            Assume.That(testManualElementsCommandElement, Is.Not.Null);

            var returnValues = testManualElementsCommandElement
                .XPathSelectElements("command:returnValues/command:returnValue", resolver)
                .ToList();
            Assert.That(returnValues, Is.Not.Empty);
            Assert.That(returnValues.Count, Is.EqualTo(2));

            {
                var returnValue = returnValues.First();
                var name = returnValue.XPathSelectElement("dev:type/maml:name", resolver);
                Assert.That(name.Value, Is.EqualTo(typeof(string).FullName));
            }

            {
                var returnValue = returnValues.Last();
                var type = returnValue.XPathSelectElement("dev:type", resolver);
                CheckManualClassType(type);

                // Currently the returnValue description is the same as the type description. If we provide another
                // means to specify the description, the following assertion should be changed.
                var description = returnValue.XPathSelectElement("maml:description", resolver);
                Assert.That(description.ToSimpleString(), Is.EqualTo(ManualClassDescription));
            }
        }

        [Test]
        public void Command_ReturnValues_ForTestMamlElements()
        {
            Assume.That(testMamlElementsCommandElement, Is.Not.Null);

            var returnValues = testMamlElementsCommandElement
                .XPathSelectElements("command:returnValues/command:returnValue", resolver)
                .ToList();
            Assert.That(returnValues, Is.Not.Empty);
            Assert.That(returnValues.Count, Is.EqualTo(2));

            {
                var returnValue = returnValues.First();
                var name = returnValue.XPathSelectElement("dev:type/maml:name", resolver);
                Assert.That(name.Value, Is.EqualTo(typeof(string).FullName));
            }

            {
                var returnValue = returnValues.Last();
                var type = returnValue.XPathSelectElement("dev:type", resolver);
                CheckMamlClassType(type);

                // Currently the returnValue description is the same as the type description. If we provide another
                // means to specify the description, the following assertion should be changed.
                var description = returnValue.XPathSelectElement("maml:description", resolver);
                Assert.That(description.ToSimpleString(), Is.EqualTo(MamlClassDescription));
            }
        }

        [Test]
        public void Command_ALertSet_ForTestManualElements()
        {
            Assume.That(testManualElementsCommandElement, Is.Not.Null);

            var alertSet = testManualElementsCommandElement.XPathSelectElement("maml:alertSet", resolver);
            Assert.That(alertSet, Is.Not.Null);
            Assert.That(alertSet.ToSimpleString(), Is.EqualTo(AlertSet));
        }

        [Test]
        public void Command_ALertSet_ForTestMamlElements()
        {
            Assume.That(testMamlElementsCommandElement, Is.Not.Null);

            var alertSet = testMamlElementsCommandElement.XPathSelectElement("maml:alertSet", resolver);
            Assert.That(alertSet, Is.Not.Null);
            Assert.That(alertSet.ToSimpleString(), Is.EqualTo(AlertSet));
        }

        private const string AlertSet =
@"<alertSet xmlns=""http://schemas.microsoft.com/maml/2004/10"">
  <title>First Note</title>
  <alert>
    <para>This is the description for the first note.</para>
  </alert>
  <title>Second Note</title>
  <alert>
    <para>This is part of the description for the second note.</para>
    <para>This is also part of the description for the second note.</para>
  </alert>
</alertSet>";

        private void CheckManualClassType(XElement type)
        {
            Assert.That(type, Is.Not.Null);

            var name = type.XPathSelectElement("maml:name", resolver);
            Assert.That(name, Is.Not.Null);
            Assert.That(name.Value, Is.EqualTo(typeof(ManualClass).FullName));

            var description = type.XPathSelectElement("maml:description", resolver);
            Assert.That(description, Is.Not.Null);
            Assert.That(description.ToSimpleString(), Is.EqualTo(ManualClassDescription));
        }

        private const string ManualClassDescription =
@"<description xmlns=""http://schemas.microsoft.com/maml/2004/10"">
  <para>This is part of the ManualClass description.</para>
  <para>This is also part of the ManualClass description.</para>
</description>";

        private void CheckMamlClassType(XElement type)
        {
            Assert.That(type, Is.Not.Null);

            var name = type.XPathSelectElement("maml:name", resolver);
            Assert.That(name, Is.Not.Null);
            Assert.That(name.Value, Is.EqualTo(typeof(MamlClass).FullName));

            var description = type.XPathSelectElement("maml:description", resolver);
            Assert.That(description, Is.Not.Null);
            Assert.That(description.ToSimpleString(), Is.EqualTo(MamlClassDescription));
        }

        private const string MamlClassDescription =
@"<description xmlns=""http://schemas.microsoft.com/maml/2004/10"">
  <para>This is the MamlClass description.</para>
</description>";
    }
}
