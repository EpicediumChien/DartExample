@echo off
cd %~dp0
echo path: %~dp0

echo stop DellTechHub service
sc stop DellTechHub 

SET Process1=Dell.CoreServices.Client.exe
SET Process2=Dell.TechHub.Analytics.SubAgent.exe
SET Process3=Dell.UCA.Manager.exe
SET Process4=Dell.TechHub.DataManager.SubAgent.exe
SET Process5=Dell.TechHub.Instrumentation.SubAgent.exe
SET Process10=Dell.TechHub.Instrumentation.UserProcess.exe
SET Process11=Dell.Update.SubAgent.exe

SET Process6=Dell.TechHub.Peripheral.Subagent.exe
SET Process7=DDPM.Subagent.exe
SET Process8=DDPM.Subagent.User.exe
SET Process9=DDPM.exe

echo kill Dell process 
echo   %Process1%, %Process2%, 
echo   %Process3%, %Process4%, 
echo   %Process5% %Process10%
echo.
echo.

taskkill /F /im %Process1% /T >nul 2>&1
taskkill /F /im %Process2% /T >nul 2>&1
taskkill /F /im %Process3% /T >nul 2>&1
taskkill /F /im %Process4% /T >nul 2>&1
taskkill /F /im %Process5% /T >nul 2>&1
taskkill /F /im %Process10% /T >nul 2>&1
taskkill /F /im %Process11% /T >nul 2>&1


echo kill DDPM process [%Process6%, %Process7%, %Process8%, %Process9%]
taskkill /F /im %Process6% /T >nul 2>&1
taskkill /F /im %Process7% /T >nul 2>&1
taskkill /F /im %Process8% /T >nul 2>&1
taskkill /F /im %Process9% /T >nul 2>&1

taskkill /F /im %AwccProces4% /T >nul 2>&1
taskkill /F /im %AwccProces5% /T >nul 2>&1

taskkill /F /im %AwccProces1% /T >nul 2>&1
taskkill /F /im %AwccProces2% /T >nul 2>&1
taskkill /F /im %AwccProces3% /T >nul 2>&1
taskkill /F /im %AwccProces6% /T >nul 2>&1
echo.
echo.
echo.

echo =====================================================
echo Going to start DellTechHub, Press any key to continue.
echo =====================================================
pause
sc start DellTechHub
