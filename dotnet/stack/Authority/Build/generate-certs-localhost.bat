@echo off

cd certs

REM Check if any of the internal certificate files do not exist
if not exist "localhost.crt" goto GENERATE
if not exist "localhost.key" goto GENERATE
if not exist "localhost.pfx" goto GENERATE
goto EOF

:GENERATE
echo Generating localhost certificates...

dotnet dev-certs https --trust -ep localhost.crt -np --format PEM
openssl pkcs12 -export -out localhost.pfx -inkey localhost.key -in localhost.crt -password pass:

:EOF

cd ..