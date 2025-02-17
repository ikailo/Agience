FROM postgres:16.3

# Install envsubst (for config substitution)
RUN apt-get update && \
    apt-get install -y gettext-base && \
    rm -rf /var/lib/apt/lists/*

# Copy the template configuration files into the container
COPY ./pg_hba.conf.template /etc/postgresql/pg_hba.conf.template
COPY ./postgresql.conf.template /etc/postgresql/postgresql.conf.template

# Copy the entrypoint into the container
COPY ./entrypoint.sh /usr/local/bin/entrypoint.sh
RUN chmod +x /usr/local/bin/entrypoint.sh

# Use custom entrypoint script
ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]

# Start PostgreSQL with the custom configuration
CMD ["postgres", "-c", "config_file=/etc/postgresql/postgresql.conf"]
