Get-ChildItem .\ -include bin,obj -Recurse | ForEach-Object ($_) { Remove-Item $_.FullName -Force -Recurse }

Start-Process -FilePath "Del_Unnecessities.bat" -Wait
# Start-Process -FilePath "build.bat" Debug -Wait