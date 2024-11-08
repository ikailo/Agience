@echo off

SET ENVIRONMENT=%1
SET SERVICE=%2

SETLOCAL

cd ..\..\Docker

IF "%SERVICE%"=="" (
    echo No specific service provided. Building all services...
    docker compose -f docker-compose.yml -f docker-compose.%ENVIRONMENT%.yml -p agience-%ENVIRONMENT% build
) ELSE (
    echo Building service: %SERVICE%...
    docker compose -f docker-compose.yml -f docker-compose.%ENVIRONMENT%.yml -p agience-%ENVIRONMENT% build %SERVICE%
)

IF %ERRORLEVEL% NEQ 0 (
    echo Image build failed, exiting script.
    exit /b %ERRORLEVEL%
)

cd ..\Build\common

call docker-down.bat %ENVIRONMENT% %SERVICE% 

cd ..\..\Docker

echo Starting containers...

IF "%SERVICE%"=="" (
    docker compose -f docker-compose.yml -f docker-compose.%ENVIRONMENT%.yml -p agience-%ENVIRONMENT% up -d
) ELSE (
    docker compose -f docker-compose.yml -f docker-compose.%ENVIRONMENT%.yml -p agience-%ENVIRONMENT% up -d %SERVICE%
)

echo Cleaning up dangling images...

docker image prune -f

IF "%SERVICE%"=="" (
    echo Restarting manage-%ENVIRONMENT%...
    timeout /t 10
    docker restart manage-%ENVIRONMENT%
) 

cd ..\Build\common

echo Done

ENDLOCAL