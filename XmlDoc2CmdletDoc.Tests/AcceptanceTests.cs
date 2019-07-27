using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;
using XmlDoc2CmdletDoc.Core;
using XmlDoc2CmdletDoc.TestModule.InputTypes;
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
        private XElement testReferencesCommandElement;
        private XElement testInputTypesCommandElement;
        private XElement testPositionedParametersCommandElement;
        private XElement testParameterlessCommandElement;
        private XElement testNestedTypeDynamicParametersCommandElement;
        private XElement testRuntimeDynamicParametersCommandElement;
        private XElement testDefaultValueCommandElement;

        [SetUp]
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
            var options = new Options(false, assemblyPath);
            var engine = new Engine();
            engine.GenerateHelp(options);

            // ASSERT
            Assert.That(File.Exists(cmdletXmlHelpPath));

            using (var stream = File.OpenRead(cmdletXmlHelpPath))
            {
                var document = XDocument.Load(stream);
                rootElement = document.Root;
            }
            testManualElementsCommandElement = rootElement.XPathSelectElement("command:command[command:details/command:name/text() = 'Test-ManualElements']", resolver);
            testMamlElementsCommandElement = rootElement.XPathSelectElement("command:command[command:details/command:name/text() = 'Test-MamlElements']", resolver);
            testReferencesCommandElement = rootElement.XPathSelectElement("command:command[command:details/command:name/text() = 'Test-References']", resolver);
            testInputTypesCommandElement = rootElement.XPathSelectElement("command:command[command:details/command:name/text() = 'Test-InputTypes']", resolver);
            testPositionedParametersCommandElement = rootElement.XPathSelectElement("command:command[command:details/command:name/text() = 'Test-PositionedParameters']", resolver);
            testParameterlessCommandElement = rootElement.XPathSelectElement("command:command[command:details/command:name/text() = 'Test-Parameterless']", resolver);
            testNestedTypeDynamicParametersCommandElement = rootElement.XPathSelectElement("command:command[command:details/command:name/text() = 'Test-NestedTypeDynamicParameters']", resolver);
            testRuntimeDynamicParametersCommandElement = rootElement.XPathSelectElement("command:command[command:details/command:name/text() = 'Test-RuntimeDynamicParameters']", resolver);
            testDefaultValueCommandElement = rootElement.XPathSelectElement("command:command[command:details/command:name/text() = 'Test-DefaultValue']", resolver);
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
            Assert.That(testManualElementsCommandElement, Is.Not.Null);

            var synopsis = testManualElementsCommandElement.XPathSelectElement("./command:details/maml:description", resolver);

            Assert.That(synopsis, Is.Not.Null);

            var expectedXml =
@"<maml:description xmlns:maml=""http://schemas.microsoft.com/maml/2004/10"">
  <maml:para>This is part of the Test-ManualElements synopsis.</maml:para>
  <maml:para>This is also part of the Test-ManualElements synopsis.</maml:para>
</maml:description>";
            Assert.That(synopsis.ToSimpleString(), Is.EqualTo(expectedXml));
        }

        [Test]
        public void Command_Details_Synopsis_ForTestMamlElements()
        {
            Assert.That(testMamlElementsCommandElement, Is.Not.Null);

            var synopsis = testMamlElementsCommandElement.XPathSelectElement("./command:details/maml:description", resolver);

            Assert.That(synopsis, Is.Not.Null);

            var expectedXml =
@"<maml:description xmlns:maml=""http://schemas.microsoft.com/maml/2004/10"">
  <maml:para>This is the Test-MamlElements synopsis.</maml:para>
</maml:description>";
            Assert.That(synopsis.ToSimpleString(), Is.EqualTo(expectedXml));
        }

        [Test]
        public void Command_Description_ForTestManualElements()
        {
            Assert.That(testManualElementsCommandElement, Is.Not.Null);

            var description = testManualElementsCommandElement.XPathSelectElement("./maml:description", resolver);

            Assert.That(description, Is.Not.Null);

            var expectedXml =
@"<maml:description xmlns:maml=""http://schemas.microsoft.com/maml/2004/10"">
  <maml:para>This is part of the Test-ManualElements description.</maml:para>
  <maml:para>This is also part of the Test-ManualElements description.</maml:para>
</maml:description>";
            Assert.That(description.ToSimpleString(), Is.EqualTo(expectedXml));
        }

        [Test]
        public void Command_Description_ForTestMamlElements()
        {
            Assert.That(testMamlElementsCommandElement, Is.Not.Null);

            var description = testMamlElementsCommandElement.XPathSelectElement("./maml:description", resolver);

            Assert.That(description, Is.Not.Null);

            var expectedXml =
@"<maml:description xmlns:maml=""http://schemas.microsoft.com/maml/2004/10"">
  <maml:para>This is the Test-MamlElements description.</maml:para>
</maml:description>";
            Assert.That(description.ToSimpleString(), Is.EqualTo(expectedXml));
        }

        [Test]
        public void Command_Description_ForReferencesElement_ContainingSeeElements()
        {
            Assert.That(testReferencesCommandElement, Is.Not.Null);

            var description = testReferencesCommandElement.XPathSelectElement("./maml:description", resolver);

            Assert.That(description, Is.Not.Null);

            var expectedXml =
@"<maml:description xmlns:maml=""http://schemas.microsoft.com/maml/2004/10"">
  <maml:para>This description for Test-References references ParameterOne and the second parameter.</maml:para>
</maml:description>";
            Assert.That(description.ToSimpleString(), Is.EqualTo(expectedXml));
        }

        [Test]
        public void Command_Syntax_NoParameterSetNames()
        {
            Assert.That(testManualElementsCommandElement, Is.Not.Null);

            var syntaxItems = testManualElementsCommandElement.XPathSelectElements("./command:syntax/command:syntaxItem", resolver).ToList();

            Assert.That(syntaxItems, Is.Not.Empty);
            Assert.That(syntaxItems.Count, Is.EqualTo(1));
        }

        [Test]
        public void Command_Syntax_MultipleParameterSetNames_ThereShouldBeACommandSyntaxItemForEachOne()
        {
            Assert.That(testMamlElementsCommandElement, Is.Not.Null);

            var syntaxItems = testMamlElementsCommandElement.XPathSelectElements("./command:syntax/command:syntaxItem", resolver).ToList();

            Assert.That(syntaxItems, Is.Not.Empty);
            Assert.That(syntaxItems.Count, Is.EqualTo(2));
        }

        [Test]
        public void Command_Syntax_MultipleParameterSetNames_EachSyntaxItemShouldContainParametersForOnlyASingleParameterSetName()
        {
            Assert.That(testMamlElementsCommandElement, Is.Not.Null);

            var syntaxItems = testMamlElementsCommandElement.XPathSelectElements("./command:syntax/command:syntaxItem", resolver).ToList();

            Assert.That(syntaxItems, Is.Not.Empty);
            Assert.That(syntaxItems.Count, Is.EqualTo(2));

            {
                var syntaxItemOne = syntaxItems[0];
                var names = syntaxItemOne.XPathSelectElements("./command:parameter/maml:name", resolver).Select(x => x.Value);
                Assert.That(names, Is.EqualTo(new[] { "ArrayParameter", "CommonParameter", "ParameterOne" }));
            }

            {
                var syntaxItemTwo = syntaxItems[1];
                var names = syntaxItemTwo.XPathSelectElements("./command:parameter/maml:name", resolver).Select(x => x.Value);
                Assert.That(names, Is.EqualTo(new[] { "ArrayParameter", "CommonParameter", "ParameterTwo" }));
            }
        }

        [Test]
        [TestCase("MandatoryParameter")]
        [TestCase("OptionalParameter")]
        [TestCase("PositionedParameter")]
        [TestCase("ValueFromPipelineParameter")]
        [TestCase("ValueFromPipelineByPropertyNameParameter")]
        [TestCase("ObsoleteParameter")] // Obsolete parameters should still have docs, so Get-Help MyCommand -Parameter ObsoleteParameter still works
        public void Command_Parameters_Parameter(string parameterName)
        {
            Assert.That(testManualElementsCommandElement, Is.Not.Null);

            var parameter = testManualElementsCommandElement.XPathSelectElement(
                $"./command:parameters/command:parameter[maml:name/text() = '{parameterName}']", resolver);
            Assert.That(parameter, Is.Not.Null);
        }
        
        [Test]
        [TestCase("MandatoryParameter", "true")]
        [TestCase("OptionalParameter", "false")]
        public void Command_Parameters_Parameter_RequiredAttribute(string parameterName, string expectedValue)
        {
            Assert.That(testManualElementsCommandElement, Is.Not.Null);

            var parameter = testManualElementsCommandElement.XPathSelectElement(
                $"./command:parameters/command:parameter[maml:name/text() = '{parameterName}']", resolver);
            Assert.That(parameter, Is.Not.Null);

            var attribute = parameter.Attribute("required");
            Assert.That(attribute.Value, Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCase("MandatoryParameter", "named")]
        [TestCase("PositionedParameter", "1")]
        public void Command_Parameters_Parameter_PositionAttribute(string parameterName, string expectedValue)
        {
            Assert.That(testManualElementsCommandElement, Is.Not.Null);

            var parameter = testManualElementsCommandElement.XPathSelectElement(
                $"./command:parameters/command:parameter[maml:name/text() = '{parameterName}']", resolver);
            Assert.That(parameter, Is.Not.Null);

            var attribute = parameter.Attribute("position");
            Assert.That(attribute.Value, Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCase("MandatoryParameter", "false")]
        [TestCase("ValueFromPipelineParameter", "true (ByValue)")]
        [TestCase("ValueFromPipelineByPropertyNameParameter", "true (ByPropertyName)")]
        public void Command_Parameters_Parameter_PipelineInputAttribute(string parameterName, string expectedValue)
        {
            Assert.That(testManualElementsCommandElement, Is.Not.Null);

            var parameter = testManualElementsCommandElement.XPathSelectElement(
                $"./command:parameters/command:parameter[maml:name/text() = '{parameterName}']", resolver);
            Assert.That(parameter, Is.Not.Null);

            var attribute = parameter.Attribute("pipelineInput");
            Assert.That(attribute.Value, Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCase("MandatoryParameter", "false")] // TODO: Globbing is always false. Once we add support for it, update this test.
        public void Command_Parameters_Parameter_GlobbingInputAttribute(string parameterName, string expectedValue)
        {
            Assert.That(testManualElementsCommandElement, Is.Not.Null);

            var parameter = testManualElementsCommandElement.XPathSelectElement(
                $"./command:parameters/command:parameter[maml:name/text() = '{parameterName}']", resolver);
            Assert.That(parameter, Is.Not.Null);

            var attribute = parameter.Attribute("globbing");
            Assert.That(attribute.Value, Is.EqualTo(expectedValue));
        }

        [TestCase("SingleAliasParameter", "Name1A")]
        [TestCase("WithDescriptionAliasParameter", "Name1B")]
        [TestCase("MultipleAliasParameter", "Name1C,Name2C,Name3C")]
        [TestCase("MandatoryParameter", null)]
        public void Command_Parameters_Parameter_AliasesAttribute(string parameterName, string expectedValue)
        {
            Assert.That(testManualElementsCommandElement, Is.Not.Null);

            var parameter = testManualElementsCommandElement.XPathSelectElement(
                $"./command:parameters/command:parameter[maml:name/text() = '{parameterName}']", resolver);
            Assert.That(parameter, Is.Not.Null);

            var attribute = parameter.Attribute("aliases");
            if (expectedValue == null) { Assert.That(attribute == null); }
            else { Assert.That(attribute.Value, Is.EqualTo(expectedValue)); }
        }

        [TestCase("SingleAliasParameter", "Name1A")]
        [TestCase("WithDescriptionAliasParameter", "Name1B")]
        [TestCase("MultipleAliasParameter", "Name1C,Name2C,Name3C")]
        public void Command_Parameters_Parameter_AliasesExistAsParametersWithNote(string parameterName, string aliases)
        {
            Assert.That(testManualElementsCommandElement, Is.Not.Null);

            foreach (var alias in aliases.Split(','))
            {
                var parameter = testManualElementsCommandElement.XPathSelectElement(
                    $"./command:parameters/command:parameter[maml:name/text() = '{alias}']", resolver);
                Assert.That(parameter, Is.Not.Null);

                var description = parameter.XPathSelectElement("./maml:description", resolver);
                Assert.That(description, Is.Not.Null);
                Assert.That(description.ToSimpleString(), Does.Match("is an alias of.*" + parameterName));
            }
        }

        [Test]
        public void Command_Parameters_Parameter_EnumValues_AddedToParameterValueGroup()
        {
            var targetName = "EnumParameter";
            CheckEnumParameterValues(targetName);
        }

        [Test]
        public void Command_Parameters_Parameter_EnumArrayValues_AddedToParameterValueGroup()
        {
            var targetName = "EnumArrayParameter";
            CheckEnumParameterValues(targetName);
        }

        [Test]
        public void Command_Parameters_Parameter_EnumListValues_AddedToParameterValueGroup()
        {
            var targetName = "EnumListParameter";
            CheckEnumParameterValues(targetName);
        }

        private void CheckEnumParameterValues(string targetName)
        {
            Assert.That(testManualElementsCommandElement, Is.Not.Null);

            var parameter = testManualElementsCommandElement.XPathSelectElement(
                $"./command:syntax/command:syntaxItem/command:parameter[maml:name/text() = '" + targetName + "']", resolver);
            Assert.That(parameter, Is.Not.Null);

            var valueGroup = parameter.XPathSelectElement("./command:parameterValueGroup", resolver);
            Assert.That(valueGroup, Is.Not.Null);

            var expectedXml =
@"<command:parameterValueGroup xmlns:command=""http://schemas.microsoft.com/maml/dev/command/2004/10"">
  <command:parameterValue required=""false"" variableLength=""false"">Regular</command:parameterValue>
  <command:parameterValue required=""false"" variableLength=""false"">Important</command:parameterValue>
  <command:parameterValue required=""false"" variableLength=""false"">Critical</command:parameterValue>
</command:parameterValueGroup>";

            Assert.That(valueGroup.ToSimpleString(), Is.EqualTo(expectedXml));
        }

        [Test]
        public void Command_Parmeters_Parameter_Description_ForTestManualElements()
        {
            Assert.That(testManualElementsCommandElement, Is.Not.Null);

            var parameter = testManualElementsCommandElement.XPathSelectElement("./command:parameters/command:parameter[maml:name/text() = 'MandatoryParameter']", resolver);
            Assert.That(parameter, Is.Not.Null);

            var description = parameter.XPathSelectElement("./maml:description", resolver);
            Assert.That(description, Is.Not.Null);

            var expectedXml =
@"<maml:description xmlns:maml=""http://schemas.microsoft.com/maml/2004/10"">
  <maml:para>This is part of the MandatoryParameter description.</maml:para>
  <maml:para>This is also part of the MandatoryParameter description.</maml:para>
</maml:description>";
            Assert.That(description.ToSimpleString(), Is.EqualTo(expectedXml));
        }

        [Test]
        public void Command_Parmeters_Parameter_Description_ForTestMamlElements()
        {
            Assert.That(testMamlElementsCommandElement, Is.Not.Null);

            var parameter = testMamlElementsCommandElement.XPathSelectElement("./command:parameters/command:parameter[maml:name/text() = 'CommonParameter']", resolver);
            Assert.That(parameter, Is.Not.Null);

            var description = parameter.XPathSelectElement("./maml:description", resolver);
            Assert.That(description, Is.Not.Null);

            var expectedXml =
@"<maml:description xmlns:maml=""http://schemas.microsoft.com/maml/2004/10"">
  <maml:para>This is the CommonParameter description.</maml:para>
</maml:description>";
            Assert.That(description.ToSimpleString(), Is.EqualTo(expectedXml));
        }

        [Test]
        public void Command_Parmeters_Parameter_Type_ForTestManualElements()
        {
            TestCommandParameterType(testManualElementsCommandElement, "MandatoryParameter", false, CheckManualClassType);
        }

        [Test]
        public void Command_Parmeters_Parameter_Type_ForTestMamlElements()
        {
            TestCommandParameterType(testMamlElementsCommandElement, "CommonParameter", false, CheckMamlClassType);
        }

        [Test]
        public void Command_Parmeters_Parameter_Type_Array_ForTestManualElements()
        {
            TestCommandParameterType(testManualElementsCommandElement, "ArrayParameter", true, CheckManualClassType);
        }

        [Test]
        public void Command_Parmeters_Parameter_Type_Array_ForTestMamlElements()
        {
            TestCommandParameterType(testMamlElementsCommandElement, "ArrayParameter", true, CheckMamlClassType);
        }

        private void TestCommandParameterType(XElement commandElement, string elementName, bool isArrayType, Action<XElement, bool, bool> checkClassType)
        {
            Assert.That(commandElement, Is.Not.Null);

            var parameter = commandElement.XPathSelectElement($"./command:parameters/command:parameter[maml:name/text() = '{elementName}']", resolver);
            Assert.That(parameter, Is.Not.Null);

            var type = parameter.XPathSelectElement("./dev:type", resolver);
            checkClassType(type, true, isArrayType);
        }

        [Test]
        public void Command_Parameters_Parameter_Type_ReportsUnderlyingTypeForNullableType()
        {
            var expectedFullTypeName = typeof(int).FullName;
            var expectedTypeName = "int"; // returns the terse type name for this one
            var targetName = "NullableParameter";

            Assert.That(testManualElementsCommandElement, Is.Not.Null);

            var parameter = testManualElementsCommandElement.XPathSelectElement("./command:parameters/command:parameter[maml:name/text() = '" + targetName + "']", resolver);
            Assert.That(parameter, Is.Not.Null);

            var fullName = parameter.XPathSelectElement("./command:parameterValue", resolver);
            Assert.That(fullName.Value, Is.EqualTo(expectedTypeName));
            var shortName = parameter.XPathSelectElement("./dev:type/maml:name", resolver);
            Assert.That(shortName.Value, Is.EqualTo(expectedFullTypeName));
        }

        [Test]
        public void Command_Parameters_Parameter_Type_ReportsActualTypeForNonNullableType()
        {
            var expectedFullTypeName = typeof(ManualClass).FullName;
            var expectedTypeName = typeof(ManualClass).Name;
            var targetName = "MandatoryParameter";

            Assert.That(testManualElementsCommandElement, Is.Not.Null);

            var parameter = testManualElementsCommandElement.XPathSelectElement("./command:parameters/command:parameter[maml:name/text() = '" + targetName + "']", resolver);
            Assert.That(parameter, Is.Not.Null);

            var fullName = parameter.XPathSelectElement("./command:parameterValue", resolver);
            Assert.That(fullName.Value, Is.EqualTo(expectedTypeName));
            var shortName = parameter.XPathSelectElement("./dev:type/maml:name", resolver);
            Assert.That(shortName.Value, Is.EqualTo(expectedFullTypeName));
        }

        [Test]
        public void Command_Parameters_DynamicParameter_Reflection()
        {
            Assert.That(testNestedTypeDynamicParametersCommandElement, Is.Not.Null);

            // We expect the static parameter to be documented.
            var staticParameter = testNestedTypeDynamicParametersCommandElement.XPathSelectElement("./command:parameters/command:parameter[maml:name/text() = 'StaticParam']", resolver);
            Assert.That(staticParameter, Is.Not.Null);

            // We also expect the dynamic parameter to be documented.
            var dynamicParameter = testNestedTypeDynamicParametersCommandElement.XPathSelectElement("./command:parameters/command:parameter[maml:name/text() = 'DynamicParam']", resolver);
            Assert.That(dynamicParameter, Is.Not.Null);

            // We don't expect non-parameters on the inner classes to be documented.
            var irrelevantProperty = testNestedTypeDynamicParametersCommandElement.XPathSelectElement("./command:parameters/command:parameter[maml:name/text() = 'IrrelevantProperty']", resolver);
            Assert.That(irrelevantProperty, Is.Null);
        }

        [Test]
        public void Command_Parameters_DynamicParameter_Runtime()
        {
            Assert.That(testRuntimeDynamicParametersCommandElement, Is.Not.Null);

            // We expect the static parameter to be documented.
            var staticParameter = testRuntimeDynamicParametersCommandElement.XPathSelectElement("./command:parameters/command:parameter[maml:name/text() = 'StaticParam']", resolver);
            Assert.That(staticParameter, Is.Not.Null);

            // We also expect the dynamic parameter containing a ParameterAttribute to be documented.
            var dynamicParameter = testRuntimeDynamicParametersCommandElement.XPathSelectElement("./command:parameters/command:parameter[maml:name/text() = 'DynamicParam']", resolver);
            Assert.That(dynamicParameter, Is.Not.Null);

            // We don't expect dynamic parameters missing a ParameterAttribute to be documented, as they
            // are not part of any parameter set.
            var irrelevantParameter = testRuntimeDynamicParametersCommandElement.XPathSelectElement("./command:parameters/command:parameter[maml:name/text() = 'IrrelevantParam']", resolver);
            Assert.That(irrelevantParameter, Is.Null);
        }

        [Test]
        public void Command_Parameters_Parameter_DefaultValue_ForString()
        {
            Assert.That(testDefaultValueCommandElement, Is.Not.Null);

            var parameter = testDefaultValueCommandElement.XPathSelectElement("./command:parameters/command:parameter[maml:name/text() = 'StringParameter']", resolver);
            Assert.That(parameter, Is.Not.Null);

            var defaultValue = parameter.XPathSelectElement("./dev:defaultValue", resolver);
            Assert.That(defaultValue, Is.Not.Null);

            Assert.That(defaultValue.Value, Is.EqualTo("default-string-value"));
        }

        [Test]
        public void Command_Parameters_Parameter_DefaultValue_ForArray()
        {
            Assert.That(testDefaultValueCommandElement, Is.Not.Null);

            var parameter = testDefaultValueCommandElement.XPathSelectElement("./command:parameters/command:parameter[maml:name/text() = 'ArrayParameter']", resolver);
            Assert.That(parameter, Is.Not.Null);

            var defaultValue = parameter.XPathSelectElement("./dev:defaultValue", resolver);
            Assert.That(defaultValue, Is.Not.Null);

            Assert.That(defaultValue.Value, Is.EqualTo("1, 2, 3"));
        }

        [Test]
        public void Command_Parameters_Parameter_DefaultValue_ForList()
        {
            Assert.That(testDefaultValueCommandElement, Is.Not.Null);

            var parameter = testDefaultValueCommandElement.XPathSelectElement("./command:parameters/command:parameter[maml:name/text() = 'ListParameter']", resolver);
            Assert.That(parameter, Is.Not.Null);

            var defaultValue = parameter.XPathSelectElement("./dev:defaultValue", resolver);
            Assert.That(defaultValue, Is.Not.Null);

            Assert.That(defaultValue.Value, Is.EqualTo("1, 2, 3"));
        }

        [Test]
        public void Command_Parameters_Parameter_DefaultValue_ForEnumerable()
        {
            Assert.That(testDefaultValueCommandElement, Is.Not.Null);

            var parameter = testDefaultValueCommandElement.XPathSelectElement("./command:parameters/command:parameter[maml:name/text() = 'EnumerableParameter']", resolver);
            Assert.That(parameter, Is.Not.Null);

            var defaultValue = parameter.XPathSelectElement("./dev:defaultValue", resolver);
            Assert.That(defaultValue, Is.Not.Null);

            Assert.That(defaultValue.Value, Is.EqualTo("1, 2, 3"));
        }

        [Test]
        public void Command_Parameters_Parameter_DefaultValue_ForEmptyEnumerable()
        {
            Assert.That(testDefaultValueCommandElement, Is.Not.Null);

            var parameter = testDefaultValueCommandElement.XPathSelectElement("./command:parameters/command:parameter[maml:name/text() = 'EmptyEnumerableParameter']", resolver);
            Assert.That(parameter, Is.Not.Null);

            var defaultValue = parameter.XPathSelectElement("./dev:defaultValue", resolver);
            Assert.That(defaultValue, Is.Null);
        }

        [Test]
        public void Command_InputTypes_ForTestManualElements()
        {
            Assert.That(testManualElementsCommandElement, Is.Not.Null);

            var inputTypes = testManualElementsCommandElement
                .XPathSelectElements("./command:inputTypes/command:inputType", resolver)
                .ToList();
            Assert.That(inputTypes, Is.Not.Empty);
            Assert.That(inputTypes.Count, Is.EqualTo(3));
            var enumerator = inputTypes.GetEnumerator();

            {
                enumerator.MoveNext();
                var inputType = enumerator.Current;
                var name = inputType.XPathSelectElement("./dev:type/maml:name", resolver);
                Assert.That(name.Value, Is.EqualTo(typeof(string).FullName));
            }

            // allow multiple inputs of the same type
            {
                enumerator.MoveNext();
                var inputType = enumerator.Current;
                var name = inputType.XPathSelectElement("./dev:type/maml:name", resolver);
                Assert.That(name.Value, Is.EqualTo(typeof(string).FullName));
            }

            {
                enumerator.MoveNext();
                var returnValue = enumerator.Current;
                var type = returnValue.XPathSelectElement("./dev:type", resolver);
                CheckManualClassType(type, true, false);

                var description = returnValue.XPathSelectElement("./maml:description", resolver);
                Assert.That(description, Is.Null);
            }
        }

        [Test]
        public void Command_InputTypes_ForTestMamlElements()
        {
            Assert.That(testMamlElementsCommandElement, Is.Not.Null);

            var inputTypes = testMamlElementsCommandElement
                .XPathSelectElements("./command:inputTypes/command:inputType", resolver)
                .ToList();
            Assert.That(inputTypes, Is.Not.Empty);
            Assert.That(inputTypes.Count, Is.EqualTo(2));

            {
                var inputType = inputTypes.First();
                var name = inputType.XPathSelectElement("./dev:type/maml:name", resolver);
                Assert.That(name.Value, Is.EqualTo(typeof(string).FullName));
            }

            {
                var returnValue = inputTypes.Last();
                var type = returnValue.XPathSelectElement("./dev:type", resolver);
                CheckMamlClassType(type, true, false);

                var description = returnValue.XPathSelectElement("./maml:description", resolver);
                Assert.That(description, Is.Null);
            }
        }

        private List<XElement> Command_InputTypes_Setup()
        {
            Assert.That(testInputTypesCommandElement, Is.Not.Null);

            var inputTypes = testInputTypesCommandElement
                .XPathSelectElements("./command:inputTypes/command:inputType", resolver)
                .ToList();
            Assert.That(inputTypes, Is.Not.Empty);
            Assert.That(inputTypes.Count, Is.EqualTo(3));
            return inputTypes;
        }

        [Test]
        public void Command_InputTypes_ExplicitHelpText()
        {
            var inputTypes = Command_InputTypes_Setup();

            // The first input type, ParameterOne is of type InputTypeClass1 and should have an explicit inputType description.
            var inputType = inputTypes[0];

            // Check we've got the right one.
            var name = inputType.XPathSelectElement("./dev:type/maml:name", resolver);
            Assert.That(name.Value, Is.EqualTo(typeof(InputTypeClass1).FullName));

            // Check that there's an explicit description.
            var explicitDescription = inputType.XPathSelectElement("./maml:description", resolver);
            Assert.That(explicitDescription, Is.Not.Null);
            var expectedDescription =
@"<maml:description xmlns:maml=""http://schemas.microsoft.com/maml/2004/10"">
  <maml:para>This is the explicit inputType description for ParameterOne.</maml:para>
</maml:description>";
            Assert.That(explicitDescription.ToSimpleString(), Is.EqualTo(expectedDescription));

            // Check that there's no generic type descrition taken from the InputTypeClass1 comment.
            var genericDescription = inputType.XPathSelectElement("./dev:type/maml:description", resolver);
            Assert.That(genericDescription, Is.Null);
        }

        [Test]
        public void Command_InputTypes_FallBackToTypeDescription()
        {
            var inputTypes = Command_InputTypes_Setup();

            // The second input type, ParameterTwo is of type InputTypeClass2 and doesn't have an explicit inputType description
            // or a parameter description, so should adopt the generic type description instead.
            var inputType = inputTypes[1];

            // Check we've got the right one.
            var name = inputType.XPathSelectElement("./dev:type/maml:name", resolver);
            Assert.That(name.Value, Is.EqualTo(typeof(InputTypeClass2).FullName));

            // Check that there's no explicit description.
            var explicitDescription = inputType.XPathSelectElement("./maml:description", resolver);
            Assert.That(explicitDescription, Is.Null);

            // Check that there's a generic type descrition taken from the InputTypeClass2 comment.
            var genericDescription = inputType.XPathSelectElement("./dev:type/maml:description", resolver);
            Assert.That(genericDescription, Is.Not.Null);
            var expectedDescription =
@"<maml:description xmlns:maml=""http://schemas.microsoft.com/maml/2004/10"">
  <maml:para>InputTypeClass2 description.</maml:para>
</maml:description>";
            Assert.That(genericDescription.ToSimpleString(), Is.EqualTo(expectedDescription));
        }

        [Test]
        public void Command_InputTypes_InheritedParameterDescription()
        {
            var inputTypes = Command_InputTypes_Setup();

            // The third input type, ParameterThree is of type InputTypeClass3 and doesn't have an explicit inputType description,
            // but it can inherit the parameter description instead.
            var inputType = inputTypes[2];

            // Check we've got the right one.
            var name = inputType.XPathSelectElement("./dev:type/maml:name", resolver);
            Assert.That(name.Value, Is.EqualTo(typeof(InputTypeClass3).FullName));

            // Check that there's an explicit description inherited from the parameter description.
            var explicitDescription = inputType.XPathSelectElement("./maml:description", resolver);
            Assert.That(explicitDescription, Is.Not.Null);
            var expectedDescription =
@"<maml:description xmlns:maml=""http://schemas.microsoft.com/maml/2004/10"">
  <maml:para>This is the fallback description for ParameterThree.</maml:para>
</maml:description>";
            Assert.That(explicitDescription.ToSimpleString(), Is.EqualTo(expectedDescription));

            // Check that there's no generic type descrition taken from the InputTypeClass3 comment.
            var genericDescription = inputType.XPathSelectElement("./dev:type/maml:description", resolver);
            Assert.That(genericDescription, Is.Null);
        }

        [Test]
        public void Command_ReturnValues_ForTestManualElements()
        {
            Assert.That(testManualElementsCommandElement, Is.Not.Null);

            var returnValues = testManualElementsCommandElement
                .XPathSelectElements("./command:returnValues/command:returnValue", resolver)
                .ToList();
            Assert.That(returnValues, Is.Not.Empty);
            Assert.That(returnValues.Count, Is.EqualTo(3));

            {
                var returnValue = returnValues[0];
                var name = returnValue.XPathSelectElement("./dev:type/maml:name", resolver);
                Assert.That(name.Value, Is.EqualTo(typeof(string).FullName));
            }

            {
                var returnValue = returnValues[1];
                var name = returnValue.XPathSelectElement("./dev:type/maml:name", resolver);
                Assert.That(name.Value, Is.EqualTo("None"));
            }

            {
                var returnValue = returnValues[2];
                var type = returnValue.XPathSelectElement("./dev:type", resolver);
                CheckManualClassType(type, false, false);

                // Currently the returnValue description is the same as the type description. If we provide another
                // means to specify the description, the following assertion should be changed.
                var description = returnValue.XPathSelectElement("./maml:description", resolver);
                Assert.That(description.ToSimpleString(), Is.EqualTo(ManualClassDescription));
            }
        }

        [Test]
        public void Command_ReturnValues_ForTestMamlElements()
        {
            Assert.That(testMamlElementsCommandElement, Is.Not.Null);

            var returnValues = testMamlElementsCommandElement
                .XPathSelectElements("./command:returnValues/command:returnValue", resolver)
                .ToList();
            Assert.That(returnValues, Is.Not.Empty);
            Assert.That(returnValues.Count, Is.EqualTo(2));

            {
                var returnValue = returnValues.First();
                var name = returnValue.XPathSelectElement("./dev:type/maml:name", resolver);
                Assert.That(name.Value, Is.EqualTo(typeof(string).FullName));
            }

            {
                var returnValue = returnValues.Last();
                var type = returnValue.XPathSelectElement("./dev:type", resolver);
                CheckMamlClassType(type, false, false);

                // Currently the returnValue description is the same as the type description. If we provide another
                // means to specify the description, the following assertion should be changed.
                var description = returnValue.XPathSelectElement("./maml:description", resolver);
                Assert.That(description.ToSimpleString(), Is.EqualTo(MamlClassDescription));
            }
        }

        [Test]
        public void Command_AlertSet_ForTestManualElements()
        {
            Assert.That(testManualElementsCommandElement, Is.Not.Null);

            var alertSet = testManualElementsCommandElement.XPathSelectElement("./maml:alertSet", resolver);
            Assert.That(alertSet, Is.Not.Null);
            Assert.That(alertSet.ToSimpleString(), Is.EqualTo(AlertSet));
        }

        [Test]
        public void Command_AlertSet_ForTestMamlElements()
        {
            Assert.That(testMamlElementsCommandElement, Is.Not.Null);

            var alertSet = testMamlElementsCommandElement.XPathSelectElement("./maml:alertSet", resolver);
            Assert.That(alertSet, Is.Not.Null);
            Assert.That(alertSet.ToSimpleString(), Is.EqualTo(AlertSet));
        }

        private const string AlertSet =
@"<maml:alertSet xmlns:maml=""http://schemas.microsoft.com/maml/2004/10"">
  <maml:title>First Note</maml:title>
  <maml:alert>
    <maml:para>This is the description for the first note.</maml:para>
  </maml:alert>
  <maml:title>Second Note</maml:title>
  <maml:alert>
    <maml:para>This is part of the description for the second note.</maml:para>
    <maml:para>This is also part of the description for the second note.</maml:para>
  </maml:alert>
</maml:alertSet>";

        [Test]
        public void Command_Examples_ForTestManualElements()
        {
            Assert.That(testManualElementsCommandElement, Is.Not.Null);

            var examples = testManualElementsCommandElement.XPathSelectElement("./command:examples", resolver);
            Assert.That(examples, Is.Not.Null);
            Assert.That(examples.ToSimpleString(), Is.EqualTo(Examples));
        }

        private const string Examples =
@"<command:examples xmlns:command=""http://schemas.microsoft.com/maml/dev/command/2004/10"">
  <command:example>
    <maml:title xmlns:maml=""http://schemas.microsoft.com/maml/2004/10"">----------  EXAMPLE 1  ----------</maml:title>
    <maml:introduction xmlns:maml=""http://schemas.microsoft.com/maml/2004/10"">
      <maml:para>This is part of the first example's introduction.</maml:para>
      <maml:para>This is also part of the first example's introduction.</maml:para>
    </maml:introduction>
    <dev:code xmlns:dev=""http://schemas.microsoft.com/maml/dev/2004/10"">New-Thingy | Write-Host</dev:code>
    <dev:remarks xmlns:dev=""http://schemas.microsoft.com/maml/dev/2004/10"">
      <maml:para xmlns:maml=""http://schemas.microsoft.com/maml/2004/10"">This is part of the first example's remarks.</maml:para>
      <maml:para xmlns:maml=""http://schemas.microsoft.com/maml/2004/10"">This is also part of the first example's remarks.</maml:para>
    </dev:remarks>
  </command:example>
  <command:example>
    <maml:title xmlns:maml=""http://schemas.microsoft.com/maml/2004/10"">----------  EXAMPLE 2  ----------</maml:title>
    <maml:introduction xmlns:maml=""http://schemas.microsoft.com/maml/2004/10"">
      <maml:para>This is the second example's introduction.</maml:para>
    </maml:introduction>
    <dev:code xmlns:dev=""http://schemas.microsoft.com/maml/dev/2004/10"">$thingy = New-Thingy
If ($thingy -eq $that) {
  Write-Host 'Same'
} else {
  $thingy | Write-Host
}</dev:code>
    <dev:remarks xmlns:dev=""http://schemas.microsoft.com/maml/dev/2004/10"">
      <maml:para xmlns:maml=""http://schemas.microsoft.com/maml/2004/10"">This is the second example's remarks.</maml:para>
    </dev:remarks>
  </command:example>
  <command:example>
    <maml:title xmlns:maml=""http://schemas.microsoft.com/maml/2004/10"">----------  EXAMPLE 3  ----------</maml:title>
    <dev:code xmlns:dev=""http://schemas.microsoft.com/maml/dev/2004/10"">New-Thingy | Write-Host</dev:code>
    <dev:remarks xmlns:dev=""http://schemas.microsoft.com/maml/dev/2004/10"">
      <maml:para xmlns:maml=""http://schemas.microsoft.com/maml/2004/10"">This is the third example's remarks.</maml:para>
    </dev:remarks>
  </command:example>
  <command:example>
    <maml:title xmlns:maml=""http://schemas.microsoft.com/maml/2004/10"">----------  EXAMPLE 4  ----------</maml:title>
    <maml:introduction xmlns:maml=""http://schemas.microsoft.com/maml/2004/10"">
      <maml:para>This is the fourth example's introduction.</maml:para>
    </maml:introduction>
    <dev:code xmlns:dev=""http://schemas.microsoft.com/maml/dev/2004/10"">New-Thingy | Write-Host</dev:code>
  </command:example>
</command:examples>";

        private void CheckManualClassType(XElement type, bool expectADescription, bool expectArrayType)
        {
            Assert.That(type, Is.Not.Null);

            var name = type.XPathSelectElement("./maml:name", resolver);
            Assert.That(name, Is.Not.Null);

            var expectedTypeName = typeof(ManualClass).FullName;

            if (expectArrayType)
                expectedTypeName = $"{expectedTypeName}[]";

            Assert.That(name.Value, Is.EqualTo(expectedTypeName));

            var description = type.XPathSelectElement("./maml:description", resolver);
            if (expectADescription)
            {
                Assert.That(description, Is.Not.Null);
                Assert.That(description.ToSimpleString(), Is.EqualTo(ManualClassDescription));
            }
            else
            {
                Assert.That(description, Is.Null);
            }
        }

        private const string ManualClassDescription =
@"<maml:description xmlns:maml=""http://schemas.microsoft.com/maml/2004/10"">
  <maml:para>This is part of the ManualClass description.</maml:para>
  <maml:para>This is also part of the ManualClass description.</maml:para>
</maml:description>";

        private void CheckMamlClassType(XElement type, bool expectADescription, bool expectArrayType)
        {
            Assert.That(type, Is.Not.Null);

            var name = type.XPathSelectElement("./maml:name", resolver);
            Assert.That(name, Is.Not.Null);

            var expectedTypeName = typeof(MamlClass).FullName;

            if (expectArrayType)
                expectedTypeName = $"{expectedTypeName}[]";

            Assert.That(name.Value, Is.EqualTo(expectedTypeName));

            var description = type.XPathSelectElement("./maml:description", resolver);
            if (expectADescription)
            {
                Assert.That(description, Is.Not.Null);
                Assert.That(description.ToSimpleString(), Is.EqualTo(MamlClassDescription));
            }
            else
            {
                Assert.That(description, Is.Null);
            }
        }

        private const string MamlClassDescription =
@"<maml:description xmlns:maml=""http://schemas.microsoft.com/maml/2004/10"">
  <maml:para>This is the MamlClass description.</maml:para>
</maml:description>";

        [Test]
        public void Command_Syntax_Obsolete_Parameters_Are_Excluded()
        {
            Assert.That(testManualElementsCommandElement, Is.Not.Null);

            var syntaxItemParameters = testManualElementsCommandElement.XPathSelectElements("./command:syntax/command:syntaxItem/command:parameter/maml:name", resolver).ToList();

            Assert.That(syntaxItemParameters, Is.Not.Empty);
            Assert.That(syntaxItemParameters.Select(x => x.Value).Contains("ObsoleteParameter"), Is.False);
        }

        [Test]
        public void Command_Syntax_Parameters_Ordered_By_Position()
        {
            Assert.That(testPositionedParametersCommandElement, Is.Not.Null);

            var syntaxItemParameters = testPositionedParametersCommandElement.XPathSelectElements("./command:syntax/command:syntaxItem/command:parameter", resolver).ToList();

            Assert.That(syntaxItemParameters, Is.Not.Empty);
            Assert.That(syntaxItemParameters.Count, Is.EqualTo(6));

            var position = syntaxItemParameters[0].Attribute("position").Value;
            Assert.That(position, Is.EqualTo("0"));

            position = syntaxItemParameters[1].Attribute("position").Value;
            Assert.That(position, Is.EqualTo("1"));

            position = syntaxItemParameters[2].Attribute("position").Value;
            Assert.That(position, Is.EqualTo("2"));

            position = syntaxItemParameters[3].Attribute("position").Value;
            Assert.That(position, Is.EqualTo("3"));

            var require = syntaxItemParameters[4].Attribute("required").Value;
            position = syntaxItemParameters[4].Attribute("position").Value;
            Assert.That(position, Is.EqualTo("named"));
            Assert.That(require, Is.EqualTo("true"));

            require = syntaxItemParameters[5].Attribute("required").Value;
            position = syntaxItemParameters[5].Attribute("position").Value;
            Assert.That(position, Is.EqualTo("named"));
            Assert.That(require, Is.EqualTo("false"));
        }

        [Test]
        public void Command_Syntax_Parameterless()
        {
            Assert.That(testParameterlessCommandElement, Is.Not.Null);

            // There shpuld be a single syntaxItem element.
            var syntaxItems = testParameterlessCommandElement.XPathSelectElements("./command:syntax/command:syntaxItem", resolver).ToList();
            Assert.That(syntaxItems, Is.Not.Empty);
            Assert.That(syntaxItems.Count, Is.EqualTo(1));

            // And it should not contain any parameters.
            var syntaxItemParameters = syntaxItems[0].XPathSelectElements("./command:parameter", resolver).ToList();
            Assert.That(syntaxItemParameters, Is.Empty);
        }
    }
}
