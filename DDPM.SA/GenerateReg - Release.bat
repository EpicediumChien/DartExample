:: GenerateReg.bat v2 by Robert_Lin
:: -------------------------------------------------
:: This batch program will generate DDPMSA.reg file base on current directory.
:: You should run this batch program when you change the base dir of the source code.
:: And then import the "DDPMSA.reg" into your Windows Registry.
:: [v2] 2024-5-23 Robert_Lin
:: Add PipPbpManager Plugin Registery keys
:: -
SET CurDir=%CD%
REM - Replace "\" with "\\"
SET BaseDir=C:\\Program Files\\Dell\\Dell Display and Peripheral Manager\\Plugins

echo BaseDir=%BaseDir%
::pause

SET OutFile=DDPMSA.Release.reg

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
echo "ExecutablePath"="%BaseDir%\\DDPM.Subagent\\DDPM.Subagent.exe">>%OutFile%
echo "RecoveryAction"=dword:00000002>>%OutFile%
echo "ExecutionContext"=dword:00000001>>%OutFile%
echo "StartupGroup"=dword:00000001>>%OutFile%
echo "StartupType"=dword:00000001>>%OutFile%
echo "MaxRecovery"=dword:00000003>>%OutFile%
echo.>>%OutFile%
echo [HKEY_LOCAL_MACHINE\SOFTWARE\Dell\Dell TechHub\AgentRegistration\{2E365D80-A333-42E9-B855-F326DFEF2C34}]>>%OutFile%
echo "ExecutablePath"="%BaseDir%\\DDPM.Subagent.User\\DDPM.Subagent.User.exe">>%OutFile%
echo "RecoveryAction"=dword:00000002>>%OutFile%
echo "ExecutionContext"=dword:00000002>>%OutFile%
echo "StartupGroup"=dword:00000001>>%OutFile%
echo "StartupType"=dword:00000001>>%OutFile%
echo "MaxRecovery"=dword:00000003>>%OutFile%
echo.>>%OutFile% 
echo [HKEY_LOCAL_MACHINE\SOFTWARE\Dell\Dell TechHub\Parameters]>>%OutFile%
echo.>>%OutFile% 
echo [HKEY_LOCAL_MACHINE\SOFTWARE\Dell\Dell TechHub\RPMRegistration]>>%OutFile%
echo.>>%OutFile% 
echo [HKEY_LOCAL_MACHINE\SOFTWARE\Dell\Dell TechHub\RPMRegistration\{9bbe5845-8c58-45b3-baea-c2c061cd8465}]>>%OutFile%
echo "ExecutablePath"="%BaseDir%\\DDPM.Subagent\\DDPM.Subagent.exe">>%OutFile%
echo.>>%OutFile% 
echo [HKEY_LOCAL_MACHINE\SOFTWARE\Dell\Dell TechHub\RPMRegistration\{9bbe5845-8c58-45b3-baea-c2c061cd8465}\PublishingAssemblies]>>%OutFile%
echo.>>%OutFile% 
echo [HKEY_LOCAL_MACHINE\SOFTWARE\Dell\Dell TechHub\RPMRegistration\{9bbe5845-8c58-45b3-baea-c2c061cd8465}\PublishingAssemblies]>>%OutFile%
echo "{F6909D0F-C70B-4B84-8EC3-9E506550A998}"="%BaseDir%\\DDPM.Subagent\\DDPM.SA.Plugins.SettingsManager.dll">>%OutFile%
echo "{2B76DC4B-39E7-4DBE-946E-5112CDAF37EA}"="%BaseDir%\\DDPM.Subagent\\DDPM.SA.Plugins.CLIManager.dll">>%OutFile%
echo "{F716E8C1-1F8D-4BC6-83DA-51CD26031335}"="%BaseDir%\\DDPM.Subagent\\DDPM.SA.Plugins.SWUpdate.dll">>%OutFile%
echo "{7D53B92E-5648-4ADB-9E33-4C74FFE7BE8C}"="%BaseDir%\\DDPM.Subagent\\DDPM.SA.Plugins.FWUpdate.dll">>%OutFile%
echo "{BFAA77E8-CADF-4CE4-9473-363E65C6B4E0}"="%BaseDir%\\DDPM.Subagent\\DDPM.SA.Plugins.PlatinumSDK.dll">>%OutFile%
echo.>>%OutFile% 
echo [HKEY_LOCAL_MACHINE\SOFTWARE\Dell\Dell TechHub\RPMRegistration\{2E365D80-A333-42E9-B855-F326DFEF2C34}]>>%OutFile%
echo "ExecutablePath"="%BaseDir%\\DDPM.Subagent.User\\DDPM.Subagent.User.exe">>%OutFile%
echo.>>%OutFile% 
echo [HKEY_LOCAL_MACHINE\SOFTWARE\Dell\Dell TechHub\RPMRegistration\{2E365D80-A333-42E9-B855-F326DFEF2C34}\PublishingAssemblies]>>%OutFile%
echo.>>%OutFile% 
echo [HKEY_LOCAL_MACHINE\SOFTWARE\Dell\Dell TechHub\RPMRegistration\{2E365D80-A333-42E9-B855-F326DFEF2C34}\PublishingAssemblies]>>%OutFile%
echo "{A409E0AF-E2C3-4568-A194-B2D173DA26D4}"="%BaseDir%\\DDPM.Subagent.User\\VcpCore.Plugins.dll">>%OutFile%
echo "{39A9CF54-2EC0-434E-A0BF-49FF43F8C824}"="%BaseDir%\\DDPM.Subagent.User\\DDPM.SA.Plugins.User.DisplayManager.dll">>%OutFile%
echo "{9829A9C5-E129-488A-A522-B8FF705051EE}"="%BaseDir%\\DDPM.Subagent.User\\DDPM.SA.Plugins.User.DeviceManager.dll">>%OutFile%
echo "{CF223214-FAF4-4595-8ED4-C6C1F65FA02C}"="%BaseDir%\\DDPM.Subagent.User\\DDPM.SA.Plugins.User.Peripherals.dll">>%OutFile%
echo "{67A0D126-10BE-4EDB-95DC-8A4162AA3F6B}"="%BaseDir%\\DDPM.Subagent.User\\DDPM.SA.Plugins.User.DisplayProperties.dll">>%OutFile%
echo "{AC2BD6A8-0678-482A-8274-3B8E80E12A81}"="%BaseDir%\\DDPM.Subagent.User\\DDPM.SA.Plugins.User.ColorPreset.dll">>%OutFile%
echo "{AFF8831F-5BEB-49F0-9098-DD59D313CB28}"="%BaseDir%\\DDPM.Subagent.User\\DDPM.SA.Plugins.User.PipPbpManager.dll">>%OutFile%
echo "{633ED971-086A-49E7-91E1-F6EB6E15CC13}"="%BaseDir%\\DDPM.Subagent.User\\DDPM.SA.Plugins.User.USBKVM.dll">>%OutFile%
echo "{149EF7F9-BF22-4E00-86B1-44CAC25CCA7E}"="%BaseDir%\\DDPM.Subagent.User\\DDPM.SA.Plugins.User.NetworkKVM.dll">>%OutFile%
echo "{C01C5C25-B7F8-4F08-8CD8-4E16928CC254}"="%BaseDir%\\DDPM.Subagent.User\\DDPM.SA.Plugins.User.Hotkey.dll">>%OutFile%
echo "{998CE5F9-19FE-4EDF-841A-2497F39623BF}"="%BaseDir%\\DDPM.Subagent.User\\DDPM.SA.Plugins.User.EasyArrange.dll">>%OutFile%
echo "{99DB213B-220B-41C7-8B3A-0131CA7A4DD1}"="%BaseDir%\\DDPM.Subagent.User\\DDPM.SA.Plugins.User.SettingsManager.dll">>%OutFile%
echo "{EEE0AA91-11EE-46BD-B1FD-88D590C3052C}"="%BaseDir%\\DDPM.Subagent.User\\DDPM.SA.Plugins.User.SchedulerManager.dll">>%OutFile%
echo "{E993A925-BAF0-42DF-B878-7EC075D5B941}"="%BaseDir%\\DDPM.Subagent.User\\CLI.Plugins.Display.dll">>%OutFile%
echo "{FE361209-1BEB-4B52-AC1A-D0227B7641CB}"="%BaseDir%\\DDPM.Subagent.User\\CLI.Plugins.Peripherals.dll">>%OutFile%
echo "{E5DA6004-21DC-4058-917F-A0ECF838EA03}"="%BaseDir%\\DDPM.Subagent.User\\DDPM.SA.Plugins.User.ActionsManger.dll">>%OutFile%
echo "{D034EE8F-7C8A-4296-8B5B-33B4E978C6B5}"="%BaseDir%\\DDPM.Subagent.User\\DDPM.SA.Plugins.User.DTPProxy.dll">>%OutFile%
echo.>>%OutFile%




