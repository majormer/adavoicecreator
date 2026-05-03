@echo off
REM ADA Voice Creator Setup Script for Windows

echo ADA Voice Creator Setup
echo ========================
echo.

REM Check if Python is installed
python --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Python is not installed or not in PATH
    echo Please install Python 3.8 or higher from https://python.org
    pause
    exit /b 1
)

echo Python found. Checking version...
python -c "import sys; exit(0 if sys.version_info >= (3, 8) else 1)"
if errorlevel 1 (
    echo ERROR: Python 3.8 or higher is required
    python --version
    pause
    exit /b 1
)

echo Python version OK.
echo.

REM Create virtual environment
echo Creating virtual environment...
if not exist venv (
    python -m venv venv
)

REM Activate virtual environment
echo Activating virtual environment...
call venv\Scripts\activate.bat

REM Install dependencies
echo Installing dependencies...
pip install --upgrade pip
pip install -r requirements.txt

echo.
echo Setup complete!
echo.
echo Next steps:
echo 1. Set your Google Cloud credentials:
echo    set GOOGLE_APPLICATION_CREDENTIALS=path\to\your\key.json
echo.
echo 2. Run ADA Voice Creator:
echo    python adavoice.py "FICSIT recommends you proceed with caution."
echo.
echo 3. Or try batch processing:
echo    python adavoice.py --batch sample_phrases.txt
echo.

pause
