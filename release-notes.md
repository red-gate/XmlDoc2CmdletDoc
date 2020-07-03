*Note: The build version number is derived from the first entry in this file.*

# 0.3.0

- Use Microsoft.PowerShell.5.ReferenceAssemblies instead of a legacy version of System.Management.Automation.

# 0.2.13

- Correctly resolve type descriptions for array-typed parameters.

# 0.2.12

- Extended the support for documenting parameters of type `Enum` to include parameters of type `IEnumerable<T> where T : Enum`.

# 0.2.11

- Added support for dynamic parameters.

# 0.2.10

- Parameters marked with the [Obsolete] attribute no longer appear in the cmdlet syntax summary, though it's still possible to provide help text for the parameter, which can be viewed using the `-Parameter` switch of the `Get-Help` cmdlet.

- Added support for excluding parameter sets by name, via a new command-line option and corresponding msbuild property.

# 0.2.9

- Fixed issue #39. Corrected a regression for the default value of string parameters.
    
# 0.2.8

- Partially addressed issues #33 and #37. Slightly improved handling of default values and array parameters. More work is required in this area, though.
    
# 0.2.7

- Fixed issue #31. MSBuild task now accommodates binary modules that are specifically targeted at only x86 or x64 architectures.

# 0.2.6

- Fixed issue #28. Ensure that help syntax is correctly displayed for parameterless cmdlets.

# 0.2.5

- Added limited support for documenting dynamic parameters. If a cmdlet implements IDynamicParameters, and its GetDynamicParameters method returns an instance of a nested type within the cmdlet, then help documentation will be extracted from the nested type's XML Doc comments.

# 0.2.4

- Fixed issue #22. When encountering a Parameter with no getter, XmlDoc2CmdletDoc now records a warning that the default value for the Parameter cannot be obtained. Previously this raised a fatal exception.

# 0.2.3

- XmlDoc2CmdletDoc now executes prior to the AfterBuild target, rather than prior to the PostBuildEvent target, to give developers the option to copy files around in either target, rather than only in the latter.

# 0.2.2

- Fixed issue #19: Help for cmdlet parameters is now explicitly ordered by Position, then Required, then Name, rather than relying on the arbitrary order of Type.GetMembers.

# 0.2.1

- First public release.