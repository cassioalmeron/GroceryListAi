# Grocery List API

A FastAPI-based grocery list application with AI chat integration supporting both ChatGPT and Ollama, with SQLAlchemy database persistence.

## Features

- ✅ CRUD operations for grocery items
- ✅ Mark items as checked/unchecked
- ✅ AI chat integration with streaming responses
- ✅ Support for both ChatGPT (OpenAI) and Ollama
- ✅ CORS enabled for frontend integration
- ✅ Server-Sent Events (SSE) for real-time streaming
- ✅ SQLAlchemy database with SQLite (or PostgreSQL/MySQL)
- ✅ Automatic timestamps (created_at, updated_at)
- ✅ Data persistence across server restarts

## Setup

### 1. Install uv (Recommended Package Manager)

Install `uv` for faster and more reliable package management:

```bash
# On Windows (PowerShell)
powershell -c "irm https://astral.sh/uv/install.ps1 | iex"

# On macOS/Linux
curl -LsSf https://astral.sh/uv/install.sh | sh

# Or using pip
pip install uv
```

### 2. Create Virtual Environment

Create and activate a virtual environment:

#### Using uv (recommended):
```bash
# Create virtual environment with uv
uv venv

# Activate virtual environment
# On Windows:
.venv\Scripts\activate
# On macOS/Linux:
source .venv/bin/activate
```

#### Using traditional Python venv:
```bash
# Create virtual environment
python -m venv venv

# Activate virtual environment
# On Windows:
venv\Scripts\activate
# On macOS/Linux:
source venv/bin/activate
```

### 3. Install Dependencies

#### Using uv (recommended):
```bash
# Install all dependencies with uv (fastest and most reliable)
uv sync
```

#### Using pip (alternative):
```bash
# Install all dependencies at once
pip install fastapi uvicorn python-dotenv openai ollama sqlalchemy

# Or install individually if you encounter conflicts
pip install fastapi uvicorn python-dotenv sqlalchemy
pip install openai ollama
```

#### Troubleshooting Installation Issues

If you encounter dependency conflicts (like `jiter` version issues), try:

```bash
# Upgrade pip first
pip install --upgrade pip

# Install with no dependencies first, then install them separately
pip install openai --no-deps
pip install httpx pydantic anyio sniffio typing-extensions

# Or use a specific version that's compatible
pip install "openai>=1.0.0,<2.0.0"
```

### 4. Configure Environment Variables

Copy the sample environment file:

```bash
cp env.sample .env
```

Edit `.env` and set your configuration:

#### For Ollama (Local AI)
```env
LLM=ollama
MODEL=llama2
DATABASE_URL=sqlite:///./grocery_list.db
```

#### For ChatGPT
```env
LLM=chatgpt
MODEL=gpt-3.5-turbo
OPENAI_API_KEY=your_actual_api_key_here
DATABASE_URL=sqlite:///./grocery_list.db
```

### 5. Run the Server

The database tables will be created automatically on first run.

```bash
python api.py
```

Or:

```bash
uvicorn api:app --reload
```

The API will be available at `http://localhost:8000`

**Note:** The SQLite database file (`grocery_list.db`) will be created in the project root directory on first run.

## API Endpoints

### Grocery Items

- **GET** `/health` - Health check
- **GET** `/items` - Get all items
- **POST** `/items` - Create a new item
- **DELETE** `/items/{item_id}` - Delete an item
- **PATCH** `/items/{item_id}/checked` - Update item checked status

### AI Chat

- **POST** `/chat` - Chat with AI (streaming response)

For detailed API documentation, see [API_CONTRACT.md](API_CONTRACT.md)

## Usage Examples

### Create an Item

```bash
curl -X POST http://localhost:8000/items \
  -H "Content-Type: application/json" \
  -d '{"description": "Milk"}'
```

### Chat with AI

```bash
curl -X POST http://localhost:8000/chat \
  -H "Content-Type: application/json" \
  -d '{"messages": [{"role": "user", "content": "What should I buy for breakfast?"}]}'
```

## Requirements

- Python 3.10+ (recommended: Python 3.11 or 3.12 for better compatibility)
- FastAPI
- Uvicorn
- python-dotenv
- SQLAlchemy
- openai (for ChatGPT support)
- ollama (for Ollama support)

### Python Version Compatibility

If you're using Python 3.8 or 3.9, you may encounter dependency conflicts. Consider upgrading to Python 3.11+ for better package compatibility.

## Environment Variables

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `LLM` | No | `ollama` | AI provider: `chatgpt` or `ollama` |
| `MODEL` | **Yes** | - | Model name (e.g., `llama2`, `gpt-3.5-turbo`) |
| `OPENAI_API_KEY` | Only for ChatGPT | - | OpenAI API key |
| `DATABASE_URL` | No | `sqlite:///./grocery_list.db` | Database connection URL |

## Database

The application uses SQLAlchemy with SQLite by default. Data persists across server restarts.

### Database Schema

```sql
Table: items
- id: Integer (Primary Key, Auto-increment)
- description: String(255) - Item description
- checked: Boolean - Check status
- created_at: DateTime - Creation timestamp
- updated_at: DateTime - Last update timestamp
```

### Using Other Databases

To use PostgreSQL or MySQL, update the `DATABASE_URL` in your `.env`:

**PostgreSQL:**
```env
DATABASE_URL=postgresql://user:password@localhost/grocery_list
```

**MySQL:**
```env
DATABASE_URL=mysql+pymysql://user:password@localhost/grocery_list
```

For more details, see [DATABASE_USAGE.md](DATABASE_USAGE.md)

## License

MIT

