# Persistence + Workflow Integration Guide

## Backend files to modify

- `backend/ChangeManagement.Api/Program.cs`
- `backend/ChangeManagement.Api/Data/ChangeManagementDbContext.cs`
- `backend/ChangeManagement.Api/Domain/Entities/*`
- `backend/ChangeManagement.Api/Repositories/*`
- `backend/ChangeManagement.Api/Services/*`
- `backend/ChangeManagement.Api/Controllers/*`
- `backend/ChangeManagement.Api/Migrations/*`

## Git branch and commit structure

```bash
git checkout -b feat/persistence-and-workflow
# commit 1: db context + migrations + startup migration
# commit 2: approvals + dashboard persistence fixes
# commit 3: attachments backend + frontend wiring
```

## Migration commands

```bash
cd backend/ChangeManagement.Api
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreateWithAttachments
dotnet ef database update
```

## Run stack

```bash
# backend
cd backend/ChangeManagement.Api
dotnet run

# frontend
cd frontend
npm install
npm run dev
```

## Deployment checklist

1. Confirm SQL Server connection string in `appsettings.json`.
2. Ensure runtime user can write to `App_Data/attachments`.
3. Startup applies `Database.Migrate()` automatically.
4. Smoke-test endpoints:
   - `GET /api/changes`
   - `POST /api/changes/{id}/approvals`
   - `POST /api/changes/{id}/approvals/{approvalId}/decision`
   - `POST /api/changes/{id}/attachments`
   - `GET /api/changes/{id}/attachments/{attachmentId}/download`
