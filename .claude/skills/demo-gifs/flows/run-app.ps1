# Reference flow "run-app" (~9 s): search the Catalog -> Run -> download (sped up) -> the app opens.
# Needs the app to be uncached, otherwise there is no download to show.
# Can also be cut from the "run-and-integrate" recording, which contains all events used here:
#   Build.ps1 -Flow run-app -Recording run-and-integrate

$Takes = [ordered]@{
    main = {
        Search-Catalog
        Invoke-RunApp | Out-Null
        Start-Sleep -Seconds 4
    }
}

# Clips shared with run-and-integrate. -Pause adds that many seconds of stillness before each step.
function Add-RunAppClips([double]$DownloadSpeed, [double]$AppHold, [double]$Pause = 0) {
    Clip main (T clickSearchMove (-0.2 - $Pause)) ([Math]::Min((T typed (0.3 + $Pause)), (T clickRunMove -0.05))) # click search box, type
    if (Test-Event progressShown) {
        Clip main (T clickRunMove -0.05) (T progressShown)               # move to Run, click, progress window appears
        $downloadEnd = T progressClosed -Fallback (T appShown -0.15)
        Clip main (T progressShown) $downloadEnd -Speed $DownloadSpeed   # download + extract, sped up
        # Main window alone until the app appears (at most 0.5 s: the app's first start can take long, and nothing moves meanwhile);
        # stop before the app window shows at its own position
        $gapEnd = [Math]::Min($downloadEnd + 0.5, (T appShown -0.1))
        if ($gapEnd - $downloadEnd -gt 0.03) { Clip main $downloadEnd $gapEnd }
    }
    else {
        Clip main (T clickRunMove -0.05) (T appShown -0.1)               # app was cached: no progress window
    }
    Clip main (T appShown 0.04) (T appShown $AppHold)                    # app running (after it was moved on stage)
}

function Get-Cuts {
    Add-RunAppClips -DownloadSpeed 12 -AppHold 3.7
}
