@echo off
if not exist out\artlockimage.exe echo "out\artlockimage.exe missing" && goto end
if not exist out\urls echo "out\urls missing" && goto end

for /f "tokens=* usebackq" %%f in (`powershell "(Get-Item -path out\artlockimage.exe).VersionInfo.ProductVersion"`) do (
set var=%%f
)
set var2=%var:"=%
set var=%var2:-=%
echo %var%
7z a out\artlockimage-%var%.zip ./out/urls ./out/artlockimage.exe
:end

