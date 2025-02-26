#!/bin/bash
set -e

# Ensure directory edists
mkdir -p /etc/mosquitto

# Check if external certs are missing
if [ ! -f "/etc/mosquitto/certs/external.key" ] || [ ! -f "/etc/mosquitto/certs/external.crt" ]; then
    echo "🔹 External certs not found. Removing external listener from Mosquitto config..."
    sed -i '/# External websockets listener/,/keyfile \/etc\/mosquitto\/certs\/external.key/d' /usr/share/mosquitto/templates/mosquitto.conf.template
fi

# Substitute environment variables in the template and write to the actual config location
envsubst < /usr/share/mosquitto/templates/mosquitto.conf.template > /etc/mosquitto/mosquitto.conf

# Update Certificates
update-ca-certificates

# Execute the command passed via CMD
exec "$@"
