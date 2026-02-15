#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT_DIR"

echo "Ensuring SQL Server is running..."
docker compose up -d sqlserver

echo "Applying EF Core migration..."
docker compose run --rm db-migrator

echo "Migration completed."
