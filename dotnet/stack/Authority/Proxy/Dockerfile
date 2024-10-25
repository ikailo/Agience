FROM nginx:1.27.0

ARG BUILD_ENVIRONMENT
ENV BUILD_ENVIRONMENT=${BUILD_ENVIRONMENT}

# Configuration
COPY Proxy/nginx.${BUILD_ENVIRONMENT}.conf /etc/nginx/nginx.conf 

# Upstream Certs
COPY Build/certs/agience-net.crt /etc/nginx/certs/agience-net.crt

#Public Certs
COPY Build/certs/localhost.crt /etc/nginx/certs/public/localhost.crt
COPY Build/certs/localhost.key /etc/nginx/certs/public/localhost.key

# Run Nginx
CMD ["nginx", "-g", "daemon off;"]
