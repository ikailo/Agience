FROM infiniteproject/icecast

ARG BUILD_ENVIRONMENT

# Copy Icecast configuration file based on build environment
COPY Stream/icecast.${BUILD_ENVIRONMENT}.xml /etc/icecast.xml

RUN chmod 644 /etc/icecast.xml && \
    chown icecast:icecast /etc/icecast.xml