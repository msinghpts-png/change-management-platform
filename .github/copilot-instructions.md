# Change Management Platform - Copilot Instructions

## Project Purpose
This repository provides a modular change management platform skeleton with a .NET 8 API, a React + TypeScript frontend, and SQL migrations.

## Coding Conventions
- C#: Use PascalCase for types, methods, and public properties. Keep controllers thin and avoid business logic in the API layer.
- TypeScript: Use explicit types for API responses and component props. Prefer named exports for shared utilities.

## Tests
- Use xUnit for API tests.
- Prefer integration-style tests that exercise HTTP endpoints via `WebApplicationFactory`.

## API Contract Style
- DTOs define request/response shapes.
- Use RESTful endpoints and consistent status codes (201 for create, 200 for update/read).

## Database Migrations
- All schema changes must be versioned migrations in `database/migrations`.
- Use `V###__description.sql` naming.
- No direct edits to production databases.
