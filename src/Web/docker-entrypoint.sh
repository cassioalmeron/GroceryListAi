#!/bin/sh
# Docker entrypoint script to inject environment variables into index.html

# Set default value if not provided
VITE_API_URL=${VITE_API_URL:-http://localhost:8000}

# Substitute environment variables in index.html
envsubst < /usr/share/nginx/html/index.html > /usr/share/nginx/html/index.html.tmp
mv /usr/share/nginx/html/index.html.tmp /usr/share/nginx/html/index.html

# Start nginx
exec nginx -g "daemon off;"
