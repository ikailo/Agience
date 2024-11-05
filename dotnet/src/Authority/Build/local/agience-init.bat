@echo off

echo Generating public certificates...

mkdir ..\certs 2>nul

call generate-certs-local.agience.ai.bat

cd ..\common

call agience-init.bat local

cd ..\local