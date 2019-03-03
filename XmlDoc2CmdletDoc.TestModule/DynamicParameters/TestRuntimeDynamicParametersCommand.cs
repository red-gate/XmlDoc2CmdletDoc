using System;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace XmlDoc2CmdletDoc.TestModule.DynamicParameters
{
    /// <summary>
    /// <para type="synopsis">This is part of the Test-RuntimeDynamicParameters synopsis.</para>
    /// <para type="description">This is part of the Test-RuntimeDynamicParameters description.</para>
    /// </summary>
    [Cmdlet(VerbsDiagnostic.Test, "RuntimeDynamicParameters")]
    public class TestRuntimeDynamicParametersCommand : Cmdlet, IDynamicParameters
    {
        /// <summary>
        /// <para type="description">This is part of the StaticParam description.</para>
        /// </summary>
        [Parameter]
        public string StaticParam { get; set; }

        public object GetDynamicParameters()
        {
            var runtimeParamDictionary = new RuntimeDefinedParameterDictionary();

            var attributes = new Collection<Attribute>();
            attributes.Add(new ParameterAttribute());

            //This parameter has a ParameterAttribute and is considered a dynamic parameter.
            var dynamicParam = new RuntimeDefinedParameter("DynamicParam", typeof(string), attributes);

            //This parameter is missing a ParameterAttribute. While it is technically still a dynamic parameter,
            //it is not considered part of any parameter set and cannot be used.
            var irrelevantParam = new RuntimeDefinedParameter("IrrelevantParam", typeof(string), new Collection<Attribute>());

            runtimeParamDictionary.Add("DynamicParam", dynamicParam);
            runtimeParamDictionary.Add("IrrelevantParam", irrelevantParam);

            return runtimeParamDictionary;
        }
    }
}
