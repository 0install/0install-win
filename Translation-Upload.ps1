Param ([Parameter(Mandatory=$True)][String]$User, [Parameter(Mandatory=$True)][String]$Password)
$ErrorActionPreference = "Stop"

function put ($relativeUri, $filePath) {
    curl.exe -k -L --user "${User}:${Password}" -i -X PUT -F "file=@$filePath" "https://www.transifex.com/api/2/project/0install-win/$relativeUri"
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

upload store-service "$PSScriptRoot\src\Store.Service\Properties\Resources"
upload central "$PSScriptRoot\src\Central.WinForms\Properties\Resources"

upload_filtered window-central_winforms_introdialog "$PSScriptRoot\src\Central.WinForms\IntroDialog"
upload_filtered window-central_winforms_mainform "$PSScriptRoot\src\Central.WinForms\MainForm"
upload_filtered window-central_winforms_portablecreatordialog "$PSScriptRoot\src\Central.WinForms\PortableCreatorDialog"
upload_filtered window-central_winforms_selectcommanddialog "$PSScriptRoot\src\Central.WinForms\SelectCommandDialog"
upload_filtered window-central_winforms_syncwizard "$PSScriptRoot\src\Central.WinForms\SyncWizard"
upload_filtered window-commands_winforms_configdialog "$PSScriptRoot\src\Commands.WinForms\ConfigDialog"
upload_filtered window-commands_winforms_feedsearchdialog "$PSScriptRoot\src\Commands.WinForms\FeedSearchDialog"
upload_filtered window-commands_winforms_integrateappform "$PSScriptRoot\src\Commands.WinForms\IntegrateAppForm"
upload_filtered window-commands_winforms_interfacedialog "$PSScriptRoot\src\Commands.WinForms\InterfaceDialog"
upload_filtered window-commands_winforms_storemanageform "$PSScriptRoot\src\Commands.WinForms\StoreManageForm"
