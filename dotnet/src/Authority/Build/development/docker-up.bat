@echo off

SET SERVICE=%1

cd ..\common

call docker-up.bat development %SERVICE%

cd ..\development

echo Done setting up Agience development.  Visit https://localhost:5002 to view the site.