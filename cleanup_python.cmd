@echo off
echo.
echo This script removes files that are not necessary for a minimal Python
echo installation and adds files that are required for portable operation.
pause

cd build\Portable\Python

::Try to get the Python DLL either from the Windows directory or a shared "applications" directory
copy %windir%\system32\python26.dll python26.dll
copy %apps%\Python\python26.dll python26.dll

::Move DLLs used by pygobject into the default search path
copy "%apps%\GTK+\bin\libgobject-2.0-0.dll" libgobject-2.0-0.dll
copy "%apps%\GTK+\bin\libgthread-2.0-0.dll" libgthread-2.0-0.dll
copy "%apps%\GTK+\bin\libglib-2.0-0.dll" libglib-2.0-0.dll

::Move DLLs used by pywin32 into the default search path
move Lib\site-packages\pywin32_system32\pythoncom26.dll pythoncom26.dll
move Lib\site-packages\pywin32_system32\pywintypes26.dll pywintypes26.dll
rmdir Lib\site-packages\pywin32_system32

::Delete unneeded files
del README.txt
del NEWS.txt
del w9xpopen.exe

del *.log
del Remove*.exe

del /s /q Scripts
rmdir /s /q include
rmdir /s /q share\gtk-doc

del DLLs\_bsddb.pyd
del libs\_bsddb.lib
rmdir /s /q Lib\bsddb
del DLLs\_sqlite3.pyd
del DLLs\sqlite3.dll
del libs\_sqlite3.lib
rmdir /s /q Lib\sqlite3

rmdir /s /q Lib\test
rmdir /s /q Lib\lib2to3
rmdir /s /q Lib\wsgiref
rmdir /s /q Lib\site-packages\adodbapi
rmdir /s /q Lib\site-packages\pythonwin
rmdir /s /q Lib\site-packages\isapi
rmdir /s /q Lib\site-packages\msilib
rmdir /s /q Lib\site-packages\json
rmdir /s /q Lib\site-packages\email
rmdir /s /q Lib\site-packages\hotshot

del Lib\site-packages\PyWin32.chm
del Lib\site-packages\win32com\readme.htm
rmdir /s /q Lib\site-packages\win32com\HTML

rmdir /s /q Lib\site-packages\win32\Demos
rmdir /s /q Lib\site-packages\win32\test
rmdir /s /q Lib\site-packages\win32com\demos
rmdir /s /q Lib\site-packages\win32com\test

rmdir /s /q Lib\site-packages\win32comext\axcontrol
rmdir /s /q Lib\site-packages\win32comext\axdebug
rmdir /s /q Lib\site-packages\win32comext\axscript
rmdir /s /q Lib\site-packages\win32comext\bits
rmdir /s /q Lib\site-packages\win32comext\directsound
rmdir /s /q Lib\site-packages\win32comext\internet
rmdir /s /q Lib\site-packages\win32comext\mapi
rmdir /s /q Lib\site-packages\win32comext\taskscheduler

rmdir /s /q Lib\site-packages\win32comext\adsi\demos
rmdir /s /q Lib\site-packages\win32comext\authorization\demos
rmdir /s /q Lib\site-packages\win32comext\ifilter\demo
rmdir /s /q Lib\site-packages\win32comext\propsys\test
rmdir /s /q Lib\site-packages\win32comext\shell\demos
rmdir /s /q Lib\site-packages\win32comext\shell\test
