@echo off

SET ENVIRONMENT=%1

setlocal EnableDelayedExpansion

:: Define the character set (alphanumeric)
set charset=abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789

:: Generate unique passwords for each field by creating a new `randomString` each time
set "randomString="
for /l %%i in (1,1,16) do (
    set /a "index=!random! %% 62"
    for %%A in (!index!) do set "randomString=!randomString!!charset:~%%A,1!"
)
SET "NEW_SOURCE_PASSWORD=!randomString!"

set "randomString="
for /l %%i in (1,1,16) do (
    set /a "index=!random! %% 62"
    for %%A in (!index!) do set "randomString=!randomString!!charset:~%%A,1!"
)
SET "NEW_RELAY_PASSWORD=!randomString!"

set "randomString="
for /l %%i in (1,1,16) do (
    set /a "index=!random! %% 62"
    for %%A in (!index!) do set "randomString=!randomString!!charset:~%%A,1!"
)
SET "NEW_ADMIN_PASSWORD=!randomString!"

:: Display generated passwords for debugging
::echo Generated source password: !NEW_SOURCE_PASSWORD!
::echo Generated relay password: !NEW_RELAY_PASSWORD!
::echo Generated admin password: !NEW_ADMIN_PASSWORD!

:: File path
SET FILE_PATH=..\..\Stream\icecast.%ENVIRONMENT%.secrets.xml
SET TEMP_FILE=%TEMP%\tempfile_icecast.tmp

SET "LINE="

:: Update XML file with new passwords
FOR /F "delims=" %%A IN ('TYPE "%FILE_PATH%"') DO (
    SET "LINE=%%A"

    SET "TRIMMED=!LINE: =!" :: Space character

    :: Check for specific lines to replace
    if "!TRIMMED!"=="<source-password>hackme</source-password>" (
        echo ^<source-password^>!NEW_SOURCE_PASSWORD!^</source-password^>>>"%TEMP_FILE%"
    ) else if "!TRIMMED!"=="<relay-password>hackme</relay-password>" (
        echo ^<relay-password^>!NEW_RELAY_PASSWORD!^</relay-password^>>>"%TEMP_FILE%"
    ) else if "!TRIMMED!"=="<admin-password>hackme</admin-password>" (
        echo ^<admin-password^>!NEW_ADMIN_PASSWORD!^</admin-password^>>>"%TEMP_FILE%"
    ) else (
        echo %%A>>"%TEMP_FILE%"
    )
)

:: Replace the original file with the updated file
MOVE /Y "%TEMP_FILE%" "%FILE_PATH%"

endlocal