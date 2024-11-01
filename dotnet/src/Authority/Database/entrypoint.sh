#!/bin/bash

# Set permissions on the cert files
chmod 644 /etc/postgresql/certs/certfile.crt
chmod 600 /etc/postgresql/certs/keyfile.key
chown postgres:postgres /etc/postgresql/certs/certfile.crt /etc/postgresql/certs/keyfile.key

# Start the main process
exec /usr/local/bin/docker-entrypoint.sh "$@"
