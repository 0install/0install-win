<#
.SYNOPSIS
Cuts a recording into <OutDir>\<name>.gif (plus an .mp4 preview) using the flow's Get-Cuts function.
Cut points are derived from the recorded event logs, so re-recordings need no manual re-timing.

.PARAMETER Flow
Name of a flow file in ..\flows (without .ps1); provides Get-Cuts.
.PARAMETER Recording
Recording directory to cut (default: the flow name). Any recording containing the events the flow's
cuts need will do, e.g., "run-app" can be cut from the "run-and-integrate" recording.
.PARAMETER Name
Output file name without extension (default: the flow name).
.PARAMETER Review
Also writes <name>-review.png, a contact sheet with 4 frames per second.
#>
param(
    [Parameter(Mandatory)][string]$Flow,
    [string]$Recording,
    [string]$Name,
    [switch]$Review
)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\Config.ps1"
$Stage = @{ Tab = 'Catalog' }
. "$PSScriptRoot\..\flows\$Flow.ps1"
if (-not $Recording) { $Recording = $Flow }
if (-not $Name) { $Name = $Flow }
$recDir = Join-Path $OutDir "recordings\$Recording"
if (-not (Test-Path $recDir)) { throw "Recording not found: $recDir" }

$OutputFps = 15
$RingSeconds = 0.4

#region Events: $EventLog[take][type] = { t (seconds on that take's video), x, y }
$EventLog = @{}
foreach ($file in Get-ChildItem $recDir -Filter '*-events.json') {
    $take = $file.Name -replace '-events\.json$'
    # gdigrab + -copyts stores wall-clock timestamps; the first frame's timestamp maps the event log onto the video
    $firstPts = [double](ffprobe -v error -select_streams v -show_entries packet=pts_time -read_intervals '%+#1' -of csv=p=0 "$recDir\$take.mkv")
    $EventLog[$take] = @{}
    foreach ($e in (Get-Content $file.FullName -Raw | ConvertFrom-Json)) {
        $EventLog[$take][$e.type] = [pscustomobject]@{ t = $e.t / 1000 - $firstPts; x = $e.x; y = $e.y }
    }
}

# Finds an event by name in any take (must be unique) or as "take:event"
function Find-Event([string]$Event) {
    if ($Event -match '^(.+):(.+)$') { return $EventLog[$Matches[1]][$Matches[2]] }
    $found = @(foreach ($take in $EventLog.Keys) { if ($EventLog[$take][$Event]) { $EventLog[$take][$Event] } })
    if ($found.Count -gt 1) { throw "Event '$Event' occurs in several takes; use 'take:$Event'" }
    $found | Select-Object -First 1
}
#endregion

#region Cut DSL used by the flows' Get-Cuts functions
# Time of an event in seconds (+ offset). -Fallback is used if the event is missing.
function T([string]$Event, [double]$Offset = 0, $Fallback = $null) {
    $e = Find-Event $Event
    if ($e) { return $e.t + $Offset }
    if ($null -ne $Fallback) { return [double]$Fallback }
    throw "Event '$Event' not found in recording '$Recording'"
}

function Test-Event([string]$Event) { [bool](Find-Event $Event) }

# Appends a clip of a take: [Start, End) seconds, played -Speed times faster, from -Crop (see $Crops),
# optionally followed by its last frame frozen for -Freeze seconds
$script:Clips = [System.Collections.Generic.List[object]]::new()
function Clip([string]$Take, [double]$Start, [double]$End, [double]$Speed = 1, [string]$Crop = 'Canvas', [double]$Freeze = 0) {
    if ($End -le $Start) { throw "Clip of take '$Take' is empty: [$Start, $End]" }
    if (-not $Crops[$Crop]) { throw "Unknown crop '$Crop'" }
    $script:Clips.Add([pscustomobject]@{ Take = $Take; Start = $Start; End = $End; Speed = $Speed; Crop = $Crop; Freeze = $Freeze })
}
#endregion

Get-Cuts
if ($script:Clips.Count -eq 0) { throw "Flow '$Flow' produced no clips" }

#region Intermediate videos: one per (take, crop), scaled to canvas size, with a ring at every logged click
$canvas = $Crops.Canvas
magick -size 44x44 xc:none -fill 'rgba(255,190,0,0.30)' -stroke 'rgba(255,160,0,0.95)' -strokewidth 3 -draw 'circle 22,22 22,4' "$recDir\ring.png"
$sources = @($script:Clips | ForEach-Object { "$($_.Take).$($_.Crop)" } | Select-Object -Unique)
foreach ($source in $sources) {
    $take, $cropName = $source -split '\.'
    $c = $Crops[$cropName]
    $sx = $canvas.W / $c.W; $sy = $canvas.H / $c.H
    $chain = "[0:v]setpts=PTS-STARTPTS,fps=$Fps,crop=$($c.W):$($c.H):$($c.X - $Capture.X):$($c.Y - $Capture.Y)"
    if ($c.W -ne $canvas.W -or $c.H -ne $canvas.H) { $chain += ",scale=$($canvas.W):$($canvas.H):flags=lanczos" }
    $chain += '[c0]'
    $i = 0
    foreach ($e in $EventLog[$take].GetEnumerator() | Where-Object { $null -ne $_.Value.x -and $_.Key -notlike '*Move' }) {
        $x = ($e.Value.x - $c.X) * $sx; $y = ($e.Value.y - $c.Y) * $sy
        if ($x -lt 0 -or $y -lt 0 -or $x -gt $canvas.W -or $y -gt $canvas.H) { continue }
        $chain += ";[c$i][1:v]overlay=x=$([int]$x - 22):y=$([int]$y - 22):shortest=1:enable='between(t,$($e.Value.t),$($e.Value.t + $RingSeconds))'[c$($i + 1)]"
        $i++
    }
    ffmpeg -hide_banner -loglevel error -y -i "$recDir\$take.mkv" -loop 1 -i "$recDir\ring.png" -filter_complex $chain -map "[c$i]" -c:v libx264rgb -crf 0 -preset ultrafast "$recDir\$source.mkv"
}
#endregion

#region Concatenate clips and encode
$inputs = @(); foreach ($source in $sources) { $inputs += @('-i', "$recDir\$source.mkv") }
$filter = ''; $labels = ''; $n = 0
foreach ($clip in $script:Clips) {
    $index = [array]::IndexOf($sources, "$($clip.Take).$($clip.Crop)")
    $filter += "[${index}:v]trim=start=$($clip.Start):end=$($clip.End),setpts=(PTS-STARTPTS)/$($clip.Speed)"
    if ($clip.Freeze -gt 0) { $filter += ",tpad=stop_mode=clone:stop_duration=$($clip.Freeze)" }
    $filter += "[s$n];"
    $labels += "[s$n]"
    $n++
}
$concat = "$filter${labels}concat=n=${n}:v=1:a=0,fps=$OutputFps"
$output = Join-Path $OutDir $Name
ffmpeg -hide_banner -loglevel error -y @inputs -filter_complex "$concat,split[a][b];[a]palettegen=max_colors=256:stats_mode=full[p];[b][p]paletteuse=dither=sierra2_4a:diff_mode=rectangle" "$output.gif"
ffmpeg -hide_banner -loglevel error -y @inputs -filter_complex $concat -c:v libx264 -crf 18 -pix_fmt yuv420p "$output.mp4"
$duration = [double](ffprobe -v error -show_entries format=duration -of csv=p=0 "$output.mp4")
"{0}.gif: {1} clips, {2:N1}s, {3:N0} KB" -f $output, $n, $duration, ((Get-Item "$output.gif").Length / 1KB)

if ($Review) {
    $dir = "$output-review"
    New-Item -ItemType Directory -Force $dir | Out-Null
    Get-ChildItem $dir -Filter *.png | Remove-Item
    ffmpeg -hide_banner -loglevel error -y -i "$output.gif" -vf 'fps=4,scale=243:-1' "$dir\f%03d.png"
    magick montage "$dir\f*.png" -tile 10x -geometry +1+1 "$output-review.png"
    "Review sheet: $output-review.png"
}
#endregion
