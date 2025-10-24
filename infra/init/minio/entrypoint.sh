#!/bin/sh
set -e

echo "Starting MinIO server..."
minio server /data --address :$MINIO_API_PORT --console-address :$MINIO_CONSOLE_PORT &

echo "Waiting for MinIO to become healthy..."
until curl -s http://127.0.0.1:$MINIO_API_PORT/minio/health/live > /dev/null; do
  echo "  -> MinIO not ready yet, retrying in 2s..."
  sleep 2
done

echo "MinIO is healthy. Running bucket initialization..."
/usr/local/bin/create-buckets.sh

echo "Initialization complete. Waiting for background processes..."
wait