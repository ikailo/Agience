FROM postgres:16.3

ARG BUILD_ENVIRONMENT

# Configuration
COPY Authority/Database/pg_hba.${BUILD_ENVIRONMENT}.conf /etc/postgresql/pg_hba.conf
COPY Authority/Database/postgresql.conf /etc/postgresql/postgresql.conf
COPY Authority/Database/entrypoint.sh /etc/postgresql/entrypoint.sh

# Set permissions for the configuration and entrypoint script
RUN chmod 644 /etc/postgresql/postgresql.conf && \
    chmod 644 /etc/postgresql/pg_hba.conf && \
    chmod 755 /etc/postgresql/entrypoint.sh && \
    chown postgres:postgres /etc/postgresql/postgresql.conf /etc/postgresql/pg_hba.conf /etc/postgresql/entrypoint.sh

# Use custom entrypoint script
ENTRYPOINT ["/etc/postgresql/entrypoint.sh"]

# Start PostgreSQL with the custom configuration
CMD ["postgres", "-c", "config_file=/etc/postgresql/postgresql.conf"]
