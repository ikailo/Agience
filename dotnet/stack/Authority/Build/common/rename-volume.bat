@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

REM Define old and new volume names
SET "OLD_VOLUME=agience-local-sass_icecast_logs"
SET "NEW_VOLUME=agience-local-saas_icecast-logs"

REM Create the new volume
echo Creating new volume: %NEW_VOLUME%
docker volume create %NEW_VOLUME%

REM Stop all containers using the old volume
echo Stopping containers using volume: %OLD_VOLUME%
FOR /F "tokens=*" %%i IN ('docker ps -q --filter volume^=%OLD_VOLUME%') DO (
    docker stop %%i
)

REM Copy data from the old volume to the new volume using a temporary container
echo Copying data from %OLD_VOLUME% to %NEW_VOLUME%
docker run --rm -v %OLD_VOLUME%:/from -v %NEW_VOLUME%:/to alpine ash -c "cd /from && cp -a . /to"

REM Start the containers again
echo Starting containers that were stopped
FOR /F "tokens=*" %%i IN ('docker ps -a --filter volume^=%OLD_VOLUME% --format "{{.ID}}"') DO (
    docker start %%i
)

REM Remove the old volume (optional)
echo Removing old volume: %OLD_VOLUME%
docker volume rm %OLD_VOLUME%

echo Volume rename completed successfully from %OLD_VOLUME% to %NEW_VOLUME%.

ENDLOCAL
