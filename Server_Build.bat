@echo off

:Begin 
Echo 請選擇目標平台 [A]ll, [W]indows, [L]inux, [M]acOS
Set /P input=

If /I "%input%"=="A" Goto All
If /I "%input%"=="a" Goto All
If /I "%input%"=="W" Goto Windows
If /I "%input%"=="w" Goto Windows
If /I "%input%"=="L" Goto Linux 
If /I "%input%"=="l" Goto Linux 
If /I "%input%"=="M" Goto MacOS
If /I "%input%"=="m" Goto MacOS
Goto Error

:All
Echo 選擇目標為[All]
rd /q /s %~dp0\Bin
cd %cd%/Server
rd /q /s %~dp0\Publish\win-x64
dotnet publish -r win-x64 -o %~dp0/Publish/win-x64
rd /q /s %~dp0\Publish\linux-x64
dotnet publish -r linux-x64 -o %~dp0/Publish/linux-x64
rd /q /s %~dp0\Publish\osx-x64
dotnet publish -r osx-x64 -o %~dp0/Publish/osx-x64
rd /q /s %~dp0\Bin
Pause
Exit

:Windows
Echo 選擇目標為[Windows]
Echo 刪除Bin
rd /q /s %~dp0\Bin
Echo 前往Server
cd %cd%/Server
Echo %cd%
Echo 刪除Publish\win-x64
rd /q /s %~dp0\Publish\win-x64]
Echo Build
dotnet publish -r win-x64 -o %~dp0/Publish/win-x64
rd /q /s %~dp0\Bin
Pause
Exit

:Linux
Echo 選擇目標為[Linux]
rd /q /s %~dp0\Bin
cd %cd%/Server
rd /q /s %~dp0\Publish\linux-x64
dotnet publish -r linux-x64 -o %~dp0/Publish/linux-x64
rd /q /s %~dp0\Bin
Pause
Exit

:MacOS
Echo 選擇目標為[MacOS]
rd /q /s %~dp0\Bin
cd %cd%/Server
rd /q /s %~dp0\Publish\osx-x64
dotnet publish -r osx-x64 -o %~dp0/Publish/osx-x64
rd /q /s %~dp0\Bin
Pause
Exit

:Error
Goto Begin 

:End 
pause