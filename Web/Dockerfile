# Build stage
FROM node:20 AS build

WORKDIR /app

# Copy package files
COPY package.json package-lock.json ./

# Install dependencies
RUN npm ci --prefer-offline --no-audit

# Copy source code
COPY . .

# Build arguments for environment variables (placed after COPY for optimal caching)
ARG VITE_API_URL=http://localhost:8000

# Build application
RUN npm run build

# Runtime stage - serve with nginx
FROM nginx:alpine

# Build argument for API port (used in nginx proxy configuration)
ARG API_PORT=5002

# Install curl for health checks
RUN apk add --no-cache curl

# Copy built application from build stage
COPY --from=build /app/dist /usr/share/nginx/html

# Create nginx configuration with API_PORT substituted at build time
RUN echo "server {" > /etc/nginx/conf.d/default.conf && \
    echo "    listen 80;" >> /etc/nginx/conf.d/default.conf && \
    echo "    server_name _;" >> /etc/nginx/conf.d/default.conf && \
    echo "" >> /etc/nginx/conf.d/default.conf && \
    echo "    location / {" >> /etc/nginx/conf.d/default.conf && \
    echo "        root /usr/share/nginx/html;" >> /etc/nginx/conf.d/default.conf && \
    echo "        try_files \$uri /index.html;" >> /etc/nginx/conf.d/default.conf && \
    echo "    }" >> /etc/nginx/conf.d/default.conf && \
    echo "" >> /etc/nginx/conf.d/default.conf && \
    echo "    location /api/ {" >> /etc/nginx/conf.d/default.conf && \
    echo "        proxy_pass http://backend:${API_PORT}/;" >> /etc/nginx/conf.d/default.conf && \
    echo "        proxy_http_version 1.1;" >> /etc/nginx/conf.d/default.conf && \
    echo "        proxy_set_header Upgrade \$http_upgrade;" >> /etc/nginx/conf.d/default.conf && \
    echo "        proxy_set_header Connection 'upgrade';" >> /etc/nginx/conf.d/default.conf && \
    echo "        proxy_set_header Host \$host;" >> /etc/nginx/conf.d/default.conf && \
    echo "        proxy_cache_bypass \$http_upgrade;" >> /etc/nginx/conf.d/default.conf && \
    echo "    }" >> /etc/nginx/conf.d/default.conf && \
    echo "" >> /etc/nginx/conf.d/default.conf && \
    echo "    location /health {" >> /etc/nginx/conf.d/default.conf && \
    echo "        access_log off;" >> /etc/nginx/conf.d/default.conf && \
    echo "        return 200 \"OK\";" >> /etc/nginx/conf.d/default.conf && \
    echo "        add_header Content-Type text/plain;" >> /etc/nginx/conf.d/default.conf && \
    echo "    }" >> /etc/nginx/conf.d/default.conf && \
    echo "}" >> /etc/nginx/conf.d/default.conf

# Expose port
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=10s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
