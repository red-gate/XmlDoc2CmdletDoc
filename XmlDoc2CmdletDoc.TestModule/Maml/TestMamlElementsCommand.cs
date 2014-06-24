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
        /// <summary>
        /// This text shouldn't appear in the cmldet help.
        /// <description type="description" xmlns="http://schemas.microsoft.com/maml/2004/10">
        ///   <para>This is the CommonParameter description.</para>
        /// </description>
        /// </summary>
        [Parameter]
        public MamlClass CommonParameter { get; set; }

        [Parameter(ParameterSetName = "One")]
        public string ParameterOne { get; set; }

        [Parameter(ParameterSetName = "Two")]
        public string ParameterTwo { get; set; }
    }
}