using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace XmlDoc2CmdletDoc.TestModule.DefaultValue
{
    /// <summary>
    /// <para type="synopsis">This is part of the Test-DefaultValue synopsis.</para>
    /// <para type="description">This is part of the Test-DefaultValue description.</para>
    /// </summary>
    [Cmdlet(VerbsDiagnostic.Test, "DefaultValue")]
    public class TestDefaultValueCommand : Cmdlet
    {
        /// <summary>
        /// <para type="description">This has the default values: 1, 2, 3.</para>
        /// </summary>
        [Parameter]
        public int[] ArrayParameter = {1, 2, 3};

        /// <summary>
        /// <para type="description">This has the default values: 1, 2, 3.</para>
        /// </summary>
        [Parameter]
        public IList<int> ListParameter = new List<int> {1, 2, 3};

        /// <summary>
        /// <para type="description">This has the default values: 1, 2, 3.</para>
        /// </summary>
        [Parameter]
        public IEnumerable<int> EnumerableParameter = Enumerable.Range(1, 3);

        /// <summary>
        /// <para type="description">This has no default value.</para>
        /// </summary>
        [Parameter]
        public IEnumerable<int> EmptyEnumerableParameter = Enumerable.Empty<int>();
    }
}
