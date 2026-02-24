# Docker

Docker Compose stack pro lokální vývoj systému VPSI.

## Služby

| Služba | Image | Port | Popis |
|--------|-------|------|-------|
| `db` | postgres:18-alpine | 5432 | PostgreSQL databáze |
| `api` | build z `backend/` | 5001 | ASP.NET Core Web API |
| `frontend` | build z `frontend/` | 5173 | React aplikace (nginx) |
| `hw-simulator` | build z `hw-simulator/` | – | Mock HW zařízení |

## Spuštění

```bash
# Celý stack
docker compose up -d

# Jen databáze
docker compose up -d db

# Sledování logů
docker compose logs -f api
```

## Konfigurace

Zkopíruj `.env.example` do `.env` a uprav hodnoty:

```bash
cp .env.example .env
```

## Zastavení

```bash
docker compose down

# Včetně dat (smazání DB volumes)
docker compose down -v
```
