using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Jolt;
using XmlDoc2CmdletDoc.Core.Domain;

namespace XmlDoc2CmdletDoc.Core
{
    /// <summary>
    /// <para>Does all the work of generating the XML help file for an assembly. See <see cref="GenerateHelp"/>.</para>
    /// <para>This class is stateless, so you can call <see cref="GenerateHelp"/> multitple times on multiple threads.</para>
    /// </summary>
    /// <remarks>
    /// A lot of the detailed help generation is also delegated to <see cref="XmlDocCommentReaderExtensions"/>.
    /// This class is generally responsible for generating the overall structure of the XML help file, whilst
    /// <see cref="XmlDocCommentReaderExtensions"/> is resonsible for generating specific items of documentation,
    /// such as command synopses, and command and parameter descriptions.
    /// </remarks>
    public class Engine
    {
        private static readonly XNamespace mshNs = XNamespace.Get("http://msh");
        private static readonly XNamespace mamlNs = XNamespace.Get("http://schemas.microsoft.com/maml/2004/10");
        private static readonly XNamespace commandNs = XNamespace.Get("http://schemas.microsoft.com/maml/dev/command/2004/10");
        private static readonly XNamespace devNs = XNamespace.Get("http://schemas.microsoft.com/maml/dev/2004/10");

        /// <summary>
        /// Public entry point that triggers the creation of the cmdlet XML help file for a single assembly.
        /// </summary>
        /// <param name="options">Defines the locations of the input assembly, the input XML doc comments file for the
        /// assembly, and where the cmdlet XML help file should be written to.</param>
        /// <returns>A code indicating the result of the help generation.</returns>
        public EngineErrorCode GenerateHelp(Options options)
        {
            try
            {
                var assembly = LoadAssembly(options);
                var commentReader = LoadComments(options);
                var cmdletTypes = GetCommands(assembly);

                var document = new XDocument(new XDeclaration("1.0", "utf-8", null),
                                             GenerateHelpItemsElement(commentReader, cmdletTypes));

                using (var stream = new FileStream(options.OutputHelpFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    document.Save(writer);
                }

                return EngineErrorCode.Success;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
                var engineException = exception as EngineException;
                return engineException == null
                           ? EngineErrorCode.UnhandledException
                           : engineException.ErrorCode;
            }
        }

        /// <summary>
        /// Loads the assembly indicated in the specified <paramref name="options"/>.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>The assembly indicated in the <paramref name="options"/>.</returns>
        private Assembly LoadAssembly(Options options)
        {
            var assemblyPath = options.AssemblyPath;
            if (!File.Exists(assemblyPath))
            {
                throw new EngineException(EngineErrorCode.AssemblyNotFound,
                                          "Assembly file not found: " + assemblyPath);
            }
            try
            {
                var assemblyDir = Path.GetDirectoryName(assemblyPath);
                AppDomain.CurrentDomain.AssemblyResolve += // TODO: Really ought to track this handler and cleanly remove it.
                    (sender, args) =>
                    {
                        var name = args.Name;
                        var i = name.IndexOf(',');
                        if (i != -1)
                        {
                            name = name.Substring(0, i);
                        }
                        name += ".dll";
                        var path = Path.Combine(assemblyDir, name);
                        return Assembly.LoadFrom(path);
                    };

                return Assembly.LoadFile(assemblyPath);
            }
            catch (Exception exception)
            {
                throw new EngineException(EngineErrorCode.AssemblyLoadError,
                                          "Failed to load assembly from file: " + assemblyPath,
                                          exception);
            }
        }

        /// <summary>
        /// Obtains an XML Doc comment reader for the assembly in the specified <paramref name="options"/>.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>A comment reader for the assembly in the <paramref name="options"/>.</returns>
        private XmlDocCommentReader LoadComments(Options options)
        {
            var docCommentsPath = options.DocCommentsPath;
            if (!File.Exists(docCommentsPath))
            {
                throw new EngineException(EngineErrorCode.AssemblyCommentsNotFound,
                                          "Assembly comments file not found: " + docCommentsPath);
            }
            try
            {
                return new XmlDocCommentReader(docCommentsPath);
            }
            catch (Exception exception)
            {
                throw new EngineException(EngineErrorCode.DocCommentsLoadError,
                                          "Failed to load XML Doc comments ffrom file: " + docCommentsPath,
                                          exception);
            }
        }

        /// <summary>
        /// Retrieves a sequence of <see cref="Command"/> instances, one for each cmdlet defined in the specified <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>A sequence of commands, one for each cmdlet defined in the <paramref name="assembly"/>.</returns>
        private static IEnumerable<Command> GetCommands(Assembly assembly)
        {
            return assembly.GetTypes()
                           .Where(type => type.IsPublic &&
                               typeof(Cmdlet).IsAssignableFrom(type) &&
                               type.GetCustomAttribute<CmdletAttribute>() != null)
                           .Select(type => new Command(type))
                           .OrderBy(command => command.Noun)
                           .ThenBy(command => command.Verb);
        }

        /// <summary>
        /// Generates the root-level <em>&lt;helpItems&gt;</em> element.
        /// </summary>
        /// <param name="commentReader"></param>
        /// <param name="commands">All of the commands in the module being documented.</param>
        /// <returns>The root-level <em>helpItems</em> element.</returns>
        private XElement GenerateHelpItemsElement(XmlDocCommentReader commentReader, IEnumerable<Command> commands)
        {
            var helpItemsElement = new XElement(mshNs + "helpItems", new XAttribute("schema", "maml"));
            foreach (var command in commands)
            {
                helpItemsElement.Add(GenerateComment("Cmdlet: " + command.Name));
                helpItemsElement.Add(GenerateCommandElement(commentReader, command));
            }
            return helpItemsElement;
        }

        /// <summary>
        /// Generates a <em>&lt;command:command&gt;</em> element for the specified command.
        /// </summary>
        /// <param name="commentReader"></param>
        /// <param name="command">The command.</param>
        /// <returns>A <em>&lt;command:command&gt;</em> element that represents the <paramref name="command"/>.</returns>
        private XElement GenerateCommandElement(XmlDocCommentReader commentReader, Command command)
        {
            return new XElement(commandNs + "command",
                                new XAttribute(XNamespace.Xmlns + "maml", mamlNs),
                                new XAttribute(XNamespace.Xmlns + "command", commandNs),
                                new XAttribute(XNamespace.Xmlns + "dev", devNs),
                                GenerateDetailsElement(commentReader, command),
                                GenerateDescriptionElement(commentReader, command),
                                GenerateSyntaxElement(commentReader, command),
                                GenerateParametersElement(commentReader, command),
                                GenerateInputTypesElement(commentReader, command),
                                GenerateReturnValuesElement(commentReader, command),
                                GenerateAlertSetElement(commentReader, command),
                                GenerateExamplesElement(commentReader, command),
                                GenerateRelatedLinksElement(commentReader, command));
        }

        /// <summary>
        /// Generates the <em>&lt;command:details&gt;</em> element for a command.
        /// </summary>
        /// <param name="commentReader"></param>
        /// <param name="command">The command.</param>
        /// <returns>A <em>&lt;command:details&gt;</em> element for the <paramref name="command"/>.</returns>
        private XElement GenerateDetailsElement(XmlDocCommentReader commentReader, Command command)
        {
            return new XElement(commandNs + "details",
                                new XElement(commandNs + "name", command.Name),
                                new XElement(commandNs + "verb", command.Verb),
                                new XElement(commandNs + "noun", command.Noun),
                                commentReader.GetCommandSynopsisElement(command));
        }

        /// <summary>
        /// Generates the <em>&lt;maml:description&gt;</em> element for a command.
        /// </summary>
        /// <param name="commentReader"></param>
        /// <param name="command">The command.</param>
        /// <returns>A <em>&lt;maml:description&gt;</em> element for the <paramref name="command"/>.</returns>
        private XElement GenerateDescriptionElement(XmlDocCommentReader commentReader, Command command)
        {
            return commentReader.GetCommandDescriptionElement(command);
        }

        /// <summary>
        /// Generates the <em>&lt;command:syntax&gt;</em> element for a command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>A <em>&lt;command:syntax&gt;</em> element for the <paramref name="command"/>.</returns>
        private XElement GenerateSyntaxElement(XmlDocCommentReader commentReader, Command command)
        {
            var syntaxElement = new XElement(commandNs + "syntax");
            IEnumerable<string> parameterSetNames = command.ParameterSetNames.ToList();
            if (parameterSetNames.Count() > 1)
            {
                parameterSetNames = parameterSetNames.Where(name => name != ParameterAttribute.AllParameterSets);
            }
            foreach (var parameterSetName in parameterSetNames)
            {
                syntaxElement.Add(GenerateComment("Parameter set: " + parameterSetName));
                syntaxElement.Add(GenerateSyntaxItemElement(commentReader, command, parameterSetName));
            }
            return syntaxElement;
        }

        /// <summary>
        /// Generates the <em>&lt;command:syntaxItem&gt;</em> element for a specific parameter set of a command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parameterSetName">The parameter set name.</param>
        /// <returns>A <em>&lt;command:syntaxItem&gt;</em> element for the specific <paramref name="parameterSetName"/> of the <paramref name="command"/>.</returns>
        private XElement GenerateSyntaxItemElement(XmlDocCommentReader commentReader, Command command, string parameterSetName)
        {
            var syntaxItemElement = new XElement(commandNs + "syntaxItem",
                                                 new XElement(mamlNs + "name", command.Name));
            foreach (var parameter in command.GetParameters(parameterSetName))
            {
                syntaxItemElement.Add(GenerateComment("Parameter: " + parameter.Name));
                syntaxItemElement.Add(GenerateParameterElement(commentReader, parameter, parameterSetName));
            }
            return syntaxItemElement;
        }

        /// <summary>
        /// Generates the <em>&lt;command:parameters&gt;</em> element for a command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>A <em>&lt;command:parameters&gt;</em> element for the <paramref name="command"/>.</returns>
        private XElement GenerateParametersElement(XmlDocCommentReader commentReader, Command command)
        {
            var parametersElement = new XElement(commandNs + "parameters");
            foreach (var parameter in command.Parameters)
            {
                parametersElement.Add(GenerateComment("Parameter: " + parameter.Name));
                parametersElement.Add(GenerateParameterElement(commentReader, parameter));
            }
            return parametersElement;
        }

        /// <summary>
        /// Generates a <em>&lt;command:parameter&gt;</em> element for a single parameter.
        /// </summary>
        /// <param name="commentReader"></param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="parameterSetName">The specific parameter set name, or <see cref="ParameterAttribute.AllParameterSets"/>.</param>
        /// <returns>A <em>&lt;command:parameter&gt;</em> element for the <paramref name="parameter"/>.</returns>
        private XElement GenerateParameterElement(XmlDocCommentReader commentReader, Parameter parameter, string parameterSetName = ParameterAttribute.AllParameterSets)
        {
            return new XElement(commandNs + "parameter",
                                new XAttribute("required", parameter.IsRequired(parameterSetName)),
                                new XAttribute("globbing", parameter.SupportsGlobbing(parameterSetName)),
                                new XAttribute("pipelineInput", parameter.IsPipeline(parameterSetName)),
                                new XAttribute("position", parameter.GetPosition(parameterSetName)),
                                new XElement(mamlNs + "name", parameter.Name),
                                commentReader.GetParameterDescriptionElement(parameter),
                                GenerateTypeElement(commentReader, parameter.ParameterType),
                                commentReader.GetParameterDefaultValueElement(parameter));
        }

        /// <summary>
        /// Generates the <em>&lt;command:inputTypes&gt;</em> element for a command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>A <em>&lt;command:inputTypes&gt;</em> element for the <paramref name="command"/>.</returns>
        private XElement GenerateInputTypesElement(XmlDocCommentReader commentReader, Command command)
        {
            var inputTypesElement = new XElement(commandNs + "inputTypes");
            var pipelineParameters = command.GetParameters(ParameterAttribute.AllParameterSets)
                                            .Where(p => p.IsPipeline(ParameterAttribute.AllParameterSets));
            foreach (var parameterType in pipelineParameters.Select(p => p.ParameterType).Distinct())
            {
                inputTypesElement.Add(GenerateInputTypeElement(commentReader, parameterType));
            }
            return inputTypesElement;
        }

        /// <summary>
        /// Generates the <em>&lt;command:inputType&gt;</em> element for a pipeline parameter.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A <em>&lt;command:inputType&gt;</em> element for the <paramref name="parameter"/>.</returns>
        private XElement GenerateInputTypeElement(XmlDocCommentReader commentReader, Type parameterType)
        {
            return new XElement(commandNs + "inputType",
                                GenerateTypeElement(commentReader, parameterType),
                                commentReader.GetTypeDescriptionElement(parameterType));
        }

        /// <summary>
        /// Generates the <em>&lt;command:returnValues&gt;</em> element for a command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>A <em>&lt;command:returnValues&gt;</em> element for the <paramref name="command"/>.</returns>
        private XElement GenerateReturnValuesElement(XmlDocCommentReader commentReader, Command command)
        {
            var returnValueElement = new XElement(commandNs + "returnValues");
            foreach (var type in command.OutputTypes)
            {
                returnValueElement.Add(GenerateComment("OutputType: " + type.Name));
                returnValueElement.Add(new XElement(commandNs + "returnValue",
                                                    GenerateTypeElement(commentReader, type),
                                                    commentReader.GetTypeDescriptionElement(type)));
            }
            return returnValueElement;
        }

        /// <summary>
        /// Generates the <em>&lt;maml:alertSet&gt;</em> element for a command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>A <em>&lt;maml:alertSet&gt;</em> element for the <paramref name="command"/>.</returns>
        private XElement GenerateAlertSetElement(XmlDocCommentReader commentReader, Command command)
        {
            return commentReader.GetCommandAlertSetElement(command);
        }

        /// <summary>
        /// Generates the <em>&lt;command:examples&gt;</em> element for a command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>A <em>&lt;command:examples&gt;</em> element for the <paramref name="command"/>.</returns>
        private XElement GenerateExamplesElement(XmlDocCommentReader commentReader, Command command)
        {
            return commentReader.GetCommandExamplesElement(command);
        }

        /// <summary>
        /// Generates the <em>&lt;maml:relatedLinks&gt;</em> element for a command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>A <em>&lt;maml:relatedLinks&gt;</em> element for the <paramref name="command"/>.</returns>
        private XElement GenerateRelatedLinksElement(XmlDocCommentReader commentReader, Command command)
        {
            return commentReader.GetCommandRelatedLinksElement(command);
        }

        /// <summary>
        /// Generates a <em>&lt;dev:type&gt;</em> element for a type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private XElement GenerateTypeElement(XmlDocCommentReader commentReader, Type type)
        {
            return new XElement(devNs + "type",
                                new XElement(mamlNs + "name", type.FullName),
                                new XElement(mamlNs + "uri"),
                                commentReader.GetTypeDescriptionElement(type));
        }

        /// <summary>
        /// Creates a comment.
        /// </summary>
        /// <param name="text">The text of the comment.</param>
        /// <returns>An <see cref="XComment"/> instance based on the specified <paramref name="text"/>.</returns>
        private XComment GenerateComment(string text) { return new XComment(string.Format(" {0} ", text)); }
    }
}
