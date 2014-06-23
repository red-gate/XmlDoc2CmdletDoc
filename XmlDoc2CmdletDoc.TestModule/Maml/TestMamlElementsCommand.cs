using System.Management.Automation;
using XmlDoc2CmdletDoc.TestModule.Manual;

namespace XmlDoc2CmdletDoc.TestModule.Maml
{
    /// <summary>
    /// Example dummy comdlet. This text shouldn't appear in the cmdlet help.
    /// <maml:description type="synopsis" xmlns:maml="http://schemas.microsoft.com/maml/2004/10">
    ///   <maml:para>This is the Test-MamlElements synopsis.</maml:para>
    /// </maml:description>
    /// <maml:description type="description" xmlns:maml="http://schemas.microsoft.com/maml/2004/10">
    ///   <maml:para>This is the Test-MamlElements description.</maml:para>
    /// </maml:description>
    /// </summary>
    [Cmdlet(VerbsDiagnostic.Test, "MamlElements")]
    [OutputType(typeof(ManualClass))]
    public class TestMamlElementsCommand : Cmdlet
    {
        [Parameter]
        public string CommonParameter { get; set; }

        [Parameter(ParameterSetName = "One")]
        public string ParameterOne { get; set; }

        [Parameter(ParameterSetName = "Two")]
        public string ParameterTwo { get; set; }
    }
}