# Tornois

Full-stack **esports tournament management** platform built with **ASP.NET Core 10**, **React + Vite**, and **PostgreSQL 16** scaffolding.

## Current progress

- ✅ Public browsing API for game titles, tournaments, teams, players, and live series
- ✅ JWT admin login, protected dashboard endpoints, and full CRUD management for games, tournaments, teams, players, and series
- ✅ React SPA for esports operations, rankings, roster views, and admin workflows
- ✅ Quartz background job scaffolding for live sync, schedules, and enrichment sync
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

## Tournament management features

- Admin CRUD for **game titles**, **tournaments**, **teams**, **players/staff**, and **series scheduling**
- Audit log entries for every management change
- Role-gated access: `superadmin` and `editor` can manage tournament data

## Seeded esports catalog

- **Game titles**: League of Legends, Counter-Strike 2, Valorant
- **Tournament examples**: LEC Spring Split, PGL Major Copenhagen, VCT EMEA Stage 1
- **Teams**: T1, G2 Esports, NAVI, Team Vitality, Fnatic, Sentinels

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
