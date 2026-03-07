# Databáze

PostgreSQL 18 schema, verzované SQL migrace a testovací data pro systém VPSI.

## Konvence

- **Pojmenování tabulek**: `snake_case` (např. `parking_session`, `device_event`)
- **Primární klíče**: UUID (`gen_random_uuid()`)
- **Časové značky**: `timestamptz` (vždy s časovou zónou)
- **Migrace**: sekvenční pojmenování `V001__popis.sql`, `V002__popis.sql`

## Struktura

| Složka | Popis |
|--------|-------|
| [`migrations/`](migrations/) | Verzované SQL migrace |
| [`seed/`](seed/) | Testovací a demo data |
| [`scripts/`](scripts/) | Utility skripty (reset, backup) |
| [`er-diagrams/`](er-diagrams/) | ER diagramy (Mermaid) |

## Lokální databáze

### Spuštění přes Docker

```bash
docker compose -f docker/docker-compose.yml up -d db
```

### Reset databáze

```bash
bash scripts/reset-local-db.sh
```

### Připojení

```
Host: localhost
Port: 5432
Database: vpsi
User: vpsi
Password: vpsi_dev
```
