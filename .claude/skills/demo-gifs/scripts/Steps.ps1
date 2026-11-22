# Building blocks for flows in the Zero Install GUI. Each step logs events that flows use for cutting
# (see reference.md for the list). Steps listed in $SkipSteps (Record.ps1 -Skip) return immediately.

$script:Main = $null        # UIA element of the main window
$script:MainHandle = [IntPtr]::Zero
$SkipSteps = @()

function Test-Skip([string]$step) { $SkipSteps -contains $step }

#region Stage
# Returns visible top-level windows (other than the main window) that overlap a crop (default: the canvas) and would show up in the recording
function Get-StageClutter([string]$Crop = 'Canvas') {
    $c = $Crops[$Crop]
    Get-TopWindows | Where-Object {
        $r = $_.Current.BoundingRectangle
        $_.Current.NativeWindowHandle -ne $script:MainHandle.ToInt32() -and $_.Current.ClassName -ne 'Progman' -and -not $_.Current.IsOffscreen -and
        $r.X -lt $c.X + $c.W -and $r.X + $r.Width -gt $c.X -and $r.Y -lt $c.Y + $c.H -and $r.Y + $r.Height -gt $c.Y
    }
}

# Puts the main window on stage before a take starts (not recorded): starts it if necessary, moves it,
# checks that no other window overlaps the canvas, selects a tab, clears the Catalog search box and parks the cursor on the tile list.
function Initialize-Stage([string]$Tab = 'Catalog', [switch]$AllowOverlap) {
    $script:Main = Get-TopWindow '^Zero Install$'
    if (-not $script:Main) {
        Start-Process $CentralExe
        $script:Main = Wait-Until { Get-TopWindow '^Zero Install$' } 60
        Start-Sleep -Seconds 3 # let the catalog load
    }
    $script:MainHandle = [IntPtr]$script:Main.Current.NativeWindowHandle
    [Gui]::MoveWindow($script:MainHandle, $MainWindow.X, $MainWindow.Y, $MainWindow.W, $MainWindow.H, $true) | Out-Null
    $clutter = @(Get-StageClutter)
    if ($clutter -and -not $AllowOverlap) {
        throw "These windows overlap the recording area (screen x $($Crops.Canvas.X)-$($Crops.Canvas.X + $Crops.Canvas.W), y $($Crops.Canvas.Y)-$($Crops.Canvas.Y + $Crops.Canvas.H)); minimize them or move them to another monitor: " +
            (($clutter | ForEach-Object { "'$($_.Current.Name)'" }) -join ', ')
    }
    [Gui]::SetForegroundWindow($script:MainHandle) | Out-Null
    Start-Sleep -Milliseconds 500

    [Gui]::SetCursorPos($MainWindow.X + $Tabs[$Tab].X, $MainWindow.Y + $Tabs[$Tab].Y) | Out-Null
    [Gui]::Click()
    Start-Sleep -Milliseconds 800
    $searchBox = Get-CatalogSearchBox
    if ($searchBox) { [Gui]::SendMessage([IntPtr]$searchBox.Current.NativeWindowHandle, 0x000C, [IntPtr]::Zero, '') | Out-Null } # WM_SETTEXT: clear without keystrokes

    [Gui]::SetCursorPos($MainWindow.X + 430, $MainWindow.Y + 436) | Out-Null
    Start-Sleep -Seconds 1
    if ([Gui]::GetForegroundWindow() -ne $script:MainHandle) { throw 'Zero Install main window is not in the foreground' }
}

# The search box's UIA name is its placeholder ("Search") only while empty, so find it by class
function Get-CatalogSearchBox {
    $catalogPane = Get-AllElements $script:Main | Where-Object { $_.Current.Name -eq 'Catalog' } | Select-Object -First 1
    if ($catalogPane) { Get-AllElements $catalogPane | Where-Object { $_.Current.ClassName -match 'EDIT' } | Select-Object -First 1 }
}
#endregion

#region Main window
# Clicks a tab header. Events: clickTabMove, clickTab
function Select-Tab([string]$Tab) {
    ClickAt ($MainWindow.X + $Tabs[$Tab].X) ($MainWindow.Y + $Tabs[$Tab].Y) 'clickTab'
    Start-Sleep -Milliseconds 500
}

# Clicks the Catalog search box and types. Events: clickSearchMove, clickSearch, typed
function Search-Catalog([string]$Text = $App.Search) {
    $searchBox = Get-CatalogSearchBox
    if (-not $searchBox) { throw 'Catalog search box not found (is the Catalog tab selected?)' }
    $pos = Get-Center $searchBox
    ClickAt $pos.X $pos.Y 'clickSearch' 700
    Start-Sleep -Milliseconds 300
    Type-Text $Text 130 $script:MainHandle
    Log 'typed'
    Start-Sleep -Milliseconds 600
}

# Finds a visible app tile. Returns its Run button rectangle and star button center.
# Tile children: "Run" split button (64x23), star (22x22, unnamed), icon (48x48), name, summary.
function Get-AppTile([string]$TileName = $App.TileName) {
    $name = Wait-Until { Get-AllElements $script:Main | Where-Object { $_.Current.Name -eq $TileName -and -not $_.Current.IsOffscreen } | Select-Object -First 1 } 10
    $tile = [System.Windows.Automation.TreeWalker]::RawViewWalker.GetParent($name)
    $children = $tile.FindAll($TreeScope::Children, $TrueCondition)
    [pscustomobject]@{
        Run = ($children | Where-Object { $_.Current.Name -eq 'Run' }).Current.BoundingRectangle
        Star = Get-Center ($children | Where-Object { $_.Current.BoundingRectangle.Width -eq 22 } | Select-Object -First 1)
    }
}

# Clicks "Run" on the tile, waits through the download and moves the app's window on stage.
# Events: clickRunMove, clickRun, progressShown, progressClosed (both only if a download happens), appShown
# Returns the app's process.
function Invoke-RunApp([string]$TileName = $App.TileName, [string]$ProcessName = $App.ProcessName, [int]$TimeoutSec = 900) {
    if (Test-Skip 'RunApp') { return }
    $run = (Get-AppTile $TileName).Run
    ClickAt ([int]($run.X + 22)) ([int]($run.Y + $run.Height / 2)) 'clickRun' # left part of the split button

    # A separate 0install-win process shows a progress window centered on screen (it re-centers itself, so leave it alone).
    # Its title varies by version ("Zero Install" up to 2.29, the app's name since 2.30), so find it by process.
    $script:progressHandle = 0
    $script:progressClosed = $false
    $app = Wait-Until {
        if (-not $script:progressHandle) {
            $helperIds = @(Get-Process -Name '0install-win' -ErrorAction SilentlyContinue | ForEach-Object Id)
            $progress = Get-TopWindows | Where-Object { $_.Current.ProcessId -in $helperIds -and -not $_.Current.IsOffscreen } | Select-Object -First 1
            if ($progress) { $script:progressHandle = $progress.Current.NativeWindowHandle; Log 'progressShown' }
        }
        elseif (-not $script:progressClosed) {
            if (-not (Get-TopWindows | Where-Object { $_.Current.NativeWindowHandle -eq $script:progressHandle })) { $script:progressClosed = $true; Log 'progressClosed' }
        }
        Get-Process -Name $ProcessName -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowHandle -ne 0 } | Select-Object -First 1
    } $TimeoutSec

    # The app opens wherever it remembers; move it on stage (cut the frames before appShown).
    # After a slow start it may open behind the main window, so raise it.
    [Gui]::MoveWindow($app.MainWindowHandle, $AppWindow.X, $AppWindow.Y, $AppWindow.W, $AppWindow.H, $true) | Out-Null
    [Gui]::BringToFront($app.MainWindowHandle)
    Log 'appShown'
    [Gui]::MoveTo($AppWindow.X + $AppWindow.W - 180, $AppWindow.Y + $AppWindow.H - 40, 400)
    $script:RunningApp = $app
    $app
}

# Closes the app started by Invoke-RunApp by clicking the X in its title bar, then refocuses the main window.
# Apps like VLC (Qt) expose no title bar buttons via UI Automation, so the X is located from the visible window frame
# (standard caption buttons: 46 px wide, centered 15 px below the top at 100% scaling). Events: clickCloseMove, clickClose, appClosed
function Close-App([int]$MoveMs = 700) {
    if ((Test-Skip 'CloseApp') -or -not $script:RunningApp) { return }
    $frame = [Gui]::GetFrameBounds($script:RunningApp.MainWindowHandle)
    ClickAt ($frame.Right - 23) ($frame.Top + 15) 'clickClose' $MoveMs
    $id = $script:RunningApp.Id
    Wait-Until { -not (Get-Process -Id $id -ErrorAction SilentlyContinue) } 15 | Out-Null
    Log 'appClosed'
    [Gui]::SetForegroundWindow($script:MainHandle) | Out-Null
    Start-Sleep -Seconds 1
}

# Clicks the star on the tile, which adds the app to "My apps" (if needed) and opens a drop-down
# (part of the main window's UIA tree; briefly shows "Working..." first). Events: clickStarMove, clickStar, popupShown
function Open-AppPopup([string]$TileName = $App.TileName) {
    $star = (Get-AppTile $TileName).Star
    ClickAt $star.X $star.Y 'clickStar'
    Wait-Until { Get-AllElements $script:Main | Where-Object { $_.Current.Name -match '^(Integrate|Modify integration|Remove)' } | Select-Object -First 1 } 10 | Out-Null
    Log 'popupShown'
    Start-Sleep -Milliseconds 900
}

# Clicks "Integrate (start menu, etc.)" / "Modify integration" in the popup, then toggles the checkboxes named in -Check
# (e.g., 'Desktop'), then OK (or Cancel) in the Integrate dialog.
# Events: clickIntegrateMove, clickIntegrate, dialogShown, check<Name>Move, check<Name> (per checkbox), clickOKMove, clickOK, dialogClosed, integrationDone
function Invoke-Integrate([string[]]$Check = @(), [switch]$Cancel, [int]$DialogHoldMs = 1500) {
    $button = Wait-Until { Get-AllElements $script:Main | Where-Object { $_.Current.Name -match '^(Integrate|Modify integration)' } | Select-Object -First 1 } 10
    $pos = Get-Center $button
    ClickAt $pos.X $pos.Y 'clickIntegrate' 500

    $dialog = Wait-Window '^Integrate' 'dialogShown' -Center
    Start-Sleep -Milliseconds $DialogHoldMs
    foreach ($name in $Check) {
        Invoke-ClickElement $name "check$($name -replace '\W')" $dialog 500
        Start-Sleep -Milliseconds 600
    }
    $dialogProcessId = $dialog.Current.ProcessId
    Invoke-ClickElement $(if ($Cancel -or $DryRun) { 'Cancel' } else { 'OK' }) 'clickOK' $dialog 700
    Wait-WindowClosed '^Integrate' 'dialogClosed'

    # The dialog's process then shows a short "Applying desktop integration" progress window; wait for that process to exit
    Wait-Until { -not (Get-Process -Id $dialogProcessId -ErrorAction SilentlyContinue) } 120 | Out-Null
    Log 'integrationDone'
}
#endregion

#region Start menu
# Opens the Start menu and types a query. Keep the cursor out of the menu: moving it there pops up stray tooltips.
# Use a separate take, cut with -Crop StartMenu. Events: startMenuTyped, resultsShown
function Search-StartMenu([string]$Query = $App.StartMenuQuery) {
    try {
        [Gui]::Key(0x5B) # Windows key
        Start-Sleep -Milliseconds 900
        [System.Windows.Forms.SendKeys]::SendWait($Query)
        Log 'startMenuTyped'
        Start-Sleep -Milliseconds 2000
        Log 'resultsShown'
        Start-Sleep -Milliseconds 500
    }
    finally {
        [Gui]::Key(0x1B) # Escape
        Start-Sleep -Milliseconds 500
    }
}
#endregion

#region Desktop
# Waits for the app's desktop shortcut, then hovers over its icon at $DesktopIcon (no click). Use a separate take, cut with -Crop Desktop.
# Desktop icons aren't exposed via UI Automation, so the position comes from Config.ps1: the top-left slot, where a new icon
# lands on an empty desktop. No window may overlap the crop.
# Events: desktopShown, hoverIconMove, hoverIcon
function Show-DesktopIcon([string]$Name = $App.TileName) {
    if (-not $AllowOverlap) {
        $clutter = @(Get-StageClutter 'Desktop')
        if ($clutter) { throw "These windows overlap the desktop crop; minimize them or move them to another monitor: " + (($clutter | ForEach-Object { "'$($_.Current.Name)'" }) -join ', ') }
    }
    $folders = [Environment]::GetFolderPath('Desktop'), [Environment]::GetFolderPath('CommonDesktopDirectory')
    Wait-Until { $folders | Where-Object { Test-Path (Join-Path $_ "$Name.lnk") } } 10 | Out-Null
    Log 'desktopShown'
    Start-Sleep -Milliseconds 1000
    Log 'hoverIconMove'
    [Gui]::MoveTo($DesktopIcon.X, $DesktopIcon.Y, 700)
    Log 'hoverIcon'
    Start-Sleep -Milliseconds 2500
}
#endregion

#region Generic building blocks for new flows
# Clicks the first element named $Name below $Root (default: main window). Events: <Label>Move, <Label>
function Invoke-ClickElement([string]$Name, [string]$Label, $Root = $script:Main, [int]$MoveMs = 600) {
    $element = Wait-Until { Get-AllElements $Root | Where-Object { $_.Current.Name -eq $Name -and -not $_.Current.IsOffscreen } | Select-Object -First 1 } 10
    $pos = Get-Center $element
    ClickAt $pos.X $pos.Y $Label $MoveMs
}

# Waits for a top-level window whose title matches $Title; -Center moves it to $DialogCenter
# (cut the frames before the event: it is logged after the move). Events: <Label>. Returns the window.
function Wait-Window([string]$Title, [string]$Label, [switch]$Center, [int]$TimeoutSec = 60) {
    $window = Wait-Until { Get-TopWindow $Title } $TimeoutSec
    if ($Center) {
        $r = $window.Current.BoundingRectangle
        [Gui]::MoveWindow([IntPtr]$window.Current.NativeWindowHandle, [int]($DialogCenter.X - $r.Width / 2), [int]($DialogCenter.Y - $r.Height / 2), [int]$r.Width, [int]$r.Height, $true) | Out-Null
    }
    Log $Label
    $window
}

# Waits until no top-level window's title matches $Title. Events: <Label>
function Wait-WindowClosed([string]$Title, [string]$Label, [int]$TimeoutSec = 120) {
    Wait-Until { -not (Get-TopWindow $Title) } $TimeoutSec | Out-Null
    Log $Label
}
#endregion
