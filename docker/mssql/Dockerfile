FROM mcr.microsoft.com/mssql/server:2019-latest

# Switch to root to install packages
USER root

# Install mssql-tools and unixodbc-dev
RUN apt-get update && \
    apt-get install -y curl gnupg && \
    curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - && \
    curl https://packages.microsoft.com/config/ubuntu/20.04/prod.list > /etc/apt/sources.list.d/mssql-release.list && \
    apt-get update && \
    ACCEPT_EULA=Y apt-get install -y mssql-tools unixodbc-dev && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Create app directory
WORKDIR /usr/src/app

# Copy scripts
COPY ./CrossSetaDeduplicator/db /usr/src/app/db
COPY ./docker/mssql/import-data.sh /usr/src/app/import-data.sh
COPY ./docker/mssql/entrypoint.sh /usr/src/app/entrypoint.sh

# Grant permissions
USER root
RUN chmod +x /usr/src/app/import-data.sh
RUN chmod +x /usr/src/app/entrypoint.sh
USER mssql

# Set entrypoint
ENTRYPOINT ["/bin/bash", "/usr/src/app/entrypoint.sh"]
