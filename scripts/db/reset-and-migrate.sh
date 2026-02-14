#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT_DIR"

echo "[1/5] Stopping existing containers (if any)..."
docker compose down --remove-orphans

echo "[2/5] Removing SQL volume to force a brand-new database..."
docker volume rm change-management-platform_sql_data >/dev/null 2>&1 || true

echo "[3/5] Starting SQL Server..."
docker compose up -d sqlserver

echo "[4/5] Applying EF Core migration (creates database + tables + seed data)..."
docker compose run --rm db-migrator

echo "[5/5] Verifying required tables..."
docker compose exec -T sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'P@ssw0rd123!' -C -d ChangeManagementDB -Q "
SET NOCOUNT ON;
SELECT s.name + '.' + t.name AS TableName
FROM sys.tables t
JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE (s.name = 'cm' AND t.name IN ('ChangeRequest','ChangeTask','ChangeApproval','ChangeAttachment','User'))
   OR (s.name = 'audit' AND t.name IN ('Event','EventType'))
   OR (s.name = 'ref' AND t.name IN ('ChangeType','ChangePriority','ChangeStatus','RiskLevel','ApprovalStatus'))
ORDER BY TableName;
"

echo "Done. New database with DBML-aligned tables is ready."
