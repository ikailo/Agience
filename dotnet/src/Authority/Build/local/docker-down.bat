@echo off

SET SERVICE=%1

cd ..\common
call docker-down.bat local %SERVICE%