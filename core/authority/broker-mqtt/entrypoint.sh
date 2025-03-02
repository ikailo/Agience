#!/bin/bash
set -e

# Ensure required directories exist
mkdir -p /etc/mosquitto/certs

# Copy certificate files from the read-only bind mount to a writable runtime directory
cp /usr/share/mosquitto/certs/* /etc/mosquitto/certs/

# Update permissions and ownership on the runtime copies
chmod 644 /etc/mosquitto/certs/*.crt
chmod 600 /etc/mosquitto/certs/*.key
chown mosquitto:mosquitto /etc/mosquitto/certs/*

# Set the AUTH_OPT_JWT_HOST variable based on LAN_EXTERNAL_AUTHORITY flag
if [ "$LAN_EXTERNAL_AUTHORITY" = "true" ]; then
  export AUTH_OPT_JWT_HOST="auth_opt_jwt_host $LAN_EXTERNAL_HOST"
else
  export AUTH_OPT_JWT_HOST="auth_opt_jwt_host $LAN_AUTHORITY_HOST"
fi

# Substitute environment variables in the template and write to the final config file
envsubst < /usr/share/mosquitto/templates/mosquitto.conf.template > /etc/mosquitto/mosquitto.conf

# If WAN_ENABLED is not true, remove all lines from "# WAN Websockets listener" to the end of the file
if [ "$WAN_ENABLED" != "true" ]; then
  sed -i '/# WAN Websockets listener/,$d' /etc/mosquitto/mosquitto.conf
fi

# Update CA certificates
update-ca-certificates

# Execute the command passed via CMD
exec "$@"
