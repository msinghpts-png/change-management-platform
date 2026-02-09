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

## API

### Routes

| Method | Route | Description |
| --- | --- | --- |
| GET | `/api/health` | Health check. |
| GET | `/api/changes` | List change requests. |
| GET | `/api/changes/{id}` | Get change request by id. |
| POST | `/api/changes` | Create a change request. |
| PUT | `/api/changes/{id}` | Update a change request. |
| GET | `/api/dashboard` | Dashboard summary stats. |

### Create change request
```json
{
  "title": "Upgrade database cluster",
  "description": "Planned upgrade window.",
  "priority": "High",
  "risk": "Medium",
  "plannedStart": "2024-01-15T10:00:00Z",
  "plannedEnd": "2024-01-15T12:00:00Z",
  "createdBy": "ops@example.com"
}
```

### Update change request
```json
{
  "title": "Upgrade database cluster",
  "description": "Updated description.",
  "status": "Scheduled",
  "priority": "High",
  "risk": "Medium",
  "plannedStart": "2024-01-15T10:00:00Z",
  "plannedEnd": "2024-01-15T12:00:00Z"
}
```

## Database
- All schema changes live in `database/migrations` and should be applied in version order.
- Migration files are versioned (`V001__...`, `V002__...`, etc.).

## Repository Structure
- `backend/ChangeManagement.Api`: .NET 8 Web API
- `backend/ChangeManagement.Api.Tests`: API test project
- `frontend`: React + TypeScript web app
- `database/migrations`: SQL migration scripts
- `docs`: Architecture and lifecycle placeholders
- `.github/workflows`: CI/CD workflows
