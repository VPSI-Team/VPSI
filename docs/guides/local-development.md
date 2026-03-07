# Lokální vývoj

Průvodce nastavením lokálního vývojového prostředí pro projekt VPSI.

## Předpoklady

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 22 LTS](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Git](https://git-scm.com/)

## Spuštění celého stacku

```bash
cd docker
docker compose up -d
```

## Spuštění jednotlivých komponent

### Backend API

```bash
dotnet run --project backend/src/Vpsi.Api
# Swagger UI: http://localhost:5001/swagger
```

### Frontend

```bash
cd frontend
npm install
npm run dev
# http://localhost:5173
```

### Databáze

```bash
# Lokální PostgreSQL přes Docker
docker compose -f docker/docker-compose.yml up -d db

# Reset databáze
bash database/scripts/reset-local-db.sh
```

### HW simulátor

```bash
dotnet run --project hw-simulator/src/HwSimulator.App -- --scenario NormalFlow
```
