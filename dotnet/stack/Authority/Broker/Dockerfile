FROM iegomez/mosquitto-go-auth:2.1.0-mosquitto_2.0.15

ARG BUILD_ENVIRONMENT

# Configuration
COPY stack/Authority/Broker/mosquitto.${BUILD_ENVIRONMENT}.conf /etc/mosquitto/mosquitto.conf

# Server Certificates
COPY stack/Authority/Build/certs/agience-net.crt /etc/mosquitto/certs/agience-net.crt
COPY stack/Authority/Build/certs/agience-net.key /etc/mosquitto/certs/agience-net.key

RUN chown mosquitto:mosquitto /etc/mosquitto/certs/agience-net.key

# CA Certificates
COPY stack/Authority/Build/certs/agience-net.crt /usr/local/share/ca-certificates/agience-net.crt

RUN update-ca-certificates