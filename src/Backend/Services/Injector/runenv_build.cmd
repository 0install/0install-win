@echo off
"%windir%\Microsoft.NET\Framework\v3.5\csc.exe" /nologo /win32manifest:"%~dp0..\..\..\Frontend\Commands.Cli\Properties\app.manifest" /out:"%~dp0runenv.clr2.template" "%~dp0runenv.cs"
"%windir%\Microsoft.NET\Framework\v4.0.30319\csc.exe" /nologo /win32manifest:"%~dp0..\..\..\Frontend\Commands.Cli\Properties\app.manifest" /out:"%~dp0runenv.clr4.template" "%~dp0runenv.cs"