namespace XmlDoc2CmdletDoc.Core
{
    /// <summary>
    /// Error codes for the <see cref="Engine"/>.
    /// </summary>
    public enum EngineErrorCode
    {
        Success = 0,
        AssemblyNotFound = 1,
        AssemblyLoadError = 2,
        AssemblyCommentsNotFound = 3,
        DocCommentsLoadError = 4,
    }
}