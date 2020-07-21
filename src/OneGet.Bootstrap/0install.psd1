@{
    ModuleVersion = '1.0.0'
    GUID = '89e16ee9-f9dd-4efd-b7a2-e0374958f6c0'
    Author = 'Bastian Eicher'
    CompanyName = '0install.net'
    Copyright = 'Copyright Bastian Eicher et al.'
    Description = 'Zero Install is a decentralized cross-platform software-installation system.'
    PowerShellVersion = '3.0'
    FunctionsToExport = @()
    #RequiredModules = @('PackageManagement')
    FileList = @('0install.dll')
    PrivateData = @{
        PackageManagementProviders = '0install.dll'
        PSData = @{
            Tags = @("PackageManagement","Provider")
            ProjectUri = 'https://0install.net'
        }
    }
}
