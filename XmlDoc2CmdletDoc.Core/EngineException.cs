using System;

namespace XmlDoc2CmdletDoc.Core
{
    public class EngineException : ApplicationException
    {
        public readonly EngineExitCode ExitCode;

        public EngineException(EngineExitCode exitCode, string message, Exception innerException = null)
            : base(message, innerException)
        {
            ExitCode = exitCode;
        }
    }
}