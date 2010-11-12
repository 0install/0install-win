@echo off
::Removes compilation artifacts and other temporary files.

rem Clear binaries (leave Documentation and Bundled intact)
rd /s /q build\Backend > NUL 2>&1
rd /s /q build\Frontend > NUL 2>&1
rd /s /q build\Tools > NUL 2>&1
rd /s /q build\Setup > NUL 2>&1

rem Clear ReSharper's cache
rd /s /q vs\_ReSharper.ZeroInstall_VS2010 > NUL 2>&1
copy "vs\ZeroInstall_VS2010.5.1.ReSharper.user.Backup" "vs\ZeroInstall_VS2010.5.1.ReSharper.user" > NUL

rem Clear object caches
del vs\*.cache > NUL 2>&1
rd /s /q vs\Central.Cli\obj > NUL 2>&1
rd /s /q vs\Central.WinForms\obj > NUL 2>&1
rd /s /q vs\Central.Wpf\obj > NUL 2>&1
rd /s /q vs\Central.Gtk\obj > NUL 2>&1
rd /s /q vs\Injector.Cli\obj > NUL 2>&1
rd /s /q vs\Injector.WinForms\obj > NUL 2>&1
rd /s /q vs\Store.Service\obj > NUL 2>&1
rd /s /q vs\Store.Management.Cli\obj > NUL 2>&1
rd /s /q vs\Store.Management.WinForms\obj > NUL 2>&1
rd /s /q vs\Common\obj > NUL 2>&1
rd /s /q vs\Common.Wpf\obj > NUL 2>&1
rd /s /q vs\Common.Gtk\obj > NUL 2>&1
rd /s /q vs\DownloadBroker\obj > NUL 2>&1
rd /s /q vs\Injector\obj > NUL 2>&1
rd /s /q vs\Model\obj > NUL 2>&1
rd /s /q vs\MyApps\obj > NUL 2>&1
rd /s /q vs\Store\obj > NUL 2>&1
rd /s /q vs\Test.Common\obj > NUL 2>&1
rd /s /q vs\Test.Backend\obj > NUL 2>&1
rd /s /q vs\Test.Frontend\obj > NUL 2>&1
rd /s /q vs\Test.Tools\obj > NUL 2>&1
rd /s /q vs\Publish\obj > NUL 2>&1
rd /s /q vs\Publish.Cli\obj > NUL 2>&1
rd /s /q vs\Publish.WinForms\obj > NUL 2>&1
rd /s /q vs\Modeling\obj > NUL 2>&1

rem Restore VS2010 solution user options (temporarily unhide for copying)
attrib -h "vs\ZeroInstall_VS2010.suo.Backup" > NUL 2>&1
attrib -h "vs\ZeroInstall_VS2010.suo" > NUL 2>&1
copy "vs\ZeroInstall_VS2010.suo.Backup" "vs\ZeroInstall_VS2010.suo" > NUL 2>&1
attrib +h "vs\ZeroInstall_VS2010.suo.Backup" > NUL 2>&1
attrib +h "vs\ZeroInstall_VS2010.suo" > NUL 2>&1

rem Restore VCE2010 solution user options (temporarily unhide for copying)
attrib -h "vs\ZeroInstall_VCE2010.suo.Backup" > NUL 2>&1
attrib -h "vs\ZeroInstall_VCE2010.suo" > NUL 2>&1
copy "vs\ZeroInstall_VCE2010.suo.Backup" "vs\ZeroInstall_VCE2010.suo" > NUL 2>&1
attrib +h "vs\ZeroInstall_VCE2010.suo.Backup" > NUL 2>&1
attrib +h "vs\ZeroInstall_VCE2010.suo" > NUL 2>&1

rem Remove NUnit logs
del *.VisualState.xml > NUL 2>&1
del TestResult.xml > NUL 2>&1
