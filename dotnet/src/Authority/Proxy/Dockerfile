FROM nginx:1.27.0

ARG BUILD_ENVIRONMENT
ENV BUILD_ENVIRONMENT=${BUILD_ENVIRONMENT}

# Create necessary directories
RUN mkdir -p /usr/local/share/agience

# Copy Configurations
COPY Authority/Proxy/nginx.${BUILD_ENVIRONMENT}.conf /etc/nginx/nginx.conf
COPY Authority/Build/certs/agience-${BUILD_ENVIRONMENT}.crt /usr/local/share/agience/agience-${BUILD_ENVIRONMENT}.crt

# Install OpenSSL, Certbot, and required capabilities for NGINX
RUN apt-get update && apt-get install -y \
    certbot \
    libcap2-bin && \
    setcap 'cap_net_bind_service=+ep' /usr/sbin/nginx && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

# Copy scripts and set permissions
COPY Authority/Build/${BUILD_ENVIRONMENT}/ /usr/local/share/agience/
COPY Authority/Proxy/entrypoint.sh /usr/local/bin/entrypoint.sh

# Set permissions
RUN find /usr/local/share/agience/ -name "*.conf" -exec chmod 644 {} \; && \
    chmod +x /usr/local/bin/entrypoint.sh && \
    chown -R root:root /usr/local/bin /usr/local/share/agience

# Set working directory and entrypoint
WORKDIR /usr/local/bin

ENTRYPOINT ["./entrypoint.sh"]
