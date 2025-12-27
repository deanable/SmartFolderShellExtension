@echo off
REM Registers the SmartFolder Shell Extension
REM Requires 'srm.exe' (SharpShell Server Registration Manager) to be in the PATH or current directory.
REM You can install it via 'dotnet tool install -g SharpShell.Tools' or download it from SharpShell GitHub.

echo Registering SmartFolder Shell Extension...
srm.exe install SmartFolder.dll -codebase

if %errorlevel% neq 0 (
    echo.
    echo Error: Failed to register extension.
    echo Please ensure 'srm.exe' is available in the system PATH or this folder.
    echo Also ensure you are running as Administrator.
) else (
    echo.
    echo Successfully registered. Restart Windows Explorer to see changes.
)
pause
