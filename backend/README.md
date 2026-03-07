# Backend

ASP.NET Core Web API s clean architecture pro system VPSI.

## Architektura

```
Vpsi.Api              -> HTTP controllery, middleware, DI konfigurace
Vpsi.Application      -> Use-casy, CQRS, business pravidla
Vpsi.Domain           -> Entity, value objects, enums, domenove eventy
Vpsi.Infrastructure   -> EF Core, repositare, externi sluzby
Vpsi.Contracts        -> Sdilene DTO, requesty, response modely
```

### Zavislosti mezi vrstvami

```
Api -> Application -> Domain
Api -> Infrastructure -> Application -> Domain
Api -> Contracts
Application -> Contracts
```

## Spusteni

```bash
dotnet run --project src/Vpsi.Api
# Swagger UI: http://localhost:5001/swagger
```

## Testy

```bash
dotnet test
```

## Struktura Application vrstvy

| Slozka | Popis |
|--------|-------|
| `Sessions/` | Parkovaci relace (vjezd, vyjezd, doba parkovani) |
| `Payments/` | Platebni logika (tarify, QR platba) |
| `Devices/` | Sprava HW zarizeni (zavory, senzory, LPR) |
| `Tariffs/` | Tarifni engine (vypocet ceny) |
