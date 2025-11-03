# GroceryListAI

A modern full-stack web application that combines grocery list management with AI-powered chat assistance. Users can create, organize, and manage their shopping lists while interacting with an intelligent assistant (powered by ChatGPT or local Ollama models) to receive shopping suggestions, recipe recommendations, and personalized assistance.

![Full-Stack Application](https://img.shields.io/badge/Full--Stack-React%20%2B%20FastAPI-brightgreen)
![TypeScript](https://img.shields.io/badge/TypeScript-5.9.3-blue)
![Python](https://img.shields.io/badge/Python-3.11+-green)
![License](https://img.shields.io/badge/License-MIT-yellow)

## Table of Contents

- [Features](#features)
- [Quick Start](#quick-start)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Installation](#installation)
- [Configuration](#configuration)
- [Usage](#usage)
- [Development](#development)
- [API Documentation](#api-documentation)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)

## Features

- **Grocery List Management**
  - Create, read, update, and delete shopping list items
  - Mark items as purchased with a single click
  - Real-time synchronization with backend
  - Persistent storage with SQLite or PostgreSQL

- **AI Chat Integration**
  - Stream-based responses for natural conversation flow
  - Real-time message streaming via Server-Sent Events (SSE)
  - Context-aware shopping suggestions
  - Support for both ChatGPT and local Ollama models
  - Flexible LLM provider switching

- **Multi-LLM Support**
  - **OpenAI ChatGPT**: Cloud-based, powerful language model
  - **Ollama**: Local models for privacy and offline operation
  - Easy configuration switching via environment variables

- **Production Ready**
  - Windows service capability for background operation
  - Type-safe codebase (TypeScript + Python type hints)
  - Async/await support for responsive user experience
  - CORS enabled for development (configurable for production)
  - Comprehensive error handling and logging

- **Responsive Design**
  - Modern React 19 interface with TypeScript
  - Clean, intuitive user experience
  - Fast performance with Vite build tool

## Quick Start

### Prerequisites

- **Backend**: Python 3.11 or higher, uv package manager
- **Frontend**: Node.js 18+ and npm
- **Optional**: Ollama (for local LLM) or OpenAI API key (for ChatGPT)

### 5-Minute Setup

```bash
# Clone and navigate to project
cd GroceryListAi

# Terminal 1: Start Backend
cd Server
uv venv && source .venv/bin/activate  # Windows: .venv\Scripts\activate
uv sync
cp env.sample .env
# Edit .env with your LLM choice (ollama or chatgpt)
python api.py

# Terminal 2: Start Frontend
cd Web
npm install
npm run dev
```

Then open http://localhost:5173 in your browser!

## Architecture

GroceryListAI follows a clean separation of concerns with a dedicated backend API and frontend UI:

```
┌─────────────────────────────────────────┐
│  Frontend (React + TypeScript)          │
│  - Todo/Grocery List Management         │
│  - AI Chat Interface                    │
│  - Real-time Response Streaming         │
└──────────────┬──────────────────────────┘
               │ HTTP/REST + SSE
               │ Port 8000
┌──────────────▼──────────────────────────┐
│  Backend (FastAPI + Python)             │
│  - Item CRUD Endpoints                  │
│  - Chat Streaming Endpoint (SSE)        │
│  - LLM Integration Layer                │
│  - Database ORM (SQLAlchemy)            │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│  Database (SQLite or PostgreSQL/MySQL)  │
│  - Items Table                          │
│  - Persistent Shopping List Data        │
└─────────────────────────────────────────┘
```

## Technology Stack

### Backend (Server/)

| Technology | Version | Purpose |
|-----------|---------|---------|
| **FastAPI** | Latest | Async web framework with automatic API documentation |
| **Uvicorn** | Latest | ASGI server for running FastAPI |
| **SQLAlchemy** | Latest | ORM for database operations |
| **Pydantic** | Latest | Data validation and serialization |
| **Python** | 3.11+ | Programming language |
| **uv** | Latest | Fast Python package installer and resolver |

### Frontend (Web/)

| Technology | Version | Purpose |
|-----------|---------|---------|
| **React** | 19.1.1 | UI library and component framework |
| **TypeScript** | 5.9.3 | Type-safe JavaScript superset |
| **Vite** | 7.1.7 | Fast build tool and dev server |
| **Axios** | 1.12.2 | HTTP client for API communication |
| **React Router** | 7.9.4 | Client-side routing |
| **CSS** | Native | Component-scoped styling |

## Project Structure

```
GroceryListAi/
├── Server/                                # Backend API (FastAPI)
│   ├── Models/
│   │   ├── database.py                   # SQLAlchemy setup & session
│   │   ├── item.py                       # Item ORM model
│   │   └── schemas.py                    # Pydantic schemas
│   ├── api.py                            # FastAPI application & endpoints
│   ├── llm.py                            # ChatGPT & Ollama integration
│   ├── main.py                           # CLI for service management
│   ├── service_manager.py                # Windows service handler
│   ├── logger.py                         # Logging configuration
│   ├── pyproject.toml                    # Dependencies (uv format)
│   ├── .env                              # Environment config (DO NOT commit)
│   ├── env.sample                        # .env template
│   ├── README.md                         # Backend documentation
│   ├── API_CONTRACT.md                   # API specifications
│   ├── DATABASE_USAGE.md                 # Database guide
│   └── logs/                             # Runtime logs
│
├── Web/                                  # Frontend (React + TypeScript)
│   ├── src/
│   │   ├── Pages/
│   │   │   ├── Home/                    # Main application page
│   │   │   └── Tests/                   # Development/testing page
│   │   ├── components/
│   │   │   ├── TodoList/                # Grocery list component
│   │   │   └── Chat/                    # AI chat component
│   │   ├── assets/                      # Images & static resources
│   │   ├── App.tsx                      # Main router & layout
│   │   ├── main.tsx                     # Application entry point
│   │   ├── App.css                      # App-level styles
│   │   └── index.css                    # Global styles
│   ├── public/                          # Static files
│   ├── package.json                     # Dependencies & scripts
│   ├── vite.config.ts                   # Vite configuration
│   ├── tsconfig.json                    # TypeScript configuration
│   ├── eslint.config.js                 # Code quality rules
│   ├── index.html                       # HTML template
│   ├── README.md                        # Frontend documentation
│   └── dist/                            # Production build
│
├── README.md                             # This file
├── claude.md                             # Claude AI context
└── .gitignore                            # Git ignore rules
```

## Installation

### Backend Setup (Server/)

#### Step 1: Navigate to Server Directory

```bash
cd Server
```

#### Step 2: Create Virtual Environment

Using `uv` (recommended):

```bash
uv venv
source .venv/bin/activate        # On macOS/Linux
# OR
.venv\Scripts\activate           # On Windows
```

#### Step 3: Install Dependencies

```bash
uv sync
```

#### Step 4: Configure Environment

```bash
cp env.sample .env
```

Edit `.env` and configure your settings:

```env
# LLM Provider Selection
LLM=ollama                    # Options: ollama, chatgpt
MODEL=llama2                  # Model name (varies by provider)
OPENAI_API_KEY=               # Required only if LLM=chatgpt

# Database Configuration
DATABASE_URL=sqlite:///./items.db

# API Server
API_PORT=8000

# Windows Service (optional)
SERVICE_NAME=GroceryListAI
```

#### Step 5: Run the Server

```bash
python api.py
```

Server will be available at `http://localhost:8000`

Access interactive API docs at `http://localhost:8000/docs`

### Frontend Setup (Web/)

#### Step 1: Navigate to Web Directory

```bash
cd Web
```

#### Step 2: Install Dependencies

```bash
npm install
```

#### Step 3: Start Development Server

```bash
npm run dev
```

Frontend will be available at `http://localhost:5173`

## Configuration

### Environment Variables

Create a `.env` file in the `Server/` directory with the following variables:

```env
# ========================
# LLM Configuration
# ========================
LLM=ollama                    # chatgpt or ollama
MODEL=llama2                  # Model identifier
OPENAI_API_KEY=your-key-here  # Required for ChatGPT

# ========================
# Database Configuration
# ========================
# SQLite (Default - no setup required)
DATABASE_URL=sqlite:///./items.db

# PostgreSQL (Production recommended)
# DATABASE_URL=postgresql://user:password@localhost:5432/grocerydb

# MySQL
# DATABASE_URL=mysql+pymysql://user:password@localhost/grocerydb

# ========================
# API Configuration
# ========================
API_PORT=8000                 # Server port
CORS_ORIGINS=["http://localhost:5173"]  # Frontend URLs

# ========================
# Logging (Optional)
# ========================
LOG_LEVEL=INFO                # DEBUG, INFO, WARNING, ERROR

# ========================
# Windows Service (Optional)
# ========================
SERVICE_NAME=GroceryListAI
SERVICE_DISPLAY_NAME=Grocery List AI Service
```

### LLM Configuration

#### Using Ollama (Local)

1. **Install Ollama**: https://ollama.ai
2. **Pull a model**:
   ```bash
   ollama pull llama2
   ```
3. **Start Ollama**:
   ```bash
   ollama serve
   ```
4. **Configure `.env`**:
   ```env
   LLM=ollama
   MODEL=llama2
   ```

#### Using ChatGPT (OpenAI)

1. **Get API Key**: https://platform.openai.com/api-keys
2. **Configure `.env`**:
   ```env
   LLM=chatgpt
   MODEL=gpt-3.5-turbo
   OPENAI_API_KEY=sk-...
   ```

### Database Configuration

#### SQLite (Development - Default)

No additional setup required. Uses file-based database.

```env
DATABASE_URL=sqlite:///./items.db
```

#### PostgreSQL (Production Recommended)

1. **Install PostgreSQL**: https://www.postgresql.org/download/
2. **Create database**:
   ```bash
   createdb grocerydb
   ```
3. **Configure connection**:
   ```env
   DATABASE_URL=postgresql://user:password@localhost:5432/grocerydb
   ```

#### MySQL

1. **Install MySQL**: https://www.mysql.com/downloads/
2. **Create database**:
   ```bash
   mysql -u root -p -e "CREATE DATABASE grocerydb;"
   ```
3. **Configure connection**:
   ```env
   DATABASE_URL=mysql+pymysql://user:password@localhost/grocerydb
   ```

## Usage

### Running the Full Application

#### Development Mode

**Terminal 1 - Backend:**
```bash
cd Server
source .venv/bin/activate      # Activate virtual environment
python api.py
```

**Terminal 2 - Frontend:**
```bash
cd Web
npm run dev
```

Open http://localhost:5173 in your browser.

#### Production Build

**Frontend:**
```bash
cd Web
npm run build
npm run preview
```

**Backend:**
```bash
cd Server
python api.py
```

### Windows Service (Optional)

Run commands with administrator privileges:

```bash
cd Server
python main.py install      # Install as Windows service
python main.py start        # Start the service
python main.py stop         # Stop the service
python main.py uninstall    # Remove the service
```

### API Endpoints

| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | `/health` | Health check |
| GET | `/items` | List all grocery items |
| POST | `/items` | Create new item |
| PATCH | `/items/{id}/checked` | Toggle item completion status |
| DELETE | `/items/{id}` | Delete an item |
| POST | `/chat` | Stream chat responses (SSE) |

Full API documentation available at `http://localhost:8000/docs` when server is running.

## Development

### Project Commands

#### Backend (Server/)

```bash
# Install dependencies
uv sync

# Run development server
python api.py

# Run as Windows service
python main.py install
python main.py start
python main.py stop
python main.py uninstall

# View logs
tail -f logs/app.log
```

#### Frontend (Web/)

```bash
# Install dependencies
npm install

# Development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Lint and check code quality
npm run lint
```

### Code Style Guidelines

**Python Backend**
- Follow PEP 8 style guide
- Use type hints for all functions
- Use async/await for I/O operations
- Validate requests with Pydantic
- Document endpoints and functions

**TypeScript Frontend**
- Use strict TypeScript mode
- Name components with PascalCase
- Use functional components with hooks
- Keep components single-responsibility
- Use component-scoped CSS

### Testing

#### Backend

Currently uses manual testing:
- Use FastAPI Swagger UI: `http://localhost:8000/docs`
- Use FastAPI ReDoc: `http://localhost:8000/redoc`
- Test with curl or Postman

#### Frontend

Currently uses manual testing:
- Test in browser at `http://localhost:5173`
- Use browser DevTools for debugging
- Run ESLint for code quality: `npm run lint`

Future: Integrate pytest (backend) and Vitest (frontend)

## API Documentation

### Detailed API Specifications

Full API contract documentation available in:
- Backend: `Server/API_CONTRACT.md`
- Frontend: `Web/API_CONTRACT.md`

### Chat Endpoint (SSE Streaming)

The `/chat` endpoint uses Server-Sent Events for real-time response streaming:

```javascript
// JavaScript example
const eventSource = new EventSource('/chat?message=What%20is%20milk%20good%20for?');

eventSource.onmessage = (event) => {
  console.log('Response chunk:', event.data);
};

eventSource.onerror = () => {
  eventSource.close();
};
```

## Troubleshooting

### Backend Issues

#### Port Already in Use

```bash
# Find process using port 8000 and kill it
lsof -i :8000              # macOS/Linux
netstat -ano | findstr :8000  # Windows
```

#### Import Errors

```bash
# Ensure virtual environment is activated
source .venv/bin/activate     # macOS/Linux
.venv\Scripts\activate        # Windows

# Reinstall dependencies
uv sync --refresh
```

#### Ollama Connection Error

```bash
# Ensure Ollama is running
ollama serve

# Verify model is pulled
ollama list
ollama pull llama2
```

#### OpenAI API Error

- Verify API key in `.env` is correct
- Check API quota and billing at platform.openai.com
- Ensure internet connection is active

### Frontend Issues

#### Port Conflicts

```bash
# Change Vite port in vite.config.ts
export default {
  server: {
    port: 5174  // Change port here
  }
}
```

#### Dependencies Not Installing

```bash
# Clear cache and reinstall
rm -rf node_modules package-lock.json
npm install
```

#### CORS Errors

- Ensure backend is running on port 8000
- Verify CORS is enabled in backend configuration
- Check browser console for specific error messages

### Database Issues

#### SQLite File Not Created

- Ensure `Server/` directory is writable
- Check `.env` DATABASE_URL is correct

#### PostgreSQL Connection Refused

```bash
# Verify PostgreSQL is running
psql -U postgres  # macOS/Linux
net start PostgreSQL14  # Windows
```

## Contributing

Contributions are welcome! Follow these guidelines:

1. **Fork the repository**
2. **Create a feature branch**: `git checkout -b feature/your-feature`
3. **Make your changes** with clear, descriptive commits
4. **Test your changes** thoroughly
5. **Push to your branch**: `git push origin feature/your-feature`
6. **Open a Pull Request** with a detailed description

### Development Workflow

- Work from `main` branch
- Create feature branches for new features
- Write clear commit messages
- Ensure code follows project style guidelines
- Test both backend and frontend before submitting PR

## License

MIT License - See LICENSE file for details

## Support

For issues, questions, or suggestions:

- **GitHub Issues**: Report bugs and request features
- **Documentation**: Check `Server/README.md` and `Web/README.md`
- **API Docs**: View interactive docs at `http://localhost:8000/docs`

## Acknowledgments

- **FastAPI**: Modern, fast web framework for building APIs
- **React**: UI library for building interactive interfaces
- **OpenAI**: ChatGPT API for advanced language capabilities
- **Ollama**: Local LLM support for privacy-first operations
- **SQLAlchemy**: Powerful ORM for database operations

---

**Happy grocery shopping with AI! 🛒✨**
