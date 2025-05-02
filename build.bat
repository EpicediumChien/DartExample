@echo OFF
:: start /wait /B cmd.exe /C .\del_files.bat




set NET=net8.0
:: dotnet.exe build -c "Debug" /p:Framework=%NET% /p:platform="Any CPU" /p:EnableWindowsTargeting=true ".\DDPM.SA\DDPM.SA.sln"
:: dotnet.exe build -c "Debug" /p:Framework=%NET% /p:platform="Any CPU" /p:EnableWindowsTargeting=true ".\DDPM.UI\DDPM.UI.sln"
:: dotnet.exe clean /p:Framework=%NET% /p:platform="Any CPU"/p:EnableWindowsTargeting=true ".\DDPM.SA\DDPM.SA.sln"
:: dotnet.exe clean /p:Framework=%NET% /p:platform="Any CPU"/p:EnableWindowsTargeting=true ".\DDPM.UI\DDPM.UI.sln"
:: Command line ==> build.bat Release
set build_arch="Any CPU"


::Build for [Release] or [Debug]
set ConfigType=%1


::-----------
::SA related folders
set Dir_Subagent_assemblies=.\DDPM.SA\dll
set Dir_Subagent_CommonDll=.\DDPM.SA\bin\CommonDll\%ConfigType%\net8.0-windows10.0.19041.0
set Dir_Subagent_user=.\DDPM.SA\bin\DDPM.Subagent.User\%ConfigType%\net8.0-windows10.0.19041.0
set Dir_Subagent_sys=.\DDPM.SA\bin\DDPM.Subagent\%ConfigType%\net8.0-windows10.0.19041.0
set Dir_Subagent_cli=.\DDPM.SA\bin\CLI.Subagent\%ConfigType%\net8.0-windows10.0.19041.0
::-----------
::UI related folders
set Dir_UI_CommonDll=.\DDPM.UI\CommonDll
set Dir_UI_output=.\DDPM.UI\bin\net8.0-windows10.0.19041.0



::It's going to build UI.
set GetGotoUI=%2

if "%GetGotoUI%"=="UI" goto BuildUI

:: Call msbuild environment.
:: start /B cmd.exe /C .\SetVSBuildEnvironment.bat


::goto FileCopy

::
:: Build DdmLibrary.dll
::
echo Clean DDPM.SA\DdmLibrary(Decrypt)
dotnet.exe clean /p:Framework=%NET% /p:platform=%build_arch% /p:EnableWindowsTargeting=true ".\DDPM.SA\Decrypt\Decrypt.sln"
if errorlevel 1 goto errorDdmLibrary
echo Build DdmLibrary
dotnet.exe build -c %ConfigType% /p:Framework=%NET% /p:platform=%build_arch% /p:EnableWindowsTargeting=true ".\DDPM.SA\Decrypt\Decrypt.sln"
if errorlevel 1 goto errorDdmLibrary
echo *************************************
echo BUILD DdmLibrary SUCCESS
echo BUILD DdmLibrary SUCCESS
echo BUILD DdmLibrary SUCCESS
echo *************************************
xcopy /Y ".\DDPM.SA\Decrypt\ConsoleApp2\bin\%ConfigType%\%NET%-windows10.0.19041.0\DdmLibrary.dll" ".\DDPM.SA\dll\"  
xcopy /Y ".\DDPM.SA\Decrypt\ConsoleApp2\bin\%ConfigType%\%NET%-windows10.0.19041.0\DdmLibrary.deps.json" ".\DDPM.SA\dll\"  



::
:: Build DDPM.Easy.Common
::
echo Clean DDPM.UI\DDPM.Easy.Common
dotnet.exe clean /p:Framework=%NET% /p:platform=%build_arch% /p:EnableWindowsTargeting=true ".\DDPM.SA\Common\DDPM.Easy.Common\DDPM.Easy.Common.sln"
if errorlevel 1 goto errorEAComm
echo Build VCPSDK
dotnet.exe build -c %ConfigType% /p:Framework=%NET% /p:platform=%build_arch% /p:EnableWindowsTargeting=true ".\DDPM.SA\Common\DDPM.Easy.Common\DDPM.Easy.Common.sln"
if errorlevel 1 goto errorEAComm
echo *************************************
echo BUILD DDPM.Easy.Common SUCCESS
echo BUILD DDPM.Easy.Common SUCCESS
echo BUILD DDPM.Easy.Common SUCCESS
echo *************************************
xcopy /Y /S /Q ".\DDPM.SA\bin\CommonDll\%ConfigType%\%NET%-windows10.0.19041.0\DDPM.Easy.Common.dll" ".\DDPM.SA\dll\DDPM.Easy.Common.dll"  
xcopy /Y /S /Q ".\DDPM.SA\bin\CommonDll\%ConfigType%\%NET%-windows10.0.19041.0\DDPM.Easy.Common.deps.json" ".\DDPM.SA\dll\DDPM.Easy.Common.deps.json"  



echo Clean VCPSDK
dotnet.exe clean /p:Framework=%NET% /p:platform=%build_arch% /p:EnableWindowsTargeting=true ".\DDPM.SA\VCPSDK\VCPSDK.sln"
:: msbuild .\DDPM.UI\DDPM.UI.sln  /t:clean /p:platform=%build_arch% /p:configuration=%ConfigType%
if errorlevel 1 goto errorVCPSDK
:: pause
echo Build VCPSDK
dotnet.exe build -c %ConfigType% /p:Framework=%NET% /p:platform=%build_arch% /p:EnableWindowsTargeting=true ".\DDPM.SA\VCPSDK\VCPSDK.sln"
:: msbuild .\DDPM.UI\DDPM.UI.sln /p:platform=%build_arch% /p:configuration=%ConfigType%
if errorlevel 1 goto errorVCPSDK
echo *************************************
echo BUILD VCPSDK SUCCESS
echo BUILD VCPSDK SUCCESS
echo BUILD VCPSDK SUCCESS
echo *************************************




echo Clean SA
:: dotnet clean .\Display001\CommModule\AwCommModule.sln /p:platform="x64" /p:configuration=%ConfigType%
:: msbuild .\DDPM.SA\DDPM.SA.sln /t:clean /p:platform=%build_arch% /p:configuration=%ConfigType%
dotnet.exe clean /p:Framework=%NET% /p:platform=%build_arch% /p:EnableWindowsTargeting=true ".\DDPM.SA\DDPM.SA.sln"
if errorlevel 1 goto errorSA

:: pause
echo Build SA
dotnet.exe build -c %ConfigType% /p:Framework=%NET% /p:platform=%build_arch% /p:EnableWindowsTargeting=true ".\DDPM.SA\DDPM.SA.sln"
:: msbuild .\DDPM.SA\DDPM.SA.sln  /p:platform=%build_arch% /p:configuration=%ConfigType%
if errorlevel 1 goto errorSA

xcopy "%Dir_Subagent_CommonDll%\*.*" "%Dir_UI_CommonDll%\" /Y /S /Q
xcopy "%Dir_Subagent_CommonDll%\DDPM.SA.Common.*" "%RootDir%\DdpmSwUpdater\CommonDll\" /Y /S /Q
xcopy "%Dir_Subagent_CommonDll%\VcpCore.Common.*" "%RootDir%\DdpmSwUpdater\CommonDll\" /Y /S /Q

echo *************************************
echo BUILD SA SUCCESS
echo BUILD SA SUCCESS
echo BUILD SA SUCCESS
echo *************************************




:BuildUI

echo Clean UI
dotnet.exe clean /p:Framework=%NET% /p:platform=%build_arch% /p:EnableWindowsTargeting=true ".\DDPM.UI\DDPM.UI.sln"
:: msbuild .\DDPM.UI\DDPM.UI.sln  /t:clean /p:platform=%build_arch% /p:configuration=%ConfigType%
if errorlevel 1 goto errorUI
:: pause
echo Build UI
dotnet.exe build -c %ConfigType% /p:Framework=%NET% /p:platform=%build_arch% /p:EnableWindowsTargeting=true ".\DDPM.UI\DDPM.UI.sln"
:: msbuild .\DDPM.UI\DDPM.UI.sln /p:platform=%build_arch% /p:configuration=%ConfigType%
if errorlevel 1 goto errorUI
echo *************************************
echo BUILD UI SUCCESS
echo BUILD UI SUCCESS
echo BUILD UI SUCCESS
echo BUILD UI SUCCESS
echo *************************************



echo Clean DdpmSwUpdater
dotnet.exe clean /p:Framework=%NET% /p:platform=%build_arch% /p:EnableWindowsTargeting=true ".\DdpmSwUpdater\DdpmSwUpdater.sln"
:: msbuild .\DDPM.UI\DDPM.UI.sln  /t:clean /p:platform=%build_arch% /p:configuration=%ConfigType%
if errorlevel 1 goto errorUI
:: pause
echo Build UI
dotnet.exe build -c %ConfigType% /p:Framework=%NET% /p:platform=%build_arch% /p:EnableWindowsTargeting=true ".\DdpmSwUpdater\DdpmSwUpdater.sln"
:: msbuild .\DDPM.UI\DDPM.UI.sln /p:platform=%build_arch% /p:configuration=%ConfigType%
if errorlevel 1 goto errorUI
echo *************************************
echo BUILD Mini SUCCESS
echo *************************************





goto PassDone

:errorDdmLibrary
echo ----------------------------------------
echo ---- ERROR : Build DdmLibrary ERROR ----
echo ----------------------------------------
goto errorDone

:errorEAComm
    @echo.
    @echo  #####       #             
    @echo  #          # #        
    @echo  #         #   #        
    @echo  ####     #     #    
    @echo  #        #######     
    @echo  #        #     #      
    @echo  #####    #     #   
    @echo.
goto errorDone


:errorVCPSDK
    @echo.
    @echo   ####    #####    #    #       
    @echo  #        #    #   #   #  
    @echo  #        #    #   #  #   
    @echo   ####    #    #   ### 
    @echo       #   #    #   #  #
    @echo       #   #    #   #   #
    @echo   ####    ####     #    #
    @echo.
goto errorDone

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
echo copy support list.
xcopy /E /i ".\DDPM.SA\dll\LSTDDPM" ".\DDPM.SA\bin\DDPM.Subagent.User\%ConfigType%\%NET%-windows10.0.19041.0\" /Y

echo Del SA all *.pdb 
del /S ".\DDPM.SA\bin\*.pdb"
echo Del UI all *.pdb 
del /S ".\DDPM.UI\bin\*.pdb"
echo Del UI all *.pdb 
del /S ".\DDPM.SA\VCPSDK\VCPSDK\bin\*.pdb"

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

:Finished
