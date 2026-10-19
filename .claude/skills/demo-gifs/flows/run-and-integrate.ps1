# Reference flow "run-and-integrate" (~15 s): like run-app, then close the app -> star (add to "My apps")
# -> "Integrate (start menu, etc.)" -> OK -> cut to the Start menu showing the app.
# Needs the app to be uncached and not in "My apps". Leaves the app integrated with the dialog's defaults.

. "$PSScriptRoot\run-app.ps1"

$Takes = [ordered]@{
    main = {
        Search-Catalog
        Invoke-RunApp | Out-Null
        Start-Sleep -Seconds 4
        Close-App
        Open-AppPopup
        Invoke-Integrate
        Start-Sleep -Seconds 2
    }
    startmenu = {
        Search-StartMenu
    }
}

function Get-Cuts {
    Add-RunAppClips -DownloadSpeed 16 -AppHold 1.5
    Clip main (T clickStarMove -0.1) (T clickStar 0.63)                                               # star -> "Added to My apps" popup
    Clip main (T clickIntegrateMove -0.15) ([Math]::Min((T clickIntegrate 0.54), (T dialogShown -0.1))) # "Integrate (start menu, etc.)", stop before the dialog shows at its default position
    Clip main (T dialogShown 0.04) (T dialogShown 0.95)                                               # Integrate dialog (after it was centered)
    Clip main (T clickOKMove -0.08) (T dialogClosed -0.015)                                           # click OK
    Clip main (T integrationDone 0.05) (T integrationDone 0.75)                                       # star now shows "integrated"
    # Settled Start menu results only (its initial view shows recent files), then freeze the last clean frame
    Clip startmenu (T resultsShown -1.4) (T resultsShown -0.05) -Crop StartMenu -Freeze 1.3
}
