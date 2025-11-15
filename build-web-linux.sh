#!/bin/bash
# Build Web App and Update Frontend Container - Linux/macOS Script
# This script builds the web application and updates the Docker frontend container

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo ""
echo "========================================"
echo -e "${BLUE}Build Web App and Update Container${NC}"
echo "========================================"
echo ""

# Check if we're in the right directory
if [ ! -f "Web/package.json" ]; then
    echo -e "${RED}Error: package.json not found in Web directory${NC}"
    echo "Please run this script from the project root directory"
    exit 1
fi

# Kill any running Node processes to avoid file locks
echo -e "${YELLOW}[0/4] Terminating any running Node processes...${NC}"
pkill -f node 2>/dev/null || true
sleep 2
echo -e "${GREEN}[0/4] Node processes terminated${NC}"
echo ""

# Step 1: Install dependencies
echo -e "${YELLOW}[1/4] Installing npm dependencies...${NC}"
cd Web
npm install
if [ $? -ne 0 ]; then
    echo -e "${RED}Error: npm install failed${NC}"
    cd ..
    exit 1
fi
echo -e "${GREEN}[1/4] npm dependencies installed successfully${NC}"
echo ""

# Step 2: Build the application
echo -e "${YELLOW}[2/4] Building the web application...${NC}"
npm run build
if [ $? -ne 0 ]; then
    echo -e "${RED}Error: npm build failed${NC}"
    cd ..
    exit 1
fi
echo -e "${GREEN}[2/4] Web application built successfully${NC}"
echo ""

# Step 3: Return to project root
cd ..

# Step 4: Build Docker container
echo -e "${YELLOW}[3/4] Building Docker frontend container...${NC}"
docker-compose build frontend
if [ $? -ne 0 ]; then
    echo -e "${RED}Error: docker-compose build failed${NC}"
    exit 1
fi
echo -e "${GREEN}[3/4] Docker container built successfully${NC}"
echo ""

# Success message
echo "========================================"
echo -e "${GREEN}[4/4] Build Complete!${NC}"
echo "========================================"
echo ""
echo "The frontend container has been updated"
echo "with the latest changes."
echo ""
echo "To start the application, run:"
echo -e "${BLUE}  docker-compose up${NC}"
echo ""
exit 0
