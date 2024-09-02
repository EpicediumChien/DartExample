@echo OFF
set NET=net8.0
:: dotnet.exe build -c "Debug" /p:Framework=%NET% /p:platform="Any CPU" /p:EnableWindowsTargeting=true ".\DDPM.SA\DDPM.SA.sln"
:: dotnet.exe build -c "Debug" /p:Framework=%NET% /p:platform="Any CPU" /p:EnableWindowsTargeting=true ".\DDPM.UI\DDPM.UI.sln"
:: dotnet.exe clean /p:Framework=%NET% /p:platform="Any CPU"/p:EnableWindowsTargeting=true ".\DDPM.SA\DDPM.SA.sln"
:: dotnet.exe clean /p:Framework=%NET% /p:platform="Any CPU"/p:EnableWindowsTargeting=true ".\DDPM.UI\DDPM.UI.sln"
:: Command line ==> build.bat Release
set build_arch="Any CPU"


::Build for [Release] or [Debug]
set ConfigType=%1

:: Call msbuild environment.
:: start /B cmd.exe /C .\SetVSBuildEnvironment.bat

RD /S /Q "_BIN"
::goto FileCopy

echo Clean SA
:: dotnet clean .\Display001\CommModule\AwCommModule.sln /p:platform="x64" /p:configuration=%ConfigType%
:: msbuild .\DDPM.SA\DDPM.SA.sln /t:clean /p:platform=%build_arch% /p:configuration=%ConfigType%
dotnet.exe clean /p:Framework=%NET% /p:platform=%build_arch% /p:EnableWindowsTargeting=true ".\DDPM.SA\DDPM.SA.sln"

if errorlevel 1 goto errorSA
:: pause
echo Build SA
dotnet.exe build -c %ConfigType% /p:Framework=%NET% /p:platform="Any CPU" /p:EnableWindowsTargeting=true ".\DDPM.SA\DDPM.SA.sln"
:: msbuild .\DDPM.SA\DDPM.SA.sln  /p:platform=%build_arch% /p:configuration=%ConfigType%
if errorlevel 1 goto errorSA
echo *************************************
echo BUILD SA SUCCESS
echo *************************************






echo Clean UI
dotnet.exe clean /p:Framework=%NET% /p:platform=%build_arch% /p:EnableWindowsTargeting=true ".\DDPM.UI\DDPM.UI.sln"
:: msbuild .\DDPM.UI\DDPM.UI.sln  /t:clean /p:platform=%build_arch% /p:configuration=%ConfigType%
if errorlevel 1 goto errorUI
:: pause
echo Build UI
dotnet.exe build -c %ConfigType% /p:Framework=%NET% /p:platform="Any CPU" /p:EnableWindowsTargeting=true ".\DDPM.UI\DDPM.UI.sln"
:: msbuild .\DDPM.UI\DDPM.UI.sln /p:platform=%build_arch% /p:configuration=%ConfigType%
if errorlevel 1 goto errorUI
echo *************************************
echo BUILD UI SUCCESS
echo *************************************





goto PassDone

:errorSA
    @echo.
    @echo   ####       #             
    @echo  #          # #        
    @echo  #         #   #        
    @echo   ####    #     #    
    @echo       #   #######     
    @echo       #   #     #      
    @echo   ####    #     #   
    @echo.
goto errorDone

:errorUI
    @echo.
    @echo  #    #     ###           
    @echo  #    #      #         
    @echo  #    #      #           
    @echo  #    #      #         
    @echo  #    #      #     
    @echo  #    #      #      
    @echo   ####      ###        
    @echo.
goto errorDone


:errorDone
    @echo.
    @echo  #######    #      ###   #       
    @echo  #         # #      #    #       
    @echo  #        #   #     #    #       
    @echo  #####   #     #    #    #       
    @echo  #       #######    #    #       
    @echo  #       #     #    #    #       
    @echo  #       #     #   ###   ####### 
    @echo.
goto Finished


:PassDone
    @echo.
    @echo   #####  #     #  #####   #####  #######  #####   #####  
    @echo  #     # #     # #     # #     # #       #     # #     # 
    @echo  #       #     # #       #       #       #       #       
    @echo   #####  #     # #       #       #####    #####   #####  
    @echo        # #     # #       #       #             #       # 
    @echo  #     # #     # #     # #     # #       #     # #     # 
    @echo   #####   #####   #####   #####  #######  #####   #####  
    @echo.

pause


:FileCopy

echo Del SA all *.pdb 
del /S ".\DDPM.SA\bin\*.pdb"
echo Del UI all *.pdb 
del /S ".\DDPM.UI\bin\*.pdb"

RD /S /Q "_BIN"

mkdir "_BIN"
mkdir "_BIN\SA"
mkdir "_BIN\UI"


xcopy /E /i ".\DDPM.SA\bin\CLI.Subagent\%ConfigType%\%NET%-windows10.0.19041.0\*.*" ".\_BIN\SA\CLI"
xcopy /E /i ".\DDPM.SA\bin\DDPM.Subagent\%ConfigType%\%NET%-windows10.0.19041.0\*.*" ".\_BIN\SA\System"
xcopy /E /i ".\DDPM.SA\bin\DDPM.Subagent.User\%ConfigType%\%NET%-windows10.0.19041.0\*.*" ".\_BIN\SA\User"
:: pause

xcopy /E /i ".\DDPM.UI\bin\%NET%-windows10.0.19041.0\*.*" ".\_BIN\UI"  
