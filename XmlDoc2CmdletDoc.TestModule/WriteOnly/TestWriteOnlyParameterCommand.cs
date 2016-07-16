using System.Management.Automation;

namespace XmlDoc2CmdletDoc.TestModule.WriteOnly
{
    /// <summary>
    /// Cmdlet that has a parameter with no getter. This exists to demonstrate that XmlDoc2CmdletDoc
    /// doesn't blow up when trying to determine the default value of a parameter that cannot be read.
    /// </summary>
    [Cmdlet(VerbsDiagnostic.Test, "WriteOnlyParameter")]
    public class TestWriteOnlyParameterCommand : Cmdlet
    {
        /// <summary>
        /// This parameter has no getter.
        /// </summary>
        [Parameter]
        public string WriteOnly { set { } }
    }
}