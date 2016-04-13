# This file is a bootstrapper for the real build file. It's purpose is as follows:
#
# 1. Define some top-level fubctions (build, clean, rebuild) that can be used to kick off the build from the command-line.
# 2. Download nuget.exe and then obtain some NuGet packages that the real build script relies on.
# 3. Import the RedGate.Build module to make available some convenient build cmdlets.

$VerbosePreference = 'Continue'          # Want useful output in our build log files.
$ProgressPreference = 'SilentlyContinue' # Progress logging slows down TeamCity when downloading files with Invoke-WebRequest.
$ErrorActionPreference = 'Stop'          # Abort quickly on error.

function global:Build
{
    [CmdletBinding()]
    param(
        [string[]] $Task = @('Default'),

        [ValidateSet('Release', 'Debug')]
        [string] $Configuration = 'Release'
    )

    Push-Location $PsScriptRoot -Verbose
    try
    {
        # Obtain nuget.exe
        $NuGetVersion = [version] '3.3.0'
        $NuGetPath = '.\nuget.exe'
        if (-not (Test-Path $NuGetPath) -or (Get-Item $NuGetPath).VersionInfo.ProductVersion -ne $NuGetVersion)
        {
            $NuGetUrl = "https://dist.nuget.org/win-x86-commandline/v$NuGetVersion/nuget.exe"
            Write-Host "Downloading $NuGetUrl"
            Invoke-WebRequest $NuGetUrl -OutFile $NuGetPath
        }
        
        # Restore the 'build-level' nuget packages into .build/packages if necessary.
        $NuGetConfigXml = [xml](Get-Content 'packages.config')
        $NuGetConfigXml.packages.package | ForEach-Object {
            & $NuGetPath install $_.id `
                -Version $_.version `
                -OutputDirectory 'packages' `
                -ExcludeVersion `
                -PackageSaveMode nuspec
        }

        # Import the RedGate.Build module.
        Import-Module '.\packages\RedGate.Build\tools\RedGate.Build.psm1' -Force

        # Call the actual build script.
        & '.\packages\Invoke-Build\tools\Invoke-Build.ps1' -File .\build.ps1 -Task $Task -Configuration $Configuration
    }
    finally
    {
        Pop-Location
    }
}

function global:Clean 
{
    Build -Task Clean
}

function global:Rebuild
{
    [CmdletBinding()]
    param([ValidateSet('Release', 'Debug')] [string] $Configuration = 'Release')

    Build -Task Rebuild -Configuration $Configuration
}

Write-Host 'This is the XmlDoc2CmdletDoc repo. Here are the available commands:' -ForegroundColor Magenta
Write-Host "    Build [-Task <task-list>] [-Configuration <Debug|Release>]" -ForegroundColor Green
Write-Host "    Clean" -ForegroundColor Green
Write-Host "    Rebuild [-Configuration <Debug|Release>]" -ForegroundColor Green
