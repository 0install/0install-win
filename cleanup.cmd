@echo off
::Removes compilation artifacts and other temporary files.

rem Clear binaries (but leave Documentation intact since it takes very long to build)
rd /s /q build\Backend > NUL 2>&1
rd /s /q build\Frontend > NUL 2>&1
rd /s /q build\Tools > NUL 2>&1
rd /s /q build\Updater > NUL 2>&1
rd /s /q build\Setup > NUL 2>&1
rd /s /q build\Documentation\working > NUL 2>&1
del build\Documentation\*.log > NUL 2>&1

rem Clear ReSharper's cache
rd /s /q src\_ReSharper.ZeroInstall_VS2010 > NUL 2>&1

rem Clear object caches
attrib -h src\*.suo > NUL 2>&1
del src\*.suo > NUL 2>&1
del src\*.cache > NUL 2>&1
rd /s /q src\Alias.Cli\obj > NUL 2>&1
rd /s /q src\Central\obj > NUL 2>&1
rd /s /q src\Central.WinForms\obj > NUL 2>&1
rd /s /q src\Central.Gtk\obj > NUL 2>&1
rd /s /q src\Commands\obj > NUL 2>&1
rd /s /q src\Commands.Cli\obj > NUL 2>&1
rd /s /q src\Commands.WinForms\obj > NUL 2>&1
rd /s /q src\Commands.Gtk\obj > NUL 2>&1
rd /s /q src\Hooking\obj > NUL 2>&1
rd /s /q src\Launcher.Cli\obj > NUL 2>&1
rd /s /q src\Store.Service\obj > NUL 2>&1
rd /s /q src\Store.Management.Cli\obj > NUL 2>&1
rd /s /q src\Store.Management.WinForms\obj > NUL 2>&1
rd /s /q src\Common\obj > NUL 2>&1
rd /s /q src\Common.Gtk\obj > NUL 2>&1
rd /s /q src\Fetchers\obj > NUL 2>&1
rd /s /q src\Injector\obj > NUL 2>&1
rd /s /q src\Model\obj > NUL 2>&1
rd /s /q src\DesktopIntegration\obj > NUL 2>&1
rd /s /q src\Store\obj > NUL 2>&1
rd /s /q src\Test.Common\obj > NUL 2>&1
rd /s /q src\Test.Backend\obj > NUL 2>&1
rd /s /q src\Test.Frontend\obj > NUL 2>&1
rd /s /q src\Test.Tools\obj > NUL 2>&1
rd /s /q src\Publish\obj > NUL 2>&1
rd /s /q src\Publish.Cli\obj > NUL 2>&1
rd /s /q src\Publish.WinForms\obj > NUL 2>&1
rd /s /q src\Capture\obj > NUL 2>&1
rd /s /q src\Capture.Cli\obj > NUL 2>&1
rd /s /q src\Capture.WinForms\obj > NUL 2>&1
rd /s /q src\Updater\obj > NUL 2>&1
rd /s /q src\Updater.WinForms\obj > NUL 2>&1
rd /s /q src\Modeling\obj > NUL 2>&1

rem Remove MonoDevelop user preferences
del "src\*.userprefs" > NUL 2>&1

rem Remove NUnit logs
del *.VisualState.xml > NUL 2>&1
del TestResult.xml > NUL 2>&1
