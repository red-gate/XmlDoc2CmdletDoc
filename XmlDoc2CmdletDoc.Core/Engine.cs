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
        public EngineErrorCode GenerateHelp(Options options)
        {
            try
            {
                var assembly = LoadAssembly(options);
                var commentReader = LoadComments(options);
                var cmdletTypes = GetCmdletTypes(assembly);

                var mshNs = XNamespace.Get("http://msh");
                var mamlNs = XNamespace.Get("http://schemas.microsoft.com/maml/2004/10");
                var commandNs = XNamespace.Get("http://schemas.microsoft.com/maml/dev/command/2004/10");
                var devNs = XNamespace.Get("http://schemas.microsoft.com/maml/dev/2004/10");

                var document = new XDocument(new XDeclaration("1.0", "utf-8", null));

                var helpItemsElement = new XElement(mshNs + "helpItems",
                                                    new XAttribute("schema", "maml"));

                foreach (var type in cmdletTypes)
                {
                    var commandElement = new XElement(commandNs + "command",
                                                      new XAttribute(XNamespace.Xmlns + "maml", mamlNs),
                                                      new XAttribute(XNamespace.Xmlns + "command", commandNs),
                                                      new XAttribute(XNamespace.Xmlns + "dev", devNs));
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
    }
}
