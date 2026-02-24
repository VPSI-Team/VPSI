# HW Simulator

.NET Worker Service simulujici hardwarova zarizeni parkovaciho systemu VPSI.

## Simulovana zarizeni

| Zarizeni | Popis |
|----------|-------|
| LPR kamera | Rozpoznani registracni znacky pri vjezdu/vyjezdu |
| Zavora | Otevreni/zavreni zavory na zaklade prikazu |
| Senzor obsazenosti | Detekce pritomnosti vozidla na parkovacim miste |
| Informacni tabule | Zobrazeni poctu volnych mist |

## Scenare

| Scenar | Popis |
|--------|-------|
| `NormalFlow` | Standardni prujezd: vjezd -> parkovani -> platba -> vyjezd |
| `ErrorScenario` | Chybove stavy: duplikaty SPZ, misread, vypadek komunikace |

## Spusteni

```bash
dotnet run --project src/HwSimulator.App -- --scenario NormalFlow
```

## Struktura

| Slozka | Popis |
|--------|-------|
| `Simulators/` | Implementace jednotlivych HW zarizeni |
| `Scenarios/` | Preddefinovane testovaci scenare |
| `Transport/` | Komunikacni vrstva (REST klient, MQTT publisher) |

## Komunikace

Simulator odesila eventy:
- **REST** -> Backend API (`POST /api/device-events`)
- **MQTT** -> Broker pro real-time notifikace

Format eventu odpovida JSON Schema v `contracts/json-schema/`.
