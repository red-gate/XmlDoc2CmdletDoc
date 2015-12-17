[CmdletBinding()]
param([string]$Configuration = 'Release')

use 14.0 MSBuild

# Useful paths used by multiple tasks.
$RepositoryRoot = "$PsScriptRoot\.." | Resolve-Path
$SolutionPath = "$RepositoryRoot\XmlDoc2CmdletDoc.sln" | Resolve-Path
$NuGetPath = "$PsScriptRoot\nuget.exe" | Resolve-Path

# Helper function for clearer logging of each task.
function Write-Info {
    param ([string] $Message)

    Write-Host "## $Message ##" -ForegroundColor Magenta
}

# Environment-specific configuration should happen here (and only here!)
task Init {
    Write-Info 'Establishing build properties'

    # Establish IsAutobuild property.
    $script:IsAutomatedBuild = $env:BRANCH_NAME -and $env:BUILD_NUMBER
    Write-Host "Is automated build = $IsAutomatedBuild"
    
    # Establish assembly version number
    $script:Version = [version]'99.99.99.99' # Default for dev builds.
    if ($env:BUILD_NUMBER) {
        $Version = [version]$env:BUILD_NUMBER # Build server override.
    }
    Write-Host "Version = '$Version'"
    
    # Establish NuGet package version.
    $BranchName = 'dev' # Default for dev builds.
    if ($IsAutomatedBuild) {
        $BranchName = $env:BRANCH_NAME # Build server override.
    }
    $IsDefaultBranch = $BranchName -eq 'master'
    $script:NuGetPackageVersion = New-NuGetPackageVersion -Version $Version -BranchName $BranchName -IsDefaultBranch $IsDefaultBranch
    Write-Host "NuGet package version = $NuGetPackageVersion"
}

# Clean task, deletes all build output folders.
task Clean {
    Write-Info 'Cleaning build output'

    Get-ChildItem $RepositoryRoot -Exclude @('packages') -Include @('bin', 'obj') -Directory -Recurse | ForEach-Object {
        Write-Host "Deleting $_"
        Remove-Item $_ -Force -Recurse
    }
}

# RestorePackages task, restores all the NuGet packages.
task RestorePackages {
    Write-Info "Restoring NuGet packages for solution $SolutionPath"

    & $NuGetPath @('restore', $SolutionPath)
}

# UpdateAssemblyInfo task, updates the AssemblyVersion, AssemblyFileVersion and AssemblyInformationlVersion attributes in the source code.
task UpdateAssemblyInfo  Init, {
    Write-Info 'Updating assembly information'

    $SolutionInfoPath = [string] ("$RepositoryRoot\SolutionInfo.cs" | Resolve-Path)
    $CurrentContent = Get-Content $SolutionInfoPath -Encoding UTF8
    $NewContent = $CurrentContent `
        -replace '(?<=Assembly(File)?Version\(")[0-9\.\*]*(?="\))', $Version.ToString() `
        -replace '(?<=AssemblyInformationalVersion\(")[a-zA-Z0-9\.\-]*(?="\))', $NuGetPackageVersion
    $NewContent | Out-File $SolutionInfoPath -Encoding utf8
}

# Compile task, runs MSBuild to build the solution.
task Compile  UpdateAssemblyInfo, RestorePackages, {
    Write-Info "Compiling solution $SolutionPath"

    exec {
        msbuild "$SolutionPath" `
        /nodeReuse:False `
        /target:Build `
        /property:Configuration=$Configuration `
        $AdditionalMSBuildParameters
    }
}

# Package task, create the NuGet package.
task Package  Compile, {
    Write-Info 'Generating NuGet package'

    $BasePath = "$RepositoryRoot\RedGate.PowerShell" | Resolve-Path
    $CSProjPath = "$BasePath\RedGate.PowerShell.csproj" | Resolve-Path

    # Run NuGet pack.
    $Parameters = @(
        'pack',
        "$CSProjPath",
        '-Version', $NuGetPackageVersion,
        '-OutputDirectory', $BuildDir,
        '-BasePath', $BasePath
        '-Properties', "configuration=$Configuration"
    )
    & $NuGetPath $Parameters
}


task Build  Compile
task Rebuild  Clean, Build
task Default  Build