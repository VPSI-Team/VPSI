# ADR-0001: Clean Architecture pro backend

## Stav

Přijato

## Kontext

Backend potřebuje jasnou strukturu oddělující business logiku od infrastruktury.

## Rozhodnutí

Použijeme Clean Architecture s vrstvami:
- **Domain** – entity, value objects, doménové eventy
- **Application** – use-casy, CQRS handlery, business pravidla
- **Infrastructure** – EF Core, repositáře, externí služby
- **Api** – controllery, middleware, DI konfigurace

## Důsledky

- Doménová logika je nezávislá na frameworku
- Snadné testování (domain + application bez DB)
- Vyšší počáteční komplexita struktury
