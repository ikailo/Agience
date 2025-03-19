# Stage 1: Build the Vite app
FROM node:18-alpine AS builder

WORKDIR /app

# Declare build arguments for VITE_ variables
ARG VITE_AUTHORITY_PUBLIC_URI
ARG VITE_MANAGE_UI_ORIGIN_URI
ARG VITE_MANAGE_UI_CLIENT_ID
ARG VITE_MANAGE_UI_REDIRECT_URI
ARG VITE_MANAGE_UI_LOGOUT_REDIRECT_URI
ARG VITE_AUTHORITY_AUTHORIZATION_ENDPOINT
ARG VITE_AUTHORITY_TOKEN_ENDPOINT
ARG VITE_AUTHORITY_USERINFO_ENDPOINT
ARG VITE_AUTHORITY_REVOCATION_ENDPOINT

# Set them as environment variables so Vite can read them
ENV VITE_AUTHORITY_PUBLIC_URI=${VITE_AUTHORITY_PUBLIC_URI}
ENV VITE_MANAGE_UI_ORIGIN_URI=${VITE_MANAGE_UI_ORIGIN_URI}
ENV VITE_MANAGE_UI_CLIENT_ID=${VITE_MANAGE_UI_CLIENT_ID}
ENV VITE_MANAGE_UI_REDIRECT_URI=${VITE_MANAGE_UI_REDIRECT_URI}
ENV VITE_MANAGE_UI_LOGOUT_REDIRECT_URI=${VITE_MANAGE_UI_LOGOUT_REDIRECT_URI}
ENV VITE_AUTHORITY_AUTHORIZATION_ENDPOINT=${VITE_AUTHORITY_AUTHORIZATION_ENDPOINT}
ENV VITE_AUTHORITY_TOKEN_ENDPOINT=${VITE_AUTHORITY_TOKEN_ENDPOINT}
ENV VITE_AUTHORITY_USERINFO_ENDPOINT=${VITE_AUTHORITY_USERINFO_ENDPOINT}
ENV VITE_AUTHORITY_REVOCATION_ENDPOINT=${VITE_AUTHORITY_REVOCATION_ENDPOINT}

# Copy package files and install dependencies
COPY ./authority/manage-ui/package*.json ./
RUN npm install --omit=optional

# Copy the rest of the source code (including custom files)
COPY ./authority/manage-ui/ ./

# Build the production assets (this should output to /app/dist)
RUN npm run build

# Stage 2: Serve the built app using Nginx
FROM nginx:stable-alpine

EXPOSE 5002

# Install gettext (provides envsubst)
RUN apk update && apk add --no-cache gettext

# Remove the default nginx static assets
RUN rm -rf /usr/share/nginx/html/*

# Copy built files from the previous stage into nginx html folder
COPY --from=builder /app/dist /usr/share/nginx/html

# Copy the custom Nginx config template from the builder stage
COPY --from=builder /app/nginx.conf.template /usr/share/nginx/templates/default.conf.template

# Copy the custom entrypoint script from the builder stage
COPY --from=builder /app/entrypoint.sh /usr/local/bin/entrypoint.sh
RUN chmod +x /usr/local/bin/entrypoint.sh

# Use the custom entrypoint script
ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]

# Start Nginx in the foreground
CMD ["nginx", "-g", "daemon off;"]
