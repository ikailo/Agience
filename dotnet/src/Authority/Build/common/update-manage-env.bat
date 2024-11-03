@echo off

SET ENVIRONMENT=%1

setlocal enabledelayedexpansion

:: File path to update
set FILE_PATH_MANAGE=..\..\Manage\.env.%ENVIRONMENT%

:: Temporary file for modification
set TEMP_FILE=%TEMP%\tempfile.tmp

:: Capture the output of the docker logs command
for /f "tokens=1,2 delims==" %%i in ('docker logs identity-cont ^| findstr /r "^HostId ^HostSecret"') do (
    if "%%i"=="HostId" set "HostIdValue=%%j"
    if "%%i"=="HostSecret" set "HostSecretValue=%%j"
)

:: Ensure the values were captured
if not defined HostIdValue (
    echo Error: HostId not found.
    exit /b 1
)
if not defined HostSecretValue (
    echo Error: HostSecret not found.
    exit /b 1
)

:: Replace the existing values in the file
for /f "delims=" %%A in ('type "%FILE_PATH_MANAGE%"') do (
    set "LINE=%%A"
    if "!LINE:~0,6!"=="HostId" (
        echo HostId=!HostIdValue!>>"%TEMP_FILE%"
    ) else if "!LINE:~0,10!"=="HostSecret" (
        echo HostSecret=!HostSecretValue!>>"%TEMP_FILE%"
    ) else (
        echo %%A>>"%TEMP_FILE%"
    )
)

:: Replace the original file with the updated one
move /y "%TEMP_FILE%" "%FILE_PATH_MANAGE%"
if errorlevel 1 (
    echo Error: Failed to update the file. Could not move "%TEMP_FILE%" to "%FILE_PATH_MANAGE%".
    exit /b 1
)

echo Updated HostId and HostSecret in the file successfully.

endlocal
