<#
.SYNOPSIS
Checks localized .resx files for mechanical translation problems (broken XML, placeholder mismatches, lost line breaks, etc.).
Exits with code 1 if any errors are found.

.PARAMETER Languages
Language codes as used in the file names (e.g. fr, pt-BR). Defaults to all.

.PARAMETER Projects
Resource names or project names under src/. Defaults to all.

.PARAMETER Base
If set, only checks manually overriden entries changed since this Git revision (e.g. HEAD).

.PARAMETER CountStraySpaces
Also prints how many entries per file have a stray space between a placeholder and a quote mark (DeepL artifact).
Unlike the warning for changed entries, this counts all entries, regardless of -Base.
#>
param(
    [string[]]$Languages,
    [string[]]$Projects,
    [string]$Base,
    [switch]$CountStraySpaces
)
. "$PSScriptRoot\ResxHelpers.ps1"

$errors = 0
$warnings = 0
function Report([string]$Level, [string]$Location, [string]$Message) {
    if ($Level -eq 'ERROR') { $script:errors++ } else { $script:warnings++ }
    "$Level $Location`: $Message"
}

# Built from char codes so this file stays ASCII-only (Windows PowerShell 5.1 reads BOM-less scripts as ANSI)
$closingQuotes = -join ([char[]](0x00BB, 0x201C, 0x201D, 0x2019, 0x300D))
$openingQuotes = -join ([char[]](0x00AB, 0x201E, 0x201C, 0x300C))
$placeholder = '\{\d+(?:[,:][^}]*)?\}'
$StraySpacePattern = "$placeholder [$closingQuotes]|[$openingQuotes] $placeholder"
$FrenchQuotePattern = "$([char]0x00AB) $placeholder $([char]0x00BB)"

# Opening and closing quote marks that may enclose a placeholder, covering the conventions of all supported languages
$validQuotePairs = @(
    '""', "''",
    -join ([char[]](0x00AB, 0x00BB)), -join ([char[]](0x00BB, 0x00AB)), # << >> and >> <<
    -join ([char[]](0x201E, 0x201C)), -join ([char[]](0x201E, 0x201D)), # low-9 with left or right double quote
    -join ([char[]](0x201C, 0x201D)), -join ([char[]](0x201D, 0x201D)), # English-style and Nordic double quotes
    -join ([char[]](0x2018, 0x2019)), -join ([char[]](0x201A, 0x2018)), -join ([char[]](0x201A, 0x2019)), # single quotes
    -join ([char[]](0x300C, 0x300D)), -join ([char[]](0x300E, 0x300F)) # CJK corner brackets
)
$quoteClass = '[' + [regex]::Escape((-join (($validQuotePairs -join '').ToCharArray() | Sort-Object -Unique -CaseSensitive))) + ']'
$QuotedPlaceholderPattern = "(?<open>$quoteClass) ?$placeholder ?(?<close>$quoteClass)"

function Get-LineBreakCount([string]$Text) {
    ([regex]::Matches($Text, '\n')).Count
}

$straySpaceCounts = [ordered]@{}

foreach ($project in Get-ResxProjects $Projects) {
    $source = Read-ResxFile $project.Source
    foreach ($file in Get-LocalizedResxFiles $project $Languages) {
        $relPath = Get-RelativePath $file.Path
        try {
            $entries = Read-ResxFile $file.Path
        } catch {
            Report 'ERROR' $relPath "Invalid XML: $($_.Exception.Message)"
            continue
        }

        $keys = if ($Base) { Get-ChangedManualEntries $file.Path $Base | ForEach-Object Key } else { $entries.Keys }
        foreach ($key in @($keys)) {
            $entry = $entries[$key]
            $location = "$relPath [$key]"
            $original = $source[$key]
            if (-not $original) {
                Report 'WARN' $location 'Key does not exist in the source Resources.resx'
                continue
            }
            if ($original.Value -and -not $entry.Value) {
                Report 'ERROR' $location 'Translation is empty'
                continue
            }
            if ($entry.Comment -ne $ManualComment -and $entry.Comment -ne $AutoComment) {
                Report 'WARN' $location "Unexpected comment '$($entry.Comment)'"
            }

            $expected = (Get-Placeholders $original.Value) -join ','
            $actual = (Get-Placeholders $entry.Value) -join ','
            if ($expected -ne $actual) {
                Report 'ERROR' $location "Placeholders {$actual} do not match source {$expected}"
            }
            if ((Get-LineBreakCount $original.Value) -ne (Get-LineBreakCount $entry.Value)) {
                Report 'WARN' $location 'Number of line breaks differs from source'
            }
            if (($original.Value -match '^\s') -ne ($entry.Value -match '^\s') -or ($original.Value -match '\s$') -ne ($entry.Value -match '\s$')) {
                Report 'WARN' $location 'Leading/trailing whitespace differs from source'
            }
            if ($original.Value -match '[.:?!]$' -and $entry.Value -notmatch '\p{Po}$') {
                Report 'WARN' $location 'Source ends with punctuation but translation does not'
            }
            # DeepL drops access keys, so only checked for changed and manual entries
            $expectedAccessKeys = Get-AccessKeyCount $original.Value
            if (($Base -or $entry.Comment -eq $ManualComment) -and $expectedAccessKeys -le 1 -and (Get-AccessKeyCount $entry.Value) -ne $expectedAccessKeys) {
                Report 'WARN' $location "Expected $expectedAccessKeys access key marker(s) (&), like the source"
            }
            # WinForms controls are laid out for the English text
            if ($project.IsForm -and $entry.Value.Length -gt $original.Value.Length * 1.6 + 8) {
                Report 'WARN' $location 'Much longer than source; check that it fits the layout'
            }
            # DeepL artifact: stray space between a placeholder and a typographic quote on one side only (French-style spacing on both sides is fine).
            # Only checked for changed entries, since hundreds of untouched auto entries have it.
            if ($Base -and $entry.Value -match $StraySpacePattern -and $entry.Value -notmatch $FrenchQuotePattern) {
                Report 'WARN' $location 'Stray space between placeholder and quote mark'
            }
            foreach ($match in [regex]::Matches($entry.Value, $QuotedPlaceholderPattern)) {
                $pair = $match.Groups['open'].Value + $match.Groups['close'].Value
                if ($validQuotePairs -cnotcontains $pair) {
                    Report 'WARN' $location "Mismatched quote marks around placeholder: $($match.Value)"
                }
            }
        }

        if ($CountStraySpaces) {
            $straySpaceCounts[$relPath] = @($entries.Values | Where-Object { $_.Value -match $StraySpacePattern -and $_.Value -notmatch $FrenchQuotePattern }).Count
        }
    }
}

if ($CountStraySpaces) {
    foreach ($path in $straySpaceCounts.Keys) {
        if ($straySpaceCounts[$path]) { "$path`: $($straySpaceCounts[$path]) entries with stray spaces" }
    }
    $total = ($straySpaceCounts.Values | Measure-Object -Sum).Sum
    Write-Host "$total entries with stray spaces between placeholders and quote marks"
}

Write-Host "$errors error(s), $warnings warning(s)"
if ($errors -gt 0) { exit 1 }
