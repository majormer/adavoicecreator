@echo off
echo ADA Voice Creator - C# Console Application
echo ==========================================
echo.
echo Before running, make sure to:
echo 1. Copy .env.sample to .env
echo 2. Edit .env with your Google Cloud credentials
echo 3. Enable Text-to-Speech API in your Google Cloud project
echo.
echo Example commands:
echo.
echo Single phrase:
echo   dotnet run --project ADAVoice.Console --text "FICSIT recommends you proceed with caution."
echo.
echo Batch processing:
echo   dotnet run --project ADAVoice.Console --input sample_phrases.txt
echo.
echo Cost estimation:
echo   dotnet run --project ADAVoice.Console --text "Hello" --cost
echo.
echo List voices:
echo   dotnet run --project ADAVoice.Console --voices
echo.
pause
