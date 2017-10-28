Param ([Parameter(Mandatory=$True)][String]$User, [Parameter(Mandatory=$True)][String]$Password)
$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

function get ($relativeUri, $filePath) {
    0install run http://repo.roscidus.com/utils/curl -k -L --user "${User}:${Password}" -o $filePath https://www.transifex.com/api/2/project/0install-win/$relativeUri
}

function download($slug, $pathBase) {
    get "resource/$slug/translation/el/?file" "$pathBase.el.resx"
    get "resource/$slug/translation/tr/?file" "$pathBase.tr.resx"
}

download commands "$ScriptDir\src\Commands\Properties\Resources"
download store-service "$ScriptDir\src\Store.Service\Properties\Resources"
download central "$ScriptDir\src\Central\Properties\Resources"

download window-central_winforms_introdialog "$ScriptDir\src\Central.WinForms\IntroDialog"
download window-central_winforms_mainform "$ScriptDir\src\Central.WinForms\MainForm"
download window-central_winforms_portablecreatordialog "$ScriptDir\src\Central.WinForms\PortableCreatorDialog"
download window-central_winforms_selectcommanddialog "$ScriptDir\src\Central.WinForms\SelectCommandDialog"
download window-central_winforms_syncwizard "$ScriptDir\src\Central.WinForms\SyncWizard"
download window-commands_winforms_configdialog "$ScriptDir\src\Commands.WinForms\ConfigDialog"
download window-commands_winforms_feedsearchdialog "$ScriptDir\src\Commands.WinForms\FeedSearchDialog"
download window-commands_winforms_integrateappform "$ScriptDir\src\Commands.WinForms\IntegrateAppForm"
download window-commands_winforms_interfacedialog "$ScriptDir\src\Commands.WinForms\InterfaceDialog"
download window-commands_winforms_storemanageform "$ScriptDir\src\Commands.WinForms\StoreManageForm"
