@echo off
"%windir%\Microsoft.NET\Framework\v3.5\csc.exe" /nologo /win32manifest:"%~dp0..\Commands.Cli\Properties\app.manifest" /out:"%~dp0runenv.netfx20.template" "%~dp0runenv.cs"
"%windir%\Microsoft.NET\Framework\v4.0.30319\csc.exe" /nologo /win32manifest:"%~dp0..\Commands.Cli\Properties\app.manifest" /out:"%~dp0runenv.netfx40.template" "%~dp0runenv.cs"