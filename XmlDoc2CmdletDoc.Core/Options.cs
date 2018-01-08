using System;
using System.Collections.Generic;
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
        /// Indicates whether or not the presence of warnings should be treated as a failure condition.
        /// </summary>
        public readonly bool TreatWarningsAsErrors;

        /// <summary>
        /// A list of parameter sets that should be excluded from the cmdlet XML Help file.
        /// </summary>
        public readonly ISet<string> ExcludedParameterSets;

        /// <summary>
        /// Creates a new instance with the specified settings.
        /// </summary>
        /// <param name="treatWarningsAsErrors">Indicates whether or not the presence of warnings should be treated as a failure condition.</param>
        /// <param name="assemblyPath">The path of the taget assembly whose XML Doc comments file is to be converted
        /// into a cmdlet XML Help file.</param>
        /// <param name="excludedParameterSets">A list of parameter sets that should be excluded from the cmdlet XML Help file.
        /// This is intended to be used for deprecated parameter sets, to make them less discoverable.</param>
        /// <param name="outputHelpFilePath">The output path of the cmdlet XML Help file.
        /// If <c>null</c>, an appropriate default is selected based on <paramref name="assemblyPath"/>.</param>
        /// <param name="docCommentsPath">The path of the XML Doc comments file for the target assembly.
        /// If <c>null</c>, an appropriate default is selected based on <paramref name="assemblyPath"/></param>
        public Options(
            bool treatWarningsAsErrors,
            string assemblyPath,
            IReadOnlyCollection<string> excludedParameterSets,
            string outputHelpFilePath = null,
            string docCommentsPath = null)
        {
            if (assemblyPath == null) throw new ArgumentNullException(nameof(assemblyPath));

            TreatWarningsAsErrors = treatWarningsAsErrors;

            AssemblyPath = Path.GetFullPath(assemblyPath);

            ExcludedParameterSets = new HashSet<string>(excludedParameterSets);

            OutputHelpFilePath = outputHelpFilePath == null
                                     ? Path.ChangeExtension(AssemblyPath, "dll-Help.xml")
                                     : Path.GetFullPath(outputHelpFilePath);

            DocCommentsPath = docCommentsPath == null
                                  ? Path.ChangeExtension(AssemblyPath, ".xml")
                                  : Path.GetFullPath(docCommentsPath);
        }

        /// <summary>
        /// Provides a string representation of the options, for logging and debug purposes.
        /// </summary>
        public override string ToString() => $"AssemblyPath: {AssemblyPath}, " +
                                             $"ExcludedParameterSets: {string.Join(", ", ExcludedParameterSets)}" +
                                             $"OutputHelpFilePath: {OutputHelpFilePath}, " +
                                             $"TreatWarningsAsErrors {TreatWarningsAsErrors}";
    }
}
