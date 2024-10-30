#!/bin/bash

INTERNAL_CERT_PATH="/etc/nginx/certs/agience-net.crt"

echo "Checking if internal certificates exist..."

if [ -f "/usr/local/share/agience/agience-net.crt" ]; then
    echo "Copying internal certificate from source directory..."
    cp "/usr/local/share/agience/agience-net.crt" "${INTERNAL_CERT_PATH}"    
    chmod 644 "${INTERNAL_CERT_PATH}"
    chown root:root "${INTERNAL_CERT_PATH}"
fi    

if [ "${BUILD_ENVIRONMENT}" = "preview" ]; then

    DOMAIN="preview.agience.net"
    CERT_DIR="/etc/nginx/certs/public"
    CERT_PATH="${CERT_DIR}/${DOMAIN}.crt"
    KEY_PATH="${CERT_DIR}/${DOMAIN}.key"
    CSR_PATH="${CERT_DIR}/${DOMAIN}.csr"
    DOMAIN_CONF_PATH="${CERT_DIR}/${DOMAIN}.conf"
    CERTBOT_CONF_PATH="${CERT_DIR}/certbot.conf"    

    mkdir -p "${CERT_DIR}"

    echo "Checking if certificates for ${DOMAIN} require generation or renewal..."

    if [ ! -f "${CERT_PATH}" ] || ! openssl x509 -checkend 2592000 -noout -in "${CERT_PATH}"; then

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
        certbot certonly --standalone -n -vv --csr "${CSR_PATH}" --config "${CERTBOT_CONF_PATH}" --preferred-challenges http

        # Locate the new certificate files in the current directory after Certbot issues them
        CERTBOT_CERT=$(find /usr/local/bin -name '*_chain.pem' -print -quit)

        if [ -f "${CERTBOT_CERT}" ]; then
            cp "${CERTBOT_CERT}" "${CERT_PATH}"           
            rm -f "/usr/local/bin/"*"_cert.pem" "/usr/local/bin/"*"_chain.pem"
        else
            echo "Error: Certificate file not found after Certbot request."
            exit 1
        fi

        # Adjust permissions on the new certificate
        chmod 600 "${KEY_PATH}"
        chmod 644 "${CERT_PATH}"
        chown root:root "${CERT_PATH}" "${KEY_PATH}"
    
        echo "Certificate issued and applied successfully."
    else
        echo "Certificate for ${DOMAIN} is valid and does not require renewal."
    fi

fi

# Start NGINX in the foreground
echo "Starting NGINX..."
nginx -g "daemon off;"
