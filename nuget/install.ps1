param($installPath, $toolsPath, $package, $project)

# Recursive helper function
function applyProperties($items)
{
  foreach ($item in $items)
  {
    # Item type "Physical File" -> set property
    if ($item.Kind -eq "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}") {$item.Properties.Item("CopyToOutputDirectory").Value = 1}
    # Item type "Physical Folder" -> recurse
    elseif ($item.Kind -eq "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}") {applyProperties $item.ProjectItems}
  }
}

# Apply properties to all content files added to the Visual Studio project by the NuGet package
applyProperties($project.ProjectItems.Item("7zxa.dll"))
applyProperties($project.ProjectItems.Item("7zxa-x64.dll"))
