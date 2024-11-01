@echo off

SET ENVIRONMENT=%1

setlocal enabledelayedexpansion

:: Define the character set (alphanumeric + special characters)
set charset=abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789

:: Initialize the random string variable
set "randomString="

:: Loop 16 times to generate a random character each time
for /l %%i in (1,1,16) do (
    set /a "index=!random! %% 62"
    for %%A in (!index!) do set "randomString=!randomString!!charset:~%%A,1!"
)

:: The new lines to replace in both files
SET "NEW_LINE_IDENTITY=AuthorityDbPassword=!randomString!"
SET "NEW_LINE_DB=POSTGRES_PASSWORD=!randomString!"

:: File paths
SET FILE_PATH_IDENTITY=..\..\Identity\.env.%ENVIRONMENT%
SET FILE_PATH_DATABASE=..\..\Database\.env.%ENVIRONMENT%

:: Update Authority Identity file (AuthorityDbPassword=)
SET TEMP_FILE=%TEMP%\tempfile_identity.tmp
FOR /F "delims=" %%A IN ('TYPE "%FILE_PATH_IDENTITY%"') DO (
    SET "LINE=%%A"
    IF "!LINE!"=="AuthorityDbPassword=" (
        ECHO !NEW_LINE_IDENTITY!>>"%TEMP_FILE%"
    ) ELSE (
        ECHO %%A>>"%TEMP_FILE%"
    )
)
:: Replace the original Authority Identity file with the updated one
MOVE /Y "%TEMP_FILE%" "%FILE_PATH_IDENTITY%"

:: Update Authority DB file (POSTGRES_PASSWORD=)
SET TEMP_FILE=%TEMP%\tempfile_db.tmp
FOR /F "delims=" %%A IN ('TYPE "%FILE_PATH_DATABASE%"') DO (
    SET "LINE=%%A"
    IF "!LINE!"=="POSTGRES_PASSWORD=" (
        ECHO !NEW_LINE_DB!>>"%TEMP_FILE%"
    ) ELSE (
        ECHO %%A>>"%TEMP_FILE%"
    )
)
:: Replace the original Authority DB file with the updated one
MOVE /Y "%TEMP_FILE%" "%FILE_PATH_DATABASE%"

endlocal