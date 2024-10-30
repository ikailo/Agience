#!/bin/bash

DOMAIN="agience-net"
CERT_DIR="./certs"
CERT="$CERT_DIR/${DOMAIN}"
CONFIG_FILE="./internal/${DOMAIN}.conf"

mkdir -p "$CERT_DIR"

# Check if any of the internal certificate files do not exist
if [ ! -f "${CERT}.crt" ] || [ ! -f "${CERT}.key" ] || [ ! -f "${CERT}.pfx" ]; then

    echo "Generating internal certificates..."

    openssl req -x509 -nodes -days 1825 -newkey rsa:2048 -keyout "${CERT}.key" -out "${CERT}.crt" -config "$CONFIG_FILE"
    openssl pkcs12 -export -out "${CERT}.pfx" -inkey "${CERT}.key" -in "${CERT}.crt" -passout pass:
    
fi
