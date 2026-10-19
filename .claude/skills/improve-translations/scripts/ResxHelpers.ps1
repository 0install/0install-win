# Shared helpers for the improve-translations skill scripts. Dot-source this file.
# Compatible with Windows PowerShell 5.1 and PowerShell 7+.

$ErrorActionPreference = 'Stop'

$RepoRoot = (Resolve-Path "$PSScriptRoot\..\..\..\..").Path
$Utf8NoBom = New-Object System.Text.UTF8Encoding $false
$ManualComment = 'Manually overriden.'
$AutoComment = 'Automatically translated.'
$LanguagePattern = '[A-Za-z]{2,3}(-[A-Za-z]+)?'

function Get-RelativePath([string]$Path) {
    $full = [IO.Path]::GetFullPath($Path)
    if ($full.StartsWith($RepoRoot, [StringComparison]::OrdinalIgnoreCase)) {
        $full = $full.Substring($RepoRoot.Length).TrimStart('\', '/')
    }
    $full.Replace('\', '/')
}

function ConvertTo-XmlDocument([string]$Text) {
    $doc = New-Object System.Xml.XmlDocument
    $doc.LoadXml($Text.TrimStart([char]0xFEFF))
    $doc
}

function Get-NodeText($Node) {
    if ($Node) { $Node.InnerText } else { $null }
}

# Returns an ordered dictionary of key -> { Value, Comment } for all string entries.
function Get-ResxEntries([System.Xml.XmlDocument]$Doc) {
    $result = [ordered]@{}
    foreach ($node in $Doc.SelectNodes('/root/data')) {
        if ($node.GetAttribute('type') -or $node.GetAttribute('mimetype')) { continue } # Not a string
        $result[$node.GetAttribute('name')] = [pscustomobject]@{
            Value = [string](Get-NodeText $node.SelectSingleNode('value'))
            Comment = Get-NodeText $node.SelectSingleNode('comment')
        }
    }
    $result
}

function Read-ResxFile([string]$Path) {
    Get-ResxEntries (ConvertTo-XmlDocument ([IO.File]::ReadAllText($Path, [Text.Encoding]::UTF8)))
}

# Reads a .resx file as it was at a specific Git revision. Returns $null if it did not exist there.
function Read-ResxAtRevision([string]$Revision, [string]$Path) {
    $previousEncoding = [Console]::OutputEncoding
    [Console]::OutputEncoding = [Text.Encoding]::UTF8
    try {
        $lines = & git -C $RepoRoot show "${Revision}:$(Get-RelativePath $Path)" 2>$null
        if ($LASTEXITCODE -ne 0) { return $null }
    } finally {
        [Console]::OutputEncoding = $previousEncoding
    }
    Get-ResxEntries (ConvertTo-XmlDocument ($lines -join "`n"))
}

# Lists the localizable resources: English source .resx files under src/ that have localized siblings (<Base>.<lang>.resx).
# Name is "<Project>" for src/<Project>/Properties/Resources.resx and "<Project>/<Base>" for anything else
# IsForm marks WinForms designer resources.
# The optional filter matches either the full name or the project part (e.g. "Central.WinForms" includes all its forms).
function Get-ResxProjects([string[]]$Projects) {
    $srcRoot = Join-Path $RepoRoot 'src'
    Get-ChildItem $srcRoot -Recurse -Filter *.resx |
        Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' -and $_.Name -notmatch "\.$LanguagePattern(\.[oa])?\.resx$" } |
        Sort-Object FullName |
        ForEach-Object {
            $baseName = [IO.Path]::GetFileNameWithoutExtension($_.Name)
            $hasTranslations = Get-ChildItem $_.DirectoryName -Filter "$baseName.*.resx" |
                Where-Object { $_.Name -match "^$([regex]::Escape($baseName))\.$LanguagePattern\.resx$" } |
                Select-Object -First 1
            if (-not $hasTranslations) { return }

            $relative = $_.FullName.Substring($srcRoot.Length).TrimStart('\').Replace('\', '/')
            $projectName = $relative.Split('/')[0]
            $isProperties = $relative -ceq "$projectName/Properties/Resources.resx"
            $name = if ($isProperties) { $projectName } else { $relative.Substring(0, $relative.Length - '.resx'.Length) }
            if ($Projects -and -not ($Projects -contains $name -or $Projects -contains $projectName)) { return }

            [pscustomobject]@{
                Name = $name
                ProjectName = $projectName
                Directory = $_.DirectoryName
                BaseName = $baseName
                Source = $_.FullName
                IsForm = -not $isProperties -and (Test-Path (Join-Path $_.DirectoryName "$baseName.Designer.cs"))
            }
        }
}

# Lists the combined (committed) localized .resx files of a project, optionally filtered by language.
# Excludes the gitignored *.o.resx (Transifex) and *.a.resx (DeepL) intermediate files.
function Get-LocalizedResxFiles($Project, [string[]]$Languages) {
    $pattern = "^$([regex]::Escape($Project.BaseName))\.($LanguagePattern)\.resx$"
    Get-ChildItem $Project.Directory -Filter "$($Project.BaseName).*.resx" | ForEach-Object {
        if ($_.Name -match $pattern) {
            $language = $Matches[1]
            if (-not $Languages -or $Languages -contains $language) {
                [pscustomobject]@{ Language = $language; Path = $_.FullName }
            }
        }
    } | Sort-Object Language
}

# Returns the path of a project's combined localized file for a language.
function Get-LocalizedResxPath($Project, [string]$Language) {
    Join-Path $Project.Directory "$($Project.BaseName).$Language.resx"
}

# Returns the keys of the source that are meant to be translated.
# WinForms designer resources also contain layout properties and >>-prefixed metadata as plain strings; the translate workflow
# only keeps the keys present in the localized files, so do the same here.
function Get-LocalizableKeys($Project, $Source) {
    if (-not $Project.IsForm) { return @($Source.Keys) }
    $translated = @{}
    foreach ($file in Get-LocalizedResxFiles $Project) {
        foreach ($key in (Read-ResxFile $file.Path).Keys) { $translated[$key] = $true }
    }
    @($Source.Keys | Where-Object { $translated.ContainsKey($_) })
}

# Returns the entries of a localized file marked as manually overriden whose value or status differs from the Git revision.
function Get-ChangedManualEntries([string]$Path, [string]$Revision) {
    $current = Read-ResxFile $Path
    $base = Read-ResxAtRevision $Revision $Path
    foreach ($key in $current.Keys) {
        $entry = $current[$key]
        if ($entry.Comment -ne $ManualComment) { continue }
        $old = if ($base) { $base[$key] } else { $null }
        if (-not $old -or $old.Value -cne $entry.Value -or $old.Comment -ne $entry.Comment) {
            [pscustomobject]@{ Key = $key; Value = $entry.Value }
        }
    }
}

# Parses .tx/config into resources (keyed by source file path relative to the repository root) and local -> Transifex language code mappings.
function Get-TxConfig {
    $resources = @{}
    $langMap = @{}
    $section = $null
    foreach ($line in [IO.File]::ReadAllLines("$RepoRoot\.tx\config")) {
        if ($line -match '^\[(.+)\]$') {
            $section = [pscustomobject]@{ Id = $Matches[1]; SourceFile = $null; FileFilter = $null }
            if ($section.Id -ne 'main') { $resources[$section.Id] = $section }
        }
        elseif ($line -match '^\s*lang_map\s*=\s*(.+)$') {
            foreach ($pair in $Matches[1].Split(',')) {
                $remote, $local = $pair.Split(':') | ForEach-Object { $_.Trim() }
                $langMap[$local] = $remote
            }
        }
        elseif ($line -match '^\s*source_file\s*=\s*(.+)$' -and $section) { $section.SourceFile = $Matches[1].Trim().Replace('\', '/') }
        elseif ($line -match '^\s*file_filter\s*=\s*(.+)$' -and $section) { $section.FileFilter = $Matches[1].Trim().Replace('\', '/') }
    }
    $bySource = @{}
    foreach ($resource in $resources.Values) { if ($resource.SourceFile) { $bySource[$resource.SourceFile] = $resource } }
    [pscustomobject]@{ Resources = $bySource; LangMap = $langMap }
}

# Converts a .tx/config section name (o:<org>:p:<project>:r:<resource>) to the <project>.<resource> form the Transifex CLI
# accepts as a resource argument. The CLI rejects the full section name with exit code 1 and no message.
function ConvertTo-TxCliResourceId([string]$SectionName) {
    if ($SectionName -match '^o:[^:]+:p:([^:]+):r:([^:]+)$') { "$($Matches[1]).$($Matches[2])" } else { $SectionName }
}

# Extracts {0}-style format placeholders including alignment and format specifiers (e.g. "0:0.00"), ignoring escaped braces.
# Specifiers are kept since a mistranslated one (e.g. "0:0,00") silently changes the output.
function Get-Placeholders([string]$Text) {
    $unescaped = $Text.Replace('{{', '').Replace('}}', '')
    [regex]::Matches($unescaped, '\{(\d+(?:[,:][^}]*)?)\}') | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique -CaseSensitive
}

# Counts WinForms access key markers (&x), ignoring escaped ampersands (&&).
function Get-AccessKeyCount([string]$Text) {
    ([regex]::Matches($Text.Replace('&&', ''), '&\S')).Count
}
