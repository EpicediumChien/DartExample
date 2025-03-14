:: GenerateReg.bat v4 by Elie_Liao
:: -------------------------------------------------
:: This batch program will generate DDPMSA.reg file base on current directory.
:: You should run this batch program when you change the base dir of the source code.
:: And then import the "DDPMSA.reg" into your Windows Registry.
:: [v5] 2025-2-17 Dean_Yang
:: Add DTP registration to DTP instrumentation SA
:: Modify startup group of User SubAgent to 3 (Standard Subagent)
:: [v4] 2024-8-28 Elie_Liao
:: fixed wrong name (DDPM.SA.Plugins.User.ActionsManger.)
:: Add DDPM.SA.Plugins.User.DTPProxy.dll to SubAgent.Uer
:: [v3] 2024-6-27 Robert_Lin
:: Add EasyArrangeService plugin to SubAgent.User registry
:: [v2] 2024-5-23 Robert_Lin
:: Add PipPbpManager Plugin Registery keys
:: -
SET CurDir=%CD%
REM - Replace "\" with "\\"
SET BaseDir=%CurDir:\=\\%

echo BaseDir="%BaseDir%"
::pause

SET OutFile=DDPMSA.reg

echo Windows Registry Editor Version 5.00 >%OutFile%
echo.>>%OutFile%
echo [HKEY_LOCAL_MACHINE\SOFTWARE\Dell]>>%OutFile%
echo.>>%OutFile%
echo [HKEY_LOCAL_MACHINE\SOFTWARE\Dell\Dell TechHub]>>%OutFile%
echo.>>%OutFile%
echo [HKEY_LOCAL_MACHINE\SOFTWARE\Dell\Dell TechHub\AgentPlugins]>>%OutFile%
echo.>>%OutFile%
echo [HKEY_LOCAL_MACHINE\SOFTWARE\Dell\Dell TechHub\AgentRegistration]>>%OutFile%
echo.>>%OutFile%
echo [HKEY_LOCAL_MACHINE\SOFTWARE\Dell\Dell TechHub\AgentRegistration\{9bbe5845-8c58-45b3-baea-c2c061cd8465}]>>%OutFile%
echo "ExecutablePath"="%BaseDir%\\bin\\DDPM.Subagent\\Debug\\net8.0-windows10.0.19041.0\\DDPM.Subagent.exe">>%OutFile%
echo "RecoveryAction"=dword:00000002>>%OutFile%
echo "ExecutionContext"=dword:00000001>>%OutFile%
echo "StartupGroup"=dword:00000001>>%OutFile%
echo "StartupType"=dword:00000002>>%OutFile%
echo "MaxRecovery"=dword:00000003>>%OutFile%
echo.>>%OutFile%
echo [HKEY_LOCAL_MACHINE\SOFTWARE\Dell\Dell TechHub\AgentRegistration\{2E365D80-A333-42E9-B855-F326DFEF2C34}]>>%OutFile%
echo "ExecutablePath"="%BaseDir%\\bin\\DDPM.Subagent.User\\Debug\\net8.0-windows10.0.19041.0\\DDPM.Subagent.User.exe">>%OutFile%
echo "RecoveryAction"=dword:00000002>>%OutFile%
echo "ExecutionContext"=dword:00000002>>%OutFile%
::Startup group should be 3 (Standard Subagent) per DTH expert's suggestion
echo "StartupGroup"=dword:00000003>>%OutFile%
echo "StartupType"=dword:00000002>>%OutFile%
echo "MaxRecovery"=dword:00000003>>%OutFile%
echo.>>%OutFile% 
echo [HKEY_LOCAL_MACHINE\SOFTWARE\Dell\Dell TechHub\Parameters]>>%OutFile%
echo.>>%OutFile% 
echo [HKEY_LOCAL_MACHINE\SOFTWARE\Dell\Dell TechHub\RPMRegistration]>>%OutFile%
echo.>>%OutFile% 
echo [HKEY_LOCAL_MACHINE\SOFTWARE\Dell\Dell TechHub\RPMRegistration\{9bbe5845-8c58-45b3-baea-c2c061cd8465}]>>%OutFile%
echo "ExecutablePath"="%BaseDir%\\bin\\DDPM.Subagent\\Debug\\net8.0-windows10.0.19041.0\\DDPM.Subagent.exe">>%OutFile%
echo.>>%OutFile% 
echo [HKEY_LOCAL_MACHINE\SOFTWARE\Dell\Dell TechHub\RPMRegistration\{9bbe5845-8c58-45b3-baea-c2c061cd8465}\PublishingAssemblies]>>%OutFile%
echo.>>%OutFile% 
echo [HKEY_LOCAL_MACHINE\SOFTWARE\Dell\Dell TechHub\RPMRegistration\{9bbe5845-8c58-45b3-baea-c2c061cd8465}\PublishingAssemblies]>>%OutFile%
echo "{BFAA77E8-CADF-4CE4-9473-363E65C6B4E0}"="%BaseDir%\\bin\\DDPM.Subagent\\Debug\\net8.0-windows10.0.19041.0\\DDPM.SA.Plugins.PlatinumSDK.dll">>%OutFile%
echo "{F6909D0F-C70B-4B84-8EC3-9E506550A998}"="%BaseDir%\\bin\\DDPM.Subagent\\Debug\\net8.0-windows10.0.19041.0\\DDPM.SA.Plugins.SettingsManager.dll">>%OutFile%
echo "{2B76DC4B-39E7-4DBE-946E-5112CDAF37EA}"="%BaseDir%\\bin\\DDPM.Subagent\\Debug\\net8.0-windows10.0.19041.0\\DDPM.SA.Plugins.CLIManager.dll">>%OutFile%
echo "{F716E8C1-1F8D-4BC6-83DA-51CD26031335}"="%BaseDir%\\bin\\DDPM.Subagent\\Debug\\net8.0-windows10.0.19041.0\\DDPM.SA.Plugins.SWUpdate.dll">>%OutFile%
echo "{7D53B92E-5648-4ADB-9E33-4C74FFE7BE8C}"="%BaseDir%\\bin\\DDPM.Subagent\\Debug\\net8.0-windows10.0.19041.0\\DDPM.SA.Plugins.FWUpdate.dll">>%OutFile%
echo "{A9C07BA5-6499-4730-B5E1-3143F6A9409F}"="%BaseDir%\\bin\\DDPM.Subagent\\Debug\\net8.0-windows10.0.19041.0\\DDPM.SA.Plugins.CMAManager.dll">>%OutFile%
echo.>>%OutFile% 
echo [HKEY_LOCAL_MACHINE\SOFTWARE\Dell\Dell TechHub\RPMRegistration\{2E365D80-A333-42E9-B855-F326DFEF2C34}]>>%OutFile%
echo "ExecutablePath"="%BaseDir%\\bin\\DDPM.Subagent.User\\Debug\\net8.0-windows10.0.19041.0\\DDPM.Subagent.User.exe">>%OutFile%
echo.>>%OutFile% 
echo [HKEY_LOCAL_MACHINE\SOFTWARE\Dell\Dell TechHub\RPMRegistration\{2E365D80-A333-42E9-B855-F326DFEF2C34}\PublishingAssemblies]>>%OutFile%
echo.>>%OutFile% 
echo [HKEY_LOCAL_MACHINE\SOFTWARE\Dell\Dell TechHub\RPMRegistration\{2E365D80-A333-42E9-B855-F326DFEF2C34}\PublishingAssemblies]>>%OutFile%
echo "{A409E0AF-E2C3-4568-A194-B2D173DA26D4}"="%BaseDir%\\bin\\DDPM.Subagent.User\\Debug\\net8.0-windows10.0.19041.0\\VcpCore.Plugins.dll">>%OutFile%
echo "{EEB96C41-01ED-48DE-A7C7-B01E7A081CAD}"="%BaseDir%\\bin\\DDPM.Subagent.User\\Debug\\net8.0-windows10.0.19041.0\\DDPM.SA.Plugins.User.TelementryScheduler.dll">>%OutFile%
echo "{39A9CF54-2EC0-434E-A0BF-49FF43F8C824}"="%BaseDir%\\bin\\DDPM.Subagent.User\\Debug\\net8.0-windows10.0.19041.0\\DDPM.SA.Plugins.User.DisplayManager.dll">>%OutFile%
echo "{9829A9C5-E129-488A-A522-B8FF705051EE}"="%BaseDir%\\bin\\DDPM.Subagent.User\\Debug\\net8.0-windows10.0.19041.0\\DDPM.SA.Plugins.User.DeviceManager.dll">>%OutFile%
echo "{CF223214-FAF4-4595-8ED4-C6C1F65FA02C}"="%BaseDir%\\bin\\DDPM.Subagent.User\\Debug\\net8.0-windows10.0.19041.0\\DDPM.SA.Plugins.User.Peripherals.dll">>%OutFile%
echo "{67A0D126-10BE-4EDB-95DC-8A4162AA3F6B}"="%BaseDir%\\bin\\DDPM.Subagent.User\\Debug\\net8.0-windows10.0.19041.0\\DDPM.SA.Plugins.User.DisplayProperties.dll">>%OutFile%
echo "{AC2BD6A8-0678-482A-8274-3B8E80E12A81}"="%BaseDir%\\bin\\DDPM.Subagent.User\\Debug\\net8.0-windows10.0.19041.0\\DDPM.SA.Plugins.User.ColorPreset.dll">>%OutFile%
echo "{AFF8831F-5BEB-49F0-9098-DD59D313CB28}"="%BaseDir%\\bin\\DDPM.Subagent.User\\Debug\\net8.0-windows10.0.19041.0\\DDPM.SA.Plugins.User.PipPbpManager.dll">>%OutFile%
echo "{633ED971-086A-49E7-91E1-F6EB6E15CC13}"="%BaseDir%\\bin\\DDPM.Subagent.User\\Debug\\net8.0-windows10.0.19041.0\\DDPM.SA.Plugins.User.USBKVM.dll">>%OutFile%
echo "{149EF7F9-BF22-4E00-86B1-44CAC25CCA7E}"="%BaseDir%\\bin\\DDPM.Subagent.User\\Debug\\net8.0-windows10.0.19041.0\\DDPM.SA.Plugins.User.NetworkKVM.dll">>%OutFile%
echo "{C01C5C25-B7F8-4F08-8CD8-4E16928CC254}"="%BaseDir%\\bin\\DDPM.Subagent.User\\Debug\\net8.0-windows10.0.19041.0\\DDPM.SA.Plugins.User.Hotkey.dll">>%OutFile%
echo "{998CE5F9-19FE-4EDF-841A-2497F39623BF}"="%BaseDir%\\bin\\DDPM.Subagent.User\\Debug\\net8.0-windows10.0.19041.0\\DDPM.SA.Plugins.User.EasyArrange.dll">>%OutFile%
echo "{99DB213B-220B-41C7-8B3A-0131CA7A4DD1}"="%BaseDir%\\bin\\DDPM.Subagent.User\\Debug\\net8.0-windows10.0.19041.0\\DDPM.SA.Plugins.User.SettingsManager.dll">>%OutFile%
echo "{EEE0AA91-11EE-46BD-B1FD-88D590C3052C}"="%BaseDir%\\bin\\DDPM.Subagent.User\\Debug\\net8.0-windows10.0.19041.0\\DDPM.SA.Plugins.User.SchedulerManager.dll">>%OutFile%
echo "{E993A925-BAF0-42DF-B878-7EC075D5B941}"="%BaseDir%\\bin\\DDPM.Subagent.User\\Debug\\net8.0-windows10.0.19041.0\\CLI.Plugins.Display.dll">>%OutFile%
echo "{FE361209-1BEB-4B52-AC1A-D0227B7641CB}"="%BaseDir%\\bin\\DDPM.Subagent.User\\Debug\\net8.0-windows10.0.19041.0\\CLI.Plugins.Peripherals.dll">>%OutFile%
echo "{D034EE8F-7C8A-4296-8B5B-33B4E978C6B5}"="%BaseDir%\\bin\\DDPM.Subagent.User\\Debug\\net8.0-windows10.0.19041.0\\DDPM.SA.Plugins.User.DTPProxy.dll">>%OutFile%
echo "{388F486D-2A86-421C-B571-3AAF9F839BA2}"="%BaseDir%\\bin\\DDPM.Subagent.User\\Debug\\net8.0-windows10.0.19041.0\\DDPM.SA.Plugins.User.EzMemory.dll">>%OutFile%
::echo "{09F670EB-3F2B-4005-9A8B-D4BF1033A425}"="%BaseDir%\\bin\\DDPM.Subagent.User\\Debug\\net8.0-windows10.0.19041.0\\DDPM.SA.Plugins.User.CMAProxy.dll">>%OutFile%
echo.>>%OutFile%
:: This section is used to generate secret key and version info, test purpose
echo [HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{21A24609-08A2-423E-80DE-4D33A933F1A1}]>>%OutFile%
echo "DisplayName"="Dell Display and Peripheral Manager">>%OutFile%
echo "DisplayVersion"="2.0.0.70">>%OutFile%
echo "InstallLocation"="C:\\Program Files\\Dell\\Dell Display and Peripheral Manager">>%OutFile%
echo.>>%OutFile%
:: ========================================================
:: 2025-2-17 Add DTP registration to DTP instrumentation SA
:: Start from DPeM core service R23
echo [HKEY_LOCAL_MACHINE\SOFTWARE\DELL\DTP.Instrumentation.SubAgent\AgentPlugins]>>%OutFile%
echo.>>%OutFile%
echo [HKEY_LOCAL_MACHINE\SOFTWARE\DELL\DTP.Instrumentation.SubAgent\AgentPlugins\{037F8DB1-6BA6-40B4-B87E-B93032163CF4}]>>%OutFile%
echo "DllPath"="%BaseDir%\\DTP_dll.202\\Dell.TechHub.Plugins.DongleCommodity.dll">>%OutFile%
echo.>>%OutFile%
echo [HKEY_LOCAL_MACHINE\SOFTWARE\DELL\DTP.Instrumentation.SubAgent\AgentPlugins\{0c0dbe70-e66c-4200-98ad-244b2e451033}]>>%OutFile%
echo "DllPath"="%BaseDir%\\DTP_dll.202\\Dell.TechHub.Plugins.DockCommodity.dll">>%OutFile%
echo.>>%OutFile%
echo [HKEY_LOCAL_MACHINE\SOFTWARE\DELL\DTP.Instrumentation.SubAgent\AgentPlugins\{5452CA9A-BE6C-4F1B-A04D-EE6730754E5F}]>>%OutFile%
echo "DllPath"="%BaseDir%\\DTP_dll.202\\Dell.TechHub.Plugins.WebcamCommodity.dll">>%OutFile%
echo.>>%OutFile%
echo [HKEY_LOCAL_MACHINE\SOFTWARE\DELL\DTP.Instrumentation.SubAgent\AgentPlugins\{86037d2f-ed7c-45fe-a5c8-b49859a6b841}]>>%OutFile%
echo "DllPath"="%BaseDir%\\DTP_dll.202\\Dell.TechHub.Plugins.HeadsetCommodity.dll">>%OutFile%
echo.>>%OutFile%
echo [HKEY_LOCAL_MACHINE\SOFTWARE\DELL\DTP.Instrumentation.SubAgent\AgentPlugins\{8B8810F3-B076-4AED-8106-1802941B330C}]>>%OutFile%
echo "DllPath"="%BaseDir%\\DTP_dll.202\\Dell.TechHub.Plugins.GlobalPeripheralCommodity.dll">>%OutFile%
echo.>>%OutFile%
echo [HKEY_LOCAL_MACHINE\SOFTWARE\DELL\DTP.Instrumentation.SubAgent\AgentPlugins\{A647FA97-3A21-485F-ADAE-4FF579CECB81}]>>%OutFile%
echo "DllPath"="%BaseDir%\\DTP_dll.202\\Dell.TechHub.Plugins.AirAudioCommodity.dll">>%OutFile%
echo.>>%OutFile%
echo [HKEY_LOCAL_MACHINE\SOFTWARE\DELL\DTP.Instrumentation.SubAgent\AgentPlugins\{a93dd192-c426-4a6d-b10b-b38b3453085f}]>>%OutFile%
echo "DllPath"="%BaseDir%\\DTP_dll.202\\Dell.TechHub.Plugins.MouseCommodity.dll">>%OutFile%
echo.>>%OutFile%
echo [HKEY_LOCAL_MACHINE\SOFTWARE\DELL\DTP.Instrumentation.SubAgent\AgentPlugins\{ab63559e-2139-49c1-aba7-73c5ddd29e4c}]>>%OutFile%
echo "DllPath"="%BaseDir%\\DTP_dll.202\\Dell.TechHub.Plugins.KeyboardCommodity.dll">>%OutFile%
echo.>>%OutFile%
echo [HKEY_LOCAL_MACHINE\SOFTWARE\DELL\DTP.Instrumentation.SubAgent\AgentPlugins\{B939D4A6-86BC-4FC0-9923-4A0507398E2B}]>>%OutFile%
echo "DllPath"="%BaseDir%\\DTP_dll.202\\Dell.TechHub.Plugins.PenCommodity.dll">>%OutFile%
echo.>>%OutFile%
echo [HKEY_LOCAL_MACHINE\SOFTWARE\DELL\DTP.Instrumentation.SubAgent\AgentPlugins\{dafb7da9-3603-432a-8554-aefd3b3fdeba}]>>%OutFile%
echo "DllPath"="%BaseDir%\\DTP_dll.202\\Dell.TechHub.Plugins.SpeakerCommodity.dll">>%OutFile%
echo.>>%OutFile%
echo [HKEY_LOCAL_MACHINE\SOFTWARE\DELL\DTP.Instrumentation.SubAgent\AgentPlugins\{E40CF224-7E8A-455B-8F61-E9FD214C82B5}]>>%OutFile%
echo "DllPath"="%BaseDir%\\DTP_dll.202\\DDPM.Peripherals.interface.dll">>%OutFile%
echo.>>%OutFile%

