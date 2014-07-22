using System.Management.Automation;

namespace XmlDoc2CmdletDoc.TestModule.Undocumented
{
    /// <summary>
    /// This cmdlet has no cmdlet help documentation.
    /// </summary>
    [Cmdlet(VerbsDiagnostic.Test, "MamlElements")]
    [OutputType(typeof(UndocumentedClass))]
    [OutputType(typeof(string))]
    public class TestUndocumentedCommand : Cmdlet
    {
        /// <summary>
        /// This field has no cmdlet help documentation.
        /// </summary>
        [Parameter]
        public object UndocumentedField;

        /// <summary>
        /// This property has no cmdlet help documentation.
        /// </summary>
        [Parameter]
        public object UndocumentedProperty;
    }
}
