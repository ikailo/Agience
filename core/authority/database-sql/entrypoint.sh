#!/bin/bash

# Use envsubst to replace variables in mosquitto.conf.template
envsubst <  /usr/share/postgresql/templates/pg_hba.conf.template > /etc/postgresql/pg_hba.conf
envsubst <  /usr/share/postgresql/templates/postgresql.conf.template > /etc/postgresql/postgresql.conf

# Start the main process
exec /usr/local/bin/docker-entrypoint.sh "$@"