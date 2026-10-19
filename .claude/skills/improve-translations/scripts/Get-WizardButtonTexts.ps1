<#
.SYNOPSIS
Prints the Next/Back/Cancel/Finish button texts that the AeroWizard library shows in SyncWizard for each language.

.DESCRIPTION
These buttons are localized by AeroWizard's own satellite assemblies, not by this repository's .resx files, so strings that
refer to them (e.g. "click 'Next'") must match these texts. A language falls back to English if AeroWizard has no
translation for it or if the build excludes it via SatelliteResourceLanguages in src/Directory.Build.props.

.PARAMETER Languages
Language codes as used in the file names (e.g. fr, pt-BR). Defaults to all localized languages.
#>
param([string[]]$Languages)
. "$PSScriptRoot\ResxHelpers.ps1"

$csproj = [IO.File]::ReadAllText("$RepoRoot\src\Central.WinForms\Central.WinForms.csproj")
if ($csproj -notmatch '<PackageReference Include="AeroWizard" Version="([^"]+)"') { throw 'No AeroWizard PackageReference found' }
$version = $Matches[1]

$props = [IO.File]::ReadAllText("$RepoRoot\src\Directory.Build.props")
$shipped = if ($props -match '<SatelliteResourceLanguages>([^<]*)</SatelliteResourceLanguages>') { $Matches[1].Split(';') } else { $null }

$packages = (& dotnet nuget locals global-packages --list) -replace '^global-packages:\s*', ''
$libDir = Get-ChildItem (Join-Path $packages.Trim() "aerowizard\$version\lib") -Directory | Sort-Object Name | Select-Object -First 1
if (-not $libDir) { throw "AeroWizard $version not found in the NuGet package cache; run dotnet restore first" }

if (-not $Languages) {
    $Languages = Get-ResxProjects | ForEach-Object { Get-LocalizedResxFiles $_ } | ForEach-Object Language | Sort-Object -Unique
}

foreach ($language in $Languages) {
    # .NET resource fallback: the specific culture first, then its neutral parent
    $culture = @($language, $language.Split('-')[0]) | Select-Object -Unique |
        Where-Object { Test-Path (Join-Path $libDir.FullName "$_\AeroWizard.resources.dll") } | Select-Object -First 1
    if (-not $culture) { "${language}: English (AeroWizard has no translation)"; continue }
    if ($shipped -and $shipped -notcontains $culture) { "${language}: English (AeroWizard has '$culture', but SatelliteResourceLanguages excludes it from the build)"; continue }

    $assembly = [Reflection.Assembly]::LoadFile((Join-Path $libDir.FullName "$culture\AeroWizard.resources.dll"))
    $texts = @{}
    foreach ($name in $assembly.GetManifestResourceNames()) {
        $reader = New-Object Resources.ResourceReader($assembly.GetManifestResourceStream($name))
        try { foreach ($entry in $reader) { $texts[$entry.Key] = $entry.Value } } finally { $reader.Close() }
    }
    "${language}: Next = $($texts.WizardNextText), Back = $($texts.WizardBackText), Cancel = $($texts.WizardCancelText), Finish = $($texts.WizardFinishText)"
}
