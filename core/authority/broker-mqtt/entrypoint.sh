#!/bin/bash
set -e

# Ensure directory edists
mkdir -p /etc/mosquitto

# Substitute environment variables in the template and write to the actual config location
envsubst < /usr/share/mosquitto/templates/mosquitto.conf.template > /etc/mosquitto/mosquitto.conf

# Execute the command passed via CMD
exec "$@"
