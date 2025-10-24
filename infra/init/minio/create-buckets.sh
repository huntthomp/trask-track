#!/bin/sh
set -e

echo "Creating market data bucket..."

mc alias set local http://localhost:$MINIO_API_PORT "$MINIO_ROOT_USER" "$MINIO_ROOT_PASSWORD"
mc mb -p local/user-calendars || true

mc version enable local/user-calendars

echo "Bucket created successfully."