@echo off

set ENVIRONMENT=%1
SET SERVICE=%2

SETLOCAL

cd ..\..\Docker

REM Check if a service name is provided as a parameter
IF "%SERVICE%"=="" (
    echo Stopping and removing all containers...
    docker-compose -f docker-compose.yml -f docker-compose.%ENVIRONMENT%.yml -p agience-%ENVIRONMENT% down
) ELSE (
    echo Stopping and removing service: %SERVICE%...
    docker-compose -f docker-compose.yml -f docker-compose.%ENVIRONMENT%.yml -p agience-%ENVIRONMENT% down %SERVICE%
)

echo Removing unused networks...
docker network prune -f

REM echo Removing unused volumes...
REM docker volume prune -f

cd ..\Build\common

echo Done

ENDLOCAL