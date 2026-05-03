@echo off
echo ADA Voice Creator - Windows Forms Application
echo ==============================================
echo.
echo Before running, make sure to:
echo 1. Copy .env.sample to .env
echo 2. Edit .env with your Google Cloud credentials
echo 3. Enable Text-to-Speech API in your Google Cloud project
echo.
echo Starting the application...
echo.
dotnet run --project ADAVoice.UI
pause
