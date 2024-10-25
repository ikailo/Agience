@echo off

SETLOCAL ENABLEDELAYEDEXPANSION

set AGIENCE_INITIALIZE=TRUE

echo Creating certificates...

mkdir certs

call generate-certs-internal.bat
call generate-certs-localhost.bat
REM call generate-certs-local.agience.ai.bat

echo Creating configurations...

cd ..\Identity

IF NOT EXIST .env.local (
    copy .env.sample .env.local
)

cd ..\Database

IF NOT EXIST .env.local (
    copy .env.sample .env.local
    echo Generating DB password...
    call ..\Build\generate-db-password.bat
)

cd ..\Manage

IF NOT EXIST .env.local (
    copy .env.sample .env.local
)

echo Initializing database...

cd ..\Build

call docker-up-local.bat database
call ef-migrate.bat

docker exec -it database-cont psql -U agience_db_root -d agience_authority -c "CREATE EXTENSION IF NOT EXISTS pg_trgm;"
docker exec -it database-cont psql -U agience_db_root -d agience_authority -c "SELECT * FROM pg_extension WHERE extname = 'pg_trgm';"

call docker-up-local.bat identity

 timeout /t 10

echo Configuring manage host...
call configure-manage-host.bat

REM call docker-down-local.bat

ENDLOCAL