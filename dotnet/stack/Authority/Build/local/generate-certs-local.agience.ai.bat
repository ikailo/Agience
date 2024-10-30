@echo off

set DOMAIN=local.agience.ai

cd ..\certs

echo Checking if local certificates exist...

REM Check if any of the certificate files do not exist
if not exist "%DOMAIN%.crt" goto GENERATE
if not exist "%DOMAIN%.key" goto GENERATE
if not exist "%DOMAIN%.pfx" goto GENERATE
goto EOF

:GENERATE
echo Generating local certificates...

openssl req -x509 -nodes -days 1825 -newkey rsa:2048 -keyout %DOMAIN%.key -out %DOMAIN%.crt -config ..\local\%DOMAIN%.conf -extensions req_ext
openssl pkcs12 -export -out %DOMAIN%.pfx -inkey %DOMAIN%.key -in %DOMAIN%.crt -password pass:

echo Installing local certificates...

REM Check if a certificate with this CN is already installed and delete if found
certutil -store -user root | findstr /C:"%DOMAIN%" >nul
if not errorlevel 1 (
    echo Deleting existing certificate with CN=%DOMAIN%...
    certutil -delstore -user root "%DOMAIN%"
)

REM Add the new certificate to the root store
echo Adding new certificate with CN=%DOMAIN%
certutil -addstore -user root "%DOMAIN%.crt"

:EOF

echo Local certificate generation complete...
 
cd ..\local
