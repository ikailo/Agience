@echo off

SET SERVICE=%1

cd ..\common

call docker-down.bat development %SERVICE%

cd ..\development