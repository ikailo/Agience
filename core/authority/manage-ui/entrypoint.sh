#!/bin/sh
set -e

# Check if MANAGE_UI_ORIGIN_URI is set
if [ -z "$VITE_MANAGE_UI_ORIGIN_URI" ]; then
  echo "Error: VITE_MANAGE_UI_ORIGIN_URI is not set."
  exit 1
fi

# Remove protocol (if present) so we have "host:port"
ORIGIN_NO_PROTOCOL="${VITE_MANAGE_UI_ORIGIN_URI#*://}"

# Extract host and port using parameter expansion
export MANAGE_UI_ORIGIN_HOST="${ORIGIN_NO_PROTOCOL%%:*}"
export MANAGE_UI_ORIGIN_PORT="${ORIGIN_NO_PROTOCOL##*:}"

# Substitute only the desired variables; leave $uri and others intact.
envsubst '$MANAGE_UI_ORIGIN_PORT $MANAGE_UI_ORIGIN_HOST $WAN_CRT_PATH $WAN_KEY_PATH' \
  < /usr/share/nginx/templates/default.conf.template \
  > /etc/nginx/conf.d/default.conf

# Execute the CMD provided to the container (i.e. start Nginx)
exec "$@"
