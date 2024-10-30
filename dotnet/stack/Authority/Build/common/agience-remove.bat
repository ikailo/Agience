@echo off

echo *** WARNING ***
echo *** This script will remove all Agience configurations, containers, networks, and volumes. ***

choice /M "Press Y to proceed or N to cancel."

if errorlevel 2 (
    echo Operation canceled by the user.
    goto :EOF
)

set ENVIRONMENT=%1

cd ..\..\Docker

echo Stopping and removing all containers...
docker-compose -f docker-compose.yml -f docker-compose.%ENVIRONMENT%.yml -p agience-%ENVIRONMENT% down

echo Removing unused networks...
docker network prune -f

echo Removing volume...
docker volume rm agience-%ENVIRONMENT%_agience_data

cd ..\Build

echo Removing certificates...
rmdir /S /Q "certs"

REM Remove the certificates from the local machine
REM dotnet dev-certs https --clean

REM echo Removing configuration...

REM del ..\Identity\.env.local
REM del ..\Manage\.env.local
REM del ..\Database\.env.local

echo Cleanup complete.

cd common

:EOF