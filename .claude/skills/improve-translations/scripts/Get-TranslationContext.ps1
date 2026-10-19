<#
.SYNOPSIS
Dumps localized strings together with their English source and the code that uses them, for translation review.

.DESCRIPTION
For Properties/Resources.resx strings, the usages are references to Resources.<Key> in C# code.
For WinForms form strings (keys like buttonHelp.Text), the usages are the control's declaration in the form's
.Designer.cs file (which shows its type) followed by references to the control in the form's code.

.PARAMETER Languages
Language codes as used in the file names (e.g. fr, pt-BR). Defaults to all.

.PARAMETER Projects
Resource names or project names under src/). Defaults to all.

.PARAMETER Status
Only include translations that are automatically translated (Auto), manually overriden (Manual) or not translated at all (Missing). Defaults to all.

.PARAMETER Keys
Only include these resource keys.

.PARAMETER MaxUsages
Maximum number of code usages to show per key. 0 omits usages (e.g. for a glossary dump).

.PARAMETER OutFile
Writes the output to this file (UTF-8) instead of the pipeline.
#>
param(
    [string[]]$Languages,
    [string[]]$Projects,
    [ValidateSet('All', 'Auto', 'Manual', 'Missing')][string]$Status = 'All',
    [string[]]$Keys,
    [int]$MaxUsages = 3,
    [string]$OutFile
)
. "$PSScriptRoot\ResxHelpers.ps1"

function Get-Snippet([string[]]$Lines, [int]$Index) {
    $from = [Math]::Max(0, $Index - 1)
    $to = [Math]::Min($Lines.Length - 1, $Index + 1)
    ($Lines[$from..$to] | ForEach-Object { '    | ' + $_.Trim() }) -join "`n"
}

# Index all references to Resources.<Key> in C# code (excluding generated designer files)
$usages = @{}
Get-ChildItem "$RepoRoot\src" -Recurse -Filter *.cs |
    Where-Object { $_.Name -notlike '*.Designer.cs' -and $_.FullName -notmatch '\\(bin|obj)\\' } |
    ForEach-Object {
        $file = Get-RelativePath $_.FullName
        $lines = [IO.File]::ReadAllLines($_.FullName)
        for ($i = 0; $i -lt $lines.Length; $i++) {
            foreach ($match in [regex]::Matches($lines[$i], '\bResources\.(\w+)')) {
                $key = $match.Groups[1].Value
                if (-not $usages.ContainsKey($key)) { $usages[$key] = New-Object System.Collections.ArrayList }
                [void]$usages[$key].Add([pscustomobject]@{ File = $file; Line = $i + 1; Snippet = Get-Snippet $lines $i })
            }
        }
    }

# Returns the declaration and code references of the control a WinForms form key (e.g. buttonHelp.Text, $this.Text) belongs to
function Get-FormUsages($Project, [string]$Key) {
    $control = $Key.Substring(0, $Key.LastIndexOf('.'))
    $designer = Join-Path $Project.Directory "$($Project.BaseName).Designer.cs"
    $code = Join-Path $Project.Directory "$($Project.BaseName).cs"
    if ($control -eq '$this') {
        $lines = [IO.File]::ReadAllLines($designer)
        for ($i = 0; $i -lt $lines.Length; $i++) {
            if ($lines[$i] -match '\bpartial class \w+') {
                [pscustomobject]@{ File = Get-RelativePath $designer; Line = $i + 1; Snippet = '    | ' + $lines[$i].Trim() + ' (the window itself)' }
                break
            }
        }
        return
    }
    $pattern = '\b' + [regex]::Escape($control) + '\b'
    if (Test-Path $designer) {
        $lines = [IO.File]::ReadAllLines($designer)
        for ($i = 0; $i -lt $lines.Length; $i++) {
            if ($lines[$i] -match "^\s*(private|internal|protected|public)\b.*\s$([regex]::Escape($control));") {
                [pscustomobject]@{ File = Get-RelativePath $designer; Line = $i + 1; Snippet = '    | ' + $lines[$i].Trim() }
            }
        }
    }
    if (Test-Path $code) {
        $lines = [IO.File]::ReadAllLines($code)
        for ($i = 0; $i -lt $lines.Length; $i++) {
            if ($lines[$i] -match $pattern) {
                [pscustomobject]@{ File = Get-RelativePath $code; Line = $i + 1; Snippet = Get-Snippet $lines $i }
            }
        }
    }
}

function Get-StatusLabel($Entry) {
    if (-not $Entry -or -not $Entry.Value) { 'missing' }
    elseif ($Entry.Comment -eq $ManualComment) { 'manual' }
    elseif ($Entry.Comment -eq $AutoComment) { 'auto' }
    else { 'unknown' }
}

$output = foreach ($project in Get-ResxProjects $Projects) {
    $source = Read-ResxFile $project.Source
    $projectPrefix = "src/$($project.ProjectName)/"
    $translations = @(Get-LocalizedResxFiles $project $Languages | ForEach-Object {
        [pscustomobject]@{ Language = $_.Language; Entries = Read-ResxFile $_.Path }
    })

    foreach ($key in Get-LocalizableKeys $project $source) {
        if ($Keys -and $Keys -notcontains $key) { continue }

        $rows = foreach ($translation in $translations) {
            $entry = $translation.Entries[$key]
            $label = Get-StatusLabel $entry
            if ($Status -eq 'All' -or $Status -eq $label) {
                "$($translation.Language) [$label]: $(if ($entry) { $entry.Value })"
            }
        }
        if (-not $rows) { continue }

        "## $($project.Name): $key"
        "en: $($source[$key].Value)"
        $allUsages = @()
        if ($project.IsForm) {
            $allUsages = @(Get-FormUsages $project $key)
        } elseif ($usages.ContainsKey($key)) {
            # Prefer usages in the same project and non-test code
            $allUsages = @($usages[$key] | Sort-Object @{ Expression = { -not $_.File.StartsWith($projectPrefix) } }, @{ Expression = { $_.File -like '*Tests*' } }, File, Line)
        }
        if ($MaxUsages -le 0) {
            # Usages not requested (e.g. for a glossary dump)
        } elseif ($allUsages.Count -gt 0) {
            'used:'
            foreach ($usage in $allUsages | Select-Object -First $MaxUsages) {
                "  $($usage.File):$($usage.Line)"
                $usage.Snippet
            }
            if ($allUsages.Count -gt $MaxUsages) { "  (+$($allUsages.Count - $MaxUsages) more)" }
        } else {
            'used: (no direct references found)'
        }
        $rows
        ''
    }
}

if ($OutFile) {
    [IO.File]::WriteAllText($ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutFile),($output -join "`n"), $Utf8NoBom)
    Write-Host "Wrote $(@($output | Where-Object { $_ -like '## *' }).Count) keys to $OutFile"
} else {
    $output
}
