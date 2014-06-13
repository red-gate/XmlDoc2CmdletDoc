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

namespace XmlDoc2CmdletDoc.Core
{
    public class Engine
    {
        private readonly XNamespace mshNs = XNamespace.Get("http://msh");
        private readonly XNamespace mamlNs = XNamespace.Get("http://schemas.microsoft.com/maml/2004/10");
        private readonly XNamespace commandNs = XNamespace.Get("http://schemas.microsoft.com/maml/dev/command/2004/10");
        private readonly XNamespace devNs = XNamespace.Get("http://schemas.microsoft.com/maml/dev/2004/10");


        public EngineErrorCode GenerateHelp(Options options)
        {
            try
            {
                var assembly = LoadAssembly(options);
                var commentReader = LoadComments(options);
                var cmdletTypes = GetCmdletTypes(assembly);

                var document = new XDocument(new XDeclaration("1.0", "utf-8", null));
                var helpItemsElement = new XElement(mshNs + "helpItems", new XAttribute("schema", "maml"));
                foreach (var type in cmdletTypes)
                {
                    var commandElement = GenerateCommandElement(type);
                    helpItemsElement.Add(commandElement);
                }
                document.Add(helpItemsElement);

                using (var stream = new FileStream(options.OutputHelpFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    document.Save(writer);
                }

                return EngineErrorCode.Success;
            }
            catch (EngineException exception)
            {
                Console.Error.WriteLine(exception);
                return exception.ErrorCode;
            }
        }

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

        private static IEnumerable<Type> GetCmdletTypes(Assembly assembly)
        {
            return assembly.GetTypes()
                           .Where(type => type.IsPublic &&
                                          typeof(Cmdlet).IsAssignableFrom(type) &&
                                          type.GetCustomAttribute<CmdletAttribute>() != null)
                           .OrderBy(type => type.FullName);
        }

        private XElement GenerateCommandElement(Type cmdletType)
        {
            var cmdletAttribute = cmdletType.GetCustomAttribute<CmdletAttribute>();
            var verb = cmdletAttribute.VerbName;
            var noun = cmdletAttribute.NounName;
            var name = string.Format("{0}-{1}", verb, noun);

            var commandElement = new XElement(commandNs + "command",
                                              new XAttribute(XNamespace.Xmlns + "maml", mamlNs),
                                              new XAttribute(XNamespace.Xmlns + "command", commandNs),
                                              new XAttribute(XNamespace.Xmlns + "dev", devNs));
            var detailsElement = new XElement(commandNs + "details",
                                              new XElement(commandNs + "name", new XText(name)),
                                              new XElement(commandNs + "verb", new XText(verb)),
                                              new XElement(commandNs + "noun", new XText(noun)));
            commandElement.Add(detailsElement);
            commandElement.Add(GenerateReturnValueElement(cmdletType));
            return commandElement;
        }

        private XElement GenerateReturnValueElement(Type cmdletType)
        {
            var returnValueElement = new XElement(commandNs + "returnValues");
            foreach (var outputTypeAttribute in cmdletType.GetCustomAttributes<OutputTypeAttribute>())
            {
                returnValueElement.Add(new XElement(commandNs + "returnValue",
                                                    GenerateTypeElement(outputTypeAttribute.Type.First())));
            }
            return returnValueElement;
        }

        private XElement GenerateTypeElement(PSTypeName type)
        {
            return new XElement(devNs + "type",
                                new XElement(mamlNs + "name", type.Name));
        }
    }
}
