# Change Management Platform

## Overview
This repository provides a modular skeleton for a Change Management application. It includes a .NET 8 API, a React + TypeScript frontend, SQL migration placeholders, local Docker Compose setup, and CI/CD workflow scaffolding.

## Local Development

### Prerequisites
- Docker
- Docker Compose

### Start services
```bash
docker compose up --build
```

### Services
- API: http://localhost:8080
- Web: http://localhost:5173
- SQL Server: localhost:1433

## Automated Database Migration (DBML schema)

Use these scripts to automate migration commands and create/update the `ChangeManagementDB` database with all required `cm`, `ref`, and `audit` tables.

### Apply migrations (non-destructive)
```bash
./scripts/db/migrate.sh
```

### Recreate database from scratch (destructive)
This resets SQL volume data, recreates the database, applies EF migrations, and verifies required tables.
```bash
./scripts/db/reset-and-migrate.sh
```

### Direct one-shot migrator command
```bash
docker compose --profile tools run --rm db-migrator
```

## API

### Routes

| Method | Route | Description |
| --- | --- | --- |
| GET | `/api/health` | Health check. |
| GET | `/api/changes` | List change requests. |
| GET | `/api/changes/{id}` | Get change request by id. |
| POST | `/api/changes` | Create a change request. |
| PUT | `/api/changes/{id}` | Update a change request. |
| POST | `/api/changes/{id}/submit` | Submit a change request for approval. |
| GET | `/api/changes/{changeId}/tasks` | List tasks for a change. |
| POST | `/api/changes/{changeId}/tasks` | Create task for a change. |
| PUT | `/api/changes/{changeId}/tasks/{taskId}` | Update task. |
| GET | `/api/changes/{changeId}/approvals` | List approvals. |
| POST | `/api/changes/{changeId}/approvals` | Create approval. |
| POST | `/api/changes/{changeId}/approvals/{approvalId}/decision` | Record approval decision. |
| GET | `/api/changes/{changeId}/attachments` | List attachments. |
| POST | `/api/changes/{changeId}/attachments` | Upload attachment. |
| GET | `/api/dashboard` | Dashboard summary stats. |

## Database
- All schema changes live in `database/migrations` and should be applied in version order.
- EF Core migration is in `backend/ChangeManagement.Api/Migrations`.
- `db-migrator` automation applies migration(s) directly against SQL Server.

## Repository Structure
- `backend/ChangeManagement.Api`: .NET 8 Web API
- `backend/ChangeManagement.Api.Tests`: API test project
- `frontend`: React + TypeScript web app
- `database/migrations`: SQL migration scripts
- `scripts/db`: database automation scripts
- `docs`: Architecture and lifecycle placeholders
- `.github/workflows`: CI/CD workflows


## Default Dev Admin

When running in development, the API ensures a default admin exists:

- UPN: `admin@local`
- Password: `Admin123!`
- Role: `Admin`

A CAB demo user is also seeded (`cab@local` / `Admin123!`).

## CI Build Notes

GitHub Actions pipeline should run:

```bash
dotnet restore backend/ChangeManagement.Api/ChangeManagement.Api.csproj
dotnet build backend/ChangeManagement.Api/ChangeManagement.Api.csproj --no-restore
dotnet test backend/ChangeManagement.Api.Tests/ChangeManagement.Api.Tests.csproj --no-build
cd frontend
npm ci
npm run build
```

In tests, the API uses the in-memory EF provider via `CustomWebApplicationFactory`, so CI does not require SQL Server.
