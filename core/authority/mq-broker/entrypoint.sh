#!/bin/bash

# Use envsubst to replace variables in mosquitto.conf.template
envsubst < /etc/mosquitto/mosquitto.conf.template > /etc/mosquitto/mosquitto.conf

# Start the main process
exec "$@"