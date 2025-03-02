#!/bin/bash
set -e

# Create a writable directory for runtime certificates
mkdir -p /etc/postgresql/certs

# Copy certificate files from the read-only bind mount to a writable runtime directory
cp /usr/share/postgresql/certs/* /etc/postgresql/certs/

# Update permissions and ownership on the runtime copies
chmod 644 /etc/postgresql/certs/*.crt
chmod 600 /etc/postgresql/certs/*.key
chown postgres:postgres /etc/postgresql/certs/*

# Choose the certificate and key based on LAN_EXTERNAL_AUTHORITY flag
if [ "$LAN_EXTERNAL_AUTHORITY" = "true" ]; then
    export SSL_CERT_FILE="/etc/postgresql/certs/wan.crt"
    export SSL_KEY_FILE="/etc/postgresql/certs/wan.key"
else
    export SSL_CERT_FILE="/etc/postgresql/certs/lan.crt"
    export SSL_KEY_FILE="/etc/postgresql/certs/lan.key"
fi

# Substitute environment variables in the template files and write the final configs
envsubst < /usr/share/postgresql/templates/pg_hba.conf.template > /etc/postgresql/pg_hba.conf
envsubst < /usr/share/postgresql/templates/postgresql.conf.template > /etc/postgresql/postgresql.conf

# Start the main process
exec /usr/local/bin/docker-entrypoint.sh "$@"
