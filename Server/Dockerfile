# Python FastAPI Backend
FROM python:3.14-slim

WORKDIR /app

# Install system dependencies
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Copy project files
COPY pyproject.toml ./

# Install Python dependencies using pip
RUN pip install --no-cache-dir \
    dotenv>=0.9.9 \
    fastapi>=0.118.0 \
    ollama>=0.6.0 \
    openai>=2.2.0 \
    sqlalchemy>=2.0.44 \
    uvicorn>=0.37.0

# Copy application code
COPY . .

# Create logs directory
RUN mkdir -p /app/logs

# Expose port
EXPOSE 8000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --retries=3 --start-period=40s \
    CMD curl -f http://localhost:8000/health || exit 1

# Environment variables
ENV PYTHONUNBUFFERED=1
ENV API_PORT=8000

# Run the application
CMD ["python", "main.py"]
