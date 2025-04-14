FROM postgres:16

# Install envsubst (for config substitution)
RUN apt-get update && \
    apt-get install -y gettext-base && \
    rm -rf /var/lib/apt/lists/*

# Copy the template configuration files into the container
COPY ./authority/database-sql/pg_hba.conf.template /usr/share/postgresql/templates/pg_hba.conf.template
COPY ./authority/database-sql/postgresql.conf.template /usr/share/postgresql/templates/postgresql.conf.template

# Copy the entrypoint into the container
COPY ./authority/database-sql/entrypoint.sh /usr/local/bin/entrypoint.sh
RUN chmod +x /usr/local/bin/entrypoint.sh

# Use custom entrypoint script
ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]

# Start PostgreSQL with the custom configuration
CMD ["postgres", "-c", "config_file=/etc/postgresql/postgresql.conf"]
