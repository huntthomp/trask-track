#!/bin/sh
set -e

MC_USER=$MINIO_ROOT_USER
MC_PASS=$MINIO_ROOT_PASSWORD

echo "Creating market data bucket..."

mc alias set local http://localhost:9000 "$MC_USER" "$MC_PASS"
mc mb -p local/user-calendars || true

echo "Bucket created successfully."