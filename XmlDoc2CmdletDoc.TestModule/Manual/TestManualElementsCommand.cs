using System.Management.Automation;

namespace XmlDoc2CmdletDoc.TestModule.Manual
{
    /// <summary>
    /// Example dummy comdlet. This text shouldn't appear in the cmdlet help.
    /// <para type="synopsis">This is part of the Test-ManualElement synopsis.</para>
    /// <para type="description">This is part of the Test-ManualElement description.</para>
    /// </summary>
    /// <para type="synopsis">This is also part of the Test-ManualElement synopsis.</para>
    /// <para type="description">This is also part of the Test-ManualElement description.</para>
    [Cmdlet(VerbsDiagnostic.Test, "ManualElements")]
    [OutputType(typeof(ManualClass))]
    public class TestManualElementsCommand : Cmdlet
    {
        /// <summary>
        /// <para type="description">This is part of the MandatoryParameter description.</para>
        /// </summary>
        /// <para type="description">This is also part of the MandatoryParameter description.</para>
        [Parameter(Mandatory = true)]
        public string MandatoryParameter { get; set; }

        /// <summary>
        /// <para type="description">This is part of the PositionedParameter description.</para>
        /// </summary>
        /// <para type="description">This is also part of the PositionedParameter description.</para>
        [Parameter(Position = 1)]
        public string PositionedParameter { get; set; }

        /// <summary>
        /// <para type="description">This is part of the ValueFromPipelineParameter description.</para>
        /// </summary>
        /// <para type="description">This is also part of the ValueFromPipelineParameter description.</para>
        [Parameter(ValueFromPipeline = true)]
        public string ValueFromPipelineParameter { get; set; }

        /// <summary>
        /// <para type="description">This is part of the ValueFromPipelineParameterByPropertyName description.</para>
        /// </summary>
        /// <para type="description">This is also part of the ValueFromPipelineParameterByPropertyName description.</para>
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public string ValueFromPipelineByPropertyNameParameter { get; set; }
    }
}
