using System;
using System.IO;

namespace XmlDoc2CmdletDoc.Core
{
    /// <summary>
    /// Represents the settings neccesary for generating the cmdlet XML help file for a single assembly.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// The absolute path of the cmdlet assembly.
        /// </summary>
        public readonly string AssemblyPath;

        /// <summary>
        /// The absolute path of the output <c>.dll-Help.xml</c> help file.
        /// </summary>
        public readonly string OutputHelpFilePath;

        /// <summary>
        /// The absolute path of the assembly's XML Doc comments file.
        /// </summary>
        public readonly string DocCommentsPath;

        /// <summary>
        /// Creates a new instance with the specified settings.
        /// </summary>
        /// <param name="assemblyPath">The path of the taget assembly whose XML Doc comments file is to be converted
        /// into a cmdlet XML Help file.</param>
        /// <param name="outputHelpFilePath">The output path of the cmdlet XML Help file.
        /// If <c>null</c>, an appropriate default is selected based on <paramref name="assemblyPath"/>.</param>
        /// <param name="docCommentsPath">The path of the XML Doc comments file for the target assembly.
        /// If <c>null</c>, an appropriate default is selected based on <paramref name="assemblyPath"/></param>
        public Options(string assemblyPath,
                       string outputHelpFilePath = null,
                       string docCommentsPath = null)
        {
            if (assemblyPath == null) throw new ArgumentNullException("assemblyPath");

            AssemblyPath = Path.GetFullPath(assemblyPath);

            OutputHelpFilePath = outputHelpFilePath == null
                                     ? Path.ChangeExtension(AssemblyPath, "dll-Help.xml")
                                     : Path.GetFullPath(outputHelpFilePath);

            DocCommentsPath = docCommentsPath == null
                                  ? Path.ChangeExtension(AssemblyPath, ".xml")
                                  : Path.GetFullPath(docCommentsPath);
        }

        public override string ToString()
        {
            return string.Format("AssemblyPath: {0}, OutputHelpFilePath: {1}, DocCommentsPath: {2}",
                                 AssemblyPath, OutputHelpFilePath, DocCommentsPath);
        }
    }
}