FROM nginx:1.27.0

ARG BUILD_ENVIRONMENT
ENV BUILD_ENVIRONMENT=${BUILD_ENVIRONMENT}

# Create necessary directories
RUN mkdir -p /usr/local/share/agience

# Copy Configurations
COPY stack/Authority/Proxy/nginx.${BUILD_ENVIRONMENT}.conf /etc/nginx/nginx.conf
COPY stack/Authority/Build/certs/agience-net.crt /usr/local/share/agience/agience-net.crt

# Install OpenSSL, Certbot, and required capabilities for NGINX
RUN apt-get update && apt-get install -y \
    certbot \
    libcap2-bin && \
    setcap 'cap_net_bind_service=+ep' /usr/sbin/nginx && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

# Copy scripts and set permissions
COPY stack/Authority/Build/${BUILD_ENVIRONMENT}/ /usr/local/share/agience/
COPY stack/Authority/Proxy/entrypoint.sh /usr/local/bin/entrypoint.sh

# Set permissions
RUN chmod 644 /usr/local/share/agience/*.conf && \
    chmod +x /usr/local/bin/entrypoint.sh && \
    chown -R root:root /usr/local/bin /usr/local/share/agience

# Set working directory and entrypoint
WORKDIR /usr/local/bin

ENTRYPOINT ["./entrypoint.sh"]
