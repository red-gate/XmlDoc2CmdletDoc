namespace XmlDoc2CmdletDoc.Core
{
    /// <summary>
    /// Exit codes for the <see cref="Engine"/>.
    /// </summary>
    public enum EngineExitCode
    {
        /// <summary>
        /// Indicates success.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The target assembly could not be found.
        /// </summary>
        AssemblyNotFound = 1,

        /// <summary>
        /// The target assembly could not be loaded. This could indicate that the
        /// target assembly is not architecture independent.
        /// </summary>
        AssemblyLoadError = 2,

        /// <summary>
        /// The XML Doc comments file for the target assembly could not be found.
        /// </summary>
        AssemblyCommentsNotFound = 3,

        /// <summary>
        /// An error occurred whilst tring to load the target assembly's XML Doc comments file.
        /// </summary>
        DocCommentsLoadError = 4,

        /// <summary>
        /// An unspecified exception occurred.
        /// </summary>
        UnhandledException = 5,

        /// <summary>
        /// One or more warnings occurred, and warnings are treated as errors when in strict mode.
        /// </summary>
        WarningsAsErrors = 6,
    }
}