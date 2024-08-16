@echo OFF
set NET=net8.0



:FileCopy

echo Del SA all *.pdb 
del /S ".\DDPM.SA\bin\*.pdb"
echo Del UI all *.pdb 
del /S ".\DDPM.UI\bin\*.pdb"

RD /S /Q "_BIN"

mkdir "_BIN"
mkdir "_BIN\SA"
mkdir "_BIN\UI"


xcopy /E /i ".\DDPM.SA\bin\CLI.Subagent\Debug\%NET%-windows10.0.19041.0\*.*" ".\_BIN\SA\CLI"
xcopy /E /i ".\DDPM.SA\bin\DDPM.Subagent\Debug\%NET%-windows10.0.19041.0\*.*" ".\_BIN\SA\System"
xcopy /E /i ".\DDPM.SA\bin\DDPM.Subagent.User\Debug\%NET%-windows10.0.19041.0\*.*" ".\_BIN\SA\User"
:: pause

xcopy /E /i ".\DDPM.UI\bin\%NET%-windows10.0.19041.0\*.*" ".\_BIN\UI"  
