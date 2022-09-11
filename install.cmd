taskkill /IM WinCommandPalette.exe /F

rmdir InstallBuild /s /q
rmdir %USERPROFILE%\WinCommandPalette\bin /s /q

dotnet.exe publish /p:DebugType=None /p:DebugSymbols=false -c Release -o .\InstallBuild

mkdir %USERPROFILE%\WinCommandPalette\bin
copy .\InstallBuild %USERPROFILE%\WinCommandPalette\bin\

rmdir InstallBuild /s /q

start %USERPROFILE%\WinCommandPalette\bin\WinCommandPalette.exe