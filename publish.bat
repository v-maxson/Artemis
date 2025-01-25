@echo off

if not exist "bin" mkdir bin

echo Building NWDB Scraper for linux-x64...
dotnet publish "NWDB Scraper" -c Release -r linux-x64 /p:PublishSingleFile=true -o bin\nwdb
dotnet publish "NWDB Scraper" -c Release -r win-x64 /p:PublishSingleFile=true -o bin\nwdb\win-x64

echo.
echo Building Artemis for linux-x64...
dotnet publish Artemis -c Release -r linux-x64 /p:PublishSingleFile=true -o bin\artemis

echo.
echo Build process completed.
pause
