@echo off

set DOMAIN=agience-net

cd ..\certs

echo Checking if internal certificates exist...

REM Check if any of the internal certificate files do not exist
if not exist "%DOMAIN%.crt" goto GENERATE
if not exist "%DOMAIN%.key" goto GENERATE
if not exist "%DOMAIN%.pfx" goto GENERATE
goto EOF

:GENERATE
echo Generating internal certificates...

openssl req -x509 -nodes -days 1825 -newkey rsa:2048 -keyout %DOMAIN%.key -out %DOMAIN%.crt -config ..\internal\%DOMAIN%.conf
openssl pkcs12 -export -out %DOMAIN%.pfx -inkey %DOMAIN%.key -in %DOMAIN%.crt -password pass:

:EOF

echo Internal certificate generation complete...

cd ..\internal