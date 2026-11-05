@echo off
:: Builds Fix-Rockstar.exe with the C# compiler that ships with Windows. No SDK needed.
set FW=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319
"%FW%\csc.exe" /nologo /target:winexe /out:"%~dp0Fix-Rockstar.exe" /win32manifest:"%~dp0src\app.manifest" ^
  /r:"%FW%\WPF\PresentationFramework.dll" /r:"%FW%\WPF\PresentationCore.dll" /r:"%FW%\WPF\WindowsBase.dll" ^
  /r:System.Xaml.dll /r:System.Core.dll "%~dp0src\App.cs"
