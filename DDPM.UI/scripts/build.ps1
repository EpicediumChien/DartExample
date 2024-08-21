Get-Host
Get-Date -Format "dddd MM/dd/yyyy HH:mm K"
$Stage="build"
$ProjRoot= Get-Location
Write-Host "##### This's ${Stage} stage. #####"
Write-Host "Location is [$ProjRoot]"
#dotnet restore -s ..\packages "..\DDPM.SA.sln"
dotnet.exe restore --source C:\Users\DeanYang\.nuget\packages ".\DDPM.UI.sln"
dotnet.exe build --no-restore -c Release /p:platform="Any CPU" ".\DDPM.UI.sln"
#Write-Host $(Get-Location)
# do file copy from built folder to target dist, then copy make file to packing folder
#Remove-Item -Recurse -Force D:\WiX_dist\
#New-Item -Path "D:\" -Name "WiX_dist" -ItemType Directory
#New-Item -Path "D:\WiX_dist\" -Name "dist" -ItemType Directory
#
#Write-Host "ProjRoot is $ProjRoot"
#$gWxsFilePath = Join-Path -Path $ProjRoot -ChildPath "\AwccDisplayApp\bin\x64\Release\net6.0-windows10.0.22621.0\*"
#Write-Host "gWxsFilePath is $gWxsFilePath"
#Copy-Item -Path $gWxsFilePath -Destination D:\WiX_dist\dist\ -Recurse
#
#$gWxsFilePath = Join-Path -Path $ProjRoot -ChildPath "\scripts\HeatTransform.xslt"
#Write-Host "gWxsFilePath is $gWxsFilePath"
#Copy-Item -Path "$gWxsFilePath" -Destination D:\WiX_dist\
#
#$gWxsFilePath = Join-Path -Path $ProjRoot -ChildPath "\scripts\deploy_makefile.ps1"
#Write-Host "gWxsFilePath is $gWxsFilePath"
#Copy-Item -Path "$gWxsFilePath" -Destination D:\WiX_dist\
