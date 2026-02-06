@echo off
setlocal

set PROJECT_NAME=UFO 50 Mod Converter
set OUTPUT_DIR=publish

:: Clean previous publish
if exist "%OUTPUT_DIR%" (
    rmdir /s /q "%OUTPUT_DIR%"
)

:: Publish for Windows x64
echo Building for Windows x64...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "%OUTPUT_DIR%"

if %ERRORLEVEL% neq 0 (
    echo.
    echo ERROR: Build failed!
    pause
    exit /b 1
)

:: Copy config file
if exist "GMLoader.ini" (
    copy "GMLoader.ini" "%OUTPUT_DIR%\" >nul
)

:: Copy GameSpecificData
if exist "GameSpecificData" (
    xcopy "GameSpecificData" "%OUTPUT_DIR%\GameSpecificData\" /e /i /q >nul
)

:: Delete pdb
if exist "%OUTPUT_DIR%\UFO 50 Mod Converter.pdb" (
    del /q "%OUTPUT_DIR%\UFO 50 Mod Converter.pdb"
)

pause