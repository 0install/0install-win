# Low-level helpers: Win32 mouse/keyboard/window control, UI Automation lookups, event log, screen recording.
# Not specific to Zero Install; Steps.ps1 builds on these.

Add-Type -AssemblyName System.Windows.Forms, System.Drawing, UIAutomationClient, UIAutomationTypes

Add-Type -ReferencedAssemblies System.Drawing, System.Windows.Forms @'
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

public static class Gui
{
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] public static extern bool GetCursorPos(out POINT p);
    [DllImport("user32.dll")] public static extern void mouse_event(uint flags, int dx, int dy, uint data, IntPtr extra);
    [DllImport("user32.dll")] public static extern void keybd_event(byte vk, byte scan, uint flags, IntPtr extra);
    [DllImport("user32.dll")] public static extern bool MoveWindow(IntPtr h, int x, int y, int w, int hh, bool repaint);
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern IntPtr SendMessage(IntPtr h, uint msg, IntPtr w, string l);

    [StructLayout(LayoutKind.Sequential)] public struct POINT { public int X, Y; }

    // Moves the cursor with an ease-in-out curve so it looks like a human hand
    public static void MoveTo(int x, int y, int ms)
    {
        POINT p; GetCursorPos(out p);
        int steps = Math.Max(1, ms / 10);
        for (int i = 1; i <= steps; i++)
        {
            double t = (double)i / steps;
            double e = t < 0.5 ? 2 * t * t : 1 - Math.Pow(-2 * t + 2, 2) / 2;
            SetCursorPos((int)Math.Round(p.X + (x - p.X) * e), (int)Math.Round(p.Y + (y - p.Y) * e));
            Thread.Sleep(10);
        }
    }

    public static void Click()
    {
        mouse_event(0x0002, 0, 0, 0, IntPtr.Zero); // left down
        Thread.Sleep(60);
        mouse_event(0x0004, 0, 0, 0, IntPtr.Zero); // left up
    }

    public static void Key(byte vk)
    {
        keybd_event(vk, 0, 0, IntPtr.Zero);
        Thread.Sleep(30);
        keybd_event(vk, 0, 2, IntPtr.Zero);
    }

    public static void Screenshot(int x, int y, int w, int h, string path)
    {
        using (var bmp = new Bitmap(w, h))
        {
            using (var g = Graphics.FromImage(bmp)) g.CopyFromScreen(x, y, 0, 0, new Size(w, h));
            bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
'@

#region UI Automation
$UIA = [System.Windows.Automation.AutomationElement]
$TreeScope = [System.Windows.Automation.TreeScope]
$TrueCondition = [System.Windows.Automation.Condition]::TrueCondition

function Get-TopWindows { $UIA::RootElement.FindAll($TreeScope::Children, $TrueCondition) }
function Get-TopWindow([string]$titlePattern) { Get-TopWindows | Where-Object { $_.Current.Name -match $titlePattern } | Select-Object -First 1 }
function Get-AllElements($root) { $root.FindAll($TreeScope::Descendants, $TrueCondition) }

function Get-Center($element) {
    $r = $element.Current.BoundingRectangle
    [pscustomobject]@{ X = [int]($r.X + $r.Width / 2); Y = [int]($r.Y + $r.Height / 2) }
}

# Prints the UI Automation tree below an element (for discovering names when writing new flows)
function Show-Elements($root) {
    Get-AllElements $root | ForEach-Object {
        '{0} | {1} | {2} | {3}' -f $_.Current.ControlType.ProgrammaticName, $_.Current.Name, $_.Current.ClassName, $_.Current.BoundingRectangle
    }
}

function Wait-Until([scriptblock]$condition, [int]$timeoutSec = 30) {
    $sw = [Diagnostics.Stopwatch]::StartNew()
    while ($sw.Elapsed.TotalSeconds -lt $timeoutSec) {
        $result = & $condition
        if ($result) { return $result }
        Start-Sleep -Milliseconds 50
    }
    throw "Timed out waiting for condition: $condition"
}
#endregion

#region Event log (wall-clock ms; ffmpeg's gdigrab with -copyts stores wall-clock timestamps, so both line up)
$script:Events = [System.Collections.Generic.List[object]]::new()
function Reset-Events { $script:Events.Clear() }
function Log([string]$type, $x = $null, $y = $null) {
    $script:Events.Add([pscustomobject]@{ t = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds(); type = $type; x = $x; y = $y })
}
function Save-Events([string]$path) { ConvertTo-Json @($script:Events) | Set-Content $path -Encoding utf8 }
function Show-Events {
    $t0 = $script:Events[0].t
    $script:Events | Format-Table type, @{ n = 'sec'; e = { ($_.t - $t0) / 1000 } }, x, y | Out-String | Write-Host
}
#endregion

#region Safe input: never click outside the canvas, never type into a window we did not expect
function Assert-OnCanvas([int]$x, [int]$y) {
    $c = $Crops.Canvas
    if ($x -lt $c.X -or $x -gt $c.X + $c.W -or $y -lt $c.Y -or $y -gt $c.Y + $c.H) {
        throw "Refusing to click outside the canvas: $x,$y"
    }
}

# Logs "<label>Move" when the cursor starts moving and "<label>" (with coordinates) at the click.
# Build.ps1 draws a ring at every logged click and flows cut relative to both events.
function ClickAt([int]$x, [int]$y, [string]$label, [int]$moveMs = 600) {
    Assert-OnCanvas $x $y
    Log "${label}Move"
    [Gui]::MoveTo($x, $y, $moveMs)
    Start-Sleep -Milliseconds 120
    Log $label $x $y
    [Gui]::Click()
}

function Type-Text([string]$text, [int]$delayMs = 130, [IntPtr]$expectedWindow = [IntPtr]::Zero) {
    foreach ($c in $text.ToCharArray()) {
        if ($expectedWindow -ne [IntPtr]::Zero -and [Gui]::GetForegroundWindow() -ne $expectedWindow) {
            throw 'Refusing to type: target window is not in the foreground'
        }
        [System.Windows.Forms.SendKeys]::SendWait($(if ('+^%~(){}[]'.Contains([string]$c)) { "{$c}" } else { [string]$c }))
        Start-Sleep -Milliseconds $delayMs
    }
}
#endregion

#region Recording
function Start-Recording([string]$path) {
    $psi = [Diagnostics.ProcessStartInfo]::new((Get-Command ffmpeg).Source)
    $psi.Arguments = "-hide_banner -loglevel error -y -f gdigrab -framerate $Fps -offset_x $($Capture.X) -offset_y $($Capture.Y) -video_size $($Capture.W)x$($Capture.H) -i desktop -copyts -c:v libx264rgb -crf 0 -preset ultrafast `"$path`""
    $psi.UseShellExecute = $false
    $psi.RedirectStandardInput = $true
    $process = [Diagnostics.Process]::Start($psi)
    Start-Sleep -Milliseconds 1500 # let ffmpeg settle before the first action
    $process
}

function Stop-Recording($process) {
    $process.StandardInput.Write('q')
    $process.WaitForExit(30000) | Out-Null
}
#endregion
