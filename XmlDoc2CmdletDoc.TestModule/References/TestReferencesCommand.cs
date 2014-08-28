using System.Management.Automation;

namespace XmlDoc2CmdletDoc.TestModule.References
{
    /// <summary>
    /// <para type="description">This description for <see cref="TestReferencesCommand"/> references
    /// <see cref="ParameterOne"/> and <see cref="ParameterTwo">the second parameter</see>.</para>
    /// </summary>
    [Cmdlet(VerbsDiagnostic.Test, "References")]
    public class TestReferencesCommand : Cmdlet
    {
        [Parameter(Mandatory = true)]
        public string ParameterOne { get; set; }

        [Parameter(Mandatory = true)]
        public string ParameterTwo { get; set; }
    }
}