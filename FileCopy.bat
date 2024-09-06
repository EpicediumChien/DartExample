@echo OFF
set NET=net8.0


::Build for [Release] or [Debug]
set ConfigType=%1

:FileCopy
echo copy support list.
xcopy /E /i ".\DDPM.SA\dll\SupportEncrypted.txt" ".\DDPM.SA\bin\DDPM.Subagent.User\%ConfigType%\%NET%-windows10.0.19041.0\" /Y

echo Del SA all *.pdb 
del /Q /F /S ".\DDPM.SA\bin\*.pdb"
echo Del UI all *.pdb 
del /Q /F /S ".\DDPM.UI\bin\*.pdb"
echo Del UI all *.pdb 
del /Q /F /S ".\DDPM.SA\VCPSDK\VCPSDK\bin\*.pdb"

RD /S /Q "_BIN"

mkdir "_BIN"
mkdir "_BIN\SA"
mkdir "_BIN\UI"
mkdir "_BIN\VCPSDK"

xcopy /E /i ".\DDPM.SA\VCPSDK\VCPSDK\bin\%ConfigType%\%NET%-windows10.0.19041.0\*.*" ".\_BIN\VCPSDK"
xcopy /E /i ".\DDPM.SA\bin\CLI.Subagent\%ConfigType%\%NET%-windows10.0.19041.0\*.*" ".\_BIN\SA\CLI"
xcopy /E /i ".\DDPM.SA\bin\DDPM.Subagent\%ConfigType%\%NET%-windows10.0.19041.0\*.*" ".\_BIN\SA\System"
xcopy /E /i ".\DDPM.SA\bin\DDPM.Subagent.User\%ConfigType%\%NET%-windows10.0.19041.0\*.*" ".\_BIN\SA\User"
:: pause

xcopy /E /i ".\DDPM.UI\bin\%NET%-windows10.0.19041.0\*.*" ".\_BIN\UI"  
