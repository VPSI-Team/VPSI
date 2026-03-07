# Kontrakty

Jediný zdroj pravdy pro API kontrakty mezi všemi komponentami systému VPSI.

## Obsah

| Složka | Popis |
|--------|-------|
| [`openapi/`](openapi/) | OpenAPI specifikace REST API |
| [`json-schema/`](json-schema/) | JSON Schema pro HW eventy |
| [`mqtt/`](mqtt/) | MQTT topic konvence a payloady |

## Pravidla

- Každá změna kontraktu musí být **zpětně kompatibilní**, nebo zavést novou verzi
- OpenAPI spec je zdrojem pro generování TypeScript typů ve frontendu
- JSON Schema definují formát událostí z HW zařízení (LPR, závory, senzory)

## Použití

### Generování TypeScript typů z OpenAPI

```bash
cd frontend
npm run generate-types
```

### Validace HW eventů

JSON Schema ve složce `json-schema/` slouží k validaci příchozích zpráv z HW simulátoru i reálných zařízení.
