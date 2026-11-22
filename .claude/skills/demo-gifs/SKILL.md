---
name: demo-gifs
description: Record animated GIF demos of the Zero Install for Windows GUI ("Central") by scripting the real mouse and keyboard, screen-recording with ffmpeg and cutting automatically from an event log. Use when asked to create, re-record or update demo GIFs or screencasts of this GUI, e.g., for a new release, with another app, or showing a new UI flow.
---

# Demo GIFs of the Zero Install GUI

A **flow** describes one demo:
- **Takes:** scripted GUI actions. Each take is recorded as one video.
- **Cuts:** a `Get-Cuts` function that selects clips from the takes, relative to events logged while recording.

`scripts\Record.ps1 -Flow <name>` records the takes. `scripts\Build.ps1 -Flow <name>` cuts them into a GIF. Because cuts refer to events, not timestamps, re-recording (new Zero Install version, other app, other machine) needs no manual re-timing.

Output goes to `artifacts\demo-gifs\` (gitignored): `<flow>.gif`, `<flow>.mp4`, `<flow>-review.png`, and `recordings\<name>\` with the raw takes.

## Ground rules: this drives the user's real desktop

- **Ask before every recording.** Say which flow and take, roughly how long it takes, and that the user must not touch the mouse or keyboard meanwhile. Wait for a go-ahead.
- **Leave admin steps to the user.** Removing an app from a shared or machine-wide cache needs elevation. Give the commands (below) and ask the user to run them; never try to elevate yourself.
- **Report side effects.** Recording a flow can add apps to "My apps" and integrate them. Tell the user what changed. Undo rehearsal side effects (`0install remove <feed>`) unless the user wants to keep them.
- **Don't rehearse Integrate with an already integrated app.** If its capabilities changed, Zero Install asks whether to update the integration. Answer only "Cancel".
- **Check for personal data** before delivering: Start menu results, other windows, the desktop around the canvas.
- **Never weaken the safety guards.** If a script stops with "Refusing to click/type", find out why: usually focus was lost or a control moved.

## Prerequisites (check before recording)

- Primary monitor 1920×1080 at 100% scaling. Taskbar left-aligned. Same light/dark theme for all takes of a GIF.
- **No other windows in the canvas area** (screen x 652–1252, y 254–824), including the window this conversation runs in: the desktop around the Zero Install windows is recorded. `Record.ps1` refuses to start a take otherwise and names the overlapping windows. Ask the user to minimize them or move them to another monitor.
- The screen must be on and the session unlocked. If `[System.Windows.Forms.Screen]::AllScreens` shows a 1280×720 display, the monitors are off and nothing can be recorded.
- `ffmpeg` (with `gdigrab`) and `magick` on the `PATH`. If missing: `0install alias ffmpeg https://apps.0install.net/utils/ffmpeg.xml` and `0install alias magick https://apps.0install.net/utils/imagemagick.xml`.
- Zero Install at `$CentralExe` (`scripts\Config.ps1`).
- **For flows that show a download:** the app is not cached. To clear it, ask the user to run, elevated if the cache is shared:
  ```powershell
  0install store list-implementations <feed>
  0install store remove <digest>
  ```
- **For flows that add or integrate:** the app is not in "My apps" (`0install remove <feed>`).
- The app shows no first-run dialogs (VLC: `%APPDATA%\vlc\vlcrc` exists).
- **For flows that end on the desktop icon:** desktop icons are shown (View → Show desktop icons), the top-left corner (`$Crops.Desktop`) has no other icons, so the new icon lands there, and no window covers that corner.

## Workflow

1. **Pick or write a flow** (see below).
2. **Check prerequisites.** Ask the user to clear the cache if needed.
3. **Rehearse** without downloading, using cached apps and a separate recording name. `-AllowOverlap` tolerates other windows in the background:
   ```powershell
   .\scripts\Record.ps1 -Flow run-app -Recording test-run -Search winmerge -TileName WinMerge -ProcessName WinMergeU -AllowOverlap
   .\scripts\Record.ps1 -Flow run-and-integrate -Take main -Recording test-integrate -Search gimp -TileName GIMP -Skip RunApp, CloseApp -DryRun -AllowOverlap
   0install remove https://apps.0install.net/gui/gimp.xml   # the star click added it
   Get-Process WinMergeU | ForEach-Object { $_.CloseMainWindow() }   # run-app leaves the app running
   .\scripts\Build.ps1 -Flow run-app -Recording test-run -Name test-run -Review
   ```
   `run-and-integrate`'s `desktop` take needs the icon a real (not `-DryRun`) `main` take creates, so it can't be rehearsed this way. If it fails, fix it and redo just that take with `-Take desktop`.
4. **Record:** `.\scripts\Record.ps1 -Flow <flow>`. Use `-Take <name>` to redo a single take; the others are kept. A download can be recorded only once per cache clearing.
5. **Build:** `.\scripts\Build.ps1 -Flow <flow> -Review`. Look at `<flow>-review.png`, and extract single frames around each cut to check for stray frames (see reference.md). Re-run Build as often as needed; it never touches the screen.
6. **Deliver** the GIF from `artifacts\demo-gifs\`.

Paths are relative to this skill's directory (`.claude\skills\demo-gifs`); the scripts can be run from any working directory.

## Reference flows

Both reference flows were recorded on 2026-11-22 with Zero Install 2.30.0 and VLC 3.0.24. The download took 26 s in real time, and VLC's first start after it another 27 s (cut to 0.5 s). Output is 600×570 at 15 fps: GIFs stay at most 600 px wide, by cropping and window layout rather than scaling (see `Config.ps1`).

| Flow | Shows | Takes | Result |
|---|---|---|---|
| `run-app` | Catalog search "vlc" → **Run** → download at 12× → VLC opens and stays 3.7 s | `main` | 9.9 s, 199 KB |
| `run-and-integrate` | Like `run-app` (download at 16×, VLC 1.9 s, 0.4 s pause before each step), then close VLC with the X in its title bar → ☆ → **Integrate (start menu, etc.)** → tick **Desktop** → **OK** → "integrated" star → cut to the new desktop icon (1.2× zoom), cursor hovers it (frozen 1.5 s) | `main`, `desktop` | 23.7 s, 338 KB |

`run-and-integrate`'s recording contains every event `run-app` needs. Record it once and cut both GIFs from the same download:

```powershell
.\scripts\Record.ps1 -Flow run-and-integrate
.\scripts\Build.ps1 -Flow run-and-integrate -Review
.\scripts\Build.ps1 -Flow run-app -Recording run-and-integrate -Review
```

## Writing a new flow

Create `flows\<name>.ps1`. It is dot-sourced by both scripts and defines:
- **`$Takes`:** an ordered hashtable of take name → script block calling steps.
- **`Get-Cuts`:** a function calling `Clip` for each piece of the GIF, in order.
- Optionally **`$Stage = @{ Tab = 'My apps' }`** (default: Catalog) and overrides of `$App` or layout variables.

```powershell
# flows/run-with-options.ps1 (sketch; check names with Show-Elements before relying on them)
$Takes = [ordered]@{
    main = {
        Search-Catalog
        $tile = Get-AppTile
        ClickAt ([int]($tile.Run.X + 56)) ([int]($tile.Run.Y + 11)) 'clickRunArrow'  # drop-down arrow of the Run button
        Invoke-ClickElement 'Run with options' 'clickRunWithOptions'
        Wait-Window '^Run' 'optionsShown' -Center
        Start-Sleep -Seconds 3
    }
}
function Get-Cuts {
    Clip main (T clickSearchMove -0.2) (T typed 0.3)
    Clip main (T clickRunArrowMove -0.1) (T optionsShown -0.1)
    Clip main (T optionsShown 0.04) (T optionsShown 2.5)
}
```

**Steps** (`scripts\Steps.ps1`; each logs events):
- Zero Install–specific: `Search-Catalog`, `Get-AppTile`, `Invoke-RunApp`, `Close-App`, `Open-AppPopup`, `Invoke-Integrate`, `Select-Tab`, `Search-StartMenu`, `Show-DesktopIcon`.
- Generic: `Invoke-ClickElement`, `Wait-Window [-Center]`, `Wait-WindowClosed`, `ClickAt`, `Type-Text`, `Log`.

**Cut DSL** (`scripts\Build.ps1`):
- `T <event> [offset] [-Fallback t]`: event time in seconds. Use `take:event` if a name occurs in several takes.
- `Test-Event <event>`
- `Clip <take> <start> <end> [-Speed n] [-Crop Canvas|StartMenu|Desktop] [-Freeze s]`
- In PowerShell, wrap computed arguments in parentheses: `(T x 0.3)`.

Rules of thumb for cuts:
- Start a clip about 0.1–0.2 s before a `…Move` event, so the cursor starts moving on screen.
- Windows that the script moves on stage (`appShown`, `Wait-Window -Center`) are briefly visible at their own position *before* the event. Cut to `event − 0.1` and resume at `event + 0.04`.
- Pauses where nothing moves (UI Automation lookups take about 1 s) disappear when you cut around them.
- Discover control names with `Show-Elements $script:Main` (see reference.md).

**[reference.md](reference.md)** has the full step and event list, what is known about the GUI's controls, how recording and cutting work, layout constraints for other resolutions, and troubleshooting.
