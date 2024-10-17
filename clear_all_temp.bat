set "params=%*"
set RootDir=%~dp0
cd /d "%~dp0" && ( if exist "%temp%\getadmin.vbs" del "%temp%\getadmin.vbs" ) && fsutil dirty query %systemdrive% 1>nul 2>nul || (  echo Set UAC = CreateObject^("Shell.Application"^) : UAC.ShellExecute "cmd.exe", "/k cd ""%~sdp0"" && ""%~s0"" %params%", "", "runas", 1 >> "%temp%\getadmin.vbs" && "%temp%\getadmin.vbs" && exit /B )

Echo -------------------------------------------
Echo [Clear all temp folder (bin and obj)]
Echo -------------------------------------------
for /d /r "%RootDir%" %%d in (bin,obj,_bin,.vs) do (
    if exist "%%d" (
        echo Deleting folder %%d and its contents
        rd /s /q "%%d"
    )
)
exit /b 0
exit