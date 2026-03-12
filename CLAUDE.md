# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

VPSI is an automated parking system with LPR vehicle identification, barrier control, occupancy sensors, and pre-exit payment. This is a monorepo with the following components:

| Folder | Tech | Purpose |
|--------|------|---------|
| `backend/` | .NET 9, ASP.NET Core, EF Core 9 | REST API with clean architecture |
| `frontend/` | React 19, Vite 7, TypeScript 5 | Public (payment/QR) + admin web app |
| `database/` | PostgreSQL 18 | Schema, migrations, seed data |
| `hw-simulator/` | .NET 10 Worker Service | Mock HW devices (LPR, barriers, sensors) |
| `contracts/` | OpenAPI, JSON Schema, MQTT | Shared API contracts |
| `docker/` | Docker Compose | Local development stack |

## Commands

### Full stack (from repo root)
```bash
cd docker && docker compose up -d
```

Services: API at `http://localhost:5001/swagger`, Frontend at `http://localhost:5173`, PostgreSQL at `localhost:5432`.

### Backend
```bash
# Run API (from repo root)
dotnet run --project backend/src/Parking.Api

# Run all tests
dotnet test backend/Parking.sln

# Run a single test project
dotnet test backend/tests/Parking.UnitTests

# EF Core migrations (run from backend/)
dotnet ef migrations add <Name> --project src/Parking.Infrastructure --startup-project src/Parking.Api
dotnet ef database update --project src/Parking.Infrastructure --startup-project src/Parking.Api
```

### Frontend
```bash
cd frontend
npm install
npm run dev      # dev server at http://localhost:5173
npm run build    # tsc + vite build
npm run lint     # eslint
```

### HW Simulator
```bash
dotnet run --project hw-simulator/src/HwSimulator.App -- --scenario NormalFlow
dotnet run --project hw-simulator/src/HwSimulator.App -- --scenario ErrorScenario
```

### Database only (via Docker)
```bash
docker compose -f docker/docker-compose.yml up -d db
bash database/scripts/reset-local-db.sh
```

## Backend Architecture (`backend/Parking.sln`)

Clean architecture with strict layer dependencies:

```
Parking.Api          -> HTTP controllers, middleware, DI registration
Parking.Application  -> Use cases (MediatR CQRS), FluentValidation, service interfaces
Parking.Domain       -> Entities, value objects, enums, domain exceptions (no EF/HTTP)
Parking.Infrastructure -> EF Core 9 + Npgsql, repositories, service stubs
Parking.Contracts    -> Versioned DTOs under V1/ (request/response records)
```

Dependency direction: `Api → Application → Domain`, `Api → Infrastructure → Application → Domain`, `Api/Application → Contracts`.

All projects target `net9.0` (upgrade to net10.0 when .NET 10 SDK is installed) with `Nullable=enable` and `TreatWarningsAsErrors=true` (set in `backend/Directory.Build.props`).

**Application layer** is organized by domain feature: `Sessions/Commands/`, `Payments/Commands/`. Each use case has its own folder with Command + Handler + Validator.

**Test projects**: `Parking.UnitTests` (FluentAssertions + NSubstitute), `Parking.IntegrationTests` (WebApplicationFactory + InMemory EF for health/API tests).

**Key design patterns**:
- MediatR: `IRequest<T>` / `IRequestHandler<T>` for all use cases
- Value objects (`PlateNumber`, `Money`, `TimeRange`) are immutable records with factory methods that throw `ArgumentException` on invalid input
- `ParkingSession` state machine: `Active → Paid → Closed` (or `Active → Closed` for free/emergency exit); any → `Disputed`
- `IEventProcessor` is the entry point for all HW device events; routes by `HwEventType`
- Infrastructure stubs: `PlateRecognizerStub`, `PaymentGatewayStub` — replace with real integrations later

**Health checks**: `/health/live` (always OK, no deps), `/health/ready` (DB check tagged `"ready"`).

## HW Simulator Architecture

.NET Worker Service that emits events to the backend API via REST (`POST /api/device-events`). Configuration via `appsettings.json` or environment variables (`API_BASE_URL`, `SIMULATOR_SCENARIO`).

Key internal structure:
- `Simulators/` — `LprCameraSimulator`, `BarrierSimulator`, `OccupancySensorSimulator`
- `Scenarios/` — `NormalFlowScenario`, `ErrorScenario` (implements `IScenario`), `ScenarioFactory`
- `Transport/` — `IEventSender` / `HttpEventSender`

## Domain Model

Core entities: `ParkingLot`, `ParkingSpot`, `Device`, `DeviceEvent`, `Vehicle`, `ParkingSession`, `PaymentIntent`, `AppUser`, `AuditLog`.

Parking session lifecycle: `ACTIVE → PAID → CLOSED` (also `DISPUTED`). Payment intent states: `INITIATED → AUTHORIZED → CAPTURED / FAILED`.

All device events use an `idempotency_key` to prevent duplicate processing. Device types: `LPR`, `BARRIER`, `SENSOR`, `DISPLAY`, `TERMINAL`. Protocols: `REST`, `MQTT`.

## Key API Endpoints (planned)

- `POST /api/v1/hw/events` — unified HW event ingestion (idempotent)
- `GET /api/v1/parking-lots/{lotId}/status` — occupancy
- `POST /api/v1/parking-sessions/quote` — price calculation by plate
- `POST /api/v1/parking-sessions/{sessionId}/payment-intents` — initiate payment
- `POST /api/v1/payments/{paymentIntentId}/confirm` — confirm payment result
- Admin endpoints under `/api/v1/admin/`

## Development Notes

- **SDK**: .NET 9 SDK is currently installed. Upgrade to .NET 10 SDK + bump `Directory.Build.props` + EF Core / Mvc.Testing packages to `10.x` when ready.
- **ConfirmPayment**: `ConfirmPaymentCommandHandler` is intentionally a stub (`NotImplementedException`) — requires a `PaymentIntentRepository` to be added.
- **Swagger UI**: Available at `http://localhost:5261/scalar/v1` (Scalar) in Development mode.
- **HW Simulator**: communicates with the backend at `POST /api/device-events` (note: the planned contract uses `/api/v1/hw/events` — update before wiring up).
- Docker Compose environment variables can be overridden with a `.env` file in `docker/`.
