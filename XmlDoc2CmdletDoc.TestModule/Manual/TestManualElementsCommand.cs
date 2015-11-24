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
    /// <example>
    ///   <para>This is part of the first example's introduction.</para>
    ///   <para>This is also part of the first example's introduction.</para>
    ///   <code>New-Thingy | Write-Host</code>
    ///   <para>This is part of the first example's remarks.</para>
    ///   <para>This is also part of the first example's remarks.</para>
    /// </example>
    /// <example>
    ///   <para>This is the second example's introduction.</para>
    ///   <code>
    ///       $thingy = New-Thingy
    ///       If ($thingy -eq $that) {
    ///         Write-Host 'Same'
    ///       } else {
    ///         $thingy | Write-Host
    ///       }
    ///   </code>
    ///   <para>This is the second example's remarks.</para>
    /// </example>
    /// <example>
    ///   <code>New-Thingy | Write-Host</code>
    ///   <para>This is the third example's remarks.</para>
    /// </example>
    /// <example>
    ///   <para>This is the fourth example's introduction.</para>
    ///   <code>New-Thingy | Write-Host</code>
    /// </example>
    [Cmdlet(VerbsDiagnostic.Test, "ManualElements")]
    [OutputType(typeof(ManualClass))]
    [OutputType(typeof(string))]
    public class TestManualElementsCommand : Cmdlet
    {
        public enum Importance
        {
            Regular,
            Important,
            Critical
        };

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

        /// <summary>
        /// <para type="description">ValueFromPipeline string parameter is...</para>
        /// </summary>
        [Parameter(ValueFromPipeline = true)]
        public string ValueFromPipelineParameter { get; set; }

        /// <summary>
        /// <para type="description">OtherValueFromPipeline string parameter is...</para>
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public string OtherValueFromPipelineParameter { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public ManualClass ValueFromPipelineByPropertyNameParameter { get; set; }

        [Parameter(Mandatory = false)]
        public Importance EnumParameter { get; set; }

        [Parameter]
        public int? NullableParameter { get; set; }
    }
}
