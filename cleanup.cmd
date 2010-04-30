@echo off

rem Clear binaries (leave final Documentation intact, because it takes so long to generate)
rd /s /q bin > NUL 2>&1
rd /s /q build\Debug > NUL 2>&1
rd /s /q build\Release > NUL 2>&1
rd /s /q build\Setup > NUL 2>&1
rd /s /q build\Documentation\working > NUL 2>&1

rem Clear ReSharper's cache
rd /s /q vs\_ReSharper.ZeroInstall_VS2008 > NUL 2>&1
rd /s /q vs\_ReSharper.ZeroInstall_VS2010 > NUL 2>&1
copy "vs\ZeroInstall_VS2008.4.5.ReSharper.user.Backup" "vs\ZeroInstall_VS2008.4.5.ReSharper.user" > NUL
copy "vs\ZeroInstall_VS2010.5.0.ReSharper.user.Backup" "vs\ZeroInstall_VS2010.5.0.ReSharper.user" > NUL

rem Clear VS.NET's object cache
del vs\*.cache > NUL 2>&1
rd /s /q vs\Central\obj > NUL 2>&1
rd /s /q vs\Launcher.Cli\obj > NUL 2>&1
rd /s /q vs\Launcher.Gui\obj > NUL 2>&1
rd /s /q vs\CacheManagement.Cli\obj > NUL 2>&1
rd /s /q vs\CacheManagement.Gui\obj > NUL 2>&1
rd /s /q vs\FeedEditor\obj > NUL 2>&1
rd /s /q vs\Common\obj > NUL 2>&1
rd /s /q vs\Model\obj > NUL 2>&1
rd /s /q vs\Solver\obj > NUL 2>&1
rd /s /q vs\Store\obj > NUL 2>&1
rd /s /q vs\Injector\obj > NUL 2>&1

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

rem Remove NUnit logs
del *.VisualState.xml > NUL 2>&1
del TestResult.xml > NUL 2>&1