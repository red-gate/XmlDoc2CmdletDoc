using System.Management.Automation;

namespace XmlDoc2CmdletDoc.TestModule.PositionedParameters
{
    /// <summary>
    /// <para type="synopsis">This is part of the Test-PositionedParameters synopsis.</para>
    /// <para type="description">This is part of the Test-PositionedParameters description.</para>
    /// </summary>
    [Cmdlet(VerbsDiagnostic.Test, "PositionedParameters")]
    public class TestPositionedParametersCommand : Cmdlet
    {
        /// <summary>
        /// <para type="description">This is part of the ParameterA description.</para>
        /// </summary>
        [Parameter(Position = 3)]
        public string ParameterA { get; set; }

        /// <summary>
        /// <para type="description">This is part of the ParameterB description.</para>
        /// </summary>
        [Parameter(Position = 2)]
        public string ParameterB { get; set; }

        /// <summary>
        /// <para type="description">This is part of the ParameterC description.</para>
        /// </summary>
        [Parameter(Position = 0)]
        public string ParameterC { get; set; }

        /// <summary>
        /// <para type="description">This is part of the ParameterD description.</para>
        /// </summary>
        [Parameter(Position = 1)]
        public string ParameterD { get; set; }

        /// <summary>
        /// <para type="description">This is part of the ParameterE description.</para>
        /// </summary>
        [Parameter]
        public string ParameterE { get; set; }

        /// <summary>
        /// <para type="description">This is part of the ParameterF description.</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string ParameterF { get; set; }
    }
}
