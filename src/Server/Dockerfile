# ========================================
# Stage 1: Build
# ========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first for better layer caching
COPY GroceryListAi.slnx .
COPY GroceryListAi.Api/GroceryListAi.Api.csproj GroceryListAi.Api/
COPY GroceryListAi.AppHost/GroceryListAi.AppHost.csproj GroceryListAi.AppHost/
COPY GroceryListAi.Core/GroceryListAi.Core.csproj GroceryListAi.Core/
COPY GroceryListAi.ServiceDefaults/GroceryListAi.ServiceDefaults.csproj GroceryListAi.ServiceDefaults/
COPY GroceryListAi.Tests/GroceryListAi.Tests.csproj GroceryListAi.Tests/

# Restore dependencies - solution level for multi-project dependencies
RUN dotnet restore GroceryListAi.slnx

# Copy source code
COPY . .

# Build the solution - solution level to validate all projects
RUN dotnet build GroceryListAi.slnx -c Release --no-restore

# ========================================
# Stage 2: Publish
# ========================================
FROM build AS publish
RUN dotnet publish GroceryListAi.Api/GroceryListAi.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ========================================
# Stage 3: Runtime
# ========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install curl for health checks and clean up apt cache to minimize layer size
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Build argument for API port (can be overridden in docker-compose.yml)
ARG API_PORT=5002

# Expose port
EXPOSE ${API_PORT}

# Environment variables
ENV ASPNETCORE_URLS=http://+:${API_PORT}

# Health check
HEALTHCHECK --interval=30s --timeout=10s --retries=3 --start-period=40s \
    CMD curl -f http://localhost:${API_PORT}/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "GroceryListAi.Api.dll"]
