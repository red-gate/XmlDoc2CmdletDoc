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
    /// <list type="alertSet">
    ///   <item>
    ///     <term>First Note</term>
    ///     <description>
    ///     This is the description for the first note.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>Second Note</term>
    ///     <description>
    ///       <para>This is part of the description for the second note.</para>
    ///       <para>This is also part of the description for the second note.</para>
    ///     </description>
    ///   </item>
    /// </list>
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
