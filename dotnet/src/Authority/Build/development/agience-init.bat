@echo off

echo Generating public certificates...

mkdir ..\certs 2>nul

call generate-certs-localhost.bat

echo Initializing Agience...

cd ..\common
call agience-init.bat development