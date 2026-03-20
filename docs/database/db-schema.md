# Databazove schema — VPSI Parkovaci system

> Dokumentace PostgreSQL 18 schematu pro automaticky parkovaci system.
> ER diagram: [`../../database/er-diagrams/er-diagram.md`](../../database/er-diagrams/er-diagram.md)

---

## 1. Prehled

Schema obsahuje **11 tabulek** rozdelenych do tri kategorii:

| Kategorie | Tabulky | Ucel |
|-----------|---------|------|
| **Business** | `parking_lot`, `parking_spot`, `vehicle`, `parking_session`, `payment_intent`, `tariff`, `tariff_rule` | Parkovaci relace, vozidla, platby, tarify |
| **HW a konfigurace** | `device`, `device_event` | Evidence zarizeni, nezmenitelny log HW udalosti |
| **Uzivatele a audit** | `app_user`, `audit_log` | Admin uzivatele, auditni stopa operaci |

---

## 2. Architektonicka rozhodnuti

| Rozhodnuti | Duvod |
|------------|-------|
| **UUID PK (`gen_random_uuid()`)** | Bezpecnejsi v API nez sekvencni ID — nelze odhadnout pocet zaznamu ani enumerovat entity |
| **CHECK constraints misto ENUM** | Pridani nove hodnoty nevyzaduje `ALTER TYPE` a blokujici migraci; CHECK se meni jednoduse |
| **JSONB pro `device_event.payload`** | HW eventy maji heterogenni strukturu (LPR, senzor, zavora) — JSONB umoznuje GIN indexy a flexibilni dotazy |
| **Soft delete (`is_deleted` + `deleted_at`)** | Master tabulky (`parking_lot`, `parking_spot`, `vehicle`, `device`, `app_user`, `tariff`) pouzivaji soft delete pro historickou dohledatelnost; transakci se to netyka |
| **Range partitioning na `device_event` a `audit_log`** | Vysoky objem, append-only data; partitioning umoznuje efektivni pruning a archivaci starych oddilu |
| **Tarifni model s verzovanim (`valid_from`/`valid_to`)** | Zmena tarifu nesmaze historicke zaznamy; `parking_session.tariff_id` odkazuje na konkretni verzi platnou v dobe vjezdu |

---

## 3. Tabulky

### 3.1 parking_lot

**Ucel:** Konfigurace parkoviste — nazev, adresa, kapacita, casova zona.

| Sloupec | Typ | Poznamka |
|---------|-----|----------|
| `id` | `uuid PK` | `gen_random_uuid()` |
| `name` | `text NOT NULL` | Zobrazovany nazev |
| `address` | `text` | Adresa lokality |
| `capacity_total` | `int NOT NULL` | CHECK `>= 0` |
| `timezone` | `text NOT NULL` | Default `Europe/Prague` |
| `is_active` | `boolean NOT NULL` | Default `true` |
| `is_deleted` | `boolean NOT NULL` | Soft delete |
| `created_at` | `timestamptz NOT NULL` | Default `now()` |
| `updated_at` | `timestamptz NOT NULL` | Default `now()` |

### 3.2 parking_spot

**Ucel:** Jednotliva parkovaci stani s typem (standard, EV, ZTP, motocykl).

| Sloupec | Typ | Poznamka |
|---------|-----|----------|
| `id` | `uuid PK` | |
| `parking_lot_id` | `uuid FK` | Vazba na `parking_lot` |
| `code` | `text NOT NULL` | Oznaceni stani (napr. `A-01`) |
| `spot_type` | `text NOT NULL` | CHECK `IN ('STANDARD','EV','ZTP','MOTORCYCLE')` |
| `is_active` | `boolean NOT NULL` | |
| `is_deleted` | `boolean NOT NULL` | Soft delete |
| `created_at` | `timestamptz NOT NULL` | |
| `updated_at` | `timestamptz NOT NULL` | |

### 3.3 vehicle

**Ucel:** Vozidlo identifikovane SPZ. `plate_number` je osobni udaj (GDPR), `plate_hash` slouzi k vyhledavani.

| Sloupec | Typ | Poznamka |
|---------|-----|----------|
| `id` | `uuid PK` | |
| `plate_number` | `text NOT NULL` | Osobni udaj — GDPR |
| `country_code` | `text` | Kod zeme (napr. `CZ`, `SK`) |
| `plate_hash` | `text NOT NULL UNIQUE` | SHA-256 hash pro vyhledavani bez expozice SPZ |
| `vehicle_group` | `text` | Skupina (rezident, zamestnanec, VIP) |
| `is_deleted` | `boolean NOT NULL` | Soft delete |
| `created_at` | `timestamptz NOT NULL` | |
| `updated_at` | `timestamptz NOT NULL` | |

### 3.4 device

**Ucel:** Evidence HW zarizeni (LPR kamera, zavora, senzor, informacni tabule, platebni terminal) — vcetne simulovanych v MVP.

| Sloupec | Typ | Poznamka |
|---------|-----|----------|
| `id` | `uuid PK` | |
| `parking_lot_id` | `uuid FK` | Vazba na `parking_lot` |
| `name` | `text NOT NULL` | Zobrazovany nazev |
| `device_type` | `text NOT NULL` | CHECK `IN ('LPR','BARRIER','SENSOR','DISPLAY','TERMINAL')` |
| `protocol` | `text NOT NULL` | CHECK `IN ('REST','MQTT')` |
| `config` | `jsonb NOT NULL` | Default `'{}'` — specificka konfigurace zarizeni |
| `last_seen_at` | `timestamptz` | Posledni heartbeat |
| `is_active` | `boolean NOT NULL` | |
| `is_deleted` | `boolean NOT NULL` | Soft delete |
| `created_at` | `timestamptz NOT NULL` | |
| `updated_at` | `timestamptz NOT NULL` | |

### 3.5 tariff

**Ucel:** Tarifni plan prirazeny k parkovisti. Verzovany pres `valid_from`/`valid_to` — zmena tarifu vytvori novy zaznam, stary se deaktivuje.

| Sloupec | Typ | Poznamka |
|---------|-----|----------|
| `id` | `uuid PK` | |
| `parking_lot_id` | `uuid FK` | |
| `name` | `text NOT NULL` | Nazev tarifu |
| `description` | `text` | Volitelny popis |
| `is_active` | `boolean NOT NULL` | |
| `valid_from` | `timestamptz NOT NULL` | Platnost od |
| `valid_to` | `timestamptz` | Platnost do (NULL = bez omezeni) |
| `is_deleted` | `boolean NOT NULL` | Soft delete |
| `created_at` | `timestamptz NOT NULL` | |
| `updated_at` | `timestamptz NOT NULL` | |

### 3.6 tariff_rule

**Ucel:** Jednotliva pravidla tarifu — sazba za hodinu, denni strop, volne minuty, nocni sazba.

| Sloupec | Typ | Poznamka |
|---------|-----|----------|
| `id` | `uuid PK` | |
| `tariff_id` | `uuid FK` | Vazba na `tariff` |
| `rule_type` | `text NOT NULL` | CHECK `IN ('PER_HOUR','FLAT','DAILY_CAP','FREE_MINUTES','NIGHT_RATE')` |
| `amount` | `numeric(12,2) NOT NULL` | Castka |
| `currency` | `text NOT NULL` | Default `CZK` |
| `parameters` | `jsonb NOT NULL` | Doplnkove parametry (napr. `{"minutes": 15}` pro FREE_MINUTES) |
| `priority` | `int NOT NULL` | Poradi aplikace pravidel |
| `created_at` | `timestamptz NOT NULL` | |

### 3.7 parking_session

**Ucel:** Parkovaci relace — zaznamenava vjezd, vyjezd, stav, platbu. Jadro business logiky.

| Sloupec | Typ | Poznamka |
|---------|-----|----------|
| `id` | `uuid PK` | |
| `parking_lot_id` | `uuid FK` | |
| `vehicle_id` | `uuid FK` | |
| `tariff_id` | `uuid FK` | Verze tarifu platna v dobe vjezdu |
| `entry_at` | `timestamptz NOT NULL` | Cas vjezdu |
| `exit_at` | `timestamptz` | Cas vyjezdu |
| `status` | `text NOT NULL` | CHECK `IN ('ACTIVE','PAID','CLOSED','DISPUTED','CANCELLED')` |
| `entry_device_id` | `uuid` | Logicky odkaz na `device` (bez FK — viz poznamky) |
| `exit_device_id` | `uuid` | Logicky odkaz na `device` |
| `total_amount` | `numeric(12,2)` | Celkova castka |
| `currency` | `text NOT NULL` | Default `CZK` |
| `paid_at` | `timestamptz` | Cas potvrzeni platby |
| `created_at` | `timestamptz NOT NULL` | |
| `updated_at` | `timestamptz NOT NULL` | |

**CHECK constraints:**
- `status IN ('ACTIVE','PAID','CLOSED','DISPUTED','CANCELLED')`
- `total_amount >= 0` (pokud je vyplneno)

### 3.8 payment_intent

**Ucel:** Zaznam o platebnim zameru — propojeni na platebni branu, stav autorizace/zachyceni.

| Sloupec | Typ | Poznamka |
|---------|-----|----------|
| `id` | `uuid PK` | |
| `parking_session_id` | `uuid FK` | |
| `amount` | `numeric(12,2) NOT NULL` | CHECK `>= 0` |
| `currency` | `text NOT NULL` | |
| `method` | `text NOT NULL` | CHECK `IN ('CARD','MOBILE_APP','QR')` |
| `provider_ref` | `text` | Externi reference platebniho poskytovatele |
| `status` | `text NOT NULL` | CHECK `IN ('INITIATED','AUTHORIZED','CAPTURED','FAILED','REFUNDED')` |
| `created_at` | `timestamptz NOT NULL` | |
| `updated_at` | `timestamptz NOT NULL` | |

### 3.9 device_event

**Ucel:** Nezmenitelny log vsech HW udalosti. Partitioned tabulka (range by mesic). Vysoka propustnost, append-only.

| Sloupec | Typ | Poznamka |
|---------|-----|----------|
| `id` | `uuid` | Composite PK `(id, occurred_at)` |
| `device_id` | `uuid` | Logicky FK — bez constraint kvuli partitioning |
| `event_type` | `text NOT NULL` | Napr. `LPR_PLATE_READ`, `BARRIER_OPENED`, `SPOT_OCCUPIED` |
| `occurred_at` | `timestamptz` | Composite PK — partitioning kluc |
| `idempotency_key` | `text NOT NULL` | Deduplikace eventu |
| `plate_number` | `text` | SPZ (pokud je soucasti eventu) |
| `payload` | `jsonb NOT NULL` | Heterogenni data dle typu zarizeni |
| `processing_status` | `text NOT NULL` | CHECK `IN ('RECEIVED','PROCESSING','PROCESSED','FAILED')` |
| `processed_at` | `timestamptz` | |
| `created_at` | `timestamptz NOT NULL` | |

### 3.10 app_user

**Ucel:** Admin uzivatel mapovany z externiho IdP (OIDC). Role urcuje opravneni v systemu.

| Sloupec | Typ | Poznamka |
|---------|-----|----------|
| `id` | `uuid PK` | |
| `external_subject` | `text NOT NULL UNIQUE` | Subject z JWT tokenu |
| `display_name` | `text NOT NULL` | |
| `email` | `text` | |
| `role` | `text NOT NULL` | CHECK `IN ('ADMIN','TECHNICIAN','FINANCE')` |
| `is_active` | `boolean NOT NULL` | |
| `is_deleted` | `boolean NOT NULL` | Soft delete |
| `created_at` | `timestamptz NOT NULL` | |
| `updated_at` | `timestamptz NOT NULL` | |

### 3.11 audit_log

**Ucel:** Auditni stopa vsech admin operaci. Partitioned tabulka (range by mesic). Append-only — zaznamy se nikdy nemazi.

| Sloupec | Typ | Poznamka |
|---------|-----|----------|
| `id` | `uuid` | Composite PK `(id, created_at)` |
| `app_user_id` | `uuid FK` | Kdo provedl akci |
| `action` | `text NOT NULL` | Napr. `TARIFF_CREATED`, `SESSION_DISPUTED` |
| `entity_type` | `text NOT NULL` | Typ entity (napr. `tariff`, `parking_session`) |
| `entity_id` | `uuid` | ID dotcene entity |
| `old_values` | `jsonb` | Puvodni hodnoty (pro zmeny) |
| `new_values` | `jsonb` | Nove hodnoty |
| `ip_address` | `inet` | IP adresa klienta |
| `created_at` | `timestamptz` | Composite PK — partitioning kluc |

---

## 4. Indexy

| Nazev | Tabulka | Sloupce | Ucel |
|-------|---------|---------|------|
| `ix_session_active` | `parking_session` | `parking_lot_id, status, entry_at DESC` | Aktivni relace parkoviste (obsazenost) |
| `ix_session_vehicle` | `parking_session` | `vehicle_id, entry_at DESC` | Historie relaci pro vozidlo |
| `ix_device_event_device_time` | `device_event` | `device_id, occurred_at DESC` | Historie eventu zarizeni (per partition) |
| `ix_device_event_plate` | `device_event` | `plate_number, occurred_at DESC` | Vyhledani eventu podle SPZ (partial: WHERE plate_number IS NOT NULL) |
| `ix_device_event_idempotency` | `device_event` | `device_id, idempotency_key` | Deduplikace eventu |
| `ix_payment_provider_ref` | `payment_intent` | `provider_ref` | Lookup plateb od providera (partial: WHERE provider_ref IS NOT NULL) |
| `ix_payment_session` | `payment_intent` | `parking_session_id, created_at DESC` | Platby pro relaci |
| `ix_tariff_lot_valid` | `tariff` | `parking_lot_id, valid_from, valid_to` | Aktivni tarify parkoviste (partial: WHERE is_active = true) |
| `ix_audit_log_entity` | `audit_log` | `entity_type, entity_id, created_at DESC` | Dohledani zmen konkretni entity |
| `ix_audit_log_user` | `audit_log` | `app_user_id, created_at DESC` | Akce uzivatele (partial: WHERE app_user_id IS NOT NULL) |
| `ix_device_event_payload_gin` | `device_event` | `payload` (GIN) | Fulltextove prohledavani JSON payloadu |
| `ix_audit_log_old_values_gin` | `audit_log` | `old_values` (GIN) | Prohledavani puvodních hodnot |
| `ix_audit_log_new_values_gin` | `audit_log` | `new_values` (GIN) | Prohledavani novych hodnot |
| `ix_*_active` | 6 tabulek | `id` | Partial indexy `WHERE NOT is_deleted` na soft-delete tabulkach |

---

## 5. Partitioning

### Proc

Tabulky `device_event` a `audit_log` jsou append-only s vysokym objemem zapisu. Range partitioning podle mesice umoznuje:

- **Partition pruning** — dotazy s filtrem na cas ctou pouze relevantni oddily
- **Efektivni archivace** — stare oddily lze odpojit (`DETACH PARTITION`) a presunout do archivu
- **Nizsi naroky na VACUUM** — mensi oddily = rychlejsi udrzba

### Implementace

```sql
-- device_event: partitioned by month
CREATE TABLE device_event (
    id uuid NOT NULL DEFAULT gen_random_uuid(),
    device_id uuid NOT NULL,
    event_type text NOT NULL,
    occurred_at timestamptz NOT NULL,
    -- ... dalsi sloupce
    PRIMARY KEY (id, occurred_at)
) PARTITION BY RANGE (occurred_at);

-- Oddily pro rok 2026
CREATE TABLE device_event_2026_01 PARTITION OF device_event
    FOR VALUES FROM ('2026-01-01') TO ('2026-02-01');
CREATE TABLE device_event_2026_02 PARTITION OF device_event
    FOR VALUES FROM ('2026-02-01') TO ('2026-03-01');
-- ... az do 2026-12
CREATE TABLE device_event_2026_12 PARTITION OF device_event
    FOR VALUES FROM ('2026-12-01') TO ('2027-01-01');

-- Vychozi partition pro data mimo rok 2026
CREATE TABLE device_event_default PARTITION OF device_event DEFAULT;

-- audit_log: stejna strategie
CREATE TABLE audit_log (
    id uuid NOT NULL DEFAULT gen_random_uuid(),
    app_user_id uuid,  -- logicky FK na app_user (bez constraint — partitioned tabulka); null pro systemove akce
    action text NOT NULL,
    -- ... dalsi sloupce
    created_at timestamptz NOT NULL,
    PRIMARY KEY (id, created_at)
) PARTITION BY RANGE (created_at);

-- Oddily 2026-01 az 2026-12 analogicky
```

### Omezeni

- **FK na partitioned tabulku:** PostgreSQL nepodporuje FK referencujici partitioned tabulku. Proto `device_event.device_id` nema FK constraint — integrita je zajistena aplikacne.
- **Composite PK:** Partitioning kluc musi byt soucasti PK. Proto PK je `(id, occurred_at)` misto standardniho `(id)`.

---

## 6. GDPR

### SPZ jako osobni udaj

V ceskem pravnim kontextu je SPZ (registracni znacka) povazovana za osobni udaj, pokud je priraditelna konkretni fyzicke osobe. System musi:

1. **Minimalizovat expozici** — `plate_number` se pouziva pouze pri vjezdu/vyjezdu a zobrazeni ridici
2. **Hashovat pro vyhledavani** — `vehicle.plate_hash` (SHA-256) umoznuje identifikaci vozidla bez priameho cteni SPZ
3. **Definovat retenci** — SPZ a pridruzena data se mazi/anonymizuji po uplynuti retencni doby (stanoveno provozovatelem)
4. **Logovat pristup** — kazdy pristup k `plate_number` se zaznamenava v `audit_log`

### Strategie anonymizace

```sql
-- Anonymizace vozidel starsich nez retencni doba (napr. 90 dni)
UPDATE vehicle
SET plate_number = 'ANONYMIZED',
    country_code = NULL,
    is_deleted = true,
    updated_at = now()
WHERE created_at < now() - INTERVAL '90 days'
  AND is_deleted = false;

-- Anonymizace SPZ v device_event
UPDATE device_event
SET plate_number = NULL,
    payload = payload - 'plate'
WHERE occurred_at < now() - INTERVAL '90 days'
  AND plate_number IS NOT NULL;
```

---

## 7. Priklady dotazu

### Aktualni obsazenost parkoviste

```sql
SELECT
    pl.id,
    pl.name,
    pl.capacity_total,
    COUNT(ps.id) AS occupied,
    pl.capacity_total - COUNT(ps.id) AS free
FROM parking_lot pl
LEFT JOIN parking_session ps
    ON ps.parking_lot_id = pl.id
    AND ps.status = 'ACTIVE'
WHERE pl.id = :lot_id
  AND pl.is_deleted = false
GROUP BY pl.id, pl.name, pl.capacity_total;
```

### Aktivni session pro SPZ

```sql
SELECT ps.*
FROM parking_session ps
JOIN vehicle v ON v.id = ps.vehicle_id
WHERE v.plate_hash = encode(sha256(:plate_number::bytea), 'hex')
  AND ps.status = 'ACTIVE';
```

### Historie eventu zarizeni

```sql
-- Partition pruning: dotaz cte pouze relevantni mesicni oddily
SELECT
    de.event_type,
    de.occurred_at,
    de.processing_status,
    de.payload
FROM device_event de
WHERE de.device_id = :device_id
  AND de.occurred_at >= now() - INTERVAL '24 hours'
ORDER BY de.occurred_at DESC
LIMIT 100;
```

### Denni trzby

```sql
SELECT
    DATE(ps.paid_at AT TIME ZONE pl.timezone) AS den,
    COUNT(*) AS pocet_plateb,
    SUM(ps.total_amount) AS celkem,
    ps.currency
FROM parking_session ps
JOIN parking_lot pl ON pl.id = ps.parking_lot_id
WHERE ps.paid_at >= :datum_od
  AND ps.paid_at < :datum_do
  AND ps.status IN ('PAID', 'CLOSED')
GROUP BY den, ps.currency
ORDER BY den DESC;
```

---

## 8. Seed data

Testovaci data v `database/seed/` jsou navrzena pro lokalni vyvoj a integracni testy. Pokryvaji:

| Entita | Obsah | Poznamka |
|--------|-------|----------|
| `parking_lot` | 1 parkoviste "Parkoviste Centrum" | Kapacita 100 mist, timezone `Europe/Prague` |
| `parking_spot` | 100 stani | 80x STANDARD, 10x EV, 10x ZTP |
| `device` | 5 zarizeni | 2x LPR (vjezd/vyjezd), 2x BARRIER, 1x SENSOR |
| `tariff` | 2 tarify | Zakladni (40 CZK/h) a nocni (20 CZK/h), platne od 2026-01-01 |
| `tariff_rule` | 5 pravidel | FREE_MINUTES (15 min), PER_HOUR (40 CZK), DAILY_CAP (200/100 CZK), NIGHT_RATE (20 CZK) |
| `vehicle` | 2 testovaci vozidla | 1AB2345 (bez skupiny), 2CD6789 (EMPLOYEE) |
| `app_user` | 2 uzivatele | 1x ADMIN, 1x TECHNICIAN |
| `parking_session` | 2 relace | 1x ACTIVE (1AB2345), 1x CLOSED s platbou (2CD6789) |
| `payment_intent` | 1 platba | CAPTURED, 100 CZK kartou |
| `device_event` | 6 udalosti | LPR_READ a BARRIER_OPEN pro vjezdy/vyjezdy |

### Vazba na HW simulator

Seed data pouzivaji stejna `device.id` jako konfigurace HW simulatoru (`hw-simulator/`). Simulacni scenare (`NormalFlowScenario`, `ErrorScenario`) generuji eventy pro zarizeni definovana v seed datech, coz umoznuje end-to-end testovani bez realneho HW.

Postup pro lokalni vyvoj:

```bash
# 1. Spustit databazi
docker compose -f docker/docker-compose.yml up -d db

# 2. Aplikovat migrace a seed data
bash database/scripts/reset-local-db.sh

# 3. Spustit backend API
dotnet run --project backend/src/Vpsi.Api

# 4. Spustit HW simulator
dotnet run --project hw-simulator/src/HwSimulator.App
```
