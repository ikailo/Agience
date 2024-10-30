@echo off

SETLOCAL

cd ../Docker

REM Check if a service name is provided as a parameter
SET SERVICE=%1

IF "%SERVICE%"=="" (
    echo Stopping and removing all containers...
    docker-compose -f docker-compose.yml -f docker-compose.local.yml -p agience-local down
) ELSE (
    echo Stopping and removing service: %SERVICE%...
    docker-compose -f docker-compose.yml -f docker-compose.local.yml -p agience-local down %SERVICE%
)

echo Removing unused networks...
docker network prune -f

REM echo Removing unused volumes...
REM docker volume prune -f

echo Done

ENDLOCAL