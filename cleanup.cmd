@echo off
::Removes compilation artifacts and other temporary files.

rem Clear binaries
rd /s /q "%~dp0build"

rem Clear JetBrains caches
rd /s /q "%~dp0src\_ReSharper.ZeroInstall_VS2012" > NUL 2>&1
rd /s /q "%~dp0src\_dotTrace.ZeroInstall_VS2012" > NUL 2>&1
rd /s /q "%~dp0src\_TeamCity.ZeroInstall_VS2012" > NUL 2>&1

rem Clear caches and per-user preferences
attrib -h "%~dp0src\*.suo" > NUL 2>&1
del "%~dp0src\*.suo" > NUL 2>&1
del "%~dp0src\*.user" > NUL 2>&1
del "%~dp0src\*.cache" > NUL 2>&1
rd /s /q "%~dp0src\Alias.Cli\obj" > NUL 2>&1
rd /s /q "%~dp0src\Backend\obj" > NUL 2>&1
rd /s /q "%~dp0src\Capture\obj" > NUL 2>&1
rd /s /q "%~dp0src\Capture.Cli\obj" > NUL 2>&1
rd /s /q "%~dp0src\Capture.WinForms\obj" > NUL 2>&1
rd /s /q "%~dp0src\Central\obj" > NUL 2>&1
rd /s /q "%~dp0src\Central.Gtk\obj" > NUL 2>&1
rd /s /q "%~dp0src\Central.WinForms\obj" > NUL 2>&1
rd /s /q "%~dp0src\Commands\obj" > NUL 2>&1
rd /s /q "%~dp0src\Commands.Cli\obj" > NUL 2>&1
rd /s /q "%~dp0src\Commands.Gtk\obj" > NUL 2>&1
rd /s /q "%~dp0src\Commands.WinForms\obj" > NUL 2>&1
rd /s /q "%~dp0src\Common\obj" > NUL 2>&1
rd /s /q "%~dp0src\Common.Gtk\obj" > NUL 2>&1
rd /s /q "%~dp0src\Common.WinForms\obj" > NUL 2>&1
rd /s /q "%~dp0src\DesktopIntegration\obj" > NUL 2>&1
rd /s /q "%~dp0src\Fetchers\obj" > NUL 2>&1
rd /s /q "%~dp0src\Hooking\obj" > NUL 2>&1
rd /s /q "%~dp0src\Injector\obj" > NUL 2>&1
rd /s /q "%~dp0src\Launcher.Cli\obj" > NUL 2>&1
rd /s /q "%~dp0src\Model\obj" > NUL 2>&1
rd /s /q "%~dp0src\Publish\obj" > NUL 2>&1
rd /s /q "%~dp0src\Publish.Cli\obj" > NUL 2>&1
rd /s /q "%~dp0src\Publish.WinForms\obj" > NUL 2>&1
rd /s /q "%~dp0src\Sample.Boo\obj" > NUL 2>&1
rd /s /q "%~dp0src\Sample.CSharp\obj" > NUL 2>&1
rd /s /q "%~dp0src\Sample.FSharp\obj" > NUL 2>&1
rd /s /q "%~dp0src\Sample.IPython\obj" > NUL 2>&1
rd /s /q "%~dp0src\Sample.VBasic\obj" > NUL 2>&1
rd /s /q "%~dp0src\Solvers\obj" > NUL 2>&1
rd /s /q "%~dp0src\Store\obj" > NUL 2>&1
rd /s /q "%~dp0src\Store.Management.Cli\obj" > NUL 2>&1
rd /s /q "%~dp0src\Store.Service\obj" > NUL 2>&1
rd /s /q "%~dp0src\Test.Backend\obj" > NUL 2>&1
rd /s /q "%~dp0src\Test.Common\obj" > NUL 2>&1
rd /s /q "%~dp0src\Test.Frontend\obj" > NUL 2>&1
rd /s /q "%~dp0src\Test.Tools\obj" > NUL 2>&1
rd /s /q "%~dp0src\Updater\obj" > NUL 2>&1
rd /s /q "%~dp0src\Updater.WinForms\obj" > NUL 2>&1
rd /s /q "%~dp0src\TestResults" > NUL 2>&1

rem Remove MonoDevelop user preferences
del "%~dp0src\*.userprefs" > NUL 2>&1

rem Remove NUnit logs
del "%~dp0*.VisualState.xml" > NUL 2>&1
del "%~dp0TestResult.xml" > NUL 2>&1
