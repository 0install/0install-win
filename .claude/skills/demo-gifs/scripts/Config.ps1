# Shared settings for Record.ps1 and Build.ps1. Flow files may override any of these.
# All coordinates are screen pixels on the primary monitor (1920x1080 at 100% scaling).

# Zero Install GUI ("Central") to record
$CentralExe = 'C:\Program Files\Zero Install\ZeroInstall.exe'

# Recordings go to <OutDir>\recordings\<name>\, finished GIFs to <OutDir>\<flow>.gif
$RepoRoot = (Resolve-Path "$PSScriptRoot\..\..\..\..").Path
$OutDir = Join-Path $RepoRoot 'artifacts\demo-gifs'

# Default app used by the steps (each step also takes explicit parameters)
$App = @{
    FeedUri = 'https://apps.0install.net/gui/vlc.xml'
    Search = 'vlc'                 # typed into the Catalog search box
    TileName = 'VLC media player'  # name shown on the app's tile
    ProcessName = 'vlc'            # process whose main window signals "app is running"
    StartMenuQuery = 'apps: vlc'   # "apps:" keeps personal files out of the Start menu results
}

# Window layout
$MainWindow = @{ X = 590; Y = 264; W = 540; H = 551 } # Zero Install main window
$Tabs = @{                                             # tab headers, relative to the main window (not exposed via UI Automation)
    'My apps' = @{ X = 45; Y = 51 }
    'Catalog' = @{ X = 100; Y = 51 }
}
$AppWindow = @{ X = 640; Y = 300; W = 620; H = 480 }  # where a launched app's window is moved
$DialogCenter = @{ X = 945; Y = 545 }                  # where dialogs opened by Wait-Window -Center are centered

# Recording
$Capture = @{ X = 0; Y = 140; W = 1320; H = 900 }      # region grabbed by ffmpeg; must contain every crop
$Fps = 30                                              # capture frame rate

# Crops a clip can use. The GIF has the size of Canvas; other crops are scaled to it (keep the same aspect ratio).
$Crops = @{
    Canvas = @{ X = 580; Y = 250; W = 730; H = 580 }    # main window, centered progress window, star popup, centered dialogs
    StartMenu = @{ X = 13; Y = 155; W = 832; H = 661 }  # Start menu with a left-aligned taskbar
}
