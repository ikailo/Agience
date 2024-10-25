@echo off
setlocal enabledelayedexpansion

SET ASPNETCORE_ENVIRONMENT=Design

:: Change directory to the project
cd ../Identity

:: Generate a unique migration name using random numbers
set MIGRATION_NAME=migration_%random%%random%

echo Starting the migration...

:: Add a new migration
dotnet ef migrations add %MIGRATION_NAME%

endlocal
