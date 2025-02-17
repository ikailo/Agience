FROM iegomez/mosquitto-go-auth:2.1.0-mosquitto_2.0.15

# Install envsubst (for config substitution)
RUN apt-get update --allow-releaseinfo-change && \
    apt-get install -y gettext-base && \
    rm -rf /var/lib/apt/lists/*

# Copy the template conf file into the container
COPY ./mosquitto.conf.template /etc/mosquitto/mosquitto.conf.template

# Copy the entrypoint into the container
COPY ./entrypoint.sh /usr/local/bin/entrypoint.sh
RUN chmod +x /usr/local/bin/entrypoint.sh

# Run the entrypoint script
ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]

# Start Mosquitto
CMD ["mosquitto", "-c", "/etc/mosquitto/mosquitto.conf"]