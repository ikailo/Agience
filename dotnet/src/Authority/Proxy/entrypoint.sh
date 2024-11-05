#!/bin/bash

DOMAIN="preview.agience.ai"
CERT_DIR="/etc/nginx/certs/public"
CERT_PATH="${CERT_DIR}/${DOMAIN}.crt"
KEY_PATH="${CERT_DIR}/${DOMAIN}.key"
CSR_PATH="${CERT_DIR}/${DOMAIN}.csr"
DOMAIN_CONF_PATH="${CERT_DIR}/${DOMAIN}.conf"
CERTBOT_CONF_PATH="${CERT_DIR}/certbot.conf"
PUBLIC_CERT_PATH="/etc/nginx/certs/agience-net.crt"

mkdir -p "${CERT_DIR}"

echo "Copying public certificate from source directory..."
cp "/usr/local/share/agience/agience-net.crt" "${PUBLIC_CERT_PATH}"     

# TODO: This should not be in the public build. It is internal for Agience-SaaS
if [ "$BUILD_ENVIRONMENT" == "preview" ] && ( [ ! -f "${CERT_PATH}" ] || ! openssl x509 -checkend 2592000 -noout -in "${CERT_PATH}" ); then

    echo "Copying domain configuration from source directory..."
    cp "/usr/local/share/agience/${DOMAIN}.conf" "${DOMAIN_CONF_PATH}"

    echo "Copying certbot configuration from source directory..."
    cp "/usr/local/share/agience/certbot.conf" "${CERTBOT_CONF_PATH}" 

    if [ ! -f "${KEY_PATH}" ]; then
        echo "Creating private key for ${DOMAIN}..."
        openssl genpkey -algorithm RSA -out "${KEY_PATH}" -pkeyopt rsa_keygen_bits:2048
    fi    

    if [ ! -f "${CSR_PATH}" ]; then
        echo "Creating CSR for ${DOMAIN}..."
        openssl req -new -key "${KEY_PATH}" -out "${CSR_PATH}" -config "${DOMAIN_CONF_PATH}"
    fi

    echo "Requesting a new certificate for ${DOMAIN} using Certbot with HTTP-01 challenge..."
    certbot certonly --standalone -n -vvv --csr "${CSR_PATH}" --config "${CERTBOT_CONF_PATH}" --preferred-challenges http

    # Locate the new certificate files in the current directory after Certbot issues them
    CERTBOT_CERT="$(find . -name 'cert.pem')"
    CERTBOT_KEY="$(find . -name 'privkey.pem')"

    if [ -f "${CERTBOT_CERT}" ] && [ -f "${CERTBOT_KEY}" ]; then
        cp "${CERTBOT_CERT}" "${CERT_PATH}"
        cp "${CERTBOT_KEY}" "${KEY_PATH}"
    else
        echo "Error: Certificate or key file not found after Certbot request."
        tail -f /dev/null
        exit 1
    fi

    # Adjust permissions on the new certificate
    chmod 600 "${KEY_PATH}"
    chmod 644 "${CERT_PATH}"
    chown root:root "${CERT_PATH}" "${KEY_PATH}"
    
    echo "Certificate issued and applied successfully."
fi

# Start NGINX in the foreground
echo "Starting NGINX..."
nginx -g "daemon off;"