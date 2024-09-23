:: GenerateReg.bat v4 by Elie_Liao
:: -------------------------------------------------
:: This batch program will generate DDPMSA.reg file base on current directory.
:: You should run this batch program when you change the base dir of the source code.
:: And then import the "DDPMSA.reg" into your Windows Registry.
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

SET OutFile=AddDDPMKeyToReg.reg

echo Windows Registry Editor Version 5.00 >%OutFile%
echo.>>%OutFile%
:: This section is used to generate secret key and version info, test purpose
echo [HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{21A24609-08A2-423E-80DE-4D33A933F1A1}]>>%OutFile%
echo "DisplayName"="Dell Display and Peripheral Manager">>%OutFile%
echo "DisplayVersion"="2.0.0.40">>%OutFile%
echo "InstallLocation"="C:\\Program Files\\Dell\\Dell Display and Peripheral Manager">>%OutFile%
echo.>>%OutFile%




