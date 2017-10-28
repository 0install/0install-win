Param ([Parameter(Mandatory=$True)][String]$User, [Parameter(Mandatory=$True)][String]$Password)
$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

function put ($relativeUri, $filePath) {
    0install run http://repo.roscidus.com/utils/curl -k -L --user "${User}:${Password}" -i -X PUT -F "file=@$filePath" "https://www.transifex.com/api/2/project/0install-win/$relativeUri"
}

function upload($slug, $pathBase) {
    put "resource/$slug/content/" "$pathBase.resx"
    put "resource/$slug/translation/de/" "$pathBase.de.resx"
}

Add-Type -AssemblyName System.Windows.Forms
function upload_filtered($slug, $pathBase) {
    $filteredKeys = New-Object Collections.Generic.List[string]
    $reader = New-Object Resources.ResXResourceReader -ArgumentList "$pathBase.de.resx"
    $dict = $reader.GetEnumerator();
    while ($dict.MoveNext()) {
        $filteredKeys.Add($dict.Key.ToString());
    }
    $reader.Dispose()
    
    $reader = New-Object Resources.ResXResourceReader -ArgumentList "$pathBase.resx"
    $writer = New-Object Resources.ResXResourceWriter -ArgumentList "$pathBase.filtered.resx"
    $dict = $reader.GetEnumerator()
    while ($dict.MoveNext()) {
        $key = $dict.Key.ToString();
        if ($filteredKeys.Contains($key)) {
            $writer.AddResource($key, $dict.Value)
        }
    }
    $writer.Dispose()
    $reader.Dispose()
    
    put "resource/$slug/content/" "$pathBase.filtered.resx"
    put "resource/$slug/translation/de/" "$pathBase.de.resx"
}

upload commands "$ScriptDir\src\Commands\Properties\Resources"
upload store-service "$ScriptDir\src\Store.Service\Properties\Resources"
upload central "$ScriptDir\src\Central\Properties\Resources"

upload_filtered window-central_winforms_introdialog "$ScriptDir\src\Central.WinForms\IntroDialog"
upload_filtered window-central_winforms_mainform "$ScriptDir\src\Central.WinForms\MainForm"
upload_filtered window-central_winforms_portablecreatordialog "$ScriptDir\src\Central.WinForms\PortableCreatorDialog"
upload_filtered window-central_winforms_selectcommanddialog "$ScriptDir\src\Central.WinForms\SelectCommandDialog"
upload_filtered window-central_winforms_syncwizard "$ScriptDir\src\Central.WinForms\SyncWizard"
upload_filtered window-commands_winforms_configdialog "$ScriptDir\src\Commands.WinForms\ConfigDialog"
upload_filtered window-commands_winforms_feedsearchdialog "$ScriptDir\src\Commands.WinForms\FeedSearchDialog"
upload_filtered window-commands_winforms_integrateappform "$ScriptDir\src\Commands.WinForms\IntegrateAppForm"
upload_filtered window-commands_winforms_interfacedialog "$ScriptDir\src\Commands.WinForms\InterfaceDialog"
upload_filtered window-commands_winforms_storemanageform "$ScriptDir\src\Commands.WinForms\StoreManageForm"
