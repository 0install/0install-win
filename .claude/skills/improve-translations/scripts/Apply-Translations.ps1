<#
.SYNOPSIS
Applies approved translation proposals from a JSON file to the combined Resources.<lang>.resx files.
Each changed entry gets its value replaced and its comment set to "Manually overriden.".

.DESCRIPTION
Use this instead of individual Edit tool calls when applying many proposals (e.g. from per-language subagents).
The files are modified by exact in-place text replacement of the affected <data> elements only, so the combinator's
formatting, UTF-8 BOM and LF line endings stay intact and all other entries remain byte-identical.

The JSON file is an array of objects:
  {"project": "Commands", "key": "SomeKey", "old": "<current value>", "new": "<new value>", "reason": "..."}
"project" is the resource name as printed by Get-TranslationContext.ps1.
"old" must equal the current value exactly (case-sensitive, including trailing line breaks); otherwise the proposal is
skipped with a warning, so stale proposals never overwrite newer text. Use "old": "" to add a missing entry.

Each proposal is also validated against the English source: "new" must differ from "old" and keep the source's
placeholders, access key count (&), number of line breaks and leading/trailing whitespace. Proposals that fail are skipped
with a warning (also with -WhatIf), so run with -WhatIf first to check subagent output.

.PARAMETER Path
The JSON proposals file.

.PARAMETER Language
Language code as used in the file names (e.g. fr, pt-BR).

.PARAMETER SkipKeys
Proposals to leave out, as "<Project>/<Key>".

.PARAMETER SkipValidation
Proposals that may fail validation against the English source because the difference is intended
(e.g. fixing a line break the translation was missing), as "<Project>/<Key>".

.PARAMETER WhatIf
Only reports what would be applied.
#>
param(
    [Parameter(Mandatory)][string]$Path,
    [Parameter(Mandatory)][string]$Language,
    [string[]]$SkipKeys = @(),
    [string[]]$SkipValidation = @(),
    [switch]$WhatIf
)
. "$PSScriptRoot\ResxHelpers.ps1"

function ConvertTo-XmlText([string]$Text) { $Text.Replace('&', '&amp;').Replace('<', '&lt;').Replace('>', '&gt;') }
function ConvertFrom-XmlText([string]$Text) { $Text.Replace('&lt;', '<').Replace('&gt;', '>').Replace('&quot;', '"').Replace('&apos;', "'").Replace('&amp;', '&') }

# Returns the reasons why a proposal does not match the English source (empty if it does)
function Test-Proposal([string]$Source, [string]$Old, [string]$New) {
    if ($New -ceq $Old) { 'new equals old' }
    if (((Get-Placeholders $Source) -join ',') -cne ((Get-Placeholders $New) -join ',')) { 'placeholders differ from source' }
    $accessKeys = Get-AccessKeyCount $Source
    if ($accessKeys -le 1 -and (Get-AccessKeyCount $New) -ne $accessKeys) { "expected $accessKeys access key marker(s) (&), like the source" }
    if (([regex]::Matches($Source, '\n')).Count -ne ([regex]::Matches($New, '\n')).Count) { 'number of line breaks differs from source' }
    if (($Source -match '^\s') -ne ($New -match '^\s') -or ($Source -match '\s$') -ne ($New -match '\s$')) { 'leading/trailing whitespace differs from source' }
}

$utf8Bom = New-Object System.Text.UTF8Encoding $true
# ForEach-Object unrolls the array, which Windows PowerShell 5.1's ConvertFrom-Json emits as a single object
$proposals = @([IO.File]::ReadAllText((Resolve-Path $Path), [Text.Encoding]::UTF8) | ConvertFrom-Json | ForEach-Object { $_ })
$applied = 0

$projects = @{}
foreach ($project in Get-ResxProjects) { $projects[$project.Name] = $project }

foreach ($group in $proposals | Group-Object project) {
    if (-not $projects.ContainsKey($group.Name)) { Write-Warning "Unknown project '$($group.Name)', skipped $($group.Count) proposal(s)"; continue }
    $file = Get-LocalizedResxPath $projects[$group.Name] $Language
    $text = [IO.File]::ReadAllText($file, [Text.Encoding]::UTF8).TrimStart([char]0xFEFF)
    $source = Read-ResxFile $projects[$group.Name].Source

    foreach ($p in $group.Group) {
        $id = "$($p.project)/$($p.key)"
        if ($SkipKeys -contains $id) { continue }
        $new = ([string]$p.new).Replace("`r`n", "`n")
        $old = ([string]$p.old).Replace("`r`n", "`n")

        if (-not $source.Contains($p.key)) { Write-Warning "[$id] Key does not exist in the source, skipped"; continue }
        if ($SkipValidation -notcontains $id) {
            $problems = @(Test-Proposal $source[$p.key].Value $old $new)
            if ($problems) { Write-Warning "[$id] $($problems -join '; '), skipped"; continue }
        }

        $pattern = '(?s)(  <data name="' + [regex]::Escape($p.key) + '" xml:space="preserve">\n    <value>)(.*?)(</value>)(\n    <comment>[^<]*</comment>)?(\n  </data>)'
        $match = [regex]::Match($text, $pattern)
        if (-not $match.Success) {
            if ($old) { Write-Warning "[$id] Entry not found, skipped"; continue }
            $block = "  <data name=`"$($p.key)`" xml:space=`"preserve`">`n    <value>$(ConvertTo-XmlText $new)</value>`n    <comment>$ManualComment</comment>`n  </data>`n"
            $text = $text.Replace('</root>', $block + '</root>')
            $applied++
            continue
        }

        $current = ConvertFrom-XmlText $match.Groups[2].Value
        if ($current -cne $old) { Write-Warning "[$id] Current value differs from 'old', skipped"; continue }
        if ($current -ceq $new -and $match.Groups[4].Value -eq "`n    <comment>$ManualComment</comment>") { continue }

        $replacement = $match.Groups[1].Value + (ConvertTo-XmlText $new) + $match.Groups[3].Value + "`n    <comment>$ManualComment</comment>" + $match.Groups[5].Value
        $text = $text.Substring(0, $match.Index) + $replacement + $text.Substring($match.Index + $match.Length)
        $applied++
    }

    if (-not $WhatIf) { [IO.File]::WriteAllText($file, $text, $utf8Bom) }
}

Write-Host "${Language}: $(if ($WhatIf) { 'would apply' } else { 'applied' }) $applied of $($proposals.Count) proposal(s)"
