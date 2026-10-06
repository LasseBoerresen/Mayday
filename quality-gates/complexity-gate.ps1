<#
.SYNOPSIS
    Builds a .NET project or solution and fails when a method exceeds the cyclomatic complexity threshold.

.DESCRIPTION
    The threshold lives in CodeMetricsConfig.txt and the rule's severity in .editorconfig (see quality-gates/install),
    so a plain `dotnet build` enforces the same limit. This script is the fast early warning: it builds, collects the
    CA1502 errors and exits 2 with them, which Claude Code feeds back to Claude. Any other build failure also exits 2,
    with the tail of the build output. Exits 0 otherwise.

.PARAMETER ProjectRoot
    Directory to build. Defaults to the current directory.

.PARAMETER Target
    Solution or project file to build, relative to ProjectRoot. Defaults to the only .sln, else the only .csproj.
#>
[CmdletBinding()]
param(
    [string] $ProjectRoot = (Get-Location).Path,
    [string] $Target
)

$ErrorActionPreference = 'Stop'
Set-Location $ProjectRoot

if (-not $Target) {
    $found = @(Get-ChildItem -Path . -Filter *.sln)
    if ($found.Count -eq 0) { $found = @(Get-ChildItem -Path . -Filter *.csproj) }
    if ($found.Count -ne 1) {
        [Console]::Error.WriteLine("Cannot pick a build target in $ProjectRoot (found $($found.Count)); pass -Target.")
        exit 2
    }
    $Target = $found[0].Name
}

$output = dotnet build $Target --nologo --verbosity quiet --no-incremental 2>&1 | Out-String
$buildExit = $LASTEXITCODE
if ($buildExit -eq 0) { exit 0 }

$violations = $output -split "`r?`n" | Where-Object { $_ -match 'error CA1502' } | Sort-Object -Unique
if ($violations) {
    [Console]::Error.WriteLine("Cyclomatic complexity gate failed. Refactor these methods (extract methods, replace branches with data or polymorphism):`n$($violations -join "`n")")
}
else {
    $tail = ($output -split "`r?`n" | Select-Object -Last 40) -join "`n"
    [Console]::Error.WriteLine("Build failed:`n$tail")
}
exit 2
