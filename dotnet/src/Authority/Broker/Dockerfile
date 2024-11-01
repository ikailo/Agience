FROM iegomez/mosquitto-go-auth:2.1.0-mosquitto_2.0.15

ARG BUILD_ENVIRONMENT

# Copy the Mosquitto configuration
COPY Authority/Broker/mosquitto.${BUILD_ENVIRONMENT}.conf /etc/mosquitto/mosquitto.conf

# Copy the entrypoint script
COPY Authority/Broker/entrypoint.sh /usr/local/bin/entrypoint.sh

RUN chmod +x /usr/local/bin/entrypoint.sh

# Run the entrypoint script
ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]