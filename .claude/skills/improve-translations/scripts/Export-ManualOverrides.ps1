<#
.SYNOPSIS
Writes Resources.<lang>.o.resx files containing only the manually overriden entries changed since a Git revision,
at the paths .tx/config expects, and prints (or with -Push runs) the Transifex push commands for exactly those files.

.PARAMETER Base
The Git revision to compare against. Use HEAD before committing, or the parent of the translation commit afterwards.

.PARAMETER Languages
Language codes as used in the file names (e.g. fr, pt-BR). Defaults to all.

.PARAMETER Projects
Resource names or project names under src/. Defaults to all.

.PARAMETER Remove
Deletes the .o.resx files this script would generate (for cleaning up after pushing) instead of writing them.

.PARAMETER Push
Runs the push commands after writing the files (only after the user confirmed the upload) and reports the files the
Transifex CLI skipped because the remote translation is newer, with their keys and a --force command for just those.

.PARAMETER Force
With -Push: adds --force, overwriting newer remote translations of the exported keys. Combine with -Languages and
-Projects to limit it to the files that were skipped.
#>
param(
    [string]$Base = 'HEAD',
    [string[]]$Languages,
    [string[]]$Projects,
    [switch]$Remove,
    [switch]$Push,
    [switch]$Force
)
. "$PSScriptRoot\ResxHelpers.ps1"

$tx = Get-TxConfig
$pushes = [ordered]@{}
$exported = @{} # "<resource>|<tx language>" -> [pscustomobject]@{ Project; Language; Keys }
$languageTotals = [ordered]@{}

foreach ($project in Get-ResxProjects $Projects) {
    $resource = $tx.Resources[(Get-RelativePath $project.Source)]
    if (-not $resource) {
        Write-Warning "No Transifex resource configured for $($project.Name), skipping"
        continue
    }
    $resourceId = $resource.Id

    foreach ($file in Get-LocalizedResxFiles $project $Languages) {
        $changed = @(Get-ChangedManualEntries $file.Path $Base)
        if ($changed.Count -eq 0) { continue }

        $txLanguage = if ($tx.LangMap.ContainsKey($file.Language)) { $tx.LangMap[$file.Language] } else { $file.Language }
        $outPath = Join-Path $RepoRoot $resource.FileFilter.Replace('<lang>', $file.Language)
        $relPath = Get-RelativePath $outPath
        if ($Remove) {
            if (Test-Path $outPath) {
                Remove-Item $outPath
                Write-Host "Removed $relPath"
            }
            continue
        }

        # Use the source file as a template for the resx header and schema (without WinForms designer assembly/metadata entries)
        $doc = New-Object System.Xml.XmlDocument
        $doc.Load($project.Source)
        foreach ($node in @($doc.SelectNodes('/root/data | /root/assembly | /root/metadata'))) { [void]$node.ParentNode.RemoveChild($node) }
        foreach ($entry in $changed) {
            $data = $doc.CreateElement('data')
            $data.SetAttribute('name', $entry.Key)
            [void]$data.SetAttribute('space', 'http://www.w3.org/XML/1998/namespace', 'preserve')
            $value = $doc.CreateElement('value')
            $value.InnerText = $entry.Value
            [void]$data.AppendChild($value)
            [void]$doc.DocumentElement.AppendChild($data)
        }

        $settings = New-Object System.Xml.XmlWriterSettings
        $settings.Indent = $true
        $settings.Encoding = $Utf8NoBom
        $settings.NewLineChars = "`n"
        $writer = [System.Xml.XmlWriter]::Create($outPath, $settings)
        try { $doc.Save($writer) } finally { $writer.Dispose() }
        # Keys on their own line, so that long lists wrapped by the console do not split the file name line
        $keys = @($changed | ForEach-Object Key)
        Write-Host "Wrote $relPath ($($keys.Count) entries)"
        Write-Host "  $($keys -join ', ')"

        if (-not $pushes.Contains($resourceId)) { $pushes[$resourceId] = New-Object System.Collections.ArrayList }
        [void]$pushes[$resourceId].Add($txLanguage)
        $exported["$resourceId|$txLanguage"] = [pscustomobject]@{ Project = $project.Name; Language = $file.Language; Keys = $keys }
        $languageTotals[$file.Language] += $keys.Count
    }
}

if ($Remove) { return }
if ($pushes.Count -eq 0) {
    Write-Host "No manually overriden entries changed since $Base."
    return
}

$total = ($languageTotals.Values | Measure-Object -Sum).Sum
Write-Host ''
Write-Host "$total entries in $($exported.Count) files: $(($languageTotals.Keys | Sort-Object | ForEach-Object { "$_ $($languageTotals[$_])" }) -join ', ')"

$cliUrl = 'https://apps.0install.net/devel/transifex-cli.xml'
$forceArg = if ($Force) { ' --force' } else { '' }

# One push per resource, so stale .o.resx files of other resource/language combinations are never uploaded
if (-not $Push) {
    Write-Host ''
    Write-Host 'Push commands (run from the repository root):'
    foreach ($resourceId in $pushes.Keys) {
        "0install run --batch $cliUrl push --translation$forceArg --languages $($pushes[$resourceId] -join ',') $(ConvertTo-TxCliResourceId $resourceId)"
    }
    return
}

# The CLI shortens its status lines to the console width (e.g. "[fr] - Skipping becau.."), so match only their beginning
$progressPattern = '^\s*$|^# |^\S+ - (Getting|Fetching|Done)|^\S+ \[[^\]]+\] - (Uploading|Done)'
$skipped = New-Object System.Collections.ArrayList
$failed = 0
Push-Location $RepoRoot
try {
    foreach ($resourceId in $pushes.Keys) {
        $cliResource = ConvertTo-TxCliResourceId $resourceId
        $cliArgs = @('run', '--batch', $cliUrl, 'push', '--translation') + $(if ($Force) { @('--force') } else { @() }) + @('--languages', ($pushes[$resourceId] -join ','), $cliResource)
        Write-Host ''
        Write-Host "Pushing $cliResource ($($pushes[$resourceId] -join ','))"
        # Native stderr would become terminating errors with $ErrorActionPreference = 'Stop'
        $ErrorActionPreference = 'Continue'
        $output = @(& 0install @cliArgs 2>&1 | ForEach-Object { "$_" })
        $exitCode = $LASTEXITCODE
        $ErrorActionPreference = 'Stop'
        foreach ($line in $output) {
            if ($line -match '^\S+ \[(?<lang>[^\]]+)\] - Skip') {
                [void]$skipped.Add([pscustomobject]@{ Resource = $resourceId; TxLanguage = $Matches['lang'] })
            }
            if ($line -notmatch $progressPattern) { Write-Host "  $line" }
        }
        if ($exitCode -ne 0) {
            Write-Warning "Push of $cliResource failed with exit code $exitCode"
            $failed++
        }
    }
} finally {
    Pop-Location
}

Write-Host ''
if ($skipped.Count -eq 0) {
    Write-Host "Pushed all $($exported.Count) files$(if ($failed) { ", but $failed push command(s) failed" })."
    return
}

$skippedEntries = 0
Write-Host "Skipped $($skipped.Count) file(s) because the remote translation is newer:"
foreach ($s in $skipped) {
    $file = $exported["$($s.Resource)|$($s.TxLanguage)"]
    if (-not $file) { Write-Host "  $(ConvertTo-TxCliResourceId $s.Resource) [$($s.TxLanguage)]"; continue }
    $skippedEntries += $file.Keys.Count
    Write-Host "  $($file.Project) [$($file.Language)]: $($file.Keys -join ', ')"
}
Write-Host "$skippedEntries skipped entries in total. To overwrite the remote versions of just these keys (after the user agreed):"
foreach ($group in $skipped | Group-Object Resource) {
    "0install run --batch $cliUrl push --translation --force --languages $(($group.Group | ForEach-Object TxLanguage) -join ',') $(ConvertTo-TxCliResourceId $group.Name)"
}
