# Demo GIFs: reference

## Files

| File | Purpose |
|---|---|
| `scripts\Config.ps1` | Settings: Zero Install path, output dir, default `$App`, window layout, capture region, crops |
| `scripts\Helpers.ps1` | Generic: Win32 mouse/keyboard/window control, UI Automation lookups, event log, ffmpeg recording, safety guards |
| `scripts\Steps.ps1` | Zero Install GUI steps plus generic click/wait building blocks |
| `scripts\Record.ps1` | Records a flow's takes |
| `scripts\Build.ps1` | Cut DSL, intermediate videos, GIF/MP4 encoding, review sheet |
| `flows\*.ps1` | Flow definitions (`$Takes`, `Get-Cuts`) |

`Record.ps1` parameters: `-Flow`, `-Take`, `-Recording`, `-DryRun` (Cancel instead of OK), `-Skip <steps>` (`RunApp`, `CloseApp`), `-AllowOverlap` (record despite other windows in the canvas; rehearsals only), and `-Search`/`-TileName`/`-ProcessName`/`-StartMenuQuery` (override `$App`).

`Build.ps1` parameters: `-Flow`, `-Recording`, `-Name` (output name), `-Review`.

## Steps and their events

Every take logs `start` and `end`. `ClickAt <x> <y> <label>` logs `<label>Move` when the cursor starts moving and `<label>` (with coordinates) at the click. `Build.ps1` draws a ring at every logged click.

| Step | What it does | Events |
|---|---|---|
| `Initialize-Stage [-Tab] [-AllowOverlap]` | Not recorded; runs before every take. Starts or finds the main window, moves it to `$MainWindow`, refuses to continue if other windows overlap the canvas (`Get-StageClutter`), selects the tab (`$Stage.Tab`), clears the Catalog search box, parks the cursor. | – |
| `Select-Tab <tab>` | Clicks the `My apps` / `Catalog` tab header | `clickTabMove`, `clickTab` |
| `Search-Catalog [text]` | Clicks the Catalog search box and types | `clickSearchMove`, `clickSearch`, `typed` |
| `Get-AppTile [name]` | Returns `.Run` (rectangle) and `.Star` (center) of a visible tile | – |
| `Invoke-RunApp [tile] [process]` | Clicks Run, waits through the download, moves the app window to `$AppWindow`. Returns the process. | `clickRunMove`, `clickRun`, `progressShown`*, `progressClosed`*, `appShown` |
| `Close-App` | Closes the app from `Invoke-RunApp` and refocuses the main window | `appClosed` |
| `Open-AppPopup [tile]` | Clicks the star; for new apps this adds them to "My apps" | `clickStarMove`, `clickStar`, `popupShown` |
| `Invoke-Integrate [-Cancel]` | Clicks Integrate / Modify integration in the popup, then OK (Cancel with `-Cancel` or `-DryRun`) in the dialog | `clickIntegrateMove`, `clickIntegrate`, `dialogShown`, `clickOKMove`, `clickOK`, `dialogClosed`, `integrationDone` |
| `Search-StartMenu [query]` | Win key, types the query, Escape. Record it in its own take and cut with `-Crop StartMenu`. | `startMenuTyped`, `resultsShown` |
| `Invoke-ClickElement <name> <label> [root]` | Clicks the first visible UIA element with that name (default root: main window) | `<label>Move`, `<label>` |
| `Wait-Window <title regex> <label> [-Center]` | Waits for a top-level window; `-Center` moves it to `$DialogCenter`. Logs after the move. Returns the window. | `<label>` |
| `Wait-WindowClosed <title regex> <label>` | Waits until the window is gone | `<label>` |

\* only when a download happens. For a cached app, `Invoke-RunApp` logs just the clicks and `appShown`.

`Log <name>` adds custom events. A flow can call any PowerShell, e.g., `Start-Sleep` for holds or `[Gui]::MoveTo` for cursor moves without clicking.

## What's known about the GUI

From Zero Install 2.29.3; check again after UI changes.

| Element | How it's found / handled |
|---|---|
| Main window | Top-level window titled `Zero Install`. It opens centered (`CenterScreen`) on whichever monitor, so `Initialize-Stage` moves it. Default size 540×551. |
| Tab headers | Not exposed via UIA. Clicked by position: `$Tabs` (relative to the main window). |
| Catalog search box | Class `EDIT` inside the pane named `Catalog`. Its UIA name is its placeholder `Search` only while empty. Cleared with `WM_SETTEXT`. A `Categories` combo box sits next to it. |
| App tile | Parent of the text element with the app's name. Children: `Run` split button (64×23; left part runs, right part ~56 px in opens a drop-down), star (22×22, unnamed), icon (48×48), name, summary. The drop-down's item texts are "Run", "Run with options" and "Update" (from `AppResources`; UIA names unverified). |
| Bottom bar | Buttons `Options`, `Tools` (▾), `Help` (▾) and the version text. Catalog tab: `Refresh list`, `More applications`. My apps tab: `Sync`, `Update all`. |
| Download progress window | Separate `0install-win` process; top-level window titled `Zero Install`. First a splash, then one progress bar per component. Centered on screen and re-centered after solving, so leave it where it is; the canvas has to contain it (~655×300 around screen center). |
| Launched app | The app's process main window. It opens wherever it remembers, so `Invoke-RunApp` moves it; 1–2 frames before the move are visible. |
| Star popup | `ToolStripDropDown` in the main window's UIA tree (not a top-level window). Text "Added to "My apps"." plus buttons `Integrate (start menu, etc.)` and `Remove`; for integrated apps `Modify integration` and `Remove`. It extends ~235 px right of the star. After a click it briefly shows a white or greyed "Working…" state; that's real behavior. |
| Integrate dialog | Separate `0install-win` process; titled `Integrate <app>`. Opens at `WindowsDefaultLocation`, so it gets centered (one stray frame). Checkboxes Start menu (checked), Desktop, File types, AutoPlay (app-dependent), the equivalent command line, and buttons `Advanced settings`, `OK`, `Cancel`. |
| After OK | A short "Applying desktop integration" progress window titled with the app name (it may download icons). |
| Capabilities-changed prompt | For integrated apps whose feed changed: a task dialog with Yes / No / Cancel. Only press Cancel. |

## Recording

- ffmpeg `gdigrab` captures `$Capture` (1320×900 at y=140) at 30 fps, losslessly (`libx264rgb -crf 0 -preset ultrafast`), to `recordings\<name>\<take>.mkv`.
- `-copyts` keeps gdigrab's wall-clock timestamps. Events are logged as Unix ms (`<take>-events.json`). `Build.ps1` subtracts the first frame's timestamp, so event times are seconds on the take's video.
- Cursor moves ease in and out. `MoveTo` takes about 1.5× its nominal duration because of the 15.6 ms Windows timer resolution; cuts are relative to `…Move` events, so this doesn't matter.
- Safety guards: `ClickAt` refuses anything outside `$Crops.Canvas`; `Type-Text` refuses unless the expected window is in the foreground. Without them, a failed lookup could click at (0,0) and send keystrokes to whatever has focus.
- Start menu:
  - A plain search lists personal files and folders, and the menu's initial view lists recent files. Use `apps:` queries and cut only the settled results.
  - Moving the mouse inside the menu makes stray "Find results in Apps" tooltips appear. `Initialize-Stage` parks the cursor outside the `StartMenu` crop.

## Building

1. For every (take, crop) used by a clip, `Build.ps1` writes `recordings\<name>\<take>.<crop>.mkv`: the crop, scaled to canvas size if it differs, with a 44 px orange ring (0.4 s) at every logged click inside the crop.
2. Each clip is `trim` + `setpts=(PTS-STARTPTS)/speed` (+ `tpad` clone for `-Freeze`). Clips are concatenated and resampled to 15 fps.
3. GIF: `palettegen=max_colors=256:stats_mode=full` + `paletteuse=dither=sierra2_4a:diff_mode=rectangle`. MP4: H.264 CRF 18.
4. `-Review`: `<name>-review.png`, a contact sheet at 4 frames per second.

The cuts of the reference flows (`T` = event time):

| Clip | From | To | Speed |
|---|---|---|---|
| Search | `clickSearchMove − 0.2` | `typed + 0.3` | 1× |
| Run | `clickRunMove − 0.05` | `progressShown` | 1× |
| Download | `progressShown` | `progressClosed` (fallback `appShown − 0.15`) | 12× / 16× |
| Gap | `progressClosed` | `appShown − 0.1` (if > 0.03 s) | 1× |
| App | `appShown + 0.04` | `appShown + 3.7` / `+ 1.5` | 1× |
| Star + popup | `clickStarMove − 0.1` | `clickStar + 0.63` | 1× |
| Integrate click | `clickIntegrateMove − 0.15` | `min(clickIntegrate + 0.54, dialogShown − 0.1)` | 1× |
| Dialog | `dialogShown + 0.04` | `dialogShown + 0.95` | 1× |
| OK | `clickOKMove − 0.08` | `dialogClosed − 0.015` | 1× |
| Integrated star | `integrationDone + 0.05` | `integrationDone + 0.75` | 1× |
| Start menu | `resultsShown − 1.4` | `resultsShown − 0.05`, freeze 1.3 s | 1×, crop `StartMenu` |

Checking frames around a cut (times in the output GIF):

```powershell
ffmpeg -ss 5.7 -t 0.5 -i artifacts\demo-gifs\run-app.gif -vf scale=182:-1 check%02d.png
magick montage check*.png -tile 10x -geometry +1+1 check.png
```

## Adapting

| Change | What to do |
|---|---|
| New Zero Install version | Usually nothing. On a timeout, compare `Show-Elements $script:Main` with the table above. Check window size (`$MainWindow`), tab positions (`$Tabs`), button names, dialog titles, progress window title. |
| Other app | `$App` in `Config.ps1` or the flow file, or `Record.ps1` overrides. Meet the cache, My apps and first-run prerequisites. Adjust `$AppWindow` for odd window sizes. |
| Pacing | The flow's `Get-Cuts` (speeds, holds; Build only, no re-recording); `Start-Sleep` in takes; `ClickAt`'s `moveMs`; `Type-Text`'s delay. |
| Resolution / scaling | Update every rectangle in `Config.ps1`. The canvas must contain the main window, the screen-centered progress window, the star popup and centered dialogs. `$Capture` must contain all crops. |
| Taskbar centered | Move `$Crops.StartMenu`; keep its aspect ratio equal to the canvas's. |
| New crop (e.g., a zoom) | Add it to `$Crops` and use `Clip -Crop <name>`. |

Showing the UI tree while writing a flow:

```powershell
. .\scripts\Config.ps1; . .\scripts\Helpers.ps1; . .\scripts\Steps.ps1
Initialize-Stage              # clicks the Catalog tab: ask the user first, it uses the mouse
Show-Elements $script:Main
Show-Elements (Get-TopWindow '^Integrate')
```

## Troubleshooting

| Symptom | Cause / fix |
|---|---|
| `These windows overlap the recording area …` | Other windows would show behind the Zero Install windows (e.g., the terminal or chat running the agent). Minimize them or move them to another monitor; for rehearsals, `-AllowOverlap`. |
| `Zero Install main window is not in the foreground` | Something else has focus, the session is locked, or the monitors are off. Check `[System.Windows.Forms.Screen]::AllScreens`: a lone 1280×720 display means the monitors are off. |
| `Timed out waiting for condition: …` | The condition names the missing element; compare with `Show-Elements`. The take's video and events are still saved. |
| `Refusing to click / type` | A control is at an unexpected position or focus was lost. Close stray popups (Escape) and retry; don't loosen the guard. |
| `Event 'x' not found` | The take ended early or the flow's cuts need an event the recording lacks (e.g., cutting `run-and-integrate` from a `run-app` recording). |
| Stray frame at a cut | Widen the offset in `Get-Cuts` and re-run Build. |
| Screenshot fails with "The handle is invalid" | The session is locked or the display is off. |
| Takes look different | Windows switched light/dark theme between takes; re-record the odd take with `-Take`. |
| GIF file larger than expected | A crop being scaled although it has canvas size; noisy content (video playback) also costs palette quality. |
