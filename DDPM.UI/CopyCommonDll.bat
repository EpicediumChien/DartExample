@echo off
setlocal enabledelayedexpansion

cd /d %~dp0

xcopy /s /e /y "..\DDPM.SA\bin\CommonDll\Debug\net8.0-windows10.0.19041.0\"  ".\CommonDll"

pause
exit