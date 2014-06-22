using System.Management.Automation;

namespace XmlDoc2CmdletDoc.TestModule
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
        /// <para type="description">This is also part of the MandatoryParameter description.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string MandatoryParameter { get; set; }

        [Parameter]
        public ManualClass ManualClassParameter { get; set; }

        [Parameter(ValueFromPipeline = true)]
        public string ValueFromPipelineParameter { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public string ValueFromPipelineByPropertyNameParameter { get; set; }
    }
}
