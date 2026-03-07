# VPSI – Automatizované parkoviště

Systém automatizovaného parkoviště s LPR identifikací vozidel, závorovým systémem, senzory obsazenosti a platbou před výjezdem.

## Struktura projektu

| Složka | Technologie | Popis |
|--------|-------------|-------|
| [`backend/`](backend/) | .NET 10, ASP.NET Core, EF Core | Web API s clean architecture |
| [`frontend/`](frontend/) | React 19, Vite 7, TypeScript 5 | Webová aplikace (veřejná + admin) |
| [`database/`](database/) | PostgreSQL 18 | Schema, migrace, seed data |
| [`hw-simulator/`](hw-simulator/) | .NET 10 Worker | Mock HW zařízení (LPR, závory, senzory) |
| [`contracts/`](contracts/) | OpenAPI, JSON Schema, MQTT | Sdílené API kontrakty |
| [`docs/`](docs/) | Markdown, Mermaid | Architektura, ADR, průvodce |
| [`docker/`](docker/) | Docker Compose | Lokální vývojový stack |

## Rychlý start

### Předpoklady

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 22 LTS](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Spuštění celého stacku

```bash
cd docker
docker compose up -d
```

| Služba | URL |
|--------|-----|
| API (Swagger) | http://localhost:5001/swagger |
| Frontend | http://localhost:5173 |
| PostgreSQL | localhost:5432 |

### Spuštění jednotlivých komponent

```bash
# Backend API
dotnet run --project backend/src/Vpsi.Api

# Frontend
cd frontend && npm install && npm run dev

# HW simulátor
dotnet run --project hw-simulator/src/HwSimulator.App -- --scenario NormalFlow
```

## Klíčové dokumenty

- [Architektura systému](docs/architecture/vpsi.md)
- [Lokální vývoj](docs/guides/local-development.md)
