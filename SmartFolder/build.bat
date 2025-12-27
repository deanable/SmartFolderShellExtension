@echo off
REM Builds the SmartFolder Shell Extension
REM Requires the .NET SDK to be installed (https://dotnet.microsoft.com/download)

echo Building SmartFolder Shell Extension...
dotnet build -c Release

if %errorlevel% neq 0 (
    echo.
    echo Error: Build failed.
    echo Please ensure you have the .NET SDK installed and a valid .csproj file in this directory.
) else (
    echo.
    echo Build successful.
)
pause