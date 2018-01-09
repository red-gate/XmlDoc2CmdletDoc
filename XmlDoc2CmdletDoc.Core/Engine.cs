using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using XmlDoc2CmdletDoc.Core.Comments;
using XmlDoc2CmdletDoc.Core.Domain;
using XmlDoc2CmdletDoc.Core.Extensions;

namespace XmlDoc2CmdletDoc.Core
{
    /// <summary>
    /// Delegate used when reporting a warning against a reflected member.
    /// </summary>
    /// <param name="target">The reflected meber to which the warning pertains.</param>
    /// <param name="warningText">The warning message.</param>
    public delegate void ReportWarning(MemberInfo target, string warningText);

    /// <summary>
    /// <para>Does all the work of generating the XML help file for an assembly. See <see cref="GenerateHelp"/>.</para>
    /// <para>This class is stateless, so you can call <see cref="GenerateHelp"/> multitple times on multiple threads.</para>
    /// </summary>
    /// <remarks>
    /// A lot of the detailed help generation is also delegated to <see cref="CommentReaderExtensions"/>.
    /// This class is generally responsible for generating the overall structure of the XML help file, whilst
    /// <see cref="CommentReaderExtensions"/> is resonsible for generating specific items of documentation,
    /// such as command synopses, and command and parameter descriptions.
    /// </remarks>
    public class Engine
    {
        private static readonly XNamespace MshNs = XNamespace.Get("http://msh");
        private static readonly XNamespace MamlNs = XNamespace.Get("http://schemas.microsoft.com/maml/2004/10");
        private static readonly XNamespace CommandNs = XNamespace.Get("http://schemas.microsoft.com/maml/dev/command/2004/10");
        private static readonly XNamespace DevNs = XNamespace.Get("http://schemas.microsoft.com/maml/dev/2004/10");

        /// <summary>
        /// Public entry point that triggers the creation of the cmdlet XML help file for a single assembly.
        /// </summary>
        /// <param name="options">Defines the locations of the input assembly, the input XML doc comments file for the
        /// assembly, and where the cmdlet XML help file should be written to.</param>
        /// <returns>A code indicating the result of the help generation.</returns>
        public EngineExitCode GenerateHelp(Options options)
        {
            try
            {
                var warnings = new List<Tuple<MemberInfo, string>>();
                ReportWarning reportWarning = (target, warningText) => warnings.Add(Tuple.Create(target, warningText));

                var assembly = LoadAssembly(options);
                var commentReader = LoadComments(options, reportWarning);
                var cmdletTypes = GetCommands(assembly);


                var document = new XDocument(new XDeclaration("1.0", "utf-8", null),
                                             GenerateHelpItemsElement(commentReader, cmdletTypes, reportWarning, options));

                HandleWarnings(options, warnings, assembly);

                using (var stream = new FileStream(options.OutputHelpFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    document.Save(writer);
                }

                return EngineExitCode.Success;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
                if (exception is ReflectionTypeLoadException typeLoadException)
                {
                    foreach (var loaderException in typeLoadException.LoaderExceptions)
                    {
                        Console.Error.WriteLine("Loader exception: {0}", loaderException);
                    }
                }
                var engineException = exception as EngineException;
                return engineException?.ExitCode ?? EngineExitCode.UnhandledException;
            }
        }

        /// <summary>
        /// Handles the list of warnings generated once the cmdlet help XML document has been generated.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="warnings">The warnings generated during the creation of the cmdlet help XML document. Each tuple
        /// consists of the type to which the warning pertains, and the text of the warning.</param>
        /// <param name="targetAssembly">The assembly of the PowerShell module being documented.</param>
        private static void HandleWarnings(Options options, IEnumerable<Tuple<MemberInfo, string>> warnings, Assembly targetAssembly)
        {
            var groups = warnings.Where(tuple =>
                                        {
                                            // Exclude warnings about types outside of the assembly being documented.
                                            var type = tuple.Item1 as Type ?? tuple.Item1.DeclaringType;
                                            return type != null && type.Assembly == targetAssembly;
                                        })
                                 .GroupBy(tuple => GetFullyQualifiedName(tuple.Item1), tuple => tuple.Item2)
                                 .OrderBy(group => group.Key)
                                 .ToList();
            if (groups.Any())
            {
                var writer = options.TreatWarningsAsErrors ? Console.Error : Console.Out;
                writer.WriteLine("Warnings:");
                foreach (var group in groups)
                {
                    writer.WriteLine("    {0}:", group.Key);
                    foreach (var warningText in group.Distinct())
                    {
                        writer.WriteLine("        {0}", warningText);
                    }
                }
                if (options.TreatWarningsAsErrors)
                {
                    throw new EngineException(EngineExitCode.WarningsAsErrors,
                                              "Failing due to the occurence of one or more warnings");
                }
            }
        }

        /// <summary>
        /// Returns the fully-qualified name of a type, property or field.
        /// </summary>
        /// <param name="memberInfo">The member.</param>
        /// <returns>The fully qualified name of the mamber.</returns>
        private static string GetFullyQualifiedName(MemberInfo memberInfo)
        {
            var type = memberInfo as Type;
            return type != null
                ? type.FullName
                : $"{GetFullyQualifiedName(memberInfo.DeclaringType)}.{memberInfo.Name}";
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
                throw new EngineException(EngineExitCode.AssemblyNotFound,
                                          "Assembly file not found: " + assemblyPath);
            }
            try
            {
                var assemblyDir = Path.GetDirectoryName(assemblyPath) ?? "";
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
                throw new EngineException(EngineExitCode.AssemblyLoadError,
                                          "Failed to load assembly from file: " + assemblyPath,
                                          exception);
            }
        }

        /// <summary>
        /// Obtains an XML Doc comment reader for the assembly in the specified <paramref name="options"/>.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="reportWarning">Function used to log warnings.</param>
        /// <returns>A comment reader for the assembly in the <paramref name="options"/>.</returns>
        private ICommentReader LoadComments(Options options, ReportWarning reportWarning)
        {
            var docCommentsPath = options.DocCommentsPath;
            if (!File.Exists(docCommentsPath))
            {
                throw new EngineException(EngineExitCode.AssemblyCommentsNotFound,
                                          "Assembly comments file not found: " + docCommentsPath);
            }
            try
            {
                return new CachingCommentReader(
                    new RewritingCommentReader(
                        new LoggingCommentReader(
                            new JoltCommentReader(docCommentsPath), reportWarning)));
            }
            catch (Exception exception)
            {
                throw new EngineException(EngineExitCode.DocCommentsLoadError,
                                          "Failed to load XML Doc comments from file: " + docCommentsPath,
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
        /// <param name="reportWarning">Function used to log warnings.</param>
        /// <param name="options">The XmlDoc2CmdletDoc options.</param>
        /// <returns>The root-level <em>helpItems</em> element.</returns>
        private XElement GenerateHelpItemsElement(ICommentReader commentReader, IEnumerable<Command> commands, ReportWarning reportWarning, Options options)
        {
            var helpItemsElement = new XElement(MshNs + "helpItems", new XAttribute("schema", "maml"));
            foreach (var command in commands)
            {
                helpItemsElement.Add(GenerateComment("Cmdlet: " + command.Name));
                helpItemsElement.Add(GenerateCommandElement(commentReader, command, reportWarning, options));
            }
            return helpItemsElement;
        }

        /// <summary>
        /// Generates a <em>&lt;command:command&gt;</em> element for the specified command.
        /// </summary>
        /// <param name="commentReader"></param>
        /// <param name="command">The command.</param>
        /// <param name="reportWarning">Function used to log warnings.</param>
        /// <param name="options">The XmlDoc2CmdletDoc options.</param>
        /// <returns>A <em>&lt;command:command&gt;</em> element that represents the <paramref name="command"/>.</returns>
        private XElement GenerateCommandElement(ICommentReader commentReader, Command command, ReportWarning reportWarning, Options options)
        {
            return new XElement(CommandNs + "command",
                                new XAttribute(XNamespace.Xmlns + "maml", MamlNs),
                                new XAttribute(XNamespace.Xmlns + "command", CommandNs),
                                new XAttribute(XNamespace.Xmlns + "dev", DevNs),
                                GenerateDetailsElement(commentReader, command, reportWarning),
                                GenerateDescriptionElement(commentReader, command, reportWarning),
                                GenerateSyntaxElement(commentReader, command, reportWarning, options.IsExcludedParameterSetName),
                                GenerateParametersElement(commentReader, command, reportWarning),
                                GenerateInputTypesElement(commentReader, command, reportWarning),
                                GenerateReturnValuesElement(commentReader, command, reportWarning),
                                GenerateAlertSetElement(commentReader, command, reportWarning),
                                GenerateExamplesElement(commentReader, command, reportWarning),
                                GenerateRelatedLinksElement(commentReader, command, reportWarning));
        }

        /// <summary>
        /// Generates the <em>&lt;command:details&gt;</em> element for a command.
        /// </summary>
        /// <param name="commentReader"></param>
        /// <param name="command">The command.</param>
        /// <param name="reportWarning">Function used to log warnings.</param>
        /// <returns>A <em>&lt;command:details&gt;</em> element for the <paramref name="command"/>.</returns>
        private XElement GenerateDetailsElement(ICommentReader commentReader, Command command, ReportWarning reportWarning)
        {
            return new XElement(CommandNs + "details",
                                new XElement(CommandNs + "name", command.Name),
                                new XElement(CommandNs + "verb", command.Verb),
                                new XElement(CommandNs + "noun", command.Noun),
                                commentReader.GetCommandSynopsisElement(command, reportWarning));
        }

        /// <summary>
        /// Generates the <em>&lt;maml:description&gt;</em> element for a command.
        /// </summary>
        /// <param name="commentReader"></param>
        /// <param name="command">The command.</param>
        /// <param name="reportWarning">Function used to log warnings.</param>
        /// <returns>A <em>&lt;maml:description&gt;</em> element for the <paramref name="command"/>.</returns>
        private XElement GenerateDescriptionElement(ICommentReader commentReader, Command command, ReportWarning reportWarning)
        {
            return commentReader.GetCommandDescriptionElement(command, reportWarning);
        }

        /// <summary>
        /// Generates the <em>&lt;command:syntax&gt;</em> element for a command.
        /// </summary>
        /// <param name="commentReader">Provides access to the XML Doc comments.</param>
        /// <param name="command">The command.</param>
        /// <param name="reportWarning">Function used to log warnings.</param>
        /// <param name="isExcludedParameterSetName">Determines whether or not to exclude a parameter set from the cmdlet XML Help file, based on its name.</param>
        /// <returns>A <em>&lt;command:syntax&gt;</em> element for the <paramref name="command"/>.</returns>
        private XElement GenerateSyntaxElement(ICommentReader commentReader, Command command, ReportWarning reportWarning, Predicate<string> isExcludedParameterSetName)
        {
            var syntaxElement = new XElement(CommandNs + "syntax");
            IEnumerable<string> parameterSetNames = command.ParameterSetNames.ToList();
            if (parameterSetNames.Count() > 1)
            {
                parameterSetNames = parameterSetNames.Where(name => name != ParameterAttribute.AllParameterSets);
            }
            foreach (var parameterSetName in parameterSetNames.Where(name => !isExcludedParameterSetName(name)))
            {
                syntaxElement.Add(GenerateComment("Parameter set: " + parameterSetName));
                syntaxElement.Add(GenerateSyntaxItemElement(commentReader, command, parameterSetName, reportWarning));
            }
            return syntaxElement;
        }

        /// <summary>
        /// Generates the <em>&lt;command:syntaxItem&gt;</em> element for a specific parameter set of a command.
        /// </summary>
        /// <param name="commentReader">Provides access to the XML Doc comments.</param>
        /// <param name="command">The command.</param>
        /// <param name="parameterSetName">The parameter set name.</param>
        /// <param name="reportWarning">Function used to log warnings.</param>
        /// <returns>A <em>&lt;command:syntaxItem&gt;</em> element for the specific <paramref name="parameterSetName"/> of the <paramref name="command"/>.</returns>
        private XElement GenerateSyntaxItemElement(ICommentReader commentReader, Command command, string parameterSetName, ReportWarning reportWarning)
        {
            var syntaxItemElement = new XElement(CommandNs + "syntaxItem",
                                                 new XElement(MamlNs + "name", command.Name));
            foreach (var parameter in command.GetParameters(parameterSetName).OrderBy(p => p.GetPosition(parameterSetName)).
                                                                              ThenBy(p => p.IsRequired(parameterSetName) ? "0" : "1").
                                                                              ThenBy(p => p.Name))
            {
                syntaxItemElement.Add(GenerateComment("Parameter: " + parameter.Name));
                syntaxItemElement.Add(GenerateParameterElement(commentReader, parameter, parameterSetName, reportWarning));
            }
            return syntaxItemElement;
        }

        /// <summary>
        /// Generates the <em>&lt;command:parameters&gt;</em> element for a command.
        /// </summary>
        /// <param name="commentReader">Provides access to the XML Doc comments.</param>
        /// <param name="command">The command.</param>
        /// <param name="reportWarning">Function used to log warnings.</param>
        /// <returns>A <em>&lt;command:parameters&gt;</em> element for the <paramref name="command"/>.</returns>
        private XElement GenerateParametersElement(ICommentReader commentReader, Command command, ReportWarning reportWarning)
        {
            var parametersElement = new XElement(CommandNs + "parameters");
            foreach (var parameter in command.Parameters)
            {
                parametersElement.Add(GenerateComment("Parameter: " + parameter.Name));
                parametersElement.Add(GenerateParameterElement(commentReader, parameter, ParameterAttribute.AllParameterSets, reportWarning));
                GenerateAliasElements(commentReader, reportWarning, parameter, parametersElement);
            }
            return parametersElement;
        }

        // Because the proper aliases generated in GenerateParameterElement are not manifested by Get-Help,
        // this simply duplicates parameters that have aliases, substituting in the alias name.
        // Thus, one could do Get-Help xyz -param actualName or Get-Help xyz -param aliasName
        private void GenerateAliasElements(ICommentReader commentReader, ReportWarning reportWarning, Parameter parameter, XElement parametersElement)
        {
            foreach (var alias in parameter.Aliases)
            {
                var parameterElement = GenerateParameterElement(commentReader, parameter, ParameterAttribute.AllParameterSets, reportWarning);
                parametersElement.Add(parameterElement);
                var nameElement = (XElement)(parameterElement.Nodes().First(n => ((XElement)n).Name == MamlNs + "name"));
                nameElement.Value = alias;
                var descriptionElement = (XElement)(parameterElement.Nodes().FirstOrDefault(n => ((XElement)n).Name == MamlNs + "description"));
                if (descriptionElement == null)
                {
                    descriptionElement = new XElement(MamlNs + "description");
                    parameterElement.Add(descriptionElement);
                }
                descriptionElement.Add(new XElement(MamlNs + "para", $"This is an alias of the {parameter.Name} parameter."));
            }
        }

        /// <summary>
        /// Generates a <em>&lt;command:parameter&gt;</em> element for a single parameter.
        /// </summary>
        /// <param name="commentReader">Provides access to the XML Doc comments.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="parameterSetName">The specific parameter set name, or <see cref="ParameterAttribute.AllParameterSets"/>.</param>
        /// <param name="reportWarning">Function used to log warnings.</param>
        /// <returns>A <em>&lt;command:parameter&gt;</em> element for the <paramref name="parameter"/>.</returns>
        private XElement GenerateParameterElement(ICommentReader commentReader, Parameter parameter, string parameterSetName, ReportWarning reportWarning)
        {
            var element = new XElement(CommandNs + "parameter",
                                new XAttribute("required", parameter.IsRequired(parameterSetName)),
                                new XAttribute("globbing", parameter.SupportsGlobbing(parameterSetName)),
                                new XAttribute("pipelineInput", parameter.GetIsPipelineAttribute(parameterSetName)),
                                new XAttribute("position", parameter.GetPosition(parameterSetName)),
                                new XElement(MamlNs + "name", parameter.Name),
                                GenerateDescriptionElement(commentReader, parameter, reportWarning),
                                commentReader.GetParameterValueElement(parameter, reportWarning),
                                GenerateTypeElement(commentReader, parameter.ParameterType, true, reportWarning),
                                commentReader.GetParameterDefaultValueElement(parameter, reportWarning),
                                GetParameterEnumeratedValuesElement(parameter));
            var aliasNames = parameter.Aliases.ToList();
            if (aliasNames.Count > 0)
            {
                element.Add(new XAttribute("aliases", string.Join(",", aliasNames)));
            }
            return element;
        }

        /// <summary>
        /// Fetch the description from the ICommentReader.
        /// If the parameter is an Enum, add to the description a list of its legal values.
        /// </summary>
        private static XElement GenerateDescriptionElement(ICommentReader commentReader, Parameter parameter, ReportWarning reportWarning)
        {
            var descriptionElement = commentReader.GetParameterDescriptionElement(parameter, reportWarning);
            if (parameter.EnumValues.Any())
            {
                if (descriptionElement == null)
                {
                    descriptionElement = new XElement(MamlNs + "description");
                }
                descriptionElement.Add(
                    new XElement(MamlNs + "para",
                                 "Possible values: " + string.Join(", ", parameter.EnumValues)));
            }
            return descriptionElement;
        }

        /// <summary>
        /// Generates a <em>&lt;command:parameterValueGroup&gt;</em> element for a parameter
        /// in order to display enum choices in the cmdlet's syntax section.
        /// </summary>
        private XElement GetParameterEnumeratedValuesElement(Parameter parameter)
        {
            var enumValues = parameter.EnumValues.ToList();
            if (enumValues.Any())
            {
                var parameterValueGroupElement = new XElement(CommandNs + "parameterValueGroup");
                foreach (var enumValue in enumValues)
                {
                    parameterValueGroupElement.Add(GenerateParameterEnumeratedValueElement(enumValue));
                }
                return parameterValueGroupElement;
            }
            return null;
        }

        /// <summary>
        /// Generates a <em>&lt;command:parameterValue&gt;</em> element for a single enum value.
        /// </summary>
        private XElement GenerateParameterEnumeratedValueElement(string enumValue)
        {
            // These hard-coded attributes were copied from what PowerShell's own core cmdlets use
            return new XElement(CommandNs + "parameterValue",
                                new XAttribute("required", false),
                                new XAttribute("variableLength", false),
                                enumValue);
        }

        /// <summary>
        /// Generates the <em>&lt;command:inputTypes&gt;</em> element for a command.
        /// </summary>
        /// <param name="commentReader">Provides access to the XML Doc comments.</param>
        /// <param name="command">The command.</param>
        /// <param name="reportWarning">Function used to log warnings.</param>
        /// <returns>A <em>&lt;command:inputTypes&gt;</em> element for the <paramref name="command"/>.</returns>
        private XElement GenerateInputTypesElement(ICommentReader commentReader, Command command, ReportWarning reportWarning)
        {
            var inputTypesElement = new XElement(CommandNs + "inputTypes");
            var pipelineParameters = command.GetParameters(ParameterAttribute.AllParameterSets)
                                            .Where(p => p.IsPipeline(ParameterAttribute.AllParameterSets));
            foreach (var parameter in pipelineParameters)
            {
                inputTypesElement.Add(GenerateInputTypeElement(commentReader, parameter, reportWarning));
            }
            return inputTypesElement;
        }

        /// <summary>
        /// Generates the <em>&lt;command:inputType&gt;</em> element for a pipeline parameter.
        /// </summary>
        /// <param name="commentReader">Provides access to the XML Doc comments.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="reportWarning">Function used to log warnings.</param>
        /// <returns>A <em>&lt;command:inputType&gt;</em> element for the <paramref name="parameter"/>'s type.</returns>
        private XElement GenerateInputTypeElement(ICommentReader commentReader, Parameter parameter, ReportWarning reportWarning)
        {
            var inputTypeDescription = commentReader.GetInputTypeDescriptionElement(parameter, reportWarning);
            return new XElement(CommandNs + "inputType",
                                GenerateTypeElement(commentReader, parameter.ParameterType, inputTypeDescription == null, reportWarning),
                                inputTypeDescription);
        }

        /// <summary>
        /// Generates the <em>&lt;command:returnValues&gt;</em> element for a command.
        /// </summary>
        /// <param name="commentReader">Provides access to the XML Doc comments.</param>
        /// <param name="command">The command.</param>
        /// <param name="reportWarning">Function used to log warnings.</param>
        /// <returns>A <em>&lt;command:returnValues&gt;</em> element for the <paramref name="command"/>.</returns>
        private XElement GenerateReturnValuesElement(ICommentReader commentReader, Command command, ReportWarning reportWarning)
        {
            var returnValueElement = new XElement(CommandNs + "returnValues");
            foreach (var type in command.OutputTypes)
            {
                returnValueElement.Add(GenerateComment("OutputType: " + (type == typeof(void) ? "None" : type.Name)));
                var returnValueDescription = commentReader.GetOutputTypeDescriptionElement(command, type, reportWarning);
                returnValueElement.Add(new XElement(CommandNs + "returnValue",
                                                    GenerateTypeElement(commentReader, type, returnValueDescription == null, reportWarning),
                                                    returnValueDescription));
            }
            return returnValueElement;
        }

        /// <summary>
        /// Generates the <em>&lt;maml:alertSet&gt;</em> element for a command.
        /// </summary>
        /// <param name="commentReader">Provides access to the XML Doc comments.</param>
        /// <param name="command">The command.</param>
        /// <param name="reportWarning">Function used to log warnings.</param>
        /// <returns>A <em>&lt;maml:alertSet&gt;</em> element for the <paramref name="command"/>.</returns>
        private XElement GenerateAlertSetElement(ICommentReader commentReader, Command command, ReportWarning reportWarning)
        {
            return commentReader.GetCommandAlertSetElement(command, reportWarning);
        }

        /// <summary>
        /// Generates the <em>&lt;command:examples&gt;</em> element for a command.
        /// </summary>
        /// <param name="commentReader">Provides access to the XML Doc comments.</param>
        /// <param name="command">The command.</param>
        /// <param name="reportWarning">Function used to log warnings.</param>
        /// <returns>A <em>&lt;command:examples&gt;</em> element for the <paramref name="command"/>.</returns>
        private XElement GenerateExamplesElement(ICommentReader commentReader, Command command, ReportWarning reportWarning)
        {
            return commentReader.GetCommandExamplesElement(command, reportWarning);
        }

        /// <summary>
        /// Generates the <em>&lt;maml:relatedLinks&gt;</em> element for a command.
        /// </summary>
        /// <param name="commentReader">Provides access to the XML Doc comments.</param>
        /// <param name="command">The command.</param>
        /// <param name="reportWarning">Function used to log warnings.</param>
        /// <returns>A <em>&lt;maml:relatedLinks&gt;</em> element for the <paramref name="command"/>.</returns>
        private XElement GenerateRelatedLinksElement(ICommentReader commentReader, Command command, ReportWarning reportWarning)
        {
            return commentReader.GetCommandRelatedLinksElement(command, reportWarning);
        }

        /// <summary>
        /// Generates a <em>&lt;dev:type&gt;</em> element for a type.
        /// </summary>
        /// <param name="commentReader">Provides access to the XML Doc comments.</param>
        /// <param name="type">The type for which a corresopnding <em>&lt;dev:type&gt;</em> element is required.</param>
        /// <param name="includeMamlDescription">Indicates whether or not a <em>&lt;maml:description&gt;</em> element should be
        /// included for the type. A description can be obtained from the type's XML Doc comment, but it is useful to suppress it if
        /// a more context-specific description is available where the <em>&lt;dev:type&gt;</em> element is actually used.</param>
        /// <param name="reportWarning">Function used to log warnings.</param>
        /// <returns>A <em>&lt;dev:type&gt;</em> element for the specified <paramref name="type"/>.</returns>
        private XElement GenerateTypeElement(ICommentReader commentReader, Type type, bool includeMamlDescription, ReportWarning reportWarning)
        {
            return new XElement(DevNs + "type",
                                new XElement(MamlNs + "name", type == typeof(void) ? "None" : type.FullName),
                                new XElement(MamlNs + "uri"),
                                includeMamlDescription ? commentReader.GetTypeDescriptionElement(type, reportWarning) : null);
        }

        /// <summary>
        /// Creates a comment.
        /// </summary>
        /// <param name="text">The text of the comment.</param>
        /// <returns>An <see cref="XComment"/> instance based on the specified <paramref name="text"/>.</returns>
        private XComment GenerateComment(string text) { return new XComment($" {text} "); }
    }
}
