@echo off

SETLOCAL

cd ../Docker

SET SERVICE=%1

IF "%SERVICE%"=="" (
    echo No specific service provided. Building all services...
    docker compose -f docker-compose.yml -f docker-compose.local.yml -p agience-local build
) ELSE (
    echo Building service: %SERVICE%...
    docker compose -f docker-compose.yml -f docker-compose.local.yml -p agience-local build %SERVICE%
)

IF %ERRORLEVEL% NEQ 0 (
    echo Image build failed, exiting script.
    exit /b %ERRORLEVEL%
)

IF "%SERVICE%"=="" (
    call ../Build/docker-down-local.bat
) ELSE (
    call ../Build/docker-down-local.bat %SERVICE%
)

echo Starting containers...

IF "%SERVICE%"=="" (
    docker compose -f docker-compose.yml -f docker-compose.local.yml -p agience-local up -d
) ELSE (
    docker compose -f docker-compose.yml -f docker-compose.local.yml -p agience-local up -d %SERVICE%
)

echo Cleaning up dangling images...
docker image prune -f

IF "%SERVICE%"=="" (
    echo Restarting manage-cont...
    timeout /t 10
    docker restart manage-cont
) 

echo Done

ENDLOCAL