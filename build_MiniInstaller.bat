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


:: Call msbuild environment.
:: start /B cmd.exe /C .\SetVSBuildEnvironment.bat


::goto FileCopy

::
:: Build DdmLibrary.dll
::
echo Clean MiniInstaller
dotnet.exe clean /p:Framework=%NET% /p:platform=%build_arch% /p:EnableWindowsTargeting=true ".\MiniInstaller\MiniInstaller.sln"
if errorlevel 1 goto errorMiniInstaller
echo Build MiniInstaller
dotnet.exe build -c %ConfigType% /p:Framework=%NET% /p:platform=%build_arch% /p:EnableWindowsTargeting=true ".\MiniInstaller\MiniInstaller.sln"
if errorlevel 1 goto errorMiniInstaller
echo *************************************
echo BUILD DdmLibrary SUCCESS
echo BUILD DdmLibrary SUCCESS
echo BUILD DdmLibrary SUCCESS
echo *************************************


goto PassDone

:errorMiniInstaller
echo -------------------------------------------
echo ---- ERROR : Build MiniInstaller ERROR ----
echo -------------------------------------------
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
xcopy /E /i ".\DDPM.SA\dll\SupportEncrypted.txt" ".\DDPM.SA\bin\DDPM.Subagent.User\%ConfigType%\%NET%-windows10.0.19041.0\" /Y

echo Del SA *.pdb 
del /S ".\MiniInstaller\bin\%ConfigType%\%NET%-windows10.0.19041.0\*.pdb"

