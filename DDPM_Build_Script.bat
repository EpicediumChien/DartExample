@echo off
::-----------
::Base param
::-----------
IF "%1"=="?" goto _HELPER
IF "%1"=="help" goto _HELPER
IF "%1"=="Help" goto _HELPER
IF "%1"=="HELP" goto _HELPER
:: build_type : default is Release, set %1 param to Debug for RD debug build
set build_type=Release
:: build_arch : do not change this param if no specific reason
set build_arch="Any CPU"
set "RootDir=%cd%"
echo The current folder path is: [%RootDir%]
::-----
:: 1. %1 = "clear", clear all bin/obj under root dir %1
:: 2. %1 = "Debug", Debug build for RD verify and runtime debug (without DTP.Decoupling, only DDPM.SA/DDPM.UI/DdpmSwUpdater)
:: 3. %1 = "Debug_UI", Debug build for RD verify and runtime debug with UI only
:: 4. %1 = "Debug_SA", Debug build for RD verify and runtime debug with SA only
:: 5. %1 is empty, means Release build
set option_cmd=%1
IF "%1"=="Debug" (
	set build_type=Debug
	echo "*** Build with Debug(1) ***"
)
IF "%1"=="debug" (
	set build_type=Debug
	echo "*** Build with Debug(2) ***"
)
IF "%1"=="DEBUG" (
	set build_type=Debug
	echo "*** Build with Debug(3) ***"
)
::-----
IF "%1"=="debug_ui" (
	set build_type=Debug
	echo "*** Build with UI Debug(1) ***"
)
IF "%1"=="DEBUG_UI" (
	set build_type=Debug
	set option_cmd=debug_ui
	echo "*** Build with UI Debug(2) ***"
)
IF "%1"=="Debug_UI" (
	set build_type=Debug
	set option_cmd=debug_ui
	echo "*** Build with UI Debug(3) ***"
)
::-----
IF "%1"=="debug_sa" (
	set build_type=Debug
	set option_cmd=debug_sa
	echo "*** Build with SA Debug(1) ***"
)
IF "%1"=="DEBUG_SA" (
	set build_type=Debug
	set option_cmd=debug_sa
	echo "*** Build with SA Debug(2) ***"
)
IF "%1"=="Debug_SA" (
	set build_type=Debug
	set option_cmd=debug_sa
	echo "*** Build with SA Debug(3) ***"
)
echo The build type is [%build_type%]
::-----------
::SA related folders
set Dir_Subagent_assemblies=.\DDPM.SA\dll
set Dir_Subagent_CommonDll=.\DDPM.SA\bin\CommonDll\%build_type%\net8.0-windows10.0.19041.0
set Dir_Subagent_user=.\DDPM.SA\bin\DDPM.Subagent.User\%build_type%\net8.0-windows10.0.19041.0
set Dir_Subagent_sys=.\DDPM.SA\bin\DDPM.Subagent\%build_type%\net8.0-windows10.0.19041.0
set Dir_Subagent_cli=.\DDPM.SA\bin\CLI.Subagent\%build_type%\net8.0-windows10.0.19041.0
::-----------
::UI related folders
set Dir_UI_CommonDll=.\DDPM.UI\CommonDll
set Dir_UI_output=.\DDPM.UI\bin\net8.0-windows10.0.19041.0
::-----------
::Installer related folders
set Dir_Ins_cli=.\Installer\Bin\SA\CLI
set Dir_Ins_sys=.\Installer\Bin\SA\System
set Dir_Ins_user=.\Installer\Bin\SA\User
set Dir_Ins_ui=.\Installer\Bin\UI
set Dir_Ins_NKVM=.\Installer\Res
set Dir_Ins_Dependency=.\Installer\Res\Depenencies
::-----------
set target_folder=%RootDir%
IF "%option_cmd%"=="debug_ui" (
	set target_folder=%RootDir%\DDPM.UI
)
echo [target_folder] is [%target_folder%]
::---
Echo -------------------------------------------
Echo [Clear all temp folder (bin and obj)]
Echo -------------------------------------------
for /d /r "%target_folder%" %%d in (bin,obj,_bin) do (
    if exist "%%d" (
        echo Deleting folder %%d and its contents
        rd /s /q "%%d"
    )
)
echo Done.
::---
Echo -------------------------------------------
Echo [Clear all older EA folders]
Echo -------------------------------------------
rd /s /q %RootDir%\DDPM.UI\DDPM.Easy.Common
rd /s /q %RootDir%\DDPM.UI\UnitTest\DDPM.Easy.Common.Tests
echo Done.
::-----------
IF "%1"=="clear" (
    echo [Data cleared], exit directly by command code "clear"
	cd /d "%RootDir%"
	Exit /b 0
)
IF "%1"=="Clear" (
    echo [Data cleared], exit directly by command code "clear"
	cd /d "%RootDir%"
	Exit /b 0
)
IF "%1"=="CLEAR" (
    echo [Data cleared], exit directly by command code "clear"
	cd /d "%RootDir%"
	Exit /b 0
)
pasue
Read-Host
::-----------
if "%option_cmd%"=="debug_ui" goto _ONLY_UI
Echo -------------------------------------------
Echo [Build DDM decryption lib]
Echo -------------------------------------------
cd /d "%RootDir%\DDPM.SA\Decrypt"
dotnet.exe clean -c %build_type% -v minimal /p:Framework="net8.0" /p:platform=%build_arch% /p:EnableWindowsTargeting=true ".\Decrypt.sln"
echo errorlevel is %errorlevel%
if not %errorlevel% == 0 goto DDM_decrypt_CleanFail
dotnet.exe build -c %build_type% -v normal /p:Framework="net8.0" /p:platform=%build_arch% /p:EnableWindowsTargeting=true /p:DebugSymbols=false /p:DebugType=None ".\Decrypt.sln"
echo errorlevel is %errorlevel%
if not %errorlevel% == 0 goto DDM_decrypt_Fail
echo Copy DdmLibrary.dll to SA
xcopy ".\ConsoleApp2\bin\%build_type%\net8.0-windows10.0.19041.0\*.dll" "%RootDir%\DDPM.SA\dll" /Y /S /Q
echo errorlevel is %errorlevel%
if not %errorlevel% == 0 goto copy_ddm_lib_fail
xcopy ".\ConsoleApp2\bin\%build_type%\net8.0-windows10.0.19041.0\*.json" "%RootDir%\DDPM.SA\dll" /Y /S /Q
echo errorlevel is %errorlevel%
if not %errorlevel% == 0 goto copy_ddm_lib_fail
::Echo --------------------------------------------
::Echo [Build EA common]
::Echo --------------------------------------------
::::Build Easy arrange dll, it will be used for DDPM.SA
::cd /d "%RootDir%\DDPM.UI"
::dotnet.exe clean -c %build_type% -v minimal /p:Framework="net8.0" /p:platform=%build_arch% /p:EnableWindowsTargeting=true ".\DDPM.Easy.Common\DDPM.Easy.Common.sln"
::echo errorlevel is %errorlevel%
::if not %errorlevel% == 0 goto EA_CleanFail
::dotnet.exe build -c %build_type% -v normal /p:Framework="net8.0" /p:platform=%build_arch% /p:EnableWindowsTargeting=true /p:DebugSymbols=false /p:DebugType=None ".\DDPM.Easy.Common\DDPM.Easy.Common.sln"
::echo errorlevel is %errorlevel%
::if not %errorlevel% == 0 goto EA_Fail
::::Copy EA dll to SA
::xcopy "%RootDir%\DDPM.UI\bin\net8.0-windows10.0.19041.0\DDPM.Easy.Common.*" "..\DDPM.SA\dll" /Y 
::echo errorlevel is %errorlevel%
::if not %errorlevel% == 0 goto :EA_CopyDllFail
Echo --------------------------------------------
Echo [Build VCPSDK]
Echo --------------------------------------------
cd /d "%RootDir%\DDPM.SA\VCPSDK"
dotnet.exe clean -c %build_type% -v minimal /p:Framework="net8.0" /p:platform=%build_arch% /p:EnableWindowsTargeting=true ".\VCPSDK.sln"
echo errorlevel is %errorlevel%
if not %errorlevel% == 0 goto VCPSDK_CleanFail
dotnet.exe build -c %build_type% -v normal /p:Framework="net8.0" /p:platform=%build_arch% /p:EnableWindowsTargeting=true /p:DebugSymbols=false /p:DebugType=None ".\VCPSDK.sln"
echo errorlevel is %errorlevel%
if not %errorlevel% == 0 goto VCPSDK_Fail
::-----------------
:: If DTP decoupling not exist, skip it.
if not exist "%RootDir%\DTP.Decoupling" (
	echo "*** There is no DTP.Decoupling folder exist, skip [DTP] step ***"
	goto _skipDTP
)
Echo -------------------------------------------
Echo [Build DTP Decoupling]
Echo -------------------------------------------
cd /d "%RootDir%\DTP.Decoupling"
dotnet.exe clean -c %build_type% -v minimal /p:Framework="net8.0" /p:platform=%build_arch% /p:EnableWindowsTargeting=true ".\DTP.Peripheral.Commodity.Plugin.sln"
echo errorlevel is %errorlevel%
if not %errorlevel% == 0 goto DTP_CleanFail
dotnet.exe build -c %build_type% -v minimal /p:Framework="net8.0" /p:platform=%build_arch% /p:EnableWindowsTargeting=true /p:DebugSymbols=false /p:DebugType=None ".\DTP.Peripheral.Commodity.Plugin.sln"
echo errorlevel is %errorlevel%
if not %errorlevel% == 0 goto DTP_Fail
::-------------------------------------
::Prepare file for sharing to installer
RD /S /Q "_BIN"
::mkdir "_BIN"
Echo -------------------------------------------
Echo [Copy necessary output from all projects of DTP.Decoupling]
Echo -------------------------------------------
echo Del DTP all *.pdb 
del /S ".\DDPM.Interface\bin\%build_type%\*.pdb"
del /S ".\src\Plugins\DockCommodity\bin\%build_type%\*.pdb"
del /S ".\src\Plugins\DongleCommodity\bin\%build_type%\*.pdb"
del /S ".\src\Plugins\HeadsetCommodity\bin\%build_type%\*.pdb"
del /S ".\src\Plugins\KeyboardCommodity\bin\%build_type%\*.pdb"
del /S ".\src\Plugins\MouseCommodity\bin\%build_type%\*.pdb"
del /S ".\src\Plugins\PenCommodity\bin\%build_type%\*.pdb"
del /S ".\src\Plugins\SpeakerCommodity\bin\%build_type%\*.pdb"
del /S ".\src\Plugins\WebcamCommodity\bin\%build_type%\*.pdb"
del /S ".\src\Plugins\GlobalPeripheralCommodity\bin\%build_type%\*.pdb"
echo errorlevel is %errorlevel%
mkdir "_BIN"
xcopy ".\DDPM.Interface\bin\%build_type%\*.nupkg" ".\_BIN" /Y /S /Q
echo errorlevel is %errorlevel%
xcopy ".\DDPM.Interface\bin\%build_type%\DDPM.Peripherals.interface.dll" ".\_BIN" /Y /S /Q
echo errorlevel is %errorlevel%
xcopy ".\src\Plugins\DockCommodity\bin\%build_type%\Dell.TechHub.Plugins.DockCommodity.dll" ".\_BIN" /Y /S /Q
echo errorlevel is %errorlevel%
xcopy ".\src\Plugins\HeadsetCommodity\bin\%build_type%\Dell.TechHub.Plugins.HeadsetCommodity.dll" ".\_BIN" /Y /S /Q
echo errorlevel is %errorlevel%
xcopy ".\src\Plugins\KeyboardCommodity\bin\%build_type%\Dell.TechHub.Plugins.KeyboardCommodity.dll" ".\_BIN" /Y /S /Q
echo errorlevel is %errorlevel%
xcopy ".\src\Plugins\MouseCommodity\bin\%build_type%\Dell.TechHub.Plugins.MouseCommodity.dll" ".\_BIN" /Y /S /Q
echo errorlevel is %errorlevel%
xcopy ".\src\Plugins\PenCommodity\bin\%build_type%\Dell.TechHub.Plugins.PenCommodity.dll" ".\_BIN" /Y /S /Q
echo errorlevel is %errorlevel%
xcopy ".\src\Plugins\SpeakerCommodity\bin\%build_type%\Dell.TechHub.Plugins.SpeakerCommodity.dll" ".\_BIN" /Y /S /Q
echo errorlevel is %errorlevel%
xcopy ".\src\Plugins\WebcamCommodity\bin\%build_type%\Dell.TechHub.Plugins.WebcamCommodity.dll" ".\_BIN" /Y /S /Q
echo errorlevel is %errorlevel%
xcopy ".\src\Plugins\DongleCommodity\bin\%build_type%\Dell.TechHub.Plugins.DongleCommodity.dll" ".\_BIN" /Y /S /Q
echo errorlevel is %errorlevel%
xcopy ".\src\Plugins\GlobalPeripheralCommodity\bin\%build_type%\Dell.TechHub.Plugins.GlobalPeripheralCommodity.dll" ".\_BIN" /Y /S /Q
echo errorlevel is %errorlevel%
xcopy ".\src\PeripheralCommon\bin\%build_type%\Dell.TechHub.Peripheral.Common.dll" ".\_BIN" /Y /S /Q
echo errorlevel is %errorlevel%
xcopy ".\Dependencies\DPeMSDK\*.*" ".\_BIN" /Y /S /Q
echo errorlevel is %errorlevel%
xcopy ".\Dependencies\DPeMSDK\*.*" "%RootDir%\DDPM.SA\dll" /Y /S /Q
echo errorlevel is %errorlevel%
xcopy ".\_BIN\*.*" "%RootDir%\DDPM.UI\CommonDll" /Y /S /Q
echo errorlevel is %errorlevel%
xcopy ".\_BIN\*.*" "%RootDir%\DDPM.SA\dll" /Y /S /Q
echo errorlevel is %errorlevel%
goto _copyDPeMSDK

:_skipDTP
echo --------------------------------------------------------------
echo [Copy DPeM SDK 3 brothers to DDPM.SA\dll from DDPM.SA\DTP_dll]
echo --------------------------------------------------------------
cd /d "%RootDir%\DDPM.SA"
xcopy ".\DTP_dll\DPeMClientBroker.*" ".\dll" /Y /S /Q /C /I
xcopy ".\DTP_dll\DPeMClientRpcHelper.*" ".\dll" /Y /S /Q /C /I
xcopy ".\DTP_dll\DPeMPublic.Common.*" ".\dll" /Y /S /Q /C /I
echo errorlevel is %errorlevel%
echo *copy finish*
:_copyDPeMSDK
Echo --------------------------------------------
Echo [Build SA]
Echo --------------------------------------------
cd /d "%RootDir%\DDPM.SA"
dotnet.exe clean -c %build_type% -v minimal /p:Framework="net8.0" /p:platform=%build_arch% /p:EnableWindowsTargeting=true ".\DDPM.SA.sln"
echo errorlevel is %errorlevel%
if not %errorlevel% == 0 goto SA_CleanFail
dotnet.exe build -c %build_type% -v minimal /p:Framework="net8.0" /p:platform=%build_arch% /p:EnableWindowsTargeting=true /p:DebugSymbols=false /p:DebugType=None ".\DDPM.SA.sln"
echo errorlevel is %errorlevel%
if not %errorlevel% == 0 goto SA_Fail
xcopy ".\dll\LSTDDPM" ".\bin\DDPM.Subagent.User\%build_type%\net8.0-windows10.0.19041.0" /Y
Echo --------------------------------------------------
Echo [Copy common Dlls to DDPM.UI project to reference]
Echo --------------------------------------------------
cd /d "%RootDir%"
xcopy "%Dir_Subagent_CommonDll%\*.*" "%Dir_UI_CommonDll%\" /Y /S /Q
echo errorlevel is %errorlevel%
if not %errorlevel% == 0 goto SA_CopyFail
Echo [Copy common Dlls to DdpmSwUpdater project to reference]
cd /d "%RootDir%"
xcopy "%Dir_Subagent_CommonDll%\DDPM.SA.Common.*" "%RootDir%\DdpmSwUpdater\CommonDll\" /Y /S /Q
echo errorlevel is %errorlevel%
if not %errorlevel% == 0 goto SA_CopyFail
xcopy "%Dir_Subagent_CommonDll%\VcpCore.Common.*" "%RootDir%\DdpmSwUpdater\CommonDll\" /Y /S /Q
echo errorlevel is %errorlevel%
if not %errorlevel% == 0 goto SA_CopyFail
::---
if "%option_cmd%"=="debug_sa" goto _ONLY_SA
Echo --------------------------------------------
Echo [Build DdpmSwUpdater project]
Echo --------------------------------------------
cd /d "%RootDir%"
cd DdpmSwUpdater
dotnet.exe clean -c %build_type% -v minimal /p:Framework="net8.0" /p:platform=%build_arch% /p:EnableWindowsTargeting=true ".\DdpmSwUpdater.sln"
echo errorlevel is %errorlevel%
if not %errorlevel% == 0 goto DdpmSwUpdater_CleanFail
dotnet.exe build -c %build_type% -v minimal /p:Framework="net8.0" /p:platform=%build_arch% /p:EnableWindowsTargeting=true /p:DebugSymbols=false /p:DebugType=None ".\DdpmSwUpdater.sln"
echo errorlevel is %errorlevel%
if not %errorlevel% == 0 goto DdpmSwUpdater_Fail
:_ONLY_UI
Echo --------------------------------------------
Echo [Build DDPM.UI project]
Echo --------------------------------------------
cd /d "%RootDir%"
cd DDPM.UI
dotnet.exe clean -c %build_type% -v minimal /p:Framework="net8.0" /p:platform=%build_arch% /p:EnableWindowsTargeting=true ".\DDPM.UI.sln"
echo errorlevel is %errorlevel%
if not %errorlevel% == 0 goto UI_CleanFail
dotnet.exe build -c %build_type% -v minimal /p:Framework="net8.0" /p:platform=%build_arch% /p:EnableWindowsTargeting=true /p:DebugSymbols=false /p:DebugType=None ".\DDPM.UI.sln"
echo errorlevel is %errorlevel%
if not %errorlevel% == 0 goto UI_Fail
::-----------------
:_ONLY_SA
::IF "%build_type%"=="Debug" (
::	echo "*** Debug build, skip [Copy to Installer] step ***"
::	goto _skipCopyToInstaller
::)
Echo --------------------------------------------
    @echo.
    @echo   #####  #     #  #####   #####  #######  #####   #####  
    @echo  #     # #     # #     # #     # #       #     # #     # 
    @echo  #       #     # #       #       #       #       #       
    @echo   #####  #     # #       #       #####    #####   #####  
    @echo        # #     # #       #       #             #       # 
    @echo  #     # #     # #     # #     # #       #     # #     # 
    @echo   #####   #####   #####   #####  #######  #####   #####  
    @echo.
Echo --------------------------------------------
echo Press any key to process file collection into [Installer\BIN]
pause
Read-Host
Echo -----------------------------------------------------------
Echo [Copy all necessary files to InstallShield project folder]
Echo -----------------------------------------------------------
Echo [Re-create target folders]
cd /d "%RootDir%\Installer"
mkdir BIN
cd BIN
mkdir SA
mkdir UI
mkdir VCPSDK
mkdir DTP
::mkdir ICON
mkdir MINI
::mkdir CER
cd SA
mkdir CLI
mkdir User
mkdir System
::--------
Echo [Copy UI/SA files into Installer project]
cd /d %RootDir%
echo [copy SA - CLI]
xcopy "%Dir_Subagent_cli%\*.*" "%Dir_Ins_cli%\" /Y /S /Q /C /I
echo errorlevel is %errorlevel%
if not %errorlevel% == 0 goto Installer_CopyFail
echo [copy SA - user subagent]
xcopy "%Dir_Subagent_user%\*.*" "%Dir_Ins_user%\" /Y /S /Q /C /I
echo errorlevel is %errorlevel%
if not %errorlevel% == 0 goto Installer_CopyFail
echo [copy SA - sys subagent]
xcopy "%Dir_Subagent_sys%\*.*" "%Dir_Ins_sys%\" /Y /S /Q /C /I
echo errorlevel is %errorlevel%
if not %errorlevel% == 0 goto Installer_CopyFail
echo [copy UI]
xcopy "%Dir_UI_output%\*.*" "%Dir_Ins_ui%\" /Y /S /Q /C /I
echo errorlevel is %errorlevel%
if not %errorlevel% == 0 goto Installer_CopyFail
::--------
echo [copy support list to user SA folder]
cd /d "%RootDir%"
xcopy ".\DDPM.SA\dll\LSTDDPM" ".\Installer\BIN\SA\User" /Y
echo errorlevel is %errorlevel%
if not %errorlevel% == 0 goto Installer_CopyFail
echo *** supported list copied
::--------
cd /d "%RootDir%"
echo [copy VCPSDK]
xcopy ".\DDPM.SA\VCPSDK\VCPSDK\bin\%build_type%\net8.0-windows10.0.19041.0\*.*" ".\Installer\BIN\VCPSDK" /Y /S /Q /C /I
echo errorlevel is %errorlevel%
if not %errorlevel% == 0 goto Installer_CopyFail
echo *** VCPSDK copied
::--------
cd /d "%RootDir%"
echo [copy DdpmSwUpdater]
xcopy ".\DdpmSwUpdater\bin\%build_type%\net8.0-windows10.0.19041.0\*.*" ".\Installer\BIN\MINI" /Y /S /Q /C /I
echo errorlevel is %errorlevel%
if not %errorlevel% == 0 goto Installer_CopyFail
echo *** DDPM SW updater copied
::-----------------
:: If DTP decoupling not exist, skip copy.
if not exist "%RootDir%\DTP.Decoupling" (
	echo "*** There is no DTP.Decoupling folder exist, skip [DTP] copy step ***"
	goto _skipDTP2
)
cd /d "%RootDir%"
echo [copy DTP]
xcopy ".\DTP.Decoupling\_BIN\*.*" ".\Installer\BIN\DTP" /Y /S /Q /C /I
echo errorlevel is %errorlevel%
if not %errorlevel% == 0 goto Installer_CopyFail
echo *** DTP copied
:_skipDTP2
Echo -------------------------------------------
echo [remove pdb files]
Echo -------------------------------------------
cd /d "%RootDir%\Installer"
for /r ".\BIN" %%f in (*.pdb) do (
    echo Deleting %%f
    del "%%f"
)
echo *** All .pdb files have been deleted
::----------
Echo -------------------------------------------
echo [remove Obfuscar configs]
Echo -------------------------------------------
cd /d "%RootDir%\Installer"
for /r ".\BIN" %%f in (Obfuscar*.xml) do (
    echo Deleting %%f
    del "%%f"
)
echo *** All copied Obfuscar configs have been deleted
::----------
:_skipCopyToInstaller
goto End

:DDM_decrypt_Fail
Echo --------------------------------------------
Echo Build DTP fail with code %errorlevel%
goto fail_print

:DDM_decrypt_CleanFail
Echo --------------------------------------------
Echo Clean DTP fail with code %errorlevel%
goto fail_print

:DTP_Fail
Echo --------------------------------------------
Echo Build DTP fail with code %errorlevel%
goto fail_print

:DTP_CleanFail
Echo --------------------------------------------
Echo Clean DTP fail with code %errorlevel%
goto fail_print

:DTP_CopyDllFail
Echo --------------------------------------------
Echo Copy Necessary Dlls from DTP fail with code %errorlevel%
goto fail_print

:VCPSDK_CopyDllFail
Echo --------------------------------------------
Echo Copy Necessary Dlls from VCPSDK fail with code %errorlevel%
goto fail_print

:VCPSDK_Fail
Echo --------------------------------------------
Echo Build VCPSDK fail with code %errorlevel%
goto fail_print

:VCPSDK_CleanFail
Echo --------------------------------------------
Echo Clean VCPSDK fail with code %errorlevel%
goto fail_print

:EA_CopyDllFail
Echo --------------------------------------------
Echo Copy Necessary Dlls from EA to SA fail with code %errorlevel%
goto fail_print

:EA_Fail
Echo --------------------------------------------
Echo Build EA fail with code %errorlevel%
goto fail_print

:EA_CleanFail
Echo --------------------------------------------
Echo Clean EA fail with code %errorlevel%
goto fail_print

:SA_CopyDllFail
Echo --------------------------------------------
Echo Copy Necessary Dlls from DPeM commodity to DDPM.SA fail with code %errorlevel%
goto fail_print

:SA_Fail
Echo --------------------------------------------
Echo Build DDPM.SA fail with code %errorlevel%
goto fail_print

:SA_CleanFail
Echo --------------------------------------------
Echo Clean DDPM.SA fail with code %errorlevel%
goto fail_print

:SA_CopyFail
Echo --------------------------------------------
Echo Copy Common Dll from DDPM.SA to DDPM.UI fail with code %errorlevel%
goto fail_print

:UI_Fail
Echo --------------------------------------------
Echo Build DDPM.UI fail with code %errorlevel%
goto fail_print

:UI_CleanFail
Echo --------------------------------------------
Echo Clean DDPM.UI fail with code %errorlevel%
goto fail_print

:DdpmSwUpdater_Fail
Echo --------------------------------------------
Echo Build DdpmSwUpdater fail with code %errorlevel%
goto fail_print

:DdpmSwUpdater_CleanFail
Echo --------------------------------------------
Echo Clean DdpmSwUpdater fail with code %errorlevel%
goto fail_print

:Installer_CopyFail
Echo --------------------------------------------
Echo Copy DDPM files to Installer folder and failed with code %errorlevel%
goto fail_print

:Installer_Fail
Echo --------------------------------------------
Echo Build Installer fail with code %errorlevel%
goto fail_print

:Retrieve_cert_fail
Echo --------------------------------------------
Echo Retrieve Dell CICD digital cert fail with code %errorlevel%
goto fail_print

:copy_cert_cs_fail
Echo --------------------------------------------
Echo Copy thumbprint cs file to DDPM.SA fail with code %errorlevel%
goto fail_print

:copy_cert_file_fail
Echo --------------------------------------------
Echo Copy thumbprint cs file to DDPM.SA fail with code %errorlevel%
goto fail_print

:copy_ddm_lib_fail
Echo --------------------------------------------
Echo Copy DDM decryption lib to DDPM.SA fail with code %errorlevel%
goto fail_print

:copy_nkvm_cs_fail
Echo --------------------------------------------
Echo Copy DDM thumbprint definition to DDPM.SA fail with code %errorlevel%
goto fail_print

:fail_print
Echo --------------------------------------------
    @echo.
    @echo   #####   ##   ##### ##   
    @echo   #      #  #    #   #    
    @echo   #     #    #   #   #    
    @echo   ##### ######   #   #    
    @echo   #     #    #   #   #    
    @echo   #     #    #   #   #   # 
    @echo   #     #    # ##### #####
    @echo.
Echo --------------------------------------------
Exit /b %ERRORLEVEL%

:End
Echo --------------------------------------------
    @echo.
    @echo   #####  #     #  #####   #####  #######  #####   #####  
    @echo  #     # #     # #     # #     # #       #     # #     # 
    @echo  #       #     # #       #       #       #       #       
    @echo   #####  #     # #       #       #####    #####   #####  
    @echo        # #     # #       #       #             #       # 
    @echo  #     # #     # #     # #     # #       #     # #     # 
    @echo   #####   #####   #####   #####  #######  #####   #####  
    @echo.
Echo --------------------------------------------
cd /d "%RootDir%
Exit /b 0

:_HELPER
echo Batch usage:
echo   First param:
echo     1. Empty means normal release build and copy all necessary data to Installer\BIN
echo     2. "clear", clear all bin/obj under root dir
echo     3. "Debug", Debug build for RD verify and runtime debug 
echo        (DDPM.SA/DDPM.UI/DdpmSwUpdater, and if folder DTP.Decoupling exist then build it)
echo -
echo Example:
echo   [Fully Release build with installer]
echo     Command: DDPM_Build_Script.bat
echo   [Just clear all bin and obj under project folder]
echo     Command: DDPM_Build_Script.bat clear
echo   [Help RD to copy all related data to right position in Debug build]
echo     Command: DDPM_Build_Script.bat Debug
echo   [Help RD to compile UI solution only for runtime debug]
echo     Command: DDPM_Build_Script.bat Debug_UI
echo   [Help RD to compile SA solution only for runtime debug]
echo     Command: DDPM_Build_Script.bat Debug_SA
Exit /b 1
