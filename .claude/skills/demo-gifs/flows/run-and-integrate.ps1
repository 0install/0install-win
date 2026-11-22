# Reference flow "run-and-integrate" (~23 s): like run-app, then close the app with the X in its title bar -> star (add to "My apps")
# -> "Integrate (start menu, etc.)" -> tick "Desktop" -> OK -> cut to the new desktop icon.
# Needs the app to be uncached and not in "My apps", and an empty desktop (the icon lands in the top-left slot, see $Crops.Desktop).
# Leaves the app integrated with the dialog's defaults plus a desktop icon.

. "$PSScriptRoot\run-app.ps1"

$Takes = [ordered]@{
    main = {
        Search-Catalog
        Invoke-RunApp | Out-Null
        Start-Sleep -Seconds 4
        Close-App
        Open-AppPopup
        Invoke-Integrate -Check Desktop
        Start-Sleep -Seconds 2
    }
    desktop = {
        Show-DesktopIcon
    }
}

# Seconds of stillness added before each step (taken from the recording, never past the next step's cursor move)
$Pause = 0.4

function Get-Cuts {
    Add-RunAppClips -DownloadSpeed 16 -AppHold (1.5 + $Pause) -Pause $Pause
    Clip main (T clickCloseMove -0.1) ([Math]::Min((T appClosed (0.3 + $Pause)), (T clickStarMove -0.1)))         # click the app's X
    Clip main (T clickStarMove -0.1) ([Math]::Min((T clickStar (0.63 + $Pause)), (T clickIntegrateMove -0.15)))   # star -> "Added to My apps" popup
    Clip main (T clickIntegrateMove -0.15) ([Math]::Min((T clickIntegrate 0.54), (T dialogShown -0.1)))            # "Integrate (start menu, etc.)", stop before the dialog shows at its default position
    Clip main (T dialogShown 0.04) ([Math]::Min((T dialogShown (0.8 + $Pause)), (T checkDesktopMove -0.1)))       # Integrate dialog (after it was centered)
    Clip main (T checkDesktopMove -0.1) ([Math]::Min((T checkDesktop (0.45 + $Pause)), (T clickOKMove -0.08)))    # tick "Desktop"
    Clip main (T clickOKMove -0.08) (T dialogClosed -0.015)                                                       # click OK
    Clip main (T integrationDone 0.05) (T integrationDone (0.75 + $Pause))                                        # star now shows "integrated"
    # The new desktop icon, then the cursor hovers over it; freeze before a tooltip can appear
    Clip desktop (T desktopShown (0.2 - $Pause)) (T hoverIcon 0.3) -Crop Desktop -Freeze 1.5
}
