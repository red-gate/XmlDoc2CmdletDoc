using System;

namespace XmlDoc2CmdletDoc.Core
{
    public class EngineException : ApplicationException
    {
        public readonly EngineErrorCode ErrorCode;

        public EngineException(EngineErrorCode errorCode, string message, Exception innerException = null)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}