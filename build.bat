@echo off
::rm -rf bin
::rm -rf out
dotnet build
::dotnet build --configuration Release
if not %errorlevel% equ 0 goto error
echo ------------------------------------------
bin\Debug\net6.0-windows10.0.19041.0\artlockimage
echo ------------------------------------------


goto end

:error
echo ERROR!

:end