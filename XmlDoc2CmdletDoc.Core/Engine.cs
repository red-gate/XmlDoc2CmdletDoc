using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
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
                Console.WriteLine("Cmdlet types:");
                foreach (var type in cmdletTypes)
                {
                    Console.WriteLine("    " + type.FullName);
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
                                          type.IsAssignableFrom(typeof(Cmdlet)) &&
                                          type.GetCustomAttribute<CmdletAttribute>() != null)
                           .OrderBy(type => type.FullName);
        }
    }
}
