using System.Management.Automation;

namespace XmlDoc2CmdletDoc.TestModule.Manual
{
    /// <summary>
    /// Example dummy comdlet. This text shouldn't appear in the cmdlet help.
    /// <para type="synopsis">This is part of the Test-ManualElements synopsis.</para>
    /// <para type="description">This is part of the Test-ManualElements description.</para>
    /// </summary>
    /// <para type="synopsis">This is also part of the Test-ManualElements synopsis.</para>
    /// <para type="description">This is also part of the Test-ManualElements description.</para>
    [Cmdlet(VerbsDiagnostic.Test, "ManualElements")]
    [OutputType(typeof(ManualClass))]
    [OutputType(typeof(string))]
    public class TestManualElementsCommand : Cmdlet
    {
        /// <summary>
        /// <para type="description">This is part of the MandatoryParameter description.</para>
        /// </summary>
        /// <para type="description">This is also part of the MandatoryParameter description.</para>
        [Parameter(Mandatory = true)]
        public ManualClass MandatoryParameter { get; set; }

        [Parameter(Mandatory = false)]
        public string OptionalParameter { get; set; }

        [Parameter(Position = 1)]
        public string PositionedParameter { get; set; }

        [Parameter(ValueFromPipeline = true)]
        public string ValueFromPipelineParameter { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public ManualClass ValueFromPipelineByPropertyNameParameter { get; set; }
    }
}
