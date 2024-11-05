@echo off

SET SERVICE=%1

cd ..\common

call docker-up.bat local %SERVICE%

cd ..\local

echo Done setting up Agience local.  Visit https://web.local.agience.ai view the site.