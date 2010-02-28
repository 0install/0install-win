@echo off

rem Clear binaries
rd /s /q vs\bin > NUL 2>&1
rd /s /q ..\build\windows > NUL 2>&1
rd /s /q ..\build\windows_setup > NUL 2>&1

rem Clear ReSharper's cache
rd /s /q vs\_ReSharper.ZeroInstall_VS2008 > NUL 2>&1
rd /s /q vs\_ReSharper.ZeroInstall_VS2010 > NUL 2>&1
copy "vs\ZeroInstall_VS2008.4.5.ReSharper.user.Backup" "vs\ZeroInstall_VS2008.4.5.ReSharper.user" > NUL
copy "vs\ZeroInstall_VS2010.5.0.ReSharper.user.Backup" "vs\ZeroInstall_VS2010.5.0.ReSharper.user" > NUL

rem Clear VS.NET's object cache
del vs\*.cache > NUL 2>&1
rd /s /q vs\Launchpad\obj > NUL 2>&1
rd /s /q vs\FeedEditor\obj > NUL 2>&1
rd /s /q vs\Common\obj > NUL 2>&1
rd /s /q vs\Backend\obj > NUL 2>&1
rd /s /q vs\Settings\obj > NUL 2>&1

rem Restore old VS2008 solution user options (temporarily unhide for copying)
attrib -h "vs\ZeroInstall_VS2008.suo.Backup" > NUL 2>&1
attrib -h "vs\ZeroInstall_VS2008.suo" > NUL 2>&1
copy "vs\ZeroInstall_VS2008.suo.Backup" "vs\ZeroInstall_VS2008.suo" > NUL 2>&1
attrib +h "vs\ZeroInstall_VS2008.suo.Backup" > NUL 2>&1
attrib +h "vs\ZeroInstall_VS2008.suo" > NUL 2>&1

rem Restore old VS2010 solution user options (temporarily unhide for copying)
attrib -h "vs\ZeroInstall_VS2010.suo.Backup" > NUL 2>&1
attrib -h "vs\ZeroInstall_VS2010.suo" > NUL 2>&1
copy "vs\ZeroInstall_VS2010.suo.Backup" "vs\ZeroInstall_VS2010.suo" > NUL 2>&1
attrib +h "vs\ZeroInstall_VS2010.suo.Backup" > NUL 2>&1
attrib +h "vs\ZeroInstall_VS2010.suo" > NUL 2>&1
