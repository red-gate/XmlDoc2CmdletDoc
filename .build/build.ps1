# This file cannot be invoked directly; it simply contains a bunch of Invoke-Build tasks. To use it, invoke
# _init.ps1 which declares three global functions (build, clean, rebuild), then invoke one of those functions.

[CmdletBinding()]
param([string]$Configuration = 'Release')

use 14.0 MSBuild

# Useful paths used by multiple tasks.
$RepositoryRoot = "$PsScriptRoot\.." | Resolve-Path
$VersionFilePath = "$RepositoryRoot\version-number.txt" | Resolve-Path
$SolutionPath = "$RepositoryRoot\XmlDoc2CmdletDoc.sln" | Resolve-Path
$NuGetPath = "$PsScriptRoot\nuget.exe" | Resolve-Path
$DistPath = "$RepositoryRoot\dist"

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
    $script:Version = [version] (Get-Content $VersionFilePath).Trim()
    Write-Host "Version = '$Version'"
    
    # Establish NuGet package version.
    $BranchName = Get-BranchName
    $IsDefaultBranch = $BranchName -eq 'master'
    $script:NuGetPackageVersion = New-NuGetPackageVersion -Version $Version -BranchName $BranchName -IsDefaultBranch $IsDefaultBranch
    Write-Host "NuGet package version = $NuGetPackageVersion"
}

function Get-BranchName {
    # If the branch name is specified via an environment variable (i.e. on TeamCity), use it.
    if ($env:BRANCH_NAME) {
        return $env:BRANCH_NAME
    }

    # If the .git folder is present, try to get the current branch using Git.
    $DotGitDirPath = "$RepositoryRoot\.git"
    if (Test-Path $DotGitDirPath) {
        Add-Type -Path ("$PsScriptRoot\packages\GitSharp\lib\GitSharp.dll" | Resolve-Path)
        Add-Type -Path ("$PsScriptRoot\packages\SharpZipLib\lib\20\ICSharpCode.SharpZipLib.dll" | Resolve-Path)
        Add-Type -Path ("$PsScriptRoot\packages\Tamir.SharpSSH\lib\Tamir.SharpSSH.dll" | Resolve-Path)
        Add-Type -Path ("$PsScriptRoot\packages\Winterdom.IO.FileMap\lib\Winterdom.IO.FileMap.dll" | Resolve-Path)
    
        $Repository = New-Object 'GitSharp.Repository' $DotGitDirPath
        return $Repository.CurrentBranch.Name
    }

    # Otherwise, assume 'master'
    Write-Warning "Unable to determine the current branch name using either git or the BRANCH_NAME environment variable. Defaulting to 'master'."
    return 'master'
}

# Clean task, deletes all build output folders.
task Clean {
    Write-Info 'Cleaning build output'

    Get-ChildItem $RepositoryRoot -Exclude @('packages') -Include @('dist', 'bin', 'obj') -Directory -Recurse | ForEach-Object {
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

# Test task, runs the NUnit tests.
task Test  Compile, {
    Write-Info 'Running tests'

    $AssemblyPath = "$RepositoryRoot\XmlDoc2CmdletDoc.Tests\bin\$Configuration\XmlDoc2CmdletDoc.Tests.dll" | Resolve-Path
    Invoke-NUnitForAssembly -AssemblyPath $AssemblyPath `
                            -NUnitVersion '2.6.3' `
							-FrameworkVersion 'net-4.5'
}

# Package task, create the NuGet package.
task Package  Test, {
    Write-Info 'Generating NuGet package'

    # Make sure the output folder exists.
    if (-not (Test-Path $DistPath)) {
        $Null = mkdir $DistPath
    }

    # Run NuGet pack.
    $NuSpecPath = "$RepositoryRoot\XmlDoc2CmdletDoc\XmlDoc2CmdletDoc.nuspec" | Resolve-Path
    $Parameters = @(
        'pack',
        "$NuSpecPath",
        '-Version', $NuGetPackageVersion,
        '-OutputDirectory', $DistPath,
        '-Properties', "configuration=$Configuration"
    )
    & $NuGetPath $Parameters
}


task Build  Package
task Rebuild  Clean, Build
task Default  Build