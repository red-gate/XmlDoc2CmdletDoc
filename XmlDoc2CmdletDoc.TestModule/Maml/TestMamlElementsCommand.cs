using System.Management.Automation;

namespace XmlDoc2CmdletDoc.TestModule.Maml
{
    /// <summary xmlns:maml="http://schemas.microsoft.com/maml/2004/10">
    /// Example dummy comdlet. This text shouldn't appear in the cmdlet help.
    /// <maml:description type="synopsis">
    ///   <maml:para>This is the Test-MamlElements synopsis.</maml:para>
    /// </maml:description>
    /// <maml:description type="description">
    ///   <maml:para>This is the Test-MamlElements description.</maml:para>
    /// </maml:description>
    /// <maml:alertSet>
    ///   <maml:title>First Note</maml:title>
    ///   <maml:alert>
    ///     <maml:para>This is the description for the first note.</maml:para>
    ///   </maml:alert>
    ///   <maml:title>Second Note</maml:title>
    ///   <maml:alert>
    ///      <maml:para>This is part of the description for the second note.</maml:para>
    ///      <maml:para>This is also part of the description for the second note.</maml:para>
    ///   </maml:alert>
    /// </maml:alertSet>
    /// </summary>
    [Cmdlet(VerbsDiagnostic.Test, "MamlElements")]
    [OutputType(typeof(MamlClass))]
    [OutputType(typeof(string))]
    public class TestMamlElementsCommand : Cmdlet
    {
        /// <summary xmlns:maml="http://schemas.microsoft.com/maml/2004/10">
        /// This text shouldn't appear in the cmldet help.
        /// <maml:description type="description">
        ///   <maml:para>This is the CommonParameter description.</maml:para>
        /// </maml:description>
        /// </summary>
        [Parameter]
        public MamlClass CommonParameter { get; set; }

        [Parameter(ParameterSetName = "One", ValueFromPipeline = true)]
        public string ParameterOne { get; set; }

        [Parameter(ParameterSetName = "Two", ValueFromPipelineByPropertyName = true)]
        public MamlClass ParameterTwo { get; set; }
    }
}