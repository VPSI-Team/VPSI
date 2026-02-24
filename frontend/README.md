# Frontend

React 19 + TypeScript + Vite webova aplikace pro system VPSI.

## Dve casti aplikace

| Oblast | Route prefix | Popis |
|--------|-------------|-------|
| Verejna | `/` | Platba za parkovani, QR kod, stav parkoviste |
| Admin | `/admin` | Sprava relaci, tarifu, zarizeni, audit log |

## Spusteni

```bash
npm install
npm run dev
# http://localhost:5173
```

## Build

```bash
npm run build
npm run preview
```

## Struktura

| Slozka | Popis |
|--------|-------|
| `src/pages/public/` | Verejne stranky (platba, QR, stav) |
| `src/pages/admin/` | Administratorske stranky |
| `src/components/` | Sdilene React komponenty |
| `src/hooks/` | Custom React hooks |
| `src/services/` | API klient a komunikace s backendem |
| `src/types/` | TypeScript typy (generovane z OpenAPI) |
| `src/styles/` | Globalni styly |

## Generovani typu z OpenAPI

```bash
npm run generate-types
```

Typy jsou generovane z `contracts/openapi/vpsi-api-v1.yaml`.
