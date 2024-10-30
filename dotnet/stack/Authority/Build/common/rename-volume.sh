#!/bin/bash

# Define old and new volume names
OLD_VOLUME="agience-local-saas_agience_data"
NEW_VOLUME="agience-local-saas_authority-data"

# Create the new volume
echo "Creating new volume: $NEW_VOLUME"
docker volume create "$NEW_VOLUME"

# Stop all containers using the old volume
echo "Stopping containers using volume: $OLD_VOLUME"
docker ps -q --filter volume="$OLD_VOLUME" | xargs -r docker stop

# Copy data from the old volume to the new volume using a temporary container
echo "Copying data from $OLD_VOLUME to $NEW_VOLUME"
docker run --rm \
    -v "$OLD_VOLUME":/from \
    -v "$NEW_VOLUME":/to \
    alpine ash -c "cd /from && cp -a . /to"

# Start the containers again
echo "Starting containers that were stopped"
docker ps -a --filter volume="$OLD_VOLUME" --format "{{.ID}}" | xargs -r docker start

# Remove the old volume (optional)
echo "Removing old volume: $OLD_VOLUME"
docker volume rm "$OLD_VOLUME"

echo "Volume rename completed successfully from $OLD_VOLUME to $NEW_VOLUME."
