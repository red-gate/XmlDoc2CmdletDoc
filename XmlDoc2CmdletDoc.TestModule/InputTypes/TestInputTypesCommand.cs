using System.Management.Automation;

namespace XmlDoc2CmdletDoc.TestModule.InputTypes
{
    [Cmdlet(VerbsDiagnostic.Test, "InputTypes")]
    public class TestInputTypesCommand : Cmdlet
    {
        /// <summary>
        /// <para type="inputType">This is the explicit inputType description for ParameterOne.</para>
        /// </summary>
        [Parameter(ValueFromPipeline = true)]
        public InputTypeClass1 ParameterOne { get; set; }

        /// <summary>
        /// There's no explicit inputType description, so the description of the <see cref="InputTypeClass2"/> itself will be used instead.
        /// </summary>
        [Parameter(ValueFromPipeline = true)]
        public InputTypeClass2 ParameterTwo { get; set; }

        /// <summary>
        /// <para type="description">This is the fallback description for ParameterThree.</para>
        /// </summary>
        [Parameter(ValueFromPipeline = true)]
        public InputTypeClass3 ParameterThree { get; set; }
    }
}