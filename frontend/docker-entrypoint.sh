#!/bin/sh
# Writes runtime configuration so the API URL can change without rebuilding the image.
set -e
API_URL="${API_URL:-http://localhost:8080}"
printf 'window.__APP_CONFIG__ = { apiUrl: "%s" };\n' "$API_URL" > /usr/share/nginx/html/config.js
exec nginx -g 'daemon off;'
