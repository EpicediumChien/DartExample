@echo off
setlocal enabledelayedexpansion
set "ResultCode=0"

cd /d %~dp0
if exist "ListDIR.txt" del /f /s "ListDIR.txt"
if exist "ListFile.txt" del /f /s "ListFile.txt"


::Delete ".bak" files
cd /d %~dp0
dir /S /A:-D /B /O:N > ListFile.txt
for /f "delims=" %%b in (ListFile.txt) do (
    echo %%b
    echo %%~xb
    set "tmp=%%~xb"
    if "!tmp!"==".bak" del /f /s "%%b"
)

::Delete bin folders
::Delete obj folders
cd /d %~dp0
dir /s /b /ad > ListDIR.txt
for /f "delims=" %%a in (ListDIR.txt) do (
    echo %%a
    echo %%~na
    set "tmp=%%~na"
    if "!tmp!"=="bin" rmdir /s /q "%%a"
    if "!tmp!"=="obj" rmdir /s /q "%%a"
)

::Delete ".vs" folders
cd /d %~dp0
if exist "DDPM.SA"\ (
    cd DDPM.SA
    if exist ".vs"\ rmdir /s /q ".vs"
) else (
    echo "DDPM.SA NOT exist"
)

::Delete ".vs" folders
cd /d %~dp0
if exist "DDPM.UI"\ (
    cd DDPM.UI
    if exist ".vs"\ rmdir /s /q ".vs"
) else (
    echo "DDPM.UI NOT exist"
)


cd /d %~dp0
if exist "ListDIR.txt" del /f /s "ListDIR.txt"
if exist "ListFile.txt" del /f /s "ListFile.txt"

PAUSE
exit /b %ResultCode%
