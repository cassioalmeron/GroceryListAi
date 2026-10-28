# Python FastAPI Backend
FROM python:3.14-slim

WORKDIR /app

# Install system dependencies
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Copy application code (required for pyproject.toml packages)
COPY . .

# Install Python dependencies from pyproject.toml
RUN pip install --no-cache-dir .

# Create logs directory
RUN mkdir -p /app/logs

# Build argument for API port (can be overridden in docker-compose.yml)
ARG API_PORT=5002

# Expose port
EXPOSE ${API_PORT}

# Health check
HEALTHCHECK --interval=30s --timeout=10s --retries=3 --start-period=40s \
    CMD curl -f http://localhost:${API_PORT}/health || exit 1

# Environment variables
ENV PYTHONUNBUFFERED=1

# Run the application
CMD ["python", "main.py"]
