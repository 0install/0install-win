<#
.SYNOPSIS
Records the takes of a flow: for each take, puts the GUI on stage, starts ffmpeg, runs the take's steps
and writes <OutDir>\recordings\<recording>\<take>.mkv plus <take>-events.json.
Do not touch the mouse or keyboard while this runs.

.PARAMETER Flow
Name of a flow file in ..\flows (without .ps1).
.PARAMETER Take
Record only these takes (default: all takes of the flow, in order).
.PARAMETER Recording
Name of the recording directory (default: the flow name). Use a different name for rehearsals.
.PARAMETER DryRun
Click "Cancel" instead of "OK" in dialogs (Invoke-Integrate).
.PARAMETER Skip
Steps to skip, e.g. RunApp, CloseApp (for rehearsing without a download).
.PARAMETER AllowOverlap
Record even if other windows overlap the canvas (they will be visible in the video; for rehearsals only).
.PARAMETER Search
.PARAMETER TileName
.PARAMETER ProcessName
.PARAMETER StartMenuQuery
Override the corresponding $App settings (e.g., to rehearse with an already cached app).
#>
param(
    [Parameter(Mandatory)][string]$Flow,
    [string[]]$Take,
    [string]$Recording,
    [switch]$DryRun,
    [string[]]$Skip = @(),
    [switch]$AllowOverlap,
    [string]$Search,
    [string]$TileName,
    [string]$ProcessName,
    [string]$StartMenuQuery
)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\Config.ps1"
. "$PSScriptRoot\Helpers.ps1"
. "$PSScriptRoot\Steps.ps1"
$Stage = @{ Tab = 'Catalog' }
. "$PSScriptRoot\..\flows\$Flow.ps1"

foreach ($key in 'Search', 'TileName', 'ProcessName', 'StartMenuQuery') {
    if ($PSBoundParameters[$key]) { $App[$key] = $PSBoundParameters[$key] }
}
$SkipSteps = $Skip
$dir = Join-Path $OutDir "recordings\$(if ($Recording) { $Recording } else { $Flow })"
New-Item -ItemType Directory -Force $dir | Out-Null

foreach ($name in $(if ($Take) { $Take } else { @($Takes.Keys) })) {
    if (-not $Takes[$name]) { throw "Flow '$Flow' has no take '$name'" }
    Initialize-Stage $Stage.Tab -AllowOverlap:$AllowOverlap
    Write-Host "Recording take '$name' to $dir"
    Reset-Events
    $ffmpeg = Start-Recording "$dir\$name.mkv"
    Log 'start'
    try {
        & $Takes[$name]
        Log 'end'
    }
    finally {
        Stop-Recording $ffmpeg
        Save-Events "$dir\$name-events.json"
    }
    Show-Events
}
