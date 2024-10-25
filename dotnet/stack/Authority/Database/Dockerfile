FROM postgres:16.3

ARG BUILD_ENVIRONMENT

#RUN mkdir -p /etc/postgresql/certs

# Configuration
COPY Database/postgresql.${BUILD_ENVIRONMENT}.conf /etc/postgresql/postgresql.conf
COPY Database/pg_hba.${BUILD_ENVIRONMENT}.conf /etc/postgresql/pg_hba.conf

RUN chmod 644 /etc/postgresql/postgresql.conf
RUN chmod 644 /etc/postgresql/pg_hba.conf
RUN chown postgres:postgres /etc/postgresql/postgresql.conf /etc/postgresql/pg_hba.conf

# Server Certificates
COPY Build/certs/agience-net.crt /etc/postgresql/certs/agience-net.crt
COPY Build/certs/agience-net.key /etc/postgresql/certs/agience-net.key

RUN chmod 644 /etc/postgresql/certs/agience-net.crt
RUN chmod 600 /etc/postgresql/certs/agience-net.key
RUN chown postgres:postgres /etc/postgresql/certs/agience-net.crt /etc/postgresql/certs/agience-net.key

# Start PostgreSQL with the custom configuration
CMD ["postgres", "-c", "config_file=/etc/postgresql/postgresql.conf"]