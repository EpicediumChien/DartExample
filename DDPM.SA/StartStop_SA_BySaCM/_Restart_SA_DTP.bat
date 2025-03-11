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

reg import DDPM.SA.USER_STOP.reg
echo Stopped User SA.

reg import DDPM.SA_STOP.reg
echo Stopped System SA.

reg import DTP_STOP.reg
echo Stopped DTP SA.


echo Waiting start
pause

reg import DDPM.SA.USER_Start.reg
echo Started User SA.

reg import DDPM.SA_Start.reg
echo Started System SA.

reg import DTP_Start.reg
echo Started DTP SA.