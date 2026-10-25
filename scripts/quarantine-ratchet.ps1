<#
.SYNOPSIS
    Fails when the committed branch quarantines or skips a test that already exists where it left the upstream.

.DESCRIPTION
    Compares the C# files under Test/ at HEAD with those at the merge-base of HEAD and -Upstream. The set of
    quarantined and skipped tests may only shrink (MAY-80, ADR 0011):

    - A test, or test class, that exists at the merge-base may not gain [Quarantine(...)], [Trait("Quarantine", ...)]
      or a Skip = ... in its test attribute.
    - A new test may arrive quarantined only if its reason names the Linear issue that will fix it (MAY-<number>).

    Tests are named Namespace.Class.Method (a class-level mark: Namespace.Class), so a test moved to another file in
    the same namespace keeps its identity, and classes of the same name in different namespaces stay apart.
    Exits 1 and lists every violation, or 0 when the ratchet holds.

.PARAMETER RepoRoot
    The git working tree whose HEAD is checked. Defaults to the repository this script lives in.

.PARAMETER Upstream
    The branch HEAD is compared with, at their merge-base. Defaults to origin/master.
#>
[CmdletBinding()]
param(
    [string] $RepoRoot,
    [string] $Upstream = 'origin/master'
)

$ErrorActionPreference = 'Stop'
if (-not $RepoRoot) { $RepoRoot = Split-Path -Parent $PSScriptRoot }
Set-Location $RepoRoot

$keywords = 'return|await|new|throw|else|if|while|for|foreach|using|yield|case|var|nameof|typeof|lock|switch|catch'
$methodPattern = "^\s*(?:(?:public|private|protected|internal|static|async|override|virtual|sealed|unsafe)\s+)*(?!(?:$keywords)\b)[\w<>\[\],.?]+\s+(\w+)\s*(?:<[^>]*>)?\s*\("
$classPattern = '\b(?:class|record|struct)\s+(\w+)'
$namespacePattern = '^\uFEFF?namespace\s+([\w.]+)'

# Brackets that are not inside a string literal, so a reason may contain any text.
function Get-BracketDepthChange([string] $Line) {
    $code = $Line -replace '"(?:[^"\\]|\\.)*"', '""'
    ([regex]::Matches($code, '\[')).Count - ([regex]::Matches($code, '\]')).Count
}

# Splits text that starts with attributes into the attributes and any code after them on the same line, such as
# "[Fact, Quarantine("x")] void M()". Brackets inside string literals do not count.
function Split-Attributes([string] $Text) {
    $depth = 0
    $inString = $false
    for ($i = 0; $i -lt $Text.Length; $i++) {
        $c = $Text[$i]
        if ($inString) {
            if ($c -eq '\') { $i++ } elseif ($c -eq '"') { $inString = $false }
            continue
        }
        if ($c -eq '"') { $inString = $true }
        elseif ($c -eq '[') { $depth++ }
        elseif ($c -eq ']') {
            $depth--
            $rest = $Text.Substring($i + 1).Trim()
            if ($depth -eq 0 -and $rest -and -not $rest.StartsWith('[') -and -not $rest.StartsWith('//')) {
                return $Text.Substring(0, $i + 1), $rest
            }
        }
    }
    $Text, ''
}

# The tests a C# file declares and the marks on them: each attribute block before a declaration belongs to it.
function Read-TestFile([string[]] $Lines) {
    $declared = [System.Collections.Generic.List[string]]::new()
    $marks = [System.Collections.Generic.List[object]]::new()
    $namespace = $null
    $class = $null
    $attributes = @()
    $current = $null
    $depth = 0
    foreach ($line in $Lines) {
        $trimmed = $line.Trim()
        # An attribute block may end in the declaration it marks, on the same line: that rest is read as a declaration.
        if ($null -ne $current) {
            $current += "`n$trimmed"
            $depth += Get-BracketDepthChange $line
            if ($depth -gt 0) { continue }
            $attribute, $trimmed = Split-Attributes $current
            $attributes += $attribute
            $current = $null
            if (-not $trimmed) { continue }
        }
        elseif ($trimmed -eq '' -or $trimmed.StartsWith('//')) { continue }
        elseif ($trimmed.StartsWith('[')) {
            $depth = Get-BracketDepthChange $line
            if ($depth -gt 0) { $current = $trimmed; continue }
            $attribute, $trimmed = Split-Attributes $trimmed
            $attributes += $attribute
            if (-not $trimmed) { continue }
        }
        if ($trimmed -match $namespacePattern) { $namespace = $Matches[1]; $attributes = @(); continue }
        $name = $null
        # Keyed with the namespace: test classes with the same name live in several namespaces.
        if ($trimmed -match $classPattern) { $class = (@($namespace, $Matches[1]) | Where-Object { $_ }) -join '.'; $name = $class }
        elseif ($trimmed -match $methodPattern) { $name = if ($class) { "$class.$($Matches[1])" } else { $Matches[1] } }
        if ($name) {
            $declared.Add($name)
            foreach ($attribute in $attributes) {
                # The gate selects quarantined tests by trait, so setting the trait by hand quarantines a test too.
                if ($attribute -match '\bQuarantine\s*\(|\bTrait\s*\(\s*"Quarantine"') { $marks.Add([pscustomobject]@{ Name = $name; Kind = 'Quarantine'; Text = $attribute }) }
                if ($attribute -match '\bSkip\s*=') { $marks.Add([pscustomobject]@{ Name = $name; Kind = 'Skip'; Text = $attribute }) }
            }
        }
        $attributes = @()
    }
    [pscustomobject]@{ Declared = $declared; Marks = $marks }
}

function Read-TestTree([string] $Revision) {
    $files = git ls-tree -r --name-only $Revision -- Test | Where-Object { $_ -match '\.cs$' }
    $declared = [System.Collections.Generic.HashSet[string]]::new()
    $marks = [System.Collections.Generic.List[object]]::new()
    foreach ($file in $files) {
        $read = Read-TestFile (git show "${Revision}:$file")
        $read.Declared | ForEach-Object { [void]$declared.Add($_) }
        $read.Marks | ForEach-Object { $marks.Add(($_ | Add-Member -PassThru -NotePropertyName File -NotePropertyValue $file)) }
    }
    [pscustomobject]@{ Declared = $declared; Marks = $marks }
}

$base = git merge-base $Upstream HEAD
if (-not $base) { throw "No merge-base between HEAD and $Upstream; fetch $Upstream first." }

$before = Read-TestTree $base
$after = Read-TestTree 'HEAD'
$marked = @($before.Marks | ForEach-Object { "$($_.Kind) $($_.Name)" })

$violations = foreach ($mark in $after.Marks | Where-Object { $marked -notcontains "$($_.Kind) $($_.Name)" }) {
    $verb = if ($mark.Kind -eq 'Quarantine') { 'quarantined' } else { 'skipped' }
    if ($before.Declared.Contains($mark.Name)) {
        "$($mark.Name) ($($mark.File)) existed on $Upstream and is now $verb. Fix it instead."
    }
    elseif ($mark.Kind -eq 'Quarantine' -and $mark.Text -notmatch 'MAY-\d+') {
        "$($mark.Name) ($($mark.File)) is a new test quarantined without a MAY- issue in its reason."
    }
}

if ($violations) {
    Write-Host 'Quarantine ratchet: the set of quarantined and skipped tests may only shrink.'
    $violations | ForEach-Object { Write-Host "  $_" }
    exit 1
}
Write-Host "== Quarantine ratchet holds against $Upstream."
exit 0
