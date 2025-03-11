@echo off
:: 提升權限
:: Check for administrative privileges
openfiles >nul 2>&1
if %errorlevel% neq 0 (
    echo Requesting administrative privileges...
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

:: 設置目前目錄為工作目錄
cd /d %~dp0

:: 刪除 %localappdata%\Dell\Dell Display and Peripheral Manager\log 資料夾裡面的資料夾與檔案
echo Deleting files in %localappdata%\Dell\Dell Display and Peripheral Manager\log
rmdir /s /q "%localappdata%\Dell\Dell Display and Peripheral Manager\log"

:: 刪除 C:\ProgramData\Dell\DDPM.Subagent 裡面的檔案
echo Deleting files in C:\ProgramData\Dell\DDPM.Subagent
del /f /q "C:\ProgramData\Dell\DDPM.Subagent\*"

:: 刪除 C:\ProgramData\Dell\DTP\Logs\DTP.Instrumentation.SubAgent 裡面的檔案
echo Deleting files in C:\ProgramData\Dell\DTP\Logs\DTP.Instrumentation.SubAgent
del /f /q "C:\ProgramData\Dell\DTP\Logs\DTP.Instrumentation.SubAgent\*"

echo Operation completed.
pause