Param ([Parameter(Mandatory=$True)][String]$User, [Parameter(Mandatory=$True)][String]$Password)
$ErrorActionPreference = "Stop"

function get ($relativeUri, $filePath) {
    curl.exe -k -L --user "${User}:${Password}" -o $filePath https://www.transifex.com/api/2/project/0install-win/$relativeUri
}

function download($slug, $pathBase) {
    get "resource/$slug/translation/el/?file" "$pathBase.el.resx"
    get "resource/$slug/translation/tr/?file" "$pathBase.tr.resx"
}

download store-service "$PSScriptRoot\src\Store.Service\Properties\Resources"
download central "$PSScriptRoot\src\Central.WinForms\Properties\Resources"

download window-central_winforms_introdialog "$PSScriptRoot\src\Central.WinForms\IntroDialog"
download window-central_winforms_mainform "$PSScriptRoot\src\Central.WinForms\MainForm"
download window-central_winforms_portablecreatordialog "$PSScriptRoot\src\Central.WinForms\PortableCreatorDialog"
download window-central_winforms_selectcommanddialog "$PSScriptRoot\src\Central.WinForms\SelectCommandDialog"
download window-central_winforms_syncwizard "$PSScriptRoot\src\Central.WinForms\SyncWizard"
download window-commands_winforms_configdialog "$PSScriptRoot\src\Commands.WinForms\ConfigDialog"
download window-commands_winforms_feedsearchdialog "$PSScriptRoot\src\Commands.WinForms\FeedSearchDialog"
download window-commands_winforms_integrateappform "$PSScriptRoot\src\Commands.WinForms\IntegrateAppForm"
download window-commands_winforms_interfacedialog "$PSScriptRoot\src\Commands.WinForms\InterfaceDialog"
download window-commands_winforms_storemanageform "$PSScriptRoot\src\Commands.WinForms\StoreManageForm"
