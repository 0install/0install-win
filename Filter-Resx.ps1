# Generates copies of WinForms .resx files filtered down to only localizable strings (using German translations as the reference)
$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Windows.Forms

function Process($pathBase) {
    $filteredKeys = New-Object Collections.Generic.List[string]
    $reader = New-Object Resources.ResXResourceReader -ArgumentList "$PSScriptRoot\src\$pathBase.de.resx"
    $dict = $reader.GetEnumerator();
    while ($dict.MoveNext()) {
        $filteredKeys.Add($dict.Key.ToString());
    }
    $reader.Dispose()

    $reader = New-Object Resources.ResXResourceReader -ArgumentList "$PSScriptRoot\src\$pathBase.resx"
    $writer = New-Object Resources.ResXResourceWriter -ArgumentList "$PSScriptRoot\src\$pathBase.filtered.resx"
    $dict = $reader.GetEnumerator()
    while ($dict.MoveNext()) {
        $key = $dict.Key.ToString();
        if ($filteredKeys.Contains($key)) {
            $writer.AddResource($key, $dict.Value)
        }
    }
    $writer.Dispose()
    $reader.Dispose()
}

Process "Central.WinForms\IntroDialog"
Process "Central.WinForms\MainForm"
Process "Central.WinForms\PortableCreatorDialog"
Process "Central.WinForms\SelectCommandDialog"
Process "Central.WinForms\SyncWizard"
Process "Commands.WinForms\ConfigDialog"
Process "Commands.WinForms\FeedSearchDialog"
Process "Commands.WinForms\IntegrateAppForm"
Process "Commands.WinForms\InterfaceDialog"
Process "Commands.WinForms\StoreManageForm"
