@echo off
echo Testing ADA Voice Creator UI...
echo ================================
echo.
echo If you see an error dialog, please note the error message.
echo.
dotnet run --project ADAVoice.UI
echo.
echo Application exited with code: %ERRORLEVEL%
pause
