@echo off
echo ========================================================
echo   Building EyeSaver for Windows (Single-File Standalone)
echo ========================================================

where dotnet >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] .NET SDK is not found. Please install .NET 8 SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo.
echo Compiling Release binary for win-x64...
dotnet publish EyeSaver.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ../dist/windows

if %errorlevel% equ 0 (
    echo.
    echo ========================================================
    echo   BUILD SUCCESSFUL!
    echo   Output executable: dist\windows\EyeSaver.exe
    echo ========================================================
) else (
    echo.
    echo [ERROR] Build failed.
)

pause
