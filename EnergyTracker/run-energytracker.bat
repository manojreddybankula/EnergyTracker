@echo off
REM Build and run the EnergyTracker project
setlocal
cd /d %~dp0

dotnet build EnergyTracker\EnergyTracker.csproj
if %errorlevel% neq 0 (
    echo Build failed.
    exit /b %errorlevel%
)

dotnet run --project EnergyTracker\EnergyTracker.csproj
endlocal
