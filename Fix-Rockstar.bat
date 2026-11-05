@echo off
setlocal
title Rockstar Games Launcher Fix

:: Relaunch as admin if needed
net session >nul 2>&1 || (powershell -NoProfile -Command "Start-Process -Verb RunAs -FilePath '%~f0'" & exit /b)

:menu
cls
echo ==============================================
echo   ROCKSTAR LAUNCHER FIX / FiveM exit code 506
echo   "timed out loading online and offline content"
echo ==============================================
echo.
echo  Try steps one by one and test the launcher after each,
echo  or pick A to run everything at once.
echo.
echo  1  Kill Rockstar / FiveM / GTA processes
echo  2  Sync Windows clock (wrong time = connection errors)
echo  3  Flush DNS + reset network (Winsock, IP)
echo  4  Clear launcher and Social Club cache (renamed to .bak)
echo  5  Restart Rockstar service
echo  6  Start Rockstar Launcher (WITHOUT admin rights, as FiveM requires)
echo  7  Set DNS to 1.1.1.1 / 8.8.8.8          R  Restore automatic DNS
echo  8  Check hosts file (rockstar/socialclub lines)
echo  9  Check and disable proxy
echo  P  Fix folder permissions (use if it only works as admin)
echo  A  EVERYTHING (P, 1-5, 8, 9), then start launcher
echo  0  Exit
echo.
set /p c=Choice:
if /i "%c%"=="1" call :kill
if /i "%c%"=="2" call :time
if /i "%c%"=="3" call :net
if /i "%c%"=="4" call :cache
if /i "%c%"=="5" call :svc
if /i "%c%"=="6" call :run
if "%c%"=="7" call :dns
if /i "%c%"=="R" call :dnsreset
if "%c%"=="8" call :hosts
if "%c%"=="9" call :proxy
if /i "%c%"=="P" call :perm
if /i "%c%"=="A" call :kill & call :perm & call :time & call :net & call :cache & call :svc & call :hosts & call :proxy & call :run
if "%c%"=="0" exit /b
echo.
pause
goto menu

:kill
echo [1] Killing processes...
for %%p in (Launcher.exe LauncherPatcher.exe RockstarService.exe SocialClubHelper.exe PlayGTAV.exe GTA5.exe FiveM.exe) do taskkill /f /im %%p >nul 2>&1
echo     done.
exit /b

:time
echo [2] Syncing clock...
net start w32time >nul 2>&1
w32tm /resync >nul 2>&1
echo     done.
exit /b

:net
echo [3] Flushing DNS and resetting network...
ipconfig /flushdns
netsh winsock reset >nul
netsh int ip reset >nul
echo     done. (Reboot the PC for full effect.)
exit /b

:cache
echo [4] Launcher cache (original folders are renamed to *.bak)...
for %%d in ("%LOCALAPPDATA%\Rockstar Games\Launcher" "%LOCALAPPDATA%\Rockstar Games\Social Club" "%USERPROFILE%\Documents\Rockstar Games\Launcher" "%USERPROFILE%\Documents\Rockstar Games\Social Club") do (
  if exist "%%~d" (
    if exist "%%~d.bak" rd /s /q "%%~d.bak"
    ren "%%~d" "%%~nxd.bak" && echo     OK: %%~d
  )
)
echo     done.
exit /b

:svc
echo [5] Restarting Rockstar service...
sc stop "Rockstar Game Library Service" >nul 2>&1
timeout /t 2 /nobreak >nul
sc start "Rockstar Game Library Service" >nul 2>&1
echo     done.
exit /b

:perm
echo [P] Fixing Rockstar folder permissions for user %USERNAME%...
call :grant "%ProgramFiles%\Rockstar Games"
call :grant "%ProgramData%\Rockstar Games"
call :grant "%LOCALAPPDATA%\Rockstar Games"
call :grant "%USERPROFILE%\Documents\Rockstar Games"
echo     done.
exit /b

:grant
if not exist %1 exit /b
takeown /f %1 /r /d y >nul 2>&1
icacls %1 /grant "%USERNAME%":(OI)(CI)F /t /c /q >nul
echo     OK: %~1
exit /b

:dns
echo [7] Setting DNS on active adapters...
powershell -NoProfile -Command "Get-NetAdapter | ? Status -eq 'Up' | Set-DnsClientServerAddress -ServerAddresses 1.1.1.1,8.8.8.8"
ipconfig /flushdns >nul
echo     done.
exit /b

:dnsreset
echo [R] Restoring automatic DNS...
powershell -NoProfile -Command "Get-NetAdapter | ? Status -eq 'Up' | Set-DnsClientServerAddress -ResetServerAddresses"
echo     done.
exit /b

:hosts
echo [8] Checking hosts file...
set H=%SystemRoot%\System32\drivers\etc\hosts
findstr /i "rockstar socialclub" "%H%" >nul || (echo     All good, no suspicious lines. & exit /b)
echo     Found:
findstr /i "rockstar socialclub" "%H%"
set /p y=Delete these lines? Backup goes to hosts.bak [y/n]:
if /i not "%y%"=="y" exit /b
copy /y "%H%" "%H%.bak" >nul
findstr /v /i "rockstar socialclub" "%H%.bak" > "%H%"
echo     done.
exit /b

:proxy
echo [9] Checking proxy...
reg query "HKCU\Software\Microsoft\Windows\CurrentVersion\Internet Settings" /v ProxyEnable | findstr "0x1" >nul && echo     Proxy was ON, disabling.
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Internet Settings" /v ProxyEnable /t REG_DWORD /d 0 /f >nul
netsh winhttp reset proxy >nul
echo     done. Turn off any VPN manually.
exit /b

:run
echo [6] Starting launcher without admin rights...
set L=%ProgramFiles%\Rockstar Games\Launcher\Launcher.exe
if not exist "%L%" set L=%ProgramFiles(x86)%\Rockstar Games\Launcher\Launcher.exe
if exist "%L%" (explorer.exe "%L%") else echo     Launcher not found - start it manually.
exit /b
