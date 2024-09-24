:: Generate_AddDDPMKeyToReg.bat v4 by Elie_Liao
:: -------------------------------------------------
:: This is used to generate a Regkey for configuration files. 
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
echo [HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{21A24609-08A2-423E-80DE-4D33A933F1A1}]>>%OutFile%
echo "DisplayName"="Dell Display and Peripheral Manager">>%OutFile%
echo "DisplayVersion"="2.0.0.40">>%OutFile%
echo "InstallLocation"="C:\\Program Files\\Dell\\Dell Display and Peripheral Manager">>%OutFile%
echo.>>%OutFile%




