Run("ZeroInstall")

;Search for VLC in catalog
WinWaitActive("Zero Install")
Send("{RIGHT}{TAB}VLC")

;Integrate VLC
Send("{TAB}{TAB}{ENTER}{ENTER}")
Opt("WinTitleMatchMode", 2)
WinWaitActive("VLC media player")
Send("{ENTER}")

;Ensure menu entry was created
If Not FileExists(EnvGet("appdata") & "\Microsoft\Windows\Start Menu\Programs\AudioVideo\VLC media player.lnk") Then
    Exit(1)
EndIf

;Remove VLC
WinWaitActive("Zero Install")
Send("{ENTER}{TAB}{ENTER}")

WinClose("Zero Install")
