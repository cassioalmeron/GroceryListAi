@echo off
REM Build Web App and Update Frontend Container - Windows Script
REM This script builds the web application and updates the Docker frontend container

setlocal enabledelayedexpansion

REM Colors for output (using echo with labels for clarity)
echo.
echo ========================================
echo Build Web App and Update Container
echo ========================================
echo.

REM Check if we're in the right directory
if not exist "Web\package.json" (
    echo Error: package.json not found in Web directory
    echo Please run this script from the project root directory
    exit /b 1
)

REM Kill any running Node processes to avoid file locks
echo [0/4] Terminating any running Node processes...
taskkill /IM node.exe /F >nul 2>&1
timeout /t 2 /nobreak >nul
echo [0/4] Node processes terminated
echo.

REM Step 1: Install dependencies
echo [1/4] Installing npm dependencies...
cd Web
call npm install
if !errorlevel! neq 0 (
    echo Error: npm install failed
    cd ..
    exit /b 1
)
echo [1/4] npm dependencies installed successfully
echo.

REM Step 2: Build the application
echo [2/4] Building the web application...
call npm run build
if !errorlevel! neq 0 (
    echo Error: npm build failed
    cd ..
    exit /b 1
)
echo [2/4] Web application built successfully
echo.

REM Step 3: Return to project root
cd ..

REM Step 4: Build Docker container
echo [3/4] Building Docker frontend container...
call docker-compose build frontend
if !errorlevel! neq 0 (
    echo Error: docker-compose build failed
    exit /b 1
)
echo [3/4] Docker container built successfully
echo.

REM Success message
echo ========================================
echo [4/4] Build Complete!
echo ========================================
echo.
echo The frontend container has been updated
echo with the latest changes.
echo.
echo To start the application, run:
echo   docker-compose up
echo.
pause
exit /b 0
