using System;

namespace XmlDoc2CmdletDoc.Core
{
    /// <summary>
    /// Custom exception type raised by the <see cref="Engine"/> class.
    /// </summary>
    public class EngineException : ApplicationException
    {
        /// <summary>
        /// An exit code associated with the error.
        /// </summary>
        public readonly EngineExitCode ExitCode;

        /// <summary>
        /// Creates a new instance with the specified properties.
        /// </summary>
        /// <param name="exitCode">The exit code associated with the error.</param>
        /// <param name="message">An error message associated wih the error. </param>
        /// <param name="innerException">An optional inner exception associated with the error.</param>
        public EngineException(EngineExitCode exitCode, string message, Exception innerException = null)
            : base(message, innerException)
        {
            ExitCode = exitCode;
        }
    }
}