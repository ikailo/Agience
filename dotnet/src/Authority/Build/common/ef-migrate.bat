@echo off
setlocal enabledelayedexpansion

SET EF_MIGRATION=TRUE

:: Change directory to the project
cd ..\..\Identity

:: Generate a unique migration name using random numbers
set MIGRATION_NAME=migration_%random%%random%

echo Starting the migration...

:: Add a new migration
dotnet ef migrations add %MIGRATION_NAME%

cd ..\Build\common

endlocal
