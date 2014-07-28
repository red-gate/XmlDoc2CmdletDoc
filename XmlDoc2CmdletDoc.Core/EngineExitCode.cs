namespace XmlDoc2CmdletDoc.Core
{
    /// <summary>
    /// Exit codes for the <see cref="Engine"/>.
    /// </summary>
    public enum EngineExitCode
    {
        Success = 0,
        AssemblyNotFound = 1,
        AssemblyLoadError = 2,
        AssemblyCommentsNotFound = 3,
        DocCommentsLoadError = 4,
        UnhandledException = 5,
        WarningsAsErrors = 6,
    }
}