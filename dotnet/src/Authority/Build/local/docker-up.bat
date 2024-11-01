@echo off

SET SERVICE=%1

cd ..\common
call docker-up.bat local %SERVICE%