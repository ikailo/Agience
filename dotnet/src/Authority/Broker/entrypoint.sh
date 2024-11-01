#!/bin/bash

# Set permissions on certificate files for Mosquitto
chmod 644 /etc/mosquitto/certs/*.crt
chmod 600 /etc/mosquitto/certs/*.key
chown mosquitto:mosquitto /etc/mosquitto/certs/*

update-ca-certificates

exec mosquitto -c "/etc/mosquitto/mosquitto.conf" "$@"

