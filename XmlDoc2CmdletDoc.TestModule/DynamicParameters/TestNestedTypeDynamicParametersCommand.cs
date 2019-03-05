using System.Management.Automation;

namespace XmlDoc2CmdletDoc.TestModule.DynamicParameters
{
    /// <summary>
    /// <para type="synopsis">This is part of the Test-NestedTypeDynamicParameters synopsis.</para>
    /// <para type="description">This is part of the Test-NestedTypeDynamicParameters description.</para>
    /// </summary>
    [Cmdlet(VerbsDiagnostic.Test, "NestedTypeDynamicParameters")]
    public class TestNestedTypeDynamicParametersCommand : Cmdlet, IDynamicParameters
    {
        /// <summary>
        /// <para type="description">This is part of the StaticParam description.</para>
        /// </summary>
        [Parameter]
        public string StaticParam { get; set; }

        public object GetDynamicParameters() { return new NestedDynamicParameters(); }

        private class NestedDynamicParameters
        {
            /// <summary>
            /// <para type="description">This is part of the DynamicParam description.</para>
            /// </summary>
            [Parameter]
            public string DynamicParam { get; set; }

            /// <summary>
            /// <para type="description">Despite the <c>description</c> attribute, this is not a cmdlet
            /// parameter and this text should not appear in the help documentation.</para>
            /// </summary>
            public string IrrelevantProperty { get; set; }
        }

        private class UnrelatedClass
        {
            public string IrrelevantProperty { get; set; }
        }
    }
}