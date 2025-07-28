@echo off
REM Fabrikam Development Testing Shortcut (Modular Architecture)
REM This calls the PowerShell test script with modular architecture support

echo.
echo 🧪 Fabrikam Development Testing (Modular Architecture)
echo =====================================================
echo.

REM Check if new modular test script exists, otherwise fallback to old
if exist "%~dp0scripts\Test-Development-New.ps1" (
    echo Using NEW modular testing architecture...
    powershell -ExecutionPolicy Bypass -File "%~dp0scripts\Test-Development-New.ps1" %*
) else if exist "%~dp0scripts\Test-Development.ps1" (
    echo ⚠️  WARNING: Using legacy test script. Modular architecture not found.
    powershell -ExecutionPolicy Bypass -File "%~dp0scripts\Test-Development.ps1" %*
) else (
    echo ❌ No test script found!
    echo Looking for: scripts\Test-Development-New.ps1
    echo Or fallback: scripts\Test-Development.ps1
    exit /b 1
)
