@echo off
REM Unregisters the SmartFolder Shell Extension
REM Requires 'srm.exe' to be in the PATH or current directory.

echo Unregistering SmartFolder Shell Extension...
srm.exe uninstall SmartFolder.dll

if %errorlevel% neq 0 (
    echo.
    echo Error: Failed to unregister extension.
    echo Please ensure 'srm.exe' is available and you are running as Administrator.
) else (
    echo.
    echo Successfully unregistered.
)
pause
