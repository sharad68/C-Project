# Tornois

Full-stack sports data aggregation platform built with **ASP.NET Core 10**, **React + Vite**, and **PostgreSQL 16** scaffolding.

## Current progress

- ✅ Public sports browsing API backed by PostgreSQL-seeded platform data
- ✅ JWT admin login, protected dashboard endpoints, and superadmin user management
- ✅ React SPA with pages for sports, competitions, teams, players, matches, and admin operations
- ✅ Quartz background job scaffolding for live sync, fixtures sync, and enrichment sync
- ✅ EF Core PostgreSQL schema model + initial migration for 8 logical schemas
- ✅ Docker Compose setup for API, frontend, PostgreSQL, and PgAdmin

## Project structure

```text
backend/   ASP.NET Core API + application/domain/infrastructure layers
frontend/  React 19 + TypeScript + Vite SPA
```

## Local development

### 1) Backend

```bash
cd backend
dotnet test Tornois.slnx
dotnet run --project src/Tornois.Api/Tornois.Api.csproj --urls http://localhost:5000
```

Backend URL: `http://localhost:5000`
Health check: `http://localhost:5000/api/health`

### 1b) Database and migrations

```bash
cd backend
dotnet tool restore
dotnet tool run dotnet-ef database update --project src/Tornois.Infrastructure --startup-project src/Tornois.Api
```

Migration files live in `backend/src/Tornois.Infrastructure/Data/Migrations/`.

### 2) Frontend

```bash
cd frontend
npm install
npm run dev
```

Frontend URL: `http://localhost:5173`

## Demo admin users

- `superadmin / Pass@123`
- `editor / Editor@123`
- `readonly / Viewer@123`

## Docker Compose

1. Copy `.env.example` to `.env`
2. Set real secrets, especially `JWT_SIGNING_KEY`
3. Start the stack:

```bash
docker compose up --build
```

### Exposed services

| Service     | URL                     |
| ----------- | ----------------------- |
| Frontend    | `http://localhost:5173` |
| Backend API | `http://localhost:5000` |
| PgAdmin     | `http://localhost:5050` |
| PostgreSQL  | `localhost:5433`        |

## Important config

Environment variables supported via `.env` / container settings:

- `JWT_SIGNING_KEY`
- `JWT_ISSUER`
- `JWT_AUDIENCE`
- `POSTGRES_DB`
- `POSTGRES_USER`
- `POSTGRES_PASSWORD`
- `API_SPORTS_KEY`
- `THE_SPORTS_DB_KEY`

## Verification

- Frontend build: `cd frontend && npm run build`
- Backend tests: `cd backend && dotnet test Tornois.slnx`
