using System.Management.Automation;

namespace XmlDoc2CmdletDoc.TestModule.Parameterless
{
    /// <summary>
    /// <para type="synopsis">This is part of the Test-PositionedParameters synopsis.</para>
    /// <para type="description">This is part of the Test-PositionedParameters description.</para>
    /// </summary>
    [Cmdlet(VerbsDiagnostic.Test, "Parameterless")]
    public class TestParameterlessCommand : Cmdlet
    {
        // Addresses https://github.com/red-gate/XmlDoc2CmdletDoc/issues/28
        // This cmdlet has no parameters. We expect this to result in the following syntax element:
        // <command:syntax>
        // <!-- Parameter set: __AllParameterSets -->
        //   <command:syntaxItem>
        //     <maml:name>Test-Parameterless</maml:name>
        //   </command:syntaxItem>
        // </command:syntax>
    }
}