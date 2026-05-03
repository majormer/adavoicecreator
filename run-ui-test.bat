@echo off
echo Running ADA Voice Creator UI...
echo ==============================
echo.
echo The application should now launch and show:
echo 1. A warning about missing credentials (normal if not configured)
echo 2. The main UI window
echo.
echo If you don't see the UI, check for any error dialogs.
echo.
dotnet run --project ADAVoice.UI
echo.
echo UI closed. Exit code: %ERRORLEVEL%
pause
