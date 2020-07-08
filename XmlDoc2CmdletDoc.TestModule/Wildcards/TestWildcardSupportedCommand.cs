using System.Management.Automation;

namespace XmlDoc2CmdletDoc.TestModule.Wildcards
{
    /// <summary>
    /// <para type="synopsis">This is part of the Test-WildcardSupport synopsis.</para>
    /// <para type="description">This is part of the Test-WildcardSupport description.</para>
    /// </summary>
    [Cmdlet(VerbsDiagnostic.Test, "WildcardSupport")]
    public class TestWildcardSupportedCommand : Cmdlet
    {
        /// <summary>
        /// <para type="description">This supports wildcards.</para>
        /// </summary>
        [Parameter]
        [SupportsWildcards]
        public string StringParameter { get; set; }

        /// <summary>
        /// <para type="description">This does not support wildcards.</para>
        /// </summary>
        [Parameter]
        public string NonWildParameter { get; set; }
    }
}
