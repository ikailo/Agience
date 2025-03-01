#!/bin/bash
set -e

# Ensure directory exists
mkdir -p /etc/mosquitto

# Set CONFIG_FILE to the template file
CONFIG_FILE="/usr/share/mosquitto/templates/mosquitto.conf.template"

# Remove the invalid auth_opt_jwt_host line based on LAN_EXTERNAL_AUTHORITY flag
if [ "$LAN_EXTERNAL_AUTHORITY" = "true" ]; then
  sed -i '/auth_opt_jwt_host[[:space:]]\+\${LAN_AUTHORITY_HOST}/d' "$CONFIG_FILE"
else
  sed -i '/auth_opt_jwt_host[[:space:]]\+\${LAN_EXTERNAL_HOST}/d' "$CONFIG_FILE"
fi

# If WAN_ENABLED is not true, remove all lines from "# WAN Websockets listener" to the end of the file
if [ "$WAN_ENABLED" != "true" ]; then
  sed -i '/# WAN Websockets listener/,$d' "$CONFIG_FILE"
fi

# Substitute environment variables in the template and write to the actual config location
envsubst < "$CONFIG_FILE" > /etc/mosquitto/mosquitto.conf

# Update Certificates
update-ca-certificates

# Execute the command passed via CMD
exec "$@"
